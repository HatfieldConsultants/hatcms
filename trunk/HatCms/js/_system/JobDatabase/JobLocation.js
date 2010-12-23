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

	$('.jobLocation_chgSaveButton').css('display','none');
	$('.jobLocation_chgCancelButton').css('display','none');
	$('.jobLocation_addSaveButton').css('display','none');
	$('.jobLocation_addCancelButton').css('display','none');
	disableAddRow();
}

/*********************************************************
Set all the event handler for buttons
*********************************************************/
function bindEventHandler() {
	// CHG button
	$('.jobLocation_chgButton').click( function() {
		disableChgButton();
		disableAllChgRow();
		$(this).parent().find('input').css('display','inline');
		var locId = $(this).attr('title');
		enableChgRow( locId );
		$(this).css('display','none');
		disableAddButton();
		disableAddRow();
	});
	
	// CHG save button
	$('.jobLocation_chgSaveButton').click( function() {
		var locId = $(this).attr('title');
		submitForm( locId );
	});
	
	// CHG cancel button
	$('.jobLocation_chgCancelButton').click( function() {
		$(this).parent().find('input').css('display','none');
		var locId = $(this).attr('title');
		disableChgRow( locId );
		$('.jobLocation_chgButton').css('display','inline');
		enableChgButton();
		enableAddButton();
		disableAddRow();
	});
	
	// ADD button
	$('.jobLocation_addButton').click( function() {
		disableAllChgRow();
		$(this).parent().find('input').css('display','inline');
		$(this).css('display','none');
		disableChgButton();
		enableAddRow();
	});

	// ADD save button
	$('.jobLocation_addSaveButton').click( function() {
		submitForm( -1 );
	});

	// ADD cancel button
	$('.jobLocation_addCancelButton').click( function() {
		$(this).parent().find('input').css('display','none');
		$(this).parent().find('.jobLocation_addButton').css('display','inline');
		enableChgButton();
		disableAddRow();
	});
}

/*********************************************************
Set the active category ID, validate, and submit the form
*********************************************************/
function submitForm( locId ) {
	$('#jobLocation_id').val( locId );
	var valid = true;
	if ( locId == -1 ) { // add new
		$('#jobLocation_addSortOrdinal').css('border','1px solid grey');
		if ( $('#jobLocation_addSortOrdinal').val() == '' ) {
			$('#jobLocation_addSortOrdinal').css('border','2px solid red');
			valid = false;
		}
		
		$('#jobLocation_addIsAllLocations').css('border','1px solid grey');
		if ( $('#jobLocation_addIsAllLocations').val() == '' ) {
			$('#jobLocation_addIsAllLocations').css('border','2px solid red');
			valid = false;
		}

		for (var x = 0; x < langCode.length; x++) {
			$('#jobLocation_addTitle_' + langCode[x]).css('border','1px solid grey');
			if ( $('#jobLocation_addTitle_' + langCode[x]).val() == '' ) {
				$('#jobLocation_addTitle_' + langCode[x]).css('border','2px solid red');
				valid = false;
			}

			$('#jobLocation_addLocation_' + langCode[x]).css('border','1px solid grey');
			if ( $('#jobLocation_addLocation_' + langCode[x]).val() == '' ) {
				$('#jobLocation_addLocation_' + langCode[x]).css('border','2px solid red');
				valid = false;
			}
		}
	}
	else { // update existing
		$('input[name="jobLocation_sortOrdinal"]').css('border','1px solid grey');
		if ( $('input[name="jobLocation_sortOrdinal"]').val() == '' ) {
			$('input[name="jobLocation_sortOrdinal"]').css('border','2px solid red');
			valid = false;
		}
		
		$('#jobLocation_isAllLocations_' + locId).css('border','1px solid grey');
		if ( $('#jobLocation_isAllLocations_' + locId).val() == '' ) {
			$('#jobLocation_isAllLocations_' + locId).css('border','2px solid red');
			valid = false;
		}

		for (var x = 0; x < langCode.length; x++) {
			$('input[name="jobLocation_name_' + langCode[x] + '"]').css('border','1px solid grey');
			if ( $('input[name="jobLocation_name_' + langCode[x] + '"]').val() == '' ) {
				$('input[name="jobLocation_name_' + langCode[x] + '"]').css('border','2px solid red');
				valid = false;
			}
		}		
	}
	
	if ( valid == true ) {
	    document.jobLocation_Form.submit();
	}
}

