	function ListEl( id, title )
	{
		this.id = id;
		this.title = title;
	}
	
	var allCats = new Array();
	var allGeos = new Array();

	var projCatIds = new Array();
	var projGeoIds = new Array();
	

	function updateListDisplay()
	{
		var html = "";
		html += "Categories: ";

		html += "<ul>";
		if (projCatIds.length > 0)
		{		
			for(var i = 0; i < projCatIds.length; i++)
			{
				html += "<li>"+projCatIds[i].title+" <a href=\"#\" onclick=\"return removeCat("+projCatIds[i].id+");\">[remove]</a></li>";
				html += "<input type=\"hidden\" name=\"catIds\" value=\""+projCatIds[i].id+"\">";
			}
		}
		else
		{
			html += "<li><strong>this project does not belong to any categories!</strong></li>";
		}

		html += "<br>Add a category: <select id=\"AddProjCat\">";
		for(var i = 0; i < allCats.length; i++)
		{
			html += "<option value=\""+allCats[i].id+"\">"+allCats[i].title+"</option>";
		} // for
		html += "</select>";
		html += " <input type=\"button\" onclick=\"return addCat();\" value=\"add category\">";
		html += "</ul>";

		html += "Geographic Regions: ";
		html += "<ul>";
		
		if (projGeoIds.length > 0)
		{		
			for(var i = 0; i < projGeoIds.length; i++)
			{
				html += "<li>"+projGeoIds[i].title+" <a href=\"#\" onclick=\"return removeGeo("+projGeoIds[i].id+");\">[remove]</a></li>";
				html += "<input type=\"hidden\" name=\"geoIds\" value=\""+projGeoIds[i].id+"\">";
			}
		}
		else
		{
			html += "<li><strong>this project does not belong to any geographic areas!</strong></li>";
		}
		html += "<br>Add a geographic area: <select id=\"AddProjGeo\">";
		for(var i = 0; i < allGeos.length; i++)
		{
			html += "<option value=\""+allGeos[i].id+"\">"+allGeos[i].title+"</option>";
		} // for
		html += "</select>";
		html += " <input type=\"button\" onclick=\"return addGeo();\" value=\"add area\">";
		html += "</ul>";

		document.getElementById("categoryList").innerHTML = html;
	}

	function addCat()
	{
		var selEl = document.getElementById("AddProjCat");
		var selCatId = selEl.options[selEl.selectedIndex].value;

		for(var i = 0; i < projCatIds.length; i++)
		{
			if (projCatIds[i].id == selCatId)
			{
				alert("the selected category is already in the list");
				return;
			}
		}

		for(var i = 0; i < allCats.length; i++)
		{
			if (allCats[i].id == selCatId)
			{
				projCatIds[projCatIds.length] = allCats[i];
				break;
			}
		} // for
		updateListDisplay();
	} // addCat
	

	function addGeo()
	{
		var selEl = document.getElementById("AddProjGeo");
		var selGeoId = selEl.options[selEl.selectedIndex].value;

		for(var i = 0; i < projGeoIds.length; i++)
		{
			if (projGeoIds[i].id == selGeoId)
			{
				alert("the selected area is already in the list");
				return;
			}
		}

		for(var i = 0; i < allGeos.length; i++)
		{
			if (allGeos[i].id == selGeoId)
			{
				projGeoIds[projGeoIds.length] = allGeos[i];
				break;
			}
		} // for
		updateListDisplay();
	} // addGeo

	function removeCat(catId)
	{
		for(var i = 0; i < projCatIds.length; i++)
		{
			if (projCatIds[i].id == catId)
			{
				projCatIds.splice(i,1); // remove the i'th element
				break;
			}
		} // for
		updateListDisplay();
	} // removeCat

	function removeGeo(geoId)
	{
		for(var i = 0; i < projGeoIds.length; i++)
		{
			if (projGeoIds[i].id == geoId)
			{
				projGeoIds.splice(i,1); // remove the i'th element
				break;
			}
		} // for
		updateListDisplay();
	} // projGeoIds
