Imports Microsoft.VisualBasic
Imports System.Collections.Generic
Imports System.Collections

Namespace Gears.DataSource

    ''' <summary>
    ''' データベース種別
    ''' </summary>
    Public Enum DbServerType As Integer
        Oracle
        SQLServer
        OLEDB
        MySQL
        PostgreSQL
        SQLite
        'ODBC 名前つきパラメータークエリが使えないのがだいぶ致命的なので、非対応にする
    End Enum

    ''' <summary>
    ''' 楽観ロックに使用する項目の種別
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum LockType As Integer
        ''' <summary>日付型(※未実装)</summary>
        UDATE
        ''' <summary>日付文字列型(20110101など)</summary>
        UDATESTR
        ''' <summary>時刻文字列型(120000など)</summary>
        UTIMESTR
        ''' <summary>バージョン番号型</summary>
        VNUM
        ''' <summary>ユーザー</summary>
        USER
    End Enum

    ''' <summary>
    ''' SQLを構築するためのクラス
    ''' </summary>
    ''' <remarks></remarks>
    Public Class SqlBuilder
        Inherits SqlItemContainer

        ''' <summary>パラメーターの接頭辞。SQL Serverなら@など</summary>
        Private PARAM_HEAD As String = ""

        ''' <summary>
        ''' マルチバイトカラムを使用する際のエスケープ文字列<br/>
        ''' Oracleの場合、マルチバイトのカラム(日本語名の列など)は""で囲う必要がある
        ''' </summary>
        Private MULTIBYTE_FORMAT As String = ""

        ''' <summary>
        ''' ページングを行う際に使用する副問合せ表の名前(SqlServerで無名表が許されないため)
        ''' </summary>
        Private DUMMY_INDEX_FOR_ORDER_BY As String = "YB_REDRO_ROF_XEDNI_YMMUD"

        ''' <summary>SQLの抽出元。Table/Viewが設定される</summary>
        Public Property DataSource As SqlDataSource = Nothing

        <Obsolete("DataSourceプロパティを使用してください")>
        Public Sub setDataSource(ByVal sds As SqlDataSource)
            DataSource = sds
        End Sub

        <Obsolete("DataSourceプロパティを使用してください")>
        Public Function getDataSource() As SqlDataSource
            Return DataSource
        End Function

        ''' <summary>
        ''' データベース上のカラムと画面で使用する項目名が一致しない場合、変換をかけるために使用<br/>
        ''' (既存のテーブルを使用する場合など)
        ''' </summary>
        Public Property ItemColExchanger As INameExchanger = Nothing

        <Obsolete("ItemColExchangerプロパティを使用してください")>
        Public Sub setdsColConvertor(ByVal ice As INameExchanger)
            ItemColExchanger = ice
        End Sub

        <Obsolete("ItemColExchangerプロパティを使用してください")>
        Public Function getdsColConvertor() As INameExchanger
            Return ItemColExchanger()
        End Function

        Private _dbServer As DbServerType = DbServerType.Oracle
        ''' <summary>DBサーバーの種別</summary>
        Public Property DbServer() As DbServerType
            Get
                Return _dbServer
            End Get
            Set(ByVal value As DbServerType)
                _dbServer = value
                'ODBCは名前つきパラメータークエリが使えないので未対応
                Select Case _dbServer
                    Case DbServerType.SQLServer
                        PARAM_HEAD = "@"
                        MULTIBYTE_FORMAT = "[{0}]"
                    Case DbServerType.Oracle
                        PARAM_HEAD = ":"
                        MULTIBYTE_FORMAT = """{0}"""
                    Case DbServerType.OLEDB
                        PARAM_HEAD = "@"
                        MULTIBYTE_FORMAT = "[{0}]"
                    Case DbServerType.MySQL '未検証
                        PARAM_HEAD = "?"
                        MULTIBYTE_FORMAT = ""
                    Case DbServerType.PostgreSQL '未検証
                        PARAM_HEAD = ":"
                        MULTIBYTE_FORMAT = ""
                    Case DbServerType.SQLite
                        PARAM_HEAD = ":"
                        MULTIBYTE_FORMAT = "" 'マルチバイト対応なしと思われる

                End Select
            End Set
        End Property

        ''' <summary>
        ''' 一度に値を複数設定する場合のセパレータ(カンマ区切りなど)
        ''' </summary>
        Public Property ValueSeparator() As String = GearsControl.VALUE_SEPARATOR

        Public Sub New(ByRef scon As SqlItemContainer)
            MyBase.New(scon)
        End Sub

        Public Sub New(ByVal conName As String, ByVal dsName As String, Optional ByVal aType As ActionType = ActionType.SEL)
            MyBase.New(aType)
            DbServer = GearsSqlExecutor.getDbServerType(GearsSqlExecutor.getDBType(conName))
            DataSource = New SqlDataSource(dsName)
        End Sub
        Public Sub New(ByVal conName As String, ByVal ds As SqlDataSource, Optional ByVal aType As ActionType = ActionType.SEL)
            MyBase.New(aType)
            DbServer = GearsSqlExecutor.getDbServerType(GearsSqlExecutor.getDBType(conName))
            DataSource = ds
        End Sub

        Public Sub New(ByVal db As DbServerType, Optional ByVal aType As ActionType = ActionType.SEL)
            MyBase.New(aType)
            DbServer = db
        End Sub

        ''' <summary>
        ''' コピーコンストラクタ<br/>
        ''' withSelectionAndFilterオプションをFalseにすることで、選択/条件を引き継がないでインスタンスを作成可能
        ''' </summary>
        ''' <param name="sb"></param>
        ''' <param name="withSelectionAndFilter"></param>
        ''' <remarks></remarks>
        Public Sub New(ByVal sb As SqlBuilder, Optional ByVal withSelectionAndFilter As Boolean = True)
            MyBase.New(sb)
            _DataSource = New SqlDataSource(sb.DataSource())
            If Not withSelectionAndFilter Then
                Me.Selection.Clear()
                Me.Filter.Clear()
            End If
        End Sub

        Public Sub ImportSqlItem(ByRef sb As SqlBuilder)
            For Each sl As SqlSelectItem In sb.Selection
                Me.addSelection(sl)
            Next
            For Each fl As SqlFilterItem In sb.Filter
                Me.addFilter(fl)
            Next
        End Sub

        ''' <summary>選択/更新項目(SqlSelectItem)を作成するためのユーティリティ</summary>
        ''' <param name="col"></param>
        ''' <param name="pf"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function S(ByVal col As String, Optional ByVal pf As String = "") As SqlSelectItem
            Return New SqlSelectItem(col, pf)
        End Function

        ''' <summary>選択/更新項目(SqlSelectItem)を作成するためのユーティリティ(countなどの関数使用時)</summary>
        ''' <param name="col"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function C(ByVal col As String) As SqlSelectItem
            Dim sl As New SqlSelectItem(col)
            sl.IsFunction = True
            Return sl
        End Function

        ''' <summary>条件(SqlFilterItem)を作成するためのユーティリティ</summary>
        ''' <param name="col"></param>
        ''' <param name="pf"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function F(ByVal col As String, Optional ByVal pf As String = "") As SqlFilterItem
            Return New SqlFilterItem(col, pf)
        End Function

        ''' <summary>結合条件(SqlFilterItem)を作成するためのユーティリティ</summary>
        ''' <param name="col"></param>
        ''' <param name="col2"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function J(ByVal col As String, ByVal col2 As String) As SqlFilterItem
            Dim fl As New SqlFilterItem(col)
            fl.joinOn(F(col2))
            Return fl
        End Function

        ''' <summary>Table/View(SqlDataSource)を作成するためのユーティリティ</summary>
        ''' <param name="dsource"></param>
        ''' <param name="sf"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function DS(ByVal dsource As String, Optional ByVal sf As String = "") As SqlDataSource
            Return New SqlDataSource(dsource, sf)
        End Function

        <Obsolete("Selection.Where(Function(sl) Not sl.NoSelect).Count を使用してください(メソッドで提供しない)")>
        Public Function getSelectionCount() As Integer
            Return Selection.Where(Function(sl) Not sl.IsNoSelect).Count
        End Function

        ''' <summary>
        ''' このSqlBuilderをSqlDataSource化する
        ''' </summary>
        ''' <param name="suffix"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function convertToDataSource(ByVal suffix As String) As SqlDataSource
            Dim param As New Dictionary(Of String, Object)
            Dim sql As String = makeSelect(param, False)

            Dim ds As New SqlDataSource("(" + sql + ")", suffix)
            For Each item As KeyValuePair(Of String, Object) In param
                ds.setValue(item.Key, item.Value)
            Next

            Return ds

        End Function

        ''' <summary>
        ''' SQL確認用メソッド
        ''' </summary>
        ''' <param name="atype"></param>
        ''' <param name="sqlOnly"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function confirmSql(ByVal atype As ActionType, Optional ByVal sqlOnly As Boolean = False) As String
            Dim params As New Dictionary(Of String, Object)
            Dim sql As String = makeSql(params, atype)
            Dim paramStr As String = ""

            If Not sqlOnly Then
                For Each item As KeyValuePair(Of String, Object) In params
                    paramStr += item.Key + ":" + If(item.Value IsNot Nothing, item.Value.ToString, "NULL") + " / "
                Next
                Return sql + vbCrLf + paramStr

            Else
                Return sql
            End If

        End Function

        ''' <summary>
        ''' カラムをフォーマットする
        ''' </summary>
        ''' <param name="item"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function formatColumn(ByVal item As SqlItem, Optional ByVal withAlias As Boolean = False) As String
            Dim result As String = item.Column

            If Not _ItemColExchanger Is Nothing AndAlso Not String.IsNullOrEmpty(_ItemColExchanger.changeItemToCol(result)) Then
                result = _ItemColExchanger.changeItemToCol(result)
            End If

            If IsMultiByte And Not item.IsFunction Then
                result = String.Format(MULTIBYTE_FORMAT, result)
            End If

            If Not String.IsNullOrEmpty(item.Prefix) Then
                result = item.Prefix + "." + result
            End If

            If withAlias And TypeOf item Is SqlSelectItem Then
                Dim colAlias As String = CType(item, SqlSelectItem).ColAlias
                If Not String.IsNullOrEmpty(colAlias) Then
                    result += " AS " + If(IsMultiByte, String.Format(MULTIBYTE_FORMAT, colAlias), colAlias)
                End If
            End If

            Return result

        End Function

        ''' <summary>
        ''' データソースをフォーマットする
        ''' </summary>
        ''' <param name="source"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function formatSource(ByVal source As SqlDataSource) As String
            Dim result As String = source.DataSource

            If IsMultiByte And source.Value.Count = 0 Then 'マルチバイト対応が必要で、パイプライン表関数でない(引数がない)
                result = String.Format(MULTIBYTE_FORMAT, result)
            End If

            If Not String.IsNullOrEmpty(source.Schema) Then
                result = source.Schema + "." + result
            End If

            If Not String.IsNullOrEmpty(source.Suffix) Then
                result += " " + source.Suffix
            End If

            Return result

        End Function

        ''' <summary>
        ''' SQLを作成する
        ''' </summary>
        ''' <param name="params"></param>
        ''' <param name="atype"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function makeSql(ByRef params As Dictionary(Of String, Object), Optional ByVal atype As ActionType = ActionType.SEL) As String
            Dim sql As String = ""
            Select Case atype
                Case ActionType.SEL
                    sql = makeSelect(params)
                Case ActionType.UPD
                    sql = makeUpdate(params)
                Case ActionType.INS
                    sql = makeInsert(params)
                Case ActionType.DEL
                    sql = makeDelete(params)
            End Select

            Return sql

        End Function

        ''' <summary>
        ''' SELECTを作成する
        ''' </summary>
        ''' <param name="params"></param>
        ''' <param name="isNeedOrder"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function makeSelect(ByRef params As Dictionary(Of String, Object), Optional ByVal isNeedOrder As Boolean = True) As String
            Dim sql As String = ""
            Dim p_select As String = makeSelection()
            Dim p_from As String = makeFrom(params)
            Dim p_where As String = makeWhere(params)
            Dim p_groupby As String = makeGroupBy()

            'SELECT
            sql += "SELECT " + If(p_select <> "", p_select, "*")

            'FROM
            sql += " FROM " + p_from

            'WHERE
            sql += If(p_where <> "", " WHERE " + p_where, "")

            'GROUP BY
            sql += If(p_groupby <> "", " GROUP BY " + p_groupby, "")

            'ORDER BY
            If Not isNeedOrder Then
                Dim p_orderby As String = makeOrderBy()

                If RowsInPage() <= 0 Then 'ページング設定なし
                    sql += If(p_orderby <> "", " ORDER BY " + p_orderby, "")
                Else
                    If DbServer = DbServerType.Oracle Or DbServer = DbServerType.SQLServer Then
                        '行番号を分析関数により付与

                        Dim temp As String = " SELECT * FROM ( SELECT timilhtiw.* , ROW_NUMBER() OVER(ORDER BY "
                        Dim whereForLimit As String = " WHERE RNUM BETWEEN " + (RowIndexOfPage() + 1).ToString + " AND " + (RowIndexOfPage() + RowsInPage()).ToString
                        If p_orderby <> "" Then
                            temp += p_orderby
                            temp += " ) AS RNUM FROM ( " + sql + " ) timilhtiw ) weiVdetimil "
                            temp += whereForLimit + " ORDER BY " + p_orderby
                        Else
                            temp += DUMMY_INDEX_FOR_ORDER_BY
                            temp += " ) AS RNUM FROM ( SELECT MTtimilhtiw.* , 1 AS " + DUMMY_INDEX_FOR_ORDER_BY + " FROM ( " + sql + " ) MTtimilhtiw ) timilhtiw ) weiVdetimil "
                            temp += whereForLimit
                        End If

                        sql = temp

                    ElseIf DbServer = DbServerType.MySQL Or DbServer = DbServerType.PostgreSQL Or DbServer = DbServerType.SQLite Then
                        'OFFSET / LIMIT が使用可能なDB
                        If p_orderby <> "" Then
                            sql += " ORDER BY " + p_orderby
                        End If
                        sql += " LIMIT " + RowsInPage.ToString + " OFFSET " + RowIndexOfPage.ToString

                    End If

                End If

            End If

            Return sql

        End Function

        ''' <summary>
        ''' UPDATE文を作成する
        ''' </summary>
        ''' <param name="params"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function makeUpdate(ByRef params As Dictionary(Of String, Object)) As String
            Dim sql As String = ""
            Dim sets As New List(Of String)

            For i As Integer = 0 To Selection.Count - 1
                Dim item As SqlSelectItem = Selection(i)
                Dim name As String = If(String.IsNullOrEmpty(item.ParamName), "u" + i.ToString, item.ParamName)

                If Not item.IsNoSelect Then
                    If item.hasValue Then
                        sets.Add(formatColumn(item) + " = " + PARAM_HEAD + name)
                        params.Add(name, item.Value)
                    Else
                        sets.Add(formatColumn(item) + " = NULL ") 'updateの場合、値がない場合NULL更新
                    End If
                End If

            Next

            sql = " UPDATE " + formatSource(_DataSource) + " SET " + String.Join(",", sets)

            Dim where As String = makeWhere(params)
            If where <> "" Then
                sql += " WHERE " + where
            End If

            Return sql

        End Function

        ''' <summary>
        ''' INSERT文を作成する
        ''' </summary>
        ''' <param name="params"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function makeInsert(ByRef params As Dictionary(Of String, Object)) As String
            Dim sql As String = ""
            Dim cols As New List(Of String)
            Dim vals As New List(Of String)

            For i As Integer = 0 To Selection.Count - 1
                Dim item As SqlSelectItem = Selection(i)
                Dim name As String = If(String.IsNullOrEmpty(item.ParamName), "i" + i.ToString, item.ParamName)

                If item.hasValue Then
                    cols.Add(formatColumn(item))
                    vals.Add(PARAM_HEAD + name)
                    params.Add(name, item.Value)
                End If
            Next

            sql = " INSERT INTO " + formatSource(DataSource) + "(" + String.Join(",", cols) + ") VALUES (" + String.Join(",", vals) + ") "

            Return sql

        End Function

        ''' <summary>
        ''' DELETE文を作成する
        ''' </summary>
        ''' <param name="params"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function makeDelete(ByRef params As Dictionary(Of String, Object)) As String
            Dim sql As String = ""
            sql += "DELETE FROM " + formatSource(DataSource)
            Dim where As String = makeWhere(params)
            If where <> "" Then
                sql += " WHERE " + where
            End If

            Return sql

        End Function

        ''' <summary>
        ''' SELECTの項目指定部分を作成する
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function makeSelection() As String
            Dim selectStr As String = ""

            Dim selects = From sl As SqlSelectItem In Selection()
                          Where Not sl.IsNoSelect
                          Let sel As String = formatColumn(sl, True)
                          Select sl

            selectStr = String.Join(",", selects)

            Return selectStr

        End Function

        ''' <summary>
        ''' SELECTのGROUP BY句を作成する
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function makeGroupBy() As String
            Dim grpStr As String = ""

            Dim groups = Selection().Where(Function(sl) sl.IsGroupBy).Select(Function(sl) formatColumn(sl))
            grpStr = String.Join(",", groups)

            Return grpStr

        End Function

        ''' <summary>
        ''' SELECTのORDER BY句を作成する
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function makeOrderBy() As String
            Dim orderStr As String = ""

            Dim orders = From sl As SqlSelectItem In Selection()
                         Where sl.OrderBy <> OrderKind.NON
                         Select formatColumn(sl) + If(sl.OrderBy = OrderKind.ASC, " ASC", " DESC")

            Return String.Join(",", orders)

        End Function

        ''' <summary>
        ''' データソースを作成
        ''' </summary>
        ''' <param name="params"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function makeFrom(ByRef params As Dictionary(Of String, Object)) As String
            Return makeRelationStr(_DataSource, params)
        End Function

        ''' <summary>
        ''' データソースを作成
        ''' </summary>
        ''' <param name="ds"></param>
        ''' <param name="params"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function makeRelationStr(ByVal ds As SqlDataSource, ByRef params As Dictionary(Of String, Object)) As String
            Dim source As String = ""

            ds.readValues(params)

            If Not ds.hasRelation Then
                source = formatSource(ds)
            Else

                Dim relations As List(Of SqlDataSource) = ds.JoinTargets
                For Each relation As SqlDataSource In relations
                    Dim relSource As String = ""
                    If relation.hasRelation Then
                        '再帰呼び出し
                        relSource = makeRelationStr(relation, params)
                    Else
                        Select Case ds.getRelation(relation.DataSource)
                            Case RelationKind.INNER_JOIN
                                relSource += " INNER JOIN " + formatSource(relation) + " ON "
                            Case RelationKind.LEFT_OUTER_JOIN
                                relSource += " LEFT OUTER JOIN " + formatSource(relation) + " ON "
                        End Select

                        Dim joins = From j As SqlFilterItem In ds.getJoinKey(relation.DataSource)
                                    Select formatColumn(j) + " = " + formatColumn(j.JoinTarget)
                        relSource = String.Join(" AND ", joins)
                    End If
                    source += relSource
                Next

                source = formatSource(ds) + source
            End If

            Return source

        End Function

        ''' <summary>
        ''' WHERE句を作成する
        ''' </summary>
        ''' <param name="params"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function makeWhere(ByRef params As Dictionary(Of String, Object)) As String
            Return makeWhere(Filter, params)
        End Function

        ''' <summary>
        ''' WHERE句を作成する
        ''' </summary>
        ''' <param name="fs"></param>
        ''' <param name="params"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function makeWhere(ByRef fs As List(Of SqlFilterItem), ByRef params As Dictionary(Of String, Object)) As String

            '指定されたグループ別に集計する(指定がない場合、グループ名は空白)
            Dim groups = From fl In fs.Select(Function(item, index) New With {index, item})
                         Order By fl.index
                         Group By Name = If(fl.item.Group Is Nothing, "", fl.item.Group.Name) Into filters = Group

            Dim gList As New List(Of String)
            Dim fList As New List(Of String)

            '条件式を作成するマクロ
            Dim makefPart = Function(tf As SqlFilterItem, tp As String)
                                Dim part As String = If(tf.Negation, "NOT ", "")
                                If tf.hasValue Then
                                    Return part + formatColumn(tf) + " " + tf.Operand + " " + PARAM_HEAD + tp
                                Else
                                    Return part + formatColumn(tf) + " IS NULL" '値がない場合NULLで比較
                                End If
                            End Function

            For gIdx As Integer = 0 To groups.Count - 1
                Dim g = groups(gIdx).filters.First.item.Group

                'グループ内のフィルタ条件を評価
                For mIdx As Integer = 0 To groups(gIdx).filters.Count - 1
                    Dim fl As SqlFilterItem = groups(gIdx).filters(mIdx).item
                    Dim fName As String = If(String.IsNullOrEmpty(fl.ParamName), "G" + gIdx.ToString + "F" + fl.toString, fl.ParamName)
                    Dim fPart As String = ""

                    If fl.hasValue AndAlso (TypeOf fl.Value Is String AndAlso fl.Value.ToString.Contains(ValueSeparator)) Then
                        'Separatorによる複数指定の場合、これをSplitしてOR条件で結合する
                        Dim values As List(Of String) = fl.Value.ToString.Split(ValueSeparator).ToList
                        Dim vList As New List(Of String)
                        For vIdx As Integer = 0 To values.Count - 1
                            Dim vName As String = fName + "V" + vIdx.ToString
                            vList.Add(makefPart(fl, vName))
                            params.Add(vName, values(vIdx))
                        Next
                        fList.Add("( " + String.Join(" OR ", vList) + " )") '複数値を指定場合、ORでつなぐ
                    Else
                        params.Add(fName, fl.Value)
                        fList.Add(makefPart(fl, fName))
                    End If
                Next

                'グループ内の条件をまとめる
                Dim gPart As String = ""
                If Not String.IsNullOrEmpty(g.Name) AndAlso g.isOrGroup Then
                    gPart = String.Join(" OR ", fList) 'OR指定グループの場合
                Else
                    gPart = String.Join(" AND ", fList)
                End If
                If fList.Count > 1 And groups.Count > 1 Then '条件式が複数あり、グループも複数指定されている場合括弧でくくる
                    gPart = "(" + gPart + ")"
                End If
                gList.Add(gPart)

                fList.Clear()

            Next

            Dim where As String = String.Join(" AND ", gList) '条件をANDで結合

            Return where

        End Function

    End Class

End Namespace
