﻿Imports System.Linq
Imports iBMSC.Editor

Partial Public Class MainWindow
    Private Sub OpenBMS(ByVal xStrAll As String, Optional xGhost As Boolean = False, Optional xComment As Boolean = False)
        KMouseOver = -1

        'Line feed validation: will remove some empty lines
        xStrAll = Replace(Replace(Replace(xStrAll, vbLf, vbCr), vbCr & vbCr, vbCr), vbCr, vbCrLf)

        Dim xStrLine() As String = Split(xStrAll, vbCrLf, , CompareMethod.Text)
        Dim xStrLine2(xStrLine.Length) As String ' Create a second array which removes expansion codes from the second for loop
        Dim xI1 As Integer
        Dim sLine As String
        Dim xExpansion As String = ""
        Dim xEditorExpansion As String = ""
        Dim nNotes As Integer = 1

        ' Assume ghost note strings contain only notes in the section. Expansion field to be saved separately
        If xGhost Then
            nNotes = Notes.Length
        ElseIf xComment Then
            xStrLine2 = xStrLine
            nNotes = Notes.Length
        Else ' Initialization
            ReDim Notes(0)
            ReDim mColumn(999)
            ReDim hWAV(1295)
            ReDim hBPM(1295)    'x10000
            ReDim hSTOP(1295)
            ReDim hBMSCROLL(1295)
            Me.InitializeNewBMS()
            Me.InitializeOpenBMS()

            With Notes(0)
                .ColumnIndex = niBPM
                .VPosition = -1
                '.LongNote = False
                '.Selected = False
                .Value = 1200000
            End With
        End If

        'old list below, not sure what this means
        'random, setRandom      0
        'endRandom              0
        'if             +1
        'else           0
        'endif          -1
        'switch, setSwitch      +1
        'case, skip, def        0
        'endSw                  -1
        'P: I'm gonna do what's called a pro gamer move

        Dim xStack As Integer = 0
        Dim nLine As Integer = -1

        For Each sLine In xStrLine
            Dim sLineTrim As String = sLine.Trim
            If sLineTrim = "" Then Continue For

            If xStack > 0 Then AddToExpansion(xExpansion, xStack, sLine) : Continue For
            If Not xGhost AndAlso Not xComment Then ' Load header if not ghost notes and not comment notes

                If sLineTrim.StartsWith("#") And Mid(sLineTrim, 5, 3) = "02:" Then
                    Dim xIndex As Integer = CInt(Mid(sLineTrim, 2, 3))
                    Dim xRatio As Double = Val(Mid(sLineTrim, 8))
                    Dim xxD As Long = GetDenominator(xRatio)
                    MeasureLength(xIndex) = xRatio * 192.0R
                    LBeat.Items(xIndex) = Add3Zeros(xIndex) & ": " & xRatio & IIf(xxD > 10000, "", " ( " & CLng(xRatio * xxD) & " / " & xxD & " ) ").ToString()
                    Continue For

                ElseIf SWIC(sLineTrim, "#WAV") Then
                    hWAV(C36to10(Mid(sLineTrim, Len("#WAV") + 1, 2))) = Mid(sLineTrim, Len("#WAV") + 4)
                    Continue For

                ElseIf SWIC(sLineTrim, "#BPM") And Not Mid(sLineTrim, Len("#BPM") + 1, 1).Trim = "" Then  'If BPM##
                    ' zdr: No limits on BPM editing.. they don't make much sense.
                    hBPM(C36to10(Mid(sLineTrim, Len("#BPM") + 1, 2))) = CLng(CDbl(Mid(sLineTrim, Len("#BPM") + 4)) * 10000)
                    Continue For

                    'No limits on STOPs either.
                ElseIf SWIC(sLineTrim, "#STOP") Then
                    hSTOP(C36to10(Mid(sLineTrim, Len("#STOP") + 1, 2))) = CLng(CDbl(Mid(sLineTrim, Len("#STOP") + 4)) * 10000)
                    Continue For

                ElseIf SWIC(sLineTrim, "#SCROLL") Then
                    hBMSCROLL(C36to10(Mid(sLineTrim, Len("#SCROLL") + 1, 2))) = CLng(CDbl(Mid(sLineTrim, Len("#SCROLL") + 4)) * 10000)
                    Continue For

                ElseIf SWIC(sLineTrim, "#TITLE") Then
                    THTitle.Text = Mid(sLineTrim, Len("#TITLE") + 1).Trim
                    Continue For

                ElseIf SWIC(sLineTrim, "#ARTIST") Then
                    THArtist.Text = Mid(sLineTrim, Len("#ARTIST") + 1).Trim
                    Continue For

                ElseIf SWIC(sLineTrim, "#GENRE") Then
                    THGenre.Text = Mid(sLineTrim, Len("#GENRE") + 1).Trim
                    Continue For

                ElseIf SWIC(sLineTrim, "#BPM") Then  'If BPM ####
                    Notes(0).Value = CLng(CDbl(Mid(sLineTrim, Len("#BPM") + 1).Trim)) * 10000
                    THBPM.Value = CDec(Notes(0).Value / 10000)
                    Continue For

                ElseIf SWIC(sLineTrim, "#PLAYER") Then
                    Dim xInt As Integer = CInt(Mid(sLineTrim, Len("#PLAYER") + 1).Trim)
                    If xInt >= 1 And xInt <= 4 Then _
                        CHPlayer.SelectedIndex = xInt - 1
                    Continue For

                ElseIf SWIC(sLineTrim, "#RANK") Then
                    Dim xInt As Integer = CInt(Mid(sLineTrim, Len("#RANK") + 1).Trim)
                    If xInt >= 0 And xInt <= 4 Then _
                        CHRank.SelectedIndex = xInt
                    Continue For

                ElseIf SWIC(sLineTrim, "#PLAYLEVEL") Then
                    THPlayLevel.Text = Mid(sLineTrim, Len("#PLAYLEVEL") + 1).Trim
                    Continue For

                ElseIf SWIC(sLineTrim, "#SUBTITLE") Then
                    THSubTitle.Text = Mid(sLineTrim, Len("#SUBTITLE") + 1).Trim
                    Continue For

                ElseIf SWIC(sLineTrim, "#SUBARTIST") Then
                    THSubArtist.Text = Mid(sLineTrim, Len("#SUBARTIST") + 1).Trim
                    Continue For

                ElseIf SWIC(sLineTrim, "#STAGEFILE") Then
                    THStageFile.Text = Mid(sLineTrim, Len("#STAGEFILE") + 1).Trim
                    Continue For

                ElseIf SWIC(sLineTrim, "#BANNER") Then
                    THBanner.Text = Mid(sLineTrim, Len("#BANNER") + 1).Trim
                    Continue For

                ElseIf SWIC(sLineTrim, "#BACKBMP") Then
                    THBackBMP.Text = Mid(sLineTrim, Len("#BACKBMP") + 1).Trim
                    Continue For

                ElseIf SWIC(sLineTrim, "#DIFFICULTY") Then
                    Try
                        CHDifficulty.SelectedIndex = Integer.Parse(Mid(sLineTrim, Len("#DIFFICULTY") + 1).Trim)
                    Catch ex As Exception
                    End Try
                    Continue For

                ElseIf SWIC(sLineTrim, "#DEFEXRANK") Then
                    THExRank.Text = Mid(sLineTrim, Len("#DEFEXRANK") + 1).Trim
                    Continue For

                ElseIf SWIC(sLineTrim, "#TOTAL") Then
                    Dim xStr As String = Mid(sLineTrim, Len("#TOTAL") + 1).Trim
                    'If xStr.EndsWith("%") Then xStr = Mid(xStr, 1, Len(xStr) - 1)
                    THTotal.Text = xStr
                    Continue For

                ElseIf SWIC(sLineTrim, "#COMMENT") Then
                    Dim xStr As String = Mid(sLineTrim, Len("#COMMENT") + 1).Trim
                    If xStr.StartsWith("""") Then xStr = Mid(xStr, 2)
                    If xStr.EndsWith("""") Then xStr = Mid(xStr, 1, Len(xStr) - 1)
                    THComment.Text = xStr
                    Continue For

                ElseIf SWIC(sLineTrim, "#LNTYPE") Then
                    'THLnType.Text = Mid(sLineTrim, Len("#LNTYPE") + 1).Trim
                    If Val(Mid(sLineTrim, Len("#LNTYPE") + 1).Trim) = 1 Then CHLnObj.SelectedIndex = 0
                    Continue For

                ElseIf SWIC(sLineTrim, "#LNOBJ") Then
                    Dim xValue As Integer = C36to10(Mid(sLineTrim, Len("#LNOBJ") + 1).Trim)
                    CHLnObj.SelectedIndex = xValue
                    Continue For

                ElseIf SWIC(sLineTrim, "#ECMD") Then
                    xEditorExpansion &= sLineTrim.Replace("#ECMD", "#") & vbCrLf
                    Continue For

                ElseIf SWIC(sLineTrim, "#ECOM") Then ' Comment notes
                    Dim xComVal As Integer = C36to10(Mid(sLineTrim, Len("#ECOM") + 1, 2).ToString())
                    hCOM(C36to10(xComVal)) = Mid(sLineTrim, Len("#ECOM") + 4)
                    If xComVal > hCOMNum Then hCOMNum = xComVal
                    Continue For

                End If
                'TODO: LNOBJ value validation

                'ElseIf SWIC(sLineTrim,"#LNTYPE") Then
                '    CAdLNTYPE.Checked = True
                '    If Mid(sLineTrim, 9) = "" Or Mid(sLineTrim, 9) = "1" Or Mid(sLineTrim, 9) = "01" Then CAdLNTYPEb.Text = "1"
                '    CAdLNTYPEb.Text = Mid(sLineTrim, 9)
            End If

            If sLineTrim.StartsWith("#") And Mid(sLineTrim, 7, 1) = ":" Then   'If the line contains Ks
                Dim xIdentifier As String = Mid(sLineTrim, 5, 2)
                If BMSChannelToColumn(xIdentifier) = 0 Then xExpansion &= sLine & vbCrLf : Continue For

                nLine += 1
                xStrLine2(nLine) = sLineTrim

            Else
                AddToExpansion(xExpansion, xStack, sLine)
            End If
        Next
        UpdateMeasureBottom()

        ' BPM must be updated before loading notes, do not combine loops
        ' xStrLine2 should contain only # lines for notes
        ReDim Preserve xStrLine2(nLine)
        For Each sLineTrim In xStrLine2

            If Not (sLineTrim.StartsWith("#") And Mid(sLineTrim, 7, 1) = ":") Then Continue For 'If the line contains Ks ' P: The hell is a K

            ' >> Measure =           Mid(sLine, 2, 3)
            ' >> Column Identifier = Mid(sLine, 5, 2)
            ' >> K =                 Mid(sLine, xI1, 2)
            Dim xMeasure As Integer = CInt(Mid(sLineTrim, 2, 3))
            Dim Channel As String = Mid(sLineTrim, 5, 2)
            If BMSChannelToColumn(Channel) = 0 Then Continue For

            If Channel = "01" Then mColumn(xMeasure) += 1 'If the identifier is 01 then add a B column in that measure
            For xI1 = 8 To Len(sLineTrim) - 1 Step 2   'For all Ks within that line ( - 1 can be ommitted )
                If Mid(sLineTrim, xI1, 2) = "00" Then Continue For 'If the K is not 00

                ReDim Preserve Notes(Notes.Length)

                With Notes(UBound(Notes))
                    .ColumnIndex = BMSChannelToColumn(Channel) +
                                        CInt(IIf(Channel = "01", 1, 0)) * (mColumn(xMeasure) - 1)
                    .LongNote = IsChannelLongNote(Channel)
                    .Hidden = IsChannelHidden(Channel)
                    .Landmine = IsChannelLandmine(Channel)
                    .Selected = False
                    .VPosition = MeasureBottom(xMeasure) + MeasureLength(xMeasure) * (xI1 / 2 - 4) / ((Len(sLineTrim) - 7) / 2)
                    .Value = C36to10(Mid(sLineTrim, xI1, 2)) * 10000
                    .Ghost = xGhost
                    .Comment = xComment

                    If Channel = "03" Then .Value = Convert.ToInt32(Mid(sLineTrim, xI1, 2), 16) * 10000
                    If Channel = "08" Then .Value = hBPM(C36to10(Mid(sLineTrim, xI1, 2)))
                    If Channel = "09" Then .Value = hSTOP(C36to10(Mid(sLineTrim, xI1, 2)))
                    If Channel = "SC" Then .Value = hBMSCROLL(C36to10(Mid(sLineTrim, xI1, 2)))
                End With

            Next
        Next

        If xEditorExpansion <> "" Then OpenBMS(xEditorExpansion,, True)

        If xGhost Or xComment Then
            If xGhost AndAlso NTInput Then ConvertBMSE2NT(nNotes)
        Else
            If NTInput Then ConvertBMSE2NT()

            LWAV.Visible = False
            LWAV.Items.Clear()
            For xI1 = 1 To 1295
                LWAV.Items.Add(C10to36(xI1) & ": " & hWAV(xI1))
                ' Add waveforms to wLWAV
                If hWAV(xI1) <> "" AndAlso ShowWaveform Then wLWAV(xI1) = LoadWaveForm(ExcludeFileName(FileName) & "\" & hWAV(xI1))
            Next
            LWAV.SelectedIndex = 0
            LWAV.Visible = True
            If ShowWaveform Then WaveformLoaded = True

            TExpansion.Text = xExpansion
        End If

        SortByVPositionQuick(0, UBound(Notes))
        UpdatePairing()
        CalculateTotalPlayableNotes()
        CalculateGreatestVPosition()
        RefreshPanelAll()
        POStatusRefresh()
    End Sub

    Private Sub AddToExpansion(ByRef xExpansion As String, ByRef xStack As Integer, ByVal sLine As String)
        Dim sLineTrim As String = sLine.Trim
        If SWIC(sLineTrim, "#IF") Or SWIC(sLineTrim, "#SWITCH") Or SWIC(sLineTrim, "#SETSWITCH") Then
            xStack += 1
            xExpansion &= sLine & vbCrLf
        ElseIf SWIC(sLineTrim, "#ENDIF") Or SWIC(sLineTrim, "#ENDSW") Then
            xStack -= 1
            xExpansion &= sLine & vbCrLf
        ElseIf sLineTrim.StartsWith("#") Then
            xExpansion &= sLine & vbCrLf
        End If
    End Sub

    ReadOnly BMSChannelList() As String = {"01", "03", "04", "06", "07", "08", "09",
                                       "11", "12", "13", "14", "15", "16", "18", "19",
                                       "21", "22", "23", "24", "25", "26", "28", "29",
                                       "31", "32", "33", "34", "35", "36", "38", "39",
                                       "41", "42", "43", "44", "45", "46", "48", "49",
                                       "51", "52", "53", "54", "55", "56", "58", "59",
                                       "61", "62", "63", "64", "65", "66", "68", "69",
                                       "D1", "D2", "D3", "D4", "D5", "D6", "D8", "D9",
                                       "E1", "E2", "E3", "E4", "E5", "E6", "E8", "E9",
                                       "SC"}
    ' 71 through 89 are reserved
    '"71", "72", "73", "74", "75", "76", "78", "79",
    '"81", "82", "83", "84", "85", "86", "88", "89",

    Private Function SWIC(str As String, strHash As String) As Boolean ' StartsWith, IgnoreCase
        Return str.StartsWith(strHash, StringComparison.CurrentCultureIgnoreCase)
    End Function

    Private Function SaveBMS(Optional xRandom As Boolean = False) As String
        CalculateGreatestVPosition()
        SortByVPositionInsertion()
        UpdatePairing()
        Dim MeasureIndex As Integer
        Dim hasOverlapping As Boolean = False
        'Dim xStrAll As String = ""   'for all 
        Dim xStrMeasure(MeasureAtDisplacement(GreatestVPosition) + 1) As String

        ' We regenerate these when traversing the bms event list.

        Dim xNTInput As Boolean = NTInput
        If GhostMode = 2 Then SwapGhostNotes() ' Revert main notes back to non-ghost notes
        ' TODO: Fix Ghost mode 1 and 2 not saving BPMs, STOPs and SCROLLs
        Dim xNotesBackup() As Note = CType(Notes.Clone(), Note()) 'All notes

        If xNTInput Then
            NTInput = False
            ConvertNT2BMSE()
        End If

        If Not xRandom Then
            ReDim hBPM(0)
            ReDim hSTOP(0)
            ReDim hBMSCROLL(0)
        End If

        Dim tempNote As Note                    'Temp K
        Dim xprevNotes(-1) As Note              'Notes too close to the next measure

        RemoveGhostNotes() ' Remove Ghost Notes from Notes()
        RemoveCommentNotes() ' Remove Comment Notes from Notes()

        For MeasureIndex = 0 To MeasureAtDisplacement(GreatestVPosition) + 1  'For xI1 in each measure
            xStrMeasure(MeasureIndex) = vbCrLf

            Dim consistentDecimalStr = WriteDecimalWithDot(MeasureLength(MeasureIndex) / 192.0R)

            ' Handle fractional measure
            If MeasureLength(MeasureIndex) <> 192.0R Then xStrMeasure(MeasureIndex) &= "#" & Add3Zeros(MeasureIndex) & "02:" & consistentDecimalStr & vbCrLf

            ' Get note count in current measure
            Dim LowerLimit As Integer = Nothing
            Dim UpperLimit As Integer = Nothing
            GetMeasureLimits(MeasureIndex, LowerLimit, UpperLimit)

            If UpperLimit - LowerLimit = 0 Then Continue For 'If there is no K in the current measure then end this loop

            ' Get notes from this measure
            Dim xUPrevText As Integer = UBound(xprevNotes)
            Dim NotesInMeasure(UpperLimit - LowerLimit + xUPrevText) As Note

            ' Copy notes from previous array
            For i = 0 To xUPrevText
                NotesInMeasure(i) = xprevNotes(i)
            Next

            ' Copy notes in current measure
            For i = LowerLimit To UpperLimit - 1
                NotesInMeasure(i - LowerLimit + xprevNotes.Length) = Notes(i)
            Next

            ' Find greatest column.
            ' Since background tracks have the highest column values
            ' this - niB will yield the number of B columns.
            Dim GreatestColumn = 0
            For Each tempNote In NotesInMeasure
                GreatestColumn = Math.Max(tempNote.ColumnIndex, GreatestColumn)
            Next

            ReDim xprevNotes(-1)
            xStrMeasure(MeasureIndex) &= GenerateBackgroundTracks(MeasureIndex, hasOverlapping, NotesInMeasure, GreatestColumn, xprevNotes)
            xStrMeasure(MeasureIndex) &= GenerateKeyTracks(MeasureIndex, hasOverlapping, NotesInMeasure, xprevNotes)
        Next

        ' Warn about 255 limit if neccesary.
        If hasOverlapping Then MsgBox(Strings.Messages.SaveWarning & vbCrLf &
                                                          Strings.Messages.NoteOverlapError & vbCrLf &
                                                Strings.Messages.SavedFileWillContainErrors, MsgBoxStyle.Exclamation)
        If UBound(hBPM) > CInt(IIf(BPMx1296, 1295, 255)) Then MsgBox(Strings.Messages.SaveWarning & vbCrLf &
                                                          Strings.Messages.BPMOverflowError & UBound(hBPM) & " > " & IIf(BPMx1296, 1295, 255).ToString() & vbCrLf &
                                                Strings.Messages.SavedFileWillContainErrors, MsgBoxStyle.Exclamation)
        If UBound(hSTOP) > CInt(IIf(STOPx1296, 1295, 255)) Then MsgBox(Strings.Messages.SaveWarning & vbCrLf &
                                                           Strings.Messages.STOPOverflowError & UBound(hSTOP) & " > " & IIf(STOPx1296, 1295, 255).ToString() & vbCrLf &
                                                  Strings.Messages.SavedFileWillContainErrors, MsgBoxStyle.Exclamation)
        If UBound(hBMSCROLL) > 1295 Then MsgBox(Strings.Messages.SaveWarning & vbCrLf &
                                           Strings.Messages.SCROLLOverflowError & UBound(hBMSCROLL) & " > " & 1295 & vbCrLf &
                                         Strings.Messages.SavedFileWillContainErrors, MsgBoxStyle.Exclamation)

        ' If xRandom then return bms style random data field, combining expansion text and main data field.
        If xRandom Then
            Return TExpansion.Text & vbCrLf & Join(xStrMeasure, "") & vbCrLf & "*---------------------- RANDOM DATA FIELD"
        End If

        ' Add expansion text
        ' Add and combine ghost notes with existing expansion text
        Dim GhostModeTemp As Integer = -1
        If GhostMode <> 0 Then
            ' Generate String array for duplicate comparison
            GhostModeTemp = GhostMode
            GhostMode = 0
            TExpansion.Text = ""
            Dim xKBackUpG() As Note = CType(xNotesBackup.Clone(), Note())
            Dim xStrCompare() As String = Split(Replace(Replace(Replace(SaveBMS(), vbLf, vbCr), vbCr & vbCr, vbCr), vbCr, vbCrLf), vbCrLf,, CompareMethod.Text)

            ' Save ghost notes
            Notes = CType(xKBackUpG.Clone(), Note())
            If xNTInput Then ConvertNT2BMSE()
            SwapGhostNotes()
            RemoveGhostNotes() ' Remove Main Notes from Notes()
            RemoveCommentNotes() ' Remove Comment Notes from Notes()
            TExpansion.Text = ExtractExpansion(ExpansionSplit(1))
            Dim xStrExpGhostNotes As String = SaveBMS(True)
            GhostMode = GhostModeTemp

            ExpansionSplit(1) = ""
            For Each xStrLine In Split(xStrExpGhostNotes, vbCrLf)
                If (Not xStrCompare.Contains(xStrLine) AndAlso xStrLine <> "*---------------------- RANDOM DATA FIELD") Or
                            SWIC(xStrLine, "#RANDOM") Or SWIC(xStrLine, "#IF") Or SWIC(xStrLine, "#ENDIF") Then
                    ExpansionSplit(1) &= xStrLine & vbCrLf
                End If
            Next
            TExpansion.Text = Join(ExpansionSplit, vbCrLf)
            xNotesBackup = CType(xKBackUpG.Clone(), Note())
        End If
        ' Combine all expansion texts
        Dim xStrExp As String = vbCrLf & "*---------------------- EXPANSION FIELD" & vbCrLf & TExpansion.Text & vbCrLf & vbCrLf
        If TExpansion.Text = "" Then xStrExp = ""

        ' Add comment notes
        Dim xStrEditorCommentNotes As String = ""
        Notes = CType(xNotesBackup.Clone(), Note())
        If xNTInput Then ConvertNT2BMSE()
        ' Swap comment notes. Not a sub/function since expected to use only once.
        For xI1 = 1 To UBound(Notes)
            Notes(xI1).Comment = Not Notes(xI1).Comment
        Next
        RemoveCommentNotes() ' Remove non-comment notes
        If UBound(Notes) > 0 Then
            Dim ExpansionTextTemp = TExpansion.Text
            TExpansion.Text = ""
            If GhostModeTemp <> -1 Then GhostMode = 0
            xStrEditorCommentNotes = SaveBMS(True).Replace("*---------------------- RANDOM DATA FIELD", "").Replace(vbCrLf & vbCrLf, vbCrLf).Replace("#", "#ECMD")
            If GhostModeTemp <> -1 Then GhostMode = GhostModeTemp
            For i = 1 To UBound(hCOM)
                If Not IsNothing(hCOM(i)) Then xStrEditorCommentNotes &= vbCrLf & "#ECOM" & C10to36(i) & " " & hCOM(i)
            Next
            TExpansion.Text = ExpansionTextTemp
        End If
        Dim xStrEditor As String = vbCrLf & "*---------------------- EDITOR EXPANSION FIELD" & vbCrLf & xStrEditorCommentNotes & vbCrLf & vbCrLf
        If xStrEditorCommentNotes = "" Then xStrEditor = ""

        ' Output main data field.
        Dim xStrMain As String = "*---------------------- MAIN DATA FIELD" & vbCrLf & vbCrLf & Join(xStrMeasure, "") & vbCrLf

        ' Restore notes
        Notes = CType(xNotesBackup.Clone(), Note())
        If xNTInput Then
            NTInput = True
        End If

        ' Return ghost notes back to Notes
        If GhostMode = 2 Then SwapGhostNotes()

        ' Generate headers now, since we have the unique BPM/STOP/etc declarations.
        Dim xStrHeader As String = GenerateHeaderMeta()
        xStrHeader &= GenerateHeaderIndexedData()

        Return xStrHeader & vbCrLf & xStrExp & vbCrLf & xStrEditor & vbCrLf & xStrMain
    End Function

    Private Function ExtractExpansion(ByVal xString As String) As String
        xString = Replace(Replace(Replace(xString, vbLf, vbCr), vbCr & vbCr, vbCr), vbCr, vbCrLf)
        Dim xStrLine() As String = Split(xString, vbCrLf, , CompareMethod.Text)
        Dim xExpansion As String = ""
        Dim sLine As String
        Dim xStack As Integer = 0
        For Each sLine In xStrLine
            Dim sLineTrim As String = sLine.Trim
            If xStack > 0 Then
                AddToExpansion(xExpansion, xStack, sLine)
                Continue For

            ElseIf sLineTrim.StartsWith("#") And Mid(sLineTrim, 7, 1) = ":" Then   'If the line contains Ks
                Dim xIdentifier As String = Mid(sLineTrim, 5, 2)
                If BMSChannelToColumn(xIdentifier) = 0 Then xExpansion &= sLine & vbCrLf

            Else
                AddToExpansion(xExpansion, xStack, sLine)

            End If
        Next

        Return xExpansion
    End Function

    Private Function GenerateHeaderMeta() As String
        Dim xStrHeader As String = vbCrLf & "*---------------------- HEADER FIELD" & vbCrLf & vbCrLf
        xStrHeader &= "#PLAYER " & (CHPlayer.SelectedIndex + 1) & vbCrLf
        xStrHeader &= "#GENRE " & THGenre.Text & vbCrLf
        xStrHeader &= "#TITLE " & THTitle.Text & vbCrLf
        xStrHeader &= "#ARTIST " & THArtist.Text & vbCrLf
        xStrHeader &= "#BPM " & WriteDecimalWithDot(Notes(0).Value / 10000) & vbCrLf
        xStrHeader &= "#PLAYLEVEL " & THPlayLevel.Text & vbCrLf
        xStrHeader &= "#RANK " & CHRank.SelectedIndex & vbCrLf
        xStrHeader &= vbCrLf
        If THSubTitle.Text <> "" Then xStrHeader &= "#SUBTITLE " & THSubTitle.Text & vbCrLf
        If THSubArtist.Text <> "" Then xStrHeader &= "#SUBARTIST " & THSubArtist.Text & vbCrLf
        If THStageFile.Text <> "" Then xStrHeader &= "#STAGEFILE " & THStageFile.Text & vbCrLf
        If THBanner.Text <> "" Then xStrHeader &= "#BANNER " & THBanner.Text & vbCrLf
        If THBackBMP.Text <> "" Then xStrHeader &= "#BACKBMP " & THBackBMP.Text & vbCrLf
        xStrHeader &= vbCrLf
        If CHDifficulty.SelectedIndex > 0 Then xStrHeader &= "#DIFFICULTY " & CHDifficulty.SelectedIndex & vbCrLf
        If THExRank.Text <> "" Then xStrHeader &= "#DEFEXRANK " & THExRank.Text & vbCrLf
        If THTotal.Text <> "" Then xStrHeader &= "#TOTAL " & THTotal.Text & vbCrLf
        If THComment.Text <> "" Then xStrHeader &= "#COMMENT """ & THComment.Text & """" & vbCrLf
        'If THLnType.Text <> "" Then xStrHeader &= "#LNTYPE " & THLnType.Text & vbCrLf
        If CHLnObj.SelectedIndex > 0 Then xStrHeader &= "#LNOBJ " & C10to36(CHLnObj.SelectedIndex) & vbCrLf _
                                     Else xStrHeader &= "#LNTYPE 1" & vbCrLf
        xStrHeader &= vbCrLf
        Return xStrHeader
    End Function

    Private Function GenerateHeaderIndexedData() As String
        Dim xStrHeader As String = ""

        For i = 1 To UBound(hWAV)
            If Not hWAV(i) = "" Then xStrHeader &= "#WAV" & C10to36(i) &
                                                    " " & hWAV(i) & vbCrLf
        Next
        For i = 1 To UBound(hBPM)
            xStrHeader &= "#BPM" &
                IIf(BPMx1296, C10to36(i), Mid("0" & Hex(i), Len(Hex(i)))).ToString() &
                " " & WriteDecimalWithDot(hBPM(i) / 10000) & vbCrLf
        Next
        For i = 1 To UBound(hSTOP)
            xStrHeader &= "#STOP" &
                IIf(STOPx1296, C10to36(i), Mid("0" & Hex(i), Len(Hex(i)))).ToString() &
                " " & WriteDecimalWithDot(hSTOP(i) / 10000) & vbCrLf
        Next
        For i = 1 To UBound(hBMSCROLL)
            xStrHeader &= "#SCROLL" &
                C10to36(i) & " " & WriteDecimalWithDot(hBMSCROLL(i) / 10000) & vbCrLf
        Next

        Return xStrHeader
    End Function

    Private Sub GetMeasureLimits(MeasureIndex As Integer, ByRef LowerLimit As Integer, ByRef UpperLimit As Integer)
        Dim NoteCount = UBound(Notes)
        LowerLimit = 0

        For i = 1 To NoteCount  'Collect Ks in the same measure
            If MeasureAtDisplacement(Notes(i).VPosition) >= MeasureIndex Then
                LowerLimit = i
                Exit For
            End If 'Lower limit found
        Next

        UpperLimit = 0

        For i = LowerLimit To NoteCount
            If MeasureAtDisplacement(Notes(i).VPosition) > MeasureIndex Then
                UpperLimit = i
                Exit For 'Upper limit found
            End If
        Next

        If UpperLimit < LowerLimit Then UpperLimit = NoteCount + 1
    End Sub

    Private Function GenerateKeyTracks(MeasureIndex As Integer, ByRef hasOverlapping As Boolean, NotesInMeasure() As Note, ByRef xprevNotes() As Note) As String
        Dim CurrentBMSChannel As String
        Dim Ret As String = ""

        For Each CurrentBMSChannel In BMSChannelList 'Start rendering other notes
            Dim relativeMeasurePos(-1) As Double 'Ks in the same column
            Dim NoteStrings(-1) As String        'Ks in the same column

            ' Background tracks take care of this.
            If CurrentBMSChannel = "01" Then Continue For


            For NoteIndex = 0 To UBound(NotesInMeasure) 'Find Ks in the same column (xI4 is TK index)

                Dim currentNote As Note = NotesInMeasure(NoteIndex)
                If GetBMSChannelBy(currentNote) = CurrentBMSChannel Then

                    ReDim Preserve relativeMeasurePos(UBound(relativeMeasurePos) + 1)
                    ReDim Preserve NoteStrings(UBound(NoteStrings) + 1)
                    relativeMeasurePos(UBound(relativeMeasurePos)) = currentNote.VPosition - MeasureBottom(MeasureAtDisplacement(currentNote.VPosition))
                    If relativeMeasurePos(UBound(relativeMeasurePos)) < 0 Then relativeMeasurePos(UBound(relativeMeasurePos)) = 0

                    If CurrentBMSChannel = "03" Then 'If integer bpm
                        NoteStrings(UBound(NoteStrings)) = Mid("0" & Hex(currentNote.Value \ 10000), Len(Hex(currentNote.Value \ 10000)))
                    ElseIf CurrentBMSChannel = "08" Then 'If bpm requires declaration
                        Dim BpmIndex As Integer
                        For BpmIndex = 1 To UBound(hBPM) ' find BPM value in existing array
                            If currentNote.Value = hBPM(BpmIndex) Then Exit For
                        Next
                        If BpmIndex > UBound(hBPM) Then ' Didn't find it, add it
                            ReDim Preserve hBPM(UBound(hBPM) + 1)
                            hBPM(UBound(hBPM)) = currentNote.Value
                        End If
                        NoteStrings(UBound(NoteStrings)) = IIf(BPMx1296, C10to36(BpmIndex), Mid("0" & Hex(BpmIndex), Len(Hex(BpmIndex)))).ToString()
                    ElseIf CurrentBMSChannel = "09" Then 'If STOP
                        Dim StopIndex As Integer
                        For StopIndex = 1 To UBound(hSTOP) ' find STOP value in existing array
                            If currentNote.Value = hSTOP(StopIndex) Then Exit For
                        Next

                        If StopIndex > UBound(hSTOP) Then ' Didn't find it, add it
                            ReDim Preserve hSTOP(UBound(hSTOP) + 1)
                            hSTOP(UBound(hSTOP)) = currentNote.Value
                        End If
                        NoteStrings(UBound(NoteStrings)) = IIf(STOPx1296, C10to36(StopIndex), Mid("0" & Hex(StopIndex), Len(Hex(StopIndex)))).ToString()
                    ElseIf CurrentBMSChannel = "SC" Then 'If SCROLL
                        Dim ScrollIndex As Integer
                        For ScrollIndex = 1 To UBound(hBMSCROLL) ' find SCROLL value in existing array
                            If currentNote.Value = hBMSCROLL(ScrollIndex) Then Exit For
                        Next

                        If ScrollIndex > UBound(hBMSCROLL) Then ' Didn't find it, add it
                            ReDim Preserve hBMSCROLL(UBound(hBMSCROLL) + 1)
                            hBMSCROLL(UBound(hBMSCROLL)) = currentNote.Value
                        End If
                        NoteStrings(UBound(NoteStrings)) = C10to36(ScrollIndex)
                    Else
                        NoteStrings(UBound(NoteStrings)) = C10to36(currentNote.Value \ 10000)
                    End If
                End If
            Next

            If relativeMeasurePos.Length = 0 Then Continue For

            Dim xGCD As Double = MeasureLength(MeasureIndex)
            For i = 0 To UBound(relativeMeasurePos)        'find greatest common divisor
                If relativeMeasurePos(i) > 0 Then xGCD = GCD(xGCD, relativeMeasurePos(i))
            Next

            Dim xStrKey() As String
            ReDim xStrKey(CInt(MeasureLength(MeasureIndex) / xGCD) - 1)
            For i = 0 To UBound(xStrKey)           'assign 00 to all keys
                xStrKey(i) = "00"
            Next

            For i = 0 To UBound(relativeMeasurePos)        'assign K texts
                Dim CBMSCI As Integer = CInt(CurrentBMSChannel)
                If CInt(relativeMeasurePos(i) / xGCD) > UBound(xStrKey) Then
                    ReDim Preserve xprevNotes(UBound(xprevNotes) + 1)
                    With xprevNotes(UBound(xprevNotes))
                        .ColumnIndex = BMSChannelToColumn(BMSChannelList(CBMSCI))
                        .LongNote = IsChannelLongNote(BMSChannelList(CBMSCI))
                        .Hidden = IsChannelHidden(BMSChannelList(CBMSCI))
                        .VPosition = MeasureBottom(MeasureIndex)
                        .Value = C36to10(NoteStrings(i))
                    End With
                    If BMSChannelList(CBMSCI) = "08" Then _
                        xprevNotes(UBound(xprevNotes)).Value = CLng(IIf(BPMx1296, hBPM(C36to10(NoteStrings(i))), hBPM(Convert.ToInt32(NoteStrings(i), 16))))
                    If BMSChannelList(CBMSCI) = "09" Then _
                        xprevNotes(UBound(xprevNotes)).Value = CLng(IIf(STOPx1296, hSTOP(C36to10(NoteStrings(i))), hSTOP(Convert.ToInt32(NoteStrings(i), 16))))
                    If BMSChannelList(CBMSCI) = "SC" Then _
                        xprevNotes(UBound(xprevNotes)).Value = hBMSCROLL(C36to10(NoteStrings(i)))
                    Continue For
                End If
                If xStrKey(CInt(relativeMeasurePos(i) / xGCD)) <> "00" Then
                    hasOverlapping = True
                End If

                xStrKey(CInt(relativeMeasurePos(i) / xGCD)) = NoteStrings(i)
            Next

            Ret &= "#" & Add3Zeros(MeasureIndex) & CurrentBMSChannel & ":" & Join(xStrKey, "") & vbCrLf
        Next

        Return Ret
    End Function

    Private Function GenerateBackgroundTracks(MeasureIndex As Integer, ByRef hasOverlapping As Boolean, NotesInMeasure() As Note, GreatestColumn As Integer, ByRef xprevNotes() As Note) As String
        Dim relativeNotePositions() As Double 'Ks in the same column
        Dim noteStrings() As String    'Ks in the same column
        Dim Ret As String = ""

        For ColIndex = niB To GreatestColumn 'Start rendering B notes (xI3 is columnindex)
            ReDim relativeNotePositions(-1) 'Ks in the same column
            ReDim noteStrings(-1)      'Ks in the same column

            For I = 0 To UBound(NotesInMeasure) 'Find Ks in the same column (xI4 is TK index)
                If NotesInMeasure(I).ColumnIndex = ColIndex Then

                    ReDim Preserve relativeNotePositions(UBound(relativeNotePositions) + 1)
                    ReDim Preserve noteStrings(UBound(noteStrings) + 1)

                    relativeNotePositions(UBound(relativeNotePositions)) = NotesInMeasure(I).VPosition - MeasureBottom(MeasureAtDisplacement(NotesInMeasure(I).VPosition))
                    If relativeNotePositions(UBound(relativeNotePositions)) < 0 Then relativeNotePositions(UBound(relativeNotePositions)) = 0

                    noteStrings(UBound(noteStrings)) = C10to36(NotesInMeasure(I).Value \ 10000)
                End If
            Next

            Dim xGCD As Double = MeasureLength(MeasureIndex)
            For i = 0 To UBound(relativeNotePositions)        'find greatest common divisor
                If relativeNotePositions(i) > 0 Then xGCD = GCD(xGCD, relativeNotePositions(i))
            Next

            Dim xStrKey(CInt(MeasureLength(MeasureIndex) / xGCD) - 1) As String
            For i = 0 To UBound(xStrKey)           'assign 00 to all keys
                xStrKey(i) = "00"
            Next

            For i = 0 To UBound(relativeNotePositions)        'assign K texts
                If CInt(relativeNotePositions(i) / xGCD) > UBound(xStrKey) Then

                    ReDim Preserve xprevNotes(UBound(xprevNotes) + 1)

                    With xprevNotes(UBound(xprevNotes))
                        .ColumnIndex = ColIndex
                        .VPosition = MeasureBottom(MeasureIndex)
                        .Value = C36to10(noteStrings(i))
                    End With

                    Continue For
                End If
                If xStrKey(CInt(relativeNotePositions(i) / xGCD)) <> "00" Then hasOverlapping = True
                xStrKey(CInt(relativeNotePositions(i) / xGCD)) = noteStrings(i)
            Next

            Ret &= "#" & Add3Zeros(MeasureIndex) & "01:" & Join(xStrKey, "") & vbCrLf
        Next

        Return Ret
    End Function

    Private Function OpenSM(ByVal xStrAll As String) As Boolean
        KMouseOver = -1

        Dim xStrLine() As String = Split(xStrAll, vbCrLf)
        'Remove comments starting with "//"
        For xI1 As Integer = 0 To UBound(xStrLine)
            If xStrLine(xI1).Contains("//") Then xStrLine(xI1) = Mid(xStrLine(xI1), 1, InStr(xStrLine(xI1), "//") - 1)
        Next

        xStrAll = Join(xStrLine, "")
        xStrLine = Split(xStrAll, ";")

        Dim iDiff As Integer = 0
        Dim iCurrentDiff As Integer = 0
        Dim xTempSplit() As String = Split(xStrAll, "#NOTES:")
        Dim xTempStr() As String = {}
        If xTempSplit.Length > 2 Then
            ReDim Preserve xTempStr(UBound(xTempSplit) - 1)
            For xI1 As Integer = 1 To UBound(xTempSplit)
                xTempSplit(xI1) = Mid(xTempSplit(xI1), InStr(xTempSplit(xI1), ":") + 1)
                xTempSplit(xI1) = Mid(xTempSplit(xI1), InStr(xTempSplit(xI1), ":") + 1).Trim
                xTempStr(xI1 - 1) = Mid(xTempSplit(xI1), 1, InStr(xTempSplit(xI1), ":") - 1)
                xTempSplit(xI1) = Mid(xTempSplit(xI1), InStr(xTempSplit(xI1), ":") + 1).Trim
                xTempStr(xI1 - 1) &= " : " & Mid(xTempSplit(xI1), 1, InStr(xTempSplit(xI1), ":") - 1)
            Next

            Dim xDiag As New dgImportSM(xTempStr)
            If xDiag.ShowDialog() = Windows.Forms.DialogResult.Cancel Then Return True
            iDiff = xDiag.iResult
        End If

        Dim sL As String
        ReDim Notes(0)
        ReDim mColumn(999)
        ReDim hWAV(1295)
        ReDim hBPM(1295)    'x10000
        ReDim hSTOP(1295)
        ReDim hBMSCROLL(1295)
        Me.InitializeNewBMS()

        With Notes(0)
            .ColumnIndex = niBPM
            .VPosition = -1
            '.LongNote = False
            '.Selected = False
            .Value = 1200000
        End With

        For Each sL In xStrLine
            If UCase(sL).StartsWith("#TITLE:") Then
                THTitle.Text = Mid(sL, Len("#TITLE:") + 1)

            ElseIf UCase(sL).StartsWith("#SUBTITLE:") Then
                If Not UCase(sL).EndsWith("#SUBTITLE:") Then THTitle.Text &= " " & Mid(sL, Len("#SUBTITLE:") + 1)

            ElseIf UCase(sL).StartsWith("#ARTIST:") Then
                THArtist.Text = Mid(sL, Len("#ARTIST:") + 1)

            ElseIf UCase(sL).StartsWith("#GENRE:") Then
                THGenre.Text = Mid(sL, Len("#GENRE:") + 1)

            ElseIf UCase(sL).StartsWith("#BPMS:") Then
                Dim xLine As String = Mid(sL, Len("#BPMS:") + 1)
                Dim xItem() As String = Split(xLine, ",")

                Dim xVal1 As Double
                Dim xVal2 As Long

                For xI1 As Integer = 0 To UBound(xItem)
                    xVal1 = CDbl(Mid(xItem(xI1), 1, InStr(xItem(xI1), "=") - 1))
                    xVal2 = CLng(Mid(xItem(xI1), InStr(xItem(xI1), "=") + 1))

                    If xVal1 <> 0 Then
                        ReDim Preserve Notes(Notes.Length)
                        With Notes(UBound(Notes))
                            .ColumnIndex = niBPM
                            '.LongNote = False
                            '.Hidden = False
                            '.Selected = False
                            .VPosition = xVal1 * 48
                            .Value = xVal2 * 10000
                        End With
                    Else
                        Notes(0).Value = xVal2 * 10000
                    End If
                Next

            ElseIf UCase(sL).StartsWith("#NOTES:") Then
                If iCurrentDiff <> iDiff Then iCurrentDiff += 1 : Continue For

                iCurrentDiff += 1
                Dim xLine As String = Mid(sL, Len("#NOTES:") + 1)
                Dim xItem() As String = Split(xLine, ":")
                For xI1 As Integer = 0 To UBound(xItem)
                    xItem(xI1) = xItem(xI1).Trim
                Next

                If xItem.Length <> 6 Then Continue For

                THPlayLevel.Text = xItem(3)

                Dim xM() As String = Split(xItem(5), ",")
                For xI1 As Integer = 0 To UBound(xM)
                    xM(xI1) = xM(xI1).Trim
                Next

                For xI1 As Integer = 0 To UBound(xM)
                    For xI2 As Integer = 0 To Len(xM(xI1)) - 1 Step 4
                        If xM(xI1)(xI2) <> "0" Then
                            ReDim Preserve Notes(Notes.Length)
                            With Notes(UBound(Notes))
                                .ColumnIndex = niA1
                                .LongNote = xM(xI1)(xI2) = "2" Or xM(xI1)(xI2) = "3"
                                '.Hidden = False
                                '.Selected = False
                                .VPosition = (192 \ (Len(xM(xI1)) \ 4)) * xI2 \ 4 + xI1 * 192
                                .Value = 10000
                            End With
                        End If
                        If xM(xI1)(xI2 + 1) <> "0" Then
                            ReDim Preserve Notes(Notes.Length)
                            With Notes(UBound(Notes))
                                .ColumnIndex = niA2
                                .LongNote = xM(xI1)(xI2 + 1) = "2" Or xM(xI1)(xI2 + 1) = "3"
                                '.Hidden = False
                                '.Selected = False
                                .VPosition = (192 \ (Len(xM(xI1)) \ 4)) * xI2 \ 4 + xI1 * 192
                                .Value = 10000
                            End With
                        End If
                        If xM(xI1)(xI2 + 2) <> "0" Then
                            ReDim Preserve Notes(Notes.Length)
                            With Notes(UBound(Notes))
                                .ColumnIndex = niA3
                                .LongNote = xM(xI1)(xI2 + 2) = "2" Or xM(xI1)(xI2 + 2) = "3"
                                '.Hidden = False
                                '.Selected = False
                                .VPosition = (192 \ (Len(xM(xI1)) \ 4)) * xI2 \ 4 + xI1 * 192
                                .Value = 10000
                            End With
                        End If
                        If xM(xI1)(xI2 + 3) <> "0" Then
                            ReDim Preserve Notes(Notes.Length)
                            With Notes(UBound(Notes))
                                .ColumnIndex = niA4
                                .LongNote = xM(xI1)(xI2 + 3) = "2" Or xM(xI1)(xI2 + 3) = "3"
                                '.Hidden = False
                                '.Selected = False
                                .VPosition = (192 \ (Len(xM(xI1)) \ 4)) * xI2 \ 4 + xI1 * 192
                                .Value = 10000
                            End With
                        End If
                    Next
                Next
            End If
        Next

        If NTInput Then ConvertBMSE2NT()

        LWAV.Visible = False
        LWAV.Items.Clear()
        For xI1 As Integer = 1 To 1295
            LWAV.Items.Add(C10to36(xI1) & ": " & hWAV(xI1))
        Next
        LWAV.SelectedIndex = 0
        LWAV.Visible = True

        THBPM.Value = CDec(Notes(0).Value / 10000)
        SortByVPositionQuick(0, UBound(Notes))
        UpdatePairing()
        CalculateTotalPlayableNotes()
        CalculateGreatestVPosition()
        RefreshPanelAll()
        POStatusRefresh()
        Return False
    End Function

    ''' <summary>Do not clear Undo.</summary>
    Private Sub OpeniBMSC(ByVal Path As String)
        KMouseOver = -1

        Dim br As New BinaryReader(New FileStream(Path, FileMode.Open, FileAccess.Read), System.Text.Encoding.Unicode)

        If br.ReadInt32 = &H534D4269 Then
            If br.ReadByte = CByte(&H43) Then
                Dim xMajor As Integer = br.ReadByte
                Dim xMinor As Integer = br.ReadByte
                Dim xBuild As Integer = br.ReadByte

                ClearUndo()
                ReDim Notes(0)
                ReDim mColumn(999)
                ReDim hWAV(1295)
                Me.InitializeNewBMS()
                Me.InitializeOpenBMS()

                With Notes(0)
                    .ColumnIndex = niBPM
                    .VPosition = -1
                    '.LongNote = False
                    '.Selected = False
                    .Value = 1200000
                End With

                Do Until br.BaseStream.Position >= br.BaseStream.Length
                    Dim BlockID As Integer = br.ReadInt32()

                    Select Case BlockID

                        Case &H66657250     'Preferences
                            Dim xPref As Integer = br.ReadInt32

                            NTInput = CBool(xPref And &H1)
                            TBNTInput.Checked = NTInput
                            mnNTInput.Checked = NTInput
                            POBLong.Enabled = Not NTInput
                            POBLongShort.Enabled = Not NTInput

                            ErrorCheck = CBool(xPref And &H2)
                            TBErrorCheck.Checked = ErrorCheck
                            TBErrorCheck_Click(TBErrorCheck, New System.EventArgs)

                            PreviewOnClick = CBool(xPref And &H4)
                            TBPreviewOnClick.Checked = PreviewOnClick
                            TBPreviewOnClick_Click(TBPreviewOnClick, New System.EventArgs)

                            ShowFileName = CBool(xPref And &H8)
                            TBShowFileName.Checked = ShowFileName
                            TBShowFileName_Click(TBShowFileName, New System.EventArgs)

                            mnSMenu.Checked = CBool(xPref And &H100)
                            mnSTB.Checked = CBool(xPref And &H200)
                            mnSOP.Checked = CBool(xPref And &H400)
                            mnSStatus.Checked = CBool(xPref And &H800)
                            mnSLSplitter.Checked = CBool(xPref And &H1000)
                            mnSRSplitter.Checked = CBool(xPref And &H2000)

                            CGShow.Checked = CBool(xPref And &H4000)
                            CGShowS.Checked = CBool(xPref And &H8000)
                            CGShowBG.Checked = CBool(xPref And &H10000)
                            CGShowM.Checked = CBool(xPref And &H20000)
                            CGShowMB.Checked = CBool(xPref And &H40000)
                            CGShowV.Checked = CBool(xPref And &H80000)
                            CGShowC.Checked = CBool(xPref And &H100000)
                            CGBLP.Checked = CBool(xPref And &H200000)
                            CGSTOP.Checked = CBool(xPref And &H400000)
                            CGSCROLL.Checked = CBool(xPref And &H20000000)
                            CGBPM.Checked = CBool(xPref And &H800000)

                            CGSnap.Checked = CBool(xPref And &H1000000)
                            CGDisableVertical.Checked = CBool(xPref And &H2000000)
                            cVSLockL.Checked = CBool(xPref And &H4000000)
                            cVSLock.Checked = CBool(xPref And &H8000000)
                            cVSLockR.Checked = CBool(xPref And &H10000000)

                            CGDivide.Value = br.ReadInt32
                            CGSub.Value = br.ReadInt32
                            gSlash = br.ReadInt32
                            CGHeight.Value = CDec(br.ReadSingle)
                            CGWidth.Value = CDec(br.ReadSingle)
                            CGB.Value = br.ReadInt32

                        Case &H64616548     'Header
                            THTitle.Text = br.ReadString
                            THArtist.Text = br.ReadString
                            THGenre.Text = br.ReadString
                            Notes(0).Value = br.ReadInt64
                            Dim xPlayerRank As Integer = br.ReadByte
                            THPlayLevel.Text = br.ReadString

                            CHPlayer.SelectedIndex = xPlayerRank And &HF
                            CHRank.SelectedIndex = xPlayerRank >> 4

                            THSubTitle.Text = br.ReadString
                            THSubArtist.Text = br.ReadString
                            'THMaker.Text = br.ReadString
                            THStageFile.Text = br.ReadString
                            THBanner.Text = br.ReadString
                            THBackBMP.Text = br.ReadString
                            'THMidiFile.Text = br.ReadString
                            CHDifficulty.SelectedIndex = br.ReadByte
                            THExRank.Text = br.ReadString
                            THTotal.Text = br.ReadString
                            'THVolWAV.Text = br.ReadString
                            THComment.Text = br.ReadString
                            'THLnType.Text = br.ReadString
                            CHLnObj.SelectedIndex = br.ReadInt16

                        Case &H564157       'WAV List
                            Dim xWAVOptions As Integer = br.ReadByte
                            WAVMultiSelect = CBool(xWAVOptions And &H1)
                            CWAVMultiSelect.Checked = WAVMultiSelect
                            CWAVMultiSelect_CheckedChanged(CWAVMultiSelect, New EventArgs)
                            WAVChangeLabel = CBool(xWAVOptions And &H2)
                            CWAVChangeLabel.Checked = WAVChangeLabel
                            CWAVChangeLabel_CheckedChanged(CWAVChangeLabel, New EventArgs)

                            Dim xWAVCount As Integer = br.ReadInt32
                            For xxi As Integer = 1 To xWAVCount
                                Dim xI As Integer = br.ReadInt16
                                hWAV(xI) = br.ReadString
                            Next

                        Case &H74616542     'Beat
                            nBeatN.Value = br.ReadInt16
                            nBeatD.Value = br.ReadInt16
                            'nBeatD.SelectedIndex = br.ReadByte

                            Dim xBeatChangeMode As Integer = br.ReadByte
                            Dim xBeatChangeList As RadioButton() = {CBeatPreserve, CBeatMeasure, CBeatCut, CBeatScale}
                            xBeatChangeList(xBeatChangeMode).Checked = True
                            CBeatPreserve_Click(xBeatChangeList(xBeatChangeMode), New System.EventArgs)

                            Dim xBeatCount As Integer = br.ReadInt32
                            For xxi As Integer = 1 To xBeatCount
                                Dim xIndex As Integer = br.ReadInt16
                                MeasureLength(xIndex) = br.ReadDouble
                                Dim xRatio As Double = MeasureLength(xIndex) / 192.0R
                                Dim xxD As Long = GetDenominator(xRatio)
                                LBeat.Items(xIndex) = Add3Zeros(xIndex) & ": " & xRatio & IIf(xxD > 10000, "", " ( " & CLng(xRatio * xxD) & " / " & xxD & " ) ").ToString()
                            Next

                        Case &H6E707845     'Expansion Code
                            TExpansion.Text = br.ReadString

                        Case &H65746F4E     'Note
                            Dim xNoteUbound As Integer = br.ReadInt32
                            ReDim Preserve Notes(xNoteUbound)
                            For i As Integer = 1 To UBound(Notes)
                                Notes(i).FromBinReader(br)
                            Next

                        Case &H6F646E55     'Undo / Redo Commands
                            Dim URCount As Integer = br.ReadInt32   'Should be 100
                            sI = br.ReadInt32

                            For xI As Integer = 0 To 99
                                Dim xUndoCount As Integer = br.ReadInt32
                                Dim xBaseUndo As New UndoRedo.Void
                                Dim xIteratorUndo As UndoRedo.LinkedURCmd = xBaseUndo

                                For xxj As Integer = 1 To xUndoCount
                                    Dim xByteLen As Integer = br.ReadInt32
                                    Dim xByte() As Byte = br.ReadBytes(xByteLen)
                                    xIteratorUndo.Next = UndoRedo.fromBytes(xByte)
                                    xIteratorUndo = xIteratorUndo.Next
                                Next

                                sUndo(xI) = xBaseUndo.Next

                                Dim xRedoCount As Integer = br.ReadInt32
                                Dim xBaseRedo As New UndoRedo.Void
                                Dim xIteratorRedo As UndoRedo.LinkedURCmd = xBaseRedo
                                For xxj As Integer = 1 To xRedoCount
                                    Dim xByteLen As Integer = br.ReadInt32
                                    Dim xByte() As Byte = br.ReadBytes(xByteLen)
                                    xIteratorRedo.Next = UndoRedo.fromBytes(xByte)
                                    xIteratorRedo = xIteratorRedo.Next
                                Next
                                sRedo(xI) = xBaseRedo.Next
                            Next

                    End Select
                Loop

            End If
        End If
        br.Close()

        TBUndo.Enabled = sUndo(sI).ofType <> UndoRedo.opNoOperation
        TBRedo.Enabled = sRedo(sIA).ofType <> UndoRedo.opNoOperation
        mnUndo.Enabled = sUndo(sI).ofType <> UndoRedo.opNoOperation
        mnRedo.Enabled = sRedo(sIA).ofType <> UndoRedo.opNoOperation

        LWAV.Visible = False
        LWAV.Items.Clear()
        For xI1 As Integer = 1 To 1295
            LWAV.Items.Add(C10to36(xI1) & ": " & hWAV(xI1))
        Next
        LWAV.SelectedIndex = 0
        LWAV.Visible = True

        THBPM.Value = CDec(Notes(0).Value / 10000)
        SortByVPositionQuick(0, UBound(Notes))
        UpdatePairing()
        UpdateMeasureBottom()
        CalculateTotalPlayableNotes()
        CalculateGreatestVPosition()
        RefreshPanelAll()
        POStatusRefresh()
    End Sub

    Private Sub SaveiBMSC(ByVal Path As String)
        CalculateGreatestVPosition()
        SortByVPositionInsertion()
        UpdatePairing()

        Try

            Dim bw As New BinaryWriter(New IO.FileStream(Path, FileMode.Create), System.Text.Encoding.Unicode)

            'bw.Write("iBMSC".ToCharArray)
            bw.Write(&H534D4269)
            bw.Write(CByte(&H43))
            bw.Write(CByte(My.Application.Info.Version.Major))
            bw.Write(CByte(My.Application.Info.Version.Minor))
            bw.Write(CByte(My.Application.Info.Version.Build))

            'Preferences
            'bw.Write("Pref".ToCharArray)
            bw.Write(&H66657250)
            Dim xPref As Integer = 0
            If NTInput Then xPref = xPref Or &H1
            If ErrorCheck Then xPref = xPref Or &H2
            If PreviewOnClick Then xPref = xPref Or &H4
            If ShowFileName Then xPref = xPref Or &H8
            If mnSMenu.Checked Then xPref = xPref Or &H100
            If mnSTB.Checked Then xPref = xPref Or &H200
            If mnSOP.Checked Then xPref = xPref Or &H400
            If mnSStatus.Checked Then xPref = xPref Or &H800
            If mnSLSplitter.Checked Then xPref = xPref Or &H1000
            If mnSRSplitter.Checked Then xPref = xPref Or &H2000
            If gShowGrid Then xPref = xPref Or &H4000
            If gShowSubGrid Then xPref = xPref Or &H8000
            If gShowBG Then xPref = xPref Or &H10000
            If gShowMeasureNumber Then xPref = xPref Or &H20000
            If gShowMeasureBar Then xPref = xPref Or &H40000
            If gShowVerticalLine Then xPref = xPref Or &H80000
            If gShowC Then xPref = xPref Or &H100000
            If gDisplayBGAColumn Then xPref = xPref Or &H200000
            If gSTOP Then xPref = xPref Or &H400000
            If gBPM Then xPref = xPref Or &H800000
            If gSCROLL Then xPref = xPref Or &H20000000
            If gSnap Then xPref = xPref Or &H1000000
            If DisableVerticalMove Then xPref = xPref Or &H2000000
            If spLock(0) Then xPref = xPref Or &H4000000
            If spLock(1) Then xPref = xPref Or &H8000000
            If spLock(2) Then xPref = xPref Or &H10000000
            bw.Write(xPref)
            bw.Write(BitConverter.GetBytes(gDivide))
            bw.Write(BitConverter.GetBytes(gSub))
            bw.Write(BitConverter.GetBytes(gSlash))
            bw.Write(BitConverter.GetBytes(gxHeight))
            bw.Write(BitConverter.GetBytes(gxWidth))
            bw.Write(BitConverter.GetBytes(gColumns))

            'Header
            'bw.Write("Head".ToCharArray)
            bw.Write(&H64616548)
            bw.Write(THTitle.Text)
            bw.Write(THArtist.Text)
            bw.Write(THGenre.Text)
            bw.Write(Notes(0).Value)
            Dim xPlayer As Integer = CHPlayer.SelectedIndex
            Dim xRank As Integer = CHRank.SelectedIndex << 4
            bw.Write(CByte(xPlayer Or xRank))
            bw.Write(THPlayLevel.Text)

            bw.Write(THSubTitle.Text)
            bw.Write(THSubArtist.Text)
            'bw.Write(THMaker.Text)
            bw.Write(THStageFile.Text)
            bw.Write(THBanner.Text)
            bw.Write(THBackBMP.Text)
            'bw.Write(THMidiFile.Text)
            bw.Write(CByte(CHDifficulty.SelectedIndex))
            bw.Write(THExRank.Text)
            bw.Write(THTotal.Text)
            'bw.Write(THVolWAV.Text)
            bw.Write(THComment.Text)
            'bw.Write(THLnType.Text)
            bw.Write(CShort(CHLnObj.SelectedIndex))

            'Wav List
            'bw.Write(("WAV" & vbNullChar).ToCharArray)
            bw.Write(&H564157)

            Dim xWAVOptions As Integer = 0
            If WAVMultiSelect Then xWAVOptions = xWAVOptions Or &H1
            If WAVChangeLabel Then xWAVOptions = xWAVOptions Or &H2
            bw.Write(CByte(xWAVOptions))

            Dim xWAVCount As Integer = 0
            For i As Integer = 1 To UBound(hWAV)
                If hWAV(i) <> "" Then xWAVCount += 1
            Next
            bw.Write(xWAVCount)

            For i As Integer = 1 To UBound(hWAV)
                If hWAV(i) = "" Then Continue For
                bw.Write(CShort(i))
                bw.Write(hWAV(i))
            Next

            'Beat
            'bw.Write("Beat".ToCharArray)
            bw.Write(&H74616542)
            'Dim xNumerator As Short = nBeatN.Value
            'Dim xDenominator As Short = nBeatD.Value
            'Dim xBeatChangeMode As Byte = BeatChangeMode
            bw.Write(CShort(nBeatN.Value))
            bw.Write(CShort(nBeatD.Value))
            bw.Write(CByte(BeatChangeMode))

            Dim xBeatCount As Integer = 0
            For i As Integer = 0 To UBound(MeasureLength)
                If MeasureLength(i) <> 192.0R Then xBeatCount += 1
            Next
            bw.Write(xBeatCount)

            For i As Integer = 0 To UBound(MeasureLength)
                If MeasureLength(i) = 192.0R Then Continue For
                bw.Write(CShort(i))
                bw.Write(MeasureLength(i))
            Next

            'Expansion Code
            'bw.Write("Expn".ToCharArray)
            bw.Write(&H6E707845)
            bw.Write(TExpansion.Text)

            'Note
            'bw.Write("Note".ToCharArray)
            bw.Write(&H65746F4E)
            bw.Write(UBound(Notes))
            For i As Integer = 1 To UBound(Notes)
                Notes(i).WriteBinWriter(bw)
            Next

            'Undo / Redo Commands
            'bw.Write("Undo".ToCharArray)
            bw.Write(&H6F646E55)
            bw.Write(100)
            bw.Write(sI)

            For i As Integer = 0 To 99
                'UndoCommandsCount
                Dim countUndo As Integer = 0
                Dim pUndo As UndoRedo.LinkedURCmd = sUndo(i)
                While pUndo IsNot Nothing
                    countUndo += 1
                    pUndo = pUndo.Next
                End While
                bw.Write(countUndo)

                'UndoCommands
                pUndo = sUndo(i)
                For xxi As Integer = 1 To countUndo
                    Dim bUndo() As Byte = pUndo.toBytes
                    bw.Write(bUndo.Length)  'Length
                    bw.Write(bUndo)         'Command
                    pUndo = pUndo.Next
                Next

                'RedoCommandsCount
                Dim countRedo As Integer = 0
                Dim pRedo As UndoRedo.LinkedURCmd = sRedo(i)
                While pRedo IsNot Nothing
                    countRedo += 1
                    pRedo = pRedo.Next
                End While
                bw.Write(countRedo)

                'RedoCommands
                pRedo = sRedo(i)
                For xxi As Integer = 1 To countRedo
                    Dim bRedo() As Byte = pRedo.toBytes
                    bw.Write(bRedo.Length)
                    bw.Write(bRedo)
                    pRedo = pRedo.Next
                Next
            Next

            bw.Close()

        Catch ex As Exception

            MsgBox(ex.Message)

        End Try

    End Sub

End Class
