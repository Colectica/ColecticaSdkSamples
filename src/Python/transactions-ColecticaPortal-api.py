#
# Example Colectica Portal api access using local username and password
# and built in JWT endpoint
#
# (OpenID Connect and Windows Auth are also available as a alternative setups)
#
import requests
import json

hostname = "statcan.eval.colectica.org"
username = "user@example.com"
password = "userExamplePassword"

#
# First obtain a JWT access token
# documented at https://docs.colectica.com/portal/technical/deployment/local-jwt-provider/#usage
#

tokenEndpoint = "https://" + hostname + "/token/createtoken"
response = requests.post(tokenEndpoint, json={'username': username, 'password': password})

if response.ok is not True:
    print("Could not get token. Status code: ", response.status_code)
    quit()

jsonResponse = response.json()
jwtToken = jsonResponse["access_token"]
tokenHeader = {'Authorization': 'Bearer ' + jwtToken}

# In the example, you can see the token returned
#print("jwtToken is: ", jwtToken)




#
# Create the DDI items. Defining DDI xml is provided automatically when using the Colectica SDK
#

seriesId = 'c81e9f52-cd81-41ff-8f54-2101835d62ea'
seriesVersion = 1
studyId = 'c5e46bf5-eec3-4db3-860e-2a14759c5c1e'
studyVersion = 1

seriesDdi = """<Fragment xmlns:r="ddi:reusable:3_3" xmlns="ddi:instance:3_3">
  <Group isUniversallyUnique="true" versionDate="2020-07-21T21:08:38.6591606Z" xmlns="ddi:group:3_3">
    <r:URN>urn:ddi:int.example:""" + seriesId + ':' + str(seriesVersion) +  """</r:URN>
    <r:Agency>int.example</r:Agency>
    <r:ID>""" + seriesId  +  """</r:ID>
    <r:Version>"""  + str(seriesVersion) +  """</r:Version>
    <r:Citation>
      <r:Title>
        <r:String xml:lang="en-US">Portal transaction API Python Example Series</r:String>
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
        <r:String xml:lang="en-US">Portal transaction Api Python Example Study</r:String>
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
# Example Portal transaction api calls
#

# Create a new transaction

transaction = requests.post("https://"+hostname+"/api/v1/transaction", headers=tokenHeader)
transactionJson = transaction.json()
transactionId = transactionJson["TransactionId"]


# Add changed items to the transaction. See transaction types at https://docs.colectica.com/changelog/6.0/#id3

jsonAddItemsRequest =  {
     "TransactionId": transactionId,
     "Items": [
        {
            "ItemType" : seriesItemType,
            "AgencyId" : "int.example",
            "Version" : seriesVersion, 
            "Identifier": seriesId,
            "Item" : seriesDdi,
            "VersionDate":"2020-07-21T21:08:38.6591606Z",
            "VersionResponsibility":username,
            "IsPublished":False,
            "IsDeprecated":False,
            "IsProvisional":False,
            "ItemFormat":ddi33Type
        },
        {
            "ItemType" : studyItemType,
            "AgencyId" : "int.example",
            "Version" : studyVersion, 
            "Identifier": studyId,
            "Item" : studyDdi,
            "VersionDate":"2020-07-21T21:09:01.1222125Z",
            "VersionResponsibility":username,
            "IsPublished":False,
            "IsDeprecated":False,
            "IsProvisional":False,
            "ItemFormat":ddi33Type

        }
     ]
}


response = requests.post("https://"+hostname+"/api/v1/transaction/_addItemsToTransaction", headers=tokenHeader, json=jsonAddItemsRequest)

if response.ok:
    print(response.json())

# commit the transaction

jsonCommitRequest = {
  "TransactionType": "CommitAsLatestWithLatestChildrenAndPropagateVersions",
  "ScopedSets": [ ],
  "TransactionId": transactionId,
  "VersionRationale": {
            "en-US": "Commit the series and Study",
            "fr-CA": "Engagez la série et étudiez"
         },
  "SetName": "",
  "NamedOptions": []
}

response = requests.post("https://"+hostname+"/api/v1/transaction/_commitTransaction", headers=tokenHeader, json=jsonCommitRequest)

if response.ok:
    print(response.json())