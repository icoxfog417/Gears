Imports Microsoft.VisualBasic
Imports Gears

Namespace DataSource.Groups
    Public Class AREA
        Inherits GDSTemplate

        Public Sub New(ByVal conStr As String)
            MyBase.New(conStr, SqlBuilder.newDataSource("AREA"))
            If GExecutor.getDbServerType = DbServerType.OLEDB Then
                setViewAndTarget(SqlBuilder.newDataSource("[AREA$]"))
            End If

        End Sub

        Public Overrides Function makeSqlBuilder(ByRef data As Gears.GearsDTO) As SqlBuilder
            Dim sqlb As SqlBuilder = MyBase.makeSqlBuilder(data)

            sqlb.addSelection(SqlBuilder.newSelect("AREA").ASC)
            sqlb.addSelection(SqlBuilder.newSelect("AREANAME"))

            Return sqlb

        End Function

    End Class

End Namespace
