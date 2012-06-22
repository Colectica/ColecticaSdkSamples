using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Algenta.Colectica.Model;
using Algenta.Colectica.Model.Ddi;
using Algenta.Colectica.Model.Ddi.Serialization;
using Algenta.Colectica.Model.Utility;

namespace ColecticaSdkSamples.Basic
{
	/// <summary>
	/// Contains methods demonstrating how to use the Colectica SDK
	/// to perform some basic operations, like creating DDI items, 
	/// saving DDI files, and loading DDI files.
	/// </summary>
	public class Intro
	{
		/// <summary>
		/// This method builds up a DdiInstance and writes it to a 
		/// valid DDI 3.1. XML file.
		/// </summary>
		public void BuildSomeDdiAndWriteToXml()
		{
			// It is helpful to set some default properties before
			// working with the SDK's model. These two properties
			// determine the default language and agency identifier
			// for every item.
			MultilingualString.CurrentCulture = "en-US";
			VersionableBase.DefaultAgencyId = "example.org";

			// Start out by creating a new DDIInstance.
			// The DdiInstance can hold StudyUnits, Groups, and ResourcePackages.
			DdiInstance instance = new DdiInstance();
			instance.DublinCoreMetadata.Title.Current = "My First Instance";

			// Since we set the CurrentCulture to "en-US", that last line is 
			// equivalent to this next one.
			instance.DublinCoreMetadata.Title["en-US"] = "My First Instance";

			// We can set multiple languages, if we want to.
			instance.DublinCoreMetadata.Title["fr"] = "TODO";

			// Add a ResourcePackage to the DdiInstance. There are three things to do.
			// 1. First, create it. 
			// 2. Then, set whatever properties you like. Here, we just set the Title.
			// 3. Add the item to it's parent. In this case, that's the DdiInstance.
			ResourcePackage resourcePackage = new ResourcePackage();
			resourcePackage.DublinCoreMetadata.Title.Current = "RP1";
			instance.AddChild(resourcePackage);

			// Now let's add a ConceptScheme to the ResourcePackage. We'll do this 
			// using the same three steps we used to create the ResourcePackage.
			ConceptScheme conceptScheme = new ConceptScheme();
			conceptScheme.ItemName.Current = "My Concepts";
			conceptScheme.Description.Current = "Just some concepts for testing.";
			resourcePackage.AddChild(conceptScheme);

			// Let's add some Concepts to the ConceptScheme. 
			string[] conceptLabels = { "Pet", "Dog", "Cat", "Bird", "Fish", "Monkey" };
			foreach (string label in conceptLabels)
			{
				// Again, for each Concept we create, we want to perform the 
				// same three steps as above: 
				// 1. instantiate, 2. assign properties, 3. add to parent.
				Concept concept = new Concept();
				concept.Label.Current = label;
				conceptScheme.AddChild(concept);
			}

			// Let's create a collection of questions.
			QuestionScheme questionScheme = new QuestionScheme();
			questionScheme.ItemName.Current = "Sample Questions";
			resourcePackage.QuestionSchemes.Add(questionScheme);
			

			// First, we can ask for a name. This will just collect textual data.
			Question q1 = new Question();
			q1.QuestionText.Current = "What is your name?";
			q1.ResponseDomains.Add(new TextDomain());
			questionScheme.Questions.Add(q1);

			// Next, we can ask what method of transportation somebody used to get to Minneapolis.
			Question transportationQuestion = new Question();
			transportationQuestion.QuestionText.Current = "How did you get to Minneapolis?";

			// For this question, the respondent will choose from a list of answers.
			// Let's make that list.
			CategoryScheme catScheme = new CategoryScheme();
			resourcePackage.CategorySchemes.Add(catScheme);

			CodeScheme codeScheme = new CodeScheme();
			resourcePackage.CodeSchemes.Add(codeScheme);

			// Add the first category and code: Airplane
			Category airplaneCategory = new Category();
			airplaneCategory.Label.Current = "Airplane";
			Code airplaneCode = new Code
			{
				Value = "0",
				Category = airplaneCategory
			};
			catScheme.Categories.Add(airplaneCategory);
			codeScheme.Codes.Add(airplaneCode);

			// Car
			Category carCategory = new Category();
			carCategory.ItemName.Current = "Car";
			Code carCode = new Code
			{
				Value = "1",
				Category = carCategory
			};
			catScheme.Categories.Add(carCategory);
			codeScheme.Codes.Add(carCode);

			// Train
			Category trainCategory = new Category();
			trainCategory.ItemName.Current = "Train";
			Code trainCode = new Code
			{
				Value = "2",
				Category = trainCategory
			};
			catScheme.Categories.Add(trainCategory);
			codeScheme.Codes.Add(trainCode);
	
			// Now that we have a Category and CodeScheme, we can create
			// a CodeDomain and assign this as the type of data the transportation
			// question will collect.
			CodeDomain codeDomain = new CodeDomain();
			codeDomain.Codes = codeScheme;
			transportationQuestion.ResponseDomains.Add(codeDomain);
			questionScheme.Questions.Add(transportationQuestion);

			// We have created a DdiInstance, a ResourcePackage, some concepts,
			// and some questions.
			//
			// Now what?
			//
			// Let's save all this to a DDI 3.1 XML file.
			//
			// First, we can call EnsureCompliance to make sure
			// our objects have all fields that are required
			// by the DDI 3.1 schemas. If we missed anything, 
			// this method will fill in some defaults for us.
			DDIWorkflowSerializer.EnsureCompliance(instance);

			// Now, create the serializer object that will save our items to XML.
			// Setting UseConciseBoundedDescription to false makes sure we 
			// write every item, and not just references to items.
			DDIWorkflowSerializer serializer = new DDIWorkflowSerializer();
			serializer.UseConciseBoundedDescription = false;

			// Getting a valid XML representation of our model is just one method call.
			XmlDocument xmlDoc = serializer.Serialize(instance);

			// Finally, save the XML document to a file.
			xmlDoc.Save("sample.xml");
		}

