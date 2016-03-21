/*
 *****************************************************************************************************
 *
 *  MtpsPrepTool.EXE                           v-maklum                          October 23, 2007
 * 
 *  PROGRAM DESCRIPTION:
 *    This program accepts multiple Havana/Help 2.0 (.HxS file extension) archive(s) from either
 *    the first Migration Tool published by a-geralh or from Help Producer v2.2 and performs post
 *    processing there by making the specified Help 2.0 archive(s) compliant for publication to
 *    the most current version of MTPS (MSDN-TechNet Publishing System).
 *  
 *  MtpsPrepTool.CS
 * 
 *  CODE FILE DESCRIPTION:
 *    This file provides the main implementation for processing Help 2.0 archives, particularly for
 *    HTML topic file GUID parsing and analysis. The Image Processing presently included with this
 *    file will be factored out later on to a separate implementation file including improved logging.
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

using System.Threading;
using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MtpsPrepTool
{
    /// <summary>
    /// The MainForm implementation is found here.
    /// Although this winform application tool is compiled for any processor, the core
    /// library required for Art/Image processing was compiled only for x86, so the complete
    /// tool is best run as a 32 bit applications.
    /// </summary>
    public partial class MainForm : Form
    {
        #region Internal namespace accessible variables
        /// <summary>
        /// Determines if the log file has been appended
        /// </summary>
        private static bool logAppended;
        /// <summary>
        /// Location of the MtpsPrepTool output log file
        /// </summary>
        internal static string logFile = "";
        /// <summary>
        /// Topic and Guid counter part association and data file management object
        /// (Knows an object TopicGuidAsso type relationship)
        /// </summary>
        internal static TopicGuidAsso association;
        /// <summary>
        /// Html markup addins implementation object
        /// </summary>
        internal static HtmlmarkupAddins addins;
        /// <summary>
        /// ImageProcessing implementation object
        /// </summary>
        internal ImageProcessing imageprocessing;
        /// <summary>
        /// Main processing thread
        /// </summary>
        internal Thread processingThread;
        /// <summary>
        /// Current archive number for Html Processing
        /// </summary>
        internal static int archiveNumber;
        /// <summary>
        /// Private storage for each set of Archive Attributes
        /// </summary>
        internal static List<string[]> archiveAttributes;
        /// <summary>
        /// Thread responsible for decompiling a given archive
        /// </summary>
        internal Thread decompile;
        /// <summary>
        /// Thread responsible for recompiling a given archive
        /// </summary>
        internal Thread recompile;
        /// <summary>
        /// Progress complete status indicator form object
        /// </summary>
        internal static ProgressComplete progresscomplete;
        #endregion

        #region Private member variables
        /// <summary>
        /// Location of the HxComp.exe help archive compiler
        /// </summary>
        private string HxComp = "";
        /// <summary>
        /// Progress page status indicator form object
        /// </summary>
        private ProgressPage progresspage;
        /// <summary>
        /// Help archive absolute paths
        /// </summary>
        private static string[] helpArchives;
        #endregion

        #region MainForm default constructor
        /// <summary>
        /// MainForm default constructor
        /// </summary>
        internal MainForm()
        {
            InitializeComponent();

            // This object setup
            association = new TopicGuidAsso();
            association.listBoxOutput = this.listBoxOutput;
            imageprocessing = new ImageProcessing();
            btnFindArchives.Enabled = false;
            btnSetArchiveAttributes.Enabled = false;
            logAppended = false;
            CallRaiseExitEvent = new RaisingExitEvent(RaiseExitEvent);
            ProgressPage.mainFormReference = this;
            archiveNumber = 0;

            // Status child forms setup
            archiveAttributes = new List<string[]>();
            progresspage = new ProgressPage();
            progresscomplete = new ProgressComplete();
            progresspage.TopLevel = false;
            progresspage.Name = "ProgressPage";
            progresspage.Parent = pnlDisplay;
            progresscomplete.TopLevel = false;
            progresscomplete.Name = "ProgressComplete";
            progresscomplete.Parent = pnlDisplay;
            pnlDisplay.Controls.Add(progresspage);
            pnlDisplay.Controls.Add(progresscomplete);
        }
        #endregion

        /// <summary>
        /// Browse button click event
        /// </summary>
        /// <param name="sender">Source object</param>
        /// <param name="e">Arguements included with source object</param>
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.RootFolder = Environment.SpecialFolder.MyComputer;
            folderBrowserDialog1.ShowDialog();
            txtSourceFolder.Text = ("" == folderBrowserDialog1.SelectedPath) ?
                txtSourceFolder.Text : folderBrowserDialog1.SelectedPath;
        }
        /// <summary>
        /// btnSubmit enabled property changed event handler
        /// </summary>
        /// <param name="sender">Source object</param>
        /// <param name="e">Arguements included with source object</param>
        private void btnSubmit_EnabledChanged(object sender, EventArgs e)
        {
            if (null != processingThread)
            {
                if (processingThread.IsAlive && false == btnSubmit.Enabled &&
                    !HtmlmarkupAddins.cancelClicked)
                {
                    progresscomplete.SendToBack();
                    progresscomplete.Hide();
                    progresspage.BringToFront();
                    progresspage.Show();
                }
                else
                {
                    progresspage.SendToBack();
                    progresspage.Hide();
                    progresscomplete.BringToFront();
                    progresscomplete.Show();
                    progresscomplete.Activate();
                }
            }
        }
        /// <summary>
        /// btnFindArchives click event handler
        /// </summary>
        /// <param name="sender">Source object</param>
        /// <param name="e">Arguements included with source object</param>
        private void btnFindArchives_Click(object sender, EventArgs e)
        {
            if (!Init(txtSourceFolder.Text, ref helpArchives))
                RaiseExitEvent();
            archiveAttributes = new List<string[]>();
            btnSetArchiveAttributes.Enabled = true;
            btnSubmit.Enabled = false;
        }
        /// <summary>
        /// btnSetArchiveAttributes click event handler
        /// </summary>
        /// <param name="sender">Source object</param>
        /// <param name="e">Arguements included with source object</param>
        private void btnSetArchiveAttributes_Click(object sender, EventArgs e)
        {
            int numFiles = helpArchives.Length;
            // Addin requested html markup
            for (int i = 0; i < numFiles; ++i)
            {
                addins = new HtmlmarkupAddins();
                if (ProgressPage.mainFormReference.listArchives.Items.Count != 0)
                {
                    addins.txtProcessingArchive.Text = (string)
                        ProgressPage.mainFormReference.listArchives.Items[i];
                }
                if (MainForm.archiveAttributes.Count == numFiles)
                {
                    addins.txtDNLinkText.Text = archiveAttributes[i][0];
                    addins.txtDownloadLink.Text = archiveAttributes[i][1];
                    addins.dtpPublishDate.Text = archiveAttributes[i][2];
                }
                addins.ShowDialog(this);
            }
            if (!HtmlmarkupAddins.cancelClicked)
                btnSubmit.Enabled = true;
            else
                btnSubmit.Enabled = false;
            HtmlmarkupAddins.cancelClicked = false;
        }
        /// <summary>
        /// btnSubmit button click event
        /// </summary>
        /// <param name="sender">Source object</param>
        /// <param name="e">Arguements included with source object</param>
        private void btnSubmit_Click(object sender, EventArgs e)
        {
            // Main() MtpsPrepTool application processing method/operation
            processingThread = new Thread(new ThreadStart(LaunchProcessing));
            Thread callingThread = new Thread(new ThreadStart(CallingThreadHandler));
            processingThread.Start();
            btnSetArchiveAttributes.Enabled = false;
            btnSubmit.Enabled = false;
            callingThread.Start();
        }
        /// <summary>
        /// Helper method responsible for launching the main processing thread
        /// </summary>
        private void LaunchProcessing()
        {
            try
            {
                ProcessHelpArchives(txtSourceFolder.Text);
            }
            catch (ThreadAbortException error)
            {
                string archive = association.HelpArchive.Remove(0,
                    association.HelpArchive.LastIndexOf('\\') + 1);
                progresscomplete.logSummary.Add("The processing of HxS Archive " + archive +
                    " has been aborted. The processing of all other HxS Archives has stopped. " +
                    "Press the View Log button for more details.");
                listBoxOutput.Items.Add("The processing of HxS Archive " + archive +
                    " has been aborted. The processing of all other HxS Archives has stopped. " +
                    "Error details: " + error.ToString());
                MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
                DialogResult result = MessageBox.Show("The processing of HxS Archive\n" + archive +
                    "\nhas been aborted.\nThe processing of all other HxS Archives has also stopped.\n" +
                    "Press OK button immediatly bellow to restart the MtpsPrepTool\n(this application).",
                    "Main Processing Aborted",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.ServiceNotification, false);
                if (DialogResult.OK == result)
                    Application.Restart();
            }
            catch (Exception error)
            {
                progresscomplete.logSummary.Add("An error occured while processing HxS Archive " +
                    association.HelpArchive.Remove(0, association.HelpArchive.LastIndexOf('\\') + 1));
                listBoxOutput.Items.Add("An error occured while processing HxS Archive " +
                    association.HelpArchive + ". Error details: " + error.ToString());
                MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
                MessageBox.Show("An error occured while processing HxS Archive\n" +
                    association.HelpArchive.Remove( 0, association.HelpArchive.LastIndexOf('\\') + 1 )
                    + ".\nPress View Log button for details");
                MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
            }
            // Fire/Raise custom event ExitEvent
            RaiseExitEvent();
        } // End of void LaunchProcessing()
        /// <summary>
        /// Helper thread that that allows processingThread and MainForm
        /// to run undisturbed
        /// </summary>
        public void CallingThreadHandler()
        {
            processingThread.Join();
        }
        /// <summary>
        /// Displays a tooltip of the selected Source Folder.
        /// </summary>
        /// <param name="sender">Source object</param>
        /// <param name="e">Arguements included with source object</param>
        private void txtSourceFolder_MouseHover(object sender, EventArgs e)
        {
            toolTip1.Show(txtSourceFolder.Text, txtSourceFolder, 10000);
        }
        /// <summary>
        /// txtSourceFolder text changed event handler
        /// </summary>
        /// <param name="sender">Source object</param>
        /// <param name="e">Arguements included with source object</param>
        private void txtSourceFolder_TextChanged(object sender, EventArgs e)
        {
            btnFindArchives.Enabled = true;
        }
        /// <summary>
        /// Displays the About Box with instructions.
        /// </summary>
        /// <param name="sender">Source object</param>
        /// <param name="e">Arguements included with source object</param>
        private void btnAbout_Click(object sender, EventArgs e)
        {
            AboutBox1 about = new AboutBox1();
            about.Show();
        }
        /// <summary>
        /// Exits the main form.
        /// </summary>
        /// <param name="sender">Source object</param>
        /// <param name="e">Arguements included with source object</param>
        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
            Close();
        }
        /// <summary>
        /// Checks for the existance for the Microsoft Help 2.0 SDK, and also ultimately
        /// for the required HxComp.exe help archive compiler application.
        /// </summary>
        /// <returns>True if installed, otherwise false</returns>
        private bool IsHelp20SDKInstalled()
        {
            bool found_SDK = false;
            string[] locations = 
            {
                // Default install location for Visual Studio 2005 SDK w/ Help 2.0 SDK
                @"C:\Program Files\Visual Studio 2005 SDK\2007.02\VisualStudioIntegration\Archive\HelpIntegration",
                @"C:\Program Files (x86)\Visual Studio 2005 SDK\2007.02\VisualStudioIntegration\Archive\HelpIntegration",
                // Install locations for VS .NET 2003 Help 2.0 SDK
                @"C:\Program Files\Microsoft Help 2.0 SDK",
                @"C:\Program Files (x86)\Microsoft Help 2.0 SDK"
            };
            foreach (string location in locations)
            {
                string[] files = new string[0];
                try
                {
                    files = Directory.GetFiles(location);
                }
                catch (DirectoryNotFoundException)
                {
                    listBoxOutput.Items.Add("Microsoft Integerated Help 2.0 SDK not found at location " +
                        HxComp + ", trying next location.");
                }
                foreach (string file in files)
                {
                    if ( file.EndsWith("hxcomp.exe") )
                    {
                        HxComp = location + "\\hxcomp.exe";
                        found_SDK = true;
                        break;
                    }
                }
                if (found_SDK)
                    break;
            }
            if (found_SDK)
            {
                listBoxOutput.Items.Add("Found Visual Studio 2005 SDK w/ Help 2.0 SDK compiler at location: " +
                    HxComp);
                MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
            }
            else
            {
                MessageBox.Show("Help 2.0 SDK is not installed on your C Drive at the default install location.\n" +
                    "For Help Producer, Help 2.0 Compiler, or MtpsPrepTool to work, the Help 2.0 SDK is required\n" +
                    "which is only available with the Visual Studio 2005 SDK, which in turn also requires that\n" +
                    "Visual Studio 2005 be installed. This software can be installed from the following locations:\n" +
                    @"<\\PRODUCTS\PUBLIC\products\Developers\Visual Studio 2005\STD\VS\setup.exe> to install\n" +
                    "Visual Studio 2005 Standard Edition for PC (x86 size of 1.1GB), and also\n" +
                    @"<\\PRODUCTS\PUBLIC\Products\Developers\Visual Studio 2005\SDK\VsSDKFebruary2007.exe> for the\n" +
                    "Visual Studio 2005 SDK v4.0 released February 2007 for PC (x86 size of 124 MB).\n" +
                    "Please install the required SDK to the default location of the C Drive and try again.\n" +
                    "Processing Halted.", "Visual Studio SDK Help Integration Not Found", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification,
                    false);
                listBoxOutput.Items.Add("Help 2.0 SDK is not installed on your C Drive at the default install location.");
                listBoxOutput.Items.Add("For Help Producer, Help 2.0 Compiler, or MtpsPrepTool to work, the Help 2.0 SDK is required");
                listBoxOutput.Items.Add("which is only available with the Visual Studio 2005 SDK, which in turn also requires that");
                listBoxOutput.Items.Add("Visual Studio 2005 be installed. This software can be installed from the following locations:");
                listBoxOutput.Items.Add(@"<\\PRODUCTS\PUBLIC\products\Developers\Visual Studio 2005\STD\VS\setup.exe> to install");
                listBoxOutput.Items.Add("Visual Studio 2005 Standard Edition for PC (x86 size of 1.1GB), and also");
                listBoxOutput.Items.Add(@"<\\PRODUCTS\PUBLIC\Products\Developers\Visual Studio 2005\SDK\VsSDKFebruary2007.exe> for the");
                listBoxOutput.Items.Add("Visual Studio 2005 SDK v4.0 released February 2007 for PC (x86 size of 124 MB).");
                listBoxOutput.Items.Add("Please install the required SDK to the default location of the C Drive and try again.\n");
                listBoxOutput.Items.Add("Processing Halted.");
                progresscomplete.logSummary.Add("Help 2.0 SDK is not installed on your C Drive at the default install location.");
                MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
                found_SDK = false;
            }
            MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
            return found_SDK;
        }
        /// <summary>
        /// Searches the help archive source path and returns all of the .hxs help archives
        /// </summary>
        /// <param name="sourcePath">Source path of the help archives</param>
        /// <returns>A list of all the help archive (.HxS) files, otherwise null</returns>
        private string[] GetHelpArchives(ref string sourcePath)
        {
            // Set the main log file path and log file name.
            sourcePath = txtSourceFolder.Text;
            association.SourcePath = sourcePath;
            logFile = sourcePath + @"\OutputLog.txt";
            MainForm.exitRaised = (File.Exists(logFile)) ? true : false;

            string[] fileNames = null;
            Process proc = Process.GetCurrentProcess();
            proc.StartInfo.Domain = System.Net.CredentialCache.DefaultNetworkCredentials.Domain;
            proc.StartInfo.UserName = System.Net.CredentialCache.DefaultNetworkCredentials.UserName;
            string pwd = System.Net.CredentialCache.DefaultNetworkCredentials.Password;
            if (null == proc.StartInfo.Password)
                proc.StartInfo.Password = new System.Security.SecureString();
            for (int i = 0; 0 < pwd.Length; ++i)
                proc.StartInfo.Password.AppendChar(pwd[i]);
            proc.Refresh();
            if (File.Exists(sourcePath)) // Check if a file e.g. (.hxs) was specified
                sourcePath = sourcePath.Substring(0, sourcePath.LastIndexOf('\\'));
            /*
            // Check for path too long
            if (sourcePath.Length > 128)
                MessageBox.Show("The length of the input source path (> 128) is greater than half of\n" +
                    "the maximum allowed number of characters of any path (255 characters).\n" +
                    "Please consider using a mapped network drive from a folder share, or\n" +
                    "a local path that is significantly shorter with no spaces. There is a\n" +
                    "very strong chance that decompilation/recompilation will not succeed.",
                    "Input Source Path Too Long", MessageBoxButtons.OK, MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification, false);
             */
            if (Directory.Exists(sourcePath)) // Check for a valid directory input source path
                fileNames = Directory.GetFiles(sourcePath, "*.hxs", SearchOption.TopDirectoryOnly);
            else
            {
                MessageBox.Show("The input source path to the help archives you specified does\n" +
                    "NOT exist. Please try again by specifying a valid directory.", "Invalid " +
                    "Directory Specified", MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification, false);
                return null; // No help archives found
            }
            // Check for read only help archives
            bool isReadOnly = false;
            foreach (string fileName in fileNames)
            {
                FileAttributes attribs = File.GetAttributes(fileName),
                    compareAttribs1 = FileAttributes.Archive | FileAttributes.ReadOnly,
                    compareAttribs2 = FileAttributes.Archive | FileAttributes.ReadOnly | FileAttributes.Hidden;
                if (attribs == compareAttribs1 || attribs == compareAttribs2)
                {
                    MessageBox.Show("One or more Help Archives are Read Only.\n\nThe help archive(s) will" +
                        " now be set to Archive file attribute since they must be written to\nfor Asset ID insertion.\n\n" +
                        "If you need to process more archives in a different directory, please check first\n" +
                        "that the help arhchives are not set to Read Only.\n\nPlease press OK to continue.\n",
                        "One or More Read Only Help Archive(s) Detected", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    isReadOnly = true;
                    break;
                }
            }
            if (isReadOnly)
            {
                foreach (string fileName in fileNames)
                    File.SetAttributes(fileName, FileAttributes.Archive);
                isReadOnly = false;
            }
            // Check for HxS Archives already compiled, and if found, exclude them
            ArrayList list = new ArrayList(fileNames);
            for (int i = 0; i < fileNames.Length; ++i)
                if (fileNames[i].EndsWith(".MTPS.HxS", StringComparison.OrdinalIgnoreCase))
                    list.Remove(fileNames[i]);
            fileNames = (string []) list.ToArray(typeof(string));
            // Check for at least one help archive existing in the archive folder location
            foreach (string fileName in fileNames)
            {
                if (0 != fileNames.Length && fileName.EndsWith(".hxs", StringComparison.OrdinalIgnoreCase))
                    return fileNames;
            }
            // No help archives were found
            listBoxOutput.Items.Add("The decompilation process did not succeed.");
            listBoxOutput.Items.Add("No help (.HxS) archives were found at the specified location.");
            MessageBox.Show("No help archives (.HxS input, not .MTPS.HxS output) was found at the specified location.",
                "No Help 2.0 Archives Were Found", MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification, false);
            MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
            return null;
        }
        /// <summary>
        /// Helper Thread delegate responsible for calling the decompilation process
        /// </summary>
        private void ThreadDecompiler()
        {
            try
            {
                ResetIODirectory(association.SourcePath);
                // Decompile Archive
                Exception PROBLEM = DecompileArchive(association.HelpArchive, association.SourcePath);
                if (PROBLEM != null)
                    throw new ApplicationException("An Error Occured Decompiling. Error Details: " +
                        PROBLEM.ToString());
            }
            catch (Exception error)
            {
                listBoxOutput.Items.Add("Decompilation error occured. Error Details: " + error.ToString());
                progresscomplete.logSummary.Add("Decompilation error occured. Please see log for details.");
                MessageBox.Show("Decompilation error occured. Please see log for details.",
                    "Decompilation Error",MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification,
                    false);
                MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
                RaiseExitEvent();
                return;
            }
        }
        /// <summary>
        /// Decompilation ThreadStart delegate
        /// </summary>
        private void DecompileJoin()
        {
            decompile.Join();
        }
        /// <summary>
        /// Helper method Decompiles the specified Help Archive.
        /// </summary>
        /// <returns>Decompile error status</returns>
        /// <param name="helpArchive">Next specified .HxS Havana help archive for decompiling</param>
        /// <param name="sourcePath">Source path of the help archives</param>
        /// <returns>Decompilation error status, null for success and an Exception
        /// Object for failure</returns>
        private Exception DecompileArchive(string helpArchive, string sourcePath)
        {
            listBoxOutput.Items.Add("Begin decompiling help archive " + helpArchive);
            Process decompile = new Process();
            decompile.StartInfo.FileName = HxComp;
            decompile.StartInfo.Arguments = "-u \"" + helpArchive + "\" -d \"" +
                sourcePath + "\\TestOutput\" -l \"" + sourcePath + "\\decompile.log\"";
            decompile.Start();
            decompile.WaitForExit();
            //Thread decompileJoin = new Thread(new ThreadStart(DecompileJoin));
            //decompileJoin.Start();
            decompile.Close();
            decompile.Dispose();
            string[] lines = File.ReadAllLines(sourcePath + "\\decompile.log", Encoding.UTF8);
            string entireDecompileLog = File.ReadAllText(sourcePath + "\\decompile.log", Encoding.UTF8);
            foreach (string line in lines)
            {
                string[] words = line.Split(new char[] { ' ' }, 256, StringSplitOptions.RemoveEmptyEntries);
                foreach (string word in words)
                    if ("Fatal" == word || "Error" == word)
                    {
                        listBoxOutput.Items.Add("An error has occured in the decompilation process.");
                        progresscomplete.logSummary.Add("An error has occured in the decompilation " +
                            "process. Press View Log button for more details.");
                        listBoxOutput.Items.Add("The decompile error is: ");
                        listBoxOutput.Items.AddRange(lines);
                        MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
                        return new Exception("The decompile error is: " + entireDecompileLog);
                    }
            }
            listBoxOutput.Items.AddRange(lines);
            File.Delete(sourcePath + "\\decompile.log");
            listBoxOutput.Items.Add("End decompiling help archive " + helpArchive);
            listBoxOutput.Items.Add(helpArchive.Remove(0, helpArchive.LastIndexOf('\\') + 1) +
                " was successfully decompiled.");
            progresscomplete.logSummary.Add(association.HelpArchive.Remove( 0,
                association.HelpArchive.LastIndexOf('\\') + 1 ) + " was successfully decompiled.");
            MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
            return null;
        }
        /// <summary>
        /// Helper Thread delegate responsible for calling the recompilation process
        /// </summary>
        private void ThreadRecompiler()
        {
            try
            {
                // Recompile Archive
                Exception PROBLEM = RecompileArchive(association.HelpArchive, association.SourcePath);
                if (PROBLEM != null)
                    throw new ApplicationException("An Error Occured Recompiling. Error Details: " +
                        PROBLEM.ToString());
                // Write out log file for all logged events
                WriteLogFile(logFile, this.listBoxOutput);
                // Again reset archive IO processing folder
                ResetIODirectory(association.SourcePath);
            }
            catch (Exception error)
            {
                listBoxOutput.Items.Add("Recompilation error occured. Error Details: " + error.ToString());
                progresscomplete.logSummary.Add("Recompilation error occured. Please see log for details.");
                MessageBox.Show("Recompilation error occured. Please see log for details.",
                    "Decompilation Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification,
                    false);
                MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
                RaiseExitEvent();
                return;
            }
        }
        /// <summary>
        /// Recompilation ThreadStart delegate
        /// </summary>
        private void RecompileJoin()
        {
            recompile.Join();
        }
        /// <summary>
        /// Helper method Recompiles the specified Help Archive.
        /// </summary>
        /// <param name="helpArchive">Next specified .HxS Havana help archive for recompiling</param>
        /// <param name="sourcePath">Source path of the help archives</param>
        /// <returns>Recompilation error status, null for success and an
        /// Exception object for failure</returns>
        private Exception RecompileArchive(string helpArchive, string sourcePath)
        {
            // Check that DExplore is NOT running (open file handle on archive about to be recompiled)
            Process [] dExplore = Process.GetProcessesByName("dexplore");
            foreach( Process dexplore in dExplore )
                if (null != dexplore && 0 != dexplore.HandleCount)
                {
                    dexplore.Kill();
                    dexplore.Close();
                    dexplore.Dispose();
                }
            listBoxOutput.Items.Add("Begin recompiling help archive " + helpArchive);
            Process recompile = new Process();
            recompile.StartInfo.FileName = HxComp;
            string hxcFile = Directory.GetFiles(sourcePath + "\\TestOutput\\",
                "*.HxC", SearchOption.TopDirectoryOnly)[0];
            string recompileName = helpArchive.Remove(helpArchive.Length - 4, 4);
            recompile.StartInfo.Arguments = "-o \"" + recompileName + ".MTPS.HxS\" -p \"" + 
                hxcFile + "\" -r \"" + sourcePath + "\\TestOutput\" -l \"" +
                sourcePath + "\\recompile.log\"";
            recompile.Start();
            recompile.WaitForExit(100000);
            //Thread recompileJoin = new Thread(new ThreadStart(RecompileJoin));
            //recompileJoin.Start();
            string[] lines = File.ReadAllLines(sourcePath + "\\recompile.log", Encoding.UTF8);
            foreach (string line in lines)
            {
                string [] words = line.Split(new char[] {' '}, 256, StringSplitOptions.RemoveEmptyEntries);
                string entireCompileLog = File.ReadAllText(sourcePath + "\\recompile.log", Encoding.UTF8);
                foreach(string word in words)
                    if ("Fatal" == word || "Error" == word)
                    {
                        listBoxOutput.Items.Add("An error has occured in the recompilation process.");
                        progresscomplete.logSummary.Add("An error has occured in the recompilation " +
                            "process. Press View Log button for more details.");
                        listBoxOutput.Items.Add("The recompile error is: ");
                        listBoxOutput.Items.AddRange(lines);
                        WriteLogFile(logFile, this.listBoxOutput);
                        return new Exception("The recompile error is: " + entireCompileLog);
                    }
            }
            listBoxOutput.Items.AddRange(lines);
            File.Delete(sourcePath + "\\recompile.log");
            listBoxOutput.Items.Add("End recompiling help archive " + helpArchive);
            progresscomplete.logSummary.Add(helpArchive.Remove( 0,
                helpArchive.LastIndexOf('\\') + 1) + " was successfully recompiled.");
            listBoxOutput.Items.Add(helpArchive + " was successfully recompiled.");
            listBoxOutput.Items.Add("The MTPS Publish Preparation operation completed for: " + helpArchive);
            MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
            return null;
        }
        /// <summary>
        /// Helper method that "resets" the required Help Archive IO
        /// processing directory.
        /// </summary>
        /// <param name="sourcePath">Source path of the help archives</param>
        private void ResetIODirectory(string sourcePath)
        {
            if (Directory.Exists(sourcePath + "\\TestOutput"))
            {   // Try deleting the processing folder twice when compiling mutiple archives
                listBoxOutput.Items.Add("Deleting temporary processing directory of " +
                            sourcePath + "\\TestOutput");
                for (int i = 0; i < 2; ++i)
                {
                    try
                    {
                        Directory.Delete(sourcePath + "\\TestOutput", true);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            listBoxOutput.Items.Add("Creating temporary processing directory of " +
                sourcePath + "\\TestOutput");
            Directory.CreateDirectory(sourcePath + "\\TestOutput");
        }
        /// <summary>
        /// Write log file to the specified location.
        /// </summary>
        /// <param name="logFilePath">Location to write out the log file</param>
        /// <param name="listBoxOutput">The ListBox control that contains the logs</param>
        internal static void WriteLogFile(string logFilePath, ListBox listBoxOutput)
        {
            int numMtpsFiles = Directory.GetFiles(
               association.SourcePath, "*.MTPS.HxS",
                SearchOption.TopDirectoryOnly).Length;
            if (helpArchives.Length == numMtpsFiles && !logAppended && MainForm.exitRaised)
            {
                listBoxOutput.Items.AddRange(File.ReadAllLines(logFile));
                logAppended = true;
                exitRaised = false;
                return;
            }
            string[] allLines = new string[listBoxOutput.Items.Count];
            listBoxOutput.Items.CopyTo(allLines, 0);
            File.WriteAllLines(logFilePath, allLines, Encoding.UTF8);
            string summary = "Writing the log of " +
                logFilePath.Remove(0, logFilePath.LastIndexOf('\\') + 1) + " succeeded.";
            if( !progresscomplete.logSummary.Contains(summary) )
                progresscomplete.logSummary.Add( summary );
        }
        /// <summary>
        /// Helper method that inserts GUID line at the specified location.
        /// </summary>
        /// <param name="insertAt">Insertion location within an HtmlTopic file</param>
        /// <param name="line">Line found with leading tab characters</param>
        /// <param name="htmlFile">Path to Html Topic file under processing</param>
        /// <param name="sourceList">ArrayList for processing the Html Topic file</param>
        /// <param name="oldGUID">Specifies the old GUID applied from previous build</param>
        /// <returns>The complete XML line containing the new GUID</returns>
        private string InsertGuid(int insertAt, string line, string htmlFile, ref ArrayList sourceList, string oldGUID)
        {
            int numTabs = 0;
            string GUID = "";
            for (int i = 0; i < line.Length && '<' != line[i]; ++i)
                if ('\t' == line[i])
                    numTabs++; // Count number of leading tabs
            string tabInsert = "";
            for (int i = 0; i < numTabs + 1; ++i)
                tabInsert += '\t'; // build leading tabs for insert
            if (line.EndsWith("</HEAD>"))
                tabInsert += '\t';
            // Insert the newline with Guid
            if ("" == oldGUID)
                GUID = tabInsert + "<MSHelp:Attr Name=\"AssetID\" Value=\"" + Guid.NewGuid().ToString() + "\" />";
            else
                GUID = oldGUID;
            sourceList.Insert(insertAt, GUID);
            string[] allLines = new string[sourceList.Count];
            sourceList.CopyTo(allLines); // Convert sourceList to string[]
            File.WriteAllLines(htmlFile, allLines, Encoding.UTF8);
            string fileName = htmlFile.Substring(htmlFile.LastIndexOf('\\') + 1,
                htmlFile.Length - htmlFile.LastIndexOf('\\') - 1);
            listBoxOutput.Items.Add("Wrote AssetID MSHelp Attrib. to " +
                fileName + " on Line: " + insertAt);
            MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
            return GUID;
        }
        /// <summary>
        /// Helper method that finds the appropriate line in Html topic file for where the
        /// GUID needs to be inserted into each Html topic file's Xml Data island.
        /// Added fix: Original GUIDs will be saved with each originally associated Html Topic
        /// file and in each .HxS help archive as a separate text file in the same location as
        /// each help archive. This will allow the assigning of each original GUID when a given
        /// help archive is rebuilt.
        /// </summary>
        /// <param name="helpArchive">Name of the help archive being processed</param>
        /// <param name="sourcePath">Source path of the help archives</param>
        /// <returns>0 if inserts succeeded, otherwise -1</returns>
        private int FindLinesForInsert(string helpArchive, string sourcePath)
        {
            StreamReader guidReader1 = null, guidReader2 = null;
            if (File.Exists(sourcePath + "\\TestOutput\\html\\a_GUID_List.txt"))
                guidReader1 = new StreamReader(sourcePath + "\\TestOutput\\html\\a_GUID_List.txt");
            else if (File.Exists(helpArchive + "_GUID_List.txt"))
                guidReader2 = new StreamReader(helpArchive + "_GUID_List.txt");
            string[] htmlFiles = Directory.GetFiles(sourcePath + @"\TestOutput\html\",
                "*.htm?", SearchOption.TopDirectoryOnly);
            for (int mainLoop = 0; mainLoop < htmlFiles.Length; ++mainLoop)
            {
                string newGUID = "", archiveName = helpArchive.Remove(0, helpArchive.LastIndexOf('\\') + 1),
                    htmlFile = htmlFiles[mainLoop];
                // Check for rebuild from help producer by checking for and processing new Xml data files.
                if (File.Exists(helpArchive + "_GUID_List.xml") ||
                    File.Exists(sourcePath + "\\TestOutput\\html\\" + archiveName + "_GUID_List.xml"))
                {
                    if (!File.Exists(sourcePath + "\\TestOutput\\html\\" + archiveName + "_GUID_List.xml"))
                        File.Copy(helpArchive + "_GUID_List.xml",
                            sourcePath + "\\TestOutput\\html\\" + archiveName + "_GUID_List.xml");
                    else if(!File.Exists(helpArchive + "_GUID_List.xml"))
                        File.Copy(sourcePath + "\\TestOutput\\html\\" + archiveName + "_GUID_List.xml",
                            helpArchive + "_GUID_List.xml");
                    ArrayList filelist_HxF = new ArrayList(File.ReadAllLines(
                        Directory.GetFiles(sourcePath + "\\TestOutput", "*.HxF", SearchOption.TopDirectoryOnly)[0]));
                    if (!filelist_HxF.Contains("\t<File Url=\"html\\" + helpArchive + "_GUID_List.xml\" />"))
                    {
                        filelist_HxF.Insert(filelist_HxF.Count - 1, "\t<File Url=\"html\\" +
                            helpArchive.Remove(0, helpArchive.LastIndexOf('\\') + 1) + "_GUID_List.xml\" />");
                        string[] lines = new string[filelist_HxF.Count];
                        filelist_HxF.CopyTo(lines);
                        File.WriteAllLines( Directory.GetFiles(sourcePath +
                            "\\TestOutput", "*.HxF", SearchOption.TopDirectoryOnly)[0], lines);
                    }
                    if (!association.UpdateXml(htmlFiles[mainLoop]))
                    {
                        progresscomplete.logSummary.Add("An error occured while updating the Xml data files.");
                        return -1;
                    }
                    else
                    {
                        string status = "Preforming Xml data files update(s)...";
                        if (!progresscomplete.logSummary.Contains(status))
                            progresscomplete.logSummary.Add(status);
                        continue;
                    }
                }
                int insertAt = 0;
                ArrayList sourceList = new ArrayList(File.ReadAllLines(htmlFile, Encoding.UTF8));
                foreach (string line in sourceList)
                {
                    // Case 1: Html Topic file already has an Asset ID Microsoft Help Attribute.
                    if (line.Contains("<MSHelp:Attr Name=\"AssetID\" Value=\""))
                    {
                        if (File.Exists(sourcePath + "\\TestOutput\\html\\a_GUID_List.txt")
                            && !File.Exists(helpArchive + "_GUID_List.txt"))
                        {
                            listBoxOutput.Items.Add("Found data file " + sourcePath +
                                "\\TestOutput\\html\\a_GUID_List.txt, but not the data file " +
                                helpArchive + "_GUID_List.txt, rebuilding the data file " +
                                helpArchive + "_GUID_List.txt");
                            File.Copy(sourcePath + "\\TestOutput\\html\\a_GUID_List.txt",
                                helpArchive + "_GUID_List.txt", true);
                        }
                        string[] guidList = null;
                        if (File.Exists(helpArchive + "_GUID_List.txt"))
                            guidList = File.ReadAllLines(helpArchive + "_GUID_List.txt");
                        if(!File.Exists(sourcePath + "\\TestOutput\\html\\a_GUID_List.txt") &&
                            guidList == null ||
                            guidList.Length != Directory.GetFiles(sourcePath + "\\TestOutput\\html",
                            "*.htm?", SearchOption.TopDirectoryOnly).Length)
                        {
                            listBoxOutput.Items.Add("Data file " + helpArchive +
                                "_GUID_List.txt is incomplete, rebuilding with record addtion " + line);
                            StreamWriter writer = File.AppendText(helpArchive + "_GUID_List.txt");
                            writer.WriteLine(line);
                            writer.Close();
                            File.Copy(helpArchive + "_GUID_List.txt",
                                sourcePath + "\\TestOutput\\html\\a_GUID_List.txt", true);
                        }
                        break;
                    }
                    // Case 2: Html Topic has an Xml Data Island
                    else if (line.EndsWith("</xml>"))
                    {
                        insertAt = sourceList.IndexOf(line);
                        string[] GUIDS = new string[0];
                        if (File.Exists(helpArchive + "_GUID_List.txt"))
                            GUIDS = File.ReadAllLines(helpArchive + "_GUID_List.txt");
                        if (GUIDS.Length != Directory.GetFiles(sourcePath +
                            "\\TestOutput\\html", "*.htm?", SearchOption.TopDirectoryOnly).Length)
                        {
                            newGUID = InsertGuid(insertAt, line, htmlFile, ref sourceList, "");
                            if (!File.Exists(helpArchive + "_GUID_List.txt"))
                            {
                                FileStream fs = File.Create(helpArchive + "_GUID_List.txt");
                                fs.Close();
                            }
                            else if (null != guidReader2)
                                guidReader2.Close();
                            StreamWriter writer = File.AppendText(helpArchive + "_GUID_List.txt");
                            writer.WriteLine(newGUID);
                            writer.Close();
                        }
                        else
                        {
                            string oldGUID = (null != guidReader1) ? guidReader1.ReadLine() : guidReader2.ReadLine();
                            newGUID = InsertGuid(insertAt, line, htmlFile, ref sourceList, oldGUID);
                            GUIDS = new string[0];
                        }
                        string a_GUID_List = sourcePath + "\\TestOutput\\html\\a_GUID_List.txt";
                        if (File.Exists(a_GUID_List))
                            GUIDS = File.ReadAllLines(a_GUID_List);
                        if (GUIDS.Length != Directory.GetFiles(sourcePath +
                            "\\TestOutput\\html", "*.htm?", SearchOption.TopDirectoryOnly).Length)
                        {
                            if (!File.Exists(a_GUID_List))
                            {
                                FileStream fs = File.Create(a_GUID_List);
                                fs.Close();
                            }
                            else if (null != guidReader1)
                                guidReader1.Close();
                            StreamWriter sw = File.AppendText(a_GUID_List);
                            sw.WriteLine(newGUID);
                            sw.Close();
                        }
                        break;
                    }
                    // Case 3a: Find insert position after <TITLE> element line
                    else if (line.EndsWith("</TITLE>"))
                        insertAt = sourceList.IndexOf(line) + 1;
                    // Case 3b: Xml Data Island is NOT in the Html Topic file
                    else if (line.EndsWith("</HEAD>"))
                    {
                        string tabInsert = "";
                        for (int i = 0; i < line.Length && '<' != line[i]; ++i)
                            if ('\t' == line[i])
                                tabInsert += line[i];
                        tabInsert += '\t';
                        sourceList.Insert(insertAt, tabInsert + "<xml>");
                        sourceList.Insert(insertAt + 1, tabInsert + "</xml>");
                        string[] lines = new string[sourceList.Count];
                        sourceList.CopyTo(lines);
                        File.WriteAllLines(htmlFile, lines, Encoding.UTF8);
                        mainLoop--;
                        break;
                    }
                }
            }
            MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
            return 0;
        }
        /// <summary>
        /// Help method that initializes retrieval of the Help 2.0 Archives
        /// </summary>
        /// <param name="sourcePath">Help 2.0 HxS Archive Folder location</param>
        /// <param name="helpArchives">Array of Help 2.0 HxS Archive absolute paths</param>
        /// <returns>True if one or more Help 2.0 HxS Archive(s) were found, otherwise False</returns>
        internal bool Init(string sourcePath, ref string[] helpArchives)
        {
            try
            {
                listArchives.Items.Clear();
                helpArchives = GetHelpArchives(ref sourcePath);
                if (null == helpArchives)
                    return false;
                else if (!IsHelp20SDKInstalled())
                    return false;
                foreach (string helpArchive in helpArchives)
                    listArchives.Items.Add(helpArchive.Remove( 0,
                        helpArchive.LastIndexOf('\\') + 1) );
                association.SourcePath = sourcePath;

                // Set ProgressComplete static member references
                ProgressComplete.SourcePath = sourcePath;
                ProgressComplete.logFile = logFile;
                return true;
            }
            catch (System.Security.SecurityException securityError)
            {
                MessageBox.Show("You have attempted to execute this tool from a mapped network drive\n" +
                    "or a remote folder share where this tool is being hosted. Please first\n" +
                    "download this tool to a local folder on your local machine such as C:\\,\n" +
                    "and then re-execute this tool.\nPress View Log button for more details",
                    "Remote Execution Security Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification, false);
                listBoxOutput.Items.Add("You have attempted to execute this tool from a mapped network drive " +
                    "or a remote folder share where this tool is being hosted. Please first " +
                    "download this tool to a local folder on your local machine such as C:\\, " +
                    "and then re-execute this tool. Error details: " + securityError.ToString());
                MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
                return false;
            }
        }
        /// <summary>
        /// Processes the specifed source folder path containing HxS Help Archives
        /// by checking for 8-4-4-4-12 GUID inside of each of the Html Help topics
        /// within the Xml Data Island and for correct Art/Image formats and size.
        /// Please note that the Microsoft Help 2.0 SDK must be installed, and is
        /// explicitly checked for otherwise this tool will not succeed.
        /// </summary>
        /// <param name="sourcePath">Reference to Help Archive (.HxS) location</param>
        internal void ProcessHelpArchives(string sourcePath)
        {
            // Set temporary processing drive letter
            ProcessingDrive processingdrive = new ProcessingDrive();
            if (Directory.Exists(processingdrive.DriveLetter + ":\\"))
                processingdrive.DismountDrive();
            ProcessingDrive.originalSourcePath = sourcePath;
            processingdrive.MountDrive();
            // Begin the HxS Help 2.0 Archive Processing
            foreach (string helpArchive in helpArchives)
            {
                // Set TopicGuidAsso HelpArchive location and SourcePath
                association.HelpArchive = helpArchive;
                // Decompile Archive
                decompile = new Thread(new ThreadStart(ThreadDecompiler));
                decompile.Start();
                decompile.Join();
                // Check for existing AssetID, if not then insert new AssetID in html header data island(s).
                if (-1 == FindLinesForInsert(helpArchive, association.SourcePath))
                    return;
                // Convert flat files to readable associative Xml
                if (!association.WriteXmlFile())
                    return;
                // Checks that each Art/Image file has correct maximum width and supported MTPS image format.
                if (-1 == imageprocessing.CheckArtImageCollection(helpArchive, association.SourcePath))
                    return;
                // Perform Html Addin operations and Malformed Html Removal operations
                if (-1 == addins.ProcessHtml())
                    return;
                // Recompile Archive
                recompile = new Thread(new ThreadStart(ThreadRecompiler));
                recompile.Start();
                recompile.Join();
            } // End main loop processing
            // Recursivly remove the TestOutput processing directory.
            if (Directory.Exists(association.SourcePath + "\\TestOutput"))
            {
                Directory.Delete(association.SourcePath + "\\TestOutput", true);
                // Dismount temporary processing drive
                processingdrive.DismountDrive();
            }
        } // End of void ProcessHelpArchives(string sourcePath)
    } // End of MtpsPrepTool.MainForm
} // End of MtpsPrepTool namespace