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
# Create the DDI items. Defining DDI xml is provided automatically when using the Colectica SDK
#

seriesId = '3d62a996-fbd2-4b4b-8d6d-c6e1b225db55'
seriesVersion = 1
studyId = '2cda67eb-3058-4127-82b0-79d218eae429'
studyVersion = 1

seriesDdi = """<Fragment xmlns:r="ddi:reusable:3_3" xmlns="ddi:instance:3_3">
  <Group isUniversallyUnique="true" versionDate="2020-07-21T21:08:38.6591606Z" xmlns="ddi:group:3_3">
    <r:URN>urn:ddi:int.example:""" + seriesId + ':' + str(seriesVersion) +  """</r:URN>
    <r:Agency>int.example</r:Agency>
    <r:ID>""" + seriesId  +  """</r:ID>
    <r:Version>"""  + str(seriesVersion) +  """</r:Version>
    <r:Citation>
      <r:Title>
        <r:String xml:lang="en-US">Repository Python Example Series</r:String>
        <r:String xml:lang="fr-CA">Série d'exemples Python</r:String>
      </r:Title>
    </r:Citation>
    <r:StudyUnitReference>
      <r:Agency>int.example</r:Agency>
      <r:ID>""" + studyId  +  """</r:ID>
      <r:Version>""" + str(studyVersion)  +  """</r:Version>
      <r:TypeOfObject>StudyUnit</r:TypeOfObject>
    </r:StudyUnitReference>
  </Group>
</Fragment>"""

studyDdi = """<Fragment xmlns:r="ddi:reusable:3_3" xmlns="ddi:instance:3_3">
  <StudyUnit isUniversallyUnique="true" versionDate="2020-07-21T21:09:01.1222125Z" xmlns="ddi:studyunit:3_3">
    <r:URN>urn:ddi:int.example:""" + studyId + ':' + str(studyVersion) +  """</r:URN>
    <r:Agency>int.example</r:Agency>
    <r:ID>""" + studyId +  """</r:ID>
    <r:Version>""" + str(studyVersion) +  """</r:Version>
    <r:Citation>
      <r:Title>
        <r:String xml:lang="en-US">Repository Python Example Study</r:String>
        <r:String xml:lang="fr-CA">Étude d'exemple Python</r:String>
      </r:Title>
    </r:Citation>
  </StudyUnit>
</Fragment>"""

#   See Item Type Identifiers for a list of identifiers for all item types.
#   https://docs.colectica.com/repository/technical/item-type-identifiers/
seriesItemType = '4bd6eef6-99df-40e6-9b11-5b8f64e5cb23'
studyItemType = '30ea0200-7121-4f01-8d21-a931a182b86d'
ddi32Type = 'C0CA1BD4-1839-4233-A5B5-906DA0302B89'
ddi33Type='DC337820-AF3A-4C0B-82F9-CF02535CDE83'


#
# We will use the transaction API to automatically version the items.
#
# Alternatively, we can use the register items call directly and handle the versioning manually
#
transaction = service.CreateTransaction()
print('Transaction created. ID:' + str(transaction.TransactionId))
print(transaction)


addItemRequest = {
    'TransactionId' : transaction.TransactionId,
    'Items' : {
        'RepositoryItem' : [
            {
                'ItemType' : seriesItemType,
                'AgencyId' : 'int.example',
                'Version' : seriesVersion, 
                'Identifier': seriesId,
                'Item' : seriesDdi,
                'Notes' : {},
                'VersionDate':'2020-07-21T21:08:38.6591606Z',
                'VersionResponsibility':username,
                'VersionRationale':{},
                'IsPublished':False,
                'IsDeprecated':False,
                'IsProvisional':False,
                'ItemFormat':ddi33Type

            },
            {
                'ItemType' : studyItemType,
                'AgencyId' : 'int.example',
                'Version' : studyVersion, 
                'Identifier': studyId,
                'Item' : studyDdi,
                'Notes' : {},
                'VersionDate':'2020-07-21T21:09:01.1222125Z',
                'VersionResponsibility':username,
                'VersionRationale':{},
                'IsPublished':False,
                'IsDeprecated':False,
                'IsProvisional':False,
                'ItemFormat':ddi33Type

            }

        ]
    }
}


transaction = service.AddItemsToTransaction(addItemRequest)

# register all items added to the repository transaction
commitTransactionRequest = {
    'TransactionType' : 'CommitAsLatestWithLatestChildrenAndPropagateVersions',
    'ScopedSets':{},
    'TransactionId':transaction.TransactionId,
    'VersionRationale':{
        'KeyValueOfstringstring': [
            {'Key': 'en-US', 'Value': 'Commit the series and Study'},
            {'Key': 'fr-CA', 'Value': 'Engagez la série et étudiez'}
        ]
    }
}

transaction = service.CommitTransaction(commitTransactionRequest)

#
# get the version history of the series
history = service.GetVersionHistory(seriesId,'int.example')

print('Series version history')
print(history)

