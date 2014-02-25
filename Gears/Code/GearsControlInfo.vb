Imports Microsoft.VisualBasic
Imports System.Collections.Generic
Imports System.Web.UI.WebControls

Namespace Gears

    <Serializable()>
    Public Class GearsControlInfo

        Private _controlId As String = ""
        Public ReadOnly Property ControlID() As String
            Get
                Return _controlId
            End Get
        End Property

        Private _dataSourceId As String = ""
        Public ReadOnly Property DataSourceID() As String
            Get
                Return _dataSourceId
            End Get
        End Property

        Private _value As String = ""
        Public ReadOnly Property Value() As String
            Get
                Return _value
            End Get
        End Property

        Private _pastValue As String = ""
        Public ReadOnly Property PastValue() As String
            Get
                Return _pastValue
            End Get
        End Property

        Private _isFormAttribute As Boolean = False
        Public Property IsFormAttribute() As Boolean
            Get
                Return _isFormAttribute
            End Get
            Set(ByVal value As Boolean)
                _isFormAttribute = value
            End Set
        End Property

        Private _isFilterAttribute As Boolean = False
        Public Property IsFilterAttribute() As Boolean
            Get
                Return _isFilterAttribute
            End Get
            Set(ByVal value As Boolean)
                _isFilterAttribute = value
            End Set
        End Property

        Private _isKey As Boolean = False
        Public Property IsKey() As Boolean
            Get
                Return _isKey
            End Get
            Set(ByVal value As Boolean)
                _isKey = value
            End Set
        End Property

        Private _operatorAttribute As String = ""
        Public Property OperatorAttribute() As String
            Get
                Return _operatorAttribute
            End Get
            Set(ByVal value As String)
                _operatorAttribute = value
            End Set
        End Property

        Public Sub New(ByVal conId As String, ByVal ds As String, ByVal val As String, Optional ByVal p As String = "")
            _controlId = conId
            _dataSourceId = ds
            _value = val
            _pastValue = p
        End Sub
        Public Sub New(ByVal conId As String, ByVal ds As String, ByVal val As String, ByVal p As String, _
                       ByVal key As Boolean, ByVal form As Boolean, ByVal filter As Boolean, ByVal opr As String)
            _controlId = conId
            _dataSourceId = ds
            _value = val
            _pastValue = p
            _isKey = key
            _isFormAttribute = form
            _isFilterAttribute = filter
            _operatorAttribute = opr
        End Sub

        Public Sub New(ByRef g As GearsControl)
            Me.New(g.ControlID, g.DataSourceID, g.getValue, g.getLoadedValue, _
                   g.key, g.IsFormAttribute, g.IsFilterAttribute, g.OperatorAttribute)
        End Sub
        Public Sub New(ByRef g As GearsControlInfo)
            Me.New(g.ControlID, g.DataSourceID, g.Value, g.PastValue, _
                   g.IsKey, g.IsFormAttribute, g.IsFilterAttribute, g.OperatorAttribute)
        End Sub


        Public Overrides Function toString() As String
            Dim str As String = ""
            '文字数の都合上英語表示
            str += _dataSourceId
            If String.IsNullOrEmpty(_operatorAttribute) Then
                str += " = "
            Else
                str += " " + _operatorAttribute + " "
            End If

            str += Value + " ( past : " + PastValue + ")"
            If _isKey Then
                str += Value + " !key! "
            End If

            Return str

        End Function

    End Class

End Namespace
