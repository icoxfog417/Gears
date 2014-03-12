Imports Microsoft.VisualBasic
Imports Gears
Imports Gears.DataSource

Namespace DataSource
    Public Class GROUPN
        Inherits GDSTemplate

        Public Sub New(ByVal conStr As String)
            MyBase.New(conStr, SqlBuilder.DS("GROUPN"))
            If GExecutor.getDbServerType = DbServerType.OLEDB Then
                setViewAndTarget(SqlBuilder.DS("[GROUPN$]"))
            End If

        End Sub

        Public Overrides Function makeSqlBuilder(ByRef data As Gears.GearsDTO) As SqlBuilder
            Dim sqlb As SqlBuilder = MyBase.makeSqlBuilder(data)

            sqlb.addSelection(SqlBuilder.S("GROUPN").ASC)
            sqlb.addSelection(SqlBuilder.S("GROUPNAME"))

            Return sqlb

        End Function

    End Class

End Namespace
