Imports Microsoft.VisualBasic
Imports Gears
Imports Gears.DataSource

Namespace DataSource

    Public Class EMP
        Inherits GDSTemplate

        Public Sub New(ByVal conStr As String)
            MyBase.New(conStr, SqlBuilder.DS("V_EMP"), SqlBuilder.DS("EMP"))
            addLockCheckCol("UPD_YMD", LockType.UDATESTR)
            addLockCheckCol("UPD_HMS", LockType.UTIMESTR)
        End Sub

    End Class

End Namespace
