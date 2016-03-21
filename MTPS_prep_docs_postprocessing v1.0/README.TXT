• For this development and tech support for Tech Writers that regularly publish technical documents to TechNet and MSDN documentation websites, I was made owner of delivering a fully custom designed middle tier post processing application to integrate with a new document build/publish system for the Microsoft Solution Accelerators group.
• While under development, I also gave good quality customer tech support for helping customers to use the old legacy build system and also to eventually transition from it to the new documentation build system that in progress.
Environment: C#, .NET, XHTML, XML, XSLT, XSD, ADO.NET, ASP.NET XML Web Services (SOAP+WSDL) and regular expressions

The post review/take away learned lesson for this project is to pay as close attention about design requirements, and in this case was additional implementation of ASP.NET XML Web Service connection publishing endpoints exposed by the technical publishing service.


- MtpsPrepTool (Zebra) User Guide -

The following steps show the current MTPS Publish Process from a document project being authored in Microsoft Word 
2003 or Microsoft Word 2007 to the displaying of that document project in Microsoft TechNet Production Stagging 
website.
The order of the Publication Process is as follows:
 
Steps for Setup:
1)	The computer (running Windows XP or Vista) performing the publication must have either Microsoft Word 2003 
or Microsoft Word 2007 installed and also have access to the Word Document Project being published.
2)	(Help Producer Requirement) Install Visual Studio 2005 SDK from 
file://products/PUBLIC/Products/Developers/Visual%20Studio%202005/SDK/VsSDKFebruary2007.exe 
3)	Install Help Producer v2.2 from http://www.mgtek.com/helpproducer/downloads/.
4)	Help Producer installs as a trial and must apply a license before use 
(http://www.mgtek.com/helpproducer/sales/). Each license is aloud one per person per computer according to 
EULA. For testing only a license key will be provided to anyone needing one so they can build and later must be 
removed.
5)	Install MtpsPrepTool code named Zebra from http://mattktestsuppor/MtpsPrepTool/.
6)	Install MTPS Build System Client from http://b6udevmtpgwb03:82/en-us/library/bb310709.aspx 
7)	RMs: Know how or what you should do to create, or have created for you, your document project’s upper level 
HxT TOC file <Your Archive Name>.MTPS.HxT.
8)	Setup is complete.
Steps for Publication Process:
1)	Open .docx or .doc document project in Microsoft Word.
2)	Apply the Solutions Accelerator document template, if required otherwise skip this step. (Word 2007: Developer 
Tab-->Document Template)
3)	Configure Help Producer Project by clicking (Word 2007) on Add-Ins Tab-->MGTEK Help Producer : Configure 
Help Project button.
4)	Set the following Help Project settings (Configuration file will provide this later on):
a.	"Project->Project Properties->Help System & Theme" set to "Microsoft HTML Help 2.0 (MSDN 2.0)".
b.	"Filter->Advanced->Allow creation of PNG images" set to Unchecked/False.
c.	"Project->Variables->Create Copyright Link" set to True.
d.	"Project->Variables->Create Feedback Link" set to False.
e.	"Project->Variables->Enforce MSDN 2.0 Styles" set to True.
f.	"Project->Variables->Copyright URL" empty.
g.	"Filter->Mappings" configured as desired, especially taking note of the settings for "Source Style"
Two possible “Source Style” settings are under test currently. Depending on your document project’s 
needs, you can leave these settings as default/unchanged, use one of these test configurations, or use 
your own related settings:
i.	Heading 1 through Heading 6 mapped to "Element" as TOPIC - Topic,1 through Topic – Topic,6 
ii.	Heading 1 mapped to “Element” as TOPIC – TOPIC,1 and Heading 2 through Heading 6 mapped 
to “Element” as Heading 2 through Heading 6.
iii.	Heading 1 through Heading 3 mapped to “Element” as Topic – Topic,1 through Topic – Topic,3; 
Heading 4 mapped to “Element” as Heading 1; Heading 5 mapped to “Element” as Heading 2; 
Heading 6 mapped to “Element” as Heading 3
5)	Click the button labeled “MGTEK Help Producer : Export Help Project”. A subfolder named “output” will be 
created with Help 2.0 Export saved here in the path of <Your Document Project Path>\output.
6)	Click the button labeled “MGTEK Help Producer : Compile Help Project”. Your HxS Archive file is saved to the 
location of <Your Document Project Path>\output\Help.HxS.
7)	Rename <Your Document Project Path>\output\Help.HxS to a meaningful name related to your document 
project.
8)	Launch MtpsPrepTool (Code named Zebra).
9)	Paste or browse to the path of <Your Document Project Path>\output\<Your Archive Name>.HxS to HxS Folder 
text box.
10)	Click the “Find Archives” button.
11)	Click the “Set Archives Attribute” button and fill out information needed for publication date and text/html 
applied to rightrail/sidebar link for each HxS Archive detected. Title text is required, download forward link is 
optional.
12)	Click Create MTPS files.
13)	Click OK on popup dialogue if no download forward link was entered.
14)	Decide whether to allow MtpsPrepTool (Zebra) to manage your images or not for your document project by 
clicking OK or CANCEL.
15)	When processing finishes, you will have the following files outputted/written in the path of  <Your Document 
Project Path>\output\ which are the following:
a.	<Your Archive Name>.HxS_GUID_List.xml --> This file is the AssetID GUID list for your HxS Archive. 
Important: Do not delete/loose this file! It is required with <Your Archive Name>.MTPS.HxS to 
recompile through Help Producer/MtpsPrepTool (Zebra) again. A copy of this file is saved with your HxS 
Archive.
b.	<Your Archive Name>.HxS_ImageProcessing.log.txt --> This file is the log for image processing.
c.	<Your Archive Name>.MTPS.HxS --> This file is the post processed MTPS compliant HxS Archive.
d.	OutputLog.txt --> This file is the main log file for your run.
16)	Create upper level HxT TOC file <Your Archive Name>.MTPS.HxT and save it to <Your Document Project 
Path>\output\:
Important: RMs should consult their appropriate TechNet engagement process for the correct
specifications of this document. This Xml configuration file will determine exactly where your document 
project will become visible in MTPS Production Staging.
Example <Your Archive Name>.MTPS.HxT file:
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE HelpTOC>
<HelpTOC DTDVersion="1.0" PluginStyle="Flat">
	<HelpTOCNode NodeType="Regular" Title="<Your Document Set Title>">
		<HelpTOCNode NodeType="Regular" Title="<Your Document Project Title>">
			<HelpTOCNode NodeType="TOC" Title="<Your Document Project Title>" 
