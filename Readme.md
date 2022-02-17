# asp net core with rate limit feature

## topics
* asp net rate limit for Request Throttling
* k6 scripts for load testing

## features 
* redis distributed cache
* redis processing strategy
* modified ClientRateLimitMiddleware and cache key build logic for user request rate limit counter

## prerequisites
* asp net core
* redis
* identity service
* k6


## noted
* should revise settings for custom usage
* can run redis or some other db with docker for testing

## loading test with k6

``` bash
# -d,-duration => test duration limit
# -u,--vus => number of virtual users
# -i,--iterations => script total iteration limit (among all VUs)

# sample request in certain duration
k6 run -duration 4s --vus 2 ./script.js

# sample request in certain iterations
k6 run -i 10 -u 10 ./script.js

# pass env variable
--env MY_HOSTNAME=https://localhost:5013

--env TOKEN=sampleToken

#
k6 run --env MY_HOSTNAME=https://localhost:5013 --env TOKEN=sampleToken -i 10 -u 10 ./loadTestScripts/PostSubmit.js 
```
