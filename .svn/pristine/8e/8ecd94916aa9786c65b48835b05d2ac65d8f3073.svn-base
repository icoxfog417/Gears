<%@ Control Language="VB" AutoEventWireup="false" CodeFile="UnitItem.ascx.vb" Inherits="UnitItem" %>
<table <%= TableStyle()%>" >
    <tr >
        <td class="gs-layout-label" <%=If(IsHorizontal,"valign=""top""","")%> >
			<%-- Gearsコントロール対象外にするために、あえてidのネーミングルールをはずす --%>
            <asp:Label id="labelItem" runat="server" Text="" CssClass=gs-label ClientIDMode=Static><%= LabelText%></asp:Label>
        </td>
    <%If Not IsHorizontal Then%>
    </tr>
    <tr>
    <%End If%>
        <td class="gs-layout-ctrl" >
            <asp:Panel id="pnlCtlFolder" runat="server" ClientIDMode=Predictable style="display:inline"> 
            </asp:Panel>
            <%If SearchAction <> "" Then%>
                <button id="btnSearchCode<%=Me.ID %>" class="gs-for-search-button" type="button" value="＊" onclick="<%=SearchAction %>" />＊</button>
            <%End If%>

    <%If pnlCtlFolder.Controls.Count > 1 Then%>
        <%= UnitItem.closing()%>
    <%End If%>