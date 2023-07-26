# API Documentation

## Models


```C#
enum ResourceType{
  Coins = 0,
  Rolls = 1
}
```

## User Authentication Controller

# Login
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
