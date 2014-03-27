Namespace Gears.Util

    ''' <summary>
    ''' Controlを扱うためのユーティリティ
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ControlSearcher

        ''' <summary>
        ''' 各コントロールを処理するためのデリゲート
        ''' </summary>
        ''' <param name="control"></param>
        ''' <param name="dto"></param>
        ''' <remarks></remarks>
        Public Delegate Sub fetchControl(ByVal control As Control, ByRef dto As GearsDTO)

        ''' <summary>
        ''' コントロールが処理対象であるかを判断するためのデリゲート
        ''' </summary>
        ''' <param name="control"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Delegate Function isFetchTgt(ByVal control As Control) As Boolean

        ''' <summary>
        ''' 与えられたコントロールに対し、子コントロールの探索を実施する(※自身は含まれないため注意)<br/>
        ''' isTargetの判定がTrueになるものに対し、callbackが実行される
        ''' </summary>
        ''' <param name="parent"></param>
        ''' <param name="callback"></param>
        ''' <param name="isTarget"></param>
        ''' <param name="dto"></param>
        ''' <remarks></remarks>
        Public Shared Sub fetchControls(ByVal parent As Control, ByVal callback As fetchControl, ByVal isTarget As isFetchTgt, Optional ByRef dto As GearsDTO = Nothing)

            If parent IsNot Nothing AndAlso parent.HasControls() Then
                Dim child As Control
                For Each child In parent.Controls()
                    If isTarget Is Nothing OrElse isTarget(child) Then '対象と判定されたもののみ、Fetchする(ない場合All True)
                        callback(child, dto)
                    End If
                    fetchControls(child, callback, isTarget, dto)
                Next
            End If

        End Sub

        ''' <summary>
        ''' 与えられたコントロールに対し、子コントロールの探索を実施する(※自身は含まれないため注意)<br/>
        ''' isFetchがTrueのもののみ検索を行い、isTargetの判定がTrueになるものに対しcallbackが実行される
        ''' </summary>
        ''' <param name="parent"></param>
        ''' <param name="callback"></param>
        ''' <param name="isTarget"></param>
        ''' <param name="dto"></param>
        ''' <remarks></remarks>
        Public Shared Sub fetchControls(ByVal parent As Control, ByVal callback As fetchControl, ByVal isTarget As isFetchTgt, ByVal isFetch As isFetchTgt, Optional ByRef dto As GearsDTO = Nothing)

            If parent IsNot Nothing AndAlso parent.HasControls() Then
                Dim child As Control
                For Each child In parent.Controls()
                    If isTarget Is Nothing OrElse isTarget(child) Then '対象と判定されたもののみ、Fetchする(ない場合All True)
                        callback(child, dto)
                    End If
                    If isFetch Is Nothing OrElse isFetch(child) Then
                        fetchControls(child, callback, isTarget, isFetch, dto)
                    End If
                Next
            End If

        End Sub


        ''' <summary>
        ''' 与えられたコントロールに対し、親コントロールの探索を実施する(※自身は含まれないため注意)<br/>
        ''' isTargetの判定がTrueになるものに対し、callbackが実行される
        ''' </summary>
        ''' <param name="con"></param>
        ''' <param name="callback"></param>
        ''' <param name="isTarget"></param>
        ''' <param name="dto"></param>
        ''' <remarks></remarks>
        Public Shared Sub fetchParents(ByVal con As Control, ByVal callback As fetchControl, ByVal isTarget As isFetchTgt, Optional ByRef dto As GearsDTO = Nothing)

            '自身の親がexcept対象であるか否かを確認する
            Dim parent As Control = con.Parent
            While Not parent Is Nothing
                If isTarget Is Nothing OrElse isTarget(parent) Then
                    callback(parent, dto)
                ElseIf TypeOf parent Is Page Then
                    parent = Nothing 'ページコントロールは画面に1つしか存在しないため、Pageに達したら抜ける(無限ループの保険)
                End If
                If parent IsNot Nothing Then parent = parent.Parent
            End While

        End Sub

        ''' <summary>
        ''' 指定されたコントロールを検索する
        ''' </summary>
        ''' <param name="con">検索対象コントロール</param>
        ''' <param name="conid">検索するコントロールのID</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function findControl(ByRef con As Control, ByVal conid As String) As Control
            Dim findout As Control = Nothing
            ControlSearcher.fetchControls(con,
                           Sub(control As Control, ByRef dto As GearsDTO)
                               findout = control
                           End Sub,
                           Function(control As Control) As Boolean
                               Return control.ID = conid
                           End Function)
            Return findout
        End Function

        ''' <summary>
        ''' Submitイベントを発生させたコントロールを特定する
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetSubmitCausedControl(ByVal page As Page) As Control
            Dim result As Control = Nothing
            Dim controlName As String = page.Request.Params("__EVENTTARGET")

            If Not String.IsNullOrEmpty(controlName) Then
                result = findControl(page, controlName)

            Else
                Dim imageControl As Control = Nothing

                For i As Integer = 0 To page.Request.Form.Keys.Count - 1
                    Dim conInForm As Control = page.FindControl(page.Request.Form.Keys(i))
                    If TypeOf conInForm Is Button Then
                        result = conInForm
                    End If

                    If page.Request.Form.Keys(i).EndsWith(".x") Or page.Request.Form.Keys(i).EndsWith(".y") Then
                        imageControl = page.FindControl(page.Request.Form.Keys(i).Substring(0, page.Request.Form.Keys(i).Length - 2))
                    End If
                Next

                If result Is Nothing Then
                    result = imageControl
                End If

            End If

            Return result

        End Function

        ''' <summary>
        ''' イベントが発生したコントロールから、非同期更新を発生させたUpdatePanelを特定する
        ''' </summary>
        ''' <param name="causedControl"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetAsynchronousPostBackPanel(ByVal page As Page, ByVal causedControl As Control) As Control
            If causedControl Is Nothing Then Return Nothing

            Dim con As Control = causedControl
            While Not con Is Nothing And Not TypeOf con Is UpdatePanel
                con = con.Parent
            End While
            If con Is Nothing Then
                ControlSearcher.fetchControls(page, Sub(control As Control, ByRef dto As GearsDTO)
                                                        For Each trigger As AsyncPostBackTrigger In CType(control, UpdatePanel).Triggers
                                                            If trigger.ControlID = causedControl.ID Then
                                                                con = control
                                                            End If
                                                        Next
                                                    End Sub,
                                               Function(control As Control) As Boolean
                                                   If TypeOf control Is UpdatePanel Then Return True Else Return False
                                               End Function)

            End If

            Return con

        End Function

    End Class


End Namespace
