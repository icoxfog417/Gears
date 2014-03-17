Imports Microsoft.VisualBasic
Imports System.Web.UI
Imports System.Collections
Imports System
Imports System.Web.UI.WebControls
Imports System.Reflection
Imports Gears.DataSource
Imports Gears.Validation
Imports Gears.Binder
Imports System.Data
Imports Gears.Util

Namespace Gears

    ''' <summary>
    ''' System.Web.UI.WebControlをラップするControl<br/>
    ''' 1.値のセットについて<br/>
    ''' 基本的にdataBindにより選択リストの取得、dataAttachにより値のセットが行われます。<br/>
    ''' 例えば、A・B・Cというの選択肢があり選択中の値はB、という場合、A・B・Cという値はdataBindによりロードされ、
    ''' Bという値はdataAttachにより設定されます。dataAttachは主に特定の一行のDataTableを引数とし、この中で自Controlに該当する値を選択しセットします
    ''' <see cref="Gears.Binder.GearsDataBinder.dataAttach"/>。<br/>
    ''' なお、値の取得/設定はgetValue/setValueを使用し直接的に行うことも可能です。<br/>
    ''' <br/>
    ''' dataBindに使用されるGearsDataSourceは、基本的にIDから推定されます<a href="http://gearssite.apphb.com/GearsSampleControl.aspx" target="_blank">名称規約について</a><br/>
    ''' 簡単な例としては、ddlUNITというコントロールの場合「UNIT」がデータソースクラス名称として推定されます。<br/>
    ''' <br/>
    ''' 2.値の検証(バリデーション)について<br/>
    ''' GearsControlはバリデーションの機能も保持しています。バリデーションのための設定情報はCssClassから抽出されます。<br/>
    ''' <seealso cref="GearsControl.loadAttribute"/>
    ''' </summary>
    ''' <remarks></remarks>
    Public Class GearsControl
        Implements IAttributeHolder

        ''' <summary>IDに含まれることで、更新用フォームであることを示す文字列</summary>
        Public Const ID_ATTR_FORM As String = "GFORM"

        ''' <summary>IDに含まれることで、検索フォームであることを示す文字列</summary>
        Public Const ID_ATTR_FILTER As String = "GFILTER"

        ''' <summary>IDに含まれることで、更新キーであることを示す文字列</summary>
        Public Const ID_ATTR_KEY As String = "KEY"

        ''' <summary>IDに含まれることで、登録対象コントロールであることを示す文字列</summary>
        Public Const ID_ATTR_GCON As String = "GCON"

        ''' <summary>IDに含まれることで、登録対象だが送信対象外であることを示す文字列</summary>
        Public Const ID_ATTR_GDISP As String = "GDISP"

        ''' <summary>検索時のオペレーターを指定するための属性</summary>
        Public Const ATTR_OPERATOR As String = "OPERATOR"

        ''' <summary>名称空間を指定するための属性</summary>
        Public Const ATTR_DS_NAMESPACE As String = "DSNAMESPACE"

        ''' <summary>接続文字列を指定するための属性</summary>
        Public Const ATTR_DS_CONNECTION_NAME As String = "DSCONNECTIONNAME"

        ''' <summary>Serializeを行うための区切り文字</summary>
        Public Const VALUE_SEPARATOR As String = vbVerticalTab

        ''' <summary>ID内をSplitするためのSeparator</summary>
        Private Const ID_SEPARATOR As String = "__"

        ''' <summary>
        ''' データソースクラスが格納されているアセンブリ名
        ''' </summary>
        Private Shared DataSourceAssembleyName As String = ""

        Private _control As Control = Nothing
        ''' <summary>
        ''' コントロールの取得
        ''' </summary>
        Public Function Control() As Control
            Return _control
        End Function

        ''' <summary>
        ''' 型指定によるコントロールの取得
        ''' </summary>
        ''' <typeparam name="T">WebControl型</typeparam>
        Public Function Control(Of T As WebControl)() As T
            Return CType(_control, T)
        End Function

        ''' <summary>
        ''' コントロールID(Control.IDと等価)
        ''' </summary>
        Public ReadOnly Property ControlID As String
            Get
                Return _control.ID
            End Get
        End Property

        ''' <summary>
        ''' DBにアクセスするための接続文字列
        ''' </summary>
        Public Property ConnectionName() As String

        ''' <summary>
        ''' データソースクラスの名称空間
        ''' </summary>
        Public Property DsNameSpace() As String

        ''' <summary>コントロールに設定されたIDをID_SEPARATORでSplitしたもの</summary>
        Private _idAttributes As ArrayList = New ArrayList()

        ''' <summary>
        ''' コントロール種別を抽出するクラス関数
        ''' </summary>
        ''' <param name="id"></param>
        Public Shared Function extractControlType(ByVal id As String) As String
            Dim idElements As String() = id.Split(New String() {ID_SEPARATOR}, StringSplitOptions.None)
            Return idElements(0).Substring(0, 3).ToUpper

        End Function

        ''' <summary>
        ''' コントロールがキーか否か
        ''' </summary>
        Public Property IsKey As Boolean

        ''' <summary>
        ''' このコントロールをキーとして設定する
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub asKey()
            _IsKey = True
        End Sub

        Private _isFormAttribute As Boolean = False
        ''' <summary>
        ''' コントロールが更新フォームか否か
        ''' </summary>
        Public ReadOnly Property IsFormAttribute() As Boolean
            Get
                Return _isFormAttribute
            End Get
        End Property

        Private _isFilterAttribute As Boolean = False
        ''' <summary>
        ''' コントロールが検索フォームか否か
        ''' </summary>
        Public ReadOnly Property IsFilterAttribute() As Boolean
            Get
                Return _isFilterAttribute
            End Get
        End Property

        ''' <summary>
        ''' 値がセットされるのみで、フォーム等で送信対象から除外されるか否か<br/>
        ''' ID属性にGDISPが指定されている場合、デフォルトでこの設定となる
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsDisplayOnly As Boolean

        ''' <summary>
        ''' 検索時のオペレーターを示す値(主に検索フォームでのlikeなど)
        ''' </summary>
        Public Property OperatorAttribute() As String

        Private _dataSourceID As String = ""
        ''' <summary>
        ''' データロード元となるデータソースクラスの名称(基本的にIDから判断される)
        ''' </summary>
        Public ReadOnly Property DataSourceID() As String
            Get
                Return _dataSourceID
            End Get
        End Property

        ''' <summary>
        ''' データソースクラスIDを取得するクラス関数
        ''' </summary>
        ''' <param name="id"></param>
        Public Shared Function extractDataSourceid(ByVal id As String) As String
            If Not id Is Nothing Then
                Dim idElements As String() = id.Split(New String() {ID_SEPARATOR}, StringSplitOptions.None)
                If idElements.Length > 0 Then
                    Return idElements(0).Substring(3)
                Else
                    Return ""
                End If
            Else
                Return ""
            End If
        End Function

        ''' <summary>
        ''' データベースからデータを抽出するGearsDataSource
        ''' </summary>
        Public Property DataSource As GearsDataSource = Nothing

        ''' <summary>
        ''' データソースの値をコントロールにバインドするIDataBinder
        ''' </summary>
        Public Property DataBinder As IDataBinder = New GearsDataBinder

        Private _attributes As GearsAttribute = Nothing
        ''' <summary>
        ''' バリデーションを行うための情報となるGearsAttribute<see cref="Gears.Validation.GearsAttribute"/><br/>
        ''' </summary>
        ''' <remarks></remarks>
        Public Property GAttribute As GearsAttribute Implements IAttributeHolder.GAttribute
            Get
                Return _attributes
            End Get
            Set(value As GearsAttribute)
                'コンテナである場合Setで追加を行う
                If TypeOf _attributes Is GearsAttributeContainer And TypeOf value Is GearsAttribute Then
                    CType(_attributes, GearsAttributeContainer).addAttribute(value)
                Else
                    _attributes = value
                End If
            End Set
        End Property

        Private _gcssClass As String = ""
        ''' <summary>
        ''' コントロールに適用するスタイルを表す文字列
        ''' </summary>
        Public ReadOnly Property GCssClass As String Implements IAttributeHolder.GCssClass
            Get
                Return _gcssClass
            End Get
        End Property

        ''' <summary>ロード時の値</summary>
        Public Property LoadedValue As String = ""

        ''' <summary>
        ''' コンストラクタ<br/>
        ''' Controlと接続文字列を受け取り、ControlのIDからデータソースクラスを判定し設定する
        ''' </summary>
        ''' <param name="con">コントロール</param>
        ''' <param name="conName">接続文字列</param>
        ''' <param name="dns">データソースクラスの名称空間</param>
        ''' <param name="isAutoLoadAttr">属性を自動ロードするか(デフォルトTrue)</param>
        ''' <remarks></remarks>
        Public Sub New(ByRef con As Control, ByVal conName As String, Optional ByVal dns As String = "", Optional isAutoLoadAttr As Boolean = True)
            Me.ConnectionName = conName
            DsNameSpace = dns
            initInstance(con, isAutoLoadAttr)

        End Sub

        ''' <summary>
        ''' データソースクラスを直接受け取るコンストラクタ
        ''' </summary>
        ''' <param name="con"></param>
        ''' <param name="gs"></param>
        ''' <param name="isAutoLoadAttr"></param>
        ''' <remarks></remarks>
        Public Sub New(ByRef con As Control, ByRef gs As GearsDataSource, Optional isAutoLoadAttr As Boolean = True)
            Me.ConnectionName = gs.ConnectionName
            _DataSource = gs
            initInstance(con, isAutoLoadAttr)

        End Sub

        ''' <summary>
        ''' GearsControlの初期化処理を行う
        ''' </summary>
        ''' <param name="con"></param>
        ''' <param name="isAutoLoadAttr"></param>
        ''' <remarks></remarks>
        Private Sub initInstance(ByRef con As Control, ByVal isAutoLoadAttr As Boolean)
            _control = extractControl(con)
            readIDByGearsRule(_control.ID)

            'WHEREを作成する際のオペレーターの設定
            If TypeOf _control Is WebControl Then
                Dim wcon As WebControl = CType(_control, WebControl)
                If Not wcon.Attributes(ATTR_OPERATOR) Is Nothing Then
                    _OperatorAttribute = wcon.Attributes(ATTR_OPERATOR)
                End If
            End If

            loadDataSource()

            If isAutoLoadAttr Then
                loadAttribute()
            End If

        End Sub

        ''' <summary>
        ''' Controlに設定されたIDから、コントロール種別、データソース名などを読み込む
        ''' </summary>
        ''' <param name="id"></param>
        ''' <remarks></remarks>
        Private Sub readIDByGearsRule(ByVal id As String)
            If id <> "" Then
                Dim idElements As String() = id.Split(New String() {ID_SEPARATOR}, StringSplitOptions.None)
                '_controlType = idElements(0).Substring(0, 3).ToUpper '先頭3文字でコントロール種別
                _dataSourceID = idElements(0).Substring(3) '3文字目以降でデータソースクラス名

                If idElements.Length > 1 Then 'Separatorで区切られた属性情報がある場合
                    For i As Integer = 1 To idElements.Length - 1
                        _idAttributes.Add(idElements(i).ToUpper) '全て大文字にして格納
                    Next

                End If

                'キー値設定
                If isIdAttributeExist(ID_ATTR_KEY) Then
                    _IsKey = True
                End If

                'コントロールの種別設定
                If DataSourceID.ToUpper = ID_ATTR_FORM Or isIdAttributeExist(ID_ATTR_FORM) Then
                    _isFormAttribute = True
                ElseIf DataSourceID.ToUpper = ID_ATTR_FILTER Or isIdAttributeExist(ID_ATTR_FILTER) Then
                    _isFilterAttribute = True
                End If

                If isIdAttributeExist(ID_ATTR_GDISP) Then
                    IsDisplayOnly = True
                End If
            End If
        End Sub

        ''' <summary>
        ''' データソースクラスのロード
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub loadDataSource()
            If ConnectionName <> "" And DataSourceID <> "" Then
                Try
                    Dim className As String = ""
                    If Not String.IsNullOrEmpty(DsNameSpace) Then '名称空間がある場合
                        className = DsNameSpace + "." + DataSourceID
                    Else
                        className = DataSourceID
                    End If

                    'アプリケーションドメイン内から、該当クラスを検索(ライブラリ化する場合必須(相手先アセンブリを参照するため))
                    Dim classtype As Type = Nothing
                    If Not String.IsNullOrEmpty(DataSourceAssembleyName) Then
                        Dim assem As Assembly = AppDomain.CurrentDomain.GetAssemblies.SingleOrDefault(Function(t) t.FullName = DataSourceAssembleyName)
                        If Not assem Is Nothing Then
                            classtype = assem.GetTypes().SingleOrDefault(Function(t) t.FullName = className)
                        End If
                    End If

                    If classtype Is Nothing Then
                        For Each assem As Assembly In AppDomain.CurrentDomain.GetAssemblies
                            If Not (assem.GetName().FullName.Contains("System")) And Not (assem.GetName().FullName.Contains("Microsoft")) Then 'システム系は除く
                                classtype = assem.GetTypes().SingleOrDefault(Function(t) t.FullName = className)
                                If Not classtype Is Nothing Then
                                    DataSourceAssembleyName = assem.GetName.FullName
                                    Exit For
                                End If
                            End If
                        Next

                    End If

                    If Not classtype Is Nothing Then
                        Dim instance As Object = Activator.CreateInstance(classtype, ConnectionName)
                        _DataSource = CType(instance, GearsDataSource)
                    End If

                Catch ex As Exception
                    GearsLogStack.setLog("コントロール" + _control.ID + " へのデータソース登録に失敗しました(データソースID：" + DataSourceID + ")")

                End Try

            End If
        End Sub

        ''' <summary>
        ''' 設定されたCSSからバリデーションのための属性をロードする
        ''' </summary>
        ''' <param name="css"></param>
        ''' <remarks></remarks>
        Public Sub loadAttribute(Optional ByVal css As String = "")
            Dim attributeStr As String = ""
            If css = "" Then
                If TypeOf _control Is WebControl Then
                    attributeStr = CType(_control, WebControl).CssClass
                End If
            Else
                attributeStr = css
            End If

            'CssスタイルからAttributeの取得
            Dim attrCreator As New GearsAttributeCreator(DsNameSpace)
            attrCreator.createAttributesFromString(attributeStr)
            _gcssClass = attrCreator.getCssClass
            _attributes = attrCreator.pack

        End Sub

        ''' <summary>
        ''' 特定のアトリビュートを保持しているか判定する
        ''' </summary>
        ''' <param name="attr"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function isIdAttributeExist(ByVal attr As String) As Boolean
            Return isIdAttributeExist(_control.ID, attr)
        End Function

        ''' <summary>
        ''' IDに特定のアトリビュートが含まれているか判定する
        ''' </summary>
        ''' <param name="attr"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function isIdAttributeExist(ByVal id As String, ByVal attr As String) As Boolean
            Dim idEls() As String = id.Split(ID_SEPARATOR)
            Dim result As Boolean = False
            If idEls.Length > 1 Then
                For i As Integer = 0 To idEls.Length - 1
                    If idEls(i).ToUpper = attr Then
                        result = True
                        Exit For
                    End If
                Next
            End If
            Return result

        End Function

        ''' <summary>
        ''' コントロールのアトリビュートを取得する
        ''' </summary>
        ''' <param name="attr"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function getControlAttribute(ByVal attr As String) As String
            Return getControlAttribute(Me.Control, attr)
        End Function

        ''' <summary>
        ''' コントロールのアトリビュートを取得する
        ''' </summary>
        ''' <param name="attr"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function getControlAttribute(ByVal con As Control, ByVal attr As String) As String
            Dim result As String = ""
            If TypeOf con Is WebControl Then
                Dim wControl = CType(con, WebControl)
                'コントロールにアトリビュートが保持されているかどうか判定する
                If wControl.Attributes(attr) IsNot Nothing Then
                    result = wControl.Attributes(attr).ToString
                End If
            End If
            Return result
        End Function

        ''' <summary>
        ''' 自身のデータソースをロードし、バインドする
        ''' </summary>
        ''' <param name="dto"></param>
        ''' <remarks></remarks>
        Public Function dataBind(Optional ByVal dto As GearsDTO = Nothing) As Boolean
            Dim result As Boolean = True
            Dim valueNow As String = Me.getValue

            If Not DataSource Is Nothing Then
                'Load可能なControlやGridViewなど限られているので、パフォーマンスを考慮しControlを限定してもよいかも
                Try
                    Dim tmpDto As GearsDTO = dto
                    If dto Is Nothing Then
                        tmpDto = New GearsDTO(ActionType.SEL)
                    End If

                    Dim resultSet As DataTable = DataSource.execute(tmpDto)
                    If DataBinder.isBindable(_control) Then
                        result = dataBind(resultSet)
                    End If

                Catch ex As Exception
                    Throw
                End Try
            Else
                GearsLogStack.setLog("コントロール" + _control.ID + " にはデータソースが登録されていないため、処理は行われません")
            End If

            If Not String.IsNullOrEmpty(valueNow) Then Me.setValue(valueNow) 'セットされていた値を復元する

            Return result

        End Function

        ''' <summary>
        ''' データテーブルをコントロールにバインドする
        ''' </summary>
        ''' <param name="dset"></param>
        Public Function dataBind(ByVal dset As DataTable) As Boolean
            Dim result As Boolean = True
            Dim valueNow As String = Me.getValue

            Try
                result = DataBinder.dataBind(_control, dset)
            Catch ex As Exception
                Throw
            End Try

            If String.IsNullOrEmpty(valueNow) Then Me.setValue(valueNow) 'セットされていた値を復元する

            Return result

        End Function

        ''' <summary>
        ''' 他のデータソースの値を自身に適用する(値としてセットする)
        ''' </summary>
        ''' <param name="gds"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function dataAttach(ByRef gds As GearsDataSource) As Boolean
            If Not gds Is Nothing Then
                dataAttach(gds.gResultSet)
            End If
            Return True
        End Function

        ''' <summary>
        ''' 他のデータソースの値を自身に適用する(値としてセットする)<br/>
        ''' DataTableの指定がない場合は、自身のデータソースを使用
        ''' </summary>
        ''' <param name="dset"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function dataAttach(Optional ByRef dset As DataTable = Nothing) As Boolean
            Dim result As Boolean = True
            Dim dataResource As DataTable = Nothing

            If dset Is Nothing And DataSource IsNot Nothing Then
                dataResource = DataSource.gResultSet
            Else
                dataResource = dset
            End If

            Try
                DataBinder.dataAttach(_control, dataResource)
            Catch ex As Exception
                Throw
            End Try

            Return result

        End Function

        ''' <summary>
        ''' コントロールの値を取得する
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function getValue() As String
            Dim value As Object = DataBinder.getValue(_control)
            Dim result As String = If(value Is Nothing, "", value.ToString)

            If Not _attributes Is Nothing AndAlso _attributes.hasMarker(GetType(Marker.MarkerNumeric)) Then
                If String.IsNullOrWhiteSpace(result.ToString) Then
                    result = Nothing '数値の場合Nothing化する
                End If
            End If
            Return result

        End Function

        ''' <summary>
        ''' コントロールへ値をセットする
        ''' </summary>
        ''' <param name="value"></param>
        ''' <remarks></remarks>
        Public Sub setValue(ByVal value As Object)
            DataBinder.setValue(_control, value)
        End Sub

        ''' <summary>
        ''' 自身の情報をControlInfoオブジェクトへ格納
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function createControlInfo() As List(Of GearsControlInfo)
            Dim result As New List(Of GearsControlInfo)
            If TypeOf _control Is GridView Then
                Dim gView As GridView = CType(_control, GridView)
                If gView.DataKeyNames IsNot Nothing AndAlso _
                    gView.DataKeyNames.Length > 0 AndAlso _
                    gView.SelectedDataKey IsNot Nothing Then

                    For Each keyName As String In gView.DataKeyNames
                        Dim cf As New GearsControlInfo(_control.ID, keyName, gView.SelectedDataKey(keyName).ToString())
                        cf.IsKey = True 'GridViewで値が取得できる項目は、いずれもキーとして指定された項目
                        result.Add(cf)
                    Next
                End If
            Else
                result.Add(New GearsControlInfo(Me))
            End If

            Return result

        End Function

        ''' <summary>
        ''' リスト値のシリアライズを行う(主にMultiSelect等の場合、値をシリアライズし複数値を単一値にする)
        ''' </summary>
        ''' <param name="list"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function serializeValue(ByRef list As ArrayList) As String
            Dim str As String = ""
            For Each value As Object In list
                str += value.ToString + VALUE_SEPARATOR
            Next
            If str.Length > 0 Then
                str = str.Substring(0, str.LastIndexOf(VALUE_SEPARATOR))
            End If

            Return str

        End Function

        ''' <summary>
        ''' リスト値のシリアライズを行う(リストコントロール)
        ''' </summary>
        ''' <param name="list"></param>
        Public Shared Function serializeValue(ByRef list As ListControl) As String
            Dim dummy As String = list.SelectedValue 'これを実行しないと選択状態が取れないことがある(バグ?)
            Dim itemList As New ArrayList

            For Each value As ListItem In list.Items
                If value.Selected Then
                    itemList.Add(value.Value)
                End If
            Next

            Return serializeValue(itemList)

        End Function

        ''' <summary>
        ''' リスト値のシリアライズを行う(GridView用)
        ''' </summary>
        ''' <param name="list"></param>
        Public Shared Function serializeValue(ByRef list As DataKey) As String
            If Not list Is Nothing Then
                Dim valueList As New ArrayList
                For i As Integer = 0 To list.Values.Count - 1
                    valueList.Add(list.Values(i).ToString)
                Next
                Return serializeValue(valueList)
            Else
                Return ""
            End If

        End Function

        ''' <summary>
        ''' リスト値のシリアライズを行う(DataTable用)
        ''' </summary>
        ''' <param name="dset"></param>
        ''' <param name="index"></param>
        Public Shared Function serializeValue(ByRef dset As DataTable, Optional ByVal index As Integer = 0) As String
            Dim keyList As New ArrayList
            If Not dset Is Nothing AndAlso dset.PrimaryKey.Count > 0 AndAlso index < dset.Rows.Count Then
                For Each keyColumn As System.Data.DataColumn In dset.PrimaryKey
                    keyList.Add(dset.Rows(index)(keyColumn.ColumnName))
                Next

                Return serializeValue(keyList)

            Else
                Return ""
            End If

        End Function

        ''' <summary>
        ''' IFormItemからControlを取得する<br/>
        ''' IFormItemはラベルとコントロールが一体となったようなコントロールをイメージしています
        ''' </summary>
        ''' <param name="con"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function extractControl(ByVal con As Control) As Control
            If TypeOf con Is IFormItem Then
                Return CType(con, IFormItem).getControl
            Else
                Return con
            End If
        End Function

        ''' <summary>
        ''' シリアライズしたオブジェクトを元に戻す
        ''' </summary>
        ''' <param name="str"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function deSerializeValue(ByVal str As String) As ArrayList
            Dim list As ArrayList = Nothing
            If Not str Is Nothing Then
                list = New ArrayList(str.Split(VALUE_SEPARATOR))
            End If
            Return list
        End Function

        ''' <summary>
        ''' バリデーション結果のメッセージを取得
        ''' </summary>
        Public Function getValidatedMsg() As String Implements IAttributeHolder.getValidatedMsg
            If Not _attributes Is Nothing Then
                Return _attributes.ErrorMessage
            Else
                Return ""
            End If
        End Function

        ''' <summary>
        ''' バリデーションの実行
        ''' </summary>
        Public Function isValidateOk() As Boolean Implements IAttributeHolder.isValidateOk
            If Not _attributes Is Nothing Then
                Return _attributes.isValidateOk(getValue)
            Else
                Return True
            End If
        End Function

        ''' <summary>
        ''' バリデーション対象となる値を取得するための関数実装
        ''' </summary>
        Public ReadOnly Property validateeValue As String Implements IAttributeHolder.validateeValue
            Get
                Return getValue()
            End Get
        End Property

    End Class

End Namespace
