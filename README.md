# City of Fort Wayne Greenways and Trails Department REST API (CRAPI) Overview

All calls will be sent to 
http://68.39.46.187:50000/GreenwayCap/DataRelay.svc/

## POST Requests

For POST, all params are sent as a JSON object in the body of the request. These headers should contain `Content-Type: application/json`.

## Auth Token

After **POST login**, the server will send back an auth token. Clients should send this token when making ANY future requests so the server knows with which user it’s dealing. The headers for all other requests should contain it like `Trails-Api-Key: ab83920bdc826bdaf`

## HTTP Status Codes

* HTTP 200: The request has completed successfully.
* HTTP 400: The request is invalid. The format of the sent data is invalid or doesn’t include all required fields.
* HTTP 401: The auth token was not included in the request headers. For POST login, The username and password are incorrect.
* HTTP 419: The auth token is no longer (or never was) valid and should be re-obtained before making any more requests.
* HTTP 500: The server is at fault.

## Date Format

All absolute dates are sent in [ISO 8601][1] format. This is a standard which has support from all of our platforms.

## Data Types

* `ExerciseType` can be `bike` or `run` or `walk`.
* `LineString` is a string where a point is float lat and float long, separated by a comma, and each point is separated with by a space. Example: `20.3323,70.4531 21.3323,71.4531 22.3323,72.4531`

# API SPEC v0.8

## POST /trails/api/1/Account/Create

This request creates a new account;

### Params

* username - string
* password - string
* dob - int, birth year, nullable
* weight - int, pounds, nullable
* sex - string, "male" or "female", nullable
* height - int, inches, nullable

ex. 
{
  "username":"ggrimm",
  "password":"wh0cares",
  "dob":1991,
  "weight":150,
  "sex":"male",
  "height":70
}

### Responses

* HTTP 200 - Account created successfully
* HTTP 401 - Username or password was rejected or already in use

## POST /trails/api/1/Login

Attempts login by authenticating username and password. For valid username/password combinations, the server returns an authorization token that is necessary for all subsequent API calls. The authorization token will be used both for authorization and for identification. See the overview section for how to include it.

### Params

* username - string
* password - string

### Responses

* HTTP 200 - Logged in successfully
  * authtoken - string, a long guid or hexidecimal string to identify this user’s requests
  ex. {
      "token": "20b4fa43-c158-465c-87fd-462c454b54c9"
    }
* HTTP 401 - Incorrect username/password

## GET /trails/api/1/Account

Requests the account information of the user. This request will fail if an auth token is not provided.

### Responses

* HTTP 200
  * dob - int, birth year, nullable
  * weight - int, pounds, nullable
  * sex - string, "male" or "female", nullable
  * height - int, inches, nullable

## POST /trails/api/1/Activity

Store a new activity, after it has been completed.

### Parameters

* time_started - string, ISO 8601 date("yyyy-MM-dd'T'HH:mm:ss")
* duration - string, ISO 8601 date('HH:mm:ss')
* mileage - float
* calories_burned - int
* exercise_type - string, an ExcerciseType datatype
* path - array of linestrings, because an activity can be paused

ex. 
{
  "username":"szook",
  "time_started":"2016-03-07T20:08:54",
  "duration":"01:20:34",
  "mileage":14.5765489456,
  "calories_burned":250,
  "exercise_type":"bike",
  "path":"0 0,1 1,2 2,3 3,4 4,5 5"
}

### Response

* HTTP 200

## GET /trails/api/1/Activity

Returns all of the activities for the current user. Does not include paths.

### Response

* HTTP 200
  * array:
    * time_started - string, ISO 8601 date("yyyy-MM-dd'T'HH:mm:ss")
    * duration - string, ISO 8601 date("HH:mm:ss")
    * mileage - float
    * calories_burned - int
    * exercise_type - string, an ExcerciseType datatype
  
## GET /Statistics

Returns the aggregate of all the activities for a user in the system.

### Response

* HTTP 200
  * type - string, "Overall", "Bike", "Run", or "Walk"
  * total_calories - int
  * total_duration - ISO 8601 duration
  * total_distance - float, in miles


## GET /trails/api/1/Statistics/All

Returns the aggregate of all the activities for every user in the system.

### Response

* HTTP 200
  * type - string, "Overall", "Bike", "Run", or "Walk"
  * total_calories - int
  * total_duration - ISO 8601 duration
  * total_distance - float, in miles


## GET /trails/api/1/Path/All

Returns the path of every activity for all users
