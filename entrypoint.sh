#!/bin/bash
set -e

# Wait for SQL Server to be ready with retries
MAX_RETRIES=12
RETRY_INTERVAL=10
for ((i=1; i<=MAX_RETRIES; i++)); do
	echo "source entrypoint.sh file..."
    echo "Checking SQL Server availability (Attempt $i/$MAX_RETRIES)..."
    nc -z sqlserver 1433 && break
    if [ $i -eq $MAX_RETRIES ]; then
        echo "SQL Server is not ready after $MAX_RETRIES attempts. Exiting."
        exit 1
    fi
    sleep $RETRY_INTERVAL
done
echo "SQL Server is ready."

# Create MewDb if it doesn’t exist using 'sa'
#if ! /opt/mssql-tools/bin/sqlcmd -S sqlserver -U sa -P sa -Q "IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'MewDb') CREATE DATABASE MewDb"; then
#    echo "Failed to create MewDb."
#    exit 1
#fi


#------------------------------ we did not use it in the end
# Grant AppUser permissions on MewDb (optional, if not already set up)
#if ! /opt/mssql-tools/bin/sqlcmd -S sqlserver -U sa -P sa -d MewDb -Q "
#IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'AppUser')
#BEGIN
#    CREATE USER AppUser FOR LOGIN AppUser;
#    ALTER ROLE db_owner ADD MEMBER AppUser;
#END"; then
#    echo "Failed to set up AppUser permissions."
#    exit 1
#fi
#------------------------------ we did not use it in the end

#------------------------------ while it seems that is working ... one last time just creating Migration folder and contents alone....
# Run EF Core migrations
# echo "Generating EF Core migrations..."
# echo "Naming migration file deploymentMigration..."
# dotnet ef migrations add deploymentMigration --context DataContext --project /src/CatApp.Shared/CatApp.Shared.csproj 
#  
# echo "Applying EF Core database updates based on last generated migration name ---> 'deploymentMigration' ..."
# dotnet ef database update --project /src/CatApp.Shared/CatApp.Shared.csproj
#------------------------------ 


#------------------------------ while it seems that is working ... id does not ....
# Run EF Core migrations
#echo "Generating EF Core migrations..."
#dotnet ef migrations add anotherMigration --context DataContext --project /src/CatApp.Shared/CatApp.Shared.csproj --startup-project /src/CatApp/CatApp.csproj || echo "Migration 'InitialCreate' already exists, skipping."
#echo "Applying EF Core migrations..."
#dotnet ef database update --project /src/CatApp.Shared/CatApp.Shared.csproj --startup-project /src/CatApp/CatApp.csproj
#------------------------------ 

#------------------------------------------------------------------------------------------- IMPORTANT   
# Because the above migrations commands are not working i have run manually via cmd 
# The typical first migration of the database in the Shared project.
# So Docker can collect it and copy it - thus - the main app can use it during start time.
#---------------------------------------------------------------------------------------------------- 

# Start the application
echo "Starting the application..."
cd /app
dotnet CatApp.dll