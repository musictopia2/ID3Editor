Imports System.IO
Imports System.Text
Friend Class ID3v1

    ' Methods
    Public Shared Sub Delete(ByVal fileName As String)

        If Not File.Exists(fileName) Then
            Throw New FileNotFoundException("You have to load an existing file before deleting the tag.")
        End If

        Dim stream As FileStream
        Try
            stream = File.Open(fileName, FileMode.Open)
        Catch ex As Exception
            Throw ex
        End Try

        Dim reader As New BinaryReader(stream)
        If SearchForTag(reader) Then
            stream.SetLength((stream.Length - 128))
        End If

        reader.Close()
        stream.Close()

    End Sub

    Public Shared Function Read(ByVal fileName As String) As SongInfo

        If Not File.Exists(fileName) Then
            Throw New FileNotFoundException()
        End If



        Dim fStream As FileStream
        Try
            fStream = File.Open(fileName, FileMode.Open)

            Dim reader As BinaryReader
            reader = New BinaryReader(fStream)

            Dim song As New SongInfo()

            If SearchForTag(reader) Then
                reader.BaseStream.Seek(CLng(-125), SeekOrigin.End)

                song.Song = Encoding.ASCII.GetString(reader.ReadBytes(30)).Trim
                song.Artist = Encoding.ASCII.GetString(reader.ReadBytes(30)).Trim
                song.Album = Encoding.ASCII.GetString(reader.ReadBytes(30)).Trim
                song.Year = Encoding.ASCII.GetString(reader.ReadBytes(4)).Trim

                Dim buffer As Byte() = reader.ReadBytes(30)
                Dim comment As String

                If (buffer(28) = 0) Then
                    song.TrackNumber = buffer(29)
                    comment = Encoding.ASCII.GetString(buffer, 0, 28).Trim
                Else
                    song.TrackNumber = 0
                    comment = Encoding.ASCII.GetString(buffer).Trim
                End If
                Dim genre As Byte = reader.ReadByte
            End If
            reader.Close()
            fStream.Close()

            Return song

        Catch ex As Exception
            Throw ex
        End Try

    End Function

    Public Shared Sub Write(ByVal fileName As String, ByVal song As SongInfo)

        If Not File.Exists(fileName) Then
            Throw New FileNotFoundException("You have to load an existing file before you can save it.")
        End If

        Dim tempSong As SongInfo = Read(fileName)

        Dim stream As FileStream
        Try

            stream = File.Open(fileName, FileMode.Open)
            Dim reader As New BinaryReader(stream)
            Dim writer As New BinaryWriter(stream)

            If SearchForTag(reader) Then
                writer.Seek(-128, SeekOrigin.End)
            Else
                writer.Seek(0, SeekOrigin.End)
            End If

            writer.BaseStream.Write(New Byte() {84, 65, 71}, 0, 3)

            Dim buffer As Byte() = New Byte() {}

            If song.Song.Length > 30 Then song.Song = song.Song.Substring(0, 30)
            buffer = Encoding.ASCII.GetBytes(song.Song)
            buffer = FillByteArray(buffer, 30)
            writer.BaseStream.Write(buffer, 0, 30)

            If song.Artist.Length > 30 Then song.Song = song.Artist.Substring(0, 30)
            buffer = Encoding.ASCII.GetBytes(song.Artist)
            buffer = FillByteArray(buffer, 30)
            writer.BaseStream.Write(buffer, 0, 30)

            If song.Album.Length > 30 Then song.Song = song.Album.Substring(0, 30)
            buffer = Encoding.ASCII.GetBytes(song.Album)
            buffer = FillByteArray(buffer, 30)
            writer.BaseStream.Write(buffer, 0, 30)

            If tempSong.Year.Length > 4 Then tempSong.Song = tempSong.Year.Substring(0, 4)
            buffer = Encoding.ASCII.GetBytes(tempSong.Year)
            buffer = FillByteArray(buffer, 4)
            writer.BaseStream.Write(buffer, 0, 4)

            buffer = Encoding.ASCII.GetBytes("")
            buffer = FillByteArray(buffer, 30)

            If Not String.IsNullOrEmpty(tempSong.TrackNumber) Then
                Try
                    Dim trackNum As Byte = Byte.Parse(tempSong.TrackNumber)
                    If (tempSong.TrackNumber <> 0) Then
                        buffer(29) = trackNum
                    End If
                Catch ex As Exception
                    buffer(29) = 0
                End Try
            Else
                buffer(29) = 0
            End If

            song.TrackNumber = tempSong.TrackNumber
            song.Year = tempSong.Year

            writer.BaseStream.Write(buffer, 0, 30)
            writer.BaseStream.Write(New Byte() {0}, 0, 1)

            writer.Close()
            reader.Close()
            stream.Close()

        Catch ex As Exception
            Throw ex
        End Try
    End Sub



    Private Shared Function SearchForTag(ByVal reader As BinaryReader) As Boolean
        Dim index As Long = reader.BaseStream.Position
        If (reader.BaseStream.Length >= 128) Then
            reader.BaseStream.Seek(CLng(-128), SeekOrigin.End)

            Dim buffer1 As Byte() = New Byte() {84, 65, 71}
            Dim buffer2 As Byte() = reader.ReadBytes(3)

            reader.BaseStream.Seek(index, SeekOrigin.Begin)

            If (((buffer1(0) = buffer2(0)) AndAlso (buffer1(1) = buffer2(1))) AndAlso (buffer1(2) = buffer2(2))) Then
                Return True
            End If
        End If

        Return False
    End Function

    Private Shared Function FillByteArray(ByVal byteArray As Byte(), ByVal size As Integer) As Byte()

        Dim buffer As Byte() = New Byte(size - 1) {}
        byteArray.CopyTo(buffer, 0)

        Dim count As Integer = byteArray.Length
        Do While (count < size)
            buffer(count) = 0
            count += 1
        Loop

        Return buffer
    End Function



End Class


