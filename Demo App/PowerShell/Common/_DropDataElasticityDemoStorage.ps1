# Script to drop Azure Storage for the the Data Elasticity demo
# Assumes the $storageContext variable is already set

# delete queues
Write-Output "=== Drop Queues ==="
Write-Output ""
Write-Output "Dropping queue: shardcreationsqueue"
Remove-AzureStorageQueue -Name shardcreationsqueue -Force -Context $storageContext
Write-Output "Dropping queue: sharddeletionsqueue"
Remove-AzureStorageQueue -Name sharddeletionsqueue -Force -Context $storageContext
Write-Output "Dropping queue: shardmappublishingsqueue"
Remove-AzureStorageQueue -Name shardmappublishingsqueue -Force -Context $storageContext
Write-Output "Dropping queue: shardsynchronizationsqueue"
Remove-AzureStorageQueue -Name shardsynchronizationsqueue -Force -Context $storageContext
Write-Output "Dropping queue: shardletmovesqueue"
Remove-AzureStorageQueue -Name shardletmovesqueue -Force -Context $storageContext

# delete tables backing the queues
Write-Output ""
Write-Output "=== Drop Queue Backing Tables ==="
Write-Output ""

Write-Output "Dropping table: shardcreationstable"
Remove-AzureStorageTable -Name shardcreationstable -Force -Context $storageContext
Write-Output "Dropping table: sharddeletionstable"
Remove-AzureStorageTable -Name sharddeletionstable -Force -Context $storageContext
Write-Output "Dropping table: shardmappublishingstable"
Remove-AzureStorageTable -Name shardmappublishingstable -Force -Context $storageContext
Write-Output "Dropping table: shardsynchronizationstable"
Remove-AzureStorageTable -Name shardsynchronizationstable -Force -Context $storageContext
Write-Output "Dropping table: shardletmovestable"
Remove-AzureStorageTable -Name shardletmovestable -Force -Context $storageContext

# delete tables tracking shards, shardlets and shardlet connection
Write-Output ""
Write-Output "Drop Range Shard Map, Shardlet and Shardlet Connection Tables"
Write-Output ""
Write-Output "Dropping table: rangeshardtable"
Remove-AzureStorageTable -Name rangeshardtable -Force -Context $storageContext 
Write-Output "Dropping table: awsalesshardletmap"
Remove-AzureStorageTable -Name awsalesshardletmap -Force -Context $storageContext
Write-Output "Dropping table: awsalesshardletconnection"
Remove-AzureStorageTable -Name awsalesshardletconnection -Force -Context $storageContext

# delete dacpac blob storage
Write-Output ""
Write-Output "Drop DacPacs"
Write-Output ""
Write-Output "Dropping container: dacpacs"
Remove-AzureStorageContainer -Name dacpacs -Context $storageContext -Force
