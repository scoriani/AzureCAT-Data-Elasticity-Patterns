<%@ Page MasterPageFile="~/Site.Master" Language="C#" AutoEventWireup="true" CodeBehind="Details.aspx.cs" Inherits="Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole.WorkloadGroupConfigs.Details" %>

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
    <asp:ObjectDataSource TypeName="Microsoft.AzureCat.Patterns.Data.Elasticity.Models.Server" SelectMethod="GetServers" runat="server" ID="ServerContext" />
    <h2>Insert TableGroupConfig</h2>
    <div>
        <%= lastMessage %>
    </div>
    <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="validation-summary-errors" />
    <asp:FormView ID="dataForm" runat="server"
                  ItemType="Microsoft.AzureCat.Patterns.Data.Elasticity.Models.TableGroupConfig"
                  InsertItemPosition="FirstItem" InsertMethod="InsertItem" UpdateMethod="UpdateItem" SelectMethod="GetItem"
                  OnItemCommand="ItemCommand" RenderOuterTable="false" DataKeyNames="TableGroupName" OnDataBound="dataForm_DataBound">
        <InsertItemTemplate>
            <div id="inserttabs">
                <ul>
                    <li><a href="#inserttabs-base">Base</a></li>
                </ul>
                <div id="inserttabs-base">
                    <fieldset>
                        <ol>
                            <li>
                                <asp:Label ID="Label1" runat="server" AssociatedControlID="InsertWorkloadGroupName">TableGroupName</asp:Label>
                                <asp:DynamicControl runat="server" DataField="TableGroupName" ID="InsertWorkloadGroupName" Mode="Insert" />
                            </li>
                            <li>
                                <asp:Label ID="Label4" runat="server" AssociatedControlID="InsertShardType">ShardType</asp:Label>
                                <asp:DynamicControl runat="server" DataField="ShardType" ID="InsertShardType" Mode="Insert" />
                            </li>
                            <li>
                                <asp:Label ID="Label5" runat="server" AssociatedControlID="InsertTargetShardCount">TargetShardCount</asp:Label>
                                <asp:DynamicControl runat="server" DataField="TargetShardCount" ID="InsertTargetShardCount" Mode="Insert" />
                            </li>
                            <li>
                                <asp:Label ID="Label6" runat="server" AssociatedControlID="InsertMaxShardCount">MaxShardCount</asp:Label>
                                <asp:DynamicControl runat="server" DataField="MaxShardCount" ID="InsertMaxShardCount" Mode="Insert" />
                            </li>
                            <li>
                                <asp:Label ID="Label7" runat="server" AssociatedControlID="InsertMaxTenantsPerShard">MaxTenantsPerShard</asp:Label>
                                <asp:DynamicControl runat="server" DataField="MaxTenantsPerShard" ID="InsertMaxTenantsPerShard" Mode="Insert" />
                            </li>
                            <li>
                                <asp:Label ID="Label8" runat="server" AssociatedControlID="InsertMinShardSizeMB">MinShardSizeMB</asp:Label>
                                <asp:DynamicControl runat="server" DataField="MinShardSizeMB" ID="InsertMinShardSizeMB" Mode="Insert" />
                            </li>
                            <li>
                                <asp:Label ID="Label9" runat="server" AssociatedControlID="InsertMaxShardSizeMB">MaxShardSizeMB</asp:Label>
                                <asp:DynamicControl runat="server" DataField="MaxShardSizeMB" ID="InsertMaxShardSizeMB" Mode="Insert" />
                            </li>
                            <li>
                                <asp:Label ID="Label10" runat="server" AssociatedControlID="InsertAllowDeployments">AllowDeployments</asp:Label>
                                <asp:DynamicControl runat="server" DataField="AllowDeployments" ID="InsertAllowDeployments" Mode="Insert" />
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
                $(function() {
                    $("#inserttabs").tabs();
                });
            </script>
        </InsertItemTemplate>
        <EditItemTemplate>
            <div id="edittabs">
                <ul>
                    <li><a href="#edittabs-base">Base</a></li>
                    <li><a href="#edittabs-servers">Servers</a></li>
                    <li><a href="#edittabs-shardmap">Shard Map</a></li>
                    <li><a href="#edittabs-pointershards">Pointer Shards</a></li>
                </ul>
                <div id="edittabs-base">
                    <fieldset>
                        <legend>Edit <%#Eval("TableGroupName") %></legend>
                        <ol>
                            <li>
                                <asp:Label runat="server" AssociatedControlID="EditWorkloadGroupID">TableGroupID</asp:Label>
                                <asp:DynamicControl runat="server" DataField="TableGroupID" ID="EditWorkloadGroupID" Mode="ReadOnly" />
                            </li>
                            <li>
                                <asp:Label ID="Label1" runat="server" AssociatedControlID="EditWorkloadGroupName">TableGroupName</asp:Label>
                                <asp:DynamicControl runat="server" DataField="TableGroupName" ID="EditWorkloadGroupName" Mode="ReadOnly" />
                            </li>
                            <li>
                                <asp:Label ID="Label2" runat="server" AssociatedControlID="EditVersion">Version</asp:Label>
                                <asp:DynamicControl runat="server" DataField="Version" ID="EditVersion" Mode="ReadOnly" />
                            </li>
                            <li>
                                <asp:Label ID="Label4" runat="server" AssociatedControlID="EditShardType">ShardType</asp:Label>
                                <asp:DynamicControl runat="server" DataField="ShardType" ID="EditShardType" Mode="Edit" />
                            </li>
                            <li>
                                <asp:Label ID="Label5" runat="server" AssociatedControlID="EditTargetShardCount">TargetShardCount</asp:Label>
                                <asp:DynamicControl runat="server" DataField="TargetShardCount" ID="EditTargetShardCount" Mode="Edit" />
                            </li>
                            <li>
                                <asp:Label ID="Label6" runat="server" AssociatedControlID="EditMaxShardCount">MaxShardCount</asp:Label>
                                <asp:DynamicControl runat="server" DataField="MaxShardCount" ID="EditMaxShardCount" Mode="Edit" />
                            </li>
                            <li>
                                <asp:Label ID="Label7" runat="server" AssociatedControlID="EditMaxTenantsPerShard">MaxTenantsPerShard</asp:Label>
                                <asp:DynamicControl runat="server" DataField="MaxTenantsPerShard" ID="EditMaxTenantsPerShard" Mode="Edit" />
                            </li>
                            <li>
                                <asp:Label ID="Label8" runat="server" AssociatedControlID="EditMinShardSizeMB">MinShardSizeMB</asp:Label>
                                <asp:DynamicControl runat="server" DataField="MinShardSizeMB" ID="EditMinShardSizeMB" Mode="Edit" />
                            </li>
                            <li>
                                <asp:Label ID="Label9" runat="server" AssociatedControlID="EditMaxShardSizeMB">MaxShardSizeMB</asp:Label>
                                <asp:DynamicControl runat="server" DataField="MaxShardSizeMB" ID="EditMaxShardSizeMB" Mode="Edit" />
                            </li>
                            <li>
                                <asp:Label ID="Label10" runat="server" AssociatedControlID="EditAllowDeployments">AllowDeployments</asp:Label>
                                <asp:DynamicControl runat="server" DataField="AllowDeployments" ID="EditAllowDeployments" Mode="Edit" />
                            </li>
                        </ol>
                    </fieldset>
                </div>
                <div id="edittabs-servers">
                    <asp:Repeater runat="server" ID="serversRepeater">
                        <HeaderTemplate>
                            <asp:DropDownList ID="editServerList" runat="server" DataSourceID="ServerContext" DataTextField="ServerInstanceName" DataValueField="ServerInstanceName"></asp:DropDownList>
                            <asp:LinkButton Text="Add Server" runat="server" CommandName="AddServer" CommandArgument='<%#String.Format("WorkgroupConfigName={0}", currentItem.TableGroupName) %>' OnCommand="ServerItem_Command"></asp:LinkButton>
                            <ul>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <li><%#Eval("ServerInstanceName") %> (<a runat="server" href='<%#String.Format("~/Servers/Details.aspx?id={0}", Eval("ServerInstanceName")) %>'>Edit</a> | 
                                <asp:LinkButton runat="server" Text="Remove" CommandName="RemoveServer" CommandArgument='<%#String.Format("WorkgroupConfigName={0}&ServerInstanceName={1}", currentItem.TableGroupName, Eval("ServerInstanceName")) %>' OnCommand="ServerItem_Command"></asp:LinkButton>)
                            </li>
                        </ItemTemplate>
                        <FooterTemplate>
                        </ul>
                        </FooterTemplate>
                    </asp:Repeater>
                </div>
                <div id="edittabs-shardmap">
                    <div>
                        <h4>Published Shard Map</h4>
                        <asp:Repeater ID="publishedShardsRepeater" runat="server" OnItemDataBound="publishedShardsRepeater_ItemDataBound">
                            <HeaderTemplate>
                                <table>
                                <tr>
                                    <th>Server</th>
                                    <th>Catalog</th>
                                    <th>Range:Low DistributionKey</th>
                                    <th>Range:High DistributionKey</th>
                                </tr>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr>
                                    <td><%#Eval("ServerInstanceName") %></td>
                                    <td><%#Eval("Catalog") %></td>
                                    <td><%#Eval("LowDistributionKey") %></td>
                                    <td><%#Eval("HighDistributionKey") %></td>
                                </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                <asp:Literal runat="server" ID="emptyMessage" Visible="false">
                                    <tr><td colspan="4" style="text-align: center">No Shards are published.</td></tr>
                                </asp:Literal>
                            </table>
                            </FooterTemplate>
                        </asp:Repeater>
                    </div>
                    <div>
                        <h4>Proposed Shard Map</h4>
                        <asp:Repeater ID="proposedShardsRepeater" runat="server" OnItemDataBound="proposedShardsRepeater_ItemDataBound">
                            <HeaderTemplate>
                                <table>
                                <tr>
                                    <th>Server</th>
                                    <th>Catalog</th>
                                    <th>Range:Low DistributionKey</th>
                                    <th>Range:High DistributionKey</th>
                                </tr>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr>
                                    <td><%#Eval("ServerInstanceName") %></td>
                                    <td><%#Eval("Catalog") %></td>
                                    <td><%#Eval("LowDistributionKey") %></td>
                                    <td><%#Eval("HighDistributionKey") %></td>
                                </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                <asp:Literal runat="server" ID="emptyMessage" Visible="false">
                                    <tr><td colspan="4" style="text-align: center">No Shards are proposed for the new map yet.</td></tr>
                                </asp:Literal>
                            </table>
                            </FooterTemplate>
                        </asp:Repeater>


                        <h5>Publishing Steps:</h5>
                        <ol>
                            <li>
                                <asp:LinkButton runat="server" CommandArgument='<%#String.Format("WorkgroupConfigName={0}", currentItem.TableGroupName) %>' CommandName="UpdateMap" OnCommand="Shard_Command">Calculate Shard Map</asp:LinkButton>
                                : Calculates a new Shardmap based on updated server & Workload Group Config values.</li>
                            <li>
                                <asp:LinkButton runat="server" CommandArgument='<%#String.Format("WorkgroupConfigName={0}", currentItem.TableGroupName) %>' CommandName="DeployMap" OnCommand="Shard_Command">Deploy to Servers</asp:LinkButton>
                                : Deploys new shards to servers but does not update the published map.</li>
                            <li>
                                <asp:LinkButton runat="server" CommandArgument='<%#String.Format("WorkgroupConfigName={0}", currentItem.TableGroupName) %>' CommandName="PublishMap" OnCommand="Shard_Command">Publish Shard Map</asp:LinkButton>
                                : Calculates Tenant Moves, Publish's new Map, Moves Tenants, and drops databases that are no longer used.</li>
                        </ol>
                    </div>
                </div>
                <div id="edittabs-pointershards">
                    <asp:Repeater runat="server" ID="pointerShardRepeater">
                        <HeaderTemplate>
                            <fieldset>
                                <ol>
                                    <li>
                                        <asp:Label runat="server" AssociatedControlID="newPointerShardList">Server</asp:Label>
                                        <asp:DropDownList ID="newPointerShardList" runat="server" DataSourceID="ServerContext" DataTextField="ServerInstanceName" DataValueField="ServerInstanceName">
                                        </asp:DropDownList><asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ErrorMessage="*" ControlToValidate="newPointerShardList" ClientIDMode="AutoID" EnableClientScript="true" ValidationGroup="AddPointerShard" />
                                    </li>
                                    <li>
                                        <asp:Label runat="server" AssociatedControlID="newCatalogName">Catalog Name</asp:Label>
                                        <asp:TextBox ID="newCatalogName" runat="server" />
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ErrorMessage="*" ControlToValidate="newCatalogName" ClientIDMode="AutoID" EnableClientScript="true" ValidationGroup="AddPointerShard" />
                                    </li>
                                    <li>
                                        <asp:Label runat="server" AssociatedControlID="newDescription">Description</asp:Label>
                                        <asp:TextBox ID="newDescription" runat="server" />
                                    </li>
                                </ol>
                            </fieldset>
                            <asp:LinkButton CausesValidation="true" ValidationGroup="AddPointerShard" ID="LinkButton1" Text="Add Shard" runat="server" CommandName="AddShard" CommandArgument='<%#String.Format("WorkgroupConfigName={0}", currentItem.TableGroupName) %>' OnCommand="PointerShard_Command"></asp:LinkButton>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <fieldset>
                                <ol>
                                    <li>
                                        <asp:Label runat="server" AssociatedControlID="pointerShardServer">Server</asp:Label>
                                        <asp:Label ID="pointerShardServer" runat="server" Text='<%#Eval("ServerInstanceName") %>' />
                                    </li>
                                    <li>
                                        <asp:Label runat="server" AssociatedControlID="pointerCatalogName">Catalog Name</asp:Label>
                                        <asp:Label ID="pointerCatalogName" runat="server" Text='<%#Eval("Catalog") %>' />
                                    </li>
                                    <li>
                                        <asp:Label runat="server" AssociatedControlID="description">Description</asp:Label>
                                        <asp:TextBox ID="description" runat="server" Text='<%#Eval("Description") %>' />
                                    </li>
                                    <asp:LinkButton ID="LinkButton1" Text="Update" runat="server" CommandName="UpdateShard" CommandArgument='<%#String.Format("WorkgroupConfigName={0}&PointerShardID={1}", currentItem.TableGroupName, Eval("PointerShardID")) %>' OnCommand="PointerShard_Command"></asp:LinkButton>
                                    | 
                            
                                    <asp:LinkButton ID="LinkButton2" Text="Delete" runat="server" CommandName="DeleteShard" CommandArgument='<%#String.Format("WorkgroupConfigName={0}&PointerShardID={1}", currentItem.TableGroupName, Eval("PointerShardID")) %>' OnCommand="PointerShard_Command"></asp:LinkButton>
                                </ol>
                            </fieldset>
                            <hr />
                        </ItemTemplate>
                        <FooterTemplate>
                        </FooterTemplate>
                    </asp:Repeater>
                    <h5>Publishing Steps:</h5>
                    <ol>
                        <li>
                            <asp:LinkButton ID="LinkButton4" runat="server" CommandArgument='<%#String.Format("WorkgroupConfigName={0}", currentItem.TableGroupName) %>' CommandName="DeployMap" OnCommand="PointerShard_Command">Deploy to Servers</asp:LinkButton>
                            : Deploys new shards to servers</li>
                        <li>
                            <asp:LinkButton ID="LinkButton3" runat="server" CommandArgument='<%#String.Format("WorkgroupConfigName={0}", currentItem.TableGroupName) %>' CommandName="MoveTenant" OnCommand="PointerShard_Command">Move Tenants</asp:LinkButton>
                            : Enter Tenant IDs to move to a Pointer Shard</li>
                    </ol>
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
                    <li><a href="#readtabs-base">Base</a></li>
                    <li><a href="#readtabs-servers">Servers</a></li>
                    <li><a href="#readtabs-shardmap">Shard Map</a></li>
                    <li><a href="#readtabs-pointershards">Pointer Shards</a></li>
                </ul>

                <div id="readtabs-base">
                    <fieldset>
                        <legend>View <%#Eval("TableGroupName") %></legend>
                        <ol>
                            <li>
                                <asp:Label ID="Label11" runat="server" AssociatedControlID="EditWorkloadGroupID">TableGroupID</asp:Label>
                                <asp:DynamicControl runat="server" DataField="TableGroupID" ID="EditWorkloadGroupID" Mode="ReadOnly" />
                            </li>
                            <li>
                                <asp:Label ID="Label1" runat="server" AssociatedControlID="EditWorkloadGroupName">TableGroupName</asp:Label>
                                <asp:DynamicControl runat="server" DataField="TableGroupName" ID="EditWorkloadGroupName" Mode="ReadOnly" />
                            </li>
                            <li>
                                <asp:Label ID="Label2" runat="server" AssociatedControlID="EditVersion">Version</asp:Label>
                                <asp:DynamicControl runat="server" DataField="Version" ID="EditVersion" Mode="ReadOnly" />
                            </li>
                            <li>
                                <asp:Label ID="Label4" runat="server" AssociatedControlID="EditShardType">ShardType</asp:Label>
                                <asp:DynamicControl runat="server" DataField="ShardType" ID="EditShardType" Mode="ReadOnly" />
                            </li>
                            <li>
                                <asp:Label ID="Label5" runat="server" AssociatedControlID="EditTargetShardCount">TargetShardCount</asp:Label>
                                <asp:DynamicControl runat="server" DataField="TargetShardCount" ID="EditTargetShardCount" Mode="ReadOnly" />
                            </li>
                            <li>
                                <asp:Label ID="Label6" runat="server" AssociatedControlID="EditMaxShardCount">MaxShardCount</asp:Label>
                                <asp:DynamicControl runat="server" DataField="MaxShardCount" ID="EditMaxShardCount" Mode="ReadOnly" />
                            </li>
                            <li>
                                <asp:Label ID="Label7" runat="server" AssociatedControlID="EditMaxTenantsPerShard">MaxTenantsPerShard</asp:Label>
                                <asp:DynamicControl runat="server" DataField="MaxTenantsPerShard" ID="EditMaxTenantsPerShard" Mode="ReadOnly" />
                            </li>
                            <li>
                                <asp:Label ID="Label8" runat="server" AssociatedControlID="EditMinShardSizeMB">MinShardSizeMB</asp:Label>
                                <asp:DynamicControl runat="server" DataField="MinShardSizeMB" ID="EditMinShardSizeMB" Mode="ReadOnly" />
                            </li>
                            <li>
                                <asp:Label ID="Label9" runat="server" AssociatedControlID="EditMaxShardSizeMB">MaxShardSizeMB</asp:Label>
                                <asp:DynamicControl runat="server" DataField="MaxShardSizeMB" ID="EditMaxShardSizeMB" Mode="ReadOnly" />
                            </li>
                            <li>
                                <asp:Label ID="Label10" runat="server" AssociatedControlID="EditAllowDeployments">AllowDeployments</asp:Label>
                                <asp:DynamicControl runat="server" DataField="AllowDeployments" ID="EditAllowDeployments" Mode="ReadOnly" />
                            </li>
                        </ol>
                    </fieldset>
                </div>

                <div id="readtabs-servers">
                    <asp:Repeater ID="serversRepeater" runat="server">
                        <HeaderTemplate>
                            <ul>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <li><%#Eval("ServerInstanceName") %></li>
                        </ItemTemplate>
                        <FooterTemplate>
                        </ul>
                        </FooterTemplate>
                    </asp:Repeater>
                </div>
                <div id="readtabs-shardmap">
                    <div>
                        <h4>Published Shard Map</h4>
                        <asp:Repeater ID="publishedShardsRepeater" runat="server" OnItemDataBound="publishedShardsRepeater_ItemDataBound">
                            <HeaderTemplate>
                                <table>
                                <tr>
                                    <th>Server</th>
                                    <th>Catalog</th>
                                    <th>Range:Low DistributionKey</th>
                                    <th>Range:High DistributionKey</th>
                                </tr>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr>
                                    <td><%#Eval("ServerInstanceName") %></td>
                                    <td><%#Eval("Catalog") %></td>
                                    <td><%#Eval("LowDistributionKey") %></td>
                                    <td><%#Eval("HighDistributionKey") %></td>
                                </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                <asp:Literal runat="server" ID="emptyMessage" Visible="false">
                                    <tr><td colspan="4" style="text-align: center">No Shards are published.</td></tr>
                                </asp:Literal>
                            </table>
                            </FooterTemplate>
                        </asp:Repeater>
                    </div>
                    <div>
                        <h4>Proposed Shard Map</h4>
                        <asp:Repeater ID="proposedShardsRepeater" runat="server" OnItemDataBound="proposedShardsRepeater_ItemDataBound">
                            <HeaderTemplate>
                                <table>
                                <tr>
                                    <th>Server</th>
                                    <th>Catalog</th>
                                    <th>Range:Low DistributionKey</th>
                                    <th>Range:High DistributionKey</th>
                                </tr>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr>
                                    <td><%#Eval("ServerInstanceName") %></td>
                                    <td><%#Eval("Catalog") %></td>
                                    <td><%#Eval("LowDistributionKey") %></td>
                                    <td><%#Eval("HighDistributionKey") %></td>
                                </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                <asp:Literal runat="server" ID="emptyMessage" Visible="false">
                                    <tr><td colspan="4" style="text-align: center">No Shards are proposed for the new map yet.</td></tr>
                                </asp:Literal>
                            </table>
                            </FooterTemplate>
                        </asp:Repeater>


                        <h5>Publishing Steps:</h5>
                        <ol>
                            <li>Calculate Shard Map : Calculates a new Shardmap based on updated server & Workload Group Config values.</li>
                            <li>Deploy to Servers : Deploys new shards to servers but does not update the published map.</li>
                            <li>Publish Shard Map : Calculates Tenant Moves, Publish's new Map, Moves Tenants, and drops databases that are no longer used.</li>
                        </ol>
                    </div>
                </div>
                <div id="readtabs-pointershards">
                    <asp:Repeater runat="server" ID="pointerShardRepeater">
                        <HeaderTemplate>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <fieldset>
                                <ol>
                                    <li>
                                        <asp:Label runat="server" AssociatedControlID="pointerShardServer">Server</asp:Label>
                                        <asp:Label ID="pointerShardServer" runat="server" Text='<%#Eval("ServerInstanceName") %>' />
                                    </li>
                                    <li>
                                        <asp:Label runat="server" AssociatedControlID="pointerCatalogName">Catalog Name</asp:Label>
                                        <asp:Label ID="pointerCatalogName" runat="server" Text='<%#Eval("Catalog") %>' />
                                    </li>
                                    <li>
                                        <asp:Label runat="server" AssociatedControlID="description">Description</asp:Label>
                                        <asp:Label ID="description" runat="server" Text='<%#Eval("Description") %>' />
                                    </li>
                                </ol>
                            </fieldset>
                            <hr />
                        </ItemTemplate>
                        <FooterTemplate>
                        </FooterTemplate>
                    </asp:Repeater>
                    <h5>Publishing Steps:</h5>
                    <ol>
                        <li>
                            Deploy to Servers : Deploys new shards to servers</li>
                        <li>
                            <asp:LinkButton ID="LinkButton3" runat="server" CommandArgument='<%#String.Format("WorkgroupConfigName={0}", currentItem.TableGroupName) %>' CommandName="MoveTenant" OnCommand="PointerShard_Command">Move Tenants</asp:LinkButton>
                            : Enter Tenant IDs to move to a Pointer Shard</li>
                    </ol>
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
        <FooterTemplate>
            <a runat="server" href="~/WorkloadGroupConfigs/Default.aspx">&lt;- Back</a>
        </FooterTemplate>
    </asp:FormView>
</asp:Content>