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
 *  HtmlmarkupAddins.CS
 * 
 *  CODE FILE DESCRIPTION:
 *    This file provides the main implementation for html markup addins such as the addin for
 *    the publication date, addin for a download link right context sidebar, and removal of the
 *    following malformed html markup added be the first Migration Tool and Help Producer:
 *        1) Errorneous <SPAN style="font-family:Symbol">?</SPAN> html elements
 *        2) Document Title html instead of just Topic Title (Disabled)
 *        3) Erroneous <div class="subSection"> has described in CIS DocStudio Build System
 *           bug number 461 
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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MtpsPrepTool
{
    /// <summary>
    /// HtmlmarkupAddins implementation for publishing date and right side bar html addins.
    /// </summary>
    internal partial class HtmlmarkupAddins : Form
    {
        #region Static Data Members
        /// <summary>
        /// Html string for insertion to help procuder topic content that will
        /// show the Published date.
        /// </summary>
        internal static string htmlPublishDate;
        /// <summary>
        /// Html string for insertion to help producer topic content that will
        /// show the right context navigation bar containing download link.
        /// </summary>
        internal static string htmlRightContext;
        /// <summary>
        /// Determines if the cancel button was clicked
        /// </summary>
        internal static bool cancelClicked;
        /// <summary>
        /// Specifies what Archive Attributes to replace
        /// </summary>
        private static int replaceAt;
        #endregion

        #region HtmlmarkupAddins other data members
        /// <summary>
        /// Actual correctly formated Document Published Date
        /// </summary>
        internal string datePublished;
        /// <summary>
        /// Indicates whether the Document Title textbox field is correct or not
        /// </summary>
        private bool titleCorrect;
        /// <summary>
        /// Topic and Guid counter part association and data file management object.
        /// (Knows an object TopicGuidAsso type relationship)
        /// </summary>
        private TopicGuidAsso association;
        #endregion

        /// <summary>
        /// HtmlmarkupAddins default constructor
        /// </summary>
        public HtmlmarkupAddins()
        {
            InitializeComponent();
            association = MainForm.association;
            htmlPublishDate = txtDownloadLink.Text;
            htmlRightContext = dtpPublishDate.Value.ToLongDateString();
            //attribsCleared = false;
            titleCorrect = false;
            cancelClicked = false;
        }
        /// <summary>
        /// Textbox txtProcessingArchive mouse hover event handler
        /// </summary>
        /// <param name="sender">Source object</param>
        /// <param name="e">Arguements included with source object</param>
        private void txtProcessingArchive_MouseHover(object sender, EventArgs e)
        {
            string [] fullPath = Directory.GetFiles(association.SourcePath,
                txtProcessingArchive.Text, SearchOption.TopDirectoryOnly);
            toolTip1.Show(fullPath[0], txtProcessingArchive, 10000);
        }
        /// <summary>
        /// Text box DownloadLink mouse hover event handler
        /// </summary>
        /// <param name="sender">Source object</param>
        /// <param name="e">Arguements included with source object</param>
        private void txtDownloadLink_MouseHover(object sender, EventArgs e)
        {
            toolTip2.Show(txtDownloadLink.Text, txtDownloadLink, 10000);
        }
        /// <summary>
        /// Text box DownloadLink mouse hover event handler
        /// </summary>
        /// <param name="sender">Source object</param>
        /// <param name="e">Arguements included with source object</param>
        private void txtDocTitle_MouseHover(object sender, EventArgs e)
        {
            toolTip3.Show(txtDNLinkText.Text, txtDNLinkText, 10000);
            txtDNLinkText.Text = txtDNLinkText.Text.Trim();
        }
        /// <summary>
        /// Helper method that updates a document/help archive's published date
        /// </summary>
        /// <param name="htmlFile">Source html file</param>
        /// <param name="line">Source html line</param>
        /// <returns>The updated publish date html</returns>
        private string UpdatePublishDate(string htmlFile, string line)
        {
            datePublished = dtpPublishDate.Value.ToLongDateString();
            string[] dayNames = { "Monday, ", "Tuesday, ", "Wednesday, ",
                "Thursday, ", "Friday, ", "Saturday, ", "Sunday, " };
            foreach (string str in dayNames)
                if (datePublished.StartsWith(str))
                    datePublished = datePublished.Remove(0, str.Length);
            association.listBoxOutput.Items.Add("Formatting publication date to " +
               MainForm.archiveAttributes[MainForm.archiveNumber][2].Trim() +
               " in html file " +
               htmlFile);
            MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
            return line + MainForm.archiveAttributes[MainForm.archiveNumber][2] +
                "</div><br /><br />";
        }
        /// <summary>
        /// Helper method that performs the actual operations for Html feature addins
        /// and also Malformed html removal operations.
        /// </summary>
        /// <returns>0 if all operations succeeded, otherwise -1</returns>
        internal int ProcessHtml()
        {
            MainForm.archiveNumber = 0;
            string htmlFileInError = "";
            try
            {
                string[] htmlFiles = Directory.GetFiles(association.SourcePath + @"\TestOutput\html\",
                    "*.htm?", SearchOption.TopDirectoryOnly);
                foreach (string htmlFile in htmlFiles)
                {
                    htmlFileInError = htmlFile;
                    // Beginning processing and adding in Html for the publication date and
                    // Html table beginning
                    string[] htmlLines = File.ReadAllLines(htmlFile);
                    string line = "", htmlBefore = "";
                    if (File.ReadAllText(htmlFile).Contains("<td style=\"border: none\">\r\n"))
                    {   // Detected html output from first Migration Tool (MNP direct to MTPS)
                        line = "<br /><div class=\"date\">Published: ";
                        htmlBefore = "          <td style=\"border: none\">";
                    }
                    else
                    {   // Detected html output from Help Producer
                        line = "<table width=\"100%\" style=\"border: none\"><tr><td style=\"border: none\">" +
                            "<br /><div class=\"date\">Published: ";
                        htmlBefore = "<DIV id=\"nstext\">";
                    }
                    for (int i = 0; i < htmlLines.Length; ++i)
                    {
                        if (htmlLines[i].EndsWith(htmlBefore) || htmlLines[i].Contains(line))
                        {
                            htmlPublishDate = UpdatePublishDate(htmlFile, htmlBefore + line);
                            association.listBoxOutput.Items.Add("Applying publication date html of " +
                                htmlPublishDate.Trim() + " in html file " + htmlFile + " on line " + i.ToString());
                            htmlLines[i] = htmlPublishDate;
                            File.WriteAllLines(htmlFile, htmlLines);
                            break;
                        }
                    }
                    // Ending processing and adding in Html for the publication date and Html table beginning

                    // Begin processing the ending Html for closing the Html Table and
                    // Rightside Context Sidebar
                    htmlLines = File.ReadAllLines(htmlFile);
                    htmlBefore = File.ReadAllText(htmlFile);
                    line = "";
                    Regex expression = new Regex("( {5,25}<a href=\")(.*)(\">)(.*)", RegexOptions.None);
                    Match match = expression.Match(htmlBefore);
                    if ( htmlBefore.Contains("<td style=\"border: none\">") && 1 ==
                        Directory.GetDirectories(MainForm.association.SourcePath + "\\TestOutput",
                        "art", SearchOption.TopDirectoryOnly).Length )
                    {   // Detected html output from first Migration Tool (MNP direct to MTPS)
                        line = string.Format(match.Groups[1].Value + "{0}" + match.Groups[3].Value +
                            match.Groups[4].Value,
                            MainForm.archiveAttributes[MainForm.archiveNumber][1]).TrimEnd();
                    }
                    else
                    {   // Detected html output from Help Producer
                        line = "</td><td width=\"155px\" style=\"border: none; vertical-align:" +
                            " text-top;\"><table width=\"100%\"><tr><td style=\"background-color: #EEEEEE\"" +
                            "><b>Download</b><br /><p><a href=\"";
                    }
                    for (int i = 0; i < htmlLines.Length; ++i)
                    {
                        if (htmlLines[i].Contains(line))
                            break;
                        else if (match.Success && htmlLines[i].Contains(match.Groups[2].Value))
                        {
                            htmlRightContext = line;
                            association.listBoxOutput.Items.Add("Applying sidebar download link of " +
                                txtDownloadLink.Text + ", and formed as the html of " + htmlRightContext.Trim() +
                                " in html file " + htmlFile + " on line " + i.ToString());
                            htmlLines[i] = htmlRightContext;
                            File.WriteAllLines(htmlFile, htmlLines);
                            break;
                        }
                        else if (htmlLines[i].TrimStart().StartsWith("<DIV class=\"footer\">"))
                        {
                            htmlRightContext = "\t\t\t" + line +
                                MainForm.archiveAttributes[MainForm.archiveNumber][1] +
                                "\">" + MainForm.archiveAttributes[MainForm.archiveNumber][0] +
                                "</a></p></td></tr></table></td></tr></table>" +
                                "<DIV class=\"footer\">";
                            association.listBoxOutput.Items.Add("Applying sidebar download link of " +
                                txtDownloadLink.Text + ", and formed as the html of " + htmlRightContext.Trim() +
                                " in html file " + htmlFile + " on line " + i.ToString());
                            htmlLines[i] = htmlRightContext;
                            File.WriteAllLines(htmlFile, htmlLines);
                            break;
                        }
                    }
                    // End processing the ending Html for closing the Html Table and
                    // Rightside Context Sidebar
                }
                Exception PROBLEM = RemoveMalformedHtml();
                if ( PROBLEM != null )
                    throw PROBLEM;
                MainForm.progresscomplete.logSummary.Add("Html Addins and Malformed Html " +
                    "Removal succeeded for " +
                    association.HelpArchive.Remove(0, association.HelpArchive.LastIndexOf('\\') + 1) + ".");
                MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
                return 0;
            }
            catch (Exception error)
            {
                string archive = association.HelpArchive.Remove(0,
                    association.HelpArchive.LastIndexOf('\\') + 1);
                MainForm.progresscomplete.logSummary.Add("An error occured while processing " +
                    "Html for " + archive + ".");
                association.listBoxOutput.Items.Add(
                    "An error occured while attempting to perform the required post " +
                    "processing for the publication date addin and/or the right side " +
                    "context sidebar addin. This error occured in the HxS Document " +
                    "Archive of " + association.HelpArchive + " and while processing " +
                    "the Html File of " + htmlFileInError + ". Error details: " + error.ToString()
                    );
                MessageBox.Show(
                    "An error occured while attempting to perform the required post\n" +
                    "processing for the publication date addin and/or the right side\n" +
                    "context sidebar addin. This error occured in the HxS Document\n" +
                    "Archive of " + archive + " and while processing\n" +
                    "the Html File of " + htmlFileInError + ".\nPress View Log button " +
                    "for more details.",
                    "Html Addin/Removal Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.ServiceNotification, false
                    );
                MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
                return -1;
            }
        }
        /// <summary>
        /// SUBMIT button click event handler
        /// </summary>
        /// <param name="sender">Source object</param>
        /// <param name="e">Arguements included with source object</param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if ( -1 == ValidateDocTitleText() )
                    return;
                if ( -1 == ValidateDownloadLink() )
                    return;
                string [] value = new string []
                {
                    txtDNLinkText.Text,
                    txtDownloadLink.Text,
                    datePublished = dtpPublishDate.Text
                };
                bool addValue = true;
                int numFiles = Directory.GetFiles(
                    association.SourcePath, "*.hxs", SearchOption.TopDirectoryOnly).Length;
                foreach (string[] compare in MainForm.archiveAttributes)
                    if (compare[0] == value[0] && compare[1] == value[1]
                        && compare[2] == value[2])
                    {
                        MainForm.archiveAttributes[MainForm.archiveAttributes.IndexOf(compare)] = value;
                        addValue = false;
                        break;
                    }
                if (addValue)
                {
                    if (MainForm.archiveAttributes.Count != numFiles)
                    {
                        MainForm.archiveAttributes.Add(value);
                        replaceAt = 0;
                        MainForm.archiveNumber++;
                    }
                    else
                    {
                        MainForm.archiveAttributes[ ( numFiles == replaceAt ) ?
                            replaceAt = 0 : replaceAt] = value;
                        replaceAt = replaceAt + 1;
                    }
                }
                MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
                Close();
            }
            catch (ApplicationException error)
            {
                association.listBoxOutput.Items.Add("An error occured while submitting this form. " +
                    "Error details: " + error.ToString());
                MessageBox.Show("An error occured while submitting this form.\nPress View Log button " +
                    "for more details.", "Additions to Html Content Submit Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.ServiceNotification, false);
                MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
                return;
            }
        }
        /// <summary>
        /// Helper method that certain unneccessarily inserted SPAN html elements
        /// done by help producer.
        /// </summary>
        /// <returns>A System.Exception object if a PROBLEM occured, otherwise null</returns>
        private Exception RemoveMalformedHtml()
        {
            try
            {
                string[] htmlFiles = Directory.GetFiles(association.SourcePath + @"\TestOutput\html\",
                    "*.htm?", SearchOption.TopDirectoryOnly);
                foreach (string htmlFile in htmlFiles)
                {
                    string[] htmlLines = File.ReadAllLines(htmlFile);
                    Regex expression = null;
                    for (int i = 0; i < htmlLines.Length; ++i)
                    {
                        // Check for and remove malformed SPAN element html inserted by Help Producer
                        expression = new Regex("<SPAN style=\"font-family:Symbol\">?.* </SPAN>");
                        if (expression.Match(htmlLines[i]).Success)
                        {
                            association.listBoxOutput.Items.Add("Removing malformed html instance of " +
                                "<SPAN style=\"font-family:Symbol\">?</SPAN>" + " in html file " + htmlFile +
                                " on line " + i.ToString());
                            htmlLines[i] = htmlLines[i].Remove(htmlLines[i].IndexOf(
                                expression.Match(htmlLines[i]).Value),
                                expression.Match(htmlLines[i]).Value.Length);
                            File.WriteAllLines(htmlFile, htmlLines);
                        }
                        /* Check for and replace all occurances of malformed HTLM inserted by first Migeration
                         * Tool as described in CIS DocStudio Build System bug number 461 <div class="subSection">
                         */
                        expression = new Regex(@"<div {0,1}class=""subSection"" {0,1}/>", RegexOptions.None);
                        if (expression.Match(htmlLines[i]).Success) // 
                        {
                            association.listBoxOutput.Items.Add("Replaced malformed HTML instance of <div " +
                                "class=\"subSection\"/> in the html file of " + htmlFile + " on line " +
                                i.ToString() + " with well formed instance of <div class=\"subSection\"></div>");
                            string replacement = @"<div class=""subSection""></div>";
                            string result = expression.Replace(htmlLines[i], replacement);
                            htmlLines[i] = result;
                            File.WriteAllLines(htmlFile, htmlLines);
                        }
                    }
                    /*
                    // Beginning of check for and remove html that prints the document title
                    association.listBoxOutput.Items.Add("Removed Document Title malformed Html instance of " +
                        txtDocTitle.Text + " in html file of " + htmlFile);
                    expression = new Regex(@"<tr>\r\n *<td align=""left"">\r\n *" +
                        txtDocTitle.Text + "</td>\r\n *</tr>", RegexOptions.None);
                    string input = File.ReadAllText(htmlFile);
                    if (expression.Match(input).Success)
                    {
                        string result = expression.Replace(input, "");
                        File.WriteAllText(htmlFile, result);
                    }
                    // End of check for and remove html that prints the document title
                    */
                }
                MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
                return null;
            }
            catch (Exception error)
            {
                string archive = association.HelpArchive.Remove(0, association.HelpArchive.LastIndexOf('\\') + 1);
                association.listBoxOutput.Items.Add("An error occured while attempting to remove malformed" +
                    " html in the HxS Help 2.0 Archive of " + association.HelpArchive + ". The error details" +
                    "are: " + error.ToString());
                MessageBox.Show("An error occured while attempting to remove malformed html in the HxS\n" +
                    "Help 2.0 Archive of\n" + archive + ".\nPress View Log button for more details.",
                    "Remove Malformed Html Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification, false);
                MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
                return error;
            }
        }
        /// <summary>
        /// Helper method that validates the text in the txtDocTitle text box
        /// </summary>
        /// <returns>0 if all the validation is successful, otherwise -1</returns>
        private int ValidateDocTitleText()
        {
            txtDNLinkText.Text = txtDNLinkText.Text.Trim();
            string title = txtDNLinkText.Text;
            char [] unsupportedCharacters = new char [] {'<','>','|','&','"','?','%','^','(',')'};
            try
            {
                int charNum = title.IndexOfAny(unsupportedCharacters);
                if (-1 != charNum)
                {
                    char character = title[charNum];
                    throw new ApplicationException("An unsuppoted character " + character +
                        " was detected in the document title. Please delete" +
                        " this unsupported character in the specified Document Title " +
                        "and try again.");
                }
                else if (title.Length > 256)
                    throw new ApplicationException("The specified document title must not " +
                        "be greater than 256 character in length.");
                else if ("" == title)
                    throw new ApplicationException("You did not specify a Document Title " +
                        "for the HxS Document Archive of " + (string)
                        ProgressPage.mainFormReference.listArchives.Items[MainForm.archiveNumber] +
                        ".\nYou MUST specifiy the FULL Document Title for this HxS Document Archive.");
                titleCorrect = true;
                MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
                return 0;
            }
            catch (ApplicationException titleError)
            {
                association.listBoxOutput.Items.Add("An error occured while processing the " +
                    "HxS Document Archive of " +
                    ProgressPage.mainFormReference.listArchives.Items[MainForm.archiveNumber] +
                    ". Error details: " + titleError.ToString());
                MessageBox.Show("An error occured while processing the HxS Document Archive of " +
                    ProgressPage.mainFormReference.listArchives.Items[MainForm.archiveNumber] +
                    "\n" + titleError.Message, "Document Title Specification Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.ServiceNotification, false);
                titleCorrect = false;
                MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
                return -1;
            }
        }
        /// <summary>
        /// btnCancel button click event handler
        /// </summary>
        /// <param name="sender">Source object</param>
        /// <param name="e">Arguements included with source object</param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            ProgressPage.mainFormReference.btnFindArchives.Enabled = true;
            ProgressPage.mainFormReference.btnSetArchiveAttributes.Enabled = true;
            MainForm.CallRaiseExitEvent();
            cancelClicked = true;
            Close();
        }
        /// <summary>
        /// Helper method that checks to see if the user is connected to the internet,
        /// and if they are, further checks the validity of the document download link
        /// by verifying it accross the internet as a System.Net.WebRequest. If no
        /// connection is detected, then the document download link is only verified as
        /// not having unsupported character.
        /// </summary>
        /// <returns>0 if all the validation is successful, otherwise -1</returns>
        private int ValidateDownloadLink()
        {
            txtDownloadLink.Text = txtDownloadLink.Text.Trim();
            string tempDownLink = txtDownloadLink.Text;
            try
            {
                string requestUriString = ("" == txtDownloadLink.Text) ?
                    "http://technet.microsoft.com" : txtDownloadLink.Text;
                System.Net.WebRequest request =
                    System.Net.WebRequest.Create(requestUriString);
                request.UseDefaultCredentials = true;
                request.Proxy = System.Net.WebRequest.GetSystemWebProxy();
                request.ImpersonationLevel =
                    System.Security.Principal.TokenImpersonationLevel.Delegation;
                request.AuthenticationLevel =
                    System.Net.Security.AuthenticationLevel.MutualAuthRequested;
                Uri uri = null;
                try
                {
                    System.Net.WebResponse response = request.GetResponse();
                    uri = response.ResponseUri;
                    tempDownLink = ("" != uri.AbsolutePath) ? requestUriString : tempDownLink;
                    if ("" == uri.AbsolutePath ||
                        "http://technet.microsoft.com" == requestUriString)
                            throw new System.Net.WebException();
                }
                catch(System.Net.WebException)
                {
                    association.listBoxOutput.Items.Add("We were not able to successfully validate " +
                        "the Download Forward Link you specified for the document with the title of " +
                        txtDNLinkText.Text + ", and compiled as the HxS Archive of " +
                        txtProcessingArchive.Text + ". You are either not connected to a network such as " +
                        "CorpNet or the internet, or you did not enter a download forward link.");
                    MessageBox.Show("We were not able to successfully validate the Download\n" +
                        "Forward Link you specified for the document with the title of\n" +
                        txtDNLinkText.Text + ",\nand compiled as the HxS Archive of\n" +
                        txtProcessingArchive.Text +
                        ".\nYou are either not connected to a network such asCorpNet or \n" +
                        "the internet, you did not enter a download forward link, or the\n" +
                        "specified Download Forward Link cannot be resolved.\n" +
                        "Press OK to continue processing.", "Download Link Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1,
                        MessageBoxOptions.ServiceNotification, false);
                    MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
                }
                if (!titleCorrect)
                    tempDownLink = "";
                char[] unsupportedCharacters = new char[] { '<', '>', '|', '"', '^', '(', ')' };
                if ( -1 != txtDownloadLink.Text.IndexOfAny(unsupportedCharacters) )
                {
                    char character = tempDownLink[tempDownLink.IndexOfAny(unsupportedCharacters)];
                    throw new ApplicationException("An unsuppoted character " + character +
                        " was detected in the specified download link. " +
                        "Please delete this unsupported character and make sure that the download" +
                        " link is correct.");
                }
                MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
                return 0;
            }
            catch (Exception error)
            {
                tempDownLink = "";
                association.listBoxOutput.Items.Add("You must specify the full Document Title and " +
                    "a valid Document Download Forward Link before proceeding. Error details: " +
                    error.ToString());
                MessageBox.Show("You must specify the full Document Title as it appears for this " +
                    "Document\nand also a valid Document Download Forward Link before proceeding.\n",
                    "Document Title/Download Link Validation Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.ServiceNotification, false);
                MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
                return -1;
            }
        }
    }
}