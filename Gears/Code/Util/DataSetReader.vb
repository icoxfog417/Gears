Imports System.Data

Namespace Gears.Util

    Public Class DataSetReader

        Public Shared Function Item(ByRef dt As DataTable, ByVal colindex As String, Optional ByVal rowIndex As Integer = 0) As Object
            Return Item(Of String)(dt, colindex, rowIndex)

        End Function
        Public Shared Function Item(ByRef dt As DataTable, ByVal colindex As Integer, Optional ByVal rowIndex As Integer = 0) As Object
            Return Item(Of Integer)(dt, colindex, rowIndex)

        End Function

        Private Shared Function Item(Of T)(ByRef dt As DataTable, ByVal colindex As T, Optional ByVal rowIndex As Integer = 0) As Object
            Dim index As String = colindex.ToString
            Dim result As Object = Nothing

            Try
                If Not dt Is Nothing AndAlso Not dt.Rows Is Nothing Then

                    If TypeOf colindex Is Integer Then
                        result = dt.Rows(rowIndex).Item(CType(index, Integer))
                    Else
                        result = dt.Rows(rowIndex).Item(index)
                    End If

                End If

            Catch ex As Exception
                result = Nothing
            End Try

            Return result

        End Function

    End Class

End Namespace
