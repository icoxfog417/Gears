Imports Microsoft.VisualBasic


Namespace Gears.Util

    ''' <summary>
    ''' ラベル/コントロールが一体となったユーザーコントロールを想定したインタフェース<br/>
    ''' これをユーザーコントロール側で実装することで、画面の作成が容易になります。
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface IFormItem

        ''' <summary>内部のコントロールID</summary>
        ReadOnly Property ControlId() As String

        ''' <summary>内部のコントロール種別</summary>
        Property ControlKind() As String

        ''' <summary>コントロールに対するラベルテキスト</summary>
        Property LabelText() As String

        ''' <summary>編集可能か否かの設定</summary>
        Property IsEditable() As Boolean

        ''' <summary>コントロールの幅</summary>
        Property Width() As Integer

        ''' <summary>コントロールの高さ</summary>
        Property Height() As Integer

        ''' <summary>コントロールのCssClass</summary>
        Property CssClass() As String

        ''' <summary>接続文字列</summary>
        Property ConnectionName() As String

        ''' <summary>データソースの名称空間</summary>
        Property DsNamespace() As String

        ''' <summary>内部のコントロールを取得する</summary>
        Function getControl() As WebControl

        ''' <summary>内部のコントロールに対し値を設定する</summary>
        Sub setValue(ByVal value As String)

        ''' <summary>内部のコントロールから値を取得する</summary>
        Function getValue() As String

    End Interface

End Namespace
