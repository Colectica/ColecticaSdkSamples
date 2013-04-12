using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Algenta.Colectica.Model.Utility;
using Algenta.Colectica.Model.Repository;
using Algenta.Colectica.Repository.Client;
using Algenta.Colectica.Model.Ddi;
using Algenta.Colectica.Model.Ddi.Serialization;
using Algenta.Colectica.Model;
using Algenta.Colectica.Model.Ddi.Utility;
using System.IO;
using System.Reflection;

namespace ColecticaSdkSamples.Basic
{
	public class RepositoryIntro
	{
		/// <summary>
		/// Configures and returns a client object that can be used to communicate
		/// with Colectica Repository via its Web Services.
		/// </summary>
		public static RepositoryClientBase GetClient()
		{
			// The WcfRepositoryClient takes a configation object
			// detailing how to connect to the Repository.
			var connectionInfo = new RepositoryConnectionInfo()
			{
				// TODO Replace this with the hostname of your Colectica Repository
				Url = "colecticatest", 

				AuthenticationMethod = RepositoryAuthenticationMethod.UserName,
				
				// TODO Replace these with a valid username and password for your Repository
				UserName = "admin",
				Password = "password",
				
				TransportMethod = RepositoryTransportMethod.NetTcp,
			};
			
			// Create the client object, passing in the connection information.
			WcfRepositoryClient client = new WcfRepositoryClient(connectionInfo);
			return client;
		}

		public void AddAgencyToRepository()
		{
			var client = GetClient();
			
			// Set the Repository to be authoritative for the agency with
			// the identifier "new.agency.id". 
			// For more information about DDI identifiers, see 
			//    http://registry.ddialliance.org/
			client.CreateRepository("new.agency.id", "Just a Test");
		}

		/// <summary>
		/// Create a simple VariableScheme and register it with the Repository.
		/// </summary>
		public void RegisterSimpleVariableScheme()
		{
			MultilingualString.CurrentCulture = "en-US";
			VersionableBase.DefaultAgencyId = "example.org";

			// Create a VariableScheme.
			VariableScheme variableScheme = new VariableScheme();
			variableScheme.ItemName.Current = "My Variables";

			// Add a Variable.
			Variable variable = new Variable();
			variable.ItemName.Current = "var1";
			variableScheme.Variables.Add(variable);

			// Grab a client for our Repository.
			var client = GetClient();

            // Register the variable.
            var options = new CommitOptions();
            client.RegisterItem(variable, options);

			// Register the list. This only registers the variableScheme,
            // not any variables contained in the scheme.
			client.RegisterItem(variableScheme, options);            
		}

		/// <summary>
		/// Load the sample DDI 3.1 file and register all its items with the Repository.
		/// </summary>
		public void RegisterFile()
		{
			// Load the sample data file.
			DDIWorkflowDeserializer deserializer = new DDIWorkflowDeserializer();
			string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var instance = deserializer.LoadDdiFile(Path.Combine(directory, "sample.xml"));

			// Gather all items instance in the DdiInstance we just loaded.
			// We need to do this to get the items in a flat list.
			DirtyItemGatherer gatherer = new DirtyItemGatherer(gatherEvenIfNotDirty:true);
			instance.Accept(gatherer);

			// Grab a client to communicate with the Repository.
			var client = GetClient();

			// Register each item with the Repository.
			foreach (var item in gatherer.DirtyItems)
			{
				client.RegisterItem(item, new CommitOptions());
			}
		}

		/// <summary>
		/// Search for some items, retrieve one, and populate its children.
		/// </summary>
		public void SearchRetrievePopulate()
		{
			var client = GetClient();

			// Create a SearchFacet, which lets us set the parameters of the search.
			SearchFacet facet = new SearchFacet();

			// Find all VariableSchemes in the Repository.
			facet.ItemTypes.Add(DdiItemType.VariableScheme);

			// If we wanted, we could search for certain text.
			//facet.SearchTerms.Add("Variables");

			// Submit the search to the Repository.
			SearchResponse response = client.Search(facet);

			if (response.ReturnedResults > 0)
			{
				// Use GetItem to retrieve the first VariableScheme in the search results.
				// By passing in Populate as the last parameter, we are requesting the 
				// Repository to send back the VariableScheme along with all child Variables information.
				// By default, the Repository would only Instantiate the child Variables, meaning
				// they would contain identification information, but all other properties would be blank.
				var item = client.GetItem(response.Results[0].CompositeId, ChildReferenceProcessing.Populate);

				// You can use the IsPopulated property to determine whether a particular has its
				// information present, or if only the identification is present.
				Console.WriteLine("Is the item populated? " + item.IsPopulated);
				
				var unpopulatedVariableScheme = client.GetItem(
					response.Results[0].CompositeId, ChildReferenceProcessing.Instantiate)
					as VariableScheme;

				Console.WriteLine("Is the first Variable populated? " + unpopulatedVariableScheme.Variables[0].IsPopulated);

				// If the child Variables are not populated, we can manually request them to be 
				// populated using the PopulateItem method.
				foreach (var variable in unpopulatedVariableScheme.Variables)
				{
					// Check to make sure this variable isn't already populated. We don't want to 
					// populate an item more than once.
					if (!variable.IsPopulated)
					{
						client.PopulateItem(variable, false, ChildReferenceProcessing.Populate);
					}
				}

				// As another alternative, you can fully populate all an item's children
				// (and their children) by using the GraphPopulator. This object will 
				// visit every child item and call PopulateItem for that item.
				GraphPopulator populator = new GraphPopulator(client);
				unpopulatedVariableScheme.Accept(populator);
			}

		}

		/// <summary>
		/// Search for items within a set.
		/// </summary>
		/// <remarks>
		/// In Colectica, a set of items can be determined by a single identifier.
		/// For example, the identifier of a VariableScheme would represent the set of 
		/// the VariableScheme, all Variables within the scheme, all Concepts referenced 
		/// by any Variables in the VariableScheme, along with all other items referenced by 
		/// any of these items.
		/// 
		/// Working with sets is a very efficient way to search for and manipulate 
		/// large amounts of metadata.
		/// </remarks>
		public void SetSearch()
		{
			var client = GetClient();

			// First, get a VariableScheme using its identification information.
			var variableScheme = client.GetLatestItem(new Guid("99049803-52f6-4364-ae9e-a8ae0259b8e3"), "example.org");

			// The GetTypedRelationships method returns a matrix representing every relationship
			// in the set defined by the identifier pass in.
			TypedAdjacencyMatrix matrix = client.GetTypedRelationships(variableScheme.CompositeId);

			// Get a list of all items within the set. This is a local operation on the matrix,
			// so it is very fast. It does not require a roundtrip to the Repository.
			HashSet<TypedIdTriple> fullSet = matrix.GetSet(variableScheme.CompositeId);

			// Get the identifiers of only the Variables in the set.
			var varSet = fullSet.Where(id => id.ItemType == DdiItemType.Variable);

			// Get descriptive information for all the variables.
			var descriptions = client.GetRepositoryItemDescriptions(varSet.ToIdentifierCollection());

			// Here is a different way of retrieving the same information.
			//SetSearchFacet facet = new SetSearchFacet();
			//facet.ItemTypes.Add(DdiItemType.Variable);
			//var results = client.SearchTypedSet(vs.CompositeId, facet);
			//var descriptions = client.GetRepositoryItemDescriptions(results.ToIdentifierCollection());
		}
	}
}
