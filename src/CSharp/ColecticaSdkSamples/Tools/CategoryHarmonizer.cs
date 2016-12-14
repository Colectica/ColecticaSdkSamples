using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Algenta.Colectica.Model.Ddi;
using Algenta.Colectica.Model.Utility;
using Algenta.Colectica.Model;
using ColecticaSdkSamples.Basic;
using Algenta.Colectica.Model.Repository;

namespace ColecticaSdkSamples.Tools
{
	public class CategoryHarmonizer
	{
		/// <summary>
		/// Counts the number of Categories with the same label that are
		/// found in a particular ResourcePackage in the Repository.
		/// For each unique category label, output the number of categories with that label.
		/// This is useful for determining whether harmonizing a repository's Category items
		/// would be a worthwhile effort.
		/// </summary>
		/// <remarks>
		/// This will provide output like this:
		///    Yes: 1
		///    No: 1
		///    Don't know: 12
		///    Refused: 12
		/// </remarks>
		public void CountUniqueCategoryLabels()
		{
			MultilingualString.CurrentCulture = "en-GB";

			var client = RepositoryIntro.GetClient();

			// To search within a certain set of items, we create a SetSearchFacet object.
			// We can set some properties on this object to tell the repository what
			// items we want to find.
			SetSearchFacet facet = new SetSearchFacet();

			// Find all Category items.
			facet.ItemTypes.Add(DdiItemType.Category);

			// Search under the item with the provided identification.
			// This method returns a collection of identifiers of all matching items.
			var categoryIDs = client.SearchTypedSet(
				new IdentifierTriple(new Guid("e92ac0d9-9f2f-4891-9c42-75bfeafc6d23"), 3, "example.org"),
				facet);

			// The identifiers by themselves don't give us much information.
			// Ask the repository for descriptions of all the categories.
			// This is a very fast call, compared to retrieving the fully populated Category objects,
			// and it has the information we need: each category's name, label, and description.
			var categoryDescriptions = client.GetRepositoryItemDescriptions(categoryIDs.ToIdentifierCollection());

			// Group the categories by label. The GroupBy method returns a list of lists, looking something like this:
			//   "Yes" -> [cat1Desc, cat2Desc, cat3Desc]
			//   "No"  -> [cat4Desc, cat5Desc, cat6Desc]
			//   ...
			var groupedCategories = categoryDescriptions.GroupBy(cat => cat.Label.Current);

			// For each unique category label, output the number of categories with that label.
			foreach (var group in groupedCategories.OrderBy(g => g.Count()))
			{
				Console.WriteLine(group.Key + ": " + group.Count().ToString());
			}
		}

		static void ExtractAndCollapseMissingCategories()
		{
			MultilingualString.CurrentCulture = "en-GB";
			VersionableBase.DefaultAgencyId = "cls";

			string[] missingCategories = 
			{
				"Refusal",
				"Don't Know",
				"Item not applicable",
				"Schedule not applicable",
				"Not applicable",
			};

			var client = RepositoryIntro.GetClient();

			//CategoryScheme newMissingScheme = new CategoryScheme();
			//newMissingScheme.ItemName.Current = "Missing";

			Dictionary<string, Category> categoryMap = new Dictionary<string, Category>();
			//foreach (string catLabel in missingCategories)
			//{
			//    Category cat = new Category();
			//    cat.IsMissing = true;
			//    cat.Label.Current = catLabel;
			//    newMissingScheme.Categories.Add(cat);

			//    categoryMap.Add(catLabel, cat);

			//    client.RegisterItem(cat, new CommitOptions());	
			//}

			//client.RegisterItem(newMissingScheme, new CommitOptions());

			CategoryScheme newMissingScheme = client.GetItem(new Guid("12aaf470-953d-4ebe-9d51-15de00a97921"), "cls", 1, ChildReferenceProcessing.Populate)
				as CategoryScheme;
			foreach (var cat in newMissingScheme.Categories)
			{
				categoryMap.Add(cat.Label.Current, cat);
			}

			// Get all codeSchemes in the repository, so we can update the category references
			// to the new, harmonized categories that we created above.
			//SetSearchFacet facet = new SetSearchFacet();
			//facet.ItemTypes.Add(DdiItemType.CodeScheme);
			//var codeSchemeIDs = client.SearchTypedSet(new IdentifierTriple(new Guid("e92ac0d9-9f2f-4891-9c42-75bfeafc6d23"), 3, "ucl.ac.uk"), facet);
			//var codeSchemes = client.GetItems(codeSchemeIDs.ToIdentifierCollection());

			ResourcePackage resourcePackage = client.GetItem(new IdentifierTriple(new Guid("e92ac0d9-9f2f-4891-9c42-75bfeafc6d23"), 3, "ucl.ac.uk"))
				as ResourcePackage;

			resourcePackage.CategorySchemes.Add(newMissingScheme);

			//foreach (CodeScheme cs in resourcePackage.CodeSchemes)
			//{
			//    client.PopulateItem(cs, false, ChildReferenceProcessing.Populate);

			//    foreach (Code code in cs.Codes)
			//    {
			//        if (categoryMap.ContainsKey(code.Category.Label.Current))
			//        {
			//            var category = categoryMap[code.Category.Label.Current];
			//            code.Category = category;
			//        }
			//    }

			//    cs.Version++;
			//    client.RegisterItem(cs, new CommitOptions());
			//}

			// TODO go through all CategorySchemes and remove the missing categories.
			// Codes will point to the new categories instead.
			// Also, rename the category schemes based on the new contents.
			foreach (CategoryScheme cs in resourcePackage.CategorySchemes)
			{
				if (cs == newMissingScheme) continue;

				client.PopulateItem(cs, false, ChildReferenceProcessing.Populate);

				var toRemove = cs.Categories.Where(cat => categoryMap.ContainsKey(cat.Label.Current)).ToList();
				foreach (var remove in toRemove)
				{
					cs.Categories.Remove(remove);
				}

				cs.ItemName.Current = string.Join(", ", cs.Categories
					.Select(cat => cat.Label.Current)
					.Take(Math.Min(cs.Categories.Count, 3))
					.ToArray());

				cs.Version++;
				client.RegisterItem(cs, new CommitOptions());
			}

			// Don't forget to publish the new ResourcePackage.
			resourcePackage.Version++;
			client.RegisterItem(resourcePackage, new CommitOptions());
		}
	}
}
