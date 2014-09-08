# Drop the Data Elasticity demo from Azure storage and Azure SQL databases

$prefix = "AdvWrk"
$tableGroup = "AwSales"
$numberOfShards = 10
$connectionString = "Server=tcp:$serverName.database.windows.net,1433;Database=master;User ID=$userName@$serverName;Password=$password;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;"

# open a local connection
$connection = New-Object System.Data.SqlClient.SqlConnection 
$connection.ConnectionString = $connectionString
$connection.open() 

try {

	# create a command
	$command = New-Object System.Data.SqlClient.SqlCommand 

	# drop main reference database
    Write-Output "Dropping root database AwMain"
	try 
	{
		$command.Connection = $connection
		$command.CommandText = "DROP Database AwMain"
		$command.ExecuteNonQuery()
	}
	catch 
	{	
			Write-Error ($_)
	}

	# drop pinned demo database
    Write-Output "Dropping pinned demo database AdvWrkAWSales_HighVolume"
	try 
	{
		$command.Connection = $connection
		$command.CommandText = "DROP Database AdvWrkAWSales_HighVolume"
		$command.ExecuteNonQuery()
	}
	catch 
	{	
			Write-Error ($_)
	}

	# drop all shards
	for ($i=1; $i -le $numberOfShards; $i++)
	{
		$dbname = $prefix + $tableGroup + $i.ToString().PadLeft(6,'0')
		Write-Output "Dropping sharded database: $dbname"
		try 
		{
			$command.CommandText = "DROP Database $dbname"
			$command.ExecuteNonQuery()
		}
		catch 
		{	
			Write-Error ($_)
		}
	}
}
finally
{
	# close a local connection
	$connection.close() 
}
