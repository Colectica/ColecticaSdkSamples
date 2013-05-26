using Algenta.Colectica.Model.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ColecticaSdkMvc.Models
{
	public class HomeModel
	{
		List<SearchResult> results = new List<SearchResult>();
		public List<SearchResult> Results
		{
			get { return this.results; }
		}
	}
}