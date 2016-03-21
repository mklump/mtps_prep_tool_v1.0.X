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
 *  ProcessDrive.CS
 * 
 *  CODE FILE DESCRIPTION:
 *    This file provides the implementation mounting and dismounting a temporary drive such
 *    that the decompilation and recompilation processes will not fail because of a path that
 *    is too long.
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
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;

namespace MtpsPrepTool
{
    /// <summary>
    /// Implementation class for Mounting and Dismounting
    /// a temporary processing drive
    /// </summary>
    class ProcessingDrive
    {
        /// <summary>
        /// Thread that performs mounting a temporary drive
        /// </summary>
        private Thread threadMount;
        /// <summary>
        /// Thread that performs dismounting a temporary drive
        /// </summary>
        private static Thread threadDismount;
        /// <summary>
        /// Drive letter used to mount the specific temporary drive
        /// </summary>
        private static char driveLetter;
        /// <summary>
        /// Original source path specification before mounting temporary drive
        /// </summary>
        internal static string originalSourcePath;
        /// <summary>
        /// Property used to set/get driveLetter specification
        /// </summary>
        public char DriveLetter
        {
            set { driveLetter = value; }
            get { return driveLetter; }
        }
        /// <summary>
        /// ProcessingDrive default constructor: Checks for the first
        /// available drive letter and assigns it as the Processing 
        /// Temporary Drive
        /// </summary>
        public ProcessingDrive()
        {
            string driveLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            for (int i = driveLetters.Length - 1; i > -1; --i)
                if( !( System.IO.Directory.Exists(driveLetters[i] + ":") ) )
                {
                    ProcessingDriveInit( driveLetters[i] );
                    break;
                }
        }
        /// <summary>
        /// Helper method that initializes the assigned Temporary Processing Drive
        /// </summary>
        /// <param name="driveLetter">Temporary processing drive letter 
        /// specification</param>
        private void ProcessingDriveInit(char driveLetter)
        {
            threadMount = new Thread(new ThreadStart(TSDelegateMountDrive));
            threadDismount = new Thread(new ThreadStart(TSDelegateDismountDrive));
            DriveLetter = driveLetter;
        }
        /// <summary>
        /// Operation/method that calls threadMount to mount
        /// a temporary drive letter as a substitute for any
        /// short path or extra long (greater than 256 characters)
        /// exceptionally long path
        /// </summary>
        internal void MountDrive()
        {
            originalSourcePath = MainForm.association.SourcePath;
            threadMount.Start();
            threadMount.Join();
            MainForm.association.SourcePath = driveLetter + ":";
        }
        /// <summary>
        /// Operation/method that calls threadDismount to dismount
        /// a temporary drive letter as a substitute for any
        /// short path or extra long (greater than 256 characters)
        /// exceptionally long path
        /// </summary>
        internal void DismountDrive()
        {
            threadDismount.Start();
            threadDismount.Join();
            MainForm.association.SourcePath = originalSourcePath;
        }
        /// <summary>
        /// ThreadStart void () signature delegate responsible for
        /// mounting a temporary processing drive
        /// </summary>
        private void TSDelegateMountDrive()
        {
            try
            {
                Process mount = new Process();
                mount.StartInfo.FileName = @"C:\WINDOWS\system32\subst.exe";
                mount.StartInfo.Arguments = driveLetter + ": \"" +
                    MainForm.association.SourcePath + "\"";
                mount.Start();
                mount.WaitForExit(3000);
            }
            catch (ObjectDisposedException e)
            {
                throw (System.ComponentModel.Win32Exception)
                    Convert.ChangeType(e,
                    typeof(System.ComponentModel.Win32Exception));
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                throw (InvalidOperationException)
                    Convert.ChangeType(e, typeof(InvalidOperationException));
            }
            catch (InvalidOperationException e)
            {
                throw (SystemException)
                    Convert.ChangeType(e,
                    typeof(SystemException));
            }
            catch (SystemException e)
            {
                throw (ApplicationException)
                    Convert.ChangeType(e,
                    typeof(ApplicationException));
            }
            catch (ApplicationException error)
            {
                MainForm.association.listBoxOutput.Items.Add("An error occured " +
                    "while attempting to mount the temporary processing drive " +
                    "letter folder sustitute of " + driveLetter +
                    ". Error details: " + error.ToString());
                MainForm.progresscomplete.logSummary.Add("An error occured " +
                    "attempting to mount temporary drive " + driveLetter + ":.");
                MessageBox.Show("An error occured while attempting to mount the " +
                    "temporary drive " + driveLetter + ":.\nPlease see the log file " +
                    "OutputLog.txt.\n", "Mount Temporary Drive Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.ServiceNotification, false);
                MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
            }
        }
        /// <summary>
        /// ThreadStart void () signature delegate responsible for
        /// dismounting a temporary processing drive
        /// </summary>
        private void TSDelegateDismountDrive()
        {
            try
            {
                Process mount = new Process();
                mount.StartInfo.FileName = @"C:\WINDOWS\system32\subst.exe";
                mount.StartInfo.Arguments = "/D " + driveLetter + ":";
                mount.Start();
                mount.WaitForExit(3000);
            }
            catch (ObjectDisposedException e)
            {
                throw (System.ComponentModel.Win32Exception)
                    Convert.ChangeType(e,
                    typeof(System.ComponentModel.Win32Exception));
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                throw (InvalidOperationException)
                    Convert.ChangeType(e, typeof(InvalidOperationException));
            }
            catch (InvalidOperationException e)
            {
                throw (SystemException)
                    Convert.ChangeType(e,
                    typeof(SystemException));
            }
            catch (SystemException e)
            {
                throw (ApplicationException)
                    Convert.ChangeType(e,
                    typeof(ApplicationException));
            }
            catch (ApplicationException error)
            {
                MainForm.association.listBoxOutput.Items.Add("An error occured " +
                    "while attempting to dismount the temporary processing drive " +
                    "letter folder sustitute of " + driveLetter +
                    ". Error details: " + error.ToString());
                MainForm.progresscomplete.logSummary.Add("An error occured " +
                    "attempting to dismount temporary drive " + driveLetter + ":.");
                MessageBox.Show("An error occured while attempting to dismount the " +
                    "temporary drive " + driveLetter + ":.\nPlease see the log file " +
                    "OutputLog.txt.\n", "Dismount Temporary Drive Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.ServiceNotification, false);
                MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
            }
        } // End of void TSDelegateDismountDrive()
    } // End of class ProcessingDrive
}
