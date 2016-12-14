using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Algenta.Colectica.Model.Ddi;
using Algenta.Colectica.Model.Utility;
using Algenta.Colectica.Model.Ddi.Serialization;
using Algenta.Colectica.Model;
using Algenta.Colectica.Repository.Client;
using Algenta.Colectica.Model.Repository;
using Algenta.Colectica.Model.Ddi.Utility;
using System.IO;
using ColecticaSdkSamples.Basic;

namespace ConsoleApplication2
{
	class Program
	{
		static void Main(string[] args)
		{
			// It is helpful to set some default properties before
			// working with the SDK's model. These two properties
			// determine the default language and agency identifier
			// for every item.
			MultilingualString.CurrentCulture = "en-US";
			VersionableBase.DefaultAgencyId = "example.org";

			// Call some sample method from the Intro class.
			Intro intro = new Intro();
			intro.BuildSomeDdiAndWriteToXml();
			intro.LoadDdiAndCountSomeElements();
			intro.QueryObjectModel();

            // Visit the graph that we made.
            var visitor = new ConsoleWriterVisitor();
            intro.Instance.Accept(visitor);

			// Also, look around the RepositoryIntro class and the various
			// classes in the Tools namespace.

			Console.WriteLine("Press any key to exit.");
			Console.ReadLine();
		}

	}


}
