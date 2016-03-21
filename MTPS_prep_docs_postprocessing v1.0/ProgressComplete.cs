/*
 *****************************************************************************************************
 *
 *  MtpsPrepTool.EXE                           v-maklum                          September 27, 2007
 * 
 *  PROGRAM DESCRIPTION:
 *    This program accepts multiple Havana/Help 2.0 (.HxS file extension) archive(s) from either
 *    the first Migration Tool published by a-geralh or from Help Producer v2.2 and performs post
 *    processing there by making the specified Help 2.0 archive(s) compliant for publication to
 *    the most current version of MTPS (MSDN-TechNet Publishing System).
 *  
 *  ProgressComplete.CS
 * 
 *  CODE FILE DESCRIPTION:
 *    This file provides the implementation for ProgressComplete child form of pnlDisplay in MainForm.
 * 
 *  BUILD REQUIREMENTS:
 *    This is a winform based C# application.  
 *    Requires adding project assembly references for all name spaces listed immediately below
 *    this identification header, including the project reference to MagickNet (MagickNet.dll)
 *    provided by Image Magick (http://www.imagemagick.org/script/index.php).
 * 
 *  RETURNS:
 *    n/a
 *
 *****************************************************************************************************
*/

using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections;

namespace MtpsPrepTool
{
    /// <summary>
    /// Implementation for ProgressComplete child form
    /// </summary>
    public partial class ProgressComplete : Form
    {
        /// <summary>
        /// Internal static SourcePath member
        /// </summary>
        internal static string SourcePath;
        /// <summary>
        /// Internal static logFile location member
        /// </summary>
        internal static string logFile;
        /// <summary>
        /// Log summary control that is later bound to the txtSummary control
        /// </summary>
        internal ArrayList logSummary;
        /// <summary>
        /// Implementation for Progress Complete status indicator form
        /// </summary>
        public ProgressComplete()
        {
            InitializeComponent();
            logSummary = new ArrayList();
        }
        /// <summary>
        /// Button click event handler for opening the log file.
        /// </summary>
        /// <param name="sender">Source object</param>
        /// <param name="e">Arguements included with source object</param>
        private void btnViewLog_Click(object sender, EventArgs e)
        {
            FileStream reader = null;
            string sourcePath = SourcePath;
            try
            {
                if (File.Exists(sourcePath))
                    sourcePath = sourcePath.Substring(0, sourcePath.LastIndexOf('\\'));
                reader = File.OpenRead(sourcePath + "\\OutputLog.txt");
            }
            catch (Exception error)
            {
                MainForm.association.listBoxOutput.Items.Add("The log file could not be found " +
                    "at location: " + sourcePath + " Error details: " + error.ToString());
                MessageBox.Show("The log file could not be found at location:\n" + sourcePath +
                    ".\nArchives have not been precessed yet, and thusly\n" +
                    "there is no log file to view.",
                    "Logfile Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
                return;
            }
            Process.Start("C:\\Windows\\System32\\notepad.exe", logFile);
        }
        /// <summary>
        /// Form.Shown event handler for ProgressComplete.Shown
        /// </summary>
        /// <param name="sender">Source object</param>
        /// <param name="e">Arguements included with source object</param>
        private void ProgressComplete_Shown(object sender, EventArgs e)
        {
            ProgressPage.mainFormReference.btnFindArchives.Enabled = true;
            ProgressPage.mainFormReference.btnSubmit.Enabled = false;
        }
    }
}