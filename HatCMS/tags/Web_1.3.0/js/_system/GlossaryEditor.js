

var GlossaryDataItemHolder = function()
{
	this.id = -1;
	this.isAcronym = false;
	this.text = "";
	this.word = "";
}

function GlossaryEditor(renderHtmlToElId, renderJSONToElId, GlossaryFormData)
{
    this.RenderToElId = renderHtmlToElId;
    this.RenderJSONToElId = renderJSONToElId;
    this.glossaryFormData = GlossaryFormData; // {};
    this.NewElementId = -100001;
    
    this.updateWord = function(dataId, wordVal)
    {
        this.glossaryFormData[dataId].word = wordVal; 
        this.updateJSON();
    };
    
    this.updateText = function(dataId, textVal)
    {
        this.glossaryFormData[dataId].text = textVal; 
        this.updateJSON();
    };
    
    this.updateAcronym = function(dataId, checked)
    {
        this.glossaryFormData['"+formElement.id+"'].isAcronym = this.checked;
        this.updateJSON();
    };
    
    this.removeData = function(dataId)
    {
       delete this.glossaryFormData[dataId]; 
       this.updateDisplay(); 
    };
    
    this.updateDisplay = function()
    {
	    var html = "<table border=\"1\"><tr><td><strong>Word</strong></td><td><strong>Acronym?</strong></td><td><strong>Definition</strong></td></tr>";
	    for(key in this.glossaryFormData)
	    {
		    var formElement  = this.glossaryFormData[key];

		    html += "<tr><td><input type=\"text\" size=\"30\" value=\""+formElement.word+"\" onkeyup=\"updateWord('"+this.RenderToElId+"', '"+formElement.id+"', this.value);\"></td><td>";
		    html += "<input type=\"checkbox\" ";
		    if (formElement.isAcronym)
			    html += "CHECKED";

		    // html += " onchange=\"this.glossaryFormData['"+formElement.id+"'].isAcronym = this.checked; this.updateJSON();\">";
		    html += " onchange=\"updateAcronym('"+this.RenderToElId+"', '"+formElement.id+"', this.checked);\">";
    		
		    html += "</td><td><textarea cols=\"50\" rows=\"3\" onkeyup=\"updateText('"+this.RenderToElId+"', '"+formElement.id+"', this.value);\">"+formElement.text+"</textarea></td>";
    		
		    var a_name = "anchor_"+formElement.id+this.RenderToElId;
		    html += "<td><a name=\""+a_name+"\"></a> <a href=\"#"+a_name+"\" onclick=\"removeData('"+this.RenderToElId+"', '"+formElement.id+"'); return false; \">[remove]</a></td></tr>";
	    }

	    html += "</table>";

	    document.getElementById(this.RenderToElId).innerHTML = html;

	    this.updateJSON();
    }; // updateDisplay()
    
    this.updateJSON = function()
    {
	    // -- update JSON form submission element
	    document.getElementById(this.RenderJSONToElId).value = JSON.stringify(this.glossaryFormData);
    };
    
    this.AddFormElement = function()
    {
	    this.NewElementId ++;
	    var newEl = new GlossaryDataItemHolder();
	    newEl.word = "";
	    newEl.id = this.NewElementId;
	    newEl.isAcronym = true;
	    newEl.text = "";

	    // -- add to glossaryFormData array
	    this.glossaryFormData[""+newEl.id] = newEl;
    	
	    this.updateDisplay();

    };
    
}
var GlossaryEditorInstances = [];

function getGlossaryEditorInstance(instanceId)
{
    for(var i=0; i < GlossaryEditorInstances.length; i++)
    {
        if (GlossaryEditorInstances[i].RenderToElId == instanceId)
            return GlossaryEditorInstances[i];
    }
    alert('Script Error: could not get Glossary Editor Instance');    
}

function updateWord(instanceId, dataId, wordVal)
{
    getGlossaryEditorInstance(instanceId).updateWord(dataId, wordVal);
}

function updateText(instanceId, dataId, wordVal)
{
    getGlossaryEditorInstance(instanceId).updateText(dataId, wordVal);
}

function updateAcronym(instanceId, dataId, checked)
{
    getGlossaryEditorInstance(instanceId).updateAcronym(dataId, checked);
}

function removeData(instanceId, dataId)
{
    getGlossaryEditorInstance(instanceId).removeData(dataId);
}

function AddGlossaryElement(instanceId)
{
    getGlossaryEditorInstance(instanceId).AddFormElement();
}


/*
function glossaryPageLoad()
{
	glossaryFormData = JSON.parse(GlossaryDataJSONString);
	updateDisplay(glossaryFormData);
}
*/