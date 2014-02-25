<%@ Page Language="VB"  MasterPageFile="pppMaster.master" AutoEventWireup="false" CodeFile="GearsSampleControl.aspx.vb" Inherits="GearsSampleControl" %>
<%@ Register src="./UnitItem.ascx" tagname="unitItem" tagprefix="ui" %>
<%@ MasterType VirtualPath="pppMaster.master" %>

<asp:Content id="clientHead" ContentPlaceHolderID="pppHead" Runat="Server" ClientIDMode=Static>
    <title>What's DataSourceClass And Convension between Control And DataSourceClass</title>

</asp:Content>

<asp:Content id="clientPageTitle" ContentPlaceHolderID="pppPageTitle" Runat="Server" ClientIDMode=Static>
    GFWでのデータソースと規約
</asp:Content>

<asp:Content id="clientOptionMenu" ContentPlaceHolderID="pppOptionMenu" Runat="Server" ClientIDMode=Static>
            <ul class="ppp-menu-links">
                <li class="ppp-menu-links"><a href="default.aspx" class="ppp-link-item">Gearsトップ</a></li>
            </ul>
</asp:Content>

<asp:Content id="clientCenter" ContentPlaceHolderID="pppContent" Runat="Server" ClientIDMode=Static>
    <div class="document-body">
        <i>GFW</i>では、データの抽出処理を<i>データソースクラス</i>として切り出し、それとコントロールの間を名称規約で結ぶことでコントロールへのデータロードを実現しています。<br/>
        そのため、ここでは<i>データソースクラス</i>と名称規約の２点に分けて解説をしていきます。
        <br/>

        <h3 class="caption">データソースクラスについて</h3>
        データ抽出の処理、具体的にはDBコネクションのオープン・SQLの発行/結果セットの取得・コントロールへのバインド、の3点処理をまとめたものです。<br/>
        ※バインド先のコントロールは、後述する名称規約によりひもつけられます。<br/>
        <br/>
        ただし、当然ゼロからこれらのコーディングをする必要はなく、実際にコーディングする必要があるのはほんの10数行程度です。これは、サンプルのApp_Code/DataSource内にある
        実際のコードを見ていただければ分かると思います。<br/>
        <br/>
        データソースクラスの仕様についてはインタフェースGearsDataSourceにまとめられていますが、この実装クラスとしてGDSTemplateを提供しています。<br/>
        GDSTemplateには良く使うほとんどの処理を実装済みのため、このクラスを継承して作成すれば実装は容易です。実際に記載する必要があるのは、
        クラス名と、選択元のテーブル、選択列くらいです。<br/>
        <br/>
        具体的な例を示します。Categoryというテーブルがあり、そのテーブルのCatIDをキー、CatTextをテキストとして使用したドロップダウンリストを作成したいとします。
        この時に作成するデータクラスは以下のようなものになります。<br/>

        <div class="ppp-indent" style="width:800px;">
            <pre class="ppp-box even" style="font-style:italic;font-size:12px">
 Public Class Category
    Inherits GDSTemplate

    'コンストラクタ conNameは、DBへの接続文字列
    Public Sub New(ByVal conName As String) 
        'Categoryテーブルからデータを抽出
        MyBase.New(conName, SqlBuilder.newDataSource("Category"))
    End Sub

    'データ抽出の実行 dataには、画面上の選択値などの情報が格納されており、ここからSQLを組み立てる
    Public Overrides Function makeSqlBuilder(ByRef data As Gears.GearsDTO) As SqlBuilder
        'SQL構築用のクラスSqlBuilderを生成(この段階で、コンストラクタで設定した抽出元(Categoryテーブル)と
        '画面上での選択値(WHERE区)が設定されている。そのため、後は選択するキー・テキストに相当する列だけ設定すればよい)
        Dim sqlb As SqlBuilder = MyBase.makeSqlBuilder(data)

        '選択する列を設定
        sqlb.addSelection(SqlBuilder.newSelect("CatID"))
        sqlb.addSelection(SqlBuilder.newSelect("CatText"))

        Return sqlb

    End Function

 End Class
            </pre>
        </div>

        <br/>

        <h3 class="caption">名称規約について</h3>
        作成した<i>データソースクラス</i>は、名称規約に基づきコントロールとひもつけられ、それによりコントロールにデータがロードされます。<br/>
        具体的な名称規約とは、以下のようなものです。<br/>
        <br/>
        <div class="ppp-indent" style="width:800px;">
            <div class="ppp-box odd" style="font-weight:bold">名称規約</div>
            <div class="ppp-box even" >
                コントロールのID = xxxDDD__aaa__・・・<br/>
                <ul>
                    <li>xxx：コントロール種別(txt/rblなど、コントロールの種別を表す3文字。<br/>
                        実際バインドする際にここでコントロール種別を判断しているわけではないので(実際のコントロール型で判断)、3文字は任意で設定可能)</li>
                    <li>DDD：データソースクラス名(文字数制限は特になし)</li>
                    <li>aaa：属性(アンダースコア2つで区切って付与する、任意の文字列)</li>
                </ul>
                例：ddlCategoryなど(この場合、データソースクラスCategoryで抽出されたデータがロードされる。
            </div>
        </div>
        <br/>
        なお、<i>GFW</i>において自動でひもつけ対象となるコントロールの一覧は下記の通りです。
        これ以外のコントロールは、後述の手法を用いて手動でコントロールとデータソースのペアを登録する必要があります。<br/>
        <br/>
        <div class="ppp-indent" style="width:800px">
            <div class="ppp-box even">
            <b>対象コントロール一覧</b>
            <table class="ppp-table">
                <tr>
                    <th>コントロール</th><th>推奨上3桁</th><th>備考</th>
                </tr>
                <tr>    <td>TextBox</td><td>txt</td><td></td>   </tr>
                <tr>    <td>DropDownList</td><td>ddl</td><td></td>   </tr>
                <tr>    <td>RadioButtonList</td><td>rbl</td><td></td>   </tr>
                <tr>    <td>CheckBoxList</td><td>cbl</td><td></td>   </tr>
                <tr>    <td>CheckBox</td><td>cbx</td><td></td>   </tr>
                <tr>    <td>Label</td><td>lbl</td><td>属性が"GCON"を含む(IDに__GCONが含まれる)場合のみ対象</td>   </tr>
                <tr>    <td>Literal</td><td>lit</td><td>属性が"GCON"を含む(IDに__GCONが含まれる)場合のみ対象</td>   </tr>
                <tr>    <td>HiddenField</td><td>hdn</td><td></td>   </tr>
            </table>
            </div>
        </div>
        <br/>

        <i>GFW</i>を使用する場合、ページの作成に当たってはGearsPageを継承して作成します。継承により、
        ページ上に名称規約に基づいたコントロールがあり、それに対応する<i>データソースクラス</i>が作成してあるならば、
        自動的にデータがロードされるようになります。

        <br/>
        <br/>
        具体的な例は以下となります。見ていただければ分かるとおり、このページのコードビハインドでは何もしていませんが、
        名称規約に基づきデータがロードされています。<br/>
        <br/>
        <div class="ppp-indent" style="width:800px">
            <div class="ppp-box even">
            <b>実例:コントロールとデータソースのひもつけ</b>
                <table class="ppp-table" style="border:2px solid white;">
                <tr>
                    <th style="width:200px">コントロール</th>
                    <th>コントロールID</th>
                    <th>データソースクラス名</th>
                    <th>ロード結果</th>
                </tr>
                <tr>
                    <td>
                        <asp:CheckBox ID="chbSAMPLE_FLG" runat="server" Text="ほげ" />
                    </td>
                    <td>
                        <%=chbSAMPLE_FLG.ID%>
                    </td>
                    <td>
                        <%=getMyControl(chbSAMPLE_FLG.ID).DataSourceID%>
                    </td>
                    <td>
                        <% If Not getMyControl(chbSAMPLE_FLG.ID).DataSourceID Is Nothing Then%>
                            <span class="ppp-msg success">データソースのロードに成功しました</span>
                        <% Else%>
                            <span class="ppp-msg error">データソースのロードに失敗しました</span>
                        <% End If%>
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:RadioButtonList ID="rblCOMP_UNIT" runat="server" RepeatLayout=Flow></asp:RadioButtonList>
                    </td>
                    <td>
                        <%=rblCOMP_UNIT.ID%>
                    </td>
                    <td>
                        <%=getMyControl(rblCOMP_UNIT.ID).DataSourceID%>
                    </td>
                    <td>
                        <% If Not getMyControl(rblCOMP_UNIT.ID).DataSourceID Is Nothing Then%>
                            <span class="ppp-msg success">データソースのロードに成功しました</span>
                        <% Else%>
                            <span class="ppp-msg error">データソースのロードに失敗しました</span>
                        <% End If%>
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:DropDownList ID="ddlCOMP_GRP" runat="server"></asp:DropDownList>
                    </td>
                    <td>
                        <%=ddlCOMP_GRP.ID%>
                    </td>
                    <td>
                        <%=getMyControl(ddlCOMP_GRP.ID).DataSourceID%>
                    </td>
                    <td>
                        <% If Not getMyControl(ddlCOMP_GRP.ID).DataSourceID Is Nothing Then%>
                            <span class="ppp-msg success">データソースのロードに成功しました</span>
                        <% Else%>
                            <span class="ppp-msg error">データソースのロードに失敗しました</span>
                        <% End If%>
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:TextBox ID="txtCOMP_UNIT" runat="server"></asp:TextBox><br/>
                        ※バインドの概念がないコントロールに対しては処理されない
                    </td>
                    <td>
                        <%=txtCOMP_UNIT.ID%>
                    </td>
                    <td>
                        <%=getMyControl(txtCOMP_UNIT.ID).DataSourceID%>
                    </td>
                    <td>
                        <% If Not getMyControl(txtCOMP_UNIT.ID).DataSourceID Is Nothing Then%>
                            <span class="ppp-msg success">データソースのロードに成功しました</span>
                        <% Else%>
                            <span class="ppp-msg error">データソースのロードに失敗しました</span>
                        <% End If%>
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:Label ID="lblCOMP_UNIT__GCON" runat="server"></asp:Label><br/>
                        ※Labelの場合、末尾に__GCONをつけると自動処理対象になる
                    </td>
                    <td>
                        <%=lblCOMP_UNIT__GCON.ID%>
                    </td>
                    <td>
                        <%=getMyControl(lblCOMP_UNIT__GCON.ID).DataSourceID%>
                    </td>
                    <td>
                        <% If Not getMyControl(lblCOMP_UNIT__GCON.ID).DataSourceID Is Nothing Then%>
                            <span class="ppp-msg success">データソースのロードに成功しました</span>
                        <% Else%>
                            <span class="ppp-msg error">データソースのロードに失敗しました</span>
                        <% End If%>
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:Label ID="lblCOMP_UNIT" runat="server"></asp:Label><br/>
                        ※Labelで__GCONをつけない場合、処理対象外になる
                    </td>
                    <td>
                        <%=lblCOMP_UNIT.ID%>
                    </td>
                    <td>
                        COMP_UNIT
                    </td>
                    <td>
                        <% If getMyControl(lblCOMP_UNIT.ID) Is Nothing Then%>
                            <span class="ppp-msg success">処理対象外です</span>
                        <% Else%>
                            <span class="ppp-msg error">__GCONを付与していないのに処理対象になっています</span>
                        <% End If%>
                    </td>
                </tr>
                </table>
            
            </div>
        </div>
        <br/>
        対象のコントロールにないコントロールの場合、また名称規約を意図的に適用させたくない場合などは、
        手動でコントロールとデータソースのペアを登録する必要があります。<br/>
        具体的には、以下のコードで登録を行います。<br/>

        <div class="ppp-indent">
            <b><i> registerMyControl(ByRef control As Control, ByRef dataSourceClass As GearsDataSource)</i></b>
        </div>
        ※<i>データソースクラス</i>の引数を省略し、コントロールのIDから名称規約に基づき推定させることも出来ます。<br/>
        <br/>
        なお、コントロールと<i>データソースクラス</i>が組み合わされたオブジェクトは、
        内部的にはGearsControlというクラスで管理されます。<br/>
        GearsControlへは、以下のメソッドを使用しアクセスできます。<br/>
        <div class="ppp-indent">
            <b><i> Dim gcon As GearsControl = getMyControl(control.ID)</i></b>
        </div>
        
        <br/>
        <br/>

    </div>

    <asp:Label id="lblMsg" runat="server" Text="" CssClass="ppp-msg error"></asp:Label>
    <br/>
    <asp:Label id="lblLog" runat="server" Text="" CssClass="ppp-msg"></asp:Label>


</asp:Content>
