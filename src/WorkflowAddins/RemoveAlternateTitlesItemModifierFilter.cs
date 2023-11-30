using System;
using System.Linq;
using System.ComponentModel.Composition;
using Algenta.Colectica.Model;
using Algenta.Colectica.Model.Workflow;
using Algenta.Colectica.Model.Ddi;

namespace WorkflowAddins;

/// <summary>
/// Removes all AlternateTitles from items that have Dublin Core information.
/// </summary>
[Export(typeof(IItemModifier))]
public class RemoveAlternateTitlesItemModifierFilter : IItemModifier
{
    public string Name
    {
        get { return "Alternate Title Filter"; }
    }

    public string Description
    {
        get { return "Remove alternate titles from Group and StudyUnit items"; }
    }

    public bool SupportsItem(Guid itemType) => itemType == DdiItemType.Group || itemType == DdiItemType.StudyUnit;

    public bool ModifyItem(IVersionable item)
    {
        if (item is not IDublinCoreDescribable dcItem)
        {
            // If the item is not modified, return false.
            return false;
        }

        if (dcItem.DublinCoreMetadata.AlternateTitle.Any())
        {
            dcItem.DublinCoreMetadata.AlternateTitle.Clear();

            // The item was modified, so return true.
            return true;
        }

        return false;
    }

}