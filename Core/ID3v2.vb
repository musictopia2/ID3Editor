Imports System.Text
Imports System.Reflection
Imports System.IO



Friend Class ID3v2

        Enum ID3v2TextEncodingTypes As Byte
            ' Fields
            ISO88591 = 0
            [Unicode] = 1
        End Enum

        ' Methods
        Public Shared Sub Delete(ByVal fileName As String)

            If Not File.Exists(fileName) Then
                Throw New FileNotFoundException("You have to load an existing file before deleting the tag.")
            End If

            Dim stream As FileStream
            Try
                stream = File.Open(fileName, FileMode.Open)
                Dim reader As New BinaryReader(stream)
                Dim writer As New BinaryWriter(stream)

                Dim header As ID3v2Header = GetHeader(reader)

                If (Not header Is Nothing) Then
                    Dim buffer As Byte() = reader.ReadBytes(CInt(reader.BaseStream.Length))
                    writer.BaseStream.SetLength((stream.Length - header.Length))
                    writer.Seek(0, SeekOrigin.Begin)
                    writer.BaseStream.Write(buffer, header.Length, (buffer.Length - header.Length))
                End If

                writer.Close()
                reader.Close()
                stream.Close()

            Catch ex As IOException
                Throw ex
            End Try
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

                '----------------------------------------------------------

                Dim header As ID3v2Header = GetHeader(reader)
                If Not header Is Nothing Then
                    'reader.BaseStream.Seek(10, SeekOrigin.Begin)

                    'Dim extHeader As ID3v2ExtendedHeader
                    'If header.ExtendedHeader Then
                    '    extHeader = New ID3v2ExtendedHeader(reader)
                    '    reader.BaseStream.Seek(16, SeekOrigin.Begin)
                    'End If

                    Dim unsynchronisation As Boolean 'me.g

                    Dim framesSize As Long 'num4
                    Dim framesBuffer As Byte() ' buffer2
                    Dim tagSize As TagSize 'a1


                    reader.BaseStream.Seek(5, SeekOrigin.Begin)

                    Dim flags As Byte = reader.ReadByte

                    If ((flags And 128) = 128) Then
                        flags = CByte((flags Xor 128))
                        unsynchronisation = True
                    End If
                    Dim extended As Boolean = ((flags And 64) = 64)
                    'Me.q = True
                    'Me.p = True


                    tagSize.a = reader.ReadByte
                    tagSize.b = reader.ReadByte
                    tagSize.c = reader.ReadByte
                    tagSize.d = reader.ReadByte


                    If extended Then
                        flags = CByte((flags - 64))
                        Try
                            Dim extSizeBuffer As Byte() = reader.ReadBytes(4)
                            Array.Reverse(extSizeBuffer)
                            Dim extSize As Integer = BitConverter.ToInt32(extSizeBuffer, 0)
                            Dim buffer3 As Byte() = reader.ReadBytes(extSize)
                            framesSize = ((GetSize(tagSize) - 4) - extSize)
                            GoTo Label_01A5
                        Catch ex As Exception
                            reader.Close()
                            fStream.Close()
                            Throw ex
                        End Try
                    End If
                    framesSize = GetSize(tagSize)
