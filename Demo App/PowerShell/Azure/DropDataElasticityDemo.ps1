# Before running this script
#  1) CD to the directory containing this script
#  2) Make sure the Azure Subscription is active.
#

# Drop the data elasticity demo on Microsoft Azure storage and databases

Add-AzureAccount

Write-Output "=== Get Azure Storage Subscription ==="

$subscriptionName = "<<your_subscription_name>>"
$storageAccountName = "<<your_storage_account_name>>"
$serverName = "<<your_server_name>>"
$userName = "Superman"
$password = "Blank123"

Select-AzureSubscription $subscriptionName
	
Write-Output ""
Write-Output " === Dropping Azure storage components ==="
Write-Output ""

$storageContext = New-AzureStorageContext -StorageAccountName $storageAccountName -StorageAccountKey (Get-AzureStorageKey –StorageAccountName $storageAccountName).Primary

..\Common\_DropDataElasticityDemoStorage.ps1

Write-Output ""
Write-Output " === Dropping Databases ==="
Write-Output ""

.\_DropDatabases.ps1
