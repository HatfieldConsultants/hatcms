To roll-out this library, execute the following SQL:

update pages set template = substring(template,10) where template like 'internal/%';
update pages set template = '_AdminMenuPopup' where template = '_AuditPopup';

And then copy the DLL file for this library into the site's BIN directory.