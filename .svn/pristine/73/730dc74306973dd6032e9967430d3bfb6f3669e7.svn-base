<%@ Page Language="VB"  MasterPageFile="pppMaster.master" AutoEventWireup="false" CodeFile="GearsSampleValidation.aspx.vb" Inherits="GearsSampleValidation" %>
<%@ Register src="./UnitItem.ascx" tagname="unitItem" tagprefix="ui" %>
<%@ MasterType VirtualPath="pppMaster.master" %>

<asp:Content id="clientHead" ContentPlaceHolderID="pppHead" Runat="Server" ClientIDMode=Static>
    <title>Validation on GearsFramework</title>

</asp:Content>

<asp:Content id="clientPageTitle" ContentPlaceHolderID="pppPageTitle" Runat="Server" ClientIDMode=Static>
    GFWのバリデーション機能
</asp:Content>

<asp:Content id="clientOptionMenu" ContentPlaceHolderID="pppOptionMenu" Runat="Server" ClientIDMode=Static>
            <ul class="ppp-menu-links">
                <li class="ppp-menu-links"><a href="default.aspx" class="ppp-link-item">Gearsトップ</a></li>
            </ul>
</asp:Content>

<asp:Content id="clientCenter" ContentPlaceHolderID="pppContent" Runat="Server" ClientIDMode=Static>
<div class="document-body">

<i>GFW</i>におけるバリデーションは、<b>「コントロールのデータ型に基づくスタイル(CSS)設定」</b>を行うことで実現されます。<br/>
これは、データ型が決まれば表示のスタイル・必要なバリデーションが同時に決まると考えられるためです。
<i>GFW</i>では、このデータ型に対する「スタイル・バリデーション処理のセット」をアトリビュートと呼んでおり、
それを実現しているクラスをアトリビュートクラスとしています。
<br/>
<br/>
具体的なアトリビュートの適用方法は以下のようになります。<br/>
<br/>
<div class="ppp-indent" style="width:800px;">
    <div class="ppp-box odd" style="font-weight:bold">アトリビュートの適用方法</div>
    <div class="ppp-box even" >
        <b>class = gears-アトリビュートクラス名_プロパティ1_設定値1_プロパティ2_設定値2・・・</b><br/>
        <br/>
        例："gears-GDate_Format_yyyyMMdd" → アトリビュートクラスGDateで、プロパティFormatに"yyyyMMdd"を設定しバリデート<br/>
        複数設定する際は、通常のクラス指定同様半角空白でつなげる。<br/>
        例："gears-GNumber gears-GByteLength_Length_5" → GNumberとGByteLength両方でバリデート<br/>
    </div>
</div>

<br/>
CSS定義に指定されたクラス/プロパティの指定は、レンダリング時にはgs-numberやgs-dateといったCSSクラスに変換されます。<br/>
どのクラスをどのCSSクラスに変換するかは、アトリビュートクラス内に設定されています。<br/>
<br/>
実際アトリビュートを適用したコントロールの例は以下になります。バリデーションと同時に、
数値入力の場合には右寄せのスタイルが適用されていることに注意してください。<br/>
<br/>

