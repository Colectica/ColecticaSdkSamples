using Algenta.Colectica.Model.Ddi;
using Algenta.Colectica.Model.Repository;
using Algenta.Colectica.Model.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColecticaSdkSamples.Basic
{
    public class Search
    {
        RepositoryClientBase client;

        public Search()
        {
            this.client = RepositoryIntro.GetClient();
        }

        public void ItemSearch()
        {
            // Search the entire repository for all variables with the word 
            // "age".
            var facet = new SearchFacet();
            facet.ItemTypes.Add(DdiItemType.Variable);
            facet.SearchTerms.Add("age");

            // Perform the search.
            var response = client.Search(facet);

            // Show a summary of the search results. How many items were found
            // and how long the search took.
            Console.WriteLine(string.Format(
                "Displaying {0} of {1} results. Search took {2}.",
                response.ReturnedResults,
                response.TotalResults,
                response.RepositoryTime));

            // Write a line for each result.
            foreach (var result in response.Results)
            {
                Console.WriteLine(result.Label["en-US"]);
            }
        }

        public void RelationshipSearch()
        {
            var physicalInstanceIdentifier = new IdentifierTriple(new Guid(), 1, "int.example");

            // Search for all VariableStatistics related to the PhysicalInstance.
            var facet = new GraphSearchFacet();
            facet.TargetItem = physicalInstanceIdentifier;
            facet.UseDistinctTargetItem = true;
            facet.UseDistinctResultItem = true;
            facet.ItemTypes.Add(DdiItemType.VariableStatistic);

            // Perform the search.
            var results = client.GetRepositoryItemDescriptionsBySubject(facet);

            // Write a line for each result.
            foreach (var result in results)
            {
                Console.WriteLine(result.Label["en-US"]);
            }
        }

        public void SetSearch()
        {
            var dataRelationshipIdentifier = new IdentifierTriple(new Guid(), 1, "int.example");

            // Search for all Categories in the DataRelationship's set.
            var facet = new SetSearchFacet();
            facet.ItemTypes.Add(DdiItemType.Category);

            var resultIdentifiers = client.SearchTypedSet(
                dataRelationshipIdentifier, 
                facet);

            // The set search only returns the identifiers of items.
            // Request the description of each result here.
            var results = client.GetRepositoryItemDescriptions(
                resultIdentifiers.ToIdentifierCollection());

            // Write a line for each result.
            foreach (var result in results)
            {
                Console.WriteLine(result.Label["en-US"]);
            }
        }
    }
}
