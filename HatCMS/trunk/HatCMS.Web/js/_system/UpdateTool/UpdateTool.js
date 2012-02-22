
$(document).ready( function() {
	alert(3);
	fetchUpdateInfo();
});


function fetchUpdateInfo()
{
    $.ajax({
        type: "GET",
        url: "http://hatcms.googlecode.com/svn/HatCMS_DatabaseMigrationGenerator/version/version.txt",
        dataType: "text"
    })
    .done(function (data){
        alert(data);
    });

}