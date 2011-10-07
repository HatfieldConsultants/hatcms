/*************************************************
* validate form for registering project
**************************************************/
$(document).ready( function() {
    $('.registerProjectform').submit( function() {
        var allFilled = true;
        $('.mandatoryField').each( function() {
            var isEmail = $(this).attr('name').toLowerCase().match(/email$/);
            
            if ( $(this).val() == '' ) {
                $(this).css('border','2px solid red');
                allFilled = false;
            }
            else if ( isEmail == 'email' ) {
                var validEmail = validateEmail( $(this).val() );
                if ( validEmail == true ) {
                    $(this).css('border','1px solid grey');
                }
                else {
                    $(this).css('border','2px solid red');
                    allFilled = false;
                }
            }
            else {
                $(this).css('border','1px solid grey');
            }
        });
        if ( allFilled == false ) {
            return false;
        }
    });
});

/*************************************************
* validate email
**************************************************/
function validateEmail( emailAddr ) {
    var reg = /^([A-Za-z0-9_\-\.])+\@([A-Za-z0-9_\-\.])+\.([A-Za-z]{2,4})$/;
    if( reg.test(emailAddr) == false ) {
        return false;
    }
    return true;
}

/*************************************************
* Restrict to numeric input e.charCode:
*   [48 to 57] 0 to 9
* And e.keyCode:
*   [8] backspace, [9] tab, [35] end, [36] home,
*   [37] left arrow, [39] right arrow, [46] delete
**************************************************/
function numbersOnly(e) {
    if (e.charCode > 0) {
        if (e.charCode<48 || e.charCode>57) { //if not a number
            return false; //disable key press
        }
        else {
            return true;
        }
    }
    else {
        if (e.keyCode!=8 && e.keyCode!=9 && e.keyCode!=35 && e.keyCode!=36 && e.keyCode!=37 && e.keyCode!=39 && e.keyCode!=46) {
            return false;
        }
        else {
            return true;
        }
    }
}