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
 *  ExitEvent.CS
 * 
 *  CODE FILE DESCRIPTION:
 *    This file provides the implementation for ExitEvent custom event type implementation.
 * 
 *  BUILD REQUIREMENTS:
 *    This is a winform based C# application.  
 *    Requires adding project assembly references for all name spaces listed immediately below
 *    this identification header, including the project reference to MagickNet (MagickNet.dll)
 *    provided by Image Magick (http://www.imagemagick.org/script/index.php).
 * 
 *  RETURNS:
 *    n/a - See individual event implementations for use
 *
 *****************************************************************************************************
*/
 
using System;
using System.Threading;
using System.Windows.Forms;

namespace MtpsPrepTool
{
    /// <summary>
    /// ExitEvent delegate
    /// </summary>
    public delegate void CustomExitEvent(object sender, EventArgs e);
    /// <summary>
    /// ExitEvent abstract class
    /// </summary>
    public interface iExitEvent
    {
        /// <summary>
        /// Add ExitEvent type registeration
        /// </summary>
        /// <param name="exitevent">ExitEvent delegate</param>
        void Add_ExitEvent(CustomExitEvent exitevent);
        /// <summary>
        /// Remove ExitEvent type registeration
        /// </summary>
        /// <param name="exitevent">ExitEvent delegate</param>
        void Remove_ExitEvent(CustomExitEvent exitevent);
        /// <summary>
        /// Launch/Raise ExitEvent custom event
        /// </summary>
        void RaiseExitEvent();
    }
    /// <summary>
    /// Implementation class object for ExitEvent custom event type
    /// </summary>
    public partial class MainForm : iExitEvent
    {
        /// <summary>
        /// Internal delegate to call RaiseExitEvent()
        /// </summary>
        internal delegate void RaisingExitEvent();
        /// <summary>
        /// This delegate enables asynchronous calls for setting
        /// the Enabled property on a button control.
        /// </summary>
        /// <param name="status">The boolean status of Button.Enabled</param>
        private delegate void SetEnabledCallBack(bool status);
        /// <summary>
        /// This delegate enables asynchronous calls for setting
        /// the Lines property on a multiline TextBox control
        /// </summary>
        /// <param name="status">The string [] log summary lines</param>
        private delegate void SetTextBoxLines(string [] status);
        /// <summary>
        /// Static internal reference to call the operation RaiseExitEvent
        /// </summary>
        internal static RaisingExitEvent CallRaiseExitEvent;
        /// <summary>
        /// Determines if Exit custom event was raised
        /// </summary>
        private static bool exitRaised;
        /// <summary>
        /// Custom exit event declaration
        /// </summary>
        public event CustomExitEvent ExitEvent;
        /// <summary>
        /// Add ExitEvent type registeration
        /// </summary>
        /// <param name="exitevent">ExitEvent delegate</param>
        public void Add_ExitEvent(CustomExitEvent exitevent)
        {
            ExitEvent += new CustomExitEvent(exitevent);
        }
        /// <summary>
        /// Remove ExitEvent type registeration
        /// </summary>
        /// <param name="exitevent">ExitEvent delegate</param>
        public void Remove_ExitEvent(CustomExitEvent exitevent)
        {
            EventHandler.RemoveAll(exitevent, exitevent);
        }
        /// <summary>
        /// Raise ExitEvent custom event - Initiates All Threads and Processes to Terminate/Exit
        /// </summary>
        public void RaiseExitEvent()
        {
            ExitEvent = new CustomExitEvent(CustomExitEvent);
            Add_ExitEvent(ExitEvent);
            ExitEvent.Invoke(new object(), new EventArgs());
        }
        /// <summary>
        /// ExitEvent custom event handler
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Included event arguements</param>
        public void CustomExitEvent(object sender, EventArgs e)
        {
            if (null != processingThread &&
                processingThread.IsAlive)
            {
                if (decompile != null && decompile.IsAlive)
                    decompile.Abort();
                if (recompile != null && recompile.IsAlive)
                    recompile.Abort();
            }
            Thread safeThread1 = new Thread(new ThreadStart(ThreadBoolSafe)),
                safeThread2 = new Thread(new ThreadStart(ThreadLinesSafe));
            safeThread1.Start();
            HtmlmarkupAddins.cancelClicked = true;
            safeThread2.Start();
            Remove_ExitEvent(ExitEvent);
            if (null != processingThread && processingThread.IsAlive)
            {
                processingThread.Abort();
            }
            exitRaised = true;
        }
        /// <summary>
        /// This method is executed on the worker thread and makes
        /// thread-safe calls on the progresscomplete.txtSummary control.
        /// </summary>
        private void ThreadLinesSafe()
        {
            string[] status = new string[progresscomplete.logSummary.Count];
            progresscomplete.logSummary.CopyTo( status, 0 );
            SetText( status );
        }
        /// <summary>
        /// Helper method that provides a thread-safe means of
        /// setting the Lines property of multiline TextBox
        /// </summary>
        /// <param name="status"></param>
        private void SetText(string[] status)
        {
            if (progresscomplete.txtSummary.InvokeRequired)
            {
                SetTextBoxLines tl = new SetTextBoxLines(SetText);
                Invoke(tl, new object[] { status });
            }
            else
            {
                progresscomplete.txtSummary.Lines = status;
            }
        }
        /// <summary>
        /// This method is executed on the worker thread and makes
        /// a thread-safe call on the btnSubmit control.
        /// </summary>
        private void ThreadBoolSafe()
        {
            SetEnabled(true);
        }
        /// <summary>
        /// Helper method that provides a thread-safe means of
        /// setting the proverty of btnSubmit.Enabled
        /// </summary>
        private void SetEnabled(bool status)
        {
            if (btnSubmit.InvokeRequired)
            {
                SetEnabledCallBack d = new SetEnabledCallBack(SetEnabled);
                Invoke(d, new object[] { status });
            }
            else
            {
                btnSubmit.Enabled = status;
            }
        }
    }
}
