<%@ Page Language="VB" MasterPageFile="pppMaster.master" AutoEventWireup="false" CodeFile="GearsSample.aspx.vb" Inherits="GearsSample" %>
<%@ Register src="UnitItem.ascx" tagname="unitItem" tagprefix="ui" %>
<%@ MasterType VirtualPath="pppMaster.master" %>

<asp:Content id="clientHead" ContentPlaceHolderID="pppHead" Runat="Server" ClientIDMode=Static>
    <title>Demo of GearsFramework</title>
    <script language="javascript">
        $(function () {
            //※Comboboxがページ内にない場合不要
            //AjaxToolKitのComboBoxのバグを修正するためのスクリプト
            adjustCombobox();

            //アコーディオン表示用処理　※ページ内にアコーディオンがない場合不要
            //adjustComboboxが終わるまで待ってから処理(IE6の場合上から順次実行されないため、setTimeoutで時間をずらす)
            if ('<%=hdnMode.Value%>' == "U") {
                //処理区分がU(更新)なら詳細画面を出す
                setTimeout(function () { makePPPSwitchArea(0) }, 50);
            } else {
                //それ以外の場合、検索画面を表示
                setTimeout(function () { makePPPSwitchArea(1) }, 50);
            }

        })
    </script>
</asp:Content>

<asp:Content id="clientPageTitle" ContentPlaceHolderID="pppPageTitle" Runat="Server" ClientIDMode=Static>
    GFW総集編
</asp:Content>

<asp:Content id="clientOptionMenu" ContentPlaceHolderID="pppOptionMenu" Runat="Server" ClientIDMode=Static>
<!-- メニューコンテンツ表示 -->
            <ul class="ppp-menu-links">
                <li class="ppp-menu-links"><a href="#" class="ppp-link-item">ヘルプ</a></li>
                <li class="ppp-menu-links"><a href="default.aspx" class="ppp-link-item">メニュー</a></li>
            </ul>
</asp:Content>

<asp:Content id="clientCenter" ContentPlaceHolderID="pppContent" Runat="Server" ClientIDMode=Static>
<!-- メインコンテンツ表示 -->
    <asp:Panel ID="pnlSAREA" runat="server" CssClass=ppp-switch-area Width=800>
        <h3 class="ppp-switch-header">フォームエリア</h3>
        <asp:Panel id="pnlGFORM" runat="server" CssClass="ppp-include-combo-area">

        <!-- コントロールの配置に当たっては、ユーザーコントロールの使用により簡単に書ける -->
        <ui:unitItem ID="EMPNO__KEY" CtlKind="TXT" runat="server" LabelText="※従業員番号" CssClass="gears-GRequired gears-GNumeric gears-GByteLength_Length_4 " />
        <ui:unitItem ID="ENAME__FORM" CtlKind="TXT" runat="server" LabelText="※名前" CssClass="gears-GRequired"/>
		<asp:HiddenField ID="hdnCOMP_UNIT" runat="server" />
        <ui:unitItem ID="COMP_UNIT_TXT__GCON" CtlKind="LBL" runat="server" LabelText="販売組織" />
        <ui:unitItem ID="COMP_GRP__FORM" CtlKind="DDL" runat="server" LabelText="営業Ｇ" />		
        <br style="clear:both"/>

        <ui:unitItem ID="JOB__FORM" runat="server" LabelText="職位" />
            <ajaxToolkit:ComboBox id="cbxJOB__FORM" runat="server" CssClass="ppp-combo" RenderMode=Block ClientIDMode=AutoID >
			</ajaxToolkit:ComboBox>
        <%= UnitItem.closing%>
        <ui:unitItem ID="MK_FLG" CtlKind="CHB" runat="server" LabelText="無効フラグ" />

        <br style="clear:both"/>
        <br/>
        <asp:Button ID="btnUpdate" runat="server" Text="更新" />
        <asp:Button ID="btnDelete" runat="server" Text="削除" />
        </asp:Panel>


        <h3 class="ppp-switch-header">フィルタエリア</h3>
        <asp:Panel id="pnlGFilter" runat="server"  >
            <asp:UpdatePanel id="udpArea" runat="server" UpdateMode=Conditional>
                <ContentTemplate>
                    <ui:unitItem ID="COMP_UNIT__FIL" CtlKind="RBL" runat="server" LabelText="販売組織" AutoPostBack=True IsHorizontal="True"/>
                    <ui:unitItem ID="COMP_GRP__FIL" CtlKind="DDL" runat="server" LabelText="営業Ｇ" IsNeedAll=True IsHorizontal="True"/>
                </ContentTemplate>
            </asp:UpdatePanel>            
            <ui:unitItem ID="ENAME__FIL" CtlKind="TXT" runat="server" LabelText="名前(※LIKE検索 オプションにOperatorを指定)" Operator="LIKE" IsHorizontal="True"/>

            <br style="clear:both"/>

            <div width="100%" style="text-align:right">
                <asp:Button id="btnSearch" runat="server" Text=" 検索実行 " />
            </div>
        </asp:Panel>

    </asp:Panel>
    
    <br/>
    <asp:Panel id="pnlList" runat="server"  >
            <asp:GridView id="grvData" runat="server" 
                AutoGenerateSelectButton=True 
                AutoGenerateColumns=False
                CssClass=ppp-table 
                HeaderStyle-CssClass=ppp-table-head EnableViewState=True RowStyle-CssClass=ppp-table-odd AlternatingRowStyle-CssClass=ppp-table-even
                DataKeyNames=EMPNO>
                <Columns>
                    <asp:BoundField DataField="EMPNO" HeaderText="従業員番号">
                    </asp:BoundField>
                    <asp:BoundField DataField="COMP_UNIT_TXT" HeaderText="販売組織">
                    </asp:BoundField>
                    <asp:BoundField DataField="COMP_GRP_TXT" HeaderText="営業Ｇ">
                    </asp:BoundField>
                    <asp:BoundField DataField="ENAME" HeaderText="名前" HeaderStyle-Width="150px">
                    </asp:BoundField>
                    <asp:BoundField DataField="JOB" HeaderText="職位">
                    </asp:BoundField>
                    <asp:BoundField DataField="MK_FLG" HeaderText="無効フラグ">
                    </asp:BoundField>
                </Columns>
            </asp:GridView>
        </asp:Panel>
    <br/>
    <br/>
    <asp:Label id="lblMsg" runat="server" Text="" CssClass="ppp-msg error"></asp:Label>
    <br/>
    <asp:Label id="lblLog" runat="server" Text="" CssClass="ppp-msg"></asp:Label>
    <asp:HiddenField ID="hdnMode" runat="server" />

 </asp:Content>