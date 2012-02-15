/*
Copyright (c) 2003-2010, CKSource - Frederico Knabben. All rights reserved.
For licensing, see LICENSE.html or http://ckeditor.com/license
*/

CKEDITOR.editorConfig = function( config )
{
	// Define changes to default configuration here. For example:
	// config.language = 'fr';
	// config.uiColor = '#AADC6E';
	
	config.removePlugins = 'elementspath,save,scayt,about,newpage,print,smiley,styles';
	config.extraPlugins = "hatCms,hatCms_styles";
	// 'SpellChecker', 'Scayt'
	
	config.toolbar = 'hatCms';
	config.toolbar_hatCms =
	[
		['Source','-','Maximize'],
		['Undo','Redo'],

		['Bold','Italic','Underline','-','Subscript','Superscript'],
		['NumberedList','BulletedList','-','Outdent','Indent'],
		['JustifyLeft','JustifyCenter','JustifyRight','JustifyBlock'],
		['Link','Unlink'],
		['Image','Table','SpecialChar'], 		
		['TextColor','BGColor','CleanHTML'],
		'/',
		['Styles','Format','Font','FontSize']
	   
	];
	config.skin = 'v2';
	config.toolbarCanCollapse = false;	
	config.resize_enabled = false;
	config.emailProtection = 'encode';
};
