# Deepflow Platform
This repository contains the code for the Deepflow Backend and Agent. We will split them out after the prototype stage but for now they are together.

## Environments
We have the following environments running Deepflow:

### Demo:
URL: `http://deepflow-demo.s3-website-ap-southeast-2.amazonaws.com/0.3.0/`
Backend Host: `54.252.191.213`

### Test:
URL: `http://deepflow-test.s3-website-ap-southeast-2.amazonaws.com/0.3.0/`
Backend Host: `54.206.127.10`

# Architecture
The Deepflow Backend sits in the cloud and receives customer data via an Agent deployed on the customers premises, behind their firewall. The data is visualised with a browser-based interface.

## Backend
The Backend is a series of services for providing fast data access and an entity model. It consists of an Ingestion API, Aggregated Store, Data API, Model Store and Model API. All of these services are hosted in a cloud environment, and so messages between the customer's network and the Backend will need to move across the internet in a secured manner.

### Ingestion API
This is a REST API exposed to the internet but secured for private login. It's job is to receive incoming data into the Deepflow Backend. It provides orders and recieves data from the Agent.

### Aggregated Store
Deepflow provides fast access to data by pre-aggregating raw data to various levels. That aggregated data is stored in the Aggregated Store. It is added to by the Ingestion API and read from the Data API.

### Data API
The Data API provides access to aggregated data stored in the Aggregated Store. Users viewing a chart will choose entities and attributes, and the browser will fire requests to the Data API, which will return the requested data as fast as possible. This is only used for aggregated data, not raw data. Instead, raw data is 
fetched directly from the Agent, which fetches the data from the data source.

## Agent
The Agent is responsible for 3 things:

### Receiving realtime notifications
The Agent wil act as a push API for data sources to notify Deepflow of new raw data. Data sources will push to this API as soon as a point becomes available. As a point is received, the Agent will first ask the data source for the most recent five minute aggregated point (this is because the Backend does not store any raw data and so is unable to generate 5 minute aggregations on its own). Secondly, the raw and aggregated point are passed to the backend, where the backend adds the aggregated point to the store and recaclulates any higher aggregations. Finally, the raw point is sent to any browsers running the Deepflow frontend, who then show the raw point appearing on the screen in realtime.

### Fulfilling raw requests for Charts
The Agent will fulfill requests for raw data from browsers running the Deepflow frontend. If the user is viewing a chart and has zoomed in all the way to a range of a few hours then instead of showing them aggregated data (which will normally be served by the cache in the Backend), the request will bypass the Deepflow backend and instead make an intranet connection directly to the Agent. The Agent will query the data source for the raw data for the requested tag and return it promptly.

### Backfilling missing data
The Agent will backfill historical data that was missed due to downtime or for performing a bulk load (when the customer first signs on). While online, the Agent will periodically ask the Backend if there are any missing ranges for the attributes configured in the system. The Backend will provide the full list of missing time ranges. Then the Agent will go through this list and fetch all the missing ranges one-by-one from the data source and push it to the Backend. After a period of time (minutes to days depending of bulk amount) the Backend will report no missing ranges and the Agent will sit idle and await further instructions.

# Implementing a data source
When launched, we will be using historians such as PI and PHD as data sources. Until then we will have a historian simulator which will perform the same functions as PI. 


## REST API
To implement a data source, such as our PI Simulator, then the data source will need to expose the following as a REST API.

### Fulfill raw time series requests
There will be a REST API which will take a tag name and time range and will return data in a specific format. This is so raw data can be shown on charts. The request from the Agent will take the form:

#### Path
```
GET http://{host}/api/v1/Tags/{tag}/Raw/Data?minTime={minTime}&maxTime={maxTime}
```
`Example: http://{host}/api/v1/Tags/TAGONE/Raw/Data?minTime=2017-10-25T09:23:16Z&maxTime=2017-10-25T10:23:16Z`

Note: The `minTime` and `maxTime` are in ISO 8601 UTC date format (with the T in the middle and the Z at the end to indicate UTC) - exactly as in the example.

#### Response Body
```
{
    "TimeRange": {
        "Min": 1508923396,
        "Max": 1508926996
    },
    "Data": [
        1508923396,
        123.456,
        1508923496,
        321.756,
        ...
    ]
}
```
Note: The Min and Max are UNIX timestamps (seconds since Jan 1 1970, 12 AM UTC - this is just the way i've done it for now). The data is an array of [UNIX timestamp, value, UNIX timestamp, value, ...]

### Fulfill aggregated time series requests
There will be a REST API which will take a tag name, time range and aggregation seconds and will return data in a specific format. This is so aggregated data can be send to the backend. The request from the Agent will take the form:

#### Path
```
GET http://{host}/api/v1/Tags/{tag}/Aggregations/{aggregationSeconds}/Data?minTime={minTime}&maxTime={maxTime}
```
`Example: http://{host}/api/v1/Tags/TAGONE/Aggregations/300/Data?minTime=2017-10-25T09:23:16Z&maxTime=2017-10-25T10:23:16Z`

Note: The `minTime` and `maxTime` are in ISO 8601 UTC date format (with the T in the middle and the Z at the end to indicate UTC) - exactly as in the example.

#### Response Body
```
{
    "TimeRange": {
        "Min": 1508923396,
        "Max": 1508926996
    },
    "Data": [
        1508923396,
        123.456,
        1508923496,
        321.756,
        ...
    ],
    "AggregationSeconds": 300
}
```
Note: The `Min` and `Max` are UNIX timestamps (seconds since Jan 1 1970, 12 AM UTC - this is just the way i've done it for now). The data is an array of [UNIX timestamp, value, UNIX timestamp, value, ...]. The `AggregationSeconds` must be set to the same value that was passed in on the request path. 

## Notification API
The data source must also notify the Agent's REST API when new raw data is available using a request of the following form:

#### Path
```
POST http://{host}:5002/api/v1/Tags/{tag}/Raw/Data
```
`Example: http://{host}:5002/api/v1/Tags/TAGONE/Raw/Data`

#### Request Body
```
{
    "TimeRange": {
        "Min": 1508923396,
        "Max": 1508923396
    },
    "Data": [
        1508923396,
        123.456
    ]
}
```
Note: The `Min` and `Max` are UNIX timestamps (seconds since Jan 1 1970, 12 AM UTC - this is just the way i've done it for now). The data is an array of [UNIX timestamp, value] of the fresh raw point.
