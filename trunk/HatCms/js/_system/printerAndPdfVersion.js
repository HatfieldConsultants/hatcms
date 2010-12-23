/*******************************************************************
* Add query string parm
*******************************************************************/
function addUrlParameter(sourceUrl, parameterName, parameterValue, replaceDuplicates) {
    if ((sourceUrl == null) || (sourceUrl.length == 0)) sourceUrl = document.location.href;
    var urlParts = sourceUrl.split('?');
    var newQueryString = '';
    if (urlParts.length > 1)
    {
        var parameters = urlParts[1].split('&');
        for (var i=0; (i < parameters.length); i++)
        {
			var parameterParts = parameters[i].split('=');
			if (!(replaceDuplicates && parameterParts[0] == parameterName))
			{
				newQueryString = (newQueryString == '') ? '?' : newQueryString + '&';
				newQueryString += parameterParts[0] + '=' + parameterParts[1];
			}
        }
    }
	
	newQueryString = (newQueryString == '') ? '?' : newQueryString + '&';
    newQueryString += parameterName + '=' + parameterValue;
    return urlParts[0] + newQueryString;
}

/*******************************************************************
* format a relative url for html2pdf
*******************************************************************/
function getRelativeUrl( sourceUrl , html2pdfUrl ) {
	sourceUrl = sourceUrl.replace('//','');
	sourceUrl = sourceUrl.substr( sourceUrl.indexOf('/') );
	return html2pdfUrl + sourceUrl;
}

/*******************************************************************
* Remove a <link> css declaration from the <head> section
* - http://www.javascriptkit.com/javatutors/loadjavascriptcss2.shtml
*******************************************************************/
function removeJsCss(filename, filetype){
    var targetelement=(filetype=='js')? 'script' : (filetype=='css')? 'link' : 'none' //determine element type to create nodelist from
    var targetattr=(filetype=='js')? 'src' : (filetype=='css')? 'href' : 'none' //determine corresponding attribute to test for
    var allsuspects=document.getElementsByTagName(targetelement)
    for (var i=allsuspects.length; i>=0; i--){ //search backwards within nodelist for matching elements to remove
        if (allsuspects[i] && allsuspects[i].getAttribute(targetattr)!=null && allsuspects[i].getAttribute(targetattr).indexOf(filename)!=-1 && allsuspects[i].getAttribute(targetattr).indexOf('fullcalendar.css')==-1 )
            allsuspects[i].parentNode.removeChild(allsuspects[i]) //remove element by calling parentNode.removeChild()
    }
}

/*******************************************************************
* Inject a <link> css declaration to the <head> section
* - http://www.javascriptkit.com/javatutors/loadjavascriptcss.shtml
*******************************************************************/
function insertJsCss(filename, filetype){
    if (filetype=='js'){
        var fileref=document.createElement('script')
        fileref.setAttribute('type','text/javascript')
        fileref.setAttribute('src', filename)
    }
    else if (filetype=='css'){
        var fileref=document.createElement('link')
        fileref.setAttribute('rel', 'stylesheet')
        fileref.setAttribute('type', 'text/css')
        fileref.setAttribute('href', filename)
    }
    if (typeof fileref!='undefined')
        document.getElementsByTagName('head')[0].appendChild(fileref)
}

/*******************************************************************
* Create the html anchor elememnt for pdf and print versions
*******************************************************************/
function createAnchor( url, displayHtml ) {
	var anchor = document.createElement('a');
	anchor.id = 'printerLinkButton';
	anchor.setAttribute('href', url);
	anchor.setAttribute('target', '_blank');
	anchor.innerHTML = displayHtml;
	anchor.style.marginLeft = '5px';
	return anchor;
}

/*******************************************************************
* Add icons for printer friendly version and pdf version
*******************************************************************/
function addPrinterAndPdfIcon( printerVer, printerIcon, pdfVer, pdfIcon, placeAfterDom ) {
	var targetDom = document.getElementById(placeAfterDom);

	if (printerVer == true) {
		var printerVerUrl = addUrlParameter( window.location.href, 'print', '1', true );
		targetDom.parentNode.insertBefore( createAnchor(printerVerUrl, '<img src="'+ printerIcon +'" border="0" />'), targetDom.nextSibling );
	}

	if (pdfVer == true) {
		var pdfVerUrl = getRelativeUrl(window.location.href,'/html2pdf/convert.php?URL=');
		targetDom.parentNode.insertBefore( createAnchor(pdfVerUrl, '<img src="'+ pdfIcon +'" border="0" />'), targetDom.nextSibling );	
	}
}

/*******************************************************************
* Remove all the existing css declaration from the <head> section
* (except the jQuery full calendar css), then add a light-weight
* css for printing.
*******************************************************************/
function renderAsPrintVersion( printerVer, printerCss ) {
	if ( !printerVer ) {
		return;
	}

	if (printerCss == undefined || printerCss == '') {
		alert('CSS file for printer version invalid.');
	}
	else {
		removeJsCss('.css','css');
		insertJsCss( printerCss,'css');
	}
}
