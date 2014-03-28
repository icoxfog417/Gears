
Namespace Gears.JS

    ''' <summary>
    ''' WebPage上で、JavaScriptオブジェクトとして扱いたい変数に対し付与するAttribute。<br/>
    ''' このアトリビュートを指定した変数は、gears.v.xxx で値を取得できる。
    ''' </summary>
    ''' <remarks></remarks>
    Public Class JSVariableAttribute
        Inherits System.Attribute

        ''' <summary>JavaScript側とサーバーサイドのプロパティ名で名前を変えたい場合に使用</summary>
        Public Property Name As String = String.Empty

    End Class

End Namespace
