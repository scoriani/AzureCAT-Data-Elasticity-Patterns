<%@ Page Title="Workload Group Configurations" MasterPageFile="~/Site.Master" Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole.Queues.Default" %>

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
    <h3>Queues</h3>
    <ul>
        <li><a runat="server" href="~/Queues/ShardCreationQueue.aspx?id=ShardCreation&Status=Queued">Shard Creation</a></li>
        <li><a runat="server" href="~/Queues/PublishMapQueue.aspx?id=PublishMap&Status=Queued">Publish Map</a></li>
        <li><a id="A1" runat="server" href="~/Queues/PrepareForTenantMoveQueue.aspx?id=TenantMove&Status=Queued">Prepare Shard For Moves</a></li>
        <li><a runat="server" href="~/Queues/TenantMoveQueue.aspx?id=TenantMove&Status=Queued">Tenant Move</a></li>
        <li><a runat="server" href="~/Queues/ShardDeletionQueue.aspx?id=ShardDeletion&Status=Queued">Shard Deletion</a></li>
    </ul>
</asp:Content>