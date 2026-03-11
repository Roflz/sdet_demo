# Run after: docker compose up -d
# Requires: SQL Server container running. Creates DB and runs schema + seed.

$server = "localhost,1433"
$user = "sa"
$password = "InsuranceDemo1!"
$db = "InsuranceDemo"

$env:SQLCMDPASSWORD = $password

# Create database (run against master)
& sqlcmd -S $server -U $user -C -Q "IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '$db') CREATE DATABASE $db"
if ($LASTEXITCODE -ne 0) {
    Write-Host "Tip: Install sqlcmd or run init-db.sql and schema.sql + seed.sql in Azure Data Studio against $server"
    exit 1
}

# Schema and seed against InsuranceDemo
& sqlcmd -S $server -U $user -d $db -C -i "sql/schema.sql"
& sqlcmd -S $server -U $user -d $db -C -i "sql/seed.sql"

Remove-Item Env:SQLCMDPASSWORD -ErrorAction SilentlyContinue
Write-Host "Database $db initialized."
