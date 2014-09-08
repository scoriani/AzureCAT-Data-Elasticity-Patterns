# Script to add Azure Storage for the the Data Elasticity demo
# Assumes the $storageContext variable is already set

# add queues
Write-Output "=== Add Queues ==="
Write-Output ""
Write-Output "Adding queue: shardcreationsqueue"
New-AzureStorageQueue -Name shardcreationsqueue -Context $storageContext
Write-Output "Adding queue: sharddeletionsqueue"
New-AzureStorageQueue -Name sharddeletionsqueue -Context $storageContext
Write-Output "Adding queue: shardmappublishingsqueue"
New-AzureStorageQueue -Name shardmappublishingsqueue -Context $storageContext
Write-Output "Adding queue: shardsynchronizationsqueue"
New-AzureStorageQueue -Name shardsynchronizationsqueue -Context $storageContext
Write-Output "Adding queue: shardletmovesqueue"
New-AzureStorageQueue -Name shardletmovesqueue -Context $storageContext

# add tables backing the queues
Write-Output ""
Write-Output "=== Add Queue Backing Tables ==="
Write-Output ""

Write-Output "Adding table: shardcreationstable"
New-AzureStorageTable -Name shardcreationstable -Context $storageContext
Write-Output "Adding table: sharddeletionstable"
New-AzureStorageTable -Name sharddeletionstable -Context $storageContext
Write-Output "Adding table: shardmappublishingstable"
New-AzureStorageTable -Name shardmappublishingstable -Context $storageContext
Write-Output "Adding table: shardsynchronizationstable"
New-AzureStorageTable -Name shardsynchronizationstable -Context $storageContext
Write-Output "Adding table: shardletmovestable"
New-AzureStorageTable -Name shardletmovestable -Context $storageContext

# add tables tracking shards, shardlets and shardlet connection
Write-Output ""
Write-Output "Add Range Shard Map, Shardlet and Shardlet Connection Tables"
Write-Output ""
Write-Output "Adding table: rangeshardtable"
New-AzureStorageTable -Name rangeshardtable -Context $storageContext 
Write-Output "Adding table: awsalesshardletmap"
New-AzureStorageTable -Name awsalesshardletmap -Context $storageContext
Write-Output "Adding table: awsalesshardletconnection"
New-AzureStorageTable -Name awsalesshardletconnection -Context $storageContext

#add dacpac blob storage
Write-Output ""
Write-Output "Add DacPacs"
Write-Output ""
Write-Output "Adding container: dacpacs"
New-AzureStorageContainer -Name dacpacs -Context $storageContext 

$dacpacPath = "..\..\DacPacs"
Write-Output "Uploading files to container: dacpacs from files in directory: $dacpacPath"
Set-AzureStorageBlobContent -File "$dacpacPath\AWRef.dacpac" -Context $storageContext -Container "dacpacs"
Set-AzureStorageBlobContent -File "$dacpacPath\AWRefSales.dacpac" -Context $storageContext -Container "dacpacs"
Set-AzureStorageBlobContent -File "$dacpacPath\AWSales.dacpac" -Context $storageContext -Container "dacpacs"
Set-AzureStorageBlobContent -File "$dacpacPath\AWSales.Deploy.azuredb.publish.xml" -Context $storageContext -Container "dacpacs"
Set-AzureStorageBlobContent -File "$dacpacPath\AWSales.Sync.azuredb.publish.xml" -Context $storageContext -Container "dacpacs"
Set-AzureStorageBlobContent -File "$dacpacPath\DataElasticity.ConfigDB.dacpac" -Context $storageContext -Container "dacpacs"




