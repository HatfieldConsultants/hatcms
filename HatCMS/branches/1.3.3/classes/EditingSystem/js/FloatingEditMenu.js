
	var dragOn =0 ;
	var dragDiv = null;
	var dragX = 0, dragY = 0;
	var zMax = 0;
	var dragInit = 0;
	var persistKey = null;

	function initDrag() {
		if (document.layers) {
			document.captureEvents(Event.MOUSEMOVE | Event.MOUSEDOWN | Event.MOUSEUP);}

		document.onmousemove = dragf;
		document.onmousedown = dragf;
		document.onmouseup = dragf;
		dragDiv = null;
		dragInit = 1;

		if (document.getElementsByTagName) {
			zMax = document.getElementsByTagName("DIV").length;}
		else if (document.all) {
			zMax = document.body.all.tags("DIV").length;}
		else if (document.layers) {
			zMax = document.layers.length;}
	}

	function dragf(arg) {
		ev = arg ? arg : event;
		if (dragDiv && ev.type == "mousedown") {
			dragOn = 1;
			dragX = (ev.pageX ? ev.pageX : ev.clientX) - parseInt(dragDiv.style.left);
			dragY = (ev.pageY ? ev.pageY : ev.clientY) - parseInt(dragDiv.style.top);
			dragDiv.style.zIndex = zMax++; // remove this line to preserve z-indexes
			return false;
		}

		if (ev.type == "mouseup") {
		    dragOn = 0;
		    if (persistKey && dragDiv){
		        FloatingMenuCookies.writeCookie(persistKey+"_left", dragDiv.style.left);
		        FloatingMenuCookies.writeCookie(persistKey+"_top", dragDiv.style.top);
		        }
			}

		if (dragDiv && ev.type == "mousemove" && dragOn) {
			dragDiv.style.left = ((ev.pageX ? ev.pageX : ev.clientX) - dragX)+'px';
			dragDiv.style.top = ((ev.pageY ? ev.pageY : ev.clientY) - dragY)+'px';
			return false;
		}

		if (ev.type == "mouseout") {
			if (!dragOn) {
				dragDiv=null;}
		}
	}

	function drag(div, _persistKey) {
		if (!dragInit) {
			initDrag();}
		if (!dragOn) {
			dragDiv = document.getElementById ? document.getElementById(div) : 
								(document.all ? document.all[div] : 
								(document.layers ? document.layers[div] : null));
			if (document.layers) {
				dragDiv.style = dragDiv;}
			dragDiv.onmouseout = dragf;
			persistKey = _persistKey;
		}
	}

	function OpenCloseDiv(divName){
		var dragDivName;
		dragDivName = document.getElementById ? document.getElementById(divName) : 
				(document.all ? document.all[divName] : 
				(document.layers ? document.layers[divName] : null));

		if (dragDivName)
		{
			if (dragDivName.style.display == "none") {
				dragDivName.style.display="block";}
			else {
				dragDivName.style.display="none";}
		}
	}

	var FloatingMenuCookies = 
	{
		DeletedCookieValue : "!!DEL~~",

		writeCookie : function(cookieName, cookieValue) 
		{      
			this._writePersistentCookie (cookieName,cookieValue,"years", 1);  
			return true;      
		},  

		deleteCookie : function (cookieName) 
		{
		  // note: we need to set the cookie to another value because most browsers do not
		  //       clean-up cookies when they expire
		  if (this._getCookieValue (cookieName)) 
			this._writePersistentCookie (cookieName,this.DeletedCookieValue,"years", -1);  
		  return true;     
		}, // _deleteCookie

		getCookieValue : function (cookieName) 
		{
		  var exp = new RegExp (escape(cookieName) + "=([^;]+)");
		  if (exp.test (document.cookie + ";")) {
			exp.exec (document.cookie + ";");
			return unescape(RegExp.$1);
		  }
		  else return null;
		}, // _getCookieValue

		_writePersistentCookie : function (CookieName, CookieValue, periodType, offset) 
		{

		  var expireDate = new Date ();
		  offset = offset / 1;
		  
		  var myPeriodType = periodType;
		  switch (myPeriodType.toLowerCase()) {
			case "years":
			  expireDate.setYear(expireDate.getFullYear()+offset);
			  break;
			case "months":
			  expireDate.setMonth(expireDate.getMonth()+offset);
			  break;
			case "days":
			  expireDate.setDate(expireDate.getDate()+offset);
			  break;
			case "hours":
			  expireDate.setHours(expireDate.getHours()+offset);
			  break;
			case "minutes":
			  expireDate.setMinutes(expireDate.getMinutes()+offset);
			  break;
			default:
			  alert ("Invalid periodType parameter for writePersistentCookie()");
			  break;
		  } 
		  
		  document.cookie = escape(CookieName ) + "=" + escape(CookieValue) + "; expires=" + expireDate.toGMTString() + "; path=/";
		} // _writePersistentCookie

	}; // hatCookies object	

var num = 1;
function EditMenuShowModal(url, width, height) 
{
	
	var winleft = (screen.width - width) / 2;
	var winUp = (screen.height - height) / 2;
	if (window.showModalDialog) 
	{
		// window.showModalDialog(url,"name","dialogWidth:255px;dialogHeight:250px");
		window.open(url,'name'+num,'left='+winleft+',top='+winUp+',height='+height+',width='+width+',toolbar=no,directories=no,status=no,menubar=no,scrollbars=1,resizable=yes,modal=yes');
		
	} else {
		window.open(url,'name'+num,'left='+winleft+',top='+winUp+',height='+height+',width='+width+',toolbar=no,directories=no,status=no,menubar=no,scrollbars=1,resizable=yes,modal=yes');
	}
	num++;
}

function EditMenuConfirmModal(prompt, url, width, height)
{
	var answer = confirm (prompt);
	if (answer)
	{
		EditMenuShowModal(url, width, height);
	}
	
}



