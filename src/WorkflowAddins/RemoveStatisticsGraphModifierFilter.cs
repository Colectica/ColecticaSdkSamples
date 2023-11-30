using System.ComponentModel.Composition;
using Algenta.Colectica.Model;
using Algenta.Colectica.Model.Workflow;
using Algenta.Colectica.Model.Utility;
using Algenta.Colectica.Model.Ddi;

namespace WorkflowAddins;

/// <summary>
/// Removes all VariableStatistics items from the graph.
/// </summary>
[Export(typeof(IGraphModifier))]
public class RemoveStatisticsGraphModifierFilter : IGraphModifier
{
    public string Name => "ControlConstruct Graph Filter";

    public string Description => "Remove variable statistics from PhysicalInstances.";

    public bool ModifyGraph(IVersionable item)
    {
        if (item is not PhysicalInstance pi)
        {
            return false;
        }

        if (pi.Statistics.Any())
        {
            pi.Statistics.Clear();
            return true;
        }

        return false;
    }
}