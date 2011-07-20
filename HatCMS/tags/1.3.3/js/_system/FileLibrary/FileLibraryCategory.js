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

	$('.fileLibraryCategory_chgSaveButton').css('display','none');
	$('.fileLibraryCategory_chgCancelButton').css('display','none');
	$('.fileLibraryCategory_addSaveButton').css('display','none');
	$('.fileLibraryCategory_addCancelButton').css('display','none');
	disableAddRow();
	$(':disabled').css('color','#000000');
	$('input[type="button"]').css({
		'font-size': 'smaller',
		'width': '60px'
	});
}

/*********************************************************
Set all the event handler for buttons
*********************************************************/
function bindEventHandler() {
	// CHG button
	$('.fileLibraryCategory_chgButton').click( function() {
		disableChgButton();
		disableDelButton();
		disableAllChgRow();
		$(this).parent().find('input').css('display','inline');
		var catId = $(this).attr('title');
		enableChgRow( catId );
		$(this).css('display','none');
		$(this).parent().find('.fileLibraryCategory_delButton').css('display','none');
		disableAddButton();
		disableAddRow();
	});
	
	// CHG save button
	$('.fileLibraryCategory_chgSaveButton').click( function() {
		var catId = $(this).attr('title');
		submitForm( catId );
	});
	
	// CHG cancel button
	$('.fileLibraryCategory_chgCancelButton').click( function() {
		$(this).parent().find('input').css('display','none');
		var catId = $(this).attr('title');
		disableChgRow( catId );
		$('.fileLibraryCategory_chgButton').css('display','inline');
		$('.fileLibraryCategory_delButton').css('display','inline');
		enableChgButton();
		enableDelButton();
		enableAddButton();
		disableAddRow();
	});
	
	// ADD button
	$('.fileLibraryCategory_addButton').click( function() {
		disableAllChgRow();
		$(this).parent().find('input').css('display','inline');
		$(this).css('display','none');
		disableChgButton();
		disableDelButton();
		enableAddRow();
	});

	// ADD save button
	$('.fileLibraryCategory_addSaveButton').click( function() {
		submitForm( -1 );
	});

	// ADD cancel button
	$('.fileLibraryCategory_addCancelButton').click( function() {
		$(this).parent().find('input').css('display','none');
		$(this).parent().find('.fileLibraryCategory_addButton').css('display','inline');
		enableChgButton();
		enableDelButton();
		disableAddRow();
	});
	
	// DEL button
	$('.fileLibraryCategory_delButton').click( function() {
		$('#fileLibraryCategory_delete').val( 'true' );
		var catId = $(this).attr('title');
		submitForm( catId );
	});
}

/*********************************************************
Set the active zone ID, validate, and submit the form
*********************************************************/
function submitForm( catId ) {
	$('#fileLibraryCategory_id').val( catId );
	var valid = true;
	if ( catId == -1 ) { // add new
		$('#fileLibraryCategory_addEventRequired').css('border','1px solid grey');
		if ( $('#fileLibraryCategory_addEventRequired').val() == '' ) {
			$('#fileLibraryCategory_addEventRequired').css('border','2px solid red');
			valid = false;
		}

		$('#fileLibraryCategory_addSortOrdinal').css('border','1px solid grey');
		if ( $('#fileLibraryCategory_addSortOrdinal').val() == '' ) {
			$('#fileLibraryCategory_addSortOrdinal').css('border','2px solid red');
			valid = false;
		}

		for (var x = 0; x < langCode.length; x++) {
			$('#fileLibraryCategory_addName_' + langCode[x]).css('border','1px solid grey');
			if ( $('#fileLibraryCategory_addName_' + langCode[x]).val() == '' ) {
				$('#fileLibraryCategory_addName_' + langCode[x]).css('border','2px solid red');
				valid = false;
			}
		}
	}
	else { // update existing
		$('#fileLibraryCategory_eventRequired_' + catId).css('border','1px solid grey');
		if ( $('#fileLibraryCategory_eventRequired_' + catId).val() == '' ) {
			$('#fileLibraryCategory_eventRequired_' + catId).css('border','2px solid red');
			valid = false;
		}

		$('input[name="fileLibraryCategory_sortOrdinal"]').css('border','1px solid grey');
		if ( $('input[name="fileLibraryCategory_sortOrdinal"]').val() == '' ) {
			$('input[name="fileLibraryCategory_sortOrdinal"]').css('border','2px solid red');
			valid = false;
		}

		for (var x = 0; x < langCode.length; x++) {
			$('input[name="fileLibraryCategory_name_' + langCode[x] + '"]').css('border','1px solid grey');
			if ( $('input[name="fileLibraryCategory_name_' + langCode[x] + '"]').val() == '' ) {
				$('input[name="fileLibraryCategory_name_' + langCode[x] + '"]').css('border','2px solid red');
				valid = false;
			}
		}		
	}
	
	if ( valid == true ) {
	    $('#fileLibraryCategory_form').submit();
	}
}

/*********************************************************
Disable all form elements of the ADD row
*********************************************************/
function disableAddRow() {
	$('.fileLibraryCategory_add').attr('disabled','disabled');
	$('.fileLibraryCategory_add').val('');
	$('.fileLibraryCategory_add').css('border','1px solid grey');
}

