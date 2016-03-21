

window.onload=LoadPage;
window.onunload=Window_Unload;
window.onresize=ResizeWindow;
window.onbeforeprint = set_to_print;
window.onafterprint = reset_form;

var vbDeclaration;
var csLang;
var cLang;
var jsharpLang;
var jsLang;

var scrollPos = 0;

var inheritedMembers;
var protectedMembers;
var netcfMembersOnly;

/*************************************************************
// Begin WSS Specific Change.
*************************************************************/
var currentMenu = null;
var blockHideMe = false;
/*************************************************************
// End WSS Specific Change.
*************************************************************/


// Initialize array of collapsed section image IDs

var collapsedSections = new Array();
var collapsedSectionsInitialized = false;

function InitCollapsedSections()
{
	var i = 0;
	var imageId = Load("imageValue" + i);
		
	while (imageId != null)
	{
		collapsedSections[imageId] = "not expanded";
		i++;
		imageId = Load("imageValue" + i);
	}
}

var noReentry = false;

function OnLoadImage()
{
    if (noReentry) return;
    if (!collapsedSectionsInitialized) { InitCollapsedSections(); collapsedSectionsInitialized = true; }
    
    elem = event.srcElement;
        
    if (ShouldExpand(elem))
    {
        noReentry = true;
        
        try
        {
			elem.src = collapseImage.src;
			ExpandSection(elem);
  			RemoveCollapsedItem(elem.id);
			collapsedSections[elem.id] = "expanded";
        }
        catch (e)
        {
        }
        
        noReentry = false;
    }
}

function ShouldExpand(elem)
{
    return (collapsedSections[elem.id] == null) || (Load("IsFirstPage") == null);
}



/*  
**********
**********   Begin
**********
*/

function LoadPage()
{
    if(IsEvilUrl())
    {
        ReloadEvilUrlAsGoodUrl();
        return; // in just a moment, page will reload.
    }
    // show correct language
    LoadLanguages();
    LoadMembersOptions();
    
    Set_up_checkboxes();

    DisplayLanguages();
    
    if(inheritedMembers=="off")
        DisplayInheritedMembers();
    if(protectedMembers=="off")
        DisplayProtectedMembers();
    if(netcfMembersOnly=="on")
        DisplayNETCFMembers();
        
    ChangeMembersOptionsFilterLabel();
    
    LoadSections();
    ResizeWindow();

    // vs70.js did this to allow up/down arrow scrolling, I think
    try { mainSection.setActive(); } catch(e) { }

    //set the scroll position
    try{mainSection.scrollTop = scrollPos;}
    catch(e){}

    /*************************************************************
    // Begin WSS Specific Change.
    *************************************************************/
    Hide_LangFilter();
    document.body.onclick = bodyOnClick;
    //initReftips();
    
    initializeMenu("InfoMenu", "InfoMenuActuator");
    
    //START: Feedback Integration

    try {
      if(newFeedback)
      {
        HeadFeedBack(headfb);
        var first=true;
        document.feedback = new FeedBack(L_alias, L_product, L_deliverable, L_productversion, L_docversion, L_FeedBackDivID);
        document.feedback.StartFeedBack(fb,first);
      }
      else
      {
        var first=true;
        document.FdBack = new FdBack(L_alias, L_product, L_deliverable, L_productversion, L_docversion, L_FeedBackDivID);
        document.FdBack.StartFdBack(fb);
      }
    }
    catch(e) {}

    //END: Feedback Integration
    //START: Search fix
    try {
      var spanElements = document.all.tags("span");
      for(i = 0; i < spanElements.length; ++i)
      {
        if(spanElements[i].id != null)
        {
            if(spanElements[i].id == "sf_LangFilter")
		  spanElements[i].innerHTML = L_sf_LangFilter;
            if(spanElements[i].id == "sf_All")
		  spanElements[i].innerHTML = L_sf_All;
            if(spanElements[i].id == "sf_Multiple")
		  spanElements[i].innerHTML = L_sf_Multiple;
            if(spanElements[i].id == "sf_VB")
		  spanElements[i].innerHTML = L_sf_VB;
            if(spanElements[i].id == "sf_Declaration")
		  spanElements[i].innerHTML = L_sf_Declaration;
            if(spanElements[i].id == "sf_Usage")
		  spanElements[i].innerHTML = L_sf_Usage;
            if(spanElements[i].id == "sf_CSharp")
		  spanElements[i].innerHTML =L_sf_CSharp;
            if(spanElements[i].id == "sf_CPlus")
		  spanElements[i].innerHTML = L_sf_CPlus;
            if(spanElements[i].id == "sf_JSharp")
		  spanElements[i].innerHTML = L_sf_JSharp;
            if(spanElements[i].id == "sf_JScript")
		  spanElements[i].innerHTML = L_sf_JScript;
            if(spanElements[i].id == "sf_Xml")
		  spanElements[i].innerHTML = L_sf_Xml;
            if(spanElements[i].id == "sf_Html")
		  spanElements[i].innerHTML = L_sf_Html;
            if(spanElements[i].id == "sf_CollAl")
		  spanElements[i].innerHTML = L_sf_CollAl;
            if(spanElements[i].id == "sf_ExpAll")
		  spanElements[i].innerHTML = L_sf_ExpAll;
            if(spanElements[i].id == "sf_Roles")
		  spanElements[i].innerHTML = L_sf_Roles;
            if(spanElements[i].id == "sf_Info")
		  spanElements[i].innerHTML = L_sf_Info;
            if(spanElements[i].id == "sf_MemAll")
		  spanElements[i].innerHTML = L_sf_MemAll;
            if(spanElements[i].id == "sf_MemFilt")
		  spanElements[i].innerHTML = L_sf_MemFilt;
            if(spanElements[i].id == "sf_MemInh")
		  spanElements[i].innerHTML = L_sf_MemInh;
            if(spanElements[i].id == "sf_MemPro")
		  spanElements[i].innerHTML = L_sf_MemPro;
            if(spanElements[i].id == "sf_MemCom")
		  spanElements[i].innerHTML = L_sf_MemCom;
	} //if
       } //for
    } //try
    catch(e)
    {}
    //END: Search fix
    /*************************************************************
    // End WSS Specific Change.
    *************************************************************/
}

function Window_Unload()
{
    if(IsGoodUrl())
    {
        // save persistable data (except when unloading from a "bad url")
    SaveLanguages();
    SaveMembersOptions();
    SaveSections();
    }
}

