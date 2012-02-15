/*
hatCms plugin modifications for CKEditor
*/

var cleanHTMLCmd =
{
	canUndo : true,    
	exec : function( editor )
	{
		editor.fire( 'saveSnapshot' );
		var html = editor.getData();
		html = cleanHTML(html);
		editor.setData(html);
		editor.fire( 'saveSnapshot' );
	}
};

var commands =
{
	insertNonBreakingSpace :
	{
		modes : { wysiwyg : 1, source : 1 },
		canUndo : true, 

		exec : function( editor )
		{
			// -- .insertHtml('&nbsp;') doesn't do anything - we need to have the surrounding spans!
			editor.insertHtml('<span>&nbsp;</span>');	
		}
	}
};



CKEDITOR.plugins.add( 'hatCms',
{   	
	requires : [ 'keystrokes' ],
	init : function( editor )
    {			
		editor.addCommand( 'insertNonBreakingSpace', commands.insertNonBreakingSpace );

		// Register the shift+space nbsp insertion keystrokes.
		var keystrokes = editor.keystrokeHandler.keystrokes;

		keystrokes[ CKEDITOR.SHIFT + 32 /* space */ ] = 'insertNonBreakingSpace';
		

		var pluginName = 'CleanHTML';
		editor.addCommand( pluginName, cleanHTMLCmd );
		editor.ui.addButton( pluginName,
				{
					label : 'Clean-up HTML',
					command : pluginName,
					icon: this.path+'cleantags.gif'
				});    
    } // init

} );


    function getQuerystringValue (winLocation, key, defaultReturnValue)
    {
      // source: http://www.bloggingdeveloper.com/post/JavaScript-QueryString-ParseGet-QueryString-with-Client-Side-JavaScript.aspx
      if (defaultReturnValue==null) defaultReturnValue="";
      key = key.replace(/[\[]/,"\\\[").replace(/[\]]/,"\\\]");
      var regex = new RegExp("[\\?&]"+key+"=([^&#]*)");
      var qs = regex.exec(winLocation);
      if(qs == null)
        return defaultReturnValue;
      else
        return qs[1];
    }

