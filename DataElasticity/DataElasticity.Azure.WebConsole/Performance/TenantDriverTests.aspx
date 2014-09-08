<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TenantDriverTests.aspx.cs" Inherits="Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole.Performance.TenantDriverTests" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
    <head runat="server">
        <title></title>
    </head>
    <body>
        <form id="form1" runat="server">
            <div>
                <asp:Label ID="lastTestSpeed" runat="server" />
                <asp:Label ID="Label3" AssociatedControlID="workloadGroupName" runat="server" Text="Fake Workload Group Name" />
                <asp:Label ID="workloadGroupName" runat="server" text="FakeWorkloadGroup"/>
                <fieldset>
                    <legend>Make Test Range</legend>
                    <ol>

                        <li>
                            <asp:Label AssociatedControlID="makeRangeSize" Text="Range Size" runat="server" />
                            <asp:TextBox ID="makeRangeSize" runat="server" /></li>
                    </ol>
                    <asp:Button ID="MakeRangeMap" runat="server" OnClick="MakeRangeMap_Click" Text="Create Range Map" />
                </fieldset>
                <hr />
                <fieldset>
                    <legend>Make Tenants For WorkloadGroup</legend>
                    <ol>
                        <li>
                            <asp:Label ID="Label2" AssociatedControlID="makeTenantsCount" Text="Tenants To Make" runat="server" />
                            <asp:TextBox ID="makeTenantsCount" runat="server" /></li>
                    </ol>
                    <asp:Button ID="MakeTenant" runat="server" OnClick="MakeTenant_Click" Text="Create Pinned Tenants" />
                </fieldset>
                <hr />
                <fieldset>
                    <legend>Lookup Tenant</legend>
                    <ol>
                        <li>
                            <asp:Label ID="Label1" AssociatedControlID="makeTenantsCount" Text="Tenants To Lookup" runat="server" />
                            <asp:TextBox ID="tenantLookupCount" runat="server" /></li>
                    </ol>
                    <asp:Button ID="LookupTenants" runat="server" OnClick="LookupTenants_Click" Text="Lookup Tenants" />
                </fieldset>
                <hr />
            </div>
        </form>
    </body>
</html>