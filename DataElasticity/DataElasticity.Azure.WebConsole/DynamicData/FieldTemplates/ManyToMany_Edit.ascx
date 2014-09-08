<%@ Control Language="C#" CodeBehind="ManyToMany_Edit.ascx.cs" Inherits="Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole.DynamicData.FieldTemplates.ManyToMany_EditField" %>

<asp:CheckBoxList ID="CheckBoxList1" runat="server" RepeatColumns="3" ondatabound="CheckBoxList1_DataBound"/>