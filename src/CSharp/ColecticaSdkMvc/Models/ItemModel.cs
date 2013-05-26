using Algenta.Colectica.Model.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ColecticaSdkMvc.Models
{
	public class ItemModel
	{
		List<RepositoryItemMetadata> history = new List<RepositoryItemMetadata>();
		public List<RepositoryItemMetadata> History
		{
			get { return this.history; }
		}

		List<RepositoryItemMetadata> referencingItems = new List<RepositoryItemMetadata>();
		public List<RepositoryItemMetadata> ReferencingItems
		{
			get { return this.referencingItems; }
		}
	}
}