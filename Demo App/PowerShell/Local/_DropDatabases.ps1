# Drop the Data Elasticity demo from local SQL databases

# Assumes your local NT account has admin capabilities

$serverName = "(localdb)\v11.0"
$prefix = "AdvWrk"
$tableGroup = "AwSales"
$numberOfShards = 10
$connectionString = "Server = $serverName; Database = master; Integrated Security = True"

# open a local connection
Write-Output "Dropping database: AwMain"
$connection = New-Object System.Data.SqlClient.SqlConnection 
$connection.ConnectionString = $connectionString
$connection.open() 

try {

	# create a command
	$command = New-Object System.Data.SqlClient.SqlCommand 
	$command.Connection = $connection

	# drop main reference database
	try 
	{
		$command.CommandText = "ALTER DATABASE AwMain SET SINGLE_USER WITH ROLLBACK IMMEDIATE"
		$command.ExecuteNonQuery()
		$command.CommandText = "DROP Database AwMain"
		$command.ExecuteNonQuery()
	}
	catch 
	{	
	}

	# drop pinned demo database
	try 
	{
		$command.Connection = $connection
		$command.CommandText = "ALTER DATABASE AdvWrkAWSales_HighVolume SET SINGLE_USER WITH ROLLBACK IMMEDIATE"
		$command.ExecuteNonQuery()
		$command.CommandText = "DROP Database AdvWrkAWSales_HighVolume"
		$command.ExecuteNonQuery()
	}
	catch 
	{	
	}

	# drop all shards
	for ($i=1; $i -le $numberOfShards; $i++)
	{
		$dbname = $prefix + $tableGroup + $i.ToString().PadLeft(6,'0')
		Write-Output "Dropping sharded database: $dbname"
		$sql = "ALTER DATABASE $dbname SET SINGLE_USER WITH ROLLBACK IMMEDIATE"
		try 
		{
			$command.CommandText = $sql
			$command.ExecuteNonQuery()
			$command.CommandText = "DROP Database $dbname"
			$command.ExecuteNonQuery()
		}
		catch 
		{	
		}
	}
}
finally
{
	# close a local connection
	$connection.close() 
}
