<%@ Page MasterPageFile="~/Site.Master" Language="C#" AutoEventWireup="true" CodeBehind="MoveTenant.aspx.cs" Inherits="Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole.WorkloadGroupConfigs.MoveTenant" %>

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
    <div>
        <%= lastMessage %>
    </div>
    <fieldset>
        <legend>Schedule Tenant Move
        </legend>
        <ol>
            <li>
                <asp:Label AssociatedControlID="txtToken" runat="server">(Optional) Tenant Token:</asp:Label>
                <asp:TextBox ID="txtToken" runat="server"></asp:TextBox>
                <asp:Button ID="convertToTenantId" runat="server" OnClick="convertToDistributionKey_Click" /></li>
            <li>
                <asp:Label ID="TenantId" AssociatedControlID="txtTenantId" runat="server">Tenant ID:</asp:Label>
                <asp:TextBox ID="txtTenantId" runat="server"></asp:TextBox></li>
            <li>
                <asp:Label ID="NewShard" AssociatedControlID="ddlNewShard" runat="server">Move To:</asp:Label>
                <asp:DropDownList ID="ddlNewShard" runat="server" />
            </li>
        </ol>
        <div style="float: right">
            <asp:Button ID="btnQueue" runat="server" OnClick="btnQueue_Click" Text="Queue Move" />
            <asp:Button ID="btnCancel" runat="server" OnClick="btnCancel_Click" Text="Cancel" />
        </div>
    </fieldset>
</asp:Content>