/*********************************************************
Disable all form elements of the ADD row
*********************************************************/
function disableAddRow() {
	$('.jobLocation_add').attr('disabled','disabled');
	$('.jobLocation_add').val('');
	$('.jobLocation_add').css('border','1px solid grey');
}

/*********************************************************
Remove form elements from the CHG row HTML DOM
*********************************************************/
function disableChgRow( locId ) {
	var idForTitle = '#jobLocation_sortOrdinal_' + locId;
	$(idForTitle).css('display','block');	
	$(idForTitle).parent().find('input').remove();
	
	idForTitle = '#jobLocation_isAllLocations_' + locId;
	var originalValue = $(idForTitle).attr('title');
	$(idForTitle).css('border','1px solid grey');
	$(idForTitle).val(originalValue);
	$(idForTitle).attr('disabled','disabled');

	for ( var x = 0; x < langCode.length; x++ ) {
		var lang = langCode[x];
		idForTitle = '#jobLocation_name_' + lang + '_' + locId;
		$(idForTitle).css('display','block');
		$(idForTitle).parent().find('input').remove();
	}
}

/*********************************************************
Enable Change/Edit button
Remove all form elements
*********************************************************/
function disableAllChgRow()  {
	$('.jobLocation_chgCancelButton').each( function() {
		var locId = $(this).attr('title');
		disableChgRow( locId );
		$(this).parent().find('input').css('display','none');
		$(this).parent().find('.jobLocation_chgButton').css('display','inline');
	});
}

/*********************************************************
Enable all form elements
*********************************************************/
function enableAddRow() {
	$('.jobLocation_add').removeAttr('disabled');
	$('.jobLocation_add').val('');
}

/*********************************************************
Read the DIV html to put into <input>
*********************************************************/
function enableChgRow( locId ) {
	var idForTitle = '#jobLocation_sortOrdinal_' + locId;
	var inputName = 'jobLocation_sortOrdinal';
	var oldHtml = $(idForTitle).html();
	$(idForTitle).css('display','none');
	var newHtml = '<input type="text" name="' + inputName + '" value="' + oldHtml + '" size="15" maxlength="40" />';
	$(idForTitle).parent().append( newHtml );
	
	idForTitle = '#jobLocation_isAllLocations_' + locId;
	$(idForTitle).removeAttr('disabled');

	for ( var x = 0; x < langCode.length; x++ ) {
		var lang = langCode[x];
		idForTitle = '#jobLocation_name_' + lang + '_' + locId;
		inputName = 'jobLocation_name_' + lang;
		oldHtml = $(idForTitle).html();
		$(idForTitle).css('display','none');
		newHtml = '<input type="text" name="' + inputName + '" value="' + oldHtml + '" size="15" maxlength="40" />';
		$(idForTitle).parent().append( newHtml );
	}
}


/*********************************************************
Disable all add button [Add]
*********************************************************/
function disableAddButton() {
	$('.jobLocation_addButton').attr('disabled','disabled');
}

/*********************************************************
Disable all chg button [Edit]
*********************************************************/
function disableChgButton() {
	$('.jobLocation_chgButton').attr('disabled','disabled');
}

/*********************************************************
Enable all add button [Add]
*********************************************************/
function enableAddButton() {
	$('.jobLocation_addButton').removeAttr('disabled');
}

/*********************************************************
Enable all chg button [Edit]
*********************************************************/
function enableChgButton() {
	$('.jobLocation_chgButton').removeAttr('disabled');
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

	// obtain all the location id when listing the table
	var locIdArray = [];
	$('.jobLocation_chgButton').each( function() {
		locIdArray.push( $(this).attr('title') );
	});
	
	// for each location id, check to see if there is a <option> tag
	// within the opener.  If yes, update the locName; if no, add to
	// the <select> tag.
	for ( var y = 0; y < langCode.length; y++ ) {
	    var lang = langCode[y];
	    for ( var x = 0; x < locIdArray.length; x++ ) {
		    var locId = locIdArray[x];
		    var locName = $('#jobLocation_name_' + lang + '_' + locId).html();
		    var existing = false;
		    $(openerBody).find('#' + optionId + lang + ' > option').each( function() {
			    if ( $(this).val() == locId ) {
				    $(this).html( locName );
				    existing = true;
			    }
		    });
		    if ( existing == false ) {
			    $(openerBody).find('#' + optionId + lang).append('<option value="' + locId + '">' + locName + '</option>');
		    }
	    }
	}
}
