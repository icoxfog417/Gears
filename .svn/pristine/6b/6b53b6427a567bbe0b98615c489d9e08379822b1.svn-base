<%@ Page Title="" Language="VB" MasterPageFile="~/pppMaster.master" AutoEventWireup="false" CodeFile="GearsSampleStyle.aspx.vb" Inherits="GearsSampleStyle" %>
<%@ Register src="UnitItem.ascx" tagname="unitItem" tagprefix="ui" %>
<%@ MasterType VirtualPath="pppMaster.master" %>

<asp:Content ID="clientHead" ContentPlaceHolderID="pppHead" Runat="Server" ClientIDMode=Static>
    <title>Style In GearsFramework</title>
    <script>
        function callName(){
            var name = document.getElementById("txtName").value;
            if (name != "") {
                alert("入力された名前は " + name + " です！");
            } else {
                alert("まだ名前の入力がありません");
            }
        }
    </script>
</asp:Content>

<asp:Content ID="clientPageTitle" ContentPlaceHolderID="pppPageTitle" Runat="Server" ClientIDMode=Static>
    GFWのスタイル
</asp:Content>

<asp:Content ID="clientOptionMenu" ContentPlaceHolderID="pppOptionMenu" Runat="Server" ClientIDMode=Static>
        <ul class="ppp-menu-links">
            <li class="ppp-menu-links"><a href="default.aspx" class="ppp-link-item">Gearsトップ</a></li>
        </ul>
</asp:Content>

<asp:Content ID="clientCenter" ContentPlaceHolderID="pppContent" Runat="Server" ClientIDMode=Static>
    <div class="document-body">
    <i>GFW</i>では、コントロールの配置だけでアプリケーションを作成できるよう、
    スタイルとコントロールを1セットにしたUnitItemというユーザーコントロールを提供しています。<br/>
    <br/>
    使用方法は、以下の通りです。※事前にユーザーコントロールをRegisterしておく必要があります。
    これは通常のユーザーコントロールを使用する再の手続きと変わりません。
    <div class="ppp-indent" style="width:800px;">
        <pre class="ppp-box even" style="font-style:italic;font-size:12px">
Pageディレクティブにて...
＜%@ Register src="UnitItem.ascx" tagname="unitItem" tagprefix="ui" %＞

配置時
＜ui:unitItem ID="Name" CtlKind="TXT" runat="server" LabelText="テキスト" /＞
        </pre>
    </div>
    配置にあたり重要な属性は以下３点になります。<br/>
    <ul>
        <li><u><b>ID</b></u><br/>
            UnitItemのIDには、データソースクラス名を指定します。
            これは、実際レンダリングされるコントロールのIDがCtlKindと組み合わせたものになるためです。<br/>
            例えば、CtlKindが"TXT"で、IDが"Name"である場合、レンダリングされたコントロールのIDは"txtName"になります。<br/>
            <br/>
        </li>

        <li><u><b>CtlKind</b></u><br/>
            コントロールの種別を英字3文字で指定します。指定可能なコントロールは下記になります。<br/>
            これを小文字にしたものが、IDに指定した文字列の頭に付与されます。<br/>
            <table>
                <tr><td style="width:70px;">値</td><td></td><td>コントロール</td></tr>
                <tr><td>・TXT</td><td>：</td><td>TextBox</td></tr>
                <tr><td>・TXTA</td><td>：</td><td>TextBox(Multiline)</td></tr>
                <tr><td>・DDL</td><td>：</td><td>DropDownList</td></tr>
                <tr><td>・RBL</td><td>：</td><td>RadioButtonList</td></tr>
                <tr><td>・CBL</td><td>：</td><td>CheckBoxList</td></tr>
                <tr><td>・CHB</td><td>：</td><td>CheckBox</td></tr>
                <tr><td>・LBL</td><td>：</td><td>Label</td></tr>
            </table>
             <br/>
        </li>
        <li><u><b>LabelText</b></u><br/>
            ラベルのテキストを設定します<br/>
            <br/>
        </li>

    </ul>
    これ以外のオプションとしては、SearchAction属性にjavaScriptの関数を指定することで、ボタンつきのコントロールにすることが出来ます。
    <br/>

    実際に配置してみたものが、以下になります。<br>
    <br>
     <div class="ppp-indent" style="width:800px;">
        <div class="ppp-box even">
            <b>ユーザーコントロール配置の例(float)</b>
            <br/>
            <ui:unitItem ID="Name" CtlKind="TXT" runat="server" LabelText="名前" SearchAction="callName()" />
            <ui:unitItem ID="Job" CtlKind="TXT" runat="server" LabelText="職種" Width=80 />
            <ui:unitItem ID="COMP_UNIT" CtlKind="RBL" runat="server" LabelText="所属事業部" />
            <br style="clear:both"/>
            <ui:unitItem ID="Comment" CtlKind="TXTA" runat="server" LabelText="幅広いコメントを受け付けているのである" Width=300 Height=50 />
            <br style="clear:both"/>
        </div>
        <div class="ppp-box even">
            <b>ユーザーコントロール配置の例(horizon)</b>
            <br/>
            <ui:unitItem ID="Name__H" CtlKind="TXT" runat="server" LabelText="名前" SearchAction="callName()" IsHorizontal="True" />
            <ui:unitItem ID="Job__H" CtlKind="TXT" runat="server" LabelText="職種" Width=80 IsHorizontal="True"/>
            <ui:unitItem ID="COMP_UNIT__H" CtlKind="RBL" runat="server" LabelText="所属事業部" IsHorizontal="True"/>
            <ui:unitItem ID="Comment__H" CtlKind="TXTA" runat="server" LabelText="幅広いコメントを受け付けているのである" Width=300 Height=50 IsHorizontal="True"/>
        </div>
    </div>
    <br/>
    上記のように、ラベルとコントロールが整理されて配置されます。注意点としては、
    floatを使用しているため最後にclear:bothのスタイルを指定したdivかbrを配置する必要がある点です。<br/>
    その他スタイルについてはCSSクラスで指定されているため、文字色や枠線の色などは簡単に変えることが出来ます。<br/>
    <br/>
    なお、コードビハインドでリレーションなど登録する際は、このユーザーコントロールから実際のコントロールを取り出す必要があります。<br/>
        <div class="ppp-indent" style="width:800px;">
        <pre class="ppp-box even" style="font-style:italic;font-size:12px">

addRelation(ddlCOMP_UNIT, COMP_GRP.getControl(Of DropDownList))
        </pre>
    </div>
    getControlを使用し、中身のコントロールを取り出せます。また、ジェネリクスを指定することでコントロールの型を指定することが出来ます。<br/>
    <br/>
    <br/>
    <br/>



    </div>
</asp:Content>

