# Transaction API Examples

The transaction API allows you to register items with the repository.
When using the transaction API, the repository is responsible for version propagation.


### Create a transaction

### Request

// Creating a transaction is a simple POST. No body is required.

```http
POST {{host}}/api/v1/transaction
Authorization: Bearer {{token}}
Content-Type: application/json
Accept: application/json
```

#### Response

```json
{
  "TransactionId": 173,
  "PersistentId": "cb1f066d-5969-4c6f-978c-0b001991930d",
  "Created": "2023-11-09T17:32:43.8539273Z",
  "Modified": "2023-11-09T17:32:43.8539274Z",
  "Status": 0,
  "TransactionType": 0,
  "Username": "jeremy@colectica.com",
  "ItemCount": 0,
  "PropagatedItemCount": 0,
  "Data": null
}
```


### Add a Concept to the transaction

#### Request

```http
POST {{host}}/api/v1/transaction/_addItemsToTransaction
Authorization: Bearer {{token}}
Content-Type: application/json
Accept: application/json

// For item type and format identifiers, see https://docs.colectica.com/repository/technical/item-type-identifiers/
{
    "transactionId": {{transactionId}},
    "items": [
        {
            "itemType": "48b7d4b4-72bf-470a-a885-720f89bfbc40",
            "agencyId": "int.example",
            "version": 0,
            "identifier": "{{conceptId}}",
            "item": '<Fragment xmlns:r="ddi:reusable:3_3" xmlns="ddi:instance:3_3"> <Concept isUniversallyUnique="true" versionDate="2023-10-03T15:26:30.6816076Z" isCharacteristic="false" xmlns="ddi:conceptualcomponent:3_3"> <r:URN>urn:ddi:int.example:{{conceptId}}:1</r:URN> <r:Agency>int.example</r:Agency> <r:ID>{{conceptId}}</r:ID> <r:Version>1</r:Version> <r:VersionResponsibility>jeremy@colectica.com</r:VersionResponsibility> <r:VersionRationale> <r:RationaleDescription> <r:String xml:lang="en-US">first draft</r:String> </r:RationaleDescription> </r:VersionRationale> <r:Label> <r:Content xml:lang="en-US">Demographics</r:Content> </r:Label> </Concept> </Fragment>',
            "versionDate": "2023-11-09T17:12:59.165Z",
            "versionResponsibility": "{{user}}",
            "itemFormat": "DC337820-AF3A-4C0B-82F9-CF02535CDE83",
            "transactionId": {{transactionId}},
        }

    ]
}
```

#### Response

```json
{
  "TransactionId": 172,
  "PersistentId": "f1f3f15c-cee3-43ad-b11b-ce092d5a2131",
  "Created": "2023-11-09T17:28:53.771013",
  "Modified": "2023-11-09T17:33:12.8412867Z",
  "Status": 1,
  "TransactionType": 1,
  "Username": "jeremy@colectica.com",
  "ItemCount": 1,
  "PropagatedItemCount": 0,
  "Data": null
}
```

### Commit the transaction

#### Request

```http
POST {{host}}/api/v1/transaction/_commitTransaction
Authorization: Bearer {{token}}
Content-Type: application/json
Accept: application/json

{
  "versionRationale": {
    "en": "Here is a log message."
  },
  "transactionType": 1,
  "transactionId": {{transactionId}}
}

// Options for transactionType are:
//
// 1 - Copy commit
// Register the items in the transaction without any updates or propagation
 
// 2 - Commit as latest with latest children and propagate versions
// Register the items as the latest versions in the Repository, updating the item version if needed.
// Update child references to the latest version of the child in the Repository or being committed.
// Update latest parent items to point to the latest items committed, or the latest updated parents (propagate versions).
// Propagation is performed within the sets specified, or across the whole repository if no set is specified.

// 3 - Commit as latest and prpagate versions
// Register the items as the latest versions in the Repository, updating the item version if needed.
// Update latest parent items to point to the latest items committed, or the latest updated parents (propagate versions).
// Propagation is performed within the sets specified, or across the whole repository if no set is specified.

// 4 - Commit as latest with latest children
// Register the items submitted as the latest versions, adjust version numbers, update references to child submitted items within submitted items.
// Update child references to the latest version of the child in the Repository or being committed.

// 5 - Commit as latest
// Register the items submitted as the latest versions, adjust version numbers, update references to child submitted items within submitted items.

// 6 - Update repository and set latest children on latest items and propagate versions
// This type of transaction is run without any user submitted items
// Each latest item in the Repository, or in the specified set, is checked for children that are not latest
// Older child references are replaced with latest child references
// Propagation of updated items is done in the same manner as CommitAsLatestWithLatestChildrenAndPropagateVersions
// Scoped Sets are currently not supported with this repository update type
```


#### Response

```json
{
  "UserCommitted": [],
  "SystemCommitted": [],
  "NotProcessed": [],
  "Transaction": {
    "TransactionId": 172,
    "PersistentId": "f1f3f15c-cee3-43ad-b11b-ce092d5a2131",
    "Created": "2023-11-09T17:28:53.771013",
    "Modified": "2023-11-09T17:28:57.7879595Z",
    "Status": 1,
    "TransactionType": 1,
    "Username": "jeremy@colectica.com",
    "ItemCount": 0,
    "PropagatedItemCount": 0,
    "Data": null
  }
}
```