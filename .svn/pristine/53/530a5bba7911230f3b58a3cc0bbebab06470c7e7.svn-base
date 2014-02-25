Imports Microsoft.VisualBasic
Imports Gears

Namespace DataSource.Groups
    Public Class GROUPN
        Inherits GDSTemplate

        Public Sub New(ByVal conStr As String)
            MyBase.New(conStr, SqlBuilder.newDataSource("GROUPN"))
            If GExecutor.getDbServerType = DbServerType.OLEDB Then
                setViewAndTarget(SqlBuilder.newDataSource("[GROUPN$]"))
            End If

        End Sub

        Public Overrides Function makeSqlBuilder(ByRef data As Gears.GearsDTO) As SqlBuilder
            Dim sqlb As SqlBuilder = MyBase.makeSqlBuilder(data)

            sqlb.addSelection(SqlBuilder.newSelect("GROUPN").ASC)
            sqlb.addSelection(SqlBuilder.newSelect("GROUPNAME"))

            Return sqlb

        End Function

    End Class

End Namespace
