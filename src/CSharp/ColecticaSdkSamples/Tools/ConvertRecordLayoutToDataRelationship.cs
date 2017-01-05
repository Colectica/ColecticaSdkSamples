using Algenta.Colectica.Model.Ddi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColecticaSdkSamples.Tools
{
    /// <summary>
    /// Upgrades a PhysicalInstance from the DDI 3.1 structure to the DDI 3.2 structure.
    /// </summary>
    public class ConvertRecordLayoutToDataRelationship
    {
        public void Convert(PhysicalInstance pi)
        {
            if (pi == null)
            {
                throw new ArgumentNullException(nameof(pi));
            }

            if (pi.RecordLayouts.Count == 0)
            {
                return;
            }

            // Create a single DataRelationship for the PhysicalInstance.
            // Within this, we will add one LogicalRecord per old RecordLayout.
            var dataRelationship = new DataRelationship() { AgencyId = pi.AgencyId };
            pi.DataRelationships.Add(dataRelationship);

            // For each record layout, add the variables to a new DataRelationship.
            foreach (var recordLayout in pi.RecordLayouts)
            {
                // Create a LogicalRecord with the same descriptive information 
                // as the RecordLayout we are migrating.
                var logicalRecord = new LogicalRecord { AgencyId = pi.AgencyId };
                dataRelationship.LogicalRecords.Add(logicalRecord);

                logicalRecord.ItemName.Copy(recordLayout.ItemName);
                logicalRecord.Label.Copy(recordLayout.Label);
                logicalRecord.Description.Copy(recordLayout.Description);

                // For each DataItem in the RecordLayout, add a VariablesInRecord entry
                // to the LogicalRecord.
                foreach (var dataItem in recordLayout.DataItems)
                {
                    logicalRecord.VariablesInRecord.Add(dataItem.Variable);
                }
            }

            // Remove the old RecordLayouts.
            pi.RecordLayouts.Clear();
        }
    }
}