function ResizeWindow()
{
    if (document.body.clientWidth==0) return;
    var header = document.all.item("header");
    var mainSection = document.all.item("mainSection");
    if (mainSection == null) return;
    
    
    document.body.scroll = "no"
    mainSection.style.overflow= "auto";
    header.style.width= document.body.offsetWidth - 2;
    //mainSection.style.paddingRight = "20px"; // Width issue code
    mainSection.style.width= document.body.offsetWidth - 4;
    mainSection.style.top=0;  
    if (document.body.offsetHeight > header.offsetHeight + 10)
        mainSection.style.height= document.body.offsetHeight - (header.offsetHeight + 10);
    else
        mainSection.style.height=0;
    
    try
    {
        mainSection.setActive();
    }
    catch(e)
    {
    }
}

function Load(key)
{
try {
    userDataCache.load("docSettings");
    var value = userDataCache.getAttribute(key);
    return value;
}
catch(e)
{
}
}

function Save(key, value)
{
try {
    userDataCache.setAttribute(key, value);
    userDataCache.save("docSettings");
}
catch (e)
{
}
}

function RemoveAttribute(key)
{
try {
    userDataCache.removeAttribute(key);
    userDataCache.save("docSettings");
}
catch(e)
{
}
}

function set_to_print()
{
    //breaks out of divs to print
    var i;

    if (window.text)document.all.text.style.height = "auto";
            
    for (i=0; i < document.all.length; i++)
    {
        if (document.all[i].tagName == "body")
        {
            document.all[i].scroll = "yes";
        }
        if (document.all[i].id == "header")
        {
            document.all[i].style.margin = "0px 0px 0px 0px";
            document.all[i].style.width = "100%";
        }
        if (document.all[i].id == "mainSection")
        {
            document.all[i].style.overflow = "visible";
            document.all[i].style.top = "5px";
            document.all[i].style.width = "100%";
            document.all[i].style.padding = "0px 10px 0px 30px";
        }
    }
}

function reset_form()
{
    //returns to the div nonscrolling region after print
     document.location.reload();
}

function IsEvilUrl()
{
    var url = "" + document.location + ".";
    var r = url.indexOf("mk:@MSITStore") != -1;
    return r;
}

function IsGoodUrl()
{
    return !IsEvilUrl();
}

function ReloadEvilUrlAsGoodUrl()
{
    var url = "" + document.location + ".";
    var i = url.indexOf("mk:@MSITStore");
    if(i != -1)
    {
        url = "ms-its:" + url.substring(14, url.length - 1);
        document.location.replace(url);
    }
}
function Set_up_checkboxes()
{
    var checkbox;
    
    checkbox = document.getElementById("vbDeclarationCheckbox");
    if(checkbox != null)
    {
        if(vbDeclaration == "on")
            checkbox.checked = true;
        else
            checkbox.checked = false;
    }
    
    checkbox = document.getElementById("csCheckbox");
    if(checkbox != null)
    {
        if(csLang == "on")
            checkbox.checked = true;
        else
            checkbox.checked = false;
    }
        
    checkbox = document.getElementById("cCheckbox");
    if(checkbox != null)
    {
        if(cLang == "on")
            checkbox.checked = true;
        else
            checkbox.checked = false;
    }
    
    checkbox = document.getElementById("jsharpCheckbox");
    if(checkbox != null)
    {
        if(jsharpLang == "on")
            checkbox.checked = true;
        else
            checkbox.checked = false;
    }
        
    checkbox = document.getElementById("jsCheckbox");
    if(checkbox != null)
    {
        if(jsLang == "on")
            checkbox.checked = true;
        else
            checkbox.checked = false;
    }
    
    checkbox = document.getElementById("inheritedCheckbox");
    if(checkbox != null)
    {
        if(inheritedMembers == "on")
            checkbox.checked = true;
        else
            checkbox.checked = false;
    }
    
    checkbox = document.getElementById("protectedCheckbox");
    if(checkbox != null)
    {
        if(protectedMembers == "on")
            checkbox.checked = true;
        else
            checkbox.checked = false;
    }
    
    checkbox = document.getElementById("netcfCheckbox");
    if(checkbox != null)
    {
        if(netcfMembersOnly == "on")
            checkbox.checked = true;
        else
            checkbox.checked = false;
    }
}

/*  
**********
**********   End
**********
*/


/*  
**********
**********   Begin Language Filtering
**********
*/

function SetLanguage(key)
{
    var i = 0;
    if(vbDeclaration == "on")
        i++;
    if(csLang == "on")
        i++;
    if(cLang == "on")
        i++;
    if(jsharpLang == "on")
        i++;
    if(jsLang == "on")
        i++;
    
    if(key.id == "vbDeclarationCheckbox")
    {
        if(vbDeclaration == "on")
        {
            if(i == 1)
            {
                key.checked = true;
                return;
            }
                
            vbDeclaration = "off";
        }
        else
            vbDeclaration = "on";
    }
    if(key.id == "csCheckbox")
    {
        if(csLang == "on")
        {
            if(i == 1)
            {
                key.checked = true;
                return;
            }
            
            csLang = "off";
        }
        else
            csLang = "on";
    }
    if(key.id == "cCheckbox")
    {
        if(cLang == "on")
        {
            if(i == 1)
            {
                key.checked = true;
                return;
            }
                
            cLang = "off";
        }
        else
            cLang = "on";
    }
    if(key.id == "jsharpCheckbox")
    {
        if(jsharpLang == "on")
        {
            if(i == 1)
            {
                key.checked = true;
                return;
            }
                
            jsharpLang = "off";
        }
        else
            jsharpLang = "on";
    }
    if(key.id == "jsCheckbox")
    {
        if(jsLang == "on")
        {
            if(i == 1)
            {
                key.checked = true;
                return;
            }
                
            jsLang = "off";
        }
        else
            jsLang = "on";
    }
    
    DisplayLanguages();
}

function DisplayLanguages()
{
    var spanElements = document.all.tags("span");
    
    var x = 0;
    if(vbDeclaration == "on")
        x++;
    if(csLang == "on")
        x++;
    if(cLang == "on")
        x++;
    if(jsharpLang == "on")
        x++;
    if(jsLang == "on")
        x++;
    
    var i;
    for(i = 0; i < spanElements.length; ++i)
    {
        if(spanElements[i].codeLanguage != null)
        {
            if(spanElements[i].codeLanguage == "VisualBasicDeclaration")
            {
                if(vbDeclaration == "on")
                    spanElements[i].style.display = "";
                else
                    spanElements[i].style.display = "none";
            }
            if(spanElements[i].codeLanguage == "CSharp")
            {
                if(csLang == "on")
                    spanElements[i].style.display = "";
                else
                    spanElements[i].style.display = "none";
            }
            if(spanElements[i].codeLanguage == "ManagedCPlusPlus")
            {
                if(cLang == "on")
                    spanElements[i].style.display = "";
                else
                    spanElements[i].style.display = "none";
            }
            if(spanElements[i].codeLanguage == "JSharp")
            {
                if(jsharpLang == "on")
                    spanElements[i].style.display = "";
                else
                    spanElements[i].style.display = "none";
            }
            if(spanElements[i].codeLanguage == "JScript")
            {
                if(jsLang == "on")
                    spanElements[i].style.display = "";
                else
                    spanElements[i].style.display = "none";
            }
            
        }
    }
    ChangeLanguageFilterLabel();
}

