// --------------------------------------------------------------------------------
// TopNav.js: 
// Matthew Hogg 20-Aug-2004
// Drives the dropdown navigation for the front-end of a site.
// --------------------------------------------------------------------------------

var navDistribute = false;
var navBrowserOK = true;
var navigation = null
var navCloseSub = true;
var navActiveSub = "";
var navOffset = 0;
var navBound = false;


// --------------------------------------------------------------------------------
// TopNavMoveX()
// Determine horizontal positioning of submenu.
// --------------------------------------------------------------------------------
// Arguments:
//	- subid: id of submenu to calculate for [string]
// Returns:
//	- desired x-coordinate for submenu [integer]
// --------------------------------------------------------------------------------
function TopNavMoveX(subid) {
	var smwidth = parseInt((f_GetElementStyle(document.getElementById(subid), "width")).replace("px", ""), 10);
	var mmwidth = parseInt(navigation.offsetWidth, 10);

	var leftX = f_GetTotalOffset(document.getElementById((subid).replace("TopNav_sub", "TopNav_")), "offsetLeft");
	var menuOffset = f_GetTotalOffset(navigation, "offsetLeft");

	if (document.all && navigator.platform.toLowerCase().indexOf("win") == -1) {
		var padding = parseInt((f_GetElementStyle(document.getElementById((subid).replace("TopNav_sub", "TopNav_")), "padding-left")).replace("px", ""), 10);
		if (isNaN(padding)) padding = 0;
		var bod = parseInt((f_GetElementStyle(document.getElementsByTagName("body")[0], "margin-left")).replace("px", ""), 10);
		
		if (navigation.offsetParent.tagName.toLowerCase() == "body") {
			return leftX - padding + bod;
		} else {
			return leftX - padding;		
		}
	} else {
		if ( ((leftX - menuOffset) + smwidth > mmwidth) && navBound) {
			return leftX - ((leftX - menuOffset) + smwidth) + mmwidth;
		} else {
			return leftX;
		}
	}	
}

// --------------------------------------------------------------------------------
// TopNavMoveY()
// Determine vertical positioning of submenu.
// --------------------------------------------------------------------------------
// Arguments:
//	- subid: id of submenu to calculate for [string]
// Returns:
//	- desired y-coordinate for submenu [integer]
// --------------------------------------------------------------------------------
function TopNavMoveY(subid) {
	var menuOffset = f_GetTotalOffset(navigation, "offsetTop");
	
	if (document.all && navigator.platform.toLowerCase().indexOf("win") == -1) {
		var bod = parseInt((f_GetElementStyle(document.getElementsByTagName("body")[0], "margin-top")).replace("px", ""), 10);
		if (navigation.offsetParent.tagName.toLowerCase() == "body") {
			return menuOffset + navigation.offsetHeight + navOffset + bod;
		} else {
			return navigation.offsetHeight + navOffset;
		}
	} else {
		return menuOffset + navigation.offsetHeight + navOffset;
	}
}

// --------------------------------------------------------------------------------
// TopNavOverlap()
// Determine if two elements are overlapping.
// --------------------------------------------------------------------------------
// Arguments:
//	- a: element 1 [object]
//	- b: element 2 [object]
// Returns:
//	- are elements overlapping? [boolean]
// --------------------------------------------------------------------------------
function TopNavOverlap(a, b) {
	var ax = f_GetTotalOffset(a, "offsetLeft");
	var ay = f_GetTotalOffset(a, "offsetTop");
	var aw = a.offsetWidth;
	var ah = a.offsetHeight;
	var bx = f_GetTotalOffset(b, "offsetLeft");
	var by = f_GetTotalOffset(b, "offsetTop");
	var bw = b.offsetWidth;
	var bh = b.offsetHeight;

    if (((ax + aw) < bx) || (ax > (bx + bw)) || ((ay + ah) < by) || (ay > (by + bh))) return false;
    else return true;
}

// --------------------------------------------------------------------------------
// TopNavFixSelects()
// Make visible any currently hidden select elements.
// --------------------------------------------------------------------------------
// Arguments:
//	- none
// Returns:
//	- nothing
// --------------------------------------------------------------------------------
function TopNavFixSelects() {
	if (document.all) {
		var selects = document.getElementsByTagName("select");
		for (var i = 0; i < selects.length; i++) {
			if (selects[i].style.visibility == "hidden") {
				selects[i].style.visibility = "visible";
			}
		}
	}
}

// --------------------------------------------------------------------------------
// TopNavShowSub()
// Show submenu for menu item that fired a mouseover event.
// --------------------------------------------------------------------------------
// Arguments:
//	- none
// Returns:
//	- nothing
// --------------------------------------------------------------------------------
function TopNavShowSub() {
	var elm = this;
	if (window.event) elm = event.srcElement;
	
	if (navActiveSub != "") {
		document.getElementById(navActiveSub).style.display = "none";
		TopNavFixSelects();
	}
	var subm = document.getElementById((elm.id).replace("TopNav_", "TopNav_sub"))
	
	if (subm) {
		if (subm.getElementsByTagName("li").length > 0) {
	
			subm.style.left = TopNavMoveX(subm.id) + "px";
			subm.style.top = TopNavMoveY(subm.id) + "px";
			subm.style.display = "block";
			
			// hide and select elements that may be under this submenu
			if (document.all) {
				var selects = document.getElementsByTagName("select");
				for (var i = 0; i < selects.length; i++) {
					if (TopNavOverlap(subm, selects[i])) { selects[i].style.visibility = "hidden"; }
				}
			}
			
			// re-hide this submenu if embedded video is underneath (fails in Opera)
			if (!document.all) {
				var flash = document.getElementsByTagName("embed");
				for (var i = 0; i < flash.length; i++) {
					if (TopNavOverlap(subm, flash[i])) {
						subm.style.display = "none";
						break;
					}
				}
			}

			if (subm.style.display != "none") {
				f_AddEvent(subm, "mouseover", NavAlive, false);
				f_AddEvent(subm, "mouseout", TopNavTryHideSub, false);
				navActiveSub = subm.id;
			} else {
				navActiveSub = "";
			}
		}
	} else {
		navActiveSub = "";
	}
	
	navCloseSub = false;
}

// --------------------------------------------------------------------------------
// TopNavTryHideSub()
// Trigger the death of a submenu.
// --------------------------------------------------------------------------------
// Arguments:
//	- none
// Returns:
//	- nothing
// --------------------------------------------------------------------------------
function TopNavTryHideSub() {
	navCloseSub = true;
	setTimeout("TopNavHideSub('" + navActiveSub + "')", 550);
}

// --------------------------------------------------------------------------------
// TopNavHideSub()
// Hide the current submenu.
// --------------------------------------------------------------------------------
// Arguments:
//	- subid - id of the submenu to be hidden [string]
// Returns:
//	- nothing
// --------------------------------------------------------------------------------
function TopNavHideSub(subid) {
	var elm = this;
	if (window.event) elm = event.srcElement;
	if (navCloseSub && document.getElementById(subid)) {
		document.getElementById(subid).style.display = "none";
		TopNavFixSelects();
	}
}

// --------------------------------------------------------------------------------
// NavAlive()
// Keep the current submenu alive.
// --------------------------------------------------------------------------------
// Arguments:
//	- none
// Returns:
//	- nothing
// --------------------------------------------------------------------------------
function NavAlive() { navCloseSub = false; }

// --------------------------------------------------------------------------------
// TopNavInit()
// Initialize the dropdown menus by wiring up the mouse events and sizing items.
// --------------------------------------------------------------------------------
// Arguments:
//	- none
// Returns:
//	- nothing
// --------------------------------------------------------------------------------
function TopNavInit() {
	// alert("init");
	if (document.getElementById) {
		navigation = document.getElementById("TopNav")
		if (navigation) {
			var items = navigation.getElementsByTagName("a")

			// clean up the "last" menu item
			if (items.length > 0) {
				items[items.length - 1].style.borderRightWidth = "0";
				items[items.length - 1].style.marginRight = "0";
			}

			// evenly distribute menu items
			if (navDistribute) {
				var widest = -1;
				var maxwidth = 0;
				var mmwidth = parseInt(navigation.offsetWidth, 10);
				var mminitial = 0;
				var mmused = 0;

				// determine some required numbers
				for (var i = 0; i < items.length; i++) {
					mminitial += parseInt(items[i].offsetWidth, 10);
					if (parseInt(items[i].offsetWidth, 10) > maxwidth) {
						maxwidth = parseInt(items[i].offsetWidth, 10);
						widest = i;
					}
				}
				
				// scale up each menu item's width
				for (var i = 0; i < items.length; i++) {
					var boxmodel = 0;
					var perc = parseInt(items[i].offsetWidth, 10) / mminitial;
					
					// adjust for the box model (borders and padding)
					boxmodel += parseInt((f_GetElementStyle(items[i], "padding-left")).replace("px", ""), 10);
					boxmodel += parseInt((f_GetElementStyle(items[i], "padding-right")).replace("px", ""), 10);
					if (f_GetElementStyle(items[i], "border-right-width").indexOf("px") != -1) boxmodel += parseInt((f_GetElementStyle(items[i], "border-right-width")).replace("px", ""), 10);
					if (f_GetElementStyle(items[i], "border-left-width").indexOf("px") != -1) boxmodel += parseInt((f_GetElementStyle(items[i], "border-left-width")).replace("px", ""), 10);
					
					items[i].style.width = Math.floor(perc * mmwidth) - boxmodel + "px";

					// older version of IE, correct for the box model
					if (document.all && document.expando && parseInt((f_GetElementStyle(items[i], "width")).replace("px", ""), 10) == items[i].offsetWidth) {
						items[i].style.width = Math.floor(perc * mmwidth) + "px";
					}
					
					mmused += Math.floor(perc * mmwidth)
				}
			}
		
			// attach mouse events
			for (var i = 0; i < items.length; i++) {
				var subm = document.getElementById(items[i].id);
				f_AddEvent(subm, "mouseover", TopNavShowSub, false);
				f_AddEvent(subm, "mouseout", TopNavTryHideSub, false);
			}
			
		}
	}
}

function f_AddEvent(elm, evType, fn, useCapture) 
{
	if (elm.addEventListener) {
		elm.addEventListener(evType, fn, useCapture);
		return true;
	} else if (elm.attachEvent) {
		var r = elm.attachEvent("on" + evType, fn);
		return r;
	} else {
		elm.setAttribute("on" + evType, fn);
	}
}

function f_GetElementStyle(elm, prop) {
	if (window.getComputedStyle) {
		return window.getComputedStyle(elm, null).getPropertyValue(prop);
	} else if (elm.currentStyle) {
		var ieProp = "";
		for (var i = 0; i < prop.length; i++) {
			if (prop.charAt(i) == "-") {
				i++;
				ieProp += prop.charAt(i).toUpperCase();
			} else {
				ieProp += prop.charAt(i);
			}
		}
		return eval("elm.currentStyle." + ieProp);
	}
}

function f_GetTotalOffset(elm, off) {
	var totalOffset = 0;
	var item = eval("elm");

	do {
		totalOffset += eval("item." + off);
		item = eval("item.offsetParent");
	} while (item != null);
	return totalOffset;
}