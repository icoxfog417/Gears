Imports Microsoft.VisualBasic

Namespace Gears.DataSource

    ''' <summary>
    ''' SQL内で使用する項目を表す抽象クラス
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()>
    Public MustInherit Class SqlItem

        Protected _column As String = ""
        ''' <summary>列項目名</summary>
        Public ReadOnly Property Column() As String
            Get
                Return _column
            End Get
        End Property

        Protected _value As Object = Nothing
        ''' <summary>値</summary>
        Public ReadOnly Property Value() As Object
            Get
                Return _value
            End Get
        End Property

        ''' <summary>前置詞</summary>
        Public Property Prefix() As String

        ''' <summary>キー項目の判定</summary>
        Public Property IsKey() As Boolean

        ''' <summary>関数か否か</summary>
        Public Property IsFunction() As Boolean

        ''' <summary>
        ''' パラメーター名称<br/>
        ''' 実際にフィルタをかけるパラーメータ名(USERID = :pUserというSQLならpUserがパラメーター名)
        ''' </summary>
        Public Property ParamName() As String

        Public Sub New()
        End Sub

        Public Sub New(ByVal item As SqlItem, Optional ByVal val As Object = Nothing)
            _column = item.Column
            _Prefix = item.Prefix
            _IsKey = item.IsKey
            _IsFunction = item.IsFunction
            _ParamName = item.ParamName

            If Not val Is Nothing Then
                _value = val
            Else
                _value = item.Value
            End If

        End Sub

        ''' <summary>
        ''' 値があるか否かの判定
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function hasValue() As Boolean
            If Not String.IsNullOrEmpty(_column) And (Not _value Is Nothing AndAlso Not String.IsNullOrEmpty(_value.ToString)) Then
                Return True
            Else
                Return False
            End If

        End Function

        ''' <summary>
        ''' prefixを付与する<br/>
        ''' 例:SELECT * FROM tab t WHERE t.DATE > '20121212' としたい場合、filter.pf("t")としてprefixを付与する
        ''' </summary>
        ''' <param name="pf"></param>
        ''' <remarks></remarks>
        Protected Sub basePf(ByVal pf As String)
            _Prefix = pf
        End Sub

        ''' <summary>
        ''' SqlDataSourceに設定されたsuffixをprefixとして付与する<br/>
        ''' 例:SELECT * FROM tab t WHERE t.DATE > '20121212' とする場合、DataSourceであるtabのsuffixはtであるため、これをフィルタのprefixとして付与
        ''' </summary>
        ''' <param name="sds"></param>
        ''' <remarks></remarks>
        Protected Sub basePf(ByRef sds As SqlDataSource)
            _Prefix = sds.Suffix
        End Sub

        ''' <summary>
        ''' キーとして設定する
        ''' </summary>
        ''' <remarks></remarks>
        Protected Sub baseAsKey()
            IsKey = True
        End Sub

    End Class

End Namespace