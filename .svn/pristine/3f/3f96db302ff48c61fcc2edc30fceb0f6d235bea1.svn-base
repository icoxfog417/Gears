Imports Microsoft.VisualBasic
Imports System.Collections.Generic
Imports System.Collections

Namespace Gears

    ''' <summary>
    ''' データベースへの更新の種別を表す
    ''' </summary>
    Public Enum ActionType As Integer
        ''' <summary>NONE:Nothingに該当。初期値用</summary>
        NONE
        ''' <summary>NONE:Nothingに該当。初期値用</summary>
        SEL
        ''' <summary>SEL :SELECT処理を表す</summary>
        UPD
        ''' <summary>UPD :UPDATE処理を表す</summary>
        INS
        ''' <summary>INS :INSERT処理を表す</summary>
        DEL
        ''' <summary>SAVE:既に該当キーが存在する場合UPDATE、そうでない場合INSERTを行う</summary>
        SAVE
    End Enum

    'データベース種別
    Public Enum DbServerType As Integer
        Oracle
        SQLServer
        OLEDB
        MySQL
        PostgreSQL
        SQLite
        'ODBC 名前つきパラメータークエリが使えないのがだいぶ致命的なので、非対応にする
    End Enum
    '楽観的ロックに使用するカラムのタイプ
    Public Enum LockType As Integer
        UDATE       '日付型(※未実装)
        UDATESTR    '日付文字列型(20110101など)
        UTIMESTR    '時刻文字列型(120000など)
        VNUM        'バージョン番号型
        USER       'ユーザー
    End Enum

    Public Class SqlBuilder

        Private PARAM_HEAD As String = "" 'パラメーターの接頭辞。SQL Serverなら@
        Private MULTIBYTE_FORMAT As String = "" 'oracleの場合、マルチバイトのカラム(日本語名の列など)の際ダブルクウォートで囲う必要がある
        Private DUMMY_INDEX_FOR_ORDER_BY As String = "YB_REDRO_ROF_XEDNI_YMMUD"

        'SQLの選択項目
        Private selection As List(Of SqlSelectItem) = New List(Of SqlSelectItem)
        'SQLの条件項目
        Private filter As List(Of SqlFilterItem) = New List(Of SqlFilterItem)
        'SQLの抽出元項目
        Private dataSource As SqlDataSource = Nothing
        '実行予定のSQLの種別
        Private predictiveType As ActionType = ActionType.NONE

        'DBカラム名の変換(任意設定)
        Private dsColConvertor As IViewItemAndColumnMapper = Nothing

        'リミット
        Private extractRange As Integer() = New Integer() {-1, -1}

        'DBサーバーの種別
        Private _dbServer As DbServerType = DbServerType.Oracle
        Public Property DbServer() As DbServerType
            Get
                Return _dbServer
            End Get
            Set(ByVal value As DbServerType)
                _dbServer = value
                'ODBCは名前つきパラメータークエリが使えないのでちと厳しい
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

        'マルチバイト対応の要否
        Private _isMultiByte As Boolean = False
        Public Property IsMultiByte() As Boolean
            Get
                Return _isMultiByte
            End Get
            Set(ByVal value As Boolean)
                _isMultiByte = value
            End Set
        End Property

        '警告無視での実行
        Private _ignoreAlert As Boolean = False
        Public Property IgnoreAlert() As Boolean
            Get
                Return _ignoreAlert
            End Get
            Set(ByVal value As Boolean)
                _ignoreAlert = value
            End Set
        End Property

        'データベース実行のタイムアウト設定
        Private _commandTimeout As Integer = -1
        Public Property CommandTimeout() As Integer
            Get
                Return _commandTimeout
            End Get
            Set(ByVal value As Integer)
                _commandTimeout = value
            End Set
        End Property

        '複数項目指定時のセパレーター
        Private _valueSeparator As String = GearsControl.VALUE_SEPARATOR
        Public Property ValueSeparator() As String
            Get
                Return _valueSeparator
            End Get
            Set(ByVal value As String)
                _valueSeparator = value
            End Set
        End Property

        Public Sub New()
        End Sub

        Public Sub New(ByVal conName As String, ByVal dsName As String, Optional ByVal aType As ActionType = ActionType.SEL)
            DbServer = GearsSqlExecutor.getDbServerType(GearsSqlExecutor.getDBType(conName))
            predictiveType = aType
            Me.setDataSource(New SqlDataSource(dsName))
        End Sub
        Public Sub New(ByVal conName As String, ByVal ds As SqlDataSource, Optional ByVal aType As ActionType = ActionType.SEL)
            DbServer = GearsSqlExecutor.getDbServerType(GearsSqlExecutor.getDBType(conName))
            predictiveType = aType
            Me.setDataSource(ds)
        End Sub

        Public Sub New(ByVal db As DbServerType, Optional ByVal aType As ActionType = ActionType.SEL)
            DbServer = db
            predictiveType = aType
        End Sub
        Public Sub New(ByVal sb As SqlBuilder, Optional ByVal withSelectionAndFilter As Boolean = True)
            DbServer = sb.DbServer
            IsMultiByte = sb.IsMultiByte
            predictiveType = sb.getPredictiveType
            extractRange = sb.getRange.Clone
            If withSelectionAndFilter Then
                selection = New List(Of SqlSelectItem)(sb.getSelection)
                filter = New List(Of SqlFilterItem)(sb.getFilter)
            End If
            dataSource = New SqlDataSource(sb.getDataSource())
        End Sub

        Public Shared Function newSelect(ByVal col As String, Optional ByVal pf As String = "") As SqlSelectItem
            Return New SqlSelectItem(col, pf)
        End Function
        Public Shared Function newFunction(ByVal col As String) As SqlSelectItem
            Dim s As New SqlSelectItem(col)
            s.IsFunction = True
            Return s
        End Function

        Public Shared Function newFilter(ByVal col As String, Optional ByVal pf As String = "") As SqlFilterItem
            Return New SqlFilterItem(col, pf)
        End Function
        Public Shared Function newJoinFilter(ByVal col As String, ByVal col2 As String) As SqlFilterItem
            Dim f As New SqlFilterItem(col)
            f.joinOn(newFilter(col2))
            Return f
        End Function

        Public Shared Function newDataSource(ByVal ds As String, Optional ByVal sf As String = "") As SqlDataSource
            Return New SqlDataSource(ds, sf)
        End Function

        Public Sub setdsColConvertor(ByVal dcc As IViewItemAndColumnMapper)
            dsColConvertor = dcc
        End Sub
        Public Function getdsColConvertor() As IViewItemAndColumnMapper
            Return dsColConvertor
        End Function

        Public Sub setPredictiveType(ByVal atype As ActionType)
            predictiveType = atype
        End Sub
        Public Function getPredictiveType() As ActionType
            Return predictiveType
        End Function

        Public Sub addSelection(ByVal ParamArray selections() As SqlSelectItem)
            For i As Integer = 0 To selections.Length - 1
                selection.Add(selections(i))
            Next
        End Sub
        Public Function getSelection() As List(Of SqlSelectItem)
            Return selection
        End Function
        Public Function getSelections(ByVal colname As String) As List(Of SqlSelectItem)
            Dim index As List(Of Integer) = getSqlItemIndex(colname, selection)
            Dim selectSet As New List(Of SqlSelectItem)
            If Not index Is Nothing Then
                For Each i As Integer In index
                    selectSet.Add(selection(i))
                Next

            End If

            Return selectSet

        End Function
        Public Function getSelection(ByVal colname As String, Optional ByVal index As Integer = 0) As SqlSelectItem
            Dim selectSet As List(Of SqlSelectItem) = getSelections(colname)
            If selectSet.Count > index And index > -1 Then
                Return selectSet(index)
            Else
                Return Nothing
            End If

        End Function

        Public Sub removeSelection(ByVal colname As String)
            Dim index As List(Of Integer) = getSqlItemIndex(colname, selection)
            If Not index Is Nothing Then
                index.Sort()
                For i As Integer = index.Count - 1 To 0 Step -1
                    selection.RemoveAt(index(i))
                Next

            End If

        End Sub

        Public Sub changeSelection(ByVal sel As SqlSelectItem)
            removeSelection(sel.Column)
            addSelection(sel)
        End Sub

        Public Sub clearSelection()
            selection.Clear()
        End Sub
        Public Function getKeySelection() As List(Of SqlSelectItem)
            Dim list As List(Of SqlSelectItem) = New List(Of SqlSelectItem)
            For Each item As SqlSelectItem In selection
                If item.IsKey Then
                    list.Add(item)
                End If
            Next
            Return list
        End Function
        Public Function getKeyFilter() As List(Of SqlFilterItem)
            Dim list As List(Of SqlFilterItem) = New List(Of SqlFilterItem)
            For Each item As SqlFilterItem In filter
                If item.IsKey Then
                    Dim keyFilters As List(Of SqlFilterItem) = getFilters(item.Column)

                    For Each f As SqlFilterItem In keyFilters
                        list.Add(f)
                    Next
                End If
            Next
            Return list
        End Function

        Public Function getSelectionCount() As Integer
            Dim i As Integer = 0
            For Each item As SqlSelectItem In selection
                If item.NoSelect Then
                    Continue For
                Else
                    i += 1
                End If
            Next
            Return i
        End Function
        Public Function getActiveSelectionCount() As Integer
            Return getActiveSqlItemCount(selection)
        End Function

        Public Sub addFilter(ByVal ParamArray filters() As SqlFilterItem)
            For i As Integer = 0 To filters.Length - 1
                filter.Add(filters(i))
            Next
        End Sub
        Public Function getFilter() As List(Of SqlFilterItem)
            Return filter
        End Function
        Public Function getFilters(ByVal colname As String) As List(Of SqlFilterItem)
            Dim index As List(Of Integer) = getSqlItemIndex(colname, filter)
            Dim filterSet As New List(Of SqlFilterItem)
            If Not index Is Nothing Then
                For Each i As Integer In index
                    filterSet.Add(filter(i))
                Next
            End If

            Return filterSet

        End Function
        Public Function getFilter(ByVal colname As String, Optional ByVal index As Integer = 0) As SqlFilterItem
            Dim filterSet As List(Of SqlFilterItem) = getFilters(colname)
            If filterSet.Count > index And index > -1 Then
                Return filterSet(index)
            Else
                Return Nothing

            End If

        End Function
        Public Sub removeFilter(ByVal colname As String)
            Dim index As List(Of Integer) = getSqlItemIndex(colname, filter)
            If Not index Is Nothing Then
                index.Sort()
                For i As Integer = index.Count - 1 To 0 Step -1
                    filter.RemoveAt(index(i))
                Next

            End If

        End Sub
        Public Sub changeFilter(ByVal fil As SqlFilterItem)
            removeFilter(fil.Column)
            addFilter(fil)
        End Sub

        Public Sub clearfilter()
            filter.Clear()
        End Sub
        Public Function getFilterCount() As Integer
            Return filter.Count
        End Function
        Public Function getActiveFilterCount() As Integer
            Return getActiveSqlItemCount(filter)
        End Function

        Private Function getSqlItemIndex(Of T As SqlItem)(ByVal colname As String, ByRef list As List(Of T)) As List(Of Integer)
            Dim index As Integer
            Dim indexSet As New List(Of Integer)
            For Each item As T In list
                If item.Column = colname Then
                    indexSet.Add(index)
                End If
                index += 1
            Next

            If indexSet.Count > 0 Then
                Return indexSet
            Else
                Return Nothing
            End If

        End Function
        Public Function getActiveSqlItemCount(Of T As SqlItem)(ByRef list As List(Of T)) As Integer
            Dim count As Integer = 0
            For Each item As SqlItem In list
                If item.isActive Then
                    count += 1
                End If
            Next
            Return count

        End Function

        Public Sub setDataSource(ByVal sds As SqlDataSource)
            dataSource = sds
        End Sub
        Public Function getDataSource() As SqlDataSource
            Return dataSource
        End Function

        Public Function convertToDataSource(ByVal suffix As String) As SqlDataSource
            Dim param As New Dictionary(Of String, String)
            Dim sql As String = makeSelect(param, False)

            Dim ds As New SqlDataSource("(" + sql + ")", suffix)
            For Each item As KeyValuePair(Of String, String) In param
                ds.setValue(item.Key, item.Value)
            Next

            Return ds

        End Function

        Public Sub limit(ByVal start As Integer, ByVal count As Integer)
            extractRange(0) = start
            extractRange(1) = count
        End Sub
        Public Function getRange() As Integer()
            Return extractRange
        End Function

        Public Function confirmSql(ByVal atype As ActionType, Optional ByVal sqlOnly As Boolean = False) As String
            Dim params As Dictionary(Of String, String) = New Dictionary(Of String, String)
            Dim sql As String = makeSql(params, atype)
            Dim paramStr As String = ""

            If Not sqlOnly Then
                For Each item As KeyValuePair(Of String, String) In params
                    paramStr += item.Key + ":" + item.Value + " / "
                Next
                Return sql + vbCrLf + paramStr

            Else
                Return sql
            End If

        End Function
        Public Function formatColumn(ByVal item As SqlItem) As String
            Dim result As String = item.Column

            If Not dsColConvertor Is Nothing AndAlso Not String.IsNullOrEmpty(dsColConvertor.changeItemToCol(result)) Then
                result = dsColConvertor.changeItemToCol(result)
            End If

            If IsMultiByte And Not item.IsFunction Then
                result = String.Format(MULTIBYTE_FORMAT, result)
            End If

            If Not String.IsNullOrEmpty(item.Prefix) Then
                result = item.Prefix + "." + result
            End If

            Return result

        End Function
        Public Function formatColumnAlias(ByVal aliasName As String) As String
            Dim result As String = aliasName

            If IsMultiByte Then
                result = String.Format(MULTIBYTE_FORMAT, result)
            End If

            Return result

        End Function
        Public Function formatSource(ByVal source As SqlDataSource) As String
            Dim result As String = source.DataSource

            If IsMultiByte And source.getValue.Count = 0 Then 'マルチバイト対応が必要で、パイプライン表関数でない
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

        Public Function makeSql(ByRef params As Dictionary(Of String, String), Optional ByVal atype As ActionType = ActionType.SEL) As String
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
        Public Function makeSelect(ByRef params As Dictionary(Of String, String), Optional ByVal isNeedOrder As Boolean = True) As String
            Dim sql As String = ""
            Dim selectStr As String = makeSelection()
            Dim from As String = makeFrom(params)
            Dim where As String = makeWhere(params)
            Dim groupby As String = makeGroupBy()
            Dim orderby As String = ""
            Dim isLimitSql As Boolean = False

            'ORDER BY 設定
            If isNeedOrder Then
                orderby = makeOrderBy()
            End If

            'リミット判定
            If extractRange(1) >= 0 Then
                isLimitSql = True
            End If

            If selectStr <> "" Then
                sql += "SELECT " + selectStr
            Else
                sql += "SELECT * " '全選択
            End If

            sql += " FROM " + from

            If where <> "" Then
                sql += " WHERE " + where
            End If
            If groupby <> "" Then
                sql += " GROUP BY " + groupby
            End If

            If Not isLimitSql And orderby <> "" Then
                sql += " ORDER BY " + orderby
            End If

            If isLimitSql Then
                If DbServer = DbServerType.Oracle Or DbServer = DbServerType.SQLServer Then

                    Dim temp As String = " SELECT * FROM ( SELECT timilhtiw.* , ROW_NUMBER() OVER(ORDER BY "
                    Dim whereForLimit As String = " WHERE RNUM BETWEEN " + (extractRange(0) + 1).ToString + " AND " + (extractRange(0) + extractRange(1)).ToString
                    If orderby <> "" Then
                        temp += orderby
                        temp += " ) AS RNUM FROM ( " + sql + " ) timilhtiw ) weiVdetimil "
                        temp += whereForLimit + " ORDER BY " + orderby
                    Else
                        temp += DUMMY_INDEX_FOR_ORDER_BY
                        temp += " ) AS RNUM FROM ( SELECT MTtimilhtiw.* , 1 AS " + DUMMY_INDEX_FOR_ORDER_BY + " FROM ( " + sql + " ) MTtimilhtiw ) timilhtiw ) weiVdetimil "
                        temp += whereForLimit
                    End If

                    sql = temp

                ElseIf DbServer <> DbServerType.OLEDB Then  'OFFSET / LIMIT がある優秀なDBたち
                    If orderby <> "" Then
                        sql += " ORDER BY " + orderby
                    End If
                    sql += " LIMIT " + extractRange(1).ToString + " OFFSET " + extractRange(0).ToString

                End If

            End If

            Return sql

        End Function
        Public Function makeUpdate(ByRef params As Dictionary(Of String, String)) As String
            Dim sql As String = ""
            For i As Integer = 0 To selection.Count - 1
                Dim item As SqlSelectItem = selection(i)
                Dim name As String = "u" + i.ToString
                Dim temp As String = ""

                If Not item.NoSelect Then 'updateの場合、空白でのupdateがあるうるので空白も対象とする
                    If Not item.Value Is Nothing Then
                        temp += formatColumn(item) + " = " + PARAM_HEAD + name
                        params.Add(name, item.Value)
                    Else
                        temp += formatColumn(item) + " = NULL "
                    End If

                    sql += temp + ","

                End If

            Next
            sql = eraseLastChar(sql, ",")

            sql = " UPDATE " + formatSource(dataSource) + " SET " + sql

            Dim where As String = makeWhere(params)
            If where <> "" Then
                sql += " WHERE " + where
            End If

            Return sql

        End Function
        Public Function makeInsert(ByRef params As Dictionary(Of String, String)) As String
            Dim sql As String = ""
            Dim cols As String = ""
            Dim vals As String = ""
            For i As Integer = 0 To selection.Count - 1
                Dim item As SqlSelectItem = selection(i)
                Dim name As String = "i" + i.ToString
                If Not String.IsNullOrEmpty(item.Value) And Not item.NoSelect Then 'insertで空白/NULLを入れたい場合テーブルのDefaultを使用する
                    cols += formatColumn(item)
                    vals += PARAM_HEAD + name
                    params.Add(name, item.Value)
                    cols += ","
                    vals += ","

                End If

            Next
            cols = eraseLastChar(cols, ",")
            vals = eraseLastChar(vals, ",")

            sql = " INSERT INTO " + formatSource(dataSource) + "(" + cols + ") VALUES (" + vals + ") "

            Return sql

        End Function
        Public Function makeDelete(ByRef params As Dictionary(Of String, String)) As String
            Dim sql As String = ""
            sql += "DELETE FROM " + formatSource(dataSource)
            Dim where As String = makeWhere(params)
            If where <> "" Then
                sql += " WHERE " + where
            End If

            Return sql

        End Function

        Public Function makeSelection() As String
            Dim selectStr As String = ""

            For i As Integer = 0 To selection.Count - 1
                Dim item As SqlSelectItem = selection(i)
                Dim itemSelect As String = ""
                If item.NoSelect Then 'orderby/groupbyのみに使用する項目の場合処理スキップ
                    Continue For
                End If

                itemSelect += formatColumn(item) '変換がある場合、DB上の列名に変換

                If Not String.IsNullOrEmpty(item.ColAlias) Then
                    itemSelect += " AS " + formatColumnAlias(item.ColAlias)
                End If

                selectStr += itemSelect + ","

            Next

            selectStr = eraseLastChar(selectStr, ",")

            Return selectStr


        End Function

        Public Function makeGroupBy() As String
            Dim grpStr As String = ""
            Dim list As List(Of SqlSelectItem) = New List(Of SqlSelectItem)

            For Each item As SqlSelectItem In selection
                If item.IsGroupBy Then
                    grpStr += formatColumn(item) + ","
                End If
            Next

            '最終の,を削除
            grpStr = eraseLastChar(grpStr, ",")
            Return grpStr

        End Function
        Public Function makeOrderBy() As String
            Dim orderStr As String = ""
            Dim list As List(Of SqlSelectItem) = New List(Of SqlSelectItem)

            For Each item As SqlSelectItem In selection
                Select Case item.OrderBy
                    Case OrderKind.ASC
                        orderStr += formatColumn(item) + " ASC,"
                    Case OrderKind.DESC
                        orderStr += formatColumn(item) + " DESC,"
                    Case Else
                        '処理無し
                End Select
            Next

            '最終の,を削除
            If orderStr.Length > 0 Then
                orderStr = orderStr.Substring(0, orderStr.Length - 1)
            End If
            Return orderStr
        End Function
        Public Function makeFrom(ByRef params As Dictionary(Of String, String)) As String
            Return makeRelationStr(dataSource, params)
        End Function
        Private Function makeRelationStr(ByVal ds As SqlDataSource, ByRef params As Dictionary(Of String, String)) As String
            Dim str As String = ""
            If ds.hasRelation Then
                Dim relations As List(Of SqlDataSource) = ds.getRels
                For Each relation As SqlDataSource In relations
                    Dim strTemp As String = ""
                    If relation.hasRelation Then
                        '再帰呼び出し
                        strTemp = makeRelationStr(relation, params)
                    Else
                        Select Case ds.getRelKind(relation.DataSource)
                            Case RelationKind.INNER_JOIN
                                strTemp += " INNER JOIN " + formatSource(relation) + " ON "
                            Case RelationKind.LEFT_OUTER_JOIN
                                strTemp += " LEFT OUTER JOIN " + formatSource(relation) + " ON "
                        End Select

                        Dim joinWhere As String = ""
                        For Each joinItem As SqlFilterItem In ds.getRelKey(relation.DataSource)
                            joinWhere += formatColumn(joinItem) + " = " + formatColumn(joinItem.JoinTarget) + " AND "
                        Next

                        strTemp += eraseLastChar(joinWhere, " AND ")

                    End If
                    str += strTemp
                Next
                str = " " + formatSource(ds) + str + " "
            Else
                str = " " + formatSource(ds) + " "
            End If
            ds.getValue(params)

            Return str

        End Function

        Public Function makeWhere(ByRef params As Dictionary(Of String, String)) As String
            Return makeWhere(filter, params)
        End Function

        Private Function makeWhere(ByRef fs As List(Of SqlFilterItem), ByRef params As Dictionary(Of String, String)) As String
            Const AND_STR As String = " AND "
            Const OR_STR As String = " OR "
            'orderListで順番を管理し、filterの実体はfilterListに格納。
            Dim orderList As New SortedList(Of String, Integer)
            Dim filterList As New SortedList(Of Integer, List(Of SqlFilterItem))
            Dim order As Integer = 0

            '基本戦略
            '受け取った選択条件の配列は、基本的に入力された順を正とする。ただ、同一列名の条件の場合これを固める。
            'その為、where区における順序は「該当列名が最初に見つかった順序」の通りとなる
            'これにグループ(括弧でくくってまとめたい条件)の概念を加味すると、以下の順の並びとする
            '「グループの初回発見順序」/「列名の初回発見順序」
            'グループがない場合、無名グループの割当を行う
            For Each item As SqlFilterItem In fs

                If item.Value Is Nothing OrElse Not item.Value = "" Then '有効なフィルタか確認(whereの場合空白指定は無視する)

                    'キーを作成
                    Dim key As String = ""
                    If Not item.Group Is Nothing Then
                        key += item.Group.Name + vbTab + item.Column
                    Else
                        key += vbTab + item.Column
                    End If

                    'オーダーを取得(なければ作成)
                    Dim itemOrder As Integer = 0
                    Dim fList As List(Of SqlFilterItem) = Nothing
                    If orderList.ContainsKey(key) Then
                        itemOrder = orderList(key)
                    Else
                        itemOrder = order
                        orderList.Add(key, itemOrder)
                        filterList.Add(itemOrder, New List(Of SqlFilterItem)) '新規リストを作成
                        order += 1
                    End If

                    fList = filterList(itemOrder)

                    'フィルタ値を格納
                    If Not item.Value Is Nothing AndAlso item.Value.Contains(ValueSeparator) Then '複数値指定の場合、分割を実施
                        Dim valuelist As ArrayList = New ArrayList(item.Value.Split(ValueSeparator)) 'タブ/カンマ区切りの場合を想定
                        For i As Integer = 0 To valuelist.Count - 1
                            Dim filter As New SqlFilterItem(item, valuelist(i))
                            fList.Add(filter)
                        Next
                    Else
                        fList.Add(item)
                    End If

                End If
            Next

            'WHERE条件を作成->同じグループが設定されているものはOR条件にまとめる。そうでないものは普通にAND
            Dim where As String = ""
            Dim groupStr As String = ""
            Dim concat As String = ""

            For i As Integer = 0 To filterList.Count - 1
                Dim keyArray As String() = Split(orderList.Keys(orderList.IndexOfValue(filterList.Keys(i))), vbTab)
                Dim fList As List(Of SqlFilterItem) = filterList(filterList.Keys(i))
                Dim filterStr As String = ""
                Dim groupBreak As Boolean = False

                'Filterの条件句作成
                For j As Integer = 0 To fList.Count - 1
                    Dim notStatement As String = ""

                    If fList(j).IsNots Then
                        notStatement = "NOT "
                    End If

                    If Not fList(j).Value Is Nothing Then
                        Dim paramName As String = ""
                        If String.IsNullOrEmpty(fList(j).ParamName) Then 'パラメーター指定がない場合勝手に振る
                            paramName = "F" + i.ToString + "V" + j.ToString
                        Else
                            paramName = fList(j).ParamName
                        End If

                        params.Add(paramName, fList(j).Value)

                        filterStr += notStatement + formatColumn(fList(j)) + " " + fList(j).Operand + " " + PARAM_HEAD + paramName + " " + OR_STR

                    Else
                        filterStr += notStatement + formatColumn(fList(j)) + " IS NULL " + OR_STR
                    End If

                Next

                'Filterの結合句作成
                If Not fList(0).Group Is Nothing AndAlso fList(0).Group.isOrGroup Then
                    concat = OR_STR
                Else
                    concat = AND_STR
                End If

                If fList.Count > 1 Then
                    groupStr += " ( " + eraseLastChar(filterStr, OR_STR) + " ) " + concat
                Else
                    groupStr += eraseLastChar(filterStr, OR_STR) + concat
                End If

                'グループのBreak判定(ループ最終か、グループ最終の場合)
                If i < filterList.Count - 1 Then
                    Dim keyNext As String() = Split(orderList.Keys(orderList.IndexOfValue(filterList.Keys(i + 1))), vbTab)
                    If Not keyNext(0).Equals(keyArray(0)) Then
                        groupBreak = True
                    End If
                Else
                    groupBreak = True
                End If

                If groupBreak Then
                    If String.IsNullOrEmpty(keyArray(0)) Then 'グループの場合
                        where += eraseLastChar(groupStr, concat) + AND_STR
                    Else
                        where += " ( " + eraseLastChar(groupStr, concat) + " ) " + AND_STR
                    End If

                    groupStr = ""
                    concat = ""
                End If
            Next

            If where <> "" Then
                where = eraseLastChar(where, AND_STR)
            End If

            Return where

        End Function
        Private Function eraseLastChar(ByVal str As String, ByVal eraseStr As String) As String
            Dim result As String = str
            If str.IndexOf(eraseStr) > -1 Then
                result = str.Substring(0, str.LastIndexOf(eraseStr))
            End If
            Return result
        End Function
        Private Function eraseFirstChar(ByVal str As String, ByVal eraseStr As String) As String
            Dim result As String = str
            If str.IndexOf(eraseStr) > -1 Then
                result = str.Substring(str.IndexOf(eraseStr) + eraseStr.Length)
            End If
            Return result
        End Function


    End Class

End Namespace
