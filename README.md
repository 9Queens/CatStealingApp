#  CatStealingApp

A .NET 8 Web API solution for downloading, tagging, and managing cats — complete with Docker support and integration tests.

## What this app does

- Offers a variety of endpoints which help users schedule asynchronous cat stealing operations (batched)
  from a third-party cat farm. It allows real-time monitoring of job status and searching acquired cats
  via tag names, serving them in a paginated way to callers.
  
- The solution offers a stealing approach that is steady as a rock and error-proof as hell. NO MATTER HOW MANY
  batches you place at the same time (I dare you to place thousands), this thing will continue to steal cats
  no matter what.

- A faster approach exists in comments that uses parallelism, but I haven’t finalized it yet… it’s like 'light' speed fast
  but loses cats (future update).

- Each batch has its own mechanism for tracking duplicate download attempts (I use hashes of the byte arrays), failures,
  or other errors for the entire operation (batch). Hangfire filters are used to capture the start and end of each batch operation
  and simultaneously append messages for failures, successes, and statistics about captured cats.


- I still have some comments here and there intentionally, for you to better understand my approach.
  I decided not to go hardcore and create a separate table for each download item — I kept it simple but
  included all required download statistics.
  You can also watch the console as I use basic logging.

- The dockerized solution is in [ debug ], NOT release (keep that in mind).
  You can, of course, change the Docker scripts — feel free.

  Actual endpoints of the CatApp:

	- GET /api/Cats/all  or /api/cat/all?page=1&pageSize=10

	- GET /api/Cats/{int:id}

	- GET /api/cats?page=1&pageSize=10 or /api/cat/by-tags?tag=cute,fluffy&page=1&pageSize=10 

	- POST /api/cat/fetch?size=0  (default will fetch 25)  or POST /api/cat/fetch?size=50
  
 	- GET /api/cat/jobs/{batchId}


        Extra endpoints :

	-  GET /api/cat/jobs/active    ( for monitoring concurrent stealing operations in real time )
	-  GET /api/cat/jobs/completed ( to display completed operations )




## Projects

- `CatApp.Shared` – Shared entities, helpers, migrations , local API responses, remote api responses, Dtos
   helpers and files that helps us created dbcontexts do be testable and datarace proof.
- `CatApp.Services` – Business logic and services
- `CatApp` – Web API project with Dockerfile
- `CatApp.Integration.Tests` – Integration tests using WebApplicationFactory
   (just a sample of full intergration tests examples for only one api endpoint using in memory test databases and
    and one full unit test only in one service  - not all )

## Docker Support

- `Dockerfile` inside the Web API project
- `docker-compose.yml` in the root
- `entrypoint.sh` in the root <---- also tools to ensure that the file format will be preserved in LF (otherwise app will never start) !!!!
-  mssql-tools < --- to help you perform SQL queries and see the data live while hitting the API


## Getting Started

	Before you do anything, have this in mind...

	Swagger documentation is included — and you can also view the DTOs:

         - http://localhost:5000/swagger/index.html ( example url from my container)

	WeatherForecast endpoint 
	 - I always leave this for legacy, it's like tradition


	Regarding containerization and dbcontext schema and data migration:

	- The database context exists inside the CatApp.Shared project — NOT in the main app (CatApp).
	  I wanted my solution to be as decoupled as possible.

	- For now, the commit has the latest migrations inside CatApp.Shared, which are copied during
          the dockerization process. You don’t need to do anything else — just run the app (either in Docker or your IDE
          as usual).

	- In the root folder of the app there’s an entrypoint.sh file. I tried to create the migrations folder
	  on the fly (for Docker), but couldn’t manage to do it — I was always getting an SQL-specific error during
	  the containerization process. (That’s the actual reason the first commit includes the Migrations folder.)
	  Apart from that, the entrypoint.sh file contains logic to wait for the database inside the Docker
          container to be available, and then starts the main app (CatApp).
          (My failed attempts to migrate on the fly during dockerization still exist but are commented out.)
	
	

To test the app using docker :
1) Navigate to the root folder of the solution to build and run the dockerized solution using powershell
```bash
docker-compose up --build

2) After successfully build and run in docker you can visit 
-  http://localhost:5000/swagger/index.html 

3) Try placing at the same time as much cat stealing operations as you want:
   api/cats/fetch    


4) To display the cocncurrent batches live ...

 try the following swagger endpoints
  - api/cats/jobs/active
  - api/cats/jobs/completed

 OR directly from withing the container with pre installed mssql-tools using powershell

 Open a powershell and type:

 >>sqlcmd -S localhost,1433 -U sa -P AnotherStrongPassword123 -d MewDb -Q "SELECT * FROM CatDownloadProgresses"

 Output :

Id                                   TotalCats   CatsDownloaded DoublicatesOccured ErrorsOccured StartedOn                                     CompletedOn                                   BatchFailures Status      Messages
------------------------------------ ----------- -------------- ------------------ ------------- --------------------------------------------- --------------------------------------------- ------------- ----------- ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
85A20A20-0D8A-4FD7-874B-150D8C282CAE           4              4                  0             0            2025-04-22 23:09:02.4692547 +00:00            2025-04-22 23:09:07.8977878 +00:00          NULL           0
[Success] 4 / 4 cats aquired!!  Job completed successfully for batch 85a20a20-0d8a-4fd7-874b-150d8c282cae at 04/22/2025 23:09:07 +00:00

FB1537E6-1002-412E-8F58-4952470CA902          10              0                  0             0            2025-04-22 23:01:27.5066667 +00:00                                          NULL             0           0 test
936EEDDC-884C-4DF1-AC58-7ADCCD0A2A46          25             25                  3             0            2025-04-22 23:10:03.7189278 +00:00            2025-04-22 23:10:43.2820748 +00:00          NULL           0
[Success] 25 / 25 cats aquired!!  Job completed successfully for batch 936eeddc-884c-4df1-ac58-7adccd0a2a46 at 04/22/2025 23:10:43 +00:00

9A2BAF59-3D60-40BC-AFF2-ADF99F6E9B45          20             20                  0             0            2025-04-22 23:09:00.4507174 +00:00            2025-04-22 23:09:32.2432449 +00:00          NULL           0
[Success] 20 / 20 cats aquired!!  Job completed successfully for batch 9a2baf59-3d60-40bc-aff2-adf99f6e9b45 at 04/22/2025 23:09:32 +00:00

F32D3E83-0979-47D5-B6B4-C52539BF5E19          20             20                  0             0            2025-04-22 23:08:57.9739198 +00:00            2025-04-22 23:09:30.2900406 +00:00          NULL           0
[Success] 20 / 20 cats aquired!!  Job completed successfully for batch f32d3e83-0979-47d5-b6b4-c52539bf5e19 at 04/22/2025 23:09:30 +00:00

BB87700A-C0D9-404A-B43F-DCB51CD15E16          65             65                 11             0            2025-04-22 23:09:05.8125968 +00:00            2025-04-22 23:10:43.0571447 +00:00          NULL           0
[Success] 65 / 65 cats aquired!!  Job completed successfully for batch bb87700a-c0d9-404a-b43f-dcb51cd15e16 at 04/22/2025 23:10:43 +00:00

7862FB19-B3FF-43AD-80EA-E215A50CDE78           5              5                  0             0            2025-04-22 23:10:00.2543490 +00:00            2025-04-22 23:10:11.3414001 +00:00          NULL           0
[Success] 5 / 5 cats aquired!!  Job completed successfully for batch 7862fb19-b3ff-43ad-80ea-e215a50cde78 at 04/22/2025 23:10:11 +00:00