function ChangeLanguageFilterLabel()
{   
    var i = 0;
    if(vbDeclaration == "on")
        i++;
    if(csLang == "on")
        i++;
    if(cLang == "on")
        i++;
    if(jsharpLang == "on")
        i++;
    if(jsLang == "on")
        i++;
        
    var labelElement;
    
    labelElement = document.getElementById("showAllLabel");
    
    if(labelElement == null)
        return;
        
    labelElement.style.display = "none";
    
    labelElement = document.getElementById("multipleLabel");
    labelElement.style.display = "none";
    
    labelElement = document.getElementById("vbLabel");
    labelElement.style.display = "none";
    
    labelElement = document.getElementById("csLabel");
    labelElement.style.display = "none";
    
    labelElement = document.getElementById("cLabel");
    labelElement.style.display = "none";
    
    labelElement = document.getElementById("jsharpLabel");
    labelElement.style.display = "none";
    
    labelElement = document.getElementById("jsLabel");
    labelElement.style.display = "none";
    
    if(i == 5)
    {
        labelElement = document.getElementById("showAllLabel");
        labelElement.style.display = "inline";
    }
    else if ((i > 1) && (i < 5))
    {
            labelElement = document.getElementById("multipleLabel");
            labelElement.style.display = "inline";
    }
    else if (i == 1)
    {
        if(vbDeclaration == "on")
        {
            labelElement = document.getElementById("vbLabel");
            labelElement.style.display = "inline";
        }
        if(csLang == "on")
        {
            labelElement = document.getElementById("csLabel");
            labelElement.style.display = "inline";
        }
        if(cLang == "on")
        {
            labelElement = document.getElementById("cLabel");
            labelElement.style.display = "inline";
        }
        if(jsharpLang == "on")
        {
            labelElement = document.getElementById("jsharpLabel");
            labelElement.style.display = "inline";
        }
        if(jsLang == "on")
        {
            labelElement = document.getElementById("jsLabel");
            labelElement.style.display = "inline";
        }
    }
}

function LoadLanguages()
{
    var value;
    value = Load("vbDeclaration");
    if(value == null)
        vbDeclaration = "on";
    else
        vbDeclaration = value;
        
    value = Load("csLang");
    if(value == null)
        csLang = "on";
    else
        csLang = value;
        
    value = Load("cLang");
    if(value == null)
        cLang = "on";
    else
        cLang = value;
    
    value = Load("jsharpLang");
    if(value == null)
        jsharpLang = "on";
    else
        jsharpLang = value;
        
    value = Load("jsLang");
    if(value == null)
        jsLang = "on";
    else
        jsLang = value;
}

function SaveLanguages()
{
    Save("vbDeclaration", vbDeclaration);
    Save("csLang", csLang);
    Save("cLang", cLang);
    Save("jsharpLang", jsharpLang);
    Save("jsLang", jsLang);
}

/*  
**********
**********   End Language Filtering
**********
*/


/*  
**********
**********   Begin Members Options Filtering
**********
*/

function SetMembersOptions(key)
{
    if(key.id == "inheritedCheckbox")
    {
        if(key.checked == true)
            inheritedMembers = "on";
        else
            inheritedMembers = "off";
        
        DisplayInheritedMembers();
    }
    if(key.id == "protectedCheckbox")
    {
        if(key.checked == true)
            protectedMembers = "on";
        else
            protectedMembers = "off";
        
        DisplayProtectedMembers();
    }
    if(key.id == "netcfCheckbox")
    {
        if(key.checked == true)
            netcfMembersOnly = "on";
        else
            netcfMembersOnly = "off";
    
        DisplayNETCFMembers();
    }
    
    ChangeMembersOptionsFilterLabel();
}

function DisplayInheritedMembers()
{
    var iMembers = document.all.tags("tr");
    var i;
    
    if(inheritedMembers == "off")
    {   
        for(i = 0; i < iMembers.length; ++i)
        {
            if(iMembers[i].name == "inheritedMember")
                iMembers[i].style.display = "none";
        }
    }
    else
    {
        for(i = 0; i < iMembers.length; ++i)
        {
            if(iMembers[i].name == "inheritedMember")
            {
                if(netcfMembersOnly == "on")
                {
                    if(iMembers[i].notSupportedOn == "netcf")
                    {
                        iMembers[i].style.display = "none";
                    }
                    else
                    {
                        iMembers[i].style.display = "";
                    }
                }
                else
                    iMembers[i].style.display = "";
            }
        }
    }
}

function DisplayProtectedMembers()
{
    var imgElements = document.getElementsByName("toggleSwitch");
    var i;
    
    if(protectedMembers == "off")
    {
        for(i = 0; i < imgElements.length; ++i)
        {
            if(imgElements[i].id.indexOf("Family", 0) == 0)
            {
                if(ItemCollapsed(imgElements[i].id) == false)
                {
                    ExpandCollapse(imgElements[i]);
                }
            }
        }
    }
    else
    {
        for(i = 0; i < imgElements.length; ++i)
        {
            if(imgElements[i].id.indexOf("Family", 0) == 0)
            {
                if(ItemCollapsed(imgElements[i].id) == true)
                {
                    ExpandCollapse(imgElements[i]);
                }
            }
        }
    }
        
}

function DisplayNETCFMembers()
{
    var netcfMembers = document.all.tags("tr");
    var i;
    
    if(netcfMembersOnly == "off")
    {   
        for(i = 0; i < netcfMembers.length; ++i)
        {
            if(netcfMembers[i].notSupportedOn == "netcf")
            {
                netcfMembers[i].style.display = "";
            }
        }
        DisplayInheritedMembers();
    }
    else
    {
        for(i = 0; i < netcfMembers.length; ++i)
        {
            if(netcfMembers[i].notSupportedOn == "netcf")
            {
                netcfMembers[i].style.display = "none";
            }
        }
    }
}

