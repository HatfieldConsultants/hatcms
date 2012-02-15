/*********************************************************
Define the lang code array
*********************************************************/
//langCode = ['en','pt'];

/*********************************************************
DOM ready funtion
*********************************************************/
$(document).ready( function() {
	initFormating();
	bindEventHandler();
});

/*********************************************************
Set the init formats
*********************************************************/
function initFormating() {
    $('td').attr('valign','top');
	$('option:selected').each( function() {
		$(this).parent().css('background-color', $(this).val() );
	});
	$('.eventCalendarCategory_chgSaveButton').css('display','none');
	$('.eventCalendarCategory_chgCancelButton').css('display','none');
	$('.eventCalendarCategory_addSaveButton').css('display','none');
	$('.eventCalendarCategory_addCancelButton').css('display','none');
	disableAddRow();
}

/*********************************************************
Set all the event handler for buttons
*********************************************************/
function bindEventHandler() {
	// CHG button
	$('.eventCalendarCategory_chgButton').click( function() {
		disableChgButton();
		disableAllChgRow();
		$(this).parent().find('input').css('display','inline');
		var catId = $(this).attr('title');
		enableChgRow( catId );
		$(this).css('display','none');
		disableAddButton();
		disableAddRow();
	});
	
	// CHG save button
	$('.eventCalendarCategory_chgSaveButton').click( function() {
		var catId = $(this).attr('title');
		submitForm( catId );
	});
	
	// CHG cancel button
	$('.eventCalendarCategory_chgCancelButton').click( function() {
		$(this).parent().find('input').css('display','none');
		var catId = $(this).attr('title');
		disableChgRow( catId );
		$('.eventCalendarCategory_chgButton').css('display','inline');
		enableChgButton();
		enableAddButton();
		disableAddRow();
	});
	
	// ADD button
	$('.eventCalendarCategory_addButton').click( function() {
		disableAllChgRow();
		$(this).parent().find('input').css('display','inline');
		$(this).css('display','none');
		disableChgButton();
		enableAddRow();
	});

	// ADD save button
	$('.eventCalendarCategory_addSaveButton').click( function() {
		submitForm( -1 );
	});

	// ADD cancel button
	$('.eventCalendarCategory_addCancelButton').click( function() {
		$(this).parent().find('input').css('display','none');
		$(this).parent().find('.eventCalendarCategory_addButton').css('display','inline');
		enableChgButton();
		disableAddRow();
	});
}

/*********************************************************
Set the active category ID, validate, and submit the form
*********************************************************/
function submitForm( catId ) {
	$('#eventCalendarCategory_id').val( catId );
	var valid = true;
	if ( catId == -1 ) { // add new
		$('#eventCalendarCategory_addColorHex').css('border','1px solid grey');
		if ( $('#eventCalendarCategory_addColorHex').val() == '' ) {
			$('#eventCalendarCategory_addColorHex').css('border','2px solid red');
			valid = false;
		}
		
		for (var x = 0; x < langCode.length; x++) {
			$('#eventCalendarCategory_addTitle_' + langCode[x]).css('border','1px solid grey');
			if ( $('#eventCalendarCategory_addTitle_' + langCode[x]).val() == '' ) {
				$('#eventCalendarCategory_addTitle_' + langCode[x]).css('border','2px solid red');
				valid = false;
			}

			$('#eventCalendarCategory_addDescription_' + langCode[x]).css('border','1px solid grey');
			if ( $('#eventCalendarCategory_addDescription_' + langCode[x]).val() == '' ) {
				$('#eventCalendarCategory_addDescription_' + langCode[x]).css('border','2px solid red');
				valid = false;
			}
		}
	}
	else { // update existing
		$('#eventCalendarCategory_colorHex_' + catId).css('border','1px solid grey');
		if ( $('#eventCalendarCategory_colorHex_' + catId).val() == '' ) {
			$('#eventCalendarCategory_colorHex_' + catId).css('border','2px solid red');
			valid = false;
		}
		
		for (var x = 0; x < langCode.length; x++) {
			$('input[name="eventCalendarCategory_title_' + langCode[x] + '"]').css('border','1px solid grey');
			if ( $('input[name="eventCalendarCategory_title_' + langCode[x] + '"]').val() == '' ) {
				$('input[name="eventCalendarCategory_title_' + langCode[x] + '"]').css('border','2px solid red');
				valid = false;
			}
			
			$('textarea[name="eventCalendarCategory_description_' + langCode[x] + '"]').css('border','1px solid grey');
			if ( $('textarea[name="eventCalendarCategory_description_' + langCode[x] + '"]').val() == '' ) {
				$('textarea[name="eventCalendarCategory_description_' + langCode[x] + '"]').css('border','2px solid red');
				valid = false;
			}			
		}		
	}
	
	if ( valid == true ) {
	    document.eventCalendarCategory_Form.submit();
	}
}

/*********************************************************
Disable all form elements of the ADD row
*********************************************************/
function disableAddRow() {
	$('.eventCalendarCategory_add').attr('disabled','disabled');
	$('.eventCalendarCategory_add').val('');
	$('.eventCalendarCategory_add').css('border','1px solid grey');
	$('#eventCalendarCategory_addColorHex').css('background-color','#FFFFFF');
}

