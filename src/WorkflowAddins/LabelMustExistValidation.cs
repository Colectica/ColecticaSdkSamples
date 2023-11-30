using System.ComponentModel.Composition;
using System.Numerics;
using Algenta.Colectica.Model;
using Algenta.Colectica.Model.Ddi;
using Algenta.Colectica.Model.Utility;
using Algenta.Colectica.Model.Workflow;

namespace WorkflowAddins;

/// <summary>
/// Validates that the all describable items have a label.
/// </summary>
[Export(typeof(IWorkflowValidationRule))]
public class LabelMustExistValidation : IWorkflowValidationRule
{
    public MultilingualString Name
    {
        get => new MultilingualString("Describable items must have a label");
    }

    public bool SupportsItem(IVersionable item, string targetStateToken) => item is IDescribable;

    public WorkflowValidationResult Validate(IVersionable item, string targetStateToken)
    {
        // If the item is not describable, then it is valid according to this rule.
        if (item is not IDescribable describable)
        {
            return new WorkflowValidationResult
            {
                IsValid = true
            };
        }

        // If there is no label, fail the validation.
        bool hasLabel = describable.Label.Any();
        string message = hasLabel ? "This is a successful validation rule." : "This is a failed validation rule.";

        return new WorkflowValidationResult
        {
            IsValid = hasLabel,
            Message = new MultilingualString(message, "en")
        };
    }
}