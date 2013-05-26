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
				Url = "localhost", 
				AuthenticationMethod = RepositoryAuthenticationMethod.Windows,
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

			// Register the item.
			client.RegisterItem(variableScheme, new CommitOptions());
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

		public void BuildAndRegisterConcepts()
		{
			// Setting the default agency like this means
			// we don't need to manually set it for every
			// item we create.
			VersionableBase.DefaultAgencyId = "int.example";

			// Create a scheme to hold the concepts.
			ConceptScheme scheme = new ConceptScheme();
			scheme.Label.Current = "Transportation Modes";

			// Create 6 concepts, setting up a small hierarchy.
			Concept transportMode = new Concept();
			transportMode.Label.Current = "Transport Mode";

			Concept auto = new Concept();
			auto.SubclassOf.Add(transportMode);
			auto.Label.Current = "Auto";

			Concept car = new Concept();
			car.SubclassOf.Add(auto);
			car.Label.Current = "Car";

			Concept truck = new Concept();
			truck.SubclassOf.Add(auto);
			truck.Label.Current = "Truck";

			Concept bike = new Concept();
			bike.SubclassOf.Add(transportMode);
			bike.Label.Current = "Bike";

			Concept walk = new Concept();
			walk.SubclassOf.Add(transportMode);
			walk.Label.Current = "Walk";

			// Add the concpts to the scheme.
			scheme.Concepts.Add(transportMode);
			scheme.Concepts.Add(auto);
			scheme.Concepts.Add(car);
			scheme.Concepts.Add(truck);
			scheme.Concepts.Add(bike);
			scheme.Concepts.Add(walk);

			var client = GetClient();
			CommitOptions options = new CommitOptions();

			// Gather all the scheme and all the items in the scheme,
			// so we can register them with a single call to the repository.
			ItemGathererVisitor gatherer = new ItemGathererVisitor();
			scheme.Accept(gatherer);

			// Register the items with the repository.
			client.RegisterItems(gatherer.FoundItems, options);

			// Alternatively, we could register the items one at a time, like this.

			//client.RegisterItem(scheme, options);
			//client.RegisterItem(transportMode, options);
			//client.RegisterItem(auto, options);
			//client.RegisterItem(car, options);
			//client.RegisterItem(truck, options);
			//client.RegisterItem(bike, options);
			//client.RegisterItem(walk, options);
		}

		public void GetItemMakeChangeAndReregister()
		{
			VersionableBase.DefaultAgencyId = "int.example";

			var client = GetClient();

			DdiClient ddiClient = new DdiClient(client);

			// Get a ConceptScheme from the repository,
			// and populate the member concepts.
			// Note that you probably have to change the UUID here
			// to represent one that is in your repository.
			var scheme = ddiClient.GetConceptScheme(
				new Guid("0cc13be5-3c89-4d1a-927f-ac7634d0c05a"),
				"int.example", 1,
				ChildReferenceProcessing.Populate);

			// Just add a description to the ConceptScheme.
			scheme.Description.Current = "This is a description we are adding.";

			// Before we register the new version of the item, we have to 
			// update the version number.
			scheme.Version++;

			// Register the item with the repository.
			var options = new CommitOptions();
			client.RegisterItem(scheme, options);
		}

		public void GetItemChangeChildAndReregister()
		{
			VersionableBase.DefaultAgencyId = "int.example";

			var client = GetClient();

			// The DdiClient class wraps the normal repository client,
			// providing extra methods to return DDI item types directly.
			// This helps avoid requiring casting of basic items.
			DdiClient ddiClient = new DdiClient(client);

			// Get a ConceptScheme from the repository,
			// and populate the member concepts.
			// Note that you probably have to change the UUID here
			// to represent one that is in your repository.
			var scheme = ddiClient.GetConceptScheme(
				new Guid("0cc13be5-3c89-4d1a-927f-ac7634d0c05a"),
				"int.example", 2,
				ChildReferenceProcessing.Populate);

			if (scheme == null || scheme.Concepts.Count < 3)
			{
				Console.WriteLine("No scheme, or not enough items in the scheme.");
				return;
			}
			
			// Grab the second concept and add a description to it.
			var concept = scheme.Concepts[2];
			concept.Description.Current = "This is the fourth concept";

			// When we change a property on the Concept, the concept's
			// IsDirty property is automatically set to true, indicating
			// that it has unsaved changes.
			Console.WriteLine("IsDirty: ", concept.IsDirty);

			// The DirtyItemGatherer will walk a set of objects and create
			// a list of all objects that are dirty.
			DirtyItemGatherer gatherer = new DirtyItemGatherer(
				gatherEvenIfNotDirty: false,
				markParentsDirty: true);
			scheme.Accept(gatherer);

			var options = new CommitOptions();

			// For every changed item, increase the version and
			// register the new version with the repository.
			foreach (var item in gatherer.DirtyItems)
			{
				item.Version++;
				client.RegisterItem(item, options);
			}
		}
	}
}
