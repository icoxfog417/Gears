﻿Imports Microsoft.VisualBasic
Imports System.Data
Imports System.Collections.Generic
Imports Gears.Validation
Imports Gears.Util

Namespace Gears.DataSource

    ''' <summary>
    ''' データソースクラス作成用の抽象クラス
    ''' </summary>
    ''' <remarks></remarks>
    Public Class GearsDataSource
        Implements IDataSource

        ''' <summary>
        ''' データを選択するためのビュー/テーブル名。
        ''' </summary>
        Private _selectView As SqlDataSource = Nothing
        Public Property SelectView As SqlDataSource
            Get
                If _selectView IsNot Nothing Then
                    Return _selectView
                Else
                    Return TargetTable
                End If
            End Get
            Set(value As SqlDataSource)
                _selectView = value
            End Set
        End Property

        Private _targetTable As SqlDataSource = Nothing
        ''' <summary>
        ''' データの更新対象となるビュー/テーブル名。
        ''' </summary>
        Public Property TargetTable As SqlDataSource
            Get
                Return _targetTable
            End Get
            Set(value As SqlDataSource)
                _targetTable = value
                If _selectView Is Nothing Then
                    _selectView = _targetTable
                End If
            End Set
        End Property

        ''' <summary>マルチバイト対応が必要なデータソースか否か(日本語名テーブルなど)</summary>
        Public Property IsMultiByte() As Boolean

        ''' <summary>ビジネスロジックによるバリデーションを行うためのオブジェクト</summary>
        Public Property ModelValidator() As AbsModelValidator

        ''' <summary>項目変換ルールの設定</summary>
        Public Property Mapper() As INameMapper = New NameMapper

        Private _connectionName As String = ""
        ''' <summary>
        ''' 接続文字列
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ConnectionName As String
            Get
                Return _connectionName
            End Get
        End Property

        Private _resultSet As New DataTable

        ''' <summary>
        ''' 実行後の結果セットを取得する
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function gResultSet() As System.Data.DataTable Implements IDataSource.gResultSet
            Return _resultSet
        End Function

        ''' <summary>
        ''' 結果セットから指定インデックス/行数の値を取得する
        ''' </summary>
        ''' <param name="index"></param>
        ''' <param name="rowIndex"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Item(ByVal index As Integer, Optional ByVal rowIndex As Integer = 0) As Object
            Return DataSetReader.Item(_resultSet, index, rowIndex)
        End Function

        ''' <summary>
        ''' 結果セットから指定カラム/行数の値を取得する
        ''' </summary>
        ''' <param name="index"></param>
        ''' <param name="rowIndex"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Item(ByVal index As String, Optional ByVal rowIndex As Integer = 0) As Object
            Return DataSetReader.Item(_resultSet, index, rowIndex)
        End Function

        ''' <summary>
        ''' 楽観ロック用の列の設定
        ''' </summary>
        ''' <param name="colname"></param>
        ''' <param name="ltype"></param>
        ''' <remarks></remarks>
        Public Sub setLockCheckColumn(ByVal colname As String, ByVal ltype As LockType)
            SelectView.setLockCheckColumn(colname, ltype)
            TargetTable.setLockCheckColumn(colname, ltype)
        End Sub

        ''' <summary>
        ''' 楽観ロック用の列の取得
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function getLockCheckColumns() As List(Of String)
            Dim locks As New List(Of String)
            For Each item As KeyValuePair(Of String, LockType) In TargetTable.LockCheckColumn
                locks.Add(item.Key)
            Next
            Return locks
        End Function

        ''' <summary>
        ''' SQL実行結果から楽観ロック用列の値を取得する
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function getLockValue() As List(Of SqlFilterItem)
            Dim param As New List(Of SqlFilterItem)
            If _resultSet.Rows.Count > 0 Then
                For Each item As KeyValuePair(Of String, LockType) In TargetTable.LockCheckColumn
                    Dim value As Object = DataSetReader.Item(_resultSet, item.Key)
                    If Not IsDBNull(value) Then
                        param.Add(New SqlFilterItem(item.Key).eq(value))
                    Else
                        param.Add(New SqlFilterItem(item.Key).eq(String.Empty))
                    End If
                Next
            End If

            Return param

        End Function

        ''' <summary>
        ''' SQLに楽観ロック列を更新する値をセットする
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' <remarks></remarks>
        Protected Sub addLockValue(ByVal sqlb As SqlBuilder)
            For Each item As KeyValuePair(Of String, LockType) In TargetTable.LockCheckColumn
                '元々設定されていた場合はそちらを優先する
                If sqlb.Selection(item.Key) Is Nothing AndAlso Not getLockTypeValue(item.Value) Is Nothing Then
                    sqlb.addSelection(SqlBuilder.S(item.Key).setValue(getLockTypeValue(item.Value)))
                End If
            Next
        End Sub

        ''' <summary>
        ''' 楽観ロックの各タイプに応じた更新値の取得
        ''' </summary>
        ''' <param name="ltype"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function getLockTypeValue(ByVal ltype As LockType) As Object
            Dim val As Object = ""
            Dim ndate As Date = Now()
            Select Case ltype
                Case LockType.UDATE
                    val = ndate
                Case LockType.UDATESTR
                    val = ndate.ToString("yyyyMMdd")
                Case LockType.UTIMESTR
                    val = ndate.ToString("HHmmss")
                Case LockType.VNUM
                    val = ndate.ToString("yyyyMMddHHmmss")
                Case LockType.USER 'HttpContextはGearsDataSourceからかなり遠いが。。。
                    If Not HttpContext.Current Is Nothing AndAlso Not HttpContext.Current.User Is Nothing AndAlso Not HttpContext.Current.User.Identity Is Nothing Then
                        val = HttpContext.Current.User.Identity.Name
                    Else
                        val = Nothing
                    End If
            End Select

            Return val

        End Function


        Protected Sub New(ByVal conName As String)
            _connectionName = conName
        End Sub

        ''' <summary>データソースとなるテーブルを指定しインスタンスを作成する</summary>
        ''' <param name="conName">接続文字列</param>
        ''' <param name="table">テーブルを示すSqlDataSource</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal conName As String, ByVal table As SqlDataSource)
            Me.New(conName)
            TargetTable = table
        End Sub

        ''' <summary>データソースとなるテーブル名を文字列で指定しインスタンスを作成する</summary>
        ''' <param name="conName">接続文字列</param>
        ''' <param name="tableName">テーブル名</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal conName As String, ByVal tableName As String)
            Me.New(conName)
            TargetTable = SqlBuilder.DS(tableName)
        End Sub

        ''' <summary>選択用のビューと更新用のテーブルをそれぞれ指定してインスタンスを作成する</summary>
        ''' <param name="conName">接続文字列</param>
        ''' <param name="view">選択用のビュー</param>
        ''' <param name="table">更新用のテーブル</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal conName As String, ByVal view As SqlDataSource, ByVal table As SqlDataSource)
            Me.New(conName)
            SelectView = view
            TargetTable = table
        End Sub

        ''' <summary>選択用のビューと更新用のテーブルをそれぞれ文字列で指定してインスタンスを作成する</summary>
        ''' <param name="conName">接続文字列</param>
        ''' <param name="viewName">選択用のビュー名</param>
        ''' <param name="tableName">更新用のテーブル名</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal conName As String, ByVal viewName As String, ByVal tableName As String)
            Me.New(conName)
            SelectView = SqlBuilder.DS(viewName)
            TargetTable = SqlBuilder.DS(tableName)
        End Sub


        ''' <summary>
        ''' 選択/更新処理を実行する
        ''' </summary>
        ''' <param name="dto"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function execute(ByVal dto As GearsDTO) As DataTable

            Try
                'データベース更新処理
                Select Case dto.Action
                    Case ActionType.DEL
                        gDelete(dto)
                    Case ActionType.INS
                        gInsert(dto)
                    Case ActionType.SAVE
                        gSave(dto)
                    Case ActionType.SEL
                        If dto.isPaging Then
                            gSelectPageBy(dto.RowsInPage(), dto.RowIndexOfPage, dto)
                        Else
                            gSelect(dto)
                        End If
                    Case ActionType.UPD
                        gUpdate(dto)
                End Select

            Catch ex As GearsException
                Throw
            Catch ex As Exception
                Throw
            End Try

            Return _resultSet

        End Function

        ''' <summary>
        ''' 受け取ったDTOをSQL実行用のオブジェクトに変換する
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function makeSqlBuilder(ByVal data As GearsDTO) As SqlBuilder
            Dim sqlb As SqlBuilder = Nothing
            Dim dbServer As DbServerType = SqlBuilder.GetDbServerType(ConnectionName)

            'SqlBuilderの作成
            If data IsNot Nothing Then
                sqlb = data.toSqlBuilder()
                sqlb.DbServer = dbServer
            Else
                sqlb = New SqlBuilder(dbServer, ActionType.SEL)
            End If

            'データソースの設定
            If sqlb Is Nothing OrElse sqlb.Action = ActionType.SEL Then
                sqlb.DataSource = SelectView
            Else
                sqlb.DataSource = TargetTable '更新用のデータソースを設定
            End If

            'プロパティの設定
            sqlb.IsMultiByte = _IsMultiByte
            If sqlb.Mapper Is Nothing Then sqlb.Mapper = Mapper

            '楽観ロックの設定
            If SelectView.LockCheckColumn.Count > 0 Then
                '選択の場合で明確な選択項目がない場合(SELECT * の場合)以外は、楽観ロックカラムを指定(選択)する
                If Not (data.Action = ActionType.SEL And sqlb.Selection.Where(Function(s) Not s.IsNoSelect).Count = 0) Then
                    addLockValue(sqlb)
                End If
            End If

            Return sqlb

        End Function

        ''' <summary>
        ''' 実行用のSQLを作成するためのメソッド
        ''' </summary>
        ''' <param name="dto"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function makeExecute(ByVal dto As GearsDTO) As SqlBuilder
            Dim sqlb As SqlBuilder = Nothing

            Try
                Dim action As ActionType = confirmRecord(dto) 'Saveの場合INS/UPDに調整など
                sqlb = makeSqlBuilder(dto)
                If dto.Action = ActionType.SAVE And action = ActionType.UPD Then
                    sqlb = keySelectToFilter(sqlb) '更新キーをフィルタに移す
                End If
                sqlb.Action = action
            Catch ex As Exception
                Throw
            End Try

            Return sqlb

        End Function

        ''' <summary>
        ''' Selectを実行する
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function gSelect(ByVal data As GearsDTO) As System.Data.DataTable Implements IDataSource.gSelect
            Dim sqlb As SqlBuilder = makeSqlBuilder(data)
            Return gSelect(sqlb)
        End Function

        ''' <summary>
        ''' メソッドチェーンを利用したSelectを実行する
        ''' </summary>
        ''' <param name="selection"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function gSelect(ByVal ParamArray selection As SqlSelectItem()) As gSourceExpression
            Return gExecute(ActionType.SEL, selection.ToList)
        End Function

        ''' <summary>
        ''' メソッドチェーンを利用したSelectを実行する
        ''' </summary>
        ''' <param name="selection"></param>
        ''' <remarks></remarks>
        Public Function gSelect(ByVal selection As List(Of SqlSelectItem)) As gSourceExpression
            Return gExecute(ActionType.SEL, selection)
        End Function

        ''' <summary>
        ''' メソッドチェーンを利用したSelectを実行する
        ''' </summary>
        ''' <param name="cons"></param>
        ''' <remarks></remarks>
        Public Function gSelect(ByVal cons As Control()) As gSourceExpression
            Return gExecute(ActionType.SEL, cons.ToList)
        End Function

        ''' <summary>
        ''' ページサイズを指定したSelectを行う
        ''' </summary>
        ''' <param name="maximumRows"></param>
        ''' <param name="startRowIndex"></param>
        ''' <param name="data"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function gSelectPageBy(ByVal maximumRows As Integer, ByVal startRowIndex As Integer, ByVal data As GearsDTO) As DataTable Implements IDataSource.gSelectPageBy
            Dim sqlb As SqlBuilder = makeSqlBuilder(data)
            sqlb.setPaging(startRowIndex, maximumRows)
            Return gSelect(sqlb)

        End Function

        ''' <summary>
        ''' Selectを実行する
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function gSelect(ByVal sqlb As SqlBuilder) As System.Data.DataTable

            Try
                Dim executor As New GearsSqlExecutor(ConnectionName)
                _resultSet = executor.load(sqlb)
                convertResultSet(_resultSet, sqlb)
                LogMessage(sqlb, executor)
            Catch ex As Exception
                LogMessage(sqlb, Nothing)
                Throw
            End Try

            Return _resultSet

        End Function

        ''' <summary>
        ''' SqlBuilderに設定された項目変換に基づき、DataTableのカラム名を変換する
        ''' </summary>
        ''' <param name="dataSet"></param>
        ''' <param name="sqlb"></param>
        ''' <remarks></remarks>
        Public Sub convertResultSet(ByRef dataSet As DataTable, ByVal sqlb As SqlBuilder)
            Dim conv As INameMapper = sqlb.Mapper
            If Not conv Is Nothing AndAlso Not dataSet Is Nothing AndAlso dataSet.Columns.Count > 0 Then
                For Each col As DataColumn In dataSet.Columns

                    If Not String.IsNullOrEmpty(conv.changeColumnToItem(col.ColumnName)) Then
                        col.ColumnName = conv.changeColumnToItem(col.ColumnName)
                    End If
                Next
            End If
        End Sub

        ''' <summary>
        ''' 件数の取得
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function gSelectCount(ByVal data As GearsDTO) As Integer Implements IDataSource.gSelectCount
            Dim sqlb As SqlBuilder = makeSqlBuilder(data)
            Return gSelectCount(sqlb)

        End Function

        ''' <summary>
        ''' 件数の取得
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function gSelectCount(ByVal sqlb As SqlBuilder) As Integer
            Dim count As Integer = 0

            Try
                Dim executor As New GearsSqlExecutor(ConnectionName)
                count = executor.count(sqlb)
                LogMessage(sqlb, executor)

            Catch ex As Exception
                LogMessage(sqlb, Nothing)
                Throw
            End Try

            Return count

        End Function

        ''' <summary>
        ''' Insertを実行する
        ''' </summary>
        ''' <param name="data"></param>
        ''' <remarks></remarks>
        Public Sub gInsert(ByVal data As GearsDTO) Implements IDataSource.gInsert
            Dim sqlb As SqlBuilder = makeExecute(data)
            gInsert(sqlb)
        End Sub

        ''' <summary>
        ''' Insertを実行する
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' <remarks></remarks>
        Public Overridable Sub gInsert(ByVal sqlb As SqlBuilder)
            executeProcess(sqlb)
        End Sub

        ''' <summary>
        ''' Updateを実行する
        ''' </summary>
        ''' <param name="data"></param>
        ''' <remarks></remarks>
        Public Sub gUpdate(ByVal data As GearsDTO) Implements IDataSource.gUpdate
            Dim sqlb As SqlBuilder = makeExecute(data)
            gUpdate(sqlb)
        End Sub

        ''' <summary>
        ''' Updateを実行する
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' <remarks></remarks>
        Public Overridable Sub gUpdate(ByVal sqlb As SqlBuilder)
            executeProcess(sqlb)
        End Sub

        ''' <summary>
        ''' Deleteを実行する
        ''' </summary>
        ''' <param name="data"></param>
        ''' <remarks></remarks>
        Public Sub gDelete(ByVal data As GearsDTO) Implements IDataSource.gDelete
            Dim sqlb As SqlBuilder = makeExecute(data)
            gDelete(sqlb)
        End Sub

        ''' <summary>
        ''' Deleteを実行する
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' <remarks></remarks>
        Public Overridable Sub gDelete(ByVal sqlb As SqlBuilder)
            executeProcess(sqlb)
        End Sub

        ''' <summary>
        ''' Saveを実行する(一致するキーがある場合Update/なければInsert)
        ''' </summary>
        ''' <param name="data"></param>
        ''' <remarks></remarks>
        Public Sub gSave(ByVal data As GearsDTO)
            Dim sqlb As SqlBuilder = makeExecute(data)
            gSave(sqlb)
        End Sub

        ''' <summary>
        ''' Saveを実行する(一致するキーがある場合Update/なければInsert)
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' <remarks></remarks>
        Public Overridable Sub gSave(ByVal sqlb As SqlBuilder)
            If sqlb.Action = ActionType.INS Then
                gInsert(sqlb)
            ElseIf sqlb.Action = ActionType.UPD Then
                gUpdate(sqlb)
            Else
                Dim action As ActionType = confirmRecord(sqlb)
                If action = ActionType.INS Then gInsert(sqlb)
                If action = ActionType.UPD Then gUpdate(keySelectToFilter(sqlb))
            End If
        End Sub

        ''' <summary>
        ''' メソッドチェーンを利用したSaveを実行する
        ''' </summary>
        ''' <param name="selection"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function gSave(ByVal ParamArray selection As SqlSelectItem()) As gSourceExpression
            Return gExecute(ActionType.SAVE, selection.ToList)
        End Function

        ''' <summary>
        ''' メソッドチェーンを利用したSaveを実行する
        ''' </summary>
        ''' <param name="selection"></param>
        ''' <remarks></remarks>
        Public Function gSave(ByVal selection As List(Of SqlSelectItem)) As gSourceExpression
            Return gExecute(ActionType.SAVE, selection)
        End Function

        ''' <summary>
        ''' メソッドチェーンを利用したSaveを実行する
        ''' </summary>
        ''' <param name="cons"></param>
        ''' <remarks></remarks>
        Public Function gSave(ByVal ParamArray cons As Control()) As gSourceExpression
            Return gExecute(ActionType.SAVE, cons.ToList)
        End Function

        ''' <summary>
        ''' メソッドチェーンで処理を行うためのダミーオブジェクト(Expression)を作成する
        ''' </summary>
        ''' <param name="action"></param>
        ''' <param name="selection"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function gExecute(ByVal action As ActionType, ByVal selection As List(Of SqlSelectItem)) As gSourceExpression
            Return New gSourceExpression(Me, action, selection)
        End Function

        ''' <summary>
        ''' メソッドチェーンで処理を行うためのダミーオブジェクト(Expression)を作成する
        ''' </summary>
        ''' <param name="action"></param>
        ''' <param name="cons"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function gExecute(ByVal action As ActionType, ByVal cons As List(Of Control)) As gSourceExpression
            Dim selection As New List(Of SqlSelectItem)
            For Each con As Control In cons
                Dim cInfos As List(Of GearsControlInfo) = con.toGControl.toControlInfo
                For Each cInfo As GearsControlInfo In cInfos
                    selection.Add(cInfo.toSelection)
                Next
            Next
            Return gExecute(action, selection)
        End Function

        ''' <summary>
        ''' 更新系処理を行うメソッド
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' <remarks></remarks>
        Protected Sub executeProcess(ByVal sqlb As SqlBuilder)

            GearsLogStack.setLog(sqlb.DataSource.Name + "へ更新前に、チェック処理を行います")
            beforeExecute(sqlb)

            Try
                Dim executor As New GearsSqlExecutor(ConnectionName)

                'Saveの場合、ActionTypeはINS/UPDのどちらかに編集されていることを前提とする
                executor.execute(sqlb)
                LogMessage(sqlb, executor)

                '実行結果を読み込む
                loadExecuted(sqlb)

            Catch ex As Exception
                LogMessage(sqlb, Nothing)
                Throw
            End Try

            afterExecute(sqlb)

        End Sub

        ''' <summary>
        ''' データベース更新後の値を読み込む
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' <remarks></remarks>
        Protected Sub loadExecuted(ByVal sqlb As SqlBuilder)

            '実行後のデータを読み込む
            Dim sqlbForResult As New SqlBuilder(sqlb)

            '楽観ロックの選択をクリア
            For Each lc As String In sqlbForResult.LockFilter.Select(Function(f) f.Column).ToList
                sqlbForResult.removeFilter(lc)
            Next

            If sqlb.Action = ActionType.INS Then
                'INSERTの場合フィルタ値がないため、設定値をフィルタ値に変換してセット
                sqlbForResult.Filter.Clear()
                For Each selection As SqlSelectItem In sqlb.Selection
                    If selection.IsKey Or (sqlb.Filter(selection.Column) IsNot Nothing AndAlso sqlb.Filter(selection.Column).IsKey) Then
                        sqlbForResult.addFilter(selection.toFilter)
                    End If
                Next
            End If

            '選択項目をクリア
            sqlbForResult.Selection.Clear()

            'SELECTビューから、更新対象を読み込む
            sqlbForResult.DataSource = SelectView

            gSelect(sqlbForResult)

            If gResultSet.Rows.Count = 0 Then '万一SELECT用のビューで検索が失敗した場合、テーブルから読み直す
                sqlbForResult.DataSource = TargetTable
                gSelect(sqlbForResult)
            End If

        End Sub

        ''' <summary>
        ''' 更新処理実行前に行われるトリガ処理
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' <remarks></remarks>
        Protected Overridable Sub beforeExecute(ByVal sqlb As SqlBuilder)
            Dim result As Boolean = True

            If Not ModelValidator Is Nothing Then
                result = False
                ModelValidator.Validate(sqlb)

                If ModelValidator.IsValid Then
                    result = True
                ElseIf ModelValidator.IsValidIgnoreAlert AndAlso sqlb.IsIgnoreAlert Then
                    result = True
                Else
                    ModelValidator.throwException()
                End If
            End If


        End Sub

        ''' <summary>
        ''' 更新処理実行後に行われるトリガ処理
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' <remarks></remarks>
        Protected Overridable Sub afterExecute(ByVal sqlb As SqlBuilder)
        End Sub

        ''' <summary>
        ''' Saveの場合のInsert/Update判定、また楽観ロックのチェックを行う
        ''' </summary>
        ''' <param name="confirmData"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function confirmRecord(ByVal confirmData As GearsDTO) As ActionType
            Return confirmRecord(makeSqlBuilder(confirmData))
        End Function

        ''' <summary>
        ''' Saveの場合のInsert/Update判定、また楽観ロックのチェックを行う
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function confirmRecord(ByVal sqlb As SqlBuilder) As ActionType
            Dim result As ActionType = sqlb.Action

            '処理用変数
            Dim keySelection As List(Of SqlSelectItem) = sqlb.Selection.Where(Function(s) s.IsKey And s.hasValue).ToList 'キー項目の列
            Dim keyFilter As List(Of SqlFilterItem) = sqlb.Filter.Where(Function(f) f.IsKey And f.hasValue).ToList

            'SAVE/Updateの場合、既存レコードの確認を行う
            If sqlb.Action = ActionType.UPD Or sqlb.Action = ActionType.SAVE Then

                '既存レコードの確認を行うSQLを作成
                Dim selSqlb As New SqlBuilder(sqlb, False) 'フィルタ/セレクタを削除しコピー
                selSqlb.DataSource = Me.TargetTable 'ビューではなく、元テーブルに対しSELECTを行う(キー項目は必ず元テーブルに含まれるが、ビューはその限りではない)

                If keyFilter.Count > 0 Then 'キー選択がある場合、フィルタとしてそのまま追加
                    Dim isKeyUpdateOccur As Boolean = False
                    keyFilter.ForEach(Sub(f)
                                          If sqlb.Selection(f.Column) IsNot Nothing AndAlso f.Value <> sqlb.Selection(f.Column).Value Then
                                              isKeyUpdateOccur = True
                                              selSqlb.addFilter(sqlb.Selection(f.Column).toFilter) '更新対象先を検索するよう差し替え
                                          Else
                                              selSqlb.addFilter(f)
                                          End If
                                      End Sub)

                    'キーを更新するUpdateは、許可されている場合のみOK
                    If isKeyUpdateOccur And Not sqlb.IsPermitOtherKeyUpdate Then
                        Throw New GearsSqlException(sqlb.Action, "キーの変更処理は許可されていません(PermitOtherKeyUpdate:False)")
                    End If

                ElseIf keySelection.Count > 0 Then 'キー選択がなく、キーの更新がある場合それをフィルタとして設定
                    keySelection.ForEach(Sub(s) selSqlb.addFilter(s.toFilter))
                Else
                    Throw New GearsSqlException(sqlb.Action, "キー項目が設定されていないか空白です", "更新対象のテーブルのキーを表すGearsControlに対しsetAskey()を行うか、名称に__KEYを含めるかし、キー情報を設定してください")
                End If

                'SELECTを実行
                Dim selResult As DataTable = gSelect(selSqlb)

                '既存レコードを確認
                If selResult.Rows.Count > 0 Then 'select結果が0件以上であれば更新対象データ有り
                    Dim isLockCheckOk As Boolean = True
                    If sqlb.Action = ActionType.SAVE Then result = ActionType.UPD

                    'ロックキーが存在する場合、その一致を確認
                    If sqlb.LockFilter.Count > 0 Then
                        Dim lockValues As List(Of SqlFilterItem) = getLockValue()
                        For Each lc As SqlFilterItem In sqlb.LockFilter
                            If lockValues.Where(Function(f) f.Column = lc.Column And f.Value = lc.Value).Count = 0 Then isLockCheckOk = False
                        Next

                        If Not isLockCheckOk Then
                            Dim lockInfo As String = ""
                            lockValues.ForEach(Sub(f) lockInfo += f.Column + ":" + f.Value.ToString + " ")
                            Throw New GearsOptimisticLockException(Trim(lockInfo))
                        End If
                    Else

                    End If
                Else
                    'ActionType=UPDの場合、更新対象がないことになるがエラーにはしない
                    If sqlb.Action = ActionType.SAVE Then result = ActionType.INS
                End If

            End If

            Return result

        End Function

        ''' <summary>
        ''' SAVEの場合で、UPDATEと判定された場合キー設定されたSelectionをFilterに移す処理を行う
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' <remarks></remarks>
        Private Function keySelectToFilter(ByRef sqlb As SqlBuilder) As SqlBuilder
            Dim newBuilder As New SqlBuilder(sqlb)
            Dim keySelects As List(Of SqlSelectItem) = newBuilder.Selection.Where(Function(s) s.IsKey).ToList
            For Each s As SqlSelectItem In keySelects
                If newBuilder.Filter(s.Column) Is Nothing Then
                    newBuilder.addFilter(s.toFilter)
                End If
            Next

            Return newBuilder

        End Function

        ''' <summary>SQL実行のメッセージをログに書き込む</summary>
        ''' <param name="sqlb"></param>
        ''' <param name="executor"></param>
        ''' <remarks></remarks>
        Private Sub LogMessage(ByVal sqlb As SqlBuilder, ByVal executor As GearsSqlExecutor)
            If executor IsNot Nothing Then
                GearsLogStack.setLog(GearsDTO.ActionToString(sqlb.Action) + "処理を実行しました(" + executor.SqlElapsedTime(1000) + "sec)", 3, sqlb.confirmSql(sqlb.Action))
            Else
                GearsLogStack.setLog(GearsDTO.ActionToString(sqlb.Action) + "処理を実行します", 3, sqlb.confirmSql(sqlb.Action))
            End If
        End Sub


    End Class

End Namespace
