<%@ Page MasterPageFile="~/Site.Master" Language="C#" AutoEventWireup="true" CodeBehind="Details.aspx.cs" Inherits="Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole.Settings.Default" %>

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

    <h2>Configure Global Settings</h2>
    <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="validation-summary-errors" />
    <asp:FormView ID="dataForm" runat="server"
                  ItemType="Microsoft.AzureCat.Patterns.Data.Elasticity.Models.Settings"
                  InsertItemPosition="FirstItem" UpdateMethod="UpdateItem" SelectMethod="GetItem"
                  OnItemCommand="ItemCommand" RenderOuterTable="false">

        <EditItemTemplate>
            <div id="edittabs">
                <ul>
                    <li><a href="#edittabs-base">Settings</a></li>
                </ul>
                <div id="edittabs-base">
                    <fieldset>
                        <ol>
                            <li>
                                <asp:Label ID="Label2" runat="server" AssociatedControlID="ShardPrefix">Prefix Code</asp:Label>
                                <asp:DynamicControl runat="server" DataField="ShardPrefix" ID="ShardPrefix" Mode="Edit" />
                            </li>
                            <li>
                                <asp:Label ID="Label1" runat="server" AssociatedControlID="ShardUser">DB User Name</asp:Label>
                                <asp:DynamicControl runat="server" DataField="ShardUser" ID="ShardUser" Mode="Edit" />
                            </li>
                            <li>
                                <asp:Label ID="Label4" runat="server" AssociatedControlID="ShardPassword">Password</asp:Label>
                                <asp:DynamicControl runat="server" DataField="ShardPassword" ID="ShardPassword" Mode="Edit" />
                            </li>
                            <li>
                                <asp:Label ID="Label3" runat="server" AssociatedControlID="AdminUser">Admin User</asp:Label>
                                <asp:DynamicControl runat="server" DataField="AdminUser" ID="AdminUser" Mode="Edit" />
                            </li>
                            <li>
                                <asp:Label ID="Label5" runat="server" AssociatedControlID="ShardPassword">Admin Password</asp:Label>
                                <asp:DynamicControl runat="server" DataField="AdminPassword" ID="AdminPassword" Mode="Edit" />
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
                $(function() {
                    $("#edittabs").tabs();
                });
            </script>
        </EditItemTemplate>
        <ItemTemplate>
            <div id="readtabs">
                <ul>
                    <li><a href="#readtabs-base">Settings</a></li>
                </ul>
                <div id="readtabs-base">
                    <fieldset>
                        <ol>
                            <li>
                                <asp:Label ID="Label2" runat="server" AssociatedControlID="ShardPrefix">Prefix Code</asp:Label>
                                <asp:DynamicControl runat="server" DataField="ShardPrefix" ID="ShardPrefix" Mode="ReadOnly" />
                            </li>
                            <li>
                                <asp:Label ID="Label1" runat="server" AssociatedControlID="ShardUser">Shard User</asp:Label>
                                <asp:DynamicControl runat="server" DataField="ShardUser" ID="ShardUser" Mode="ReadOnly" />
                            </li>
                            <li>
                                <asp:Label ID="Label4" runat="server" AssociatedControlID="ShardPassword">Shard Password</asp:Label>
                                <asp:DynamicControl runat="server" DataField="ShardPassword" ID="ShardPassword" Mode="ReadOnly" />
                            </li>
                            <li>
                                <asp:Label ID="Label3" runat="server" AssociatedControlID="AdminUser">Admin User</asp:Label>
                                <asp:DynamicControl runat="server" DataField="AdminUser" ID="AdminUser" Mode="ReadOnly" />
                            </li>
                            <li>
                                <asp:Label ID="Label5" runat="server" AssociatedControlID="ShardPassword">Admin Password</asp:Label>
                                <asp:DynamicControl runat="server" DataField="AdminPassword" ID="AdminPassword" Mode="ReadOnly" />
                            </li>
                        </ol>
                    </fieldset>
                </div>
            </div>
            <div style="float: right">
                <asp:Button runat="server" ID="Edit" CommandName="Edit" Text="Edit" />
            </div>
            <script type="text/javascript">
                $(function() {
                    $("#readtabs").tabs();
                });
            </script>
        </ItemTemplate>
    </asp:FormView>
</asp:Content>