function ChangeMembersOptionsFilterLabel()
{   

    var filtered = false;
    
    if((inheritedMembers=="off") || (protectedMembers=="off") || (netcfMembersOnly=="on"))
        filtered = true;
        
    var labelElement;
    
    labelElement = document.getElementById("showAllMembersLabel");
    
    if(labelElement == null)
        return;
        
    labelElement.style.display = "none";
    
    labelElement = document.getElementById("filteredMembersLabel");
    labelElement.style.display = "none";
    
    if(filtered)
    {
        labelElement = document.getElementById("filteredMembersLabel");
        labelElement.style.display = "inline";
    }
    else
    {
        labelElement = document.getElementById("showAllMembersLabel");
        labelElement.style.display = "inline";
    }
}

function LoadMembersOptions()
{
    var value;
    value = Load("inheritedMembers");
    if(value == null)
        inheritedMembers = "on";
    else
        inheritedMembers = value;
        
    value = Load("protectedMembers");
    if(value == null)
        protectedMembers = "on";
    else
        protectedMembers = value;
        
    value = Load("netcfMembersOnly");
    if(value == null)
        netcfMembersOnly = "off";
    else
        netcfMembersOnly = value;
}

function SaveMembersOptions()
{
    Save("inheritedMembers", inheritedMembers);
    Save("protectedMembers", protectedMembers);
    Save("netcfMembersOnly", netcfMembersOnly);
}

/*  
**********
**********   End Members Options Filtering
**********
*/


/*	
**********
**********   Begin Expand/Collapse
**********
*/

var collapsedItems = new Array();

function ExpandCollapse(imageItem)
{
  try {
	noReentry = true; // Prevent entry to OnLoadImage
    
	if(ItemCollapsed(imageItem.id) == true)
	{
		imageItem.src = collapseImage.src;
		ExpandSection(imageItem);
		RemoveCollapsedItem(imageItem.id);
		
		if(imageItem.id.indexOf("Family", 0) == 0)
		{
			protectedMembers = "on";
			Set_up_checkboxes();
			ChangeMembersOptionsFilterLabel();
		}
	}
	else
	{
		imageItem.src = expandImage.src;
		CollapseSection(imageItem);
		AddCollapsedItem(imageItem.id);
	}
	
	SetCollapseAll();

	noReentry = false;
  }
  catch(e) {}
}

function ExpandCollapseAll(imageItem)
{
	noReentry = true; // Prevent entry to OnLoadImage
    
	var imgElements = document.getElementsByName("toggleSwitch");
	var i;
	
	if(ItemCollapsed(imageItem.id) == true)
	{
		imageItem.src = collapseAllImage.src;
		RemoveCollapsedItem(imageItem.id);
		for(i = 0; i < imgElements.length; ++i)
		{
			imgElements[i].src = collapseImage.src;
			ExpandSection(imgElements[i]);
			RemoveCollapsedItem(imgElements[i].id);
			
			if(imgElements[i].id.indexOf("Family", 0) == 0)
				protectedMembers = "on";
		}
		SetToggleAllLabel(false);
	}
	else
	{
		imageItem.src = expandAllImage.src;
		AddCollapsedItem(imageItem.id);
		for(i = 0; i < imgElements.length; ++i)
		{
			imgElements[i].src = expandImage.src;
			CollapseSection(imgElements[i]);
			AddCollapsedItem(imgElements[i].id);
		}
		SetToggleAllLabel(true);
	}
	
	noReentry = false;
}

function ExpandCollapse_CheckKey(imageItem)
{
	if(window.event.keyCode == 13)
		ExpandCollapse(imageItem);
}

function ExpandCollapseAll_CheckKey(imageItem)
{
	if(window.event.keyCode == 13)
		ExpandCollapseAll(imageItem);
}

function ExpandSection(imageItem)
{
    try {
	imageItem.parentElement.parentElement.nextSibling.style.display	= "";
    }
    catch(e){}
}

function CollapseSection(imageItem)
{
    try {
	imageItem.parentElement.parentElement.nextSibling.style.display	= "none";
    }
    catch(e){}
}

function SetCollapseAll()
{
	var imageElement = document.getElementById("toggleAllImage");
	
	if(imageElement == null)
		return;
		
	var imgElements = document.getElementsByName("toggleSwitch");
	var allCollapsed = true;
	var i;
		
	for(i = 0; i < imgElements.length; ++i)
	{
		allCollapsed = allCollapsed && ItemCollapsed(imgElements[i].id)
	}
	
	if(allCollapsed)
	{
		imageElement.src = expandAllImage.src;
		AddCollapsedItem(imageElement.id);
	}
	else
	{
		imageElement.src = collapseAllImage.src;
		RemoveCollapsedItem(imageElement.id);
	}
	
	SetToggleAllLabel(allCollapsed);
}

function SetToggleAllLabel(allCollapsed)
{
	var labelElement;
	labelElement = document.getElementById("collapseAllLabel");
	
	if(labelElement == null)
		return;
		
	labelElement.style.display = "none";
	
	labelElement = document.getElementById("expandAllLabel");
	labelElement.style.display = "none";
	
	if(allCollapsed)
	{
		labelElement = document.getElementById("expandAllLabel");
		labelElement.style.display = "inline";
	}
	else
	{
		labelElement = document.getElementById("collapseAllLabel");
		labelElement.style.display = "inline";
	}
}

function ItemCollapsed(imageId)
{
	var i;
	
	for(i = 0; i < collapsedItems.length; ++i)
	{
		if(imageId == collapsedItems[i])
			return true;
	}
	
	return false;
}

function AddCollapsedItem(imageId)
{	
    if(ItemCollapsed(imageId) == false)
	collapsedItems[collapsedItems.length] = imageId;
}

function RemoveCollapsedItem(imageId)
{
	var i;
	
	for(i = 0; i < collapsedItems.length; ++i)
	{
		if(imageId == collapsedItems[i])
			collapsedItems.splice(i, 1);
	}
}

function SaveSections()
{
	var i;
	var x = 0;
	
	CleanUserDataStore();
	
	for(i = 0; i < collapsedItems.length; ++i)
	{
		if(ShouldSave(collapsedItems[i]) == true)
		{
			Save("imageValue" + x, collapsedItems[i]);
			x++;
		}
	}
	
	Save("IsFirstPage", false);
}

