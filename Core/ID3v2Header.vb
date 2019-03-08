Friend Class ID3v2Header
    ' Methods
    Public Sub New(ByVal headerBytes As Byte())

        Me.Version = New Version(headerBytes(3), headerBytes(4))

        Me._flags = headerBytes(5)
        Me.UseUnsynchronisation = (Me._flags And 128 = 128)
        Me.ExtendedHeader = Me._flags And 64 = 64
        Me.Experimental = Me._flags And 32 = 32
        Me.FooterPresent = Me._flags And 16 = 16
        Me._length = ReadUnsynchronizedData(headerBytes, 6, 4)

    End Sub

    Public Overrides Function ToString() As String
        Return String.Concat(New String() {"Use unsynchronisation" & ChrW(9), Me.UseUnsynchronisation.ToString, ChrW(10) & "Experimental" & ChrW(9) & ChrW(9), Me.Experimental.ToString, ChrW(10) & "Footer present" & ChrW(9) & ChrW(9), Me.FooterPresent.ToString, ChrW(10) & "Length" & ChrW(9) & ChrW(9) & ChrW(9), Me.Length.ToString, ChrW(10) & "Version" & ChrW(9) & ChrW(9) & ChrW(9), Me.Version.ToString})
    End Function


    Private Function ReadUnsynchronizedData(ByVal array As Byte(), ByVal offset As Integer, ByVal length As Integer) As Integer
        Dim result As Integer = 0

        Dim num As Integer = (offset + length)
        Dim i As Integer = offset

        Do While (i < num)
            result = ((result << 7) Or array(i))
            i += 1
        Loop

        Return result
    End Function


    ' Properties
    Public ReadOnly Property Experimental() As Boolean

    Public ReadOnly Property ExtendedHeader() As Boolean

    Public ReadOnly Property FooterPresent() As Boolean

    Public ReadOnly Property Length() As Integer
        Get
            Return (Me._length + 10)
        End Get
    End Property

    Public ReadOnly Property UseUnsynchronisation() As Boolean

    Public ReadOnly Property Version() As Version
    Private ReadOnly _flags As Byte
    Private ReadOnly _length As Integer
End Class


