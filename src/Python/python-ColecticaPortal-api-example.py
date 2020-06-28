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
# Example Portal api calls
#

# get Repository information
response = requests.get("https://"+hostname+"/api/v1/repository/info", headers=tokenHeader)

if response.ok:
    print(response.json())


# perform a general search
#   The identifier 4bd6eef6-99df-40e6-9b11-5b8f64e5cb23 is the item type identifier for the Series item type.
#   See Item Type Identifiers for a list of identifiers for all item types.
#   https://docs.colectica.com/repository/technical/item-type-identifiers/

jsonquery =  {
     "Cultures": [
         "en-US"
     ],
     "ItemTypes": [
         "4bd6eef6-99df-40e6-9b11-5b8f64e5cb23"
     ],
     "LanguageSortOrder": [
         "en-US"
     ],
     "MaxResults": 0,
     "RankResults": True,
     "ResultOffset": 0,
     "ResultOrdering": "None",
     "SearchDepricatedItems": False,
     "SearchTerms": [
         "Common"
     ],
     "SearchLatestVersion": True
 }
response = requests.post("https://"+hostname+"/api/v1/_query", headers=tokenHeader, json=jsonquery)
if response.ok:
    print(response.json())