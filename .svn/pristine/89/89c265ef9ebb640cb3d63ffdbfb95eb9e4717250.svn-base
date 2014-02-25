Imports Microsoft.VisualBasic
Imports System.Runtime.Serialization

Namespace Gears

    Public Class GearsDataAccessException
        Inherits GearsException

        Protected Const MSG_ATYPE As String = "MSG_ATYPE"

        'コンストラクタ
        Public Sub New()
            MyBase.new()
        End Sub
        Public Sub New(atype As ActionType, message As String, ByVal innerException As Exception)
            MyBase.New(message, innerException)
            setActionType(atype)
        End Sub
        Public Sub New(atype As ActionType, message As String, ByVal ParamArray detail() As String)
            MyBase.New(message)
            addMsgDebug(detail)
            setActionType(atype)
        End Sub

        'メソッド
        Public Function getActionType() As String
            Return getMsg(MSG_ATYPE)
        End Function
        Public Sub clearActionType()
            clearMsg(MSG_ATYPE)
        End Sub
        Public Sub setActionType(ByVal a As ActionType)
            setMsg(MSG_ATYPE, GearsDTO.getAtypeString(a))
        End Sub

    End Class

End Namespace
