﻿Imports iBMSC.Editor

Module BMS
    Public Function IsChannelLongNote(ByVal I As String) As Boolean
        Dim xI As Integer
        If Not Integer.TryParse(I, xI) Then Return False
        Return xI >= 50 And xI < 90
    End Function

    Public Function IsChannelHidden(ByVal I As String) As Boolean
        Dim xI As Integer
        If Not Integer.TryParse(I, xI) Then Return False
        Return (xI >= 30 And xI < 50) Or (xI >= 70 And xI < 90)
    End Function

    Public Function IsChannelLandmine(ByVal I As String) As Boolean
        Dim LandmineStart = C36to10("D0")
        Dim LandmineEnd = C36to10("EZ")

        Dim xI As Integer = C36to10(I)

        Return xI > LandmineStart And xI < LandmineEnd
    End Function
End Module
