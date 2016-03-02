# AppApi
POST account/create
  The POST call will return 200 is successful and 401 if failed
   json format 
{
  "username":"ggrimm",
  "password":"wh0cares",
  "dob":1991,
  "weight":150,
  "sex":"male",
  "height":70
}

GET account 
  The GET call will return an array with 5 variables 
    1. "200" so you know it works
    2. "dob" in year 
    3. "weight" as int
    4. "sex" 
    5. "height" as int
