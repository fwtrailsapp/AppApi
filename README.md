# City of Fort Wayne Rivergreenways API (CRAPI) Overview

## POST Requests

For POST, all params are sent as a JSON object in the body of the request. These headers should contain `Content-Type: application/json`.

## Date Format

All absolute dates are sent in [ISO 8601][1] format. This is a standard which has support from all of our platforms.

## Data Types

* `ExerciseType` can be `bike` or `run` or `walk`.
* `LineString` is a string where a point is float lat and float long, separated by a comma, and each point is separated with by a space. Example: `20.3323,70.4531 21.3323,71.4531 22.3323,72.4531`

# API SPEC v1

## POST /Account/Create

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

## GET /Account/(username)

Requests the account information of the user.

### Responses

* HTTP 200
  * dob - int, birth year, nullable
  * weight - int, pounds, nullable
  * sex - string, "male" or "female", nullable
  * height - int, inches, nullable

## POST /Activity

Store a new activity, after it has been completed.

### Parameters

* username - string
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

## GET /Activity/(username)

Returns all of the activities for the current user. Does not include paths.

### Response

* HTTP 200
  * array:
    * time_started - string, ISO 8601 date("yyyy-MM-dd'T'HH:mm:ss")
    * duration - string, ISO 8601 date('HH:mm:ss')
    * mileage - float
    * calories_burned - int
    * exercise_type - string, an ExcerciseType datatype
  
## GET /Statistics/(username)

Returns the aggregate of all the activities for every user in the system.

### Response

* HTTP 200
  * total_calories - int
  * total_duration - [ISO 8601 duration][1], total duration of all activities.
  * total_distance - float, in miles, total distance traveled for all activities.
  *
Bike Activity
  * total_calories - int
  * total_duration - [ISO 8601 duration][1], total duration of all activities.
  * total_distance - float, in miles, total distance traveled for all activities.
  * 
Run Activity
  * total_calories - int
  * total_duration - [ISO 8601 duration][1], total duration of all activities.
  * total_distance - float, in miles, total distance traveled for all activities.
Walk Activity
  * total_calories - int
  * total_duration - [ISO 8601 duration][1], total duration of all activities.
  * total_distance - float, in miles, total distance traveled for all activities.
  
