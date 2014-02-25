<%@ Page Title="" Language="VB" MasterPageFile="~/pppMaster.master" AutoEventWireup="false" CodeFile="GearsSampleAuthorization.aspx.vb" Inherits="GearsSampleAuthorization" %>
<%@ Register src="./UnitItem.ascx" tagname="unitItem" tagprefix="ui" %>
<%@ MasterType VirtualPath="pppMaster.master" %>

<asp:Content ID="clientHead" ContentPlaceHolderID="pppHead" Runat="Server" ClientIDMode=Static>
    <title>Authorization Control In GearsFramework</title>
</asp:Content>

<asp:Content ID="clientPageTitle" ContentPlaceHolderID="pppPageTitle" Runat="Server" ClientIDMode=Static>
    GFWの権限制御機能
</asp:Content>

<asp:Content ID="clientOptionMenu" ContentPlaceHolderID="pppOptionMenu" Runat="Server" ClientIDMode=Static>
        <ul class="ppp-menu-links">
            <li class="ppp-menu-links"><a href="default.aspx" class="ppp-link-item">Gearsトップ</a></li>
        </ul>
</asp:Content>

<asp:Content ID="clientCenter" ContentPlaceHolderID="pppContent" Runat="Server" ClientIDMode=Static>
    <div class="document-body">
    ASP.NETでは標準で権限管理の機能(Membership/RoleProvider)が存在しますが、単純なページへのアクセス可否だけでなく、
    ページ内の操作についても権限で管理を行いたい場合があります。<br/>
    <br/>
    <i>GFW</i>では、コントロールに属性を設定することで、ロールによるコントロールの有効/無効を切り替えることが出来ます。<br/>
    具体的には、以下のように設定を行います。<br/>
    <br/>

    <div class="ppp-indent" style="width:800px;">
        <pre class="ppp-box even" style="font-style:italic;font-size:12px">

 ＜asp:Button id="btnEdit" runat="server" Text ="編集実行" AuthorizationAllow="EDIT_OK" /＞
        </pre>
    </div>
    <br/>
    AuthorizationAllowの属性にロールを設定することで、
    そこに設定されたロールを保持しないユーザーの場合コントロールが無効(Enable=False)となります。<br/>
    この機能を使用することで、コードビハインドを汚すことなくページ内での権限管理が可能になります。<br/>
    <br/>
    また、追加属性 RoleEvalAction="VISIBLE"("ENABLE") を設定することで、権限がない場合の表示スタイルを非表示／無効に切り替えることが出来ます。<br/>
    <br/>
    </div>
</asp:Content>
