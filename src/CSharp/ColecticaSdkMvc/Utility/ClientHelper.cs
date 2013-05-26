using Algenta.Colectica.Model.Repository;
using Algenta.Colectica.Repository.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ColecticaSdkMvc.Utility
{
	public class ClientHelper
	{
		public static WcfRepositoryClient GetClient()
		{
			RepositoryConnectionInfo info = new RepositoryConnectionInfo()
			{
				// TODO Update the hostname as appropriate.
				Url = "localhost",
				AuthenticationMethod = RepositoryAuthenticationMethod.Windows,
				TransportMethod = RepositoryTransportMethod.NetTcp
			};

			var client = new WcfRepositoryClient(info);
			return client;
		}

	}
}