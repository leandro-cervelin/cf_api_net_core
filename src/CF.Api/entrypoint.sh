#!/bin/bash

set -e
run_cmd="dotnet run --server.urls http://*:80"

until dotnet ef database update; do
>&2 echo "SQL Server is starting up"
sleep 1
done

>&2 echo "SQL Server is up - executing command"
exec $run_cmd
sleep 10

>&2 echo "Creating Database"
/opt/mssql-tools/bin/sqlcmd -S db -U sa -P CF@!1234FC6549 -i create-database.sql