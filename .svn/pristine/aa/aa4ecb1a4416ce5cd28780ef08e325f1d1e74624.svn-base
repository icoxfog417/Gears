Imports Microsoft.VisualBasic
Imports System.Data
Imports Gears

Namespace GSDataSource

    Public Class MK_FLG
        Inherits GDSTemplate
        Public Sub New(ByVal conName As String)
            MyBase.New(conName, SqlBuilder.newDataSource("CMN_M_HANYO"))

        End Sub

        Public Overrides Function makeSqlBuilder(ByRef data As Gears.GearsDTO) As SqlBuilder
            Dim sqlb As SqlBuilder = MyBase.makeSqlBuilder(data)
            sqlb.addSelection(SqlBuilder.newSelect("HYMST1_KEY"))
            sqlb.addSelection(SqlBuilder.newSelect("HYMST1_TXT"))
            sqlb.addFilter(SqlBuilder.newFilter("HYMST_ID").eq("CM053"))

            Dim convertor As New ViewItemAndColumnMapperTemplete
            convertor.addRule("MK_FLG", "HYMST1_KEY")
            sqlb.setdsColConvertor(convertor)

            Return sqlb

        End Function
    End Class

End Namespace
