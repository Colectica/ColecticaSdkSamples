using Algenta.Colectica.Model.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Algenta.Colectica.Model;
using Algenta.Colectica.Model.Ddi;

namespace ColecticaSdkSamples.Basic
{
    public class ConsoleWriterVisitor : VersionableVisitorBase
    {
        public override void BeginVisitItem(IVersionable item)
        {
            base.BeginVisitItem(item);

            var versionable = item as VersionableBase;
            if (versionable != null)
            {
                Console.WriteLine("Beginning visit to " + DdiItemType.GetLabelForItemType(item.ItemType) + " " + versionable.DisplayLabel);
            }
        }

        public override void EndVisitItem(IVersionable item)
        {
            base.EndVisitItem(item);

            var versionable = item as VersionableBase;
            if (versionable != null)
            {
                Console.WriteLine("Ending visit to " + DdiItemType.GetLabelForItemType(item.ItemType) + " " + versionable.DisplayLabel);
            }
        }
    }
}
