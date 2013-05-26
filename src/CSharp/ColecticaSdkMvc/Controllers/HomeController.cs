using Algenta.Colectica.Model;
using Algenta.Colectica.Model.Ddi;
using Algenta.Colectica.Model.Ddi.Utility;
using Algenta.Colectica.Model.Repository;
using Algenta.Colectica.Model.Utility;
using Algenta.Colectica.Repository.Client;
using ColecticaSdkMvc.Models;
using ColecticaSdkMvc.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ColecticaSdkMvc.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
			// Since all the information in the sample Repository is
			// in en-US, we can set the CurrentCulture here and 
			// use MultilingualString's Current property to access
			// the text.
			//
			// If your data has multiple languages, you may want to
			// access those specific languages instead.
			MultilingualString.CurrentCulture = "en-US";

			// Create a new SearchFacet that will find all
			// StudyUnits, CodeSchemes, and CategorySchemes.
			SearchFacet facet = new SearchFacet();
			facet.ItemTypes.Add(DdiItemType.StudyUnit);
			facet.ItemTypes.Add(DdiItemType.CodeScheme);
			facet.ItemTypes.Add(DdiItemType.CategoryScheme);

			// Set the sort order of the results. Options are 
			// Alphabetical, ItemType, MetadataRank, and VersionDate.
			facet.ResultOrdering = SearchResultOrdering.ItemType;

			// Add SearchTerms to the facet to only return results that contain the specified text.
			//facet.SearchTerms.Add("isco");

			// Add Cultures to only search for text in certain languages.
			//facet.Cultures.Add("da-DK");

			// Use MaxResults and ResultOffset to implement paging, if large numbers of items may be returned.
			//facet.MaxResults = 100;
			//facet.ResultOffset = 0;

			// Now that we have a facet, search for the items in the Repository.
			// The client object takes care of making the Web Services calls.
			var client = ClientHelper.GetClient();
			SearchResponse response = client.Search(facet);

			// Create the model object, and add all the search results to 
			// the model's list of results so they can be displayed.
			HomeModel model = new HomeModel();
			foreach (var result in response.Results)
			{
				model.Results.Add(result);
			}

			// Return the view, passing in the model.
            return View(model);
        }
    }
}
