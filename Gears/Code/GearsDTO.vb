Imports Microsoft.VisualBasic
Imports System.Collections.Generic

Namespace Gears

    ''' <summary>
    ''' Controlの値などをGearsDataSourceへ渡す役割を担うクラス<br/>
    ''' GearsDTOはGearsDataSourceのmakeSqlBuilderを介すことで最終的なSQL実行オブジェクトとなる。
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()>
    Public Class GearsDTO
        Inherits SqlItemContainer

        ''' <summary>コントロール情報</summary>
        Private _controlInfo As New Dictionary(Of String, List(Of GearsControlInfo))

        ''' <summary>コントロール情報の取得</summary>
        Public Function ControlInfo() As Dictionary(Of String, List(Of GearsControlInfo))
            Return _controlInfo
        End Function

        ''' <summary>コントロール情報の取得(ID指定)</summary>
        Public Function ControlInfo(ByVal id As String) As List(Of GearsControlInfo)
            If (_controlInfo.ContainsKey(id)) Then
                Return _controlInfo(id)
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="atype"></param>
        Public Sub New(ByVal atype As ActionType)
            MyBase.New(atype)
        End Sub

        ''' <summary>
        ''' コピーコンストラクタ
        ''' </summary>
        ''' <param name="gto"></param>
        ''' <remarks></remarks>
        Public Sub New(ByRef gto As GearsDTO)
            MyBase.New(gto)
            If Not gto Is Nothing Then
                _controlInfo = New Dictionary(Of String, List(Of GearsControlInfo))(gto._controlInfo)
            End If

        End Sub

        ''' <summary>Control情報の追加</summary>
        Public Sub addControlInfo(ByRef conf As GearsControlInfo)
            If (_controlInfo.ContainsKey(conf.ControlID)) Then
                _controlInfo(conf.ControlID).Add(conf)
            Else
                Dim list As List(Of GearsControlInfo) = New List(Of GearsControlInfo)
                list.Add(conf)
                _controlInfo.Add(conf.ControlID, list)
            End If
        End Sub

        ''' <summary>Control情報の追加(GearsControlから)</summary>
        Public Sub addControlInfo(ByRef con As GearsControl)
            Dim list As List(Of GearsControlInfo) = con.createControlInfo
            For Each conf As GearsControlInfo In list
                addControlInfo(conf)
            Next
        End Sub

        ''' <summary>Control情報の削除</summary>
        Public Sub removeControlInfo(ByVal id As String)
            If (_controlInfo.ContainsKey(id)) Then
                _controlInfo.Remove(id)
            End If
        End Sub

        ''' <summary>
        ''' DTOをSqlBuilderに変換します<br/>
        ''' 具体的には、コントロール情報(GearsControlInfo)をselect/filter項目に変換します
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function generateSqlBuilder() As SqlBuilder
            Dim sqlb As SqlBuilder = New SqlBuilder(Me)

            'セレクタ/フィルタの設定
            For Each item As KeyValuePair(Of String, List(Of GearsControlInfo)) In _controlInfo
                For Each ginfo As GearsControlInfo In item.Value
                    Dim opr As String = SqlFilterItem.TXT_EQUAL
                    If Not String.IsNullOrEmpty(ginfo.OperatorAttribute()) Then
                        opr = ginfo.OperatorAttribute()
                    End If

                    If ginfo.IsFormAttribute Then
                        If ginfo.IsKey Then
                            sqlb.addSelection(SqlBuilder.newSelect(ginfo.DataSourceID).setValue(ginfo.Value).key)
                            If ginfo.PastValue <> "" Then
                                sqlb.addFilter(SqlBuilder.newFilter(ginfo.DataSourceID).filterAs(opr, ginfo.PastValue).key) '前回ロード時の値をフィルタ値として設定
                            Else
                                sqlb.addFilter(SqlBuilder.newFilter(ginfo.DataSourceID).filterAs(opr, ginfo.Value).key) '前回ロード時の値がない場合、現在値をフィルタ値として設定
                            End If
                        Else
                            sqlb.addSelection(SqlBuilder.newSelect(ginfo.DataSourceID).setValue(ginfo.Value))
                        End If

                    Else 'フィルタパネル、もしくは何も設定がない場合はフィルタ値として判断する
                        If ginfo.IsKey Then
                            sqlb.addFilter(SqlBuilder.newFilter(ginfo.DataSourceID).filterAs(opr, ginfo.Value).key)
                        Else
                            sqlb.addFilter(SqlBuilder.newFilter(ginfo.DataSourceID).filterAs(opr, ginfo.Value))
                        End If
                    End If

                Next
            Next

            '追加で設定されたselect/filterをセット
            'select以外の場合、キーが複数存在することは出来ない
            For Each item In Selection()
                If Action <> ActionType.SEL And (sqlb.Selection(item.Column) IsNot Nothing And item.IsKey) Then
                    sqlb.removeSelection(item.Column)
                End If
                sqlb.addSelection(New SqlSelectItem(item))
            Next
            For Each item In Filter()
                If Action <> ActionType.SEL And (sqlb.Filter(item.Column) IsNot Nothing And item.IsKey) Then  'select以外でキーが複数存在することは出来ない
                    sqlb.removeFilter(item.Column)
                End If
                sqlb.addFilter(New SqlFilterItem(item))
            Next

            Return sqlb

        End Function

        Public Overrides Function toString() As String
            Dim str As String = ""
            Dim sql As SqlBuilder = generateSqlBuilder()
            sql.DataSource = SqlBuilder.newDataSource("dummy")
            str += "更新タイプ：" + ActionToString(Action) + vbCrLf
            If Action = ActionType.SAVE Then
                'この時点ではInsertになるかUpdateになるか判別がつかないため、両方表示
                str += "予想実行SQL(更新対象はdummyで表示)：" + vbCrLf + sql.confirmSql(ActionType.UPD) + vbCrLf + sql.confirmSql(ActionType.INS)
            Else
                str += "予想実行SQL(更新対象はdummyで表示)：" + vbCrLf + sql.confirmSql(Action)
            End If
            Return str

        End Function

        ''' <summary>
        ''' DTOの段階でSQLを確認するためのメソッド<br/>
        ''' SqlDataSourceを引数として渡さない場合、table/viewに当たる個所はdummyで表示されます
        ''' </summary>
        ''' <param name="ds"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function confirmSql(Optional ByVal ds As SqlDataSource = Nothing) As String
            Dim str As String = ""
            Dim sql As SqlBuilder = generateSqlBuilder()

            If ds Is Nothing Then
                sql.DataSource = SqlBuilder.newDataSource("dummy")
            Else
                sql.DataSource = ds
            End If

            Return sql.confirmSql(Action)

        End Function

    End Class

End Namespace
