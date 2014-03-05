Imports Microsoft.VisualBasic
Imports Gears
Imports Gears.DataSource

Namespace DataSource.Groups
    Public Class UNITS
        Inherits GDSTemplate

        Public Sub New(ByVal conStr As String)
            MyBase.New(conStr, SqlBuilder.newDataSource("UNITS"))
            If GExecutor.getDbServerType = DbServerType.OLEDB Then
                setViewAndTarget(SqlBuilder.newDataSource("[UNITS$]"))
            End If

        End Sub

        Public Overrides Function makeSqlBuilder(ByRef data As Gears.GearsDTO) As SqlBuilder
            Dim sqlb As SqlBuilder = MyBase.makeSqlBuilder(data)

            sqlb.addSelection(SqlBuilder.newSelect("UNITS").ASC)
            sqlb.addSelection(SqlBuilder.newSelect("UNITSNAME"))

            Return sqlb

        End Function

    End Class

End Namespace
