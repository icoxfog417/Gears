Imports Microsoft.VisualBasic

Namespace Gears.Validation

    Public Class GearsAttributeContainer
        Inherits GearsAttribute '面倒なので継承してしまっているが、不都合出るようなら解消(Compositパターンに則るならインタフェースだが、実装がかぶるのでこの形)

        Private attributes As New List(Of GearsAttribute)

        Public Sub New()
            MyBase.New()
        End Sub
        Public Sub New(ga As GearsAttributeContainer) 'コピーコンストラクタ
            MyBase.New()
            attributes = New List(Of GearsAttribute)(ga.ListUp())
        End Sub

        Public Overrides Function ListUp() As List(Of GearsAttribute)
            Return attributes
        End Function

        Public Sub addAttribute(a As GearsAttribute)
            attributes.Add(a)
        End Sub

        Public Sub clearAttributes()
            initProperty()
            attributes.Clear()
        End Sub
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

