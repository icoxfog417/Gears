Imports Microsoft.VisualBasic
Imports System.Data
Imports Gears

Namespace GSDataSource

    Public Class COMP_GRP
        Inherits GDSTemplate
        Public Sub New(ByVal conName As String)
            MyBase.New(conName, SqlBuilder.newDataSource("CMN_M_HANYO"))

        End Sub

        Public Overrides Function makeSqlBuilder(ByRef data As Gears.GearsDTO) As SqlBuilder
            Dim sqlb As SqlBuilder = MyBase.makeSqlBuilder(data)
            sqlb.addSelection(SqlBuilder.newSelect("HYMST1_KEY"))
            sqlb.addSelection(SqlBuilder.newSelect("HYMST1_TXT"))
            sqlb.addFilter(SqlBuilder.newFilter("HYMST_ID").eq("CM010"))

            Dim convertor As New ViewItemAndColumnMapperTemplete
            convertor.addRule("COMP_GRP", "HYMST1_KEY")
            convertor.addRule("COMP_UNIT", "HYMST2_KEY")
            sqlb.setdsColConvertor(convertor)

            Return sqlb

        End Function


    End Class

End Namespace
