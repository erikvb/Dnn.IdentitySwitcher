<%@ Control Language="vb" AutoEventWireup="false" CodeFile="Settings.ascx.vb" Inherits="Apollo.DNN.Modules.UserSwitcher.Settings" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<table cellspacing="0" cellpadding="2" border="0" summary="ModuleName Settings Design Table" id="tblDesignTable" runat="server">
    <tr>
        <td class="SubHead" width="150"><dnn:label id="plIncludeHostUser" runat="server" controlname="cbIncludeHostUser" suffix=":"></dnn:label></td>
        <td valign="bottom" >
            <asp:CheckBox ID="cbIncludeHostUser" runat="server" />
        </td>
    </tr>
</table>
