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
   (just a sample of full intergration tests examples regarding only one api endpoint with inmemory test database creation
    and one full unit test only in one service  - not all )

## Docker Support

- `Dockerfile` inside the Web API project
- `docker-compose.yml` in the root
- `entrypoint.sh` in the root


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
	
	


You can run set up the dockerized solution with :
```bash
docker-compose up --build