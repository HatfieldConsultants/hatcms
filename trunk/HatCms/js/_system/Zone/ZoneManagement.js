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

	$('.zoneManagement_chgSaveButton').css('display','none');
	$('.zoneManagement_chgCancelButton').css('display','none');
	$('.zoneManagement_addSaveButton').css('display','none');
	$('.zoneManagement_addCancelButton').css('display','none');
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
	$('.zoneManagement_chgButton').click( function() {
		disableChgButton();
		disableDelButton();
		disableAllChgRow();
		$(this).parent().find('input').css('display','inline');
		var zoneId = $(this).attr('title');
		enableChgRow( zoneId );
		$(this).css('display','none');
		$(this).parent().find('.zoneManagement_delButton').css('display','none');
		disableAddButton();
		disableAddRow();
	});
	
	// CHG save button
	$('.zoneManagement_chgSaveButton').click( function() {
		var zoneId = $(this).attr('title');
		submitForm( zoneId );
	});
	
	// CHG cancel button
	$('.zoneManagement_chgCancelButton').click( function() {
		$(this).parent().find('input').css('display','none');
		var zoneId = $(this).attr('title');
		disableChgRow( zoneId );
		$('.zoneManagement_chgButton').css('display','inline');
		$('.zoneManagement_delButton').css('display','inline');
		enableChgButton();
		enableDelButton();
		enableAddButton();
		disableAddRow();
	});
	
	// ADD button
	$('.zoneManagement_addButton').click( function() {
		disableAllChgRow();
		$(this).parent().find('input').css('display','inline');
		$(this).css('display','none');
		disableChgButton();
		disableDelButton();
		enableAddRow();
	});

	// ADD save button
	$('.zoneManagement_addSaveButton').click( function() {
		submitForm( -1 );
	});

	// ADD cancel button
	$('.zoneManagement_addCancelButton').click( function() {
		$(this).parent().find('input').css('display','none');
		$(this).parent().find('.zoneManagement_addButton').css('display','inline');
		enableChgButton();
		enableDelButton();
		disableAddRow();
	});
	
	// DEL button
	$('.zoneManagement_delButton').click( function() {
	    var answer = confirm('Are you sure to delete?');
	    if (answer) {
		    $('#zoneManagement_delete').val( 'true' );
		    var zoneId = $(this).attr('title');
		    submitForm( zoneId );
		}
	});
}

/*********************************************************
Set the active zone ID, validate, and submit the form
*********************************************************/
function submitForm( zoneId ) {
	$('#zoneManagement_id').val( zoneId );
	var valid = true;
	if ( zoneId == -1 ) { // add new
		$('#zoneManagement_addStartingPageId').css('border','1px solid grey');
		if ( $('#zoneManagement_addStartingPageId').val() == '' ) {
			$('#zoneManagement_addStartingPageId').css('border','2px solid red');
			valid = false;
		}

		$('#zoneManagement_addName').css('border','1px solid grey');
		if ( $('#zoneManagement_addName').val() == '' ) {
			$('#zoneManagement_addName').css('border','2px solid red');
			valid = false;
		}
	}
	else { // update existing
		$('#zoneManagement_startingPageId_' + zoneId).css('border','1px solid grey');
		if ( $('#zoneManagement_startingPageId_' + zoneId).val() == '' ) {
			$('#zoneManagement_startingPageId_' + zoneId).css('border','2px solid red');
			valid = false;
		}

		$('input[name="zoneManagement_name"]').css('border','1px solid grey');
		if ( $('input[name="zoneManagement_name"]').val() == '' ) {
			$('input[name="zoneManagement_name"]').css('border','2px solid red');
			valid = false;
		}
	}
	
	if ( valid == true ) {
	    document.getElementById('zoneManagement_Form').submit();
	}
}

/*********************************************************
Disable all form elements of the ADD row
*********************************************************/
function disableAddRow() {
	$('.zoneManagement_add').attr('disabled','disabled');
	$('.zoneManagement_add').val('');
	$('.zoneManagement_add').css('border','1px solid grey');
}

/*********************************************************
Remove form elements from the CHG row HTML DOM
*********************************************************/
function disableChgRow( zoneId ) {
	var idForTitle = '#zoneManagement_startingPageId_' + zoneId;
	var originalValue = $(idForTitle).attr('title');
	$(idForTitle).css('border','1px solid grey');
	$(idForTitle).val(originalValue);
	$(idForTitle).attr('disabled','disabled');

	idForTitle = '#zoneManagement_name_' + zoneId;
	$(idForTitle).css('display','block');
	$(idForTitle).parent().find('input').remove();
}

/*********************************************************
Enable Change/Edit button
Remove all form elements
*********************************************************/
function disableAllChgRow()  {
	$('.zoneManagement_chgCancelButton').each( function() {
		var zoneId = $(this).attr('title');
		disableChgRow( zoneId );
		$(this).parent().find('input').css('display','none');
		$(this).parent().find('.zoneManagement_chgButton').css('display','inline');
		$(this).parent().find('.zoneManagement_delButton').css('display','inline');
	});
}

/*********************************************************
Enable all form elements
*********************************************************/
function enableAddRow() {
	$('.zoneManagement_add').removeAttr('disabled');
	$('.zoneManagement_add').val('');
}

/*********************************************************
Read the DIV html to put into <input>
*********************************************************/
function enableChgRow( zoneId ) {
	var idForTitle = '#zoneManagement_startingPageId_' + zoneId;
	$(idForTitle).removeAttr('disabled');

	idForTitle = '#zoneManagement_name_' + zoneId;
	var inputName = 'zoneManagement_name';
	var oldHtml = $(idForTitle).html();
	$(idForTitle).css('display','none');
	var newHtml = '<input type="text" name="' + inputName + '" value="' + oldHtml + '" size="15" maxlength="40" />';
	$(idForTitle).parent().append( newHtml );
}


/*********************************************************
Disable all add button [Add]
*********************************************************/
function disableAddButton() {
	$('.zoneManagement_addButton').attr('disabled','disabled');
}

/*********************************************************
Disable all chg button [Edit]
*********************************************************/
function disableChgButton() {
	$('.zoneManagement_chgButton').attr('disabled','disabled');
}

/*********************************************************
Disable all del button [Edit]
*********************************************************/
function disableDelButton() {
	$('.zoneManagement_delButton').attr('disabled','disabled');
}

/*********************************************************
Enable all add button [Add]
*********************************************************/
function enableAddButton() {
	$('.zoneManagement_addButton').removeAttr('disabled');
}

/*********************************************************
Enable all chg button [Edit]
*********************************************************/
function enableChgButton() {
	$('.zoneManagement_chgButton').removeAttr('disabled');
}

/*********************************************************
Enable all del button [Edit]
*********************************************************/
function enableDelButton() {
	$('.zoneManagement_delButton').removeAttr('disabled');
}
