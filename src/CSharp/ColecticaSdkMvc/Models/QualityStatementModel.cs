using Algenta.Colectica.Model.Ddi;
using Algenta.Colectica.Model.Ddi.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace ColecticaSdkMvc.Models
{
    public class QualityStatementModel : ItemModel
    {
        public QualityStatement QualityStatement { get; protected set; }

        private ObservableCollection<QualityStatementNode> RootNodesField = new ObservableCollection<QualityStatementNode>();
        public ObservableCollection<QualityStatementNode> RootNodes
        {
            get { return this.RootNodesField; }
        }

        public QualityStatementModel(QualityStatement qualityStatement)
        {
            this.QualityStatement = qualityStatement;

            // Create a hierarchy of QualityStatement items, mirroring the 
            // corresponding Concept hierarchy.
            var rootItems = QualityStatementNode.GetHierarchy(QualityStatement);
            foreach (var item in rootItems)
            {
                this.RootNodes.Add(item);
            }
        }
    }

}