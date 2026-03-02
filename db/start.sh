#!/bin/bash

# author: martin@affolter.net

password="Som3V3ryS3cretP4ssw0rd!"
rootpath="../src"

echo
echo
echo "starting databases in docker compose"
echo
echo

# write init sql
echo "create database example" > init.sql

# connection strings
localconnstring="Server=localhost,1436;Database=example;User Id=sa;Password=${password};Encrypt=False"
pgconnstring="Host=localhost;Port=5436;Database=example;Username=postgres;Password=${password}"

echo "SQL Server: $localconnstring"
echo "PostgreSQL: $pgconnstring"

# start containers
docker compose up -d --build --wait

generate() {

    local projectname=$1
    update_tool_path="$rootpath/$projectname/$projectname.Update"
    update_tool_dll="$projectname.Update.dll"

    # update db - from localhost
    pushd .

    cd "$update_tool_path"
    dotnet publish -c Release -o ./pub
    cd "pub"

    echo "dotnet $update_tool_dll \"dbup\" \"$localconnstring\" -h All"
    dotnet $update_tool_dll "dbup" "$localconnstring" "-h All"

    echo "dotnet $update_tool_dll \"gen\" \"$localconnstring\""
    dotnet $update_tool_dll "gen" "$localconnstring"
    popd
}

generate_pg() {

    local projectname=$1
    update_tool_path="$rootpath/$projectname/$projectname.Update"
    update_tool_dll="$projectname.Update.dll"

    # update db - from localhost
    pushd .

    cd "$update_tool_path"
    dotnet publish -c Release -o ./pub
    cd "pub"

    echo "dotnet $update_tool_dll \"dbup\" \"$pgconnstring\""
    dotnet $update_tool_dll "dbup" "$pgconnstring"

    echo "dotnet $update_tool_dll \"gen\" \"$pgconnstring\""
    dotnet $update_tool_dll "gen" "$pgconnstring"
    popd
}

# SQL Server examples
generate "Example"
generate "ExampleHistory"
generate "ExampleUserDate"
generate "ExampleVersion"
generate "ExampleVersionUserDate"
generate "ExampleVersionUserDateHistory"

# PostgreSQL examples
generate_pg "ExamplePg"
generate_pg "ExamplePgHistory"
generate_pg "ExamplePgUserDate"
generate_pg "ExamplePgVersion"
generate_pg "ExamplePgVersionUserDate"
generate_pg "ExamplePgVersionUserDateHistory"