/*********************************************************
Remove form elements from the CHG row HTML DOM
*********************************************************/
function disableChgRow( catId ) {
	var idForTitle = '#fileLibraryCategory_eventRequired_' + catId;
	var originalValue = $(idForTitle).attr('title');
	$(idForTitle).css('border','1px solid grey');
	$(idForTitle).val(originalValue);
	$(idForTitle).attr('disabled','disabled');

	idForTitle = '#fileLibraryCategory_sortOrdinal_' + catId;
	$(idForTitle).css('display','block');
	$(idForTitle).parent().find('input').remove();

	for ( var x = 0; x < langCode.length; x++ ) {
		var lang = langCode[x];
		idForTitle = '#fileLibraryCategory_name_' + lang + '_' + catId;
		$(idForTitle).css('display','block');
		$(idForTitle).parent().find('input').remove();
	}
}

/*********************************************************
Enable Change/Edit button
Remove all form elements
*********************************************************/
function disableAllChgRow()  {
	$('.fileLibraryCategory_chgCancelButton').each( function() {
		var catId = $(this).attr('title');
		disableChgRow( catId );
		$(this).parent().find('input').css('display','none');
		$(this).parent().find('.fileLibraryCategory_chgButton').css('display','inline');
		$(this).parent().find('.fileLibraryCategory_delButton').css('display','inline');
	});
}

/*********************************************************
Enable all form elements
*********************************************************/
function enableAddRow() {
	$('.fileLibraryCategory_add').removeAttr('disabled');
	$('.fileLibraryCategory_add').val('');
}

/*********************************************************
Read the DIV html to put into <input>
*********************************************************/
function enableChgRow( catId ) {
	var idForTitle = '#fileLibraryCategory_eventRequired_' + catId;
	$(idForTitle).removeAttr('disabled');

	idForTitle = '#fileLibraryCategory_sortOrdinal_' + catId;
	var inputName = 'fileLibraryCategory_sortOrdinal';
	var oldHtml = $(idForTitle).html();
	$(idForTitle).css('display','none');
	var newHtml = '<input type="text" name="' + inputName + '" value="' + oldHtml + '" size="15" maxlength="40" />';
	$(idForTitle).parent().append( newHtml );

	for ( var x = 0; x < langCode.length; x++ ) {
		var lang = langCode[x];
		idForTitle = '#fileLibraryCategory_name_' + lang + '_' + catId;
		inputName = 'fileLibraryCategory_name_' + lang;
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
	$('.fileLibraryCategory_addButton').attr('disabled','disabled');
}

/*********************************************************
Disable all chg button [Edit]
*********************************************************/
function disableChgButton() {
	$('.fileLibraryCategory_chgButton').attr('disabled','disabled');
}

/*********************************************************
Disable all del button [Edit]
*********************************************************/
function disableDelButton() {
	$('.fileLibraryCategory_delButton').attr('disabled','disabled');
}

/*********************************************************
Enable all add button [Add]
*********************************************************/
function enableAddButton() {
	$('.fileLibraryCategory_addButton').removeAttr('disabled');
}

/*********************************************************
Enable all chg button [Edit]
*********************************************************/
function enableChgButton() {
	$('.fileLibraryCategory_chgButton').removeAttr('disabled');
}

/*********************************************************
Enable all del button [Edit]
*********************************************************/
function enableDelButton() {
	$('.fileLibraryCategory_delButton').removeAttr('disabled');
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
	$('.fileLibraryCategory_chgButton').each( function() {
		categoryIdArray.push( $(this).attr('title') );
	});
	
	// for each category id, check to see if there is a <option> tag
	// within the opener.  If yes, update the title; if no, add to
	// the <select> tag.
	for ( var y = 0; y < langCode.length; y++ ) {
	    var lang = langCode[y];
	    for ( var x = 0; x < categoryIdArray.length; x++ ) {
		    var catId = categoryIdArray[x];
		    var catName = $('#fileLibraryCategory_name_' + lang + '_' + catId).html();
			var eventRequired = $('#fileLibraryCategory_eventRequired_' + catId).val();
		    var existing = false;
		    $(openerBody).find('.' + optionId + '_' + lang + ' > option').each( function() {
			    if ( $(this).val() == catId ) {
				    $(this).html( catName );
					$(this).attr('title',eventRequired);
				    existing = true;
			    }
		    });
		    if ( existing == false ) {
				var htmlOption = '<option value="' + catId + '" id="fileLibrary_catName_' + lang + '_' + catId + '" title="' + eventRequired + '">' + catName + '</option>';
			    $(openerBody).find('.' + optionId + '_' + lang).append(htmlOption);
		    }
	    }
	}

	// for delete, remove <option> tag from <select> tag
	for ( var y = 0; y < langCode.length; y++ ) {
	    var lang = langCode[y];
		$(openerBody).find('.fileLibrary_categoryId_' + lang + ' > option').each( function() {
			var deletedOption = true;
			for ( var x = 0; x < categoryIdArray.length; x++ ) {
				var catId = categoryIdArray[x];
				if ( $(this).attr('value') == catId ) {
					deletedOption = false;
					break;
				}
			}
			if ( deletedOption == true ) {
				$(this).remove();
			}
		});
	}
}