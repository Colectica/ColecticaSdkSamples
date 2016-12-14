using Algenta.Colectica.Model.Ddi;
using Algenta.Colectica.Model.Ddi.Serialization;
using Algenta.Colectica.Model.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColecticaSdkSamples.Basic
{
    /// <summary>
    /// Contains methods for working with QualityStatements.
    /// </summary>
    public class Quality
    {
        /// <summary>
        /// Create a QualityStatement from scratch, and add it to a Repository.
        /// </summary>
        public QualityStatement CreateAndRegisterQualityStatement()
        {
            // Create the QualityStatement object and give it a label.
            QualityStatement statement = new QualityStatement();
            statement.Label.Current = "Sample Quality Statement";

            // A QualityStatement is made up of QualityStatementItems.
            // QualityStatementItems have two important pieces of information:
            // a defining Concept, and some content.
            // The defining Concept specifies the type of information recorded
            // by the item. For example, "Contact organization".
            // The content is the actual information, like "Statistics Denmark".

            // First, let's create the Concepts.
            Concept concept1 = new Concept();
            concept1.Label.Current = "Contact organization";

            Concept concept2 = new Concept();
            concept2.Label.Current = "Statistical Unit";

            Concept concept3 = new Concept();
            concept3.Label.Current = "Statitistical Population";

            // Now let's create the QualityStatementItems, and assign the appropriate
            // Concepts and some information.
            QualityStatementItem item1 = new QualityStatementItem();
            item1.ComplianceConcept = concept1;
            item1.ComplianceDescription.Current = "Statistics Denmark";

            QualityStatementItem item2 = new QualityStatementItem();
            item2.ComplianceConcept = concept2;
            item2.ComplianceDescription.Current = "Person";

            QualityStatementItem item3 = new QualityStatementItem();
            item3.ComplianceConcept = concept3;
            item3.ComplianceDescription.Current = "Denmark";

            // Add each of the items to the QualityStatement.
            statement.Items.Add(item1);
            statement.Items.Add(item2);
            statement.Items.Add(item3);

            // Add the QualityStatement and the Concepts to the Repository.
            var client = RepositoryIntro.GetClient();
            CommitOptions options = new CommitOptions();
            client.RegisterItem(statement, options);
            client.RegisterItem(concept1, options);
            client.RegisterItem(concept2, options);
            client.RegisterItem(concept3, options);


            // Also, write an XML file just so we can see how things look.
            string fileName = @"statement.xml";
            DDIWorkflowSerializer serializer = new DDIWorkflowSerializer();
            serializer.SerializeFragments(fileName, statement);

            return statement;
        }

        /// <summary>
        /// Assigns a QualityStatement to a StudyUnit.
        /// </summary>
        public void AssignQualityStatementToStudy()
        {
            // Create a new StudyUnit with some basic information.
            StudyUnit study = new StudyUnit();
            study.DublinCoreMetadata.Title.Current = "Sample Study";

            // Grab the QualityStatement.
            var statement = CreateAndRegisterQualityStatement();

            // Assign the QualityStatement to the StudyUnit.
            study.QualityStatements.Add(statement);

            // Register the StudyUnit with the Repository.
            var client = RepositoryIntro.GetClient();
            client.RegisterItem(study, new CommitOptions());
        }

    }
}
