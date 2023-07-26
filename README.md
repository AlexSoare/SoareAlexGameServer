# API Documentation

## Models


```C#
enum ResourceType{
  Coins = 0,
  Rolls = 1
}
```

## User Authentication Controller

### Login
- Endpoint: POST /api/UserAuthentification/Login
- Description: Logs in a user and generates an authentication token.
- Request Model:
```json
{
  "DeviceId": "string"
}
```
- Response Model:
```json
{
  "PlayerId": "string",
  "AlreadyOnline": "boolean",
  "AuthToken": "string",
  "Status": "integer"
}
```
- Status Codes:
  - 200 OK: Successful login.
  - 400 Bad Request: Invalid request or missing DeviceId.
  - 500 Internal Server Error: Unexpected error during login.

## Resources Controller

### Update Resources
- Endpoint: POST /api/Resources/UpdateResources
- Description: Updates the resources for the authenticated user by setting its value directly as the received value.
- Request Model:
```json
{
  "ResourceType": "ResourceType(number)",
  "ResourceValue": "number"
}
```
- Response Model:
```json
{
  "UpdatedResources": [
    {
      "ResourceType": "ResourceType",
      "Value": "number"
    }
  ],
  "Status": "integer"
}
```
- Status Codes:
  - 200 OK: Successful resource update.
  - 400 Bad Request: Invalid request or insufficient resources.
  - 500 Internal Server Error: Unexpected error during resource update.

### Get Resource
- Endpoint: GET /api/Resources/GetResource?ResourceType={resourceType}
- Description: Retrieves the value of a specific resource for the authenticated user.
- Response Model:
```json
{
  "ResourceValue": "number",
  "Status": "integer"
}
```

## Gifts Controller

### Send Gift
- Endpoint: POST /api/Gifts/SendGift
- Description: Sends a gift to a friend, updating resources for both the sender and the receiver.
- Request Model:
```json
{
  "FriendPlayerId": "string",
  "ResourceType": "ResourceType(number)",
  "ResourceValue": "number"
}
```
- Response Model:
```json
{
  "Status": "integer"
}
```
- Status Codes:
  - 200 OK: Successful gift sending.
  - 400 Bad Request: Invalid request, sending a gift to yourself, or insufficient resources.
  - 404 Not Found: Friend player not found.
  - 500 Internal Server Error: Unexpected error during gift sending.


Note: For authentication and authorization, the API requires an authentication token (Bearer JWT) in the request headers.