		/// <summary>
		/// Loads a DDI file named sample.xml and outputs some item counts.
		/// </summary>
		public void LoadDdiAndCountSomeElements()
		{
			// Create the deserializer that will read the DDI 3.1 file and
			// return a populated DdiInstance object.
			DDIWorkflowDeserializer deserializer = new DDIWorkflowDeserializer();

			// Load the DdiInstance from the XML file.
			string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var instance = deserializer.LoadDdiFile(Path.Combine(directory, "sample.xml"));


			// Output some item counts.
			Console.WriteLine("ResourcePackages: " + instance.ResourcePackages.Count);

			if (instance.ResourcePackages.Count > 0)
			{
				Console.WriteLine("ConceptSchemes: " + instance.ResourcePackages[0].ConceptSchemes.Count);

				if (instance.ResourcePackages[0].ConceptSchemes.Count > 0)
				{
					Console.WriteLine("Concepts: " + instance.ResourcePackages[0].ConceptSchemes[0].Concepts.Count);
				}

				Console.WriteLine("QuestionSchemes: " + instance.ResourcePackages[0].QuestionSchemes.Count);
				Console.WriteLine("CategorySchemes: " + instance.ResourcePackages[0].CategorySchemes.Count);
				Console.WriteLine("CodeSchemes: " + instance.ResourcePackages[0].CodeSchemes.Count);
			}
		}

		public void QueryObjectModel()
		{
			// Load the sample data file.
			DDIWorkflowDeserializer deserializer = new DDIWorkflowDeserializer();
			string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var instance = deserializer.LoadDdiFile(Path.Combine(directory, "sample.xml"));

			// Use LINQ to find the first category with the label "Car".
			var carCat = (from rp in instance.ResourcePackages
						  from cs in rp.CategorySchemes
						  from cat in cs.Categories
						  where cat.ItemName.ContainsKey("en-US") && cat.ItemName["en-US"] == "Car"
						  select cat).FirstOrDefault();

			// Output some information about the category.
			if (carCat != null)
			{
				Console.WriteLine("Found the category. Its identifier is " + carCat.Identifier);
			}
			else
			{
				Console.WriteLine("Could not find a category labeled 'Car'");
			}
		}
	}
}
