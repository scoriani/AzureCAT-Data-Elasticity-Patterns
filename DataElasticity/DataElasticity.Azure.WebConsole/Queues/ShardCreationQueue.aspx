<%@ Page MasterPageFile="~/Site.Master" Language="C#" AutoEventWireup="true" CodeBehind="ShardCreationQueue.aspx.cs" Inherits="Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole.Queues.ShardCreationQueue" %>

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
    <asp:Label runat="server" AssociatedControlID="" Text="Filter" />
    <asp:DropDownList ID="Filter" runat="server" OnSelectedIndexChanged="Filter_SelectedIndexChanged" AutoPostBack="true">
        <asp:ListItem>Queued</asp:ListItem>
        <asp:ListItem>InProcess</asp:ListItem>
        <asp:ListItem>Completed</asp:ListItem>
        <asp:ListItem>Errored</asp:ListItem>
    </asp:DropDownList>
    <asp:Repeater runat="server" ID="queueList">
        <HeaderTemplate>
            <ul>
        </HeaderTemplate>
        <ItemTemplate>
            <li>Status: <%#Eval("Status") %><br />
                Message: <%#Eval("Message") %><br />
                Last Touched Date (UTC): <%#Eval("LastTouched") %><br />
                Workload Group: <%#Eval("TableGroupName") %><br />
                ServerInstanceName: <%#Eval("ServerInstanceName") %><br />
                Catalog: <%#Eval("Catalog") %><br />
            </li>
        </ItemTemplate>
        <FooterTemplate>
        </ul>
        </FooterTemplate>
    </asp:Repeater>
</asp:Content>