// -- add specialized tabs to existing dialogs
// ref: http://cksource.com/forums/viewtopic.php?f=11&t=16587&start=0
CKEDITOR.on( 'dialogDefinition', function( ev )
   {
	  // Take the dialog name and its definition from the event
	  // data.
	  var dialogName = ev.data.name;
	  var dialogDefinition = ev.data.definition;
	  
	  

	  // Check if the definition is from the dialog we're
	  // interested on (the 'Image' dialog).
	  if ( dialogName == 'image' )
	  {
		 // -- override the onOk event so that resized images are handled by the server		 
		  dialogDefinition.onOk = CKEDITOR.tools.override( dialogDefinition.onOk, function( origOnOk )
		  {
		      return function( data )
			  {
				  var imgUrl = this.getValueOf('info', 'txtUrl');
				  var imgWidth = this.getValueOf('info', 'txtWidth');
				  var imgHeight = this.getValueOf('info', 'txtHeight');
				  var appPath = this.getValueOf('advanced','appPath');

				  var thumbUrl = appPath+'_system/tools/showThumb.aspx?file='+encodeURIComponent(imgUrl)+"&w="+imgWidth+"&h="+imgHeight; 

				  // alert('Override image.onOk - '+imgWidth+" x "+imgHeight);
				  
				  this.setValueOf('info', 'txtUrl', thumbUrl);
				  origOnOk.call( this, data );
			  };
		  });

		 // -- override the onShow event so that resized images are handled by the server		 
		  dialogDefinition.onShow = CKEDITOR.tools.override( dialogDefinition.onShow, function( origOnShow )
		  {
		      return function( data )
			  {
				  origOnShow.call( this, data );
				  
				  var thumbUrl = this.getValueOf('info', 'txtUrl');
				  if(thumbUrl.indexOf('umb.aspx') >= 0)
				  {
				     // the url is resized on the server. Change the UI values to hide that fact.
					 var imgPath = getQuerystringValue(thumbUrl, "file", "");
					 var imgWidth = getQuerystringValue(thumbUrl, "w", "");
					 var imgHeight = getQuerystringValue(thumbUrl, "h", "");
					 if (imgPath != "" && imgWidth != "" && imgHeight != "")
					 {
						imgPath = decodeURIComponent(imgPath);
						this.setValueOf('info', 'txtUrl', imgPath);
						this.setValueOf('info', 'txtWidth', imgWidth);
						this.setValueOf('info', 'txtHeight', imgHeight);	
						
						var aPath =imgPath.substr(0, imgPath.indexOf('showThumb.aspx'));
						this.setValueOf('advanced', 'appPath', aPath);
					 }
				  }				  				  
			  };
		  });


		 // -- add new tab to the dialog
		 var insertBeforeId = 'info';
		 
		 dialogDefinition.minWidth = 700;
		 dialogDefinition.minHeight = 410;
		 dialogDefinition.addContents({
			id: 'Browse',
			label: 'Browse',
			accessKey: 'B',
			padding: 0,
			elements: 
			[
				{
					type : 'html',
					html: '<iframe onload="var iframe = CKEDITOR.document.getById( this.id ), parentContainer = iframe.getParent(); parentContainer.setStyles( { width : \'700px\', height : \'400px\'} );"  style="border: 1px solid black; width: 700px; height: 400px;" src="'+CKEDITOR.basePath+'../../../_system/tools/FCKHelpers/InlineImageBrowser2.aspx?ck=1">'
				}
			]
		},insertBeforeId);

			// Get a reference to the "Link Info" tab.
			var advTab = dialogDefinition.getContents( 'advanced' );

			// Add a text field to the "info" tab.
			advTab.add( {
					type : 'text',
					label : 'hatCMS Application Path (do not change)',
					id : 'appPath',
					'default' : '/'
				});

		

	   } // if image dialog
	   else if ( dialogName == 'link')
	   {
			var insertBeforeId = 'info';
			dialogDefinition.minWidth = 700;
			dialogDefinition.minHeight = 410;
			dialogDefinition.addContents({
				id: 'Browse',
				label: 'Link to a Page',
				accessKey: 'B',
				padding: 0,
				elements: 
				[
					{
						type : 'html',
						html: '<iframe onload="var iframe = CKEDITOR.document.getById( this.id ), parentContainer = iframe.getParent(); parentContainer.setStyles( { width : \'700px\', height : \'400px\'} );" width="700" height="400" style="border: 1px solid black; width: 700px; height: 400px; zIndex: 1000;" src="'+CKEDITOR.basePath+'../../../_system/tools/ckhelpers/InlinePageBrowser.aspx?ck=1">',					
						onShow: function()
							{
								setTimeout(function() { // ie shows an error because the element isn't ready when changing focus
									CKEDITOR.dialog.getCurrent().selectPage('Browse');
								},100);
							}
					}
				]
			},insertBeforeId); // addContents

			dialogDefinition.addContents({
				id: 'File',
				label: 'Link to a File',
				accessKey: 'F',
				padding: 0,
				elements: 
				[
					{
						type : 'html',
						html: '<iframe onload="var iframe = CKEDITOR.document.getById( this.id ), parentContainer = iframe.getParent(); parentContainer.setStyles( { width : \'700px\', height : \'400px\'} );"  style="border: 1px solid black; width: 700px; height: 400px;" src="'+CKEDITOR.basePath+'../../../_system/tools/ckhelpers/InlineFileBrowser.aspx?ck=1">'					
					}
				]
			},insertBeforeId); // addContents
	   } // else
   }); 

// -- another cleaner: http://booden.net/ContentCleaner.aspx
/*****************************************************************
 * Word HTML Cleaner
 * Copyright (C) 2005 Connor McKay
 * http://ethilien.net/websoft/wordcleaner/cleaner.htm
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details.
*****************************************************************/

