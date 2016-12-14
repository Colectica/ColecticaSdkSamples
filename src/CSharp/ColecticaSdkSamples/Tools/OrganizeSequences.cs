using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Algenta.Colectica.Model.Utility;
using Algenta.Colectica.Model;
using ColecticaSdkSamples.Basic;
using Algenta.Colectica.Model.Ddi;
using Algenta.Colectica.Model.Ddi.Utility;
using Algenta.Colectica.Model.Repository;
using Algenta.Colectica.Model.Ddi.Serialization;

namespace ColecticaSdkSamples.Tools
{
	public class OrganizeSequences
	{
		public void RewriteCCS()
		{
			MultilingualString.CurrentCulture = "en-GB";
			VersionableBase.DefaultAgencyId = "example.org";

			var client = RepositoryIntro.GetClient();

			var instance = client.GetItem(new Guid("b9ee3aa5-5bc5-43ed-a24e-f560abb30801"), "example.org", 2)
				as DdiInstance;

			//DDIWorkflowDeserializer deserializer = new DDIWorkflowDeserializer();
			//var instance = deserializer.LoadDdiFile(@"D:\Downloads\filesforMinneapolis\filesforMinneapolis\bcs08v08.xml");

			GraphPopulator populator = new GraphPopulator(client);
			populator.ChildProcessing = ChildReferenceProcessing.Populate;
			instance.Accept(populator);

			var resourcePackage = instance.ResourcePackages[0];

			var instrument = instance.ResourcePackages[0].DataCollections[0].Instruments[0];

			//var topLevelSequence = client.GetLatestItem(new Guid("ceaa9acf-2b2f-4c41-b298-b9f419412586"), "cls")
			//    as CustomSequenceActivity;

			var topLevelSequence = instrument.Sequence;

			var moduleSequences = topLevelSequence.Activities;

			foreach (CustomSequenceActivity module in moduleSequences.OfType<CustomSequenceActivity>())
			{
				DirtyItemGatherer gatherer = new DirtyItemGatherer(true);
				module.Accept(gatherer);

				var allChildren = gatherer.DirtyItems;

				ControlConstructScheme ccs = new ControlConstructScheme();
				ccs.ItemName.Copy(module.ItemName);

				foreach (var child in allChildren.OfType<ActivityBase>())
				{
					ccs.ControlConstructs.Add(child);
				}

				//client.RegisterItem(ccs, new CommitOptions());

				resourcePackage.ControlConstructSchemes.Add(ccs);
			}

			DDIWorkflowSerializer serializer = new DDIWorkflowSerializer();
			serializer.UseConciseBoundedDescription = false;
			var doc = serializer.Serialize(instance);
			doc.Save(@"d:\ColecticaOutput\bcsv8.xml");
		}
	}
}