/*********************************************************
Remove form elements from the CHG row HTML DOM
*********************************************************/
function disableChgRow( catId ) {
	$('#eventCalendarCategory_colorHex_' + catId).attr('disabled','disabled');
	var originalColor = $('#eventCalendarCategory_colorHex_' + catId).attr('title');
	$('#eventCalendarCategory_colorHex_' + catId).css('background-color',originalColor);
	$('#eventCalendarCategory_colorHex_' + catId).css('border','1px solid grey');
	$('#eventCalendarCategory_colorHex_' + catId).val(originalColor);

	for ( var x = 0; x < langCode.length; x++ ) {
		var lang = langCode[x];
		var idForTitle = '#eventCalendarCategory_title_' + lang + '_' + catId;
		$(idForTitle).css('display','block');
		$(idForTitle).parent().find('input').remove();
		
		var idForDesc = '#eventCalendarCategory_description_' + lang + '_' + catId;
		$(idForDesc).css('display','block');
		$(idForDesc).parent().find('textarea').remove();
	}
}

/*********************************************************
Enable Change/Edit button
Remove all form elements
*********************************************************/
function disableAllChgRow()  {
	$('.eventCalendarCategory_chgCancelButton').each( function() {
		var catId = $(this).attr('title');
		disableChgRow( catId );
		$(this).parent().find('input').css('display','none');
		$(this).parent().find('.eventCalendarCategory_chgButton').css('display','inline');
	});
}

/*********************************************************
Enable all form elements
*********************************************************/
function enableAddRow() {
	$('.eventCalendarCategory_add').removeAttr('disabled');
	$('.eventCalendarCategory_add').val('');
	$('#eventCalendarCategory_addColorHex').css('background-color','#FFFFFF');
}

/*********************************************************
Enable the Color Hex dropdown list
Read the DIV html to put into <input> or <textarea>
*********************************************************/
function enableChgRow( catId ) {
	$('#eventCalendarCategory_colorHex_' + catId).removeAttr('disabled');

	for ( var x = 0; x < langCode.length; x++ ) {
		var lang = langCode[x];
		var idForTitle = '#eventCalendarCategory_title_' + lang + '_' + catId;
		var inputName = 'eventCalendarCategory_title_' + lang;
		var oldHtml = $(idForTitle).html();
		$(idForTitle).css('display','none');
		var newHtml = '<input type="text" name="' + inputName + '" value="' + oldHtml + '" size="15" maxlength="40" />';
		$(idForTitle).parent().append( newHtml );
		
		var idForDesc = '#eventCalendarCategory_description_' + lang + '_' + catId;
		var inputName2 = 'eventCalendarCategory_description_' + lang;
		var oldHtml2 = $(idForDesc).html();
		$(idForDesc).css('display','none');
		var newHtml2 = '<textarea rows="3" cols=\"20\" name="' + inputName2 + '">' + br2nl(oldHtml2) + '</textarea>';
		$(idForDesc).parent().append( newHtml2 );
	}
}


/*********************************************************
Disable all add button [Add]
*********************************************************/
function disableAddButton() {
	$('.eventCalendarCategory_addButton').attr('disabled','disabled');
}

/*********************************************************
Disable all chg button [Edit]
*********************************************************/
function disableChgButton() {
	$('.eventCalendarCategory_chgButton').attr('disabled','disabled');
}

/*********************************************************
Enable all add button [Add]
*********************************************************/
function enableAddButton() {
	$('.eventCalendarCategory_addButton').removeAttr('disabled');
}

/*********************************************************
Enable all chg button [Edit]
*********************************************************/
function enableChgButton() {
	$('.eventCalendarCategory_chgButton').removeAttr('disabled');
}

/*********************************************************
Replace the <br /> tag by new line char "\n"
*********************************************************/
function br2nl( inputStr ) {
	return inputStr.replace(/<br\s*\/?>/mg,"\n");
}

/*********************************************************
Update the html dropdown list during the pop up page onload()
*********************************************************/
function updateOpener( optionId ) {
    // if no parent html, no <select> to update
	var openerBody = window.opener.document.body;
	if (openerBody == null || openerBody === undefined) {
	    return;
	}

	// obtain all the category id when listing the table
	var categoryIdArray = [];
	$('.eventCalendarCategory_chgButton').each( function() {
		categoryIdArray.push( $(this).attr('title') );
	});
	
	// for each category id, check to see if there is a <option> tag
	// within the opener.  If yes, update the title; if no, add to
	// the <select> tag.
	for ( var y = 0; y < langCode.length; y++ ) {
	    var lang = langCode[y];
	    for ( var x = 0; x < categoryIdArray.length; x++ ) {
		    var catId = categoryIdArray[x];
		    var title = $('#eventCalendarCategory_title_' + lang + '_' + catId).html();
		    var existing = false;
		    $(openerBody).find('.' + optionId + '_' + lang + ' > option').each( function() {
			    if ( $(this).val() == catId ) {
				    $(this).html( title );
				    existing = true;
			    }
		    });
		    if ( existing == false ) {
			    $(openerBody).find('.' + optionId + '_' + lang).append('<option value="' + catId + '">' + title + '</option>');
		    }
	    }
	}
}