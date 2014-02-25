<%@ Page Language="VB"  MasterPageFile="pppMaster.master" AutoEventWireup="false" CodeFile="GearsSampleRelation.aspx.vb" Inherits="GearsSampleRelation" %>
<%@ Register src="./UnitItem.ascx" tagname="unitItem" tagprefix="ui" %>
<%@ MasterType VirtualPath="pppMaster.master" %>

<asp:Content id="clientHead" ContentPlaceHolderID="pppHead" Runat="Server" ClientIDMode=Static>
    <title>Control Relation And Data Linkage on GFW</title>

</asp:Content>

<asp:Content id="clientPageTitle" ContentPlaceHolderID="pppPageTitle" Runat="Server" ClientIDMode=Static>
    GFWでの関連とデータ連動
</asp:Content>

<asp:Content id="clientOptionMenu" ContentPlaceHolderID="pppOptionMenu" Runat="Server" ClientIDMode=Static>
            <ul class="ppp-menu-links">
                <li class="ppp-menu-links"><a href="default.aspx" class="ppp-link-item">Gearsトップ</a></li>
            </ul>
</asp:Content>

<asp:Content id="clientCenter" ContentPlaceHolderID="pppContent" Runat="Server" ClientIDMode=Static>
 <div class="document-body">
    <i>GFW</i>では、コントロール間の関連をデータの関連に反映させることが出来ます。<br/>
    例えば、以下のように事業部の選択値が部門のリストに影響するといった例です。こうした動作を、
    コントロール間の関連定義のみで実現できます。<br/>
    <br/>
    <div class="ppp-indent" style="width:800px">
        <div class="ppp-box even">
        <b>コントロール同士の関連例</b>
        <asp:UpdatePanel id="udpTest2" runat="server" UpdateMode=Conditional>
            <ContentTemplate>
                <table class="ppp-table">
                    <tr >
                        <th>事業部
                        </th>
                        <th>部門
                        </th>
                    </tr>
                    <tr>
                        <td>
                            <asp:DropDownList id="ddlCOMP_UNIT" runat="server" RepeatDirection=Horizontal AutoPostBack=True RepeatLayout=Flow >
                            </asp:DropDownList>
                            <br/><span style="font-size:0.3em">ddlCOMP_UNIT</span>
                        </td>
                        <td>
                            <asp:DropDownList id="ddlCOMP_GRP" runat="server" >
                                <asp:ListItem Value="" Text="(すべて)" Default="True" Position="F"></asp:ListItem>
                            </asp:DropDownList>      
                           <br/><span style="font-size:0.3em">ddlCOMP_GRP</span>             
                        </td>
                    </tr>
                </table>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="ddlCOMP_UNIT" EventName="SelectedIndexChanged"/>
            </Triggers>
        </asp:UpdatePanel>

        </div>
    </div>
    <br/>
    上記の動作を行うために必要なコードは、以下の２行のみです。<br/>
    <br/>
    <div class="ppp-indent">
        Page_Loadで・・・<br/>
        <b><i>addRelation(ddlCOMP_UNIT, ddlCOMP_GRP)</i></b>
        <br/><br/>
        ddlCOMP_UNIT_SelectedIndexChangedで・・・<br/>
        <b><i>executeBehavior(ddlCOMP_UNIT)</i></b>
    </div>
    <br/>
    事業部のコントロールであるddlCOMP_UNITが部門のコントロールであるddlCOMP_GRPへ関連することを示し、
    変更が発生した場合に関連先へ通知する、という構成になっています。<br/>
    <br/>
    
    <i>executeBehavior</i>の呼び出しから始まる一連のデータ更新処理は、全３ステップで行われます。<br/>
    <ul>
        <li><u>１．起点コントロールの値の収集</u><br/>
            起点となるコントロール(addRelationの左辺に当たるコントロール)に設定された値を収集します。
        </li>
        <li><u>２．関連先コントロールへの通知・更新</u><br/>
            収集した起点コントロールの値を、関連先のコントロール(addRelationの右辺に当たるコントロール)へ通知します。<br/>
            通知を受け取った関連先コントロールは、受け取った起点コントロールの情報を使用して、自身のデータを更新します。
        </li>
        <li><u>３．更新データの展開</u><br/>
            Panelなどの複合コントロールの場合は、配下のコントロールに自身の更新結果を反映させます。
        </li>
    </ul>
    以下では、これらの処理の詳細について説明します。
    <br/>
    <br/>
    <h3 class="caption">１．起点コントロールの値の収集</h3>
    まず、起点となるコントロールの値がどのように変化したのか、変化後の値を収集します。<br/>
    収集した値は、このコントロールがこの値、といった形でGearsDTOというクラスにまとめられます。
    これは後続のデータ更新処理を行う際、SQLの元ネタとなります(具体的にはINSERT/UPDATE、WHERE区の設定値となります)。<br/>
    <br/>
    「起点となるコントロール」は単一のコントロール以外に、複数のコントロールをまとめたPanelのようなコントロールも設定できます。
    一覧を検索する際などはむしろこのパターンが多いでしょう。<br/>
    <i>GFW</i>で、以下のネーミングルールを持つパネルは特殊な働きをします。<br/>
    <br/>
    <div class="ppp-indent" style="width:800px;">
        <div class="ppp-box odd" style="font-weight:bold">GFWにおける特殊パネル</div>
        <div class="ppp-box even" >
            <ul>
                <li><b>選択項目用パネル(IDに「<i>__GFILTER</i>がつくもの」)</b></li>
                    <ul>
                        <li>データの選択(SELECT)を行う際に使用します。パネル配下のコントロールの値が、制約条件として使用されます。</li>
                    </ul>
                    <br/>
                </li>
                <li><b>フォーム用パネル(IDに「<i>__GFORM</i>がつくもの」)</b>
                    <ul>
                        <li>データの更新(UPDATE/INSERT/DELETE)を行う際に使用します。パネル配下のコントロールの値は、DBの更新値として使用されます。
                        </li>
                        <li>※UPDATE/DELETEを行う場合は更新対象を特定する必要があるため、キー項目のコントロールを明確にしておく必要があります。<br/>
                            キー項目のコントロールは、IDにKEY属性をつける(IDに__KEYを含む)ことで設定できます(GearsControlを呼び出し直接setAskey()を実行しても可)。
                        </li>
                    </ul>
                </li>
            </ul>
            <span class="ppp-msg success">※なお、パネルは手動でコントロール・データソースのペアを登録する必要がある点に注意してください</span>
        </div>
    </div>
    <br/>
    少しルールが多いため。補足します。<br/>
    注意を払うべき点は以下３点です。それ以外に難しい点はありません。<br/>
    <ul>
        <li>Panelは手動で登録を行う必要がある<br/>
            関連を登録するには、コントロール・データソースのペアが登録されている必要があります。
            Panelはこの登録が自動的に行われないため、<i>registerControl</i>を使用し手動で登録する必要があります。<br/>
            <a href="GearsSampleControl.aspx" class="ppp-link-item" ><i>GFWでのデータソースと規約</i>参照</a>
            <br/><br/>
        </li>
        <li>選択用と更新用で、使用すべきパネルは異なる<br/>
            選択用は「__GFILTER」・更新用は「__GFORM」をIDに含むPanelを使用します。<br/>
            使い分けているのは、選択の際と更新の際でコントロールの設定値の役割が異なるためです(指定がない場合選択用と解釈されます)。<br/>
            <br/>
        </li>
        <li>更新用パネルの場合、キーとなるコントロールを用意しておく必要がある<br/>
            更新に際しては、どこかにデータのレコードを一意に特定できるキーがあり、それがキーとして設定されている必要があります。<br/>
        </li>
    </ul>
    実際起点コントロールからデータ収集を収集してみたものが以下の例になります。<br/>
    選択項目用パネルとフォーム用パネルでは生成されるSQLが異なることが分かると思います。<br/>
    <br/>
    <div class="ppp-indent" >
        <div class="ppp-box even">
        <b>関連元コントロール情報の収集</b>
            <asp:UpdatePanel id="udpTest5" runat="server" UpdateMode=Conditional>
                <ContentTemplate>
                    <table class="ppp-table">
                        <tr >
                            <th style="width:180px">関連元コントロール
                            </th>
                            <th style="width:100px" style="text-align:center;align:center">情報収集実行
                            </th>
                            <th style="width:400px">収集された情報(GearsDTO)
                            </th>
                            <th style="width:400px">
                                収集された情報から作成されるSQL<br/>
                                <asp:DropDownList ID="ddlTest5SQLType" runat="server">
                                    <asp:ListItem Value="SELECT" Text="SELECT" />
                                    <asp:ListItem Value="UPDATE" Text="UPDATE" />
                                    <asp:ListItem Value="INSERT" Text="INSERT" />
                                    <asp:ListItem Value="DELETE" Text="DELETE" />
                                </asp:DropDownList>
                                                    
                            </th>
                        </tr>
                        <tr>
                            <td>
                                単一コントロール<br/>
                                <asp:DropDownList id="ddlEMPNO__5__1" runat="server" >
                                </asp:DropDownList> 
                            </td>
                            <td>
                                <asp:Button ID="btnClick__5__1" runat="server" Text="→" />
                                </asp:Panel>
                            </td>
                            <td>
                                <asp:Label id="lblDTO__1__1" runat="server" Text="" CssClass="ppp-msg"></asp:Label>
                            </td>
                            <td>
                                <asp:Label id="lblDTO__1__2" runat="server" Text="" CssClass="ppp-msg"></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                選択項目用パネル(__GFILTER)
                                <asp:Panel id="pnlTest5__1__GFILTER" runat="server">
                                    <asp:DropDownList id="ddlEMPNO__5__2" runat="server">
                                    </asp:DropDownList>
                                    <br/>
                                    <asp:RadioButtonList ID="rblTest5SEX__1" runat="server" RepeatLayout=Flow>
                                        <asp:ListItem Value="M" Text="男性(値M)" />
                                        <asp:ListItem Value="F" Text="女性(値F)" />
                                    </asp:RadioButtonList>
                                    <br/>
                                    <asp:DropDownList ID="ddlTest5HEIGHT__1" runat="server">
                                        <asp:ListItem Value="150" Text="身長：150-160" />
                                        <asp:ListItem Value="160" Text="身長：160-170" />
                                        <asp:ListItem Value="170" Text="身長：170-180" />
                                    </asp:DropDownList>
                                    <br/>
                                    <asp:CheckBox ID="chbTest5CHECK__1" runat="server" Text="朝食食べた" />
                                    <asp:TextBox ID="txtTest5TEXT" runat="server" Text="対象から除外"></asp:TextBox>
                                </asp:Panel>
                            </td>
                            <td>
                                <asp:Button ID="btnClick__5__2" runat="server" Text="→" />
                                </asp:Panel>
                                
                            </td>
                            <td>
                                <asp:Label id="lblDTO__2__1" runat="server" Text="" CssClass="ppp-msg"></asp:Label>
                            </td>
                            <td>
                                <asp:Label id="lblDTO__2__2" runat="server" Text="" CssClass="ppp-msg"></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td>
                               フォーム用パネル(__GFORM)
                                <asp:Panel id="pnlTest5__1__GFORM" runat="server">
                                    <asp:DropDownList id="ddlEMPNO__5__3" runat="server">
                                    </asp:DropDownList>
                                    <br/>
                                    <asp:RadioButtonList ID="rblTest5SEX__2" runat="server" RepeatLayout=Flow>
                                        <asp:ListItem Value="M" Text="男性(値M)" />
                                        <asp:ListItem Value="F" Text="女性(値F)" />
                                    </asp:RadioButtonList>
                                    <br/>
                                    <asp:DropDownList ID="ddlTest5HEIGHT__2" runat="server">
                                        <asp:ListItem Value="150" Text="身長：150-160" />
                                        <asp:ListItem Value="160" Text="身長：160-170" />
                                        <asp:ListItem Value="170" Text="身長：170-180" />
                                    </asp:DropDownList>
                                    <br/>
                                    <asp:CheckBox ID="chbTest5CHECK__2" runat="server" Text="朝食食べた" />
                                </asp:Panel>
                            </td>
                            <td>
                                <asp:Button ID="btnClick__5__3" runat="server" Text="→" />
                                </asp:Panel>
                                
                            </td>
                            <td>
                                <asp:Label id="lblDTO__3__1" runat="server" Text="" CssClass="ppp-msg"></asp:Label>
                            </td>
                            <td>
                                <asp:Label id="lblDTO__3__2" runat="server" Text="" CssClass="ppp-msg"></asp:Label>
                            </td>
                        </tr>
                    </table>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="btnClick__5__1" EventName="Click"/>
                    <asp:AsyncPostBackTrigger ControlID="btnClick__5__2" EventName="Click"/>
                    <asp:AsyncPostBackTrigger ControlID="btnClick__5__3" EventName="Click"/>
                </Triggers>
            </asp:UpdatePanel>
        </div>
    </div>
    <br/>
    パネル内のコントロール値については、以下のようにOperatorの属性を設定することで、SQLの条件を制御することが出来ます。<br/>
        <div class="ppp-indent" style="width:800px;">
        <pre class="ppp-box even" style="font-style:italic;font-size:12px">

 ＜asp:DropDownList id="ddlEMPNO__5__1" runat="server" Operator="LIKE" /＞
        </pre>
    </div>
    使用可能なOperatorの種類は以下の通りです。<br/>
    <div class="ppp-indent" style="width:800px;">
    <table>
        <tr><td>・GT        </td><td>：</td><td>より大きい</td></tr>
        <tr><td>・GTEQ      </td><td>：</td><td>以上</td></tr>
        <tr><td>・LT        </td><td>：</td><td>未満</td></tr>
        <tr><td>・LTEQ      </td><td>：</td><td>以下</td></tr>
        <tr><td>・LIKE      </td><td>：</td><td>中間一致検索(*AAA*)</td></tr>
        <tr><td>・START_WITH</td><td>：</td><td>前方一致検索(AAA*)</td></tr>
        <tr><td>・END_WITH  </td><td>：</td><td>後方一致検索(*AAA)</td></tr>
    </table>
    </div>
    <br/>
    <br/>
    この段階では、まだ更新対象が明らかでありません(作成されたSQLを参照すると、「更新対象はdummyで表示」となっていると思います)。<br/>
    この更新対象こそが「関連先コントロール」であり、関連先コントロールのデータソースクラスがここに設定されることで完全なSQLとなります。
    <br/>
    <br/>

    <h3 class="caption">２．関連先コントロールへの通知・更新</h3>
    
    収集された起点コントロールの情報は、関連先コントロールのデータソースと組み合わせられることで完全なSQLになります。<br/>
    この収集された情報からSQLの作成を行っているのが、データソースのメソッド<i>makeSqlBuilder</i>となります。この点については、
    <i>GFWでのデータソースと規約</i>を再度参照してみてください。<br/>
    <br/>
    なお、SQLの組み立てに際しては<b>「コントロールのデータソースクラス名」=「テーブルのカラム名」</b>であることが前提となっています。
    このルールが適用できない場合は、項目変換のルールを設定しておく必要があります。<br/>
    以下は、コントロールのデータソースクラス名はCategoryだけれども、実際のテーブル列名はItem_Categoryである、という場合の対応例です。
    <br/>
        <div class="ppp-indent" style="width:800px;">
            <pre class="ppp-box even" style="font-style:italic;font-size:12px">
