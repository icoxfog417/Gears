Imports Microsoft.VisualBasic

Namespace Gears.DataSource

    ''' <summary>
    ''' 楽観ロックの例外
    ''' </summary>
    ''' <remarks></remarks>
    Public Class GearsOptimisticLockException
        Inherits GearsSqlException

        Public Sub New()
            MyBase.New("他のユーザーにより更新されているため、更新出来ません。再読込みを行って下さい。")
        End Sub

        Public Sub New(ByVal ParamArray detail() As String)
            Me.New()
            addDetail(detail)
        End Sub

    End Class

End Namespace