function LoadSections()
{
	var i = 0;
	var imageId = Load("imageValue" + i);

	while(imageId != null)
	{
		var imageItem = document.getElementById(imageId);
		//alert(imageItem.id);
		if(imageItem != null && collapsedSections[imageId] == "not expanded")
		{
			RemoveCollapsedItem(imageId);
			
			if(imageItem.id.indexOf("Family", 0) == 0)
			{
				if(protectedMembers == "on")
				{
					ExpandCollapse(imageItem);
				}
			}
			else
			{
				ExpandCollapse(imageItem);
			}
		}
	
		i++;
		imageId = Load("imageValue" + i);
	}
	
	SetCollapseAll();

	//Added for bug 4554
	var imgElements = document.getElementsByName("toggleSwitch");
	var i;
		
	for(i = 0; i < imgElements.length; ++i)
	{
		
		if(imgElements[i].id =="sectionToggleRH")
		{
			ExpandCollapse(imgElements[i]);
		}
	}

}

function CleanUserDataStore()
{
	var i = 0;
	var imageId = Load("imageValue" + i);
	
	while(imageId != null)
	{
		RemoveAttribute("imageValue" + i);
		i++;
		imageId = Load("imageValue" + i);
	}
}

function ShouldSave(imageId)
{
	var toggleName;
	
	if(imageId == "toggleAllImage")
		return false;
	
	toggleName = "procedureToggle";
	if(imageId.indexOf(toggleName, 0) == 0)
		return false;
		
	toggleName = "sectionToggle";
	if(imageId.indexOf(toggleName, 0) == 0)
		return false;
	
	return true;
}

function OpenSection(imageItem)
{
	if(ItemCollapsed(imageItem.id) == true)
		ExpandCollapse(imageItem);
}

/*	
**********
**********   End Expand/Collapse
**********
*/


/*  
**********
**********   Begin Copy Code
**********
*/

function CopyCode(key)
{
    var trElements = document.all.tags("tr");
    var i;
    for(i = 0; i < trElements.length; ++i)
    {
        if(key.parentElement.parentElement.parentElement == trElements[i].parentElement)
        {
            window.clipboardData.setData("Text", trElements[i].innerText);
        }
    }
}

function ChangeCopyCodeIcon(key)
{
    var i;
    var imageElements = document.getElementsByName("ccImage")
    for(i=0; i<imageElements.length; ++i)
    {
        if(imageElements[i].parentElement == key)
        {
            if(imageElements[i].src == copyImage.src)
                imageElements[i].src = copyHoverImage.src;
            else
                imageElements[i].src = copyImage.src;
        }
    }
}

function CopyCode_CheckKey(key)
{
    if(window.event.keyCode == 13)
        CopyCode(key);
}

/*  
**********
**********   End Copy Code
**********
*/


/*  
**********
**********   Begin Maintain Scroll Position
**********
*/

function loadAll(){
    try 
    {
        scrollPos = allHistory.getAttribute("Scroll");
    }
    catch(e){}
}

function saveAll(){
    try
    {
        allHistory.setAttribute("Scroll", mainSection.scrollTop);
    }
    catch(e){}
}

/*  
**********
**********   End Maintain Scroll Position
**********
*/


/*  
**********
**********   Begin Send Mail
**********
*/

function formatMailToLink(anchor)
{
    var release = "Release: " + anchor.doc_Release;
    var topicId = "Topic ID: " + anchor.doc_TopicID;
    var topicTitle = "Topic Title: " + anchor.doc_TopicTitle;
    var url = "URL: " + document.URL;
    var browser = "Browser: " + window.navigator.userAgent;

    var crlf = "%0d%0a"; 
    var body = release + crlf + topicId + crlf + topicTitle + crlf + url + crlf + browser + crlf + crlf + "Comments:" + crlf + crlf;
    
    anchor.href = anchor.href + "&body=" + body;
}

/*  
**********
**********   End Send Mail
**********
*/

/*************************************************************
// Begin WSS Specific Change.
*************************************************************/

if (!document.getElementById)
    document.getElementById = function() { return null; }

