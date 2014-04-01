Imports Microsoft.VisualBasic
Imports System.Runtime.Serialization

Namespace Gears.DataSource

    ''' <summary>
    ''' SQL処理実行時の例外
    ''' </summary>
    ''' <remarks></remarks>
    Public Class GearsSqlException
        Inherits GearsException

        Protected Const MSG_ATYPE As String = "MSG_ATYPE"

        'コンストラクタ
        Public Sub New()
            MyBase.New()
        End Sub

        Public Sub New(ByVal msg As String)
            MyBase.New(msg)
        End Sub

        Public Sub New(atype As ActionType, message As String, ByVal innerException As Exception)
            MyBase.New(message, innerException)
            setActionType(atype)
        End Sub
        Public Sub New(atype As ActionType, message As String, ByVal ParamArray detail() As String)
            MyBase.New(message)
            addDetail(detail)
            setActionType(atype)
        End Sub

        'メソッド
        Public Function getActionType() As String
            Return toStringDetail(MSG_ATYPE)
        End Function
        Public Sub clearActionType()
            clearDetail(MSG_ATYPE)
        End Sub
        Public Sub setActionType(ByVal a As ActionType)
            addDetail(MSG_ATYPE, GearsDTO.ActionToString(a))
        End Sub

    End Class

End Namespace
