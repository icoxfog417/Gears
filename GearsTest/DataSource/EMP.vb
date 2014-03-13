Imports Microsoft.VisualBasic
Imports Gears
Imports Gears.DataSource

Namespace DataSource

    Public Class EMP
        Inherits GearsDataSource

        Public Sub New(ByVal conStr As String)
            MyBase.New(conStr, SqlBuilder.DS("V_EMP"), SqlBuilder.DS("EMP"))
            setLockCheckColumn("UPD_YMD", LockType.UDATESTR)
            setLockCheckColumn("UPD_HMS", LockType.UTIMESTR)
        End Sub

    End Class

End Namespace