<div class="ppp-indent" style="width:800px;">
    <div class="ppp-box odd" style="font-weight:bold">バリデーション設定例</div>
    <div class="ppp-box even" >
        <asp:UpdatePanel ID="updV1" runat="server" UpdateMode=Conditional>
            <ContentTemplate>

            <asp:Panel ID="pnlValidation1" runat="server">
                <table class="ppp-table">
                    <tr >
                        <th>項目</th>
                        <th>入力</th>
                        <th>設定スタイル(Css)</th>
                    </tr>
                    <tr>
                        <td>数値</td>
                        <td><asp:TextBox ID=txtVNumber1 CssClass="gears-GNumeric" runat="server"></asp:TextBox></td>
                        <td>gears-GNumeric</td>
                    </tr>
                    <tr>
                        <td>日付(yyyyMMdd)</td>
                        <td><asp:TextBox ID=txtVDate1 CssClass="gears-GDate_Format_yyyyMMdd" runat="server"></asp:TextBox></asp:TextBox></td>
                        <td>gears-GDate_Format_yyyyMMdd</td>
                    </tr>
                    <tr>
                        <td>文字列(最大10文字)</td>
                        <td><asp:TextBox ID=txtVNumber2 CssClass="gears-GByteLengthBetween_MinLength_0_Length_10" runat="server"></asp:TextBox></td>
                        <td>gears-GByteLengthBetween_MinLength_0_Length_10</td>
                    </tr>
                    <tr>
                        <td>必須でB始まり</td>
                        <td><asp:TextBox ID=txtVString1 CssClass="gears-GRequired gears-GStartWith_Prefix_B" runat="server"></asp:TextBox></td>
                        <td>gears-GRequired gears-GStartWith_Prefix_B</td>
                    </tr>
                </table>
                <asp:Label ID="lblVResult1" runat="server" Text="Label" ></asp:Label><br/>
                <br/>
                <asp:Button ID="btnValidation1" runat="server" Text="バリデーション実行" /><br/>
            </asp:Panel>

            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnValidation1" EventName="Click"/>
            </Triggers>
            
        </asp:UpdatePanel>
    </div>
</div>
<br/>
以上がバリデーション機能の使い方となります。これ以降では、実装方法の詳細について解説します。<br/>
<br/>

<h3 class="caption">GFW内でのバリデーション処理の実装について</h3>

<i>GFW</i>ではスタイルとバリデーション処理をセットとしてとらえており、これを<i>Attribute</i>(実装クラスはGearsAttribute)と定義しています。<br/>
GearsAttributeには、CssClassプロパティによるスタイル設定と、ASP.NET標準のIValidatorを実装することによるバリデーション処理が含まれます。
新規のバリデーション処理を追加する際は、このGearsAttributeを継承してアトリビュートクラスを作成することになります。
<br/><br/>
<i>Attribute</i>は適用対象となるコントロールとひもつけられることになりますが、<i>GFW</i>ではこの「アトリビュートの適用対象」を<i>Validatee</i>として定義しており、
<i>Attribute</i>と<i>Validatee</i>をセットにしたものを<i>AttributeHolder</i>としています。<br/>
<br/>
実際の処理は<i>Validatee</i>であるコントロールのCSSから<i>Attribute</i>を生成し、この２つから<i>AttributeHolder</i>オブジェクトを作成、バリデーションを実行、
という流れになります(なお、Attributeは普通にコンストラクタからも作成可能です)。<br/>
<br/>
<i>GFW</i>での継承元ページとなるGearsPageでは、ページ内のコントロールから作成した<i>AttributeHolder</i>のリストMyControlValidatorsを保持しており、
更新系処理が行われようとした際、起点コントロールについてこのリストを使用しチェックを行っています。<br/>
※選択処理ではバリデーションは行われません。明示的にバリデーションをかけたい場合、isValidateOkをコールしてください。<br/>
<br/>
バリデーションがエラーとなった場合、対象のコントロールにエラー用のCSS(gs-validation-error)が適用されます。
複数エラーがあった場合、適用されるのは最初の1コントロールのみですが、ログ格納用変数(Log)には全てのチェック結果が入っているため、
どのコントロールにエラーがあったのかは判別可能です。<br/>
<br/>
<br/>

<h3 class="caption">GFW内でのビジネスロジックバリデーションについて</h3>
単純な項目チェックだけでなく、項目同士の関連によるチェック(ビジネスロジックチェック)を実装したいケースもあります。<br/>
(例えば、項目A・B・Cのうち必ず一つは入力しないといけない、など)。<br/>
<i>GFW</i>では、こうしたビジネスロジックにまつわるバリデーションは上記のような通常のバリデーションとは別の箇所で実装するようになっています。<br/>
具体的には、チェック処理を作成しデータソースクラスにセットします。そして、チェックは通常のバリデーションの後・データ更新処理前に行われます。<br/>
<br/>
    <div class="ppp-box even" >
        <b>チェック処理の順序</b><br/>
        <br/>
        更新処理実行　→　項目バリデーション処理　→　ビジネスロジックチェック　→　更新処理
        <br/>

    </div>
