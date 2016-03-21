/*
 *****************************************************************************************************
 *
 *  MtpsPrepTool.EXE                           v-maklum                          September 19, 2007
 * 
 *  PROGRAM DESCRIPTION:
 *    This program accepts multiple Havana/Help 2.0 (.HxS file extension) archive(s) from either
 *    the first Migration Tool published by a-geralh or from Help Producer v2.2 and performs post
 *    processing there by making the specified Help 2.0 archive(s) compliant for publication to
 *    the most current version of MTPS (MSDN-TechNet Publishing System).
 *  
 *  TopicGuidAsso.CS
 * 
 *  CODE FILE DESCRIPTION:
 *    This file provides the implementation for data processing and adds a data relationship between
 *    html topic files and their associated GUID. The resulting Xml data files are stored as
 *    <help2.0_archive_name.HxS>_GUID_list.xml both where the archive is location and also included
 *    in the archive under the subfolder of <archive_root>/html/<help2.0_archive_name.HxS>_GUID_list.xml
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
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MtpsPrepTool
{
    /// <summary>
    /// Implementation for extended post processing to allow for better
    /// data association between document topics and their assigned MTPS
    /// AssetID identification data (GUID).
    /// </summary>
    public partial class TopicGuidAsso
    {
        /// <summary>
        /// Default constructor for TopicGuidAsso object
        /// </summary>
        public TopicGuidAsso()
        {
            helpArchive = Directory.GetCurrentDirectory();
            sourcePath = Directory.GetCurrentDirectory();
            if (File.Exists(helpArchive + "_GUID_List.xml"))
                xmlFile.ReadXml(sourcePath + "\\" + helpArchive + "_GUID_List.xml", XmlReadMode.InferSchema);
            else
                xmlFile = new DataSet();
            if (File.Exists(sourcePath + "\\TestOutput\\html\\" + helpArchive + "_GUID_List.xml"))
                archiveFile.ReadXml(sourcePath + "\\TestOutput\\html\\" +
                    helpArchive + "_GUID_List.xml", XmlReadMode.InferSchema);
            else
                archiveFile = new DataSet("TopicGuidItems");
            titlesToOccurances = new SortedDictionary<string, int>();
        }
        /// <summary>
        /// SortedDictionary key/value pair collectin for tracking
        /// topic title to occurances in the xml data files.
        /// </summary>
        private SortedDictionary<string, int> titlesToOccurances;
        /// <summary>
        /// ListBox MtpsPrepTool.MainFrom.listBoxOutput reference for logging
        /// </summary>
        internal ListBox listBoxOutput;
        /// <summary>
        /// Relative help archive path for a given help archive
        /// </summary>
        private string helpArchive;
        /// <summary>
        /// Property HelpArchive (string) - get/set
        /// </summary>
        internal string HelpArchive
        {
            get { return helpArchive; }
            set { helpArchive = value; }
        }
        /// <summary>
        /// Absolute path that MtpsPrepTool is currently pointing
        /// at containing help archives for processing.
        /// (Can be either a short no-spaces local path, or a mapped network drive)
        /// </summary>
        private string sourcePath;
        /// <summary>
        /// Property SourcePath (sting) - get/set
        /// </summary>
        internal string SourcePath
        {
            get { return sourcePath; }
            set { sourcePath = value; }
        }
        /// <summary>
        /// DataSet side-by-side data that has topic-GUID association
        /// </summary>
        private DataSet xmlFile;
        /// <summary>
        /// Property XmlFile (DataSet) - Set or get the
        /// topic-GUID side-by-side association data
        /// </summary>
        internal DataSet XmlFile
        {
            get { return xmlFile; }
            set { xmlFile = value; }
        }
        /// <summary>
        /// DataSet mirror image of xmlFile in the help archive.
        /// </summary>
        private DataSet archiveFile;
        /// <summary>
        /// Property ArchiveFile (DataSet) - get/set mirror image
        /// of xmlFile in the help archive
        /// </summary>
        internal DataSet ArchiveFile
        {
            get { return archiveFile; }
            set { archiveFile = value; }
        }
        /// <summary>
        /// Writes out the topic-GUID associated data file and performs
        /// neccessary checking that both xml data file locations are
        /// identical and have neccessary file attributes.
        /// </summary>
        /// <returns>True if checking and write out succeeded, otherwise false</returns>
        internal bool WriteXmlFile()
        {
            // 1) Get both old flat no associate text files
            string[] archive = null,
                sidebyside = null;
            // 2) Reset xmlFile DataSet and archiveFile DataSet for reuse
            TopicGuidAsso assoc = new TopicGuidAsso();
            xmlFile = assoc.xmlFile;
            archiveFile = assoc.archiveFile;
            titlesToOccurances = assoc.titlesToOccurances;
            try
            {
                if (File.Exists(helpArchive + "_GUID_List.xml"))
                    return true;
                archive = File.ReadAllLines(sourcePath + "\\TestOutput\\html\\a_GUID_List.txt");
                File.Delete(sourcePath + "\\TestOutput\\html\\a_GUID_List.txt");
                sidebyside = File.ReadAllLines(helpArchive + "_GUID_List.txt");
                File.Delete(helpArchive + "_GUID_List.txt");
                if (archive.Length != sidebyside.Length)
                    throw new ApplicationException("The original flat text data files do not match.\n" +
                        "Locations:\n " + sourcePath + "\\TestOutput\\html\\a_GUID_List.txt\nAnd also: " +
                        helpArchive + "_GUID_List.txt");
            }
            catch (Exception loaderror)
            {
                MessageBox.Show("An error ocurred while loading the no associaton flat text data files.\n" +
                    "Press View Log button for more details.\n", "Flat File Load Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.ServiceNotification, false);
                listBoxOutput.Items.Add("An error ocurred while loading the flat text data files. Error details:");
                listBoxOutput.Items.Add(loaderror.ToString());
                MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
                return false;
            }
            // 3) Parse for Topic titles
            // Get all html file topic paths
            try
            {
                string[] topicPaths = Directory.GetFiles(sourcePath + "\\TestOutput\\html");
                // Setup RegEx regular expression for parsing out Topic Title
                Regex expression1 = new Regex("(?<=<TITLE>).*(?=</TITLE>)", RegexOptions.IgnoreCase),
                    expression2 = new Regex(
                    "(?<=<MSHelp:Attr Name=\"AssetID\" Value=\").*(?=\" {0,1}/>)", RegexOptions.IgnoreCase);

                // 4) Covert non-readable flat files to their Xml equivalents
                DataColumn[] dataColumns = new DataColumn[] 
                {
                    new DataColumn("ItemID", typeof(Int32), "", MappingType.Attribute),
                    new DataColumn("topictitle", typeof(string), "", MappingType.Attribute),
                    new DataColumn("topicguid", typeof(string), "", MappingType.Attribute)
                };
                archiveFile.Tables.Add("TopicGuidItem");
                archiveFile.Tables["TopicGuidItem"].Columns.AddRange(dataColumns);
                archiveFile.Tables["TopicGuidItem"].PrimaryKey =
                    new DataColumn[] { archiveFile.Tables["TopicGuidItem"].Columns["ItemID"] };
                archiveFile.Tables["TopicGuidItem"].Columns["topicguid"].Unique = true;
                for (int i = 0; i < archive.Length; ++i)
                {
                    Match match1 = expression1.Match(File.ReadAllText(topicPaths[i])),
                        match2 = expression2.Match(archive[i]);
                    archiveFile.Tables["TopicGuidItem"].LoadDataRow(
                        new object[] { i + 1, match1.Value, match2.Value }, true);
                }
                string archiveName = helpArchive.Remove(0, helpArchive.LastIndexOf('\\') + 1);
                xmlFile = archiveFile.Copy(); // Make xmlFile DataSet initially identical to archiveFile DataSet
                archiveFile.WriteXml(sourcePath + "\\TestOutput\\html\\" + archiveName +
                    "_GUID_List.xml", XmlWriteMode.WriteSchema);
                xmlFile.WriteXml(sourcePath + "\\" + archiveName + "_GUID_List.xml", XmlWriteMode.WriteSchema);
                ArrayList filelist_HxF = new ArrayList(File.ReadAllLines(
                    Directory.GetFiles(sourcePath + "\\TestOutput", "*.HxF", SearchOption.TopDirectoryOnly)[0] ));
                if (!filelist_HxF.Contains("\t<File Url=\"html\\" + archiveName + "_GUID_List.xml\" />"))
                {
                    filelist_HxF.Insert(filelist_HxF.Count - 1, "\t<File Url=\"html\\" +
                        archiveName + "_GUID_List.xml\" />");
                    string[] lines = new string[filelist_HxF.Count];
                    filelist_HxF.CopyTo(lines);
                    File.WriteAllLines( Directory.GetFiles(sourcePath +
                        "\\TestOutput", "*.HxF", SearchOption.TopDirectoryOnly)[0], lines);
                }
                MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
                if (SynchronizationCheck())
                {
                    MainForm.progresscomplete.logSummary.Add("Xml Processing of AssetID GUIDS and Topics for " +
                        helpArchive.Remove(0, helpArchive.LastIndexOf('\\') + 1) + " succeeded.");
                    return true;
                }
                else
                {
                    MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
                    return false;
                }
            }
            catch (Exception error)
            {
                MessageBox.Show("An error ocurred while processing the xml data files and/or flat text data files.\n" +
                    "Press View Log button for more details.\n", "Data File(s) Processing Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification,
                    false);
                listBoxOutput.Items.Add("An error ocurred while processing the xml data files and/or flat " +
                    "text data files. Error details:");
                MainForm.progresscomplete.logSummary.Add("An error ocurred while processing the xml data " +
                    "files and/or flat text data files.");
                listBoxOutput.Items.Add(error.ToString());
                MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
                return false;
            }
        }
        /// <summary>
        /// Removes a given Topic with its associated GUID at build time
        /// from the topic-GUID association data file. 
        /// </summary>
        /// <param name="topicTitle">Topic title to remove if in both
        /// data file locations, but not more than once in both locations</param>
        /// <param name="associatedGUID">Associated GUID to remove in
        /// both data file locations, but not more than once in both
        /// locations</param>
        /// <returns>True if removal succeeded, otherwise false</returns>
        internal bool RemoveTopicData(string topicTitle, string associatedGUID)
        {
            try
            {
                string archiveName = helpArchive.Remove(0, helpArchive.LastIndexOf('\\') + 1);
                DataRow[] rows = xmlFile.Tables["TopicGuidItem"].Select(
                    "topictitle = '" + topicTitle + "' AND topicguid = '" + associatedGUID + "'",
                    "ItemID ASC, topictitle, topicguid");
                if (0 != rows.Length)
                {
                    foreach (DataRow row in rows)
                        xmlFile.Tables["TopicGuidItem"].Rows.Remove(row); // remove selected row
                }
                else
                    throw new ApplicationException("The specified topictitle with associated topicguid does not exist.");
                rows = archiveFile.Tables["TopicGuidItem"].Select(
                    "topictitle = '" + topicTitle + "' AND topicguid = '" + associatedGUID + "'",
                    "ItemID ASC, topictitle, topicguid");
                if (0 != rows.Length)
                {
                    foreach (DataRow row in rows)
                        archiveFile.Tables["TopicGuidItem"].Rows.Remove(row); // remove selected row
                }
                else
                    throw new ApplicationException("The specified topictitle with associated topicguid does not exist.");
                xmlFile.WriteXml(sourcePath + "\\" + archiveName + "_GUID_List.xml", XmlWriteMode.WriteSchema);
                archiveFile.WriteXml(sourcePath + "\\TestOutput\\html\\" + archiveName +
                    "_GUID_List.xml", XmlWriteMode.WriteSchema);
                DataSet org1 = xmlFile.Copy(), org2 = archiveFile.Copy();
                if (SynchronizationCheck())
                {
                    xmlFile.Clear();
                    xmlFile = org1.Copy();
                    archiveFile.Clear();
                    archiveFile = org2.Copy();
                    MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
                    return true;
                }
                else
                    throw new ApplicationException("The xml files side by side the help archive " +
                        "and inside the help archive do not match.");
            }
            catch (Exception error)
            {
                MessageBox.Show("An error occured when trying to remove the topic:\n" + topicTitle +
                    "\nwith associated GUID:\n" + associatedGUID + "\nPress View Log button for more details.",
                    "Remove Topic Data Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification, false);
                listBoxOutput.Items.Add("An error occured when trying to remove the topic: " +
                    topicTitle + " with associated GUID: " + associatedGUID + ".");
                listBoxOutput.Items.Add("Error details: " + error.ToString());
                MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
                return false;
            }
        }
        /// <summary>
        /// Adds a new Topic with its new GUID at runtime time to
        /// the topic-GUID association data file.
        /// </summary>
        /// <param name="topicTitle">Topic title to add</param>
        /// <param name="newGUID">New static GUID to add with new Topic</param>
        /// <returns>True if the topic data doesn't already exist in either
        /// data file (both in the archive and side by side with the archive),
        /// otherwise false if either the Topic title or the GUID already
        /// appear.</returns>
        internal bool AddTopicData(string topicTitle, string newGUID)
        {
            try
            {
                string archiveName = helpArchive.Remove(0, helpArchive.LastIndexOf('\\') + 1);
                DataRow[] rows = xmlFile.Tables["TopicGuidItem"].Select(
                    "topictitle = '" + topicTitle + "' AND topicguid = '" + newGUID + "'",
                    "ItemID ASC, topictitle, topicguid"),
                    allRows = xmlFile.Tables["TopicGuidItem"].Select("", "ItemID ASC, topictitle, topicguid");
                int nextPriKey = ((int)allRows[allRows.Length - 1].ItemArray[0] ) + 1;
                if (0 == rows.Length)
                    xmlFile.Tables["TopicGuidItem"].Rows.Add(
                        new object[] { nextPriKey, topicTitle, newGUID }); // insert row
                else
                    throw new ApplicationException("The specified topictitle with associated topicguid already exists.");
                rows = archiveFile.Tables["TopicGuidItem"].Select(
                    "topictitle = '" + topicTitle + "' AND topicguid = '" + newGUID + "'",
                    "ItemID ASC, topictitle, topicguid");
                allRows = archiveFile.Tables["TopicGuidItem"].Select("", "ItemID ASC, topictitle, topicguid");
                nextPriKey = ((int)allRows[allRows.Length - 1].ItemArray[0]) + 1;
                if (0 == rows.Length)
                    archiveFile.Tables["TopicGuidItem"].Rows.Add(
                        new object[] { nextPriKey, topicTitle, newGUID }); // insert row
                else
                    throw new ApplicationException("The specified topictitle with associated topicguid already exists.");
                xmlFile.WriteXml(sourcePath + "\\" + archiveName + "_GUID_List.xml", XmlWriteMode.WriteSchema);
                archiveFile.WriteXml(sourcePath + "\\TestOutput\\html\\" + archiveName +
                    "_GUID_List.xml", XmlWriteMode.WriteSchema);
                DataSet org1 = xmlFile.Copy(), org2 = archiveFile.Copy();
                if (SynchronizationCheck())
                {
                    xmlFile.Clear();
                    xmlFile = org1.Copy();
                    archiveFile.Clear();
                    archiveFile = org2.Copy();
                    MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
                    return true;
                }
                else
                    throw new ApplicationException("The xml files side by side the help archive " +
                        "and inside the help archive do not match.");
            }
            catch (Exception error)
            {
                MessageBox.Show("An error occured when trying to add the topic:\n" + topicTitle +
                    "\nwith associated GUID:\n" + newGUID + "\nPress View Log button for more details.",
                    "Add Topic Data Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification, false);
                listBoxOutput.Items.Add("An error occured when trying to add the topic: " +
                    topicTitle + " with associated GUID: " + newGUID + ".");
                listBoxOutput.Items.Add("Error details: " + error.ToString());
                MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
                return false;
            }
        }
        /// <summary>
        /// Helper method responsible for checking that the data in a
        /// given help arhive and also side by side with the archive are
        /// identical.
        /// </summary>
        /// <returns>True if identical, otherwise false</returns>
        private bool SynchronizationCheck()
        {
            try
            {
                string archiveName = helpArchive.Remove(0, helpArchive.LastIndexOf('\\') + 1);
                xmlFile.Clear();
                xmlFile.ReadXml(sourcePath + "\\" + archiveName + "_GUID_List.xml", XmlReadMode.ReadSchema);
                archiveFile.Clear();
                archiveFile.ReadXml(sourcePath +
                    "\\TestOutput\\html\\" + archiveName + "_GUID_List.xml", XmlReadMode.ReadSchema);
                int length1 = xmlFile.Tables["TopicGuidItem"].Rows.Count,
                    length2 = archiveFile.Tables["TopicGuidItem"].Rows.Count;
                if (length1 != length2)
                    throw new ApplicationException("The Xml data files:\n" + archiveName +
                        "_GUID_List.xml\n and " + sourcePath + "\\TestOutput\\html\\a_GUID" +
                        "_List.xml\nAre NOT identical.\n" +
                        "Mtps Publishing Preparation Tool will now attempt to restore your " +
                        "Xml data file(s).");
                int length = (length1 < length2) ? length1 : length2;
                for (int i = 0; i < length; ++i)
                {
                    for (int j = 0; j < xmlFile.Tables["TopicGuidItem"].Rows[i].ItemArray.Length; ++j)
                    {
                        if (Convert.ToString(xmlFile.Tables["TopicGuidItem"].Rows[i].ItemArray[j]) !=
                            Convert.ToString(archiveFile.Tables["TopicGuidItem"].Rows[i].ItemArray[j]))
                        {
                            MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
                            return false;
                        }
                    }
                }
                MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
                return true;
            }
            catch (Exception PROBLEM)
            {
                listBoxOutput.Items.Add("There was an error attempting to load your " +
                    "archives Xml Topic-GUID association data file(s). Error details:");
                listBoxOutput.Items.Add(PROBLEM.ToString());
                MessageBox.Show("There was an error attempting to load your\n" +
                    "archives Xml Topic-GUID association data file(s).\nPress View Log " +
                    "button for more details.", "Data File Load Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.ServiceNotification, false);
                MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
                return false;                
            }
        }
        /// <summary>
        /// Helper method that finds the html topic file's original associated GUID in the xml
        /// and reinserts it if topic is unchanged, adds the html topic file with a new GUID if
        /// it is a new topic, or deletes the topic and associated GUID from the Xml data files
        /// if it is found that a topic was removed after rebuilding from help producer.
        /// </summary>
        /// <param name="pathHtmlFile">Path to html topic file from help producer rebuild</param>
        /// <returns>True if updates succeeded, otherwise false</returns>
        internal bool UpdateXml(string pathHtmlFile)
        {
            try
            {
                List<string> source = new List<string>(File.ReadAllLines(pathHtmlFile));
                Regex expression = new Regex("(?<=<TITLE>).*(?=</TITLE>)", RegexOptions.IgnoreCase);
                string inputText = File.ReadAllText(pathHtmlFile);
                Match match = expression.Match(inputText);
                if (titlesToOccurances.ContainsKey(match.Value))
                    titlesToOccurances[match.Value] += 1;
                else
                    titlesToOccurances.Add(match.Value, 0);
                if (!SynchronizationCheck())
                    ResynchronizeData();
                DataRow[] rows = xmlFile.Tables["TopicGuidItem"].Select("topictitle = '" +
                    match.Value + "'", "ItemID ASC, topictitle, topicguid");
                string[] topicFiles = Directory.GetFiles(sourcePath +
                    "\\TestOutput\\html", "*.htm?", SearchOption.TopDirectoryOnly);
                string correctGUID = null;
                if (1 <= rows.Length && titlesToOccurances[match.Value] < rows.Length)
                {
                    foreach (string str in source)
                        if (str.Contains("<MSHelp:Attr Name=\"AssetID\" Value=\"")) // Check for existing AssetID
                        {
                            listBoxOutput.Items.Add("The topic " + match.Value + " has AssetID " +
                                (string)rows[titlesToOccurances[match.Value]]["topicguid"] +
                                " appropriately assigned from Xml data file.");
                            if (pathHtmlFile == topicFiles[topicFiles.Length - 1])
                                RemoveXmlDuplicates(pathHtmlFile, rows);
                            MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
                            return true;
                        }
                }
                if (1 <= rows.Length && titlesToOccurances[match.Value] < rows.Length)
                {
                    // The topic is unchanged and needs its GUID reassigned
                    listBoxOutput.Items.Add("Found Topic " + match.Value +
                        " has no AssetID. Reassigning original GUID " + correctGUID);
                    correctGUID = (string)rows[titlesToOccurances[match.Value]]["topicguid"];
                    foreach (string str in source)
                        if (str.Contains("</xml>"))
                        {
                            source.Insert(source.IndexOf(str),
                                "\t\t\t<MSHelp:Attr Name=\"AssetID\" Value=\"" + correctGUID + "\" />");
                            File.WriteAllLines(pathHtmlFile, source.ToArray());
                            break;
                        }
                    if (pathHtmlFile == topicFiles[topicFiles.Length - 1])
                        rows = RemoveXmlDuplicates(pathHtmlFile, rows);
                    MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
                    return true;
                }
                if (0 == rows.Length)
                {   // The topic is new and needs a new GUID or Xml file needs to be updated
                    Guid newGuid = Guid.Empty;
                    foreach (string str in source)
                    {
                        if (str.Contains("<MSHelp:Attr Name=\"AssetID\" Value=\"")) // Check for existing AssetID
                        {
                            listBoxOutput.Items.Add("Found Topic " + match.Value +
                                " that has an AssetID, but not found in Xml data file. Updating now...");
                            break;
                        }
                        else if (str.Contains("</xml>")) // Check for end of Xml data island
                        {
                            newGuid = Guid.NewGuid();
                            listBoxOutput.Items.Add("Found new Topic " + match.Value +
                                " and has no AssetID. Assigning a new GUID " + newGuid.ToString());
                            source.Insert(source.IndexOf(str),
                                "\t\t\t<MSHelp:Attr Name=\"AssetID\" Value=\"" +
                                newGuid.ToString() + "\" />");
                            break;
                        }
                    }
                    if (!AddTopicData(match.Value, newGuid.ToString()))
                    {
                        MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
                        return false;
                    }
                    File.WriteAllLines(pathHtmlFile, source.ToArray());
                    if (pathHtmlFile == topicFiles[topicFiles.Length - 1])
                        rows = RemoveXmlDuplicates(pathHtmlFile, rows);
                    MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
                    return true;
                }
                MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
                return true;
            }
            catch (Exception error)
            {
                MessageBox.Show("An error occured while updating the Xml data files.\nPress View Log button " +
                    "for more details.", "Update Xml Data Files Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.ServiceNotification, false);
                listBoxOutput.Items.Add("An error occured while updating the Xml data files. Error details:");
                listBoxOutput.Items.Add(error.ToString());
                MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
                return false;
            }
        }
        /// <summary>
        /// Helper method that removes topics and their associated AssetID GUID from
        /// the Xml Data files after having found the corresponding topic was removed
        /// from the Help Producing build output .HxS which is input to this tool.
        /// </summary>
        /// <param name="pathHtmlFile">Path to the last Html Topic File in \TestOutput\html</param>
        /// <param name="rows">DataRow array from last select statement in UpdateXml()</param>
        /// <returns>Remaining DataRow array set after removal operation</returns>
        private DataRow[] RemoveXmlDuplicates(string pathHtmlFile, DataRow[] rows)
        {
            string[] topicFiles = Directory.GetFiles(sourcePath +
                "\\TestOutput\\html", "*.htm?", SearchOption.TopDirectoryOnly);
            if (pathHtmlFile == topicFiles[topicFiles.Length - 1]) // Check for Topics that were removed at end
            {
                listBoxOutput.Items.Add("Begin removal of Topic data that was removed by the author/editor.");
                rows = xmlFile.Tables["TopicGuidItem"].Select();
                for (int i = 0; i < rows.Length; ++i)
                    if (!titlesToOccurances.ContainsKey((string)rows[i].ItemArray[1]))
                    {
                        listBoxOutput.Items.Add("Topic " + (string)rows[i].ItemArray[1] +
                            " associated with GUID " + (string)rows[i].ItemArray[2] + " was removed.");
                        RemoveTopicData((string)rows[i].ItemArray[1], (string)rows[i].ItemArray[2]);
                        rows = xmlFile.Tables["TopicGuidItem"].Select();
                        i = -1;
                    }
                listBoxOutput.Items.Add("End removal of Topic data that was removed by the author/editor.");
            }
            MainForm.WriteLogFile(MainForm.logFile, MainForm.association.listBoxOutput);
            return rows;
        }
        /// <summary>
        /// Helper method responsible for rebalancing Xml data files after the
        /// data file checks provided by SynchronizationCheck() do not succeed.
        /// </summary>
        private void ResynchronizeData()
        {
            string archiveName = helpArchive.Remove(0, helpArchive.LastIndexOf('\\') + 1);
            xmlFile.Merge(archiveFile, true, MissingSchemaAction.Error);
            archiveFile.Merge(xmlFile, true, MissingSchemaAction.Error);
            xmlFile.WriteXml(sourcePath + "\\" + archiveName + "_GUID_List.xml", XmlWriteMode.WriteSchema);
            archiveFile.WriteXml(sourcePath + "\\TestOutput\\html\\" + archiveName +
                "_GUID_List.xml", XmlWriteMode.WriteSchema);
        }
    }
}