function initializeMenu(menuId, actuatorId) {
    var menu = document.getElementById(menuId);
    var actuator = document.getElementById(actuatorId);

    if (menu == null || actuator == null) return;

    blockHideMe = false;

    actuator.onclick = function() {
    this.showMenu();
    return false;
    }

    actuator.showMenu = function() {
        menu.style.top =  Calculate_offsetTop(window.event.srcElement) + 18; //window.event.clientY + 10 + "px";
        menu.style.left = this.offsetLeft -17 + "px";
        menu.style.visibility = "visible";
        currentMenu = menu;
    }

    actuator.onkeydown=function() {
        if(event.keyCode==40) {
		this.showMenu();
		event.keyCode = 9;
	}
    }
    menu.onmouseover=function() {
        this.style.visibility = "visible";
    }
   
    menu.onmouseout=function() {
        this.style.visibility = "hidden";
	i = 1;
	try {
		while (i < 100)
		{
			menuObj = document.getElementById(i);
			if (menuObj != null) {
				menuObj.fireEvent("onfocusout");
			}
			i++;
		}
	}
	catch (e) {i = 0;}
    }

    menu.onfocusout=function() {
        var activeObj = document.activeElement;
        if(activeObj.nodeName != "LI") {
            this.style.visibility = "hidden";
        }
    }

    menu.onkeydown=function() {
	blockHideMe = false;

	var currentObj = null;
	currentObj = document.activeElement; 

	///////////
	//shift+tab key
	///////////
	if(event.keyCode==9) {
	    // shift-tab key
	    if(event.shiftKey==true) {
		if (document.URL.indexOf(".chm::")!=-1) {
			//do nothing
		}
		else {
	        if(parseInt(currentObj.id) != "NaN") {	
	            var aId = parseInt(currentObj.id)-1;
   	            if(currentObj.id=="1") {
		       HideMe(parseInt(currentObj.id)+1, false);
 		       currentObj.fireEvent("onfocusout");
		    } 
		else
		{
		infoCount = itemCount(document);
		var collapsedSections = new Array();
		collapsedSections = ulCount(document);

		for (var a = 2; a < collapsedSections.length; a++)
		{
		    if(currentObj.id==collapsedSections[a]-1) {
		       HideMe(parseInt(currentObj.id)+1, false);
    		       currentObj.fireEvent("onfocusout");
		       aId=collapsedSections[a-1]; 
		    }
		}
		}//else
		} 
            } 
	}

	///////////
	//tab key
	///////////
	else {
	   if (document.URL.indexOf(".chm::")!=-1) {
	     currentObj = document.getElementById(currentObj.id);
	     currentObj.fireEvent("onfocusout");
	     HideMe("InfoMenu", false);	
	     currentObj = document.getElementById("collapseAllLabel");
	     currentObj.focus();
	     return;
	   } else {
		if(parseInt(currentObj.id) != "NaN") {
		infoCount = itemCount(document);
		var collapsedSections = new Array();
		collapsedSections = ulCount(document);
		//Rule out first section
		for (var a = 2; a < collapsedSections.length; a++)
		{
		    if(currentObj.id==collapsedSections[a]-2) {
			if (collapsedSections[a-1] != 1) {
				var prevOne = document.getElementById(collapsedSections[a-1]);
		        	prevOne.fireEvent("onfocusout");
			}//if
		    }//if
		}//for
	            var aId = parseInt(currentObj.id)+1;
	            var nextOne = document.getElementById(aId.toString());

	            if(nextOne !=null) {
		       if(nextOne.style.visibility=="hidden") {
		           nextOne.style.visibility = "visible";
		       }
		       if(nextOne.nodeName=="UL") {
			   nextOne.setActive();
	            	   aId = parseInt(nextOne.id)+1;
	            	   nextOne = document.getElementById(aId.toString());
	            	   if(nextOne !=null) {
		             if(nextOne.style.visibility=="hidden") {
		                nextOne.style.visibility = "visible";
		             }
		           }
		        }

                  	nextOne.fireEvent("onmouseover"); 
	            }
	        }
            }
	}
	} //if
	///////////
	// down arrow key
	///////////
	else if(event.keyCode==40) {
            var aId = parseInt(currentObj.id)+1;
	    if(parseInt(currentObj.id) != "NaN") {
		infoCount = itemCount(document);
		var collapsedSections = new Array();
		collapsedSections = ulCount(document);
		//Rule out first section
 	        if(currentObj.id=="1") {
		    HideMe(parseInt(currentObj.id)+1, false);
		    aId=collapsedSections[3]-1; 
		    currentObj.fireEvent("onfocusout");
		}
		else if(currentObj.id==collapsedSections[collapsedSections.length-1]-1||currentObj.id==infoCount) {
		    currentObj = document.getElementById(currentObj.id);
	    	    currentObj.fireEvent("onfocusout");
	    	    HideMe("InfoMenu", false);	
	    	    currentObj = document.getElementById("collapseAllLabel");
	    	    currentObj.focus();
	    	    return; 
		}
		else {
		for (var a = 2; a < collapsedSections.length; a++)
		{

    	            if(currentObj.id==collapsedSections[a]-2) {
		    	currentObj.setActive();
		    	currentObj = document.getElementById(collapsedSections[a-1]);
		    	currentObj.fireEvent("onfocusout");
	       	    } else if(currentObj.id==collapsedSections[a]-1) {
		    	HideMe(collapsedSections[a]+1, false);
		    	aId=collapsedSections[a+1]-1; 
		    	currentObj.fireEvent("onfocusout");
		    }
		}
		}
	        var nextOne = document.getElementById(aId.toString());
                if(nextOne !=null) {
		   if(nextOne.style.visibility=="hidden") {
		       nextOne.style.visibility = "visible";
		   }
		   if(nextOne.nodeName=="UL") {
		       nextOne.setActive();
	               aId = parseInt(nextOne.id)+1;
	               nextOne = document.getElementById(aId.toString());
	               if(nextOne !=null) {
		           if(nextOne.style.visibility=="hidden") {
		               nextOne.style.visibility = "visible";
		           }
		        }
		    }
		    nextOne.setActive();
               	    nextOne.fireEvent("onmouseover"); 
	        }

            }
	}

	///////////
	// right arrow key
	///////////
        else if(event.keyCode==39) {
	    if(parseInt(currentObj.id) != "NaN") {
	        var aId = parseInt(currentObj.id);
		infoCount = itemCount(document);
		var collapsedSections = new Array();
		collapsedSections = ulCount(document);
		var foundOne = false;
		for (var a = 2; a < collapsedSections.length; a++)
		{
	  	 	if (aId == 1 || aId == collapsedSections[a]-1) {
				foundOne=true;
			}
		}
		if (!foundOne) {
			return;
		}
		aId++;
	        var nextOne = document.getElementById(aId.toString());
	        if(nextOne !=null) {
		   if(nextOne.style.visibility=="hidden") {
		       nextOne.style.visibility = "visible";
		   }
		   if(nextOne.nodeName=="UL") {
		       nextOne.setActive();
	               aId = parseInt(nextOne.id)+1;
	               nextOne = document.getElementById(aId.toString());
	               if(nextOne !=null) {
		         if(nextOne.style.visibility=="hidden") {
		            nextOne.style.visibility = "visible";
		         }
		       }
		    }
		    nextOne.setActive();
               	    nextOne.fireEvent("onmouseover"); 
	        }
	    }
	} 
	///////////
        // left arrow key
	///////////
        else if(event.keyCode==37) {
	    if(parseInt(currentObj.id) != "NaN") {
	        var cId = parseInt(currentObj.id);
	        var aId = parseInt(currentObj.id)-1;
		infoCount = itemCount(document);
		var collapsedSections = new Array();
		collapsedSections = ulCount(document);
		for (var a = 1; a < collapsedSections.length; a++)
		{
			if (a != collapsedSections.length-1)
			{
		 	  if (cId >= collapsedSections[a] && cId <= collapsedSections[a+1]) {
				HideMe(collapsedSections[a], false);
    				currentObj.fireEvent("onfocusout");
				aId=collapsedSections[a]-1;	
			  }
			}
			else
			{
			  if (cId >= collapsedSections[a] && cId <= infoCount) {
				HideMe(collapsedSections[a], false);
    				currentObj.fireEvent("onfocusout");
				aId=collapsedSections[a]-1;	
			  }
			}
		}
                var prevOne = document.getElementById(aId.toString());
    	        if(prevOne !=null) {
		   if(prevOne.style.visibility=="hidden") {
		       prevOne.style.visibility = "visible";
		   }
		   prevOne.setActive();
                   prevOne.fireEvent("onmouseover");
		} 
	    }
	}
	///////////
	// up arrow key
	///////////
	else if(event.keyCode==38) {
	        if(parseInt(currentObj.id) != "NaN") {	        
	            var aId = parseInt(currentObj.id)-1;

		infoCount = itemCount(document);
		var collapsedSections = new Array();
		collapsedSections = ulCount(document);
		//Rule out first section
   	        if(currentObj.id=="1") {
		    currentObj = document.getElementById(collapsedSections[1]);
		    currentObj.fireEvent("onfocusout");
		    HideMe("InfoMenu", false);	
		    currentObj = document.getElementById("collapseAllLabel");
		    currentObj.focus();
		    return;
		}
		else {
		for (var a = 2; a < collapsedSections.length; a++)
		{
    	            if(currentObj.id==collapsedSections[a]-1) {
		       HideMe(parseInt(currentObj.id)+1, false);
 		       currentObj.fireEvent("onfocusout");
		       aId=collapsedSections[a-1]-1;
		    }
		}
		}
		    if (aId==0) {
			aId=1;
		    }
	            var prevOne = document.getElementById(aId.toString());
		    if(prevOne.nodeName=="UL") {
			prevOne.fireEvent("onfocusout");
			aId--;
	            	prevOne = document.getElementById(aId.toString());
	            	if(prevOne !=null) {
		             if(prevOne.style.visibility=="hidden") {
		                prevOne.style.visibility = "visible";
		             }
		        }
                     }
		     prevOne.setActive();
                     prevOne.fireEvent("onmouseover");	    
		} 
	    } 
	///////////
	// return key
	///////////
	else if(event.keyCode==13) {
            if(parseInt(currentObj.id) != "NaN") {
		infoCount = itemCount(document);
		var collapsedSections = new Array();
		collapsedSections = ulCount(document);
	        var aId = parseInt(currentObj.id);
		for (var a = 2; a < collapsedSections.length; a++)
		{
	  	 	if (aId >= collapsedSections[a-1] && aId <= collapsedSections[a]) {
				var linkObj = currentObj.children.item(0);
				location.href = linkObj;
		    		linkObj.fireEvent("onclick");
			}
		}
	    }
        }

	}

    navRoot = document.getElementById("InfoMenu");
    for (i=0; i<navRoot.childNodes.length; i++) {
        var node = navRoot.childNodes[i];
        if (node.nodeName=="LI") {
            node.onclick=function() {
                this.setActive();
            }
            node.onfocus=function() {
		if(this.style.visibility == "hidden") {
			this.style.visibility = "visible";
		}

	        if(parseInt(this.id)>-1) {
	            var aId = parseInt(this.id);
		infoCount = itemCount(document);
		var collapsedSections = new Array();
		collapsedSections = ulCount(document);
		for (var a = 1; a < collapsedSections.length; a++)
		{
	  	 	if (aId == collapsedSections[a]) {
		        	aId++;
	                	menuObj = document.getElementById(aId.toString())
		        	if(menuObj.style.visibility == "hidden") {
	    	            		menuObj.style.visibility = "visible";
					return;
		        	}//if
			}//if
		}//for
		}
                this.className+=" over";
            }

            node.onfocusout=function() {
                this.className=this.className.replace(" over", "");
            }

            node.onmouseover=function() {
                this.className+=" over";
	        if(parseInt(this.id)>-1) {
	            var aId = parseInt(this.id);
		    if(aId=="1") {
		        aId++;
	                menuObj = document.getElementById(aId.toString());
		        if(menuObj.style.visibility == "hidden") {
	    	            menuObj.style.visibility = "visible";
		        }
		    }
		else {
		infoCount = itemCount(document);
		var collapsedSections = new Array();
		collapsedSections = ulCount(document);
		for (var a = 2; a < collapsedSections.length; a++)
		{
	  	 	if (aId == collapsedSections[a]-1) {
				aId++;
	                	menuObj = document.getElementById(aId.toString());
		        	if(menuObj.style.visibility == "hidden") {
	    	           		menuObj.style.visibility = "visible";
				}//if
		        }//if

		}//for
		}
		}
            }
  
  
            node.onmouseout=function() {
                this.className=this.className.replace(" over", "");
            }
        }
    }
}

