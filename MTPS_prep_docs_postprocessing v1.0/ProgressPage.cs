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
 *  ProgressPage.CS
 * 
 *  CODE FILE DESCRIPTION:
 *    This file provides the implementation for ProgressPage child form of pnlDisplay in MainForm.
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MtpsPrepTool
{
    /// <summary>
    /// Implementation for the Progress indicator form
    /// </summary>
    public partial class ProgressPage : Form
    {
        /// <summary>
        /// Static MainForm reference
        /// </summary>
        internal static MainForm mainFormReference;
        /// <summary>
        /// Default constructor
        /// </summary>
        public ProgressPage()
        {
            InitializeComponent();
        }
        /// <summary>
        /// btnCancel click event handler
        /// </summary>
        /// <param name="sender">Source object</param>
        /// <param name="e">Arguements included with source object</param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            mainFormReference.RaiseExitEvent();
        }
        /// <summary>
        /// Form.Shown event handler for ProgressPage.Shown
        /// </summary>
        /// <param name="sender">Source object</param>
        /// <param name="e">Arguements included with source object</param>
        private void ProgressPage_Shown(object sender, EventArgs e)
        {
            mainFormReference.btnFindArchives.Enabled = false;
            mainFormReference.btnSetArchiveAttributes.Enabled = false;
            mainFormReference.btnSubmit.Enabled = false;
        }
    }
}