Public Class SalesData '売上データ抽出用のデータソースクラス
    Inherits GDSTemplate
    Public Sub New(ByVal conName As String)
        MyBase.New(conName, SqlBuilder.newDataSource("SalesData"))

    End Sub

    Public Overrides Function makeSqlBuilder(ByRef data As Gears.GearsDTO) As SqlBuilder
        Dim sqlb As SqlBuilder = MyBase.makeSqlBuilder(data)
        
        '変換ルールを登録。テーブルSalesDataでは、Categoryの列がItem_Categoryという名称になっている
        Dim convertor As New ViewItemAndColumnMapperTemplete
        convertor.addRule("Category", "Item_Category") 'CategoryをItem_Categoryへ変換
        sqlb.setdsColConvertor(convertor)

        Return sqlb

    End Function
End Class
            </pre>
        </div>

    変換ルールは、共通クラスのViewItemAndColumnMapperTempleteインタフェースを実装したクラスであれば可ですので、よく使う変換ルールなどは
    あらかじめクラス化可能です。<br/>
    <br/>
    以下では、実際に起点コントロールの値を関連先コントロールへ通知し、データの更新を行っています。<br/>
    起点コントロールは選択用パネル、関連先コントロールはGridViewとなっています。<br/>
    <br/>
    <div class="ppp-indent" style="width:800px">
        <div class="ppp-box even">
        <b>関連先コントロールへの通知・更新例</b>
            <asp:UpdatePanel id="udpTest4" runat="server" UpdateMode=Conditional>
                <ContentTemplate>
                    <table table class="ppp-table">
                        <tr >
                            <th style="width:250px">起点コントロール(選択パネル)
                            </th>
                            <th style="width:500px">関連先コントロール(GridView)
                            </th>
                        </tr>
                        <tr>
                            <td>
                                <asp:Panel id="pnlTest4__GFILTER" runat="server">

                                    <asp:DropDownList id="ddlCOMP_UNIT__2" runat="server" AutoPostBack=True >
                                    </asp:DropDownList>
                                    </br>
                                    <asp:DropDownList id="ddlCOMP_GRP__2" runat="server" >
                                        <asp:ListItem Value="" Text="(すべて)" Default="True" Position="F"></asp:ListItem>
                                    </asp:DropDownList>
                                    <br/>
                                    <asp:Button ID="btnClick__4" runat="server" Text="通知実行" />
                                </asp:Panel>
                            </td>
                            <td>
                                <asp:GridView id="grvData4" runat="server" 
                                    AutoGenerateSelectButton=False 
                                    AutoGenerateColumns=False
                                    CssClass=ppp-table 
                                    HeaderStyle-CssClass=ppp-table-head EnableViewState=True RowStyle-CssClass=ppp-table-odd AlternatingRowStyle-CssClass=ppp-table-even>
                                    <Columns>
                                        <asp:BoundField DataField="COMP_UNIT" HeaderText="事業部">
                                        </asp:BoundField>
                                        <asp:BoundField DataField="COMP_GRP" HeaderText="部門">
                                        </asp:BoundField>
                                        <asp:BoundField DataField="EMPNO" HeaderText="従業員番号">
                                        </asp:BoundField>
                                        <asp:BoundField DataField="ENAME" HeaderText="名前" HeaderStyle-Width="150px">
                                        </asp:BoundField>
                                        <asp:BoundField DataField="JOB" HeaderText="職位">
                                        </asp:BoundField>
                                    </Columns>
                                </asp:GridView>
                            </td>
                        </tr>
                    </table>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="ddlCOMP_UNIT__2" EventName="SelectedIndexChanged"/>
                    <asp:AsyncPostBackTrigger ControlID="btnClick__4" EventName="Click"/>
                </Triggers>
            </asp:UpdatePanel>
        </div>
    </div>

    <br/>
    <br/>

    <h3 class="caption">３．更新データの展開</h3>    
    Panelなどの複合コントロールである場合、データソースの更新結果は配下のコントロールに反映されます。<br/>
    ちょうど、テーブルから選択した1レコードの内容をフォーム上の各コントロールに展開するイメージです。<br/>
    <br/>
    展開に当たってはSQLの組み立てと同様<b>「コントロールのデータソースクラス名」=「テーブルのカラム名」</b>が適用されますが、
    前述の通り変換ルールを設定しておくことで、このルールが適用されない場合でも対応可能です。<br/>
    なお、パネル内にリレーション関係のある項目が含まれる場合でも、リレーションを解決しながら展開をしてくれます（下記の事業部と部門）。<br/>
    <br/>
    以下は、更新結果をパネル内のコントロールに反映させている例となります。<br/>
    <br/>
    <div class="ppp-indent" style="width:800px">
        <div class="ppp-box even">
        <b>更新データの展開例</b>
            <asp:UpdatePanel id="udpTest3" runat="server" UpdateMode=Conditional>
                <ContentTemplate>
                    <table table class="ppp-table">
                        <tr >
                            <th>選択条件
                            </th>
                            <th>実行結果の反映
                            </th>
                        </tr>
                        <tr>
                            <td style="width:150px">
                                従業員を選択→選択された従業員のレコードを、フォームに反映<br/>
                                <asp:DropDownList id="ddlEMPNO" runat="server" AutoPostBack=True>
                                </asp:DropDownList> 
                            </td>
                            <td>
                                フォーム用パネル(取得されたデータが表示される)：<b><%= pnlTest3__GFORM.ID%></b>
                                <asp:Panel id="pnlTest3__GFORM" runat="server">
                                    事業部&nbsp;&nbsp;&nbsp;：<asp:RadioButtonList ID="rblCOMP_UNIT__3" runat="server" RepeatDirection=Horizontal RepeatLayout=Flow></asp:RadioButtonList><br/>
                                    部門&nbsp;&nbsp;&nbsp;：<asp:DropDownList ID="ddlCOMP_GRP__3" runat="server"></asp:DropDownList><br/>                                    
                                    職種&nbsp;&nbsp;&nbsp;：<asp:TextBox ID="txtJOB__3" runat="server"></asp:TextBox><br/>
                                    給料&nbsp;&nbsp;&nbsp;：<asp:TextBox ID="txtSAL__3" runat="server"></asp:TextBox><br/>                                    
                                    雇用日：<asp:TextBox ID="txtHIREDATE__3" runat="server"></asp:TextBox><br/>                                    
                                </asp:Panel>
                            </td>
                        </tr>
                    </table>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="ddlEMPNO" EventName="SelectedIndexChanged"/>
                </Triggers>
            </asp:UpdatePanel>
        </div>
    </div>
    <br/>
    <br/>

    <asp:Label id="lblMsg" runat="server" Text="" CssClass="ppp-msg error"></asp:Label>
    <br/>

 </div>
</asp:Content>