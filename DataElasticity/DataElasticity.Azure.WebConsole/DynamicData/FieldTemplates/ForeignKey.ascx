<%@ Control Language="C#" CodeBehind="ForeignKey.ascx.cs" Inherits="Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole.DynamicData.FieldTemplates.ForeignKeyField" %>

<asp:HyperLink ID="HyperLink1" runat="server"
               Text="<%# GetDisplayString() %>"
               NavigateUrl="<%# GetNavigateUrl() %>"  />