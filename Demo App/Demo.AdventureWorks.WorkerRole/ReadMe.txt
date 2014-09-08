Note:

When publishing to Azure and utilizing DacPac services, a build will report, in the build output of the Azure project, 
that certain DLLs must be changed to copy local in order to be pushed to Azure with the package.  These DLLs do not
natively exist on the Azure Worker Role.

Make sure the follow DLLs are referenced in the Worker Role project and change their "Copy Local" property to "True":

	• C:\windows\assembly\GAC_MSIL\Microsoft.SqlServer.SqlClrProvider\11.0.0.0__89845dcd8080cc91\Microsoft.SqlServer.SqlClrProvider.dll
	• C:\Program Files (x86)\Microsoft SQL Server\110\SDK\Assemblies\Microsoft.SqlServer.ConnectionInfo.dll
	• C:\windows\Microsoft.Net\assembly\GAC_MSIL\Microsoft.SqlServer.TransactSql\v4.0_11.0.0.0__89845dcd8080cc91\Microsoft.SqlServer.TransactSql.dll
	• C:\Program Files (x86)\Microsoft SQL Server\110\SDK\Assemblies\Microsoft.SqlServer.TransactSql.ScriptDom.dll
	• C:\windows\Microsoft.Net\assembly\GAC_MSIL\Microsoft.Data.Tools.Components\v4.0_11.1.0.0__b03f5f7f11d50a3a\Microsoft.Data.Tools.Components.dll
