#
# Example Colectica Repository SOAP api access using local username and password
#

import zeep
from zeep.transports import Transport
from requests import Session
#from requests_ntlm import HttpNtlmAuth
from zeep.wsse.username import UsernameToken

hostnameAndPort = "statcan.eval.colectica.org:19994"
username = "user@example.com"
password = "userExamplePassword"

#
# First obtain a SOAP client
#

endpoint = "https://" +hostnameAndPort + "/RepositoryService.svc/basic"
wsdl = "https://" + hostnameAndPort + "/RepositoryService.svc?singleWsdl"

# Self signed certificate? You may need to disable verification
session = Session()
session.verify = False
transport = Transport(session=session)

client = zeep.Client(wsdl=wsdl, wsse=UsernameToken(username, password), transport=transport)
# Ensure the correct hostname is used, if it is different than in the wsdl
service = client.create_service('{http://ns.colectica.com/2009/07/}BasicHttpBinding_IRepositoryService', endpoint)


#
# Example calls
#

#
# Get general Repository information
response = service.GetRepositoryInfo()
print(response)

#
# perform a general search
#   The identifier 4bd6eef6-99df-40e6-9b11-5b8f64e5cb23 is the item type identifier for the Series item type.
#   See Item Type Identifiers for a list of identifiers for all item types.
#   https://docs.colectica.com/repository/technical/item-type-identifiers/

facet = {
    'Cultures' : {
        'string': ['en-US']
    }, 
    'ItemTypes' : {
        'guid': ['4bd6eef6-99df-40e6-9b11-5b8f64e5cb23']
    }, 
    'LanguageSortOrder' : {
        'string':['en-US']
    },
    'MaxResults' : 0,
    'RankResults' : True,
    'ResultOffset' : 0,
    'ResultOrdering' : 'MetadataRank',
    'SearchDepricatedItems' : False,    
    'SearchLatestVersion' : True,
    'SearchSets': {},
    'SearchTargets': {},
    'SearchTerms' : {
        'string':['Common']
    }
}

response = service.RepositorySearch(facet)
print(response)


#
# Get a list of versions of a repository item
versions = service.GetVersions('16aa1256-b872-4616-bd6c-8cafd9174c5e','int.example')
print(versions)


#
# get the version history of a repository item
history = service.GetVersionHistory('16aa1256-b872-4616-bd6c-8cafd9174c5e','int.example')
print(history)