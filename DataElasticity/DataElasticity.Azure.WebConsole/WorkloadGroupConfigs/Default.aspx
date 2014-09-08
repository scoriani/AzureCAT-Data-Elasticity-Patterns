<%@ Page Title="Workload Group Configurations" MasterPageFile="~/Site.Master" Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole.WorkloadGroupConfigs.Default" %>

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
    <h3>Workload Configurations</h3>

    <asp:Repeater runat="server" ID="workgroupList">
        <ItemTemplate>
            <li><asp:HyperLink runat="server" Text="<%# Container.DataItem %>" NavigateUrl='<%# String.Format("~/WorkloadGroupConfigs/Details.aspx?id={0}", Container.DataItem) %>' /></li>
        </ItemTemplate>
        <HeaderTemplate>
            <ul>
        </HeaderTemplate>
        <FooterTemplate>
        </ul>
            <a runat="server" href="~/WorkloadGroupConfigs/Details.aspx">Create New</a>
        </FooterTemplate>
    </asp:Repeater>
</asp:Content>