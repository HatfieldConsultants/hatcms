To roll-out this library, execute the following SQL:

update pages set template = substring(template,10) where template like 'internal/%';
update pages set template = '_AdminMenuPopup' where template = '_AuditPopup';
update pages set template = '_EventCalendarCategoryPopup'  where template like '_EditCalendarCategoriesPopup';
update pages set deleted = now() where template like '_DeleteJobPopup';
update pages set deleted = now() where template like '_DeleteNewsPopup';


And then copy the DLL file for this library into the site's BIN directory.