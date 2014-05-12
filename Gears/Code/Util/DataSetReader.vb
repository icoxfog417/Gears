Imports System.Data

Namespace Gears.Util

    ''' <summary>
    ''' DataTableの値を読み込むためのユーティリティクラス
    ''' </summary>
    ''' <remarks></remarks>
    Public Class DataSetReader

        ''' <summary>データデータブルの指定箇所の値を取得する(列名指定)</summary>
        Public Shared Function Item(ByVal dt As DataTable, ByVal colName As String, Optional ByVal rowIndex As Integer = 0) As Object
            Return Item(Of String)(dt, colName, rowIndex)

        End Function

        ''' <summary>データデータブルの指定箇所の値を取得する(列番号指定)</summary>
        Public Shared Function Item(ByVal dt As DataTable, ByVal colindex As Integer, Optional ByVal rowIndex As Integer = 0) As Object
            Return Item(Of Integer)(dt, colindex, rowIndex)

        End Function

        ''' <summary>指定行、指定列の値を取得する(列名指定)</summary>
        Public Shared Function Item(ByVal dt As DataRow, ByVal colName As String) As Object
            Return Item(Of String)(dt, colName)
        End Function

        ''' <summary>指定行、指定列の値を取得する(列番号指定)</summary>
        Public Shared Function Item(ByVal dt As DataRow, ByVal colindex As Integer) As Object
            Return Item(Of Integer)(dt, colindex)
        End Function

        ''' <summary>データデータブルの指定箇所の値を取得する</summary>
        Private Shared Function Item(Of T)(ByVal dt As DataTable, ByVal colindex As T, Optional ByVal rowIndex As Integer = 0) As Object

            If Not dt Is Nothing AndAlso Not dt.Rows Is Nothing AndAlso rowIndex < dt.Rows.Count Then
                Return Item(Of T)(dt.Columns, dt.Rows(rowIndex), colindex)
            Else
                Return Nothing
            End If

        End Function

        ''' <summary>指定行、指定列の値を取得する</summary>
        Private Shared Function Item(Of T)(ByVal dr As DataRow, ByVal colindex As T) As Object
            If dr.Table IsNot Nothing Then
                Return Item(Of T)(dr.Table.Columns, dr, colindex)
            Else
                Return Item(Of T)(Nothing, dr, colindex)
            End If

        End Function

        ''' <summary>データテーブルの値を安全に取得する</summary>
        Private Shared Function Item(Of T)(ByVal colDef As DataColumnCollection, ByVal dr As DataRow, ByVal colindex As T) As Object

            Dim index As String = colindex.ToString
            Dim result As Object = Nothing

            Try

                If TypeOf colindex Is Integer Then
                    Dim numIndex As Integer = CType(index, Integer)
                    If colDef Is Nothing OrElse numIndex < colDef.Count Then
                        result = dr.Item(numIndex)
                    End If
                Else
                    If colDef Is Nothing OrElse colDef.Contains(index) Then
                        result = dr.Item(index)
                    End If
                End If

                If IsDBNull(result) Then
                    result = String.Empty
                End If

            Catch ex As Exception
                result = Nothing
            End Try

            Return result

        End Function

    End Class

    ''' <summary>
    ''' データテーブル/列/行を扱いやすくするための拡張
    ''' </summary>
    ''' <remarks></remarks>
    Public Module DataSetExtension

        ''' <summary>指定行、指定列の値を取得する(列名指定)</summary>
        ''' <param name="row"></param>
        ''' <param name="colName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Runtime.CompilerServices.Extension()> _
        Public Function ItemOrDefault(ByVal row As DataRow, ByVal colName As String) As Object
            Return DataSetReader.Item(row, colName)
        End Function


        ''' <summary>指定行、指定列の値を取得する(列番号指定)</summary>
        ''' <param name="row"></param>
        ''' <param name="colIndex"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Runtime.CompilerServices.Extension()> _
        Public Function ItemOrDefault(ByVal row As DataRow, ByVal colIndex As Integer) As Object
            Return DataSetReader.Item(row, colIndex)
        End Function

        ''' <summary>指定行、指定列の値を取得する(列名指定)</summary>
        ''' <param name="table"></param>
        ''' <param name="colName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Runtime.CompilerServices.Extension()> _
        Public Function ItemOrDefault(ByVal table As DataTable, ByVal colName As String, Optional ByVal rowIndex As Integer = 0) As Object
            Return DataSetReader.Item(table, colName, rowIndex)
        End Function


        ''' <summary>指定行、指定列の値を取得する(列番号指定)</summary>
        ''' <param name="table"></param>
        ''' <param name="colIndex"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Runtime.CompilerServices.Extension()> _
        Public Function ItemOrDefault(ByVal table As DataTable, ByVal colIndex As Integer, Optional ByVal rowIndex As Integer = 0) As Object
            Return DataSetReader.Item(table, colIndex, rowIndex)
        End Function

        ''' <summary>
        ''' JSONへ変換する
        ''' </summary>
        ''' <param name="table"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Runtime.CompilerServices.Extension()> _
        Public Function ToListOfDictionary(ByVal table As DataTable) As List(Of Dictionary(Of String, Object))

            Dim rows As New List(Of Dictionary(Of String, Object))

            For Each r As DataRow In table.Rows
                Dim row As New Dictionary(Of String, Object)
                For Each c As DataColumn In table.Columns
                    row.Add(c.ColumnName, r.ItemOrDefault(c.ColumnName))
                Next
                rows.Add(row)
            Next

            Return rows

        End Function

    End Module

End Namespace
