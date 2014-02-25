Imports Microsoft.VisualBasic
Imports System.Web.UI
Imports System.Collections
Imports System
Imports System.Web.UI.WebControls
Imports System.Reflection

Namespace Gears

    ''' <summary>
    ''' System.Web.UI.WebControlをラップするControl<br/>
    ''' 特定のネーミングルールに基づいたControl IDから、データソースクラスを判定し値をロードします→
    ''' <a href="http://gearssite.apphb.com/GearsSampleControl.aspx" target="_blank">詳細:名称規約について</a><br/>
    ''' 
    ''' </summary>
    ''' <remarks></remarks>
    Public Class GearsControl
        Implements IAttributeHolder

        ''' <summary>IDに含まれることで、更新用フォームであることを示す文字列</summary>
        Public Const FORM_ATTRIBUTE As String = "GFORM"

        ''' <summary>IDに含まれることで、検索フォームであることを示す文字列</summary>
        Public Const FILTER_ATTRIBUTE As String = "GFILTER"

        ''' <summary>IDに含まれることで、更新キーであることを示す文字列</summary>
        Public Const KEY_ATTRIBUTE As String = "KEY"

        ''' <summary>検索時のオペレーター指定</summary>
        Public Const KEY_OPERATOR As String = "OPERATOR"

        ''' <summary>Serializeを行うための区切り文字</summary>
        Public Const VALUE_SEPARATOR As String = vbVerticalTab

        ''' <summary>ID内をSplitするためのSeparator</summary>
        Private Const ID_SEPARATOR As String = "__"

        ''' <summary>
        ''' データソースクラスが格納されているアセンブリ名
        ''' </summary>
        Private Shared DataSourceAssembleyName As String = ""

        Private control As Control = Nothing
        ''' <summary>
        ''' 基となるコントロール
        ''' </summary>
        Public ReadOnly Property ControlID() As String
            Get
                Return control.ID
            End Get
        End Property

        Private _connectionName As String
        ''' <summary>
        ''' DBにアクセスするための接続文字列
        ''' </summary>
        Public Property ConnectionName() As String
            Get
                Return _connectionName
            End Get
            Set(ByVal value As String)
                _connectionName = value
            End Set
        End Property

        Private _dsNameSpace As String
        ''' <summary>
        ''' データソースクラスの名称空間
        ''' </summary>
        Public Property DsNameSpace() As String
            Get
                Return _dsNameSpace
            End Get
            Set(ByVal value As String)
                _dsNameSpace = value
            End Set
        End Property

        Private _dataSourceID As String = ""
        ''' <summary>
        ''' データロード元となるデータソースクラスの名称(基本的にIDから判断される)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property DataSourceID() As String
            Get
                Return _dataSourceID
            End Get
        End Property

        Private _operatorAttribute As String = ""
        ''' <summary>
        ''' 検索時のオペレーターを示す値(主に検索フォームでのlikeなど)
        ''' </summary>
        Public Property OperatorAttribute() As String
            Get
                Return _operatorAttribute
            End Get
            Set(ByVal value As String)
                _operatorAttribute = value
            End Set
        End Property

        Private _controlType As String = ""
        ''' <summary>
        ''' コントロール種別を表す文字列
        ''' </summary>
        Public ReadOnly Property ControlType() As String
            Get
                Return _controlType
            End Get
        End Property

        Private _isKey As Boolean = False
        ''' <summary>
        ''' コントロールがキーか否か
        ''' </summary>
        Public ReadOnly Property IsKey As Boolean
            Get
                Return _isKey
            End Get
        End Property

        <Obsolete("IsKeyプロパティを使用してください")>
        Public Sub setAskey()
            _isKey = True
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

        ''' <summary>コントロールに設定されたIDをID_SEPARATORでSplitしたもの</summary>
        Private idAttributes As ArrayList = New ArrayList()

        ''' <summary>前回ロードされた値</summary>
        Private loadedValue As String = ""

        Private dataBinder As IDataBinder = New GBinderTemplate
        Private dataSource As GearsDataSource = Nothing
        Private _attributes As GearsAttribute = Nothing
        Public Property GAttribute As GearsAttribute Implements IAttributeHolder.GAttribute
            Get
                Return _attributes
            End Get
            Set(value As GearsAttribute)
                If TypeOf _attributes Is GearsAttributeContainer And TypeOf value Is GearsAttribute Then
                    CType(_attributes, GearsAttributeContainer).addAttribute(value)
                Else
                    _attributes = value
                End If
            End Set
        End Property

        Private _gcssClass As String = ""
        Public ReadOnly Property GCssClass As String Implements IAttributeHolder.GCssClass
            Get
                Return _gcssClass
            End Get
        End Property

        'コンストラクタ
        Public Sub New(ByRef con As Control, ByVal conName As String, Optional ByVal dns As String = "", Optional isAutoLoadAttr As Boolean = True)
            ConnectionName = conName
            DsNameSpace = dns
            initInstance(con, isAutoLoadAttr)

        End Sub
        Public Sub New(ByRef con As Control, ByRef gs As GearsDataSource, Optional isAutoLoadAttr As Boolean = True)
            ConnectionName = gs.getConnectionName
            dataSource = gs
            initInstance(con, isAutoLoadAttr)

        End Sub
        Private Sub initInstance(ByRef con As Control, ByVal isAutoLoadAttr As Boolean)
            control = con
            readIDByGearsRule(con.ID)

            'WHEREを作成する際のオペレーターの設定
            If TypeOf con Is WebControl Then
                Dim wcon As WebControl = CType(con, WebControl)
                If Not wcon.Attributes(KEY_OPERATOR) Is Nothing Then
                    _operatorAttribute = wcon.Attributes(KEY_OPERATOR)
                End If
            End If

            loadDataSource()

            If isAutoLoadAttr Then
                loadAttribute()
            End If

        End Sub

        Private Sub readIDByGearsRule(ByVal id As String) 'コントロール種別とデータソース名を取得
            If id <> "" Then
                Dim idElements As String() = id.Split(New String() {ID_SEPARATOR}, StringSplitOptions.None)
                _controlType = idElements(0).Substring(0, 3).ToUpper
                _dataSourceID = idElements(0).Substring(3)

                If idElements.Length > 1 Then
                    For i As Integer = 1 To idElements.Length - 1
                        idAttributes.Add(idElements(i).ToUpper) '全て大文字にして格納
                    Next

                End If

                'キー値設定
                If isIdAttributeExist(KEY_ATTRIBUTE) Then
                    _isKey = True
                End If

                'コントロールの種別設定
                If DataSourceID.ToUpper = FORM_ATTRIBUTE Or isIdAttributeExist(FORM_ATTRIBUTE) Then
                    _isFormAttribute = True
                ElseIf DataSourceID.ToUpper = FILTER_ATTRIBUTE Or isIdAttributeExist(FILTER_ATTRIBUTE) Then
                    _isFilterAttribute = True
                End If

            End If
        End Sub
        Public Sub loadDataSource()
            If ConnectionName <> "" And DataSourceID <> "" Then
                Try
                    Dim className As String = ""
                    If Not String.IsNullOrEmpty(DsNameSpace) Then
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
                        dataSource = CType(instance, GearsDataSource)
                    End If

                Catch ex As Exception
                    GearsLogStack.setLog("コントロール" + control.ID + " へのデータソース登録に失敗しました(データソースID：" + DataSourceID + ")")

                End Try

            End If
        End Sub
        Public Sub loadAttribute(Optional ByVal css As String = "")
            Dim attributeStr As String = ""
            If css = "" Then
                If TypeOf control Is WebControl Then
                    attributeStr = CType(control, WebControl).CssClass
                End If
            Else
                attributeStr = css
            End If

            'CssスタイルからAttributeの取得
            Dim attrCreator As New GearsAttributeCreator()
            attrCreator.createAttributesFromString(attributeStr)
            _gcssClass = attrCreator.getCssClass
            _attributes = attrCreator.pack

        End Sub


        'コントロールの設定
        Public Function getControl() As Control
            Return control
        End Function

        'データソースの設定
        Public Sub setDataSource(ByRef ds As GearsDataSource)
            dataSource = ds
        End Sub
        Public Function getDataSource() As GearsDataSource
            Return dataSource
        End Function

        'バインダーの設定
        Public Sub setDataBinder(ByRef dbn As IDataBinder)
            dataBinder = dbn
        End Sub
        Public Function getDataBinder() As IDataBinder
            Return dataBinder
        End Function

        'その他getter/setter
        Public Shared Function getDataSourceid(ByVal id As String) As String
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
        Public Shared Function getControlType(ByVal id As String) As String
            Dim idElements As String() = id.Split(New String() {ID_SEPARATOR}, StringSplitOptions.None)
            Return idElements(0).Substring(0, 3).ToUpper

        End Function
        Public Function getIdAttributes() As ArrayList
            Return idAttributes
        End Function
        Public Function isIdAttributeExist(ByVal attr As String) As Boolean
            Return isIdAttributeExist(control.ID, attr)
        End Function
        Public Shared Function isIdAttributeExist(ByVal id As String, ByVal attr As String) As String
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

        Public Sub setLoadedValue(ByVal val As String)
            loadedValue = val
        End Sub
        Public Function getLoadedValue() As String
            Return loadedValue
        End Function

        '相手先のコントロールに対する自己の振る舞いを定義するプロシージャ
        Public Function behave(ByRef dataObject As GearsDTO) As Boolean
            '自分のデータソースに相手先の変化を通知する
            If Not dataSource Is Nothing Then
                Try
                    '自身へのバインド
                    Dim behavedData = dataSource.execute(dataObject)
                    dataBind(behavedData)
                Catch ex As Exception
                    Throw
                End Try
            Else
                GearsLogStack.setLog("コントロール" + control.ID + " にはデータソースが登録されていないため、処理は行われません")

            End If
            Return True
        End Function

        Public Sub init(Optional ByVal dto As GearsDTO = Nothing)
            If Not dataSource Is Nothing Then
                Dim resultSet As System.Data.DataTable = Nothing
                If dto Is Nothing Then
                    Dim tempDto As GearsDTO = New GearsDTO(ActionType.SEL)
                    resultSet = dataSource.execute(tempDto)
                Else
                    resultSet = dataSource.execute(dto)
                End If
                dataBind(resultSet)

            End If
        End Sub
        Public Sub reload(Optional ByVal dto As GearsDTO = Nothing)
            Dim pastValue As String = Me.getValue 'コントロールに現在設定されているのデータをキープ
            init(dto)
            Me.setValue(pastValue)
        End Sub

        '自身へのバインディング
        'データをコントロールにバインドするためのメソッド
        Public Function dataBind(ByRef dset As System.Data.DataTable) As Boolean
            Dim result As Boolean = True
            Try
                result = dataBinder.dataBind(control, dset)
            Catch ex As Exception
                Throw
            End Try

            Return result

        End Function

        Public Function dataAttach(ByRef gds As GearsDataSource) As Boolean
            If Not gds Is Nothing Then
                Try
                    dataAttach(gds.gResultSet)
                Catch ex As Exception
                    Throw
                End Try
            End If

            Return True
        End Function
        Public Function dataAttach(Optional ByRef dset As System.Data.DataTable = Nothing) As Boolean
            Dim result As Boolean = True
            'デリゲート処理用グローバル変数　使用後要削除
            Dim dataPasser As System.Data.DataTable = Nothing

            If dset Is Nothing And Not dataSource Is Nothing Then
                dataPasser = dataSource.gResultSet
            Else
                dataPasser = dset
            End If

            Try
                dataBinder.dataAttach(control, dataPasser)
            Catch ex As Exception
                Throw
            End Try

            Return result

        End Function

        '自コントロールの値取得
        Public Function getValue() As String
            Dim result As String = dataBinder.getValue(control)
            '数値型の場合は、空白だと型変換ができないためNothing(NULL)での更新をおこなう
            If Not _attributes Is Nothing AndAlso _
                (_attributes.hasMarker(GetType(MarkerNumeric)) And String.IsNullOrEmpty(result)) Then
                Return Nothing
            Else
                Return result
            End If
            Return result
        End Function

        '自コントロールへの値セット
        Public Sub setValue(ByVal value As String)
            dataBinder.setValue(control, value)
        End Sub

        '自分自身の情報をオブジェクトに格納
        Public Function createControlInfo() As List(Of GearsControlInfo)
            Dim result As New List(Of GearsControlInfo)
            If TypeOf control Is GridView Then
                If Not CType(control, GridView).DataKeyNames Is Nothing AndAlso CType(control, GridView).DataKeyNames.Length > 0 AndAlso Not CType(control, GridView).SelectedDataKey Is Nothing Then
                    For Each keyName As String In CType(control, GridView).DataKeyNames
                        Dim cf As New GearsControlInfo(control.ID, keyName, CType(control, GridView).SelectedDataKey(keyName).ToString())
                        cf.IsKey = True 'GridViewで値が取得できる項目は、いずれもキーとして指定された項目
                        result.Add(cf)
                    Next
                End If
            Else
                result.Add(New GearsControlInfo(Me))
            End If

            Return result

        End Function

        'コントロールの値を文字列化する
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
        Public Shared Function serializeValue(ByRef dset As System.Data.DataTable, Optional ByVal index As Integer = 0) As String
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

        '文字列化したオブジェクトを配列化する
        Public Shared Function deSerializeValue(ByVal str As String) As ArrayList
            Dim list As ArrayList = Nothing
            If Not str Is Nothing Then
                list = New ArrayList(str.Split(VALUE_SEPARATOR))
            End If
            Return list
        End Function


        Public Function getValidatedMsg() As String Implements IAttributeHolder.getValidatedMsg
            If Not _attributes Is Nothing Then
                Return _attributes.ErrorMessage
            Else
                Return ""
            End If
        End Function

        Public Function isValidateOk() As Boolean Implements IAttributeHolder.isValidateOk
            If Not _attributes Is Nothing Then
                Return _attributes.isValidateOk(getValue)
            Else
                Return True
            End If
        End Function
        Public ReadOnly Property validateeValue As String Implements IAttributeHolder.validateeValue
            Get
                Return getValue()
            End Get
        End Property

    End Class

End Namespace
