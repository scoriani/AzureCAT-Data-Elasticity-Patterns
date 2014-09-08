# Before running this script
#  1) CD to the directory containing this script
#  2) Make sure the local storage emulator is running.
#  3) Check the values of the variables at the top of the script.
#

# Drop the data elasticity demo on local storage and databases

Write-Output "=== Get Local Storage Context ==="
Write-Output ""
$storageContext = New-AzureStorageContext -Local

# drop the Azure storage components
..\Common\_DropDataElasticityDemoStorage.ps1

Write-Output ""
Write-Output " === Dropping Databases ==="
Write-Output ""

# drop the local databases
.\_DropDatabases.ps1
