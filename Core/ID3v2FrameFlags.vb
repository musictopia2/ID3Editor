Friend Class ID3v2FrameFlags
    ' Methods
    Friend Sub New()
        Me._a()
    End Sub

    Friend Sub New(ByVal A_0 As Byte, ByVal A_1 As Byte)
        If ((A_0 = 0) AndAlso (A_1 = 0)) Then
            _a()
        Else
            Dim buffer1 As Byte() = New Byte() {A_0, A_1}
            Dim array1 As New BitArray(buffer1)
            Me.a = array1.Item(7)
            Me.b = array1.Item(6)
            Me.c = array1.Item(5)
            Me.d = array1.Item(15)
            Me.e = array1.Item(14)
            Me.f = array1.Item(13)
        End If
    End Sub

    Private Sub _a()
        a = False
        b = False
        c = False
        d = False
        e = False
        f = False
    End Sub

    Friend Function _b() As Byte()
        Dim num1 As Byte
        Dim num2 As Byte
        If Me.a Then
            num1 = CByte((num1 + 128))
        End If
        If Me.b Then
            num1 = CByte((num1 + 64))
        End If
        If Me.c Then
            num1 = CByte((num1 + 32))
        End If
        If Me.d Then
            num2 = CByte((num2 + 128))
        End If
        If Me.e Then
            num2 = CByte((num2 + 64))
        End If
        If Me.f Then
            num2 = CByte((num2 + 32))
        End If
        Return New Byte() {num1, num2}
    End Function



    ' Properties
    Public Property Compressed() As Boolean
        Get
            Return Me.d
        End Get
        Set(ByVal value As Boolean)
            Me.d = value
        End Set
    End Property

    Public Property Encrypted() As Boolean
        Get
            Return Me.e
        End Get
        Set(ByVal value As Boolean)
            Me.e = value
        End Set
    End Property

    Public Property FileAlterPreservation() As Boolean
        Get
            Return Me.b
        End Get
        Set(ByVal value As Boolean)
            Me.b = value
        End Set
    End Property

    Public Property GroupingIdentity() As Boolean
        Get
            Return Me.f
        End Get
        Set(ByVal value As Boolean)
            Me.f = value
        End Set
    End Property

    Public Property [ReadOnly]() As Boolean
        Get
            Return Me.c
        End Get
        Set(ByVal value As Boolean)
            Me.c = value
        End Set
    End Property

    Public Property TagAlterPreservation() As Boolean
        Get
            Return Me.a
        End Get
        Set(ByVal value As Boolean)
            Me.a = value
        End Set
    End Property




    ' Fields
    Private a As Boolean
    Private b As Boolean
    Private c As Boolean
    Private d As Boolean
    Private e As Boolean
    Private f As Boolean
End Class

