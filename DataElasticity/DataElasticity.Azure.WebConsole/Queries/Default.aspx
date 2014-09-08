<%@ Page MasterPageFile="~/Site.Master" Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole.Queries.Default" %>

<asp:Content runat="server" ID="FeaturedContent" ContentPlaceHolderID="FeaturedContent">
    <section class="featured">
        <div class="content-wrapper">
            <hgroup class="title">
                <h1><%: Title %>.</h1>
            </hgroup>
        </div>
    </section>
</asp:Content>
<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">

    <h2>Query</h2>
    <asp:DropDownList ID="WorkloadGroupNames" runat="server" Width="184px">
        <asp:ListItem Value="tpch">TPCH</asp:ListItem>
        <asp:ListItem Value="slk">SLK</asp:ListItem>
        <asp:ListItem Value="UserData">UserData</asp:ListItem>
    </asp:DropDownList>
    <br />
    <asp:TextBox ID="TenantToken" runat="server" Width="230px"></asp:TextBox>    
    <asp:Button ID="GetTenantId" runat="server" OnClick="GetDistributionKey_Click" Text="Get Tenant ID" />
    <asp:TextBox ID="TenantId" runat="server" Width="196px"></asp:TextBox>
    <asp:Button ID="SaveTenant" runat="server" OnClick="SaveTenant_Click" Text="Save Tenant" />
    <br />
    <asp:Button ID="ExecuteQuery" runat="server" Text="Execute Query" OnClick="ExecuteQuery_Click" />
    <asp:Button ID="ExecuteNonQuery" runat="server" Text="Execute Non-Query" OnClick="ExecuteNonQuery_Click" />
    <br />
    <asp:TextBox ID="MyQuery" runat="server" TextMode="MultiLine" Height="149px" Width="889px"></asp:TextBox>
    <asp:GridView ID="QueryResults" runat="server" Height="261px" Width="899px" BackColor="White" BorderColor="#999999" BorderStyle="Solid" BorderWidth="1px" CellPadding="3" ForeColor="Black" GridLines="Vertical">
        <AlternatingRowStyle BackColor="#CCCCCC" />
        <FooterStyle BackColor="#CCCCCC" />
        <HeaderStyle BackColor="Black" Font-Bold="True" ForeColor="White" />
        <PagerStyle BackColor="#999999" ForeColor="Black" HorizontalAlign="Center" />
        <SelectedRowStyle BackColor="#000099" Font-Bold="True" ForeColor="White" />
        <SortedAscendingCellStyle BackColor="#F1F1F1" />
        <SortedAscendingHeaderStyle BackColor="#808080" />
        <SortedDescendingCellStyle BackColor="#CAC9C9" />
        <SortedDescendingHeaderStyle BackColor="#383838" />
    </asp:GridView>
    
    

</asp:Content>