function itemCount(document)
{

	i = 1;
	j = 0;
	try {
		while (i < 100)
		{
			menuObj = document.getElementById(i);
			if (menuObj != null) {
				//j is now the total of the menu
				j++;
			}
			i++;
		}
	}
	catch (e) {i = 0;}
	return j;
}

function ulCount(document)
{
	i = 1;
	x = 2;
	j = 0
	var collapsedSections = new Array();
	//set the top collapsed section to #1
	collapsedSections[1] = 1;
	try {
		while (i < 100)
		{
			menuObj = document.getElementById(i);
			if (menuObj != null) {
				j++;
				if (menuObj.nodeName=="UL")
				{
					collapsedSections[x] = j;
					x++;
				}
			}
			i++;
		}
	}
	catch (e) {i = 0;}
	return collapsedSections;
}

function Calculate_offsetTop(e)
{
    if(e.tagName != 'DIV')
        return e.offsetTop + Calculate_offsetTop(e.offsetParent);
    else
        return e.offsetTop;
}
function HideMe(elemId, firstElem)
{
	var thisObj = document.getElementById(elemId);

	if(firstElem==true) {
		if(event.shiftKey==true) {
			thisObj.style.visibility = "hidden";
		}
	}
	else {
	    if(!blockHideMe) {
		thisObj.style.visibility = "hidden";
	    }
	}
        blockHideMe=false;
}

function Hide_LangFilter()
{
    var divElements = document.all.tags("div");
    for(i = 0; i < divElements.length; ++i)
    {
        if(divElements[i].id == "syntaxSection")
        {
            return;
        }
    }
    var spanElements = document.all.tags("span");
    var i;
    for(i = 0; i < spanElements.length; ++i)
    {
        if(spanElements[i].id == "languageFilterToolTip")
        {
            spanElements[i].style.display = "none";
            return;
        }
    }
}

// ****************************************************************************
// *                      Reftips (parameter popups)                          *
// ****************************************************************************
function initReftips(){
	var DLs = document.all.tags("DL");
	var PREs = document.all.tags("SPAN");
	if (DLs && PREs) {
		var iDL = 0;
		var iPRE = 0;
		var iSyntax = -1;
		for (var iPRE = 0; iPRE < PREs.length; iPRE++) {
			if (PREs[iPRE].className == "parameter") {
				while (iDL < DLs.length && DLs[iDL].sourceIndex < PREs[iPRE].sourceIndex)
					iDL++;			
				if (iDL < DLs.length) {
					initSyntax(PREs[iPRE], DLs[iDL]);
					iSyntax = iPRE;
				}
				else
					break;
			}
		}

		if (iSyntax >= 0) {
			var last = PREs[iSyntax];
			if (last.parentElement.tagName == "DIV") last = last.parentElement;						last.insertAdjacentHTML('afterEnd','<DIV ID=reftip CLASS=reftip STYLE="position:absolute;visibility:hidden;overflow:visible;"></DIV>');
		}
	}
}