Url="<Your Archive Name>.MTPS"/>
		</HelpTOCNode>
	</HelpTOCNode>
</HelpTOC>
End of: Example <Your Archive Name>.MTPS.HxT
17)	Lauch MTPS Build System client.
18)	Click on DocDrop… in MTPS Build System client.
19)	Double click folders to navigate to the path <DocDrop Path>\TechNet\10\en-us.
20)	Copy and paste <Your Document Project Path>\output\<Your Archive Name>.MTPS.HxS to <DocDrop 
Path>\TechNet\10\en-us.
21)	Copy and paste <Your Document Project Path>\output\<Your Archive Name>.MTPS.HxT to <DocDrop 
Path>\TechNet\10\en-us.
22)	Click New… in MTPS Build System client.
23)	In MTPS Build Editor-->DocSet Selection-->Collections, expand the tree to TechNet-->10-->en-us and click on en-
us node.
24)	In MTPS Build Editor-->Docset Selection-->Collection Docsets: (technet\10\enus), select both the check boxes for 
<Your Archive Name>.MTPS.HxS and also <Your Archive Name>.MTPS.HxT.
25)	Click the button labeled &Next>> at the bottom right of MTPS Build Editor.
26)	Under MTPS Build Editor-->Build Summary, click the first line appearing under Build Summary and confirm you 
have selected <Your Archive Name>.MTPS.HxS and also <Your Archive Name>.MTPS.HxT.
27)	Click the button labeled “F&inish” that was in the same location as &Next>>. A dialogue titled “MTPS Build 
System Tack Progress” will appear displaying the success or failure of your build.
28)	Click on the button labeled “OK” to close “MTPS Build System Tack Progress”.
29)	Click on Reports-->Build Summary in the MTPS Build System client
30)	Expand TechNet-->10, and click on the node labeled “en-us”, and watch for when the column labeled  % 
Completed reaches 100% before proceeding to the next step.
31)	Browse to the MTPS Production Staging website root of TechNet at the location of 
http://tnstage.dns.microsoft.com/en-us/library/default.aspx to find your document project’s node and to 
view/review your results.
32)	You are done.

Features released with this version of MtpsPrepTool code named (Zebra):

1) Document Topic AssetID detection and insertion. (v2.0.0.3)
2) Checking for Art/Image format and appropriate size/dimentions. (v2.0.0.3)
3) Improved Xml data representation with a more precise document Topic to AssetID data association. (v2.0.0.4)
4) Improved image procesing and handeling.(v2.1)
5) Added html insertions for publication date and download link/sidebar. (v2.1)
6) Added enhancements UI to and support for any HxS Folder path (v2.2)

