Imports Microsoft.VisualBasic

Namespace Gears

    <Serializable()>
    Public Class SqlItem
        Private _isKey As Boolean = False
        Public Property IsKey() As Boolean
            Get
                Return _isKey
            End Get
            Set(ByVal value As Boolean)
                _isKey = value
            End Set
        End Property

        Protected _column As String = ""
        Public ReadOnly Property Column() As String
            Get
                Return _column
            End Get
        End Property

        Protected _value As Object = Nothing
        Public ReadOnly Property Value() As Object
            Get
                Return _value
            End Get
        End Property

        Protected _prefix As String = ""
        Public Property Prefix() As String
            Get
                Return _prefix
            End Get
            Set(ByVal value As String)
                _prefix = value
            End Set
        End Property

        Protected _isFunction As Boolean = False
        Public Property IsFunction() As Boolean
            Get
                Return _isFunction
            End Get
            Set(ByVal value As Boolean)
                _isFunction = value
            End Set
        End Property

        Public Sub New()

        End Sub
        Public Sub New(ByVal item As SqlItem, Optional ByVal val As Object = Nothing)
            _isKey = item.IsKey
            _column = item.Column
            _prefix = item.Prefix

            If Not val Is Nothing Then
                _value = val
            Else
                _value = item.Value
            End If

        End Sub

        Public Overridable Function hasValue() As Boolean
            If Not String.IsNullOrEmpty(_column) And (Not _value Is Nothing AndAlso Not String.IsNullOrEmpty(_value.ToString)) Then
                Return True
            Else
                Return False
            End If

        End Function

    End Class

End Namespace