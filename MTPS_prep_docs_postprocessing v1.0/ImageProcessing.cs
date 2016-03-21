/*
 *****************************************************************************************************
 *
 *  MtpsPrepTool.EXE                           v-maklum                          September 26, 2007
 * 
 *  PROGRAM DESCRIPTION:
 *    This program accepts multiple Havana/Help 2.0 (.HxS file extension) archive(s) from either
 *    the first Migration Tool published by a-geralh or from Help Producer v2.2 and performs post
 *    processing there by making the specified Help 2.0 archive(s) compliant for publication to
 *    the most current version of MTPS (MSDN-TechNet Publishing System).
 *  
 *  ImageProcessing.CS
 * 
 *  CODE FILE DESCRIPTION:
 *    This file provides the implementation for Art/Image format and size validation.
 * 
 *  BUILD REQUIREMENTS:
 *    This is a winform based C# application.  
 *    Requires adding project assembly references for all name spaces listed immediately below
 *    this identification header, including the project reference to MagickNet (MagickNet.dll)
 *    provided by Image Magick (http://www.imagemagick.org/script/index.php).
 * 
 *  RETURNS:
 *    0 = If all opperations return successful with logged results
 *    -1 = If one or more opperations failed also with logged results
 *
 *****************************************************************************************************
*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Windows.Forms;

// Special Library Reference for the Tools Provided by Image Magick for the .NET runtime
// (compiled for x86 only)
using ImageMagick = MagickNet;

namespace MtpsPrepTool
{
    /// <summary>
    /// Implements the Art/Image format and size validation
    /// </summary>
    public class ImageProcessing
    {
        /// <summary>
        /// Logging control/support for ImageProcessing
        /// </summary>
        private static ListBox listBoxOutput;
        /// <summary>
        /// Location of the MtpsPrepTool output log file.
        /// </summary>
        internal static string logFile = "";
        /// <summary>
        /// Default Constructor
        /// </summary>
        internal ImageProcessing()
        {
            listBoxOutput = new ListBox();
        }
        /// <summary>
        /// Helper method that goes to the help achive\images folder to retrieve
        /// a list of all the images contained there.
        /// </summary>
        /// <param name="sourcePath">Source path of the help archives</param>
        /// <returns>An ImageMagick.ImageList collection</returns>
        private ImageMagick.ImageList GetImages(string sourcePath)
        {
            List<string> images = new List<string>(0);
            ImageMagick.ImageList imageList = new MagickNet.ImageList();
            string imageDir = null;
            if (Directory.Exists(sourcePath + @"\TestOutput\images"))
                imageDir = sourcePath + @"\TestOutput\images";
            else if (Directory.Exists(sourcePath + @"\TestOutput\art"))
                imageDir = sourcePath + @"\TestOutput\art";
            else
                imageDir = "";
            try
            {
                if( "" != imageDir )
                    images.AddRange(Directory.GetFiles(imageDir, "*", SearchOption.TopDirectoryOnly));
                if (0 == images.Count)
                {
                    MainForm.WriteLogFile(logFile, ImageProcessing.listBoxOutput);
                    return new MagickNet.ImageList(); // No files or image files where found in the help archive.
                }
                foreach (string imageName in images)
                    imageList.Add(imageName);
            }
            catch (Exception PROBLEM)
            {
                listBoxOutput.Items.Add("An error has occured while getting the archive's Art content:");
                listBoxOutput.Items.Add("Details:");
                listBoxOutput.Items.Add(PROBLEM.ToString());
                MessageBox.Show("An error has occured while getting the archive's Art content:\n\n" +
                    "Press View Log button for more details.\n", "Get Art/Document Images Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.ServiceNotification, false);
                MainForm.WriteLogFile(logFile, ImageProcessing.listBoxOutput);
                return null;
            }
            MainForm.WriteLogFile(logFile, ImageProcessing.listBoxOutput);
            return imageList;
        }
        /// <summary>
        /// Helper method that checks each Art/Image file has correct maximum width
        /// and supported MTPS image format.
        /// </summary>
        /// <param name="helpArchive">Next specified .HxS Havana help archive for Art/Image Checking</param>
        /// <param name="sourcePath">Source path of the help archives</param>
        /// <returns>CheckArtImageCollertion error status, 0 for success and -1 for failure</returns>
        internal int CheckArtImageCollection(string helpArchive, string sourcePath)
        {
            // Set the log file path and name for ImageProcessing
            logFile = helpArchive + "_ImageProcessing.log.txt";
            ImageMagick.ImageList imageList = GetImages(sourcePath);
            if (null == imageList)
                return -1;
            else if (0 == imageList.Images.Count)
            {
                listBoxOutput.Items.Add("No Art/Images where found with the HxS Help Archive named: " +
                    helpArchive);
                listBoxOutput.Items.Add("Continuing to process help archive(s)...");
            }
            ImageMagick.ImageList imageColTooLarge = new MagickNet.ImageList(),
                imageColUnsupported = new MagickNet.ImageList(),
                imageColSupported = new MagickNet.ImageList();
            foreach (ImageMagick.Image image in imageList.Images)
            {
                // Check if image format is not supported by MTPS
                if ("BMP" == image.Magick || "WMF" == image.Magick)
                {
                    imageColUnsupported.Add(image);
                    continue;
                }
                else if (image.Page.Width > 450) // Check image resolution
                {
                    imageColTooLarge.Add(image);
                    continue;
                }
                // The remaining images are supported along with resized Art/Images
                else if ("PNG" == image.Magick || "GIF" == image.Magick ||
                    "JPG" == image.Magick || "JPEG" == image.Magick)
                    imageColSupported.Add(image);
            }
            if (0 != imageColUnsupported.Count)
            {
                listBoxOutput.Items.Add("There are Art/Images in the help archive " + helpArchive +
                    " that are NOT supported by MTPS publishing system and will not be displayed.");
                listBoxOutput.Items.Add("Please consider converting these following unsupported " +
                    "Art/Images to one of the following supported formats (PNG, GIF, JPG, or JPEG):");
                foreach (ImageMagick.Image image in imageColUnsupported.Images)
                    listBoxOutput.Items.Add(image.BaseFilename);
                listBoxOutput.Items.Add("End of unsupported Art/Images list in archive " + helpArchive + ".");
                MessageBox.Show("There are Art/Images in the help archive of\n" + helpArchive +
                    "\nthat are NOT supported by MTPS publishing system and will not be displayed." +
                    "\nPlease consider converting these unsupported Art/Images to one" +
                    "\nof the following supported formats (PNG, GIF, JPG, or JPEG):\nThe unsupported Art/Images" +
                    " are logged to\n" + logFile,
                    "Unsupported Art/Images Found", MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification, false);
                imageColUnsupported.Images.Clear();
                imageColUnsupported.Dispose();
            }
            if (0 != imageColTooLarge.Count)
            {
                listBoxOutput.Items.Add("There are Art/Images in the help archive " + helpArchive +
                    " that are too wide and MUST be edited to display properly to MTPS.");
                // Get a list of images that are too wide
                foreach (ImageMagick.Image image in imageColTooLarge.Images)
                    listBoxOutput.Items.Add(image.BaseFilename);
                listBoxOutput.Items.Add("End of Art/Images found to be to wide in archive " + helpArchive + ".");
                DialogResult result = MessageBox.Show(
                    "There are Art/Images in the help archive of\n" + helpArchive +
                    "\nthat are too large in width to display correctly.\nThese images will be logged to:\n" +
                    logFile
                    + "\nThese Art/Images MUST be proportionaly reduced in size " +
                    "to properly display through MTPS.\nPlease select OKAY to resize these Art/Images" +
                    " down to 450 pixels wide each now, or CANCEL to edit later on.",
                    "Art/Images Found Too Wide", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification, false
                    );
                if (DialogResult.OK == result)
                {
                    listBoxOutput.Items.Add("Begin reducing Art/Images too large in archive " + helpArchive);
                    foreach (ImageMagick.Image image in imageColTooLarge.Images)
                    {
                        while (image.Size.Width > 450) // Resize the image too wide
                        {
                            int width = (int)((double)image.Size.Width * 0.99),
                                height = (int)((double)image.Size.Height * 0.99);
                            Size onePercentReduction = new Size(width, height);
                            image.Resize(onePercentReduction);
                        }
                        FileStream fileStream = File.Open(image.BaseFilename,
                            FileMode.Open, FileAccess.Write, FileShare.None);
                        // Write out the converted Art/Image by specifying a
                        // FileStream and the image format (image.Magick)
                        image.Write(fileStream, image.Magick);
                        listBoxOutput.Items.Add(image.BaseFilename + " was propotially reduced in size.");
                        UpdateHtmlImageSpec(image);
                        fileStream.Close();
                    }
                    listBoxOutput.Items.Add("End reducing Art/Images too large in archive " + helpArchive);
                }
                else if (DialogResult.Cancel == result)
                {
                    listBoxOutput.Items.Add("The following Art/Images were not resized and must each be edited" +
                        " down to 450px wide:");
                    foreach (ImageMagick.Image image in imageColTooLarge.Images)
                        listBoxOutput.Items.Add(image.BaseFilename);
                    listBoxOutput.Items.Add("End of Art/Images too wide list in archive " + helpArchive + ".");
                    MessageBox.Show("You have chosen NOT to resize these Art/Images that are too wide:\n" +
                        "These images logged to\n" + logFile + "\nwill not display correctly to MTPS until they are " +
                        "edited.", "Automatic Image Resizing Canceled", MessageBoxButtons.OK,
                        MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1,
                        MessageBoxOptions.ServiceNotification, false);
                }
            }
            if (0 != imageColSupported.Count)
            {
                listBoxOutput.Items.Add("The following supported Art/Images were found in archive " +
                    helpArchive + " and have the correct maximum Art/Image dimentions:");
                foreach (ImageMagick.Image image in imageColSupported.Images)
                {
                    FileStream fileStream = File.Open(image.BaseFilename,
                        FileMode.Open, FileAccess.Write, FileShare.None);
                    image.Write(fileStream, image.Magick);
                    fileStream.Close();
                    listBoxOutput.Items.Add(image.BaseFilename);
                }
                listBoxOutput.Items.Add("End of supported Art/Images list " + helpArchive + ".");
            }
            MainForm.progresscomplete.logSummary.Add("Image Processing for " +
                helpArchive.Remove(0, helpArchive.LastIndexOf('\\') + 1) + " succeeded.");
            MainForm.WriteLogFile(logFile, ImageProcessing.listBoxOutput);
            return 0;
        }
        /// <summary>
        /// Helper method that updates the source html display specifications for each Art/Image
        /// </summary>
        /// <param name="image">MagickNet.Image that was proportionally reduced in size</param>
        private void UpdateHtmlImageSpec(MagickNet.Image image)
        {
            string[] htmlFiles = Directory.GetFiles(MainForm.association.SourcePath + @"\TestOutput\html\",
                "*.htm?", SearchOption.TopDirectoryOnly);
            bool notInHtml = false;
            string htmlFilePath = "", fileName = "";
            foreach (string htmlFile in htmlFiles)
            {
                htmlFilePath = htmlFile;
                string html = File.ReadAllText(htmlFile),
                    artimagesFolder = "";
                Match match = null;
                Regex regex = null;
                fileName = image.BaseFilename.Substring(image.BaseFilename.LastIndexOf('\\') + 1);
                if (-1 == html.IndexOf(fileName, 0, html.Length, StringComparison.OrdinalIgnoreCase))
                {
                    notInHtml = true;
                    continue;
                }
                // Image specification provoded by Help Producer
                if (1 == Directory.GetDirectories(MainForm.association.SourcePath + "\\TestOutput",
                    "images", SearchOption.TopDirectoryOnly).Length)
                {
                    regex = new Regex("<img (border=\".*\" )?width=\"(.*)\" " +
                        "height=\"(.*)\" src=\"(.*)\">", RegexOptions.IgnoreCase);
                    match = regex.Match(html);
                    artimagesFolder = "../images/";
                }
                // Image specification provided by Migration Tool
                else if (1 == Directory.GetDirectories(MainForm.association.SourcePath + "\\TestOutput",
                    "art", SearchOption.TopDirectoryOnly).Length)
                {
                    regex = new Regex(
                        @"<img src=""([a-zA-Z0-9-/.]*)""( alt=""([a-zA-Z0-9-_ ]*)"")? ?/>",
                        RegexOptions.IgnoreCase);
                    match = regex.Match(html);
                    artimagesFolder = "../art/";
                }
                while (match.Success)
                {
                    if (match.Value.Contains(fileName))
                    {
                        string[] htmlLines = File.ReadAllLines(htmlFile);
                        for (int i = 0; i < htmlLines.Length; ++i)
                        {
                            if (htmlLines[i].Contains(match.Value))
                            {
                                int insertPosition = htmlLines[i].IndexOf(fileName, StringComparison.OrdinalIgnoreCase),
                                    limit = htmlLines[i].IndexOf(match.Value),
                                    fs_Char = 0;
                                for (int j = insertPosition; j > limit; --j)
                                {
                                    if ("../images/" == artimagesFolder && 0 == fs_Char)
                                    {
                                        for (int k = insertPosition; k > limit; --k)
                                        {
                                            insertPosition = (' ' == htmlLines[i][k]) ? k : insertPosition;
                                            fs_Char = (0 == fs_Char && ' ' == htmlLines[i][k]) ? k : fs_Char;
                                        }
                                        htmlLines[i] = htmlLines[i].Remove(insertPosition, fs_Char - insertPosition);
                                        j = htmlLines[i].IndexOf(fileName, StringComparison.OrdinalIgnoreCase);
                                    }
                                    if (' ' == htmlLines[i][j])
                                    {
                                        htmlLines[i] = htmlLines[i].Insert(j + 1, ("width=\"" +
                                            image.Size.Width + "\" height=\"" + image.Size.Height + "\" "));
                                        File.WriteAllLines(htmlFile, htmlLines);
                                        break;
                                    }
                                }
                                listBoxOutput.Items.Add("Updated html file " + htmlFile + " on line " + i.ToString() +
                                    " for the image display specification of " + fileName + ".");
                                MainForm.WriteLogFile(logFile, ImageProcessing.listBoxOutput);
                                return;
                            }
                        }
                    }
                    match = match.NextMatch();
                }
            }
            if (notInHtml)
                listBoxOutput.Items.Add("The art/image " + fileName + " was not specified for display" +
                    " in any html files in the path of " + MainForm.association.SourcePath +
                    @"\TestOutput\html\");
            MainForm.WriteLogFile(logFile, ImageProcessing.listBoxOutput);
            return;
        }
    }
}
