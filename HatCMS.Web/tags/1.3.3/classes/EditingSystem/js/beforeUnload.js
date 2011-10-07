/* BeforeUnload form processing */
/* source: http://codespeak.net/svn/kupu/trunk/kupu/common/kupubeforeunload.js */
/* referenced from: http://plone.org/products/plone/roadmap/24 */

submitting = false; /* set to true in form.onSubmit event */
window.onbeforeunload = function() 
{	
	if (submitting) 
	{		
		return;
	}
	
	return 'Changes made to this page have not been saved. Continuing will lose all of your changes.';
}