//remplacement characters
var rchars = [["–","—", "‘", "’", "“", "”", ' '], ["-", "-", "'", "'", '"', '"', ' ']];

//html entities translation array
var hents = new Array();
hents['¡'] = '&iexcl;';
hents['¢'] = '&cent;';
hents['£'] = '&pound;';
hents['¤'] = '&curren;';
hents['¥'] = '&yen;';
hents['¦'] = '&brvbar;';
hents['§'] = '&sect;';
hents['¨'] = '&uml;';
hents['©'] = '&copy;';
hents['ª'] = '&ordf;';
hents['«'] = '&laquo;';
hents['¬'] = '&not;';
hents['­'] = '&shy;';
hents['®'] = '&reg;';
hents['¯'] = '&macr;';
hents['°'] = '&deg;';
hents['±'] = '&plusmn;';
hents['²'] = '&sup2;';
hents['³'] = '&sup3;';
hents['´'] = '&acute;';
hents['µ'] = '&micro;';
hents['¶'] = '&para;';
hents['·'] = '&middot;';
hents['¸'] = '&cedil;';
hents['¹'] = '&sup1;';
hents['º'] = '&ordm;';
hents['»'] = '&raquo;';
hents['¼'] = '&frac14;';
hents['½'] = '&frac12;';
hents['¾'] = '&frac34;';
hents['¿'] = '&iquest;';
hents['À'] = '&Agrave;';
hents['Á'] = '&Aacute;';
hents['Â'] = '&Acirc;';
hents['Ã'] = '&Atilde;';
hents['Ä'] = '&Auml;';
hents['Å'] = '&Aring;';
hents['Æ'] = '&AElig;';
hents['Ç'] = '&Ccedil;';
hents['È'] = '&Egrave;';
hents['É'] = '&Eacute;';
hents['Ê'] = '&Ecirc;';
hents['Ë'] = '&Euml;';
hents['Ì'] = '&Igrave;';
hents['Í'] = '&Iacute;';
hents['Î'] = '&Icirc;';
hents['Ï'] = '&Iuml;';
hents['Ð'] = '&ETH;';
hents['Ñ'] = '&Ntilde;';
hents['Ò'] = '&Ograve;';
hents['Ó'] = '&Oacute;';
hents['Ô'] = '&Ocirc;';
hents['Õ'] = '&Otilde;';
hents['Ö'] = '&Ouml;';
hents['×'] = '&times;';
hents['Ø'] = '&Oslash;';
hents['Ù'] = '&Ugrave;';
hents['Ú'] = '&Uacute;';
hents['Û'] = '&Ucirc;';
hents['Ü'] = '&Uuml;';
hents['Ý'] = '&Yacute;';
hents['Þ'] = '&THORN;';
hents['ß'] = '&szlig;';
hents['à'] = '&agrave;';
hents['á'] = '&aacute;';
hents['â'] = '&acirc;';
hents['ã'] = '&atilde;';
hents['ä'] = '&auml;';
hents['å'] = '&aring;';
hents['æ'] = '&aelig;';
hents['ç'] = '&ccedil;';
hents['è'] = '&egrave;';
hents['é'] = '&eacute;';
hents['ê'] = '&ecirc;';
hents['ë'] = '&euml;';
hents['ì'] = '&igrave;';
hents['í'] = '&iacute;';
hents['î'] = '&icirc;';
hents['ï'] = '&iuml;';
hents['ð'] = '&eth;';
hents['ñ'] = '&ntilde;';
hents['ò'] = '&ograve;';
hents['ó'] = '&oacute;';
hents['ô'] = '&ocirc;';
hents['õ'] = '&otilde;';
hents['ö'] = '&ouml;';
hents['÷'] = '&divide;';
hents['ø'] = '&oslash;';
hents['ù'] = '&ugrave;';
hents['ú'] = '&uacute;';
hents['û'] = '&ucirc;';
hents['ü'] = '&uuml;';
hents['ý'] = '&yacute;';
hents['þ'] = '&thorn;';
hents['ÿ'] = '&yuml;';
hents['"'] = '&quot;';
hents['<'] = '&lt;';
hents['>'] = '&gt;';

