Imports Microsoft.VisualBasic
Imports System.Collections.Generic
Imports System.Web.UI.WebControls

Namespace Gears

    ''' <summary>
    ''' GearsControlの値や設定情報をまとめたクラス。<br/>
    ''' GearsControl本体はWebControlを含む重たいクラスであるため、本クラスに値を移すことで扱いやすくする
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()>
    Public Class GearsControlInfo

        Private _controlId As String = ""
        ''' <summary>
        ''' コントロールID
        ''' </summary>
        Public ReadOnly Property ControlID() As String
            Get
                Return _controlId
            End Get
        End Property

        Private _dataSourceId As String = ""
        ''' <summary>
        ''' データソースクラス名
        ''' </summary>
        Public ReadOnly Property DataSourceID() As String
            Get
                Return _dataSourceId
            End Get
        End Property

        Private _value As String = ""
        ''' <summary>
        ''' コントロールの値
        ''' </summary>
        Public ReadOnly Property Value() As String
            Get
                Return _value
            End Get
        End Property

        Private _isFormAttribute As Boolean = False
        ''' <summary>
        ''' 更新フォーム属性
        ''' </summary>
        Public Property IsFormAttribute() As Boolean
            Get
                Return _isFormAttribute
            End Get
            Set(ByVal value As Boolean)
                _isFormAttribute = value
            End Set
        End Property

        Private _isFilterAttribute As Boolean = False
        ''' <summary>
        ''' 検索フォーム属性
        ''' </summary>
        Public Property IsFilterAttribute() As Boolean
            Get
                Return _isFilterAttribute
            End Get
            Set(ByVal value As Boolean)
                _isFilterAttribute = value
            End Set
        End Property

        Private _isKey As Boolean = False
        ''' <summary>
        ''' キーか否か
        ''' </summary>
        Public Property IsKey() As Boolean
            Get
                Return _isKey
            End Get
            Set(ByVal value As Boolean)
                _isKey = value
            End Set
        End Property

        Private _operatorAttribute As String = ""
        ''' <summary>
        ''' 検索する際のオペレーター設定(=や>=など)
        ''' </summary>
        Public Property OperatorAttribute() As String
            Get
                Return _operatorAttribute
            End Get
            Set(ByVal value As String)
                _operatorAttribute = value
            End Set
        End Property

        ''' <summary>前回ロードされた値</summary>
        Public Property LoadedValue As String = ""

        ''' <summary>
        ''' 最低限の情報をセットするコンストラクタ
        ''' </summary>
        ''' <param name="conId"></param>
        ''' <param name="dsID"></param>
        ''' <param name="value"></param>
        ''' <remarks></remarks>
        Public Sub New(ByVal conId As String, ByVal dsId As String, ByVal value As String)
            _controlId = conId
            _dataSourceId = dsId
            _value = value
        End Sub

        ''' <summary>
        ''' GearsControlから生成を行うコンストラクタ
        ''' </summary>
        ''' <param name="gcon"></param>
        ''' <remarks></remarks>
        Public Sub New(ByRef gcon As GearsControl)
            Me.New(gcon.ControlID, gcon.DataSourceID, gcon.getValue, gcon.LoadedValue, _
                   gcon.IsKey, gcon.IsFormAttribute, gcon.IsFilterAttribute, gcon.OperatorAttribute)
        End Sub

        ''' <summary>
        ''' コピーコンストラクタ
        ''' </summary>
        ''' <param name="cInfo"></param>
        ''' <remarks></remarks>
        Public Sub New(ByRef cInfo As GearsControlInfo)
            Me.New(cInfo.ControlID, cInfo.DataSourceID, cInfo.Value, cInfo.LoadedValue, _
                   cInfo.IsKey, cInfo.IsFormAttribute, cInfo.IsFilterAttribute, cInfo.OperatorAttribute)
        End Sub

        Private Sub New(ByVal conId As String, ByVal dsId As String, ByVal value As String, ByVal lvalue As String, _
               ByVal isKey As Boolean, ByVal isForm As Boolean, ByVal isFilter As Boolean, ByVal opr As String)
            _controlId = conId
            _dataSourceId = dsId
            _value = value
            LoadedValue = lvalue
            _isKey = isKey
            _isFormAttribute = isForm
            _isFilterAttribute = isFilter
            _operatorAttribute = opr
        End Sub

        Public Overrides Function toString() As String
            Dim result As String = _dataSourceId + " {0} " + Value
            result = String.Format(result, If(Not String.IsNullOrEmpty(_operatorAttribute), _operatorAttribute, "="))

            If IsKey Then result = result + "[Key]"

            Return result

        End Function

    End Class

End Namespace
