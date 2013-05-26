using Algenta.Colectica.Model;
using Algenta.Colectica.Model.Ddi;
using Algenta.Colectica.Model.Ddi.Utility;
using Algenta.Colectica.Model.Repository;
using Algenta.Colectica.Model.Utility;
using ColecticaSdkMvc.Models;
using ColecticaSdkMvc.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ColecticaSdkMvc.Controllers
{
    public class ItemController : Controller
    {
        //
        // GET: /Item/

        public ActionResult Index(string agency, Guid id)
        {
			MultilingualString.CurrentCulture = "en-US";
			
			var client = ClientHelper.GetClient();

			// Retrieve the requested item from the Repository.
			// Populate the item's children, so we can display information about them.
			IVersionable item = client.GetLatestItem(id, agency,
				 ChildReferenceProcessing.Populate);

			// To populate more than one level of children, you can use the GraphPopulator.
			//GraphPopulator populator = new GraphPopulator(client);
			//item.Accept(populator);

			// The type of model and the view we want depends on the item type.
			// This sample only provides specific support for a few item types,
			// so we will just hard-code the type checking below.
			ItemModel model = null;
			string viewName = string.Empty;

			if (item is CategoryScheme)
			{
				var categoryList = item as CategoryScheme;

				// Create the model and set the item as a property, so it's contents can be displayed
				var categorySchemeModel = new CategorySchemeModel();
				categorySchemeModel.CategoryScheme = categoryList;
				
				model = categorySchemeModel;
				viewName = "CategoryList";
			}
			else if (item is StudyUnit)
			{
				var studyUnit = item as StudyUnit;

				// Create the model and set the item as a property, so it's contents can be displayed
				var studyModel = new StudyUnitModel();
				studyModel.StudyUnit = studyUnit;

				// Use a set search to get a list of all questions that are referenced
				// by the study. A set search will return items that may be several steps
				// away.
				SetSearchFacet setFacet = new SetSearchFacet();
				setFacet.ItemTypes.Add(DdiItemType.QuestionItem);

				var matches = client.SearchTypedSet(studyUnit.CompositeId,
					setFacet);
				var infoList = client.GetRepositoryItemDescriptions(matches.ToIdentifierCollection());

				foreach (var info in infoList)
				{
					studyModel.Questions.Add(info);
				}

				model = studyModel;
				viewName = "StudyUnit";
			}
			else if (item is CodeScheme)
			{
				var codeScheme = item as CodeScheme;

				// Create the model and set the item as a property, so it's contents can be displayed
				var codeSchemeModel = new CodeSchemeModel();
				codeSchemeModel.CodeScheme = codeScheme;

				model = codeSchemeModel;
				viewName = "CodeList";
			}
			else
			{
				model = new ItemModel();
				viewName = "GenericItem";
			}

			// Fopr all item types, get the version history of the item,
			// and add the information to the model.
			var history = client.GetVersionHistory(id, agency);
			foreach (var version in history)
			{
				model.History.Add(version);
			}

			// Use a graph search to find a list of all items that 
			// directly reference this item.
			GraphSearchFacet facet = new GraphSearchFacet();
			facet.TargetItem = item.CompositeId;
			facet.UseDistinctResultItem = true;

			var referencingItemsDescriptions = client.GetRepositoryItemDescriptionsByObject(facet);

			// Add the list of referencing items to the model.
			foreach (var info in referencingItemsDescriptions)
			{
				model.ReferencingItems.Add(info);
			}
			
			// Return the appropriate view, sending in the model.
            return View(viewName, model);
        }

    }
}