//allowed tags
var tags = ['p', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'h7', 'h8', 'ul', 'ol', 'li', 'u', 'i', 'b', 'a', 'table', 'tr', 'th', 'td', 'img', 'em', 'strong', 'br', 'sup', 'sub', 'div'];

//tags which should be removed when empty
var rempty = ['p', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'h7', 'h8', 'ul', 'ol', 'li', 'u', 'i', 'b', 'a', 'table', 'tr', 'em', 'strong'];

//allowed atributes for tags
var aattr = new Array();
aattr['a'] = ['href', 'name', 'class', 'style'];
aattr['table'] = ['border', 'bgcolor', 'class', 'style'];
aattr['th'] = ['colspan', 'rowspan'];
aattr['td'] = ['colspan', 'rowspan', 'class', 'style'];
aattr['img'] = ['src', 'width', 'height', 'alt', 'border', 'class', 'style','rel'];
aattr['div'] = ['style', 'class'];

//tags who's content should be deleted
var dctags = ['head'];

//Quote characters
var quotes = ["'", '"'];

//tags which are displayed as a block
var btags = ['p', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'h7', 'h8', 'ul', 'ol', 'table', 'tr', 'th', 'td', 'br'];

var d = '';

function cleanHTML (html)
{
	d = html;
	var o = '';
	var i;
	//Replace all whitespace characters with spaces
	d = d.replace(/(\s|&nbsp;)+/g, ' ');
	//replace weird word characters
	for (i = 0; i < rchars[0].length; i++)
		d = d.replace(new RegExp(rchars[0][i], 'g'), rchars[1][i]);
	
	//initialize flags
	//what the next character is expected to be
	var expected = '';
	//tag text
	var tag = '';
	//tag name
	var tagname = '';
	//what type of tag it is, start, end, or single
	var tagtype = 'start';
	//attribute text
	var attribute = '';
	//attribute name
	var attributen = '';
	//if the attribute has had an equals sign
	var attributeequals = false;
	//if attribute has quotes, and what they are
	var attributequotes = '';
	
	var c = '';
	var n = '';
	
	/*Parser format:
	The parser is divided into three parts:
	The first section is for when the current type of character is known
	The second is for when it is an unknown character in a tag
	The third is for anything outside of a tag
	*/
	
	//editing pass
	for (i = 0; i < d.length; i++)
	{
		//current character
		c = getc(i);
		//next character
		n = getc(i+1);
		
		//***Section for when the current character is known
		
		//if the tagname is expected
		if (expected == 'tagname')
		{
			tagname += c.toLowerCase();
			//lookahead for end of tag name
			if (n == ' ' || n == '>' || n == '/')
			{
				tag += tagname;
				expected = 'tag';
			}
		}
		//if an attribute name is expected
		else if (expected == 'attributen')
		{
			attributen += c.toLowerCase();
			//lookahead for end of attribute name
			if (n == ' ' || n == '>' || n == '/' || n == '=')
			{
				attribute += attributen;
				//check to see if its an attribute without an assigned value
				//determines whether there is anything but spaces between the attribute name and the next equals sign
				if (endOfAttr(i))
				{
					//if the attribute is allowed, add it to the output
					if (ae(attributen, aattr[tagname]))
						tag += attribute;
					
					attribute = '';
					attributen = '';
					attributeequals = false;
					attributequotes = '';
				}
				expected = 'tag';
			}
		}
		//if an attribute value is expected
		else if (expected == 'attributev')
		{
			attribute += c;
			
			//lookahead for end of value
			if ((c == attributequotes) || ((n == ' ' || n == '/' || n == '>') && !attributequotes))
			{
				//if the attribute is allowed, add it to the output
				if (ae(attributen, aattr[tagname]))
					tag += attribute;
				
				attribute = '';
				attributen = '';
				attributeequals = false;
				attributequotes = '';
				
				expected = 'tag';
			}
		}
		
		//***Section for when the character is unknown but it is inside of a tag
		
		else if (expected == 'tag')
		{
			//if its a space
			if (c == ' ')
				tag += c;
			//if its a slash after the tagname, signalling a single tag.
			else if (c == '/' && tagname)
			{
				tag += c;
				tagtype = 'single';
			}
			//if its a slash before the tagname, signalling its an end tag
			else if (c == '/')
			{
				tag += c;
				tagtype = 'end';
			}
			//if its the end of a tag
			else if (c == '>')
			{
				tag += c;
				//if the tag is allowed, add it to the output
				if (ae(tagname, tags))
					o += tag;
				
				//if its a start tag
				if (tagtype == 'start')
				{
					//if the tag is supposed to have its contents deleted
					if (ae(tagname, dctags))
					{
						//if there is an end tag, skip to it in order to delete the tags contents
						if (-1 != (endpos = d.indexOf('</' + tagname, i)))
						{
							//have to make it one less because i gets incremented at the end of the loop
							i = endpos-1;
						}
						//if there isn't an end tag, then it was probably a non-compliant single tag
					}
				}
				
				tag = '';
				tagname = '';
				tagtype = 'start';
				expected = '';
			}
			//if its an attribute name
			else if (tagname && !attributen)
			{
				attributen += c.toLowerCase();
				expected = 'attributen';
				//lookahead for end of attribute name, in case its a one character attribute name
				if (n == ' ' || n == '>' || n == '/' || n == '=')
				{
					attribute += attributen;
					//check to see if its an attribute without an assigned value
					//determines whether there is anything but spaces between the attribute name and the next equals sign
					if (endOfAttr(i))
					{
						//if the attribute is allowed, add it to the output
						if (ae(attributen, attributen))
							tag += attribute;
						
						attribute = '';
						attributen = '';
						attributeequals = false;
						attributequotes = '';
					}
					expected = 'tag';
				}
			}
			//if its a start quote for an attribute value
			else if (ae(c, quotes) && attributeequals)
			{
				attribute += c;
				attributequotes = c;
				expected = 'attributev';
			}
			//if its an attribute value
			else if (attributeequals)
			{
				attribute += c;
				expected = 'attributev';
				
				//lookahead for end of value, in case its only one character
				if ((c == attributequotes) || ((n == ' ' || n == '/' || n == '>') && !attributequotes))
				{
					//if the attribute is allowed, add it to the output
					if (ae(attributen, attributen))
						tag += attribute;
					
					attribute = '';
					attributen = '';
					attributeequals = false;
					attributequotes = '';
					
					expected = 'tag';
				}
			}
			//if its an attribute equals
			else if (c == '=' && attributen)
			{
				attribute += c;
				attributeequals = true;
			}
			//if its the tagname
			else
			{
				tagname += c.toLowerCase();
				expected = 'tagname';
				
				//lookahead for end of tag name, in case its a one character tag name
				if (n == ' ' || n == '>' || n == '/')
				{
					tag += tagname;
					expected = 'tag';
				}
			}
		}
		//if nothing is expected
		else
		{
			//if its the start of a tag
			if (c == '<')
			{
				tag = c;
				expected = 'tag';
			}
			//anything else
			else
				o += htmlentities(c);
		}
	}
	
	//beautifying regexs
	//remove duplicate spaces
	o = o.replace(/\s+/g, ' ');
	//remove unneeded spaces in tags
	o = o.replace(/\s>/g, '>');
	//remove empty tags
	//this loops until there is no change from running the regex
	var remptys = rempty.join('|');
	var oo = o;
	while ((o = o.replace(new RegExp("\\s?<(" + remptys + ")>\s*<\\/\\1>", 'gi'), '')) != oo)
		oo = o;
	//make block tags regex string
	var btagss = btags.join('|');
	//add newlines after block tags
	o = o.replace(new RegExp("\\s?</(" + btagss+ ")>", 'gi'), "</$1>\n");
	//remove spaces before block tags
	o = o.replace(new RegExp("\\s<(" + btagss + ")", 'gi'), "<$1");
	
	//fix lists
	o = o.replace(/((<p.*>\s*(&middot;|&#9642;) .*<\/p.*>\n)+)/gi, "<ul>\n$1</ul>\n");//make ul for dot lists
	o = o.replace(/((<p.*>\s*\d+\S*\. .*<\/p.*>\n)+)/gi, "<ol>\n$1</ol>\n");//make ol for numerical lists
	o = o.replace(/((<p.*>\s*[a-z]+\S*\. .*<\/p.*>\n)+)/gi, "<ol style=\"list-style-type: lower-latin;\">\n$1</ol>\n");//make ol for latin lists
	o = o.replace(/<p(.*)>\s*(&middot;|&#9642;|\d+(\S*)\.|[a-z]+\S*\.) (.*)<\/p(.*)>\n/gi, "\t<li$1>$3$4</li$5>\n");//make li
	
	//extend outer lists around the nesting lists
	o = o.replace(/<\/(ul|ol|ol style="list-style-type: lower-latin;")>\n(<(?:ul|ol|ol style="list-style-type: lower-latin;")>[\s\S]*<\/(?:ul|ol|ol style="list-style-type: lower-latin;")>)\n(?!<(ul|ol|ol style="list-style-type: lower-latin;")>)/g, "</$1>\n$2\n<$1>\n</$1>\n");
	
	//nesting lists
	o = o.replace(/<\/li>\s+<\/ol>\s+<ul>([\s\S]*?)<\/ul>\s+<ol>/g, "\n<ul>$1</ul></li>");//ul in ol
	o = o.replace(/<\/li>\s+<\/ol>\s+<ol style="list-style-type: lower-latin;">([\s\S]*?)<\/ol>\s+<ol>/g, "\n<ol style=\"list-style-type: lower-latin;\">$1</ol></li>");//latin in ol
	o = o.replace(/<\/li>\s+<\/ul>\s+<ol>([\s\S]*?)<\/ol>\s+<ul>/g, "\n<ol>$1</ol></li>");//ol in ul
	o = o.replace(/<\/li>\s+<\/ul>\s+<ol style="list-style-type: lower-latin;">([\s\S]*?)<\/ol>\s+<ul>/g, "\n<ol style=\"list-style-type: lower-latin;\">$1</ol></li>");//latin in ul
	o = o.replace(/<\/li>\s+<\/ol>\s+<ol style="list-style-type: lower-latin;">([\s\S]*?)<\/ol>\s+<ol>/g, "\n<ol style=\"list-style-type: lower-latin;\">$1</ol></li>");//ul in latin
	o = o.replace(/<\/li>\s+<\/ul>\s+<ol style="list-style-type: lower-latin;">([\s\S]*?)<\/ol>\s+<ul>/g, "\n<ol style=\"list-style-type: lower-latin;\">$1</ol></li>");//ul in latin
	//remove empty tags. this is needed a second time to delete empty lists that were created to fix nesting, but weren't needed
	o = o.replace(new RegExp("\\s?<(" + remptys + ")>\s*<\\/\\1>", 'gi'), '');
	
	return o;
}

//array equals
//loops through all the elements of an array to see if any of them equal the test.
function ae (needle, haystack)
{
	if (typeof(haystack) == 'object')
		for (var i = 0; i < haystack.length; i++)
			if (needle == haystack[i])
				return true;
	
	return false;
}

//get character
//return specified character from d
function getc (i)
{
	return d.charAt(i);
}

//end of attr
//determines if their is anything but spaces between the current character, and the next equals sign
function endOfAttr (i)
{
	var between = d.substring(i+1, d.indexOf('=', i+1));
	if (between.replace(/\s+/g, ''))
		return true;
	else
		return false;
}

function htmlentities (character)
{
	if (hents[character])
		return hents[character];
	else
		return character;
}