Label_01A5:
                    framesBuffer = reader.ReadBytes(CInt(framesSize))
                    If unsynchronisation Then
                        framesBuffer = UnsynchronisReplace(framesBuffer)
                    End If
                    Dim stream1 As New MemoryStream(framesBuffer)
                    Dim reader1 As New BinaryReader(stream1, Encoding.GetEncoding(28591))
                    Dim frameID As String = String.Empty 'text1

                    Dim i As Long 'num7
                    Do
                        Dim num1 As Byte
                        Dim num2 As Byte
                        Dim num3 As Integer
                        Try
                            frameID = New String(reader1.ReadChars(4))
                            'Microsoft.VisualBasic.CompilerServices.EmbeddedOperators.
                            If (CompilerServices.EmbeddedOperators.CompareString(frameID.Trim(New Char() {ChrW(0)}), String.Empty, False) = 0) Then 'hopefully this still works.
                                Exit Do
                            End If
                            Dim buffer1 As Byte() = reader1.ReadBytes(4)

                            Array.Reverse(buffer1)
                            num3 = BitConverter.ToInt32(buffer1, 0)

                            num1 = reader1.ReadByte
                            num2 = reader1.ReadByte
                        Catch ex As Exception
                            reader.Close()
                            fStream.Close()
                            Throw ex
                        End Try


                        If (num3 > 0) Then
                            Dim buffer5 As Byte()
                            Dim flags1 As New ID3v2FrameFlags(num1, num2)
                            Try
                                buffer5 = reader1.ReadBytes(num3)
                            Catch ex As Exception
                                'A_2 = ((10 + num4) - 1)
                                reader.Close()
                                fStream.Close()
                                Throw ex
                            End Try

                            Select Case frameID

                                Case "TIT2"
                                    song.Song = GetFrameContent(buffer5, flags1)

                                Case "TALB"
                                    song.Album = GetFrameContent(buffer5, flags1)

                                Case "TPE1"
                                    song.Artist = GetFrameContent(buffer5, flags1)

                                Case "TRCK"
                                    song.TrackNumber = GetFrameContent(buffer5, flags1)

                                Case "TYER"
                                    song.Year = GetFrameContent(buffer5, flags1)

                            End Select

                            If ((unsynchronisation AndAlso (buffer5.Length > 0)) AndAlso (buffer5((buffer5.Length - 1)) = 255)) Then
                                reader1.ReadByte()
                            End If
                        End If
                        i = (i + (10 + num3))

                    Loop While (i < framesSize)

                End If
                '----------------------------------------------------------

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

                Delete(fileName)

                stream = File.Open(fileName, FileMode.Open)
                Dim writer As New BinaryWriter(stream, Encoding.GetEncoding(28591))


                '--------------------------------------

                writer.Write(Encoding.GetEncoding(28591).GetBytes("ID3"))
                writer.Write(New Byte() {4, 0}) 'version
                writer.Write(New Byte() {0}) 'flags

                Dim framesBuffer As Byte() = GetBytesOfFrames(song, tempSong)

                Dim tagSize As TagSize = SetSize(framesBuffer.Length)

                writer.Write(tagSize.a)
                writer.Write(tagSize.b)
                writer.Write(tagSize.c)
                writer.Write(tagSize.d)


                writer.Write(framesBuffer)

                '--------------------------------------

                writer.Close()
                stream.Close()

            Catch ex As Exception
                Throw ex
            End Try
        End Sub



        Private Shared Function GetHeader(ByVal reader As BinaryReader) As ID3v2Header

            Dim header As ID3v2Header = Nothing

            Dim position As Long = reader.BaseStream.Position
            reader.BaseStream.Seek(CLng(0), SeekOrigin.Begin)

            Dim expected As Byte() = New Byte() {73, 68, 51} ' "ID3"
            Dim actual As Byte() = reader.ReadBytes(10)

            If expected(0) = actual(0) AndAlso expected(1) = actual(1) AndAlso expected(2) = actual(2) Then
                header = New ID3v2Header(actual)

                'Dim num2 As Integer = 0
                'Do While (num2 < (header1.Length - b.BaseStream.Position))
                '    Dim frame1 As ID3Frame
                '    Try
                '        frame1 = New ID3Frame(b)
                '    Catch exception1 As PaddingException
                '        Console.WriteLine(("padding found: " & ((header1.Length - num2) - 10)))
                '        Exit Do
                '    End Try
                '    num2 = (num2 + frame1.Size)
                '    Me._frames.Add(frame1.ID, frame1)
                'Loop
            End If
            reader.BaseStream.Seek(position, SeekOrigin.Begin)

            Return header
        End Function




        Private Structure TagSize
            ' Fields
            Public a As Byte
            Public b As Byte
            Public c As Byte
            Public d As Byte
        End Structure

        Private Shared Function GetSize(ByVal tagSize As TagSize) As Integer
            Dim result As Integer = (tagSize.a * 2097152)
            result = (result + (tagSize.b * 16384))
            result = (result + (tagSize.c * 128))
            Return (result + tagSize.d)
        End Function

        Private Shared Function SetSize(ByVal size As Long) As TagSize
            Dim tagSize As TagSize
            Dim num1 As Long = size
            tagSize.a = CByte(Math.Round(Math.Floor(CDbl((CDbl(num1) / 2097152)))))
            num1 = (num1 - (tagSize.a * 2097152))
            tagSize.b = CByte(Math.Round(Math.Floor(CDbl((CDbl(num1) / 16384)))))
            num1 = (num1 - (tagSize.b * 16384))
            tagSize.c = CByte(Math.Round(Math.Floor(CDbl((CDbl(num1) / 128)))))
            num1 = (num1 - (tagSize.c * 128))
            tagSize.d = CByte(num1)
            Return tagSize
        End Function



        Private Shared Function UnsynchronisReplace(ByVal framesBuffer As Byte()) As Byte()
            Dim index As Integer = 0
            Dim list As New ArrayList(framesBuffer)
            Do While True
            Dim i As Integer = CheckFF(list, index)
            If (i > -1) Then
                list.RemoveAt((i + 1))
                index = (i + 2)
            End If
            If (i = -1) Then
                Return DirectCast(list.ToArray(GetType(Byte)), Byte())
            End If
        Loop
        Return Nothing
    End Function

    Private Shared Function CheckFF(ByVal list As ArrayList, ByVal index As Integer) As Integer
        If (index >= list.Count) Then
            Return -1
        End If
        Dim listIndex As Integer = list.IndexOf(CByte(255), index)
        If (listIndex <= -1) Then
            Return -1
        End If
        If ((Microsoft.VisualBasic.CompilerServices.Conversions.ToByte(list.Item((listIndex + 1))) = 0) And
            (Microsoft.VisualBasic.CompilerServices.Conversions.ToByte(list.Item((listIndex + 2))) = 0)) Then
            Return listIndex
        End If
        Return CheckFF(list, (listIndex + 1))
    End Function



        Private Shared Function GetFrameContent(ByVal frameBytes As Byte(), ByVal frameFlags As ID3v2FrameFlags) As String

            If (frameFlags.Compressed OrElse frameFlags.Encrypted) Then
                Return String.Empty
            End If


            If (frameBytes.Length > 1) Then
                Dim content As String = ""

                Dim encodeType As ID3v2TextEncodingTypes = DirectCast(frameBytes(0), ID3v2TextEncodingTypes)

                Try
                    content = GetEncodedText(frameBytes, encodeType)
                Catch ex As Exception
                    If ex.Message = "InvalidUnicodeByteOrderException" Then
                        content = String.Empty
                    End If

                    If ex.Message = "UnsupportedUnicodeByteOrderException" Then
                        content = String.Empty
                    End If

                    If ex.Message = "InvalidTextEncodingException" Then
                        content = GetEncodedText(frameBytes, ID3v2TextEncodingTypes.ISO88591)
                    End If

                End Try

                Return content.TrimEnd(New Char() {ChrW(0)})
            Else
                Return String.Empty
            End If

        End Function

        Public Shared Function GetEncodedText(ByVal bytes As Byte(), ByVal encodeType As ID3v2TextEncodingTypes) As String
            If (bytes.Length - 1 > 0) Then
                Select Case encodeType
                    Case ID3v2TextEncodingTypes.ISO88591
                        Return New String(Encoding.GetEncoding(28591).GetChars(bytes, 1, bytes.Length - 1))
                    Case ID3v2TextEncodingTypes.Unicode
                        Return GetUnicodeText(bytes)
                End Select
                Throw New Exception("InvalidTextEncodingException")
            End If
            Return String.Empty
        End Function

        Friend Shared Function GetUnicodeText(ByVal bytes As Byte()) As String
            Dim num1 As Byte = bytes(1)
            Dim num2 As Byte = bytes((1 + 1))
            If ((num1 = 255) And (num2 = 254)) Then
                Return New String(Encoding.Unicode.GetChars(bytes, (1 + 2), (bytes.Length - 1 - 2)))
            End If
            If ((num1 = 254) And (num2 = 255)) Then
                Throw New Exception("UnsupportedUnicodeByteOrderException")
            End If
            'A_0.Exceptions.Add(New ID3v2InvalidUnicodeByteOrderException(A_0))
            Return New String(Encoding.Unicode.GetChars(bytes, 1, bytes.Length - 1))
        End Function




        Private Shared Function GetBytesOfFrames(ByVal song As SongInfo, ByVal tempSong As SongInfo) As Byte()

            Dim stream As New MemoryStream
            Dim writer As New BinaryWriter(stream)

            If Not String.IsNullOrEmpty(song.Song) Then WriteFrame(writer, "TIT2", song.Song)
            If Not String.IsNullOrEmpty(song.Album) Then WriteFrame(writer, "TALB", song.Album)
            If Not String.IsNullOrEmpty(song.Artist) Then WriteFrame(writer, "TPE1", song.Artist)
            If Not String.IsNullOrEmpty(song.TrackNumber) Then WriteFrame(writer, "TRCK", tempSong.TrackNumber)
            If Not String.IsNullOrEmpty(song.Year) Then WriteFrame(writer, "TYER", tempSong.Year)

            Return stream.ToArray

        End Function


        Private Shared Sub WriteFrame(ByVal writer As BinaryWriter, ByVal frameID As String, ByVal frameContent As String)

            Dim frameIDBytes As Byte() = Encoding.GetEncoding(28591).GetBytes(frameID)
            writer.Write(frameIDBytes)

            Dim content As Byte() = Encoding.GetEncoding(28591).GetBytes(frameContent)

            Dim size As Byte() = BitConverter.GetBytes(content.Length + 1)
            Array.Reverse(size)
            If (size.Length < 4) Then
                Dim buffer2 As Byte() = New Byte(((4 - 1) + 1) - 1) {}
                size.CopyTo(buffer2, 0)
                size = buffer2
            End If

            writer.Write(size)
            writer.Write(New Byte() {0, 0}) 'Flags
            writer.Write(CByte(ID3v2TextEncodingTypes.ISO88591)) 'encription
            writer.Write(content) ' content

        End Sub


    End Class