<br/>
このビジネスロジックチェックは、AbsModelValidatorを継承し作成します。実際のチェック処理は、
デリゲート定義AbsModelValidator.ModelValidatorを実装し、属性ModelValidationMethodを付与したものとして実装を行います(以下コード参照)。<br/>
<br/>
<div class="ppp-indent" style="width:800px;">
            <pre class="ppp-box even" style="font-style:italic;font-size:12px">
&lt;ModelValidationMethod(FalseAsAlert:=True, order:=0)&gt;
Public Function isSALTooLarge(ByVal sqlb As SqlBuilder) As Boolean
    Dim fromSal As Decimal = 0
    Dim toSal As Decimal = 0

    Decimal.TryParse(GearsSqlExecutor.getDataSetValue("SAL", empData), fromSal)
    Decimal.TryParse(getValidateeValue(sqlb, "SAL"), toSal)

    If toSal - fromSal > 1000 Then
        ErrorSource = "SAL"
        ErrorMessage = "給与の上昇幅が1000を超えています。入力ミスはありませんか？"
        Return False
    Else

        Return True
    End If

End Function
            </pre>
</div>
検証用メソッドに付与する属性ModelValidationMethodには、以下のオプションがあります。<br/>
<ul>
    <li>FalseAsAlert<br/>検証結果がFalseの時、警告とするか。警告の場合、確認ダイアログが表示される。</li>
    <li>OnlyWhenTheseValueExist<br/>特定の項目が存在する場合のみ検証を行う。項目の指定はカンマ区切り。</li>
    <li>Order<br/>検証を実行する順序</li>
</ul>
AbsModelValidatorは基本的にUnitTestに近い設計をしています。そのため、検証の開始前/開始後に対応するsetup/tearDown用のメソッドオーバーライドも提供されています。<br/>
<br/>

<br/>

<div class="ppp-indent" style="width:800px;">
    <div class="ppp-box odd" style="font-weight:bold">ビジネスロジックチェック例</div>
    <div class="ppp-box even" >
     <asp:UpdatePanel ID="updV2" runat="server" UpdateMode=Conditional>
            <ContentTemplate>
                <asp:Panel ID="pnlEMP_SAL__GFORM" runat="server">
                    従業員&nbsp;：<asp:DropDownList id="ddlEMPNO__KEY" runat="server" AutoPostBack=True></asp:DropDownList>
                    <br/>
                    <asp:Panel ID="pnlEMPAttr" runat="server">
                        職種&nbsp;&nbsp;&nbsp;：<asp:DropDownList ID="ddlJOB" runat="server"></asp:DropDownList><br/>
                        給料&nbsp;&nbsp;&nbsp;：<asp:TextBox ID="txtSAL" runat="server" CssClass="gears-GRequired gears-GNumeric" ></asp:TextBox><br/>
                        ※一度に1000円以上の値上げ→警告、職種がPRESIDENT/MANAGER以外の場合、4000円以上の設定不可。<br/>
                    </asp:Panel>
                    <br/>
                </asp:Panel>
                <br/>
                <asp:Label ID="lblMsgModelValid" runat="server" Text="" ></asp:Label><br/>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnSave" EventName="Click"/>
                <asp:AsyncPostBackTrigger ControlID="ddlEMPNO__KEY" EventName="SelectedIndexChanged"/>
            </Triggers>
    </asp:UpdatePanel>
                <asp:Button ID="btnSave" runat="server" Text="給与更新" /><br/>


    </div>
</div>
<br/><br/>
<br/><br/>
<br/><br/>


</div>

</asp:Content>
