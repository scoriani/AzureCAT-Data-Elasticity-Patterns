<%@ Control Language="C#" CodeBehind="Url.ascx.cs" Inherits="Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole.DynamicData.FieldTemplates.UrlField" %>

<asp:HyperLink ID="HyperLinkUrl" runat="server" Text="<%# FieldValueString %>" Target="_blank" />