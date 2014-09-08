<%@ Page Title="Workload Group Configurations" MasterPageFile="~/Site.Master" Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole.Servers.Default" %>

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
    <h3>Servers</h3>

    <asp:Repeater runat="server" ID="serverList">
        <ItemTemplate>
            <li><asp:HyperLink runat="server" Text='<%# Eval("ServerInstanceName") %>' NavigateUrl='<%# String.Format("~/Servers/Details.aspx?id={0}", Eval("ServerInstanceName")) %>' /></li>
        </ItemTemplate>
        <HeaderTemplate>
            <ul>
        </HeaderTemplate>
        <FooterTemplate>
        </ul>
            <a runat="server" href="~/Servers/Details.aspx">Create New</a>
        </FooterTemplate>
    </asp:Repeater>
</asp:Content>