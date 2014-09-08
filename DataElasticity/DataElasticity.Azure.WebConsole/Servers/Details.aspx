<%@ Page MasterPageFile="~/Site.Master" Language="C#" AutoEventWireup="true" CodeBehind="Details.aspx.cs" Inherits="Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole.Servers.Details" %>

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

    <h2>Configure Servers</h2>
    <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="validation-summary-errors" />
    <asp:FormView ID="dataForm" runat="server"
        ItemType="Microsoft.AzureCat.Patterns.Data.Elasticity.Models.Server"
        InsertItemPosition="FirstItem" InsertMethod="InsertItem" UpdateMethod="UpdateItem" SelectMethod="GetItem"
        OnItemCommand="ItemCommand" RenderOuterTable="false" DataKeyNames="ServerInstanceName">
        <InsertItemTemplate>
            <div id="inserttabs">
                <ul>
                    <li><a href="#inserttabs-base">Base</a></li>
                </ul>
                <div id="inserttabs-base">
                    <fieldset>
                        <ol>
                            <li>
                                <asp:Label ID="Label1" runat="server" AssociatedControlID="ServerInstanceName">ServerInstanceName</asp:Label>
                                <asp:DynamicControl runat="server" DataField="ServerInstanceName" ID="ServerInstanceName" Mode="Insert" />
                            </li>
                            <li>
                                <asp:Label ID="Label4" runat="server" AssociatedControlID="Location">Location</asp:Label>
                                <asp:DynamicControl runat="server" DataField="Location" ID="Location" Mode="Insert" />
                            </li>
                            <li>
                                <asp:Label ID="Label5" runat="server" AssociatedControlID="MaxShardsAllowed">MaxShardsAllowed</asp:Label>
                                <asp:DynamicControl runat="server" DataField="MaxShardsAllowed" ID="MaxShardsAllowed" Mode="Insert" />
                            </li>
                        </ol>
                    </fieldset>
                </div>
            </div>
            <div style="float: right">
                <asp:Button runat="server" ID="InsertButton" CommandName="Insert" Text="Save" />
                <asp:Button runat="server" ID="CancelInsertButton" CommandName="Cancel" Text="Cancel" CausesValidation="false" />
            </div>
            <script type="text/javascript">
                $(function () {
                    $("#inserttabs").tabs();
                });
            </script>
        </InsertItemTemplate>
        <EditItemTemplate>
            <div id="edittabs">
                <ul>
                    <li><a href="#edittabs-base">Base</a></li>
                </ul>
                <div id="edittabs-base">
                    <fieldset>
                        <ol>
                            <li>
                                <asp:Label ID="Label2" runat="server" AssociatedControlID="ServerID">ServerID</asp:Label>
                                <asp:DynamicControl runat="server" DataField="ServerID" ID="ServerID" Mode="ReadOnly" />
                            </li>
<%--  This value is currently meaningless needs to be fixed in the DAL                            
    <li>
                                <asp:Label ID="Label3" runat="server" AssociatedControlID="AvailableShards">AvailableShards</asp:Label>
                                <asp:DynamicControl runat="server" DataField="AvailableShards" ID="AvailableShards" Mode="ReadOnly" />
                            </li>--%>
                            <li>
                                <asp:Label ID="Label1" runat="server" AssociatedControlID="ServerInstanceName">ServerInstanceName</asp:Label>
                                <asp:DynamicControl runat="server" DataField="ServerInstanceName" ID="ServerInstanceName" Mode="ReadOnly" />
                            </li>
                            <li>
                                <asp:Label ID="Label4" runat="server" AssociatedControlID="Location">Location</asp:Label>
                                <asp:DynamicControl runat="server" DataField="Location" ID="Location" Mode="Edit" />
                            </li>
                            <li>
                                <asp:Label ID="Label5" runat="server" AssociatedControlID="MaxShardsAllowed">MaxShardsAllowed</asp:Label>
                                <asp:DynamicControl runat="server" DataField="MaxShardsAllowed" ID="MaxShardsAllowed" Mode="Edit" />
                            </li>
                        </ol>
                    </fieldset>
                </div>
            </div>
            <div style="float: right">
                <asp:Button runat="server" ID="UpdateButton" CommandName="Update" Text="Save" />
                <asp:Button runat="server" ID="CancelEditButton" CommandName="CancelEdit" Text="Cancel" CausesValidation="false" />
            </div>
            <script type="text/javascript">
                $(function () {
                    $("#edittabs").tabs();
                });
            </script>
        </EditItemTemplate>
        <ItemTemplate>
            <div id="readtabs">
                <ul>
                    <li><a href="#readtabs-base">Base</a></li>
                </ul>
                <div id="readtabs-base">
                    <fieldset>
                        <ol>
                            <li>
                                <asp:Label ID="Label2" runat="server" AssociatedControlID="ServerID">ServerID</asp:Label>
                                <asp:DynamicControl runat="server" DataField="ServerID" ID="ServerID" Mode="ReadOnly" />
                            </li>
<%--    This value is currently meaningless needs to be fixed in the DAL   
                              <li>
                                <asp:Label ID="Label3" runat="server" AssociatedControlID="AvailableShards">AvailableShards</asp:Label>
                                <asp:DynamicControl runat="server" DataField="AvailableShards" ID="AvailableShards" Mode="ReadOnly" />
                            </li>--%>
                            <li>
                                <asp:Label ID="Label1" runat="server" AssociatedControlID="ServerInstanceName">ServerInstanceName</asp:Label>
                                <asp:DynamicControl runat="server" DataField="ServerInstanceName" ID="ServerInstanceName" Mode="ReadOnly" />
                            </li>
                            <li>
                                <asp:Label ID="Label4" runat="server" AssociatedControlID="Location">Location</asp:Label>
                                <asp:DynamicControl runat="server" DataField="Location" ID="Location" Mode="ReadOnly" />
                            </li>
                            <li>
                                <asp:Label ID="Label5" runat="server" AssociatedControlID="MaxShardsAllowed">MaxShardsAllowed</asp:Label>
                                <asp:DynamicControl runat="server" DataField="MaxShardsAllowed" ID="MaxShardsAllowed" Mode="ReadOnly" />
                            </li>
                        </ol>
                    </fieldset>
                </div>
            </div>
            <div style="float: right">
                <asp:Button runat="server" ID="Edit" CommandName="Edit" Text="Edit" />
            </div>
            <script type="text/javascript">
                $(function () {
                    $("#readtabs").tabs();
                });
            </script>
        </ItemTemplate>
        <FooterTemplate>
            <a runat="server" href="~/Servers/Default.aspx">&lt;- Back</a>
        </FooterTemplate>
    </asp:FormView>
</asp:Content>
