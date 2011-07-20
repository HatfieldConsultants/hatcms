/************************************************
Onload
************************************************/
$(document).ready( function() {
	var colorActive = '#FFFFFF';
	var colorInactive = '#2d2522';
	initTabStyle( colorActive, colorInactive );
	bindEventsForTab( colorActive, colorInactive );
});

/************************************************
Set the css style
************************************************/
function initTabStyle( colorActive, colorInactive ) {
	$('.tab').css({
		'background-color': colorInactive,
		'border': '2px solid ' + colorInactive,
		'padding': '0px'
	});
	$('.tab > ul').css({
		'background-color': colorInactive,
		'list-style-type': 'none',
		'display': 'block',
		'clear': 'both',
		'margin': '0px',
		'padding': '2px'
	});
	$('.tab > ul > li').css({
		'float': 'left',
		'margin': '1px 2px'
	});

	$('.tab > ul > li > a').css({
		'text-decoration': 'none',
		'padding': '1px 10px',
		'font-size': 'smaller',
		'background-color': colorInactive,
		'color': colorActive
	});
	$('.tab > ul > li > a.tabSelected').css({
		'font-size': 'smaller',
		'background-color': colorActive,
		'color': colorInactive
	});

	$('.tab > div').css({
		'display': 'none',
		'clear': 'both',
		'background-color': colorActive,
		'padding': '0.5em 1em'
	});
	$('.tab > div.tabContentSelected').css({
		'display': 'block',
		'clear': 'both',
		'background-color': colorActive,
		'padding': '1em'
	});
}

/************************************************
Bind click, mouse enter, and mouse leave events
************************************************/
function bindEventsForTab( colorActive, colorInactive ) {
	$(".tab > ul > li > a").mouseenter( function() {
		if ( $(this).hasClass('tabSelected') == false ) {
			$(this).css({
				'background-color': colorActive,
				'color': colorInactive
			});
		}
	}).mouseleave( function() {
		if ( $(this).hasClass('tabSelected') == false ) {
			$(this).css({
				'background-color': colorInactive,
				'color': colorActive
			});
		}
	});
}

/************************************************
Upload form submit, detect if an event is needed
for file.  It reads from the 'title' attribute.
************************************************/
function uploadFormSubmit( langCode ) {
	var valid = true;
	
	var domID = '#fileLibrary_categoryId_' + langCode;
	$(domID).css('border','1px solid grey');
	if ( $(domID).val() == '' ) {
		$(domID).css('border','1px solid red');
		valid = false;
	}
	
	var selectedCategoryId = $(domID).val();
	var eventRequired = $('#fileLibrary_catName_' + langCode + '_' + selectedCategoryId).attr('title');
	
	domID = '#fileLibrary_eventPageId_' + langCode;
	$(domID).css('border','1px solid grey');
	if ( $(domID).val() == '-1' && eventRequired == 'true' ) {
		$(domID).css('border','1px solid red');
		valid = false;
	}
	
	domID = '#fileLibrary_filePath_' + langCode;
	$(domID).css('border','1px solid grey');
	if ( $(domID).val() == '' ) {
		$(domID).css('border','1px solid red');
		valid = false;
	}
	
	return valid;
}
