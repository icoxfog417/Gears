Imports Microsoft.VisualBasic
Imports Gears

Public Class EMPValidator
    Inherits AbsModelValidator

    Private _connectionName As String
    Public Property ConnectionName() As String
        Get
            Return _connectionName
        End Get
        Set(ByVal value As String)
            _connectionName = value
        End Set
    End Property

    Private empData As System.Data.DataTable = Nothing

    Public Sub New(ByVal conName As String)
        _connectionName = conName
    End Sub

    Public Overrides Sub setUpValidation(sqlb As Gears.SqlBuilder)
        '従業員データの取得
        If Not String.IsNullOrEmpty(getValidateeValue(sqlb, "EMPNO")) Then

            Dim emp As New GSDataSource.EMP(ConnectionName)
            Dim dto As New GearsDTO(ActionType.SEL)
            dto.addFilter(SqlBuilder.newFilter("EMPNO").eq(getValidateeValue(sqlb, "EMPNO")))

            empData = emp.gSelect(dto)

        End If

    End Sub

    Public Overrides Sub tearDownValidation(sqlb As Gears.SqlBuilder)
        '従業員データの開放
        empData = Nothing
    End Sub

    <ModelValidationMethod(FalseAsAlert:=True, order:=0)>
    Public Function isSALTooLarge(ByVal sqlb As SqlBuilder) As Boolean
        Dim fromSal As Decimal = 0
        Dim toSal As Decimal = 0

        Decimal.TryParse(GearsSqlExecutor.getDataSetValue("SAL", empData), fromSal)
        Decimal.TryParse(getValidateeValue(sqlb, "SAL"), toSal)

        If toSal - fromSal > 1000 Then
            ErrorSource = "SAL"
            ErrorMessage = "給与の上昇幅が1000を超えています。入力ミスはありませんか？"
            Return False
        Else

            Return True
        End If

    End Function

    <ModelValidationMethod(order:=0, OnlyWhenTheseValueExist:="JOB")>
    Public Function isSALSuitable(ByVal sqlb As SqlBuilder) As Boolean
        Dim toSal As Decimal = 0
        Decimal.TryParse(getValidateeValue(sqlb, "SAL"), toSal)

        If Not (getValidateeValue(sqlb, "JOB") = "PRESIDENT" Or getValidateeValue(sqlb, "JOB") = "MANAGER") AndAlso _
            toSal > 4000 Then

            ErrorSource = "SAL"
            ErrorMessage = "職種 " + getValidateeValue(sqlb, "JOB") + " に対し給与が大きすぎます(4000以上は PRESIDENT/MANAGERのみ)"
            Return False

        Else
            Return True
        End If

    End Function


End Class
