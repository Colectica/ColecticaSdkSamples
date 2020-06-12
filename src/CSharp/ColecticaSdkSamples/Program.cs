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
using ColecticaSdkSamples.SampleTasks;

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

            // Initialize logging.
            Logger.Instance.InitializeInAppData("ColecticaSamples-.txt");

            // Run sample tasks.
            DisseminationTasks.MainAsync().GetAwaiter().GetResult();

            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
        }

    }


}