function initSyntax(pre, dl){
	var strSyn = pre.outerHTML;
	var ichStart = strSyn.indexOf('>', 0) + 1;
	var terms = dl.children.tags("DT");
	if (terms) {
		for (var iTerm = 0; iTerm < terms.length; iTerm++) {
			if (terms[iTerm].innerHTML.indexOf("<!--join-->")!=-1){
				var word = terms[iTerm].innerText.replace(/\s$/, "");
				var ichMatch = findTerm(strSyn, ichStart, word);
				if (ichMatch < 1){
					word = word.replace(/\s/, "&nbsp;")
					ichMatch = findTerm(strSyn, ichStart, word);
				}
				while (ichMatch > 0) {
					var strTag = '<A HREF="" ONCLICK="showTip1(this)" CLASS="synParam">' + word + '</A>';

					strSyn =
						strSyn.slice(0, ichMatch) +
						strTag +
						strSyn.slice(ichMatch + word.length);
					ichMatch = findTerm(strSyn, ichMatch + strTag.length, word);
				}
				
			}
		}
		for (var iTerm = 0; iTerm < terms.length; iTerm++) {
			if (terms[iTerm].innerHTML.indexOf("<!--join-->")==-1){
			var words = terms[iTerm].innerText.replace(/\[.+\]/g, " ").replace(/,/g, " ").split(" ");
				var htm = terms[iTerm].innerHTML;
				for (var iWord = 0; iWord < words.length; iWord++) {
					var word = words[iWord];

					if (word.length > 0 && htm.indexOf(word, 0) < 0)
						word = word.replace(/:.+/, "");

					if (word.length > 0) {
						var ichMatch = findTerm(strSyn, ichStart, word);
						while (ichMatch > 0) {
							if (!isLinkText(strSyn.substring(ichMatch))){
								var strTag = '<A HREF="" ONCLICK="showTip1(this)" CLASS="synParam">' + word + '</A>';
								
								strSyn =
									strSyn.slice(0, ichMatch) +
									strTag +
									strSyn.slice(ichMatch + word.length);

								ichMatch = findTerm(strSyn, ichMatch + strTag.length, word);
							}
							else ichMatch = findTerm(strSyn, ichMatch + word.length, word);
						}
					}
				}
			}
		}
	}

	// Replace the syntax block with our modified version.
	pre.outerHTML = strSyn;
}

function findTerm(strSyn, ichPos, strTerm)
{
	var ichMatch = strSyn.indexOf(strTerm, ichPos);
	while (ichMatch >= 0) {
		var prev = (ichMatch == 0) ? '\0' : strSyn.charAt(ichMatch - 1);
		var next = strSyn.charAt(ichMatch + strTerm.length);
		if (!isalnum(prev) && !isalnum(next) && !isInTag(strSyn, ichMatch)) {
			var ichComment = strSyn.indexOf("/*", ichPos);
			while (ichComment >= 0) {
				if (ichComment > ichMatch) { 
					ichComment = -1;
					break; 
				}
				var ichEnd = strSyn.indexOf("*/", ichComment);
				if (ichEnd < 0 || ichEnd > ichMatch)
					break;
				ichComment = strSyn.indexOf("/*", ichEnd);
			}
			if (ichComment < 0) {
				ichComment = strSyn.indexOf("//", ichPos);
				var newPos = 0;
				if (ichComment >= 0) {
					while (isInTag(strSyn, ichComment)) { //checks to see if the comment is in a tag (and thus part of a URL)
						newPos = ichComment + 1;
						ichComment = strSyn.indexOf("//", newPos);
						if (ichComment < 0) 
							break;
					}
					while (ichComment >= 0) {
						if (ichComment > ichMatch) {
							ichComment = -1;
							break; 
						}
						var ichEnd = strSyn.indexOf("\n", ichComment);
						if (ichEnd < 0 || ichEnd > ichMatch)
							break;
						ichComment = strSyn.indexOf("//", ichEnd);
					}
				}
			}
			if (ichComment < 0)
				break;
		}
		ichMatch = strSyn.indexOf(strTerm, ichMatch + strTerm.length);
	}
	return ichMatch;
}
function isLinkText(strHtml){
	return strHtml.indexOf("<")==strHtml.toLowerCase().indexOf("<\/a>");
}

function isInTag(strHtml, ichPos)
{
	return strHtml.lastIndexOf('<', ichPos) >
		strHtml.lastIndexOf('>', ichPos);
}

function isalnum(ch){
	return ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9') || (ch == '_') || (ch == '-'));
}

function showTip1(link){
	bodyOnClick();
	var tip = document.all.reftip;
	if (!tip || !link)
		return;

	window.event.returnValue = false;
	window.event.cancelBubble = true;

	// Hide the tip if necessary and initialize its size.
	tip.style.visibility = "hidden";
	tip.style.pixelWidth = 260;
	tip.style.pixelHeight = 24;

	// Find the link target.
	var term = null;
	var def = null;
	var DLs = document.all.tags("DL");

	for (var iDL = 0; iDL < DLs.length; iDL++) {
		if (DLs[iDL].sourceIndex > link.sourceIndex) {
			var dl = DLs[iDL];
			var iMax = dl.children.length - 1;
			for (var iElem = 0; iElem < iMax; iElem++) {
				var dt = dl.children[iElem];
				if (dt.tagName == "DT" && dt.style.display != "none") {
					if (findTerm(dt.innerText, 0, link.innerText) >= 0) {
						var dd = dl.children[iElem + 1];
						if (dd.tagName == "DD") {
							term = dt;
							def = dd;
						}
						break;
					}
				}
			}
			break;
		}
	}

	if (def) {
		window.linkElement = link;
		window.linkTarget = term;
		tip.innerHTML = '<DL><DT>' + term.innerHTML + '</DT><DD>' + def.innerHTML + '</DD></DL>';
		window.setTimeout("moveTip()", 0);
	}
}

function moveTip(){
	var tip = document.all.reftip;
	var link = window.linkElement;
	if (!tip || !link)
		return; //error

        var oBody = window.document.all["mainSection"];

        iOffsetY = oBody.scrollTop + oBody.offsetHeight/2;
	iOffsetX = 50;

	tip.style.pixelLeft = iOffsetX;
	tip.style.pixelTop = iOffsetY;
	tip.style.visibility = "visible";
}

function hideTip1(){
	if (window.linkElement) {
		window.linkElement.style.background = "";
		window.linkElement = null;
	}

	var tip = document.all.reftip;
	if (tip) {
		tip.style.visibility = "hidden";
		tip.innerHTML = "";
	}
}

function beginsWith(s1, s2){
	// Does s1 begin with s2?
	return s1.substring(0, s2.length) == s2;
}


function bodyOnClick(){
	var elem = window.event.srcElement;
	for (; elem; elem = elem.parentElement) {
		if (elem.id == "reftip" || elem.id == "PopUp" || elem.id == "In_Popup" )
		{
		elem.style.visibility = "hidden";
		if (elem.id == "reftip")
			return;
		}
	}		
	hideTip1();
}

/*************************************************************
// End WSS Specific Change.
*************************************************************/
