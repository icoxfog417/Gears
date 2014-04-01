Imports Microsoft.VisualBasic

Namespace Gears.Validation

    ''' <summary>
    ''' 属性を保持する要素を定義するインタフェース<br/>
    ''' 属性に基づき、どのような検証を行うか、またどのようなスタイルで表示するかが決定される
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface IAttributeHolder

        ''' <summary>保持している属性</summary>
        Property GAttribute As GearsAttribute

        ''' <summary>
        ''' 検証対象となる値を取得する<br/>
        ''' Validator側で変更が行われないよう、ReadOnly扱いにする。Setterは独自に実装する必要あり
        ''' </summary>
        ReadOnly Property validateeValue As String

        ''' <summary>表示用スタイル</summary>
        ReadOnly Property GCssClass As String

        ''' <summary>バリデーションを実行する</summary>
        Function isValidateOk() As Boolean

        ''' <summary>バリデーションを行った結果のメッセージを取得する</summary>
        Function getValidationError() As String

    End Interface

End Namespace
