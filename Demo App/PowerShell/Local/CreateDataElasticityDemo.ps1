# Before running this script
#  1) CD to the directory containing this script
#  2) Make sure the local storage emulator is running.
#  3) Check the values of the variables at the top of the script.
#

Write-Output "=== Get Local Storage Context ==="
Write-Output ""
$storageContext = New-AzureStorageContext -Local

# create the Azure storage components
..\Common\_CreateDataElasticityDemoStorage.ps1

Write-Output ""
Write-Output " === Creating Databases ==="
Write-Output ""

Write-Output "Creating AwMain"
.\_DeployAwMainDacPac.ps1
