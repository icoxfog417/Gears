Imports Microsoft.VisualBasic

Namespace Gears.Validation

    ''' <summary>
    ''' 複数の属性を格納するコンテナ
    ''' </summary>
    ''' <remarks></remarks>
    Public Class GearsAttributeContainer
        Inherits GearsAttribute '面倒なので継承してしまっているが、不都合出るようなら解消(Compositパターンに則るならインタフェースだが、実装がかぶるのでこの形)

        Private attributes As New List(Of GearsAttribute)

        Public Sub New()
            MyBase.New()
        End Sub

        Public Sub New(ga As GearsAttributeContainer)
            MyBase.New()
            attributes = New List(Of GearsAttribute)(ga.ListUp())
        End Sub

        ''' <summary>
        ''' 内部に格納している属性を取得する
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function ListUp() As List(Of GearsAttribute)
            Return attributes
        End Function

        ''' <summary>
        ''' 属性を追加する
        ''' </summary>
        ''' <param name="a"></param>
        ''' <remarks></remarks>
        Public Sub addAttribute(a As GearsAttribute)
            attributes.Add(a)
        End Sub

        ''' <summary>
        ''' 格納している属性をクリアする
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub clearAttributes()
            initProperty()
            attributes.Clear()
        End Sub

        ''' <summary>
        ''' 格納している属性の中に、指定されたマーカータイプを持つものがないか確認する
        ''' </summary>
        ''' <param name="m"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function hasMarker(m As Type) As Boolean
            Dim result As Boolean = False

            For Each a As GearsAttribute In attributes
                If a.hasMarker(m) Then
                    result = True
                    Exit For
                End If
            Next

            Return result

        End Function

        ''' <summary>
        ''' 格納されている属性のCssClassを結合し、返却する
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Property CssClass() As String
            Get
                Dim csss As String = ""
                For Each item As GearsAttribute In attributes
                    csss += item.CssClass + " "
                Next
                Return Trim(csss)   '最終空白の除去
            End Get
            Protected Set(ByVal value As String)
                MyBase.CssClass = value
            End Set
        End Property

        ''' <summary>
        ''' 格納されている属性を使用し検証処理を実施する(外部からの呼び出し不可、isValidateOkからコールされる)
        ''' </summary>
        ''' <remarks></remarks>
        Protected Overrides Sub Validate()
            initProperty()
            For Each item As GearsAttribute In attributes
                If item.isValidateOk(ValidateeValue) = False Then
                    IsValid = False
                    ErrorMessage = item.ErrorMessage
                    Exit For
                End If
            Next
        End Sub

    End Class

End Namespace

