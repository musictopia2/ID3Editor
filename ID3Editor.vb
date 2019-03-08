Imports System.Collections
Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic
Public Class ID3EditorClass

    Public Shared Function ReadTag(ByVal fileName As String, ByVal tagVersion As ID3TagVersions) As SongInfo

        Select Case tagVersion
            Case ID3TagVersions.ID3v1
                Return ID3v1.Read(fileName)
            Case ID3TagVersions.ID3v2
                Return ID3v2.Read(fileName)
        End Select

        Return New SongInfo()

    End Function

    Public Shared Sub WriteTag(ByVal fileName As String, ByVal tagVersion As ID3TagVersions, ByVal song As SongInfo)

        Select Case tagVersion
            Case ID3TagVersions.ID3v1
                ID3v1.Write(fileName, song)
            Case ID3TagVersions.ID3v2
                ID3v2.Write(fileName, song)
        End Select

    End Sub

End Class






