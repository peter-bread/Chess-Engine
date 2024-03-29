'a simple, abstract square 
Public Class LogicSquare

    'the first index in GV_LogicBoard, used to represent the X location (file)
    Public Property X As Integer

    'the second index in GV_LogicBoard, used to represent the Y location (rank)
    Public Property Y As Integer

    'the type of piece, if any, that is currently occupying the LogicSquare
    Private _Type As Char

    'allows the rest of the project to access the type of piece that is occupying the LogicSquare
    Public Property Type As Char
        Get
            Return _Type
        End Get
        Set(value As Char)
            _Type = value
        End Set
    End Property


    'returns the colour of the piece that is occupying the LogicSquare
    Public ReadOnly Property Colour As Char
        Get
            If _Type = GV_NULL Then Return GV_NULL  'if no piece then returns GV_NULL
            Return If(Asc(Type) < 91, "w", "b") 'white pieces are uppercase and black pieces are lowercase, so checks ASCII value to see what case it is and thus what colour the pieces is
        End Get
    End Property

    'returns whether a piece is occupying the LogicSquare
    Public ReadOnly Property Occupied As Boolean
        Get
            If _Type = GV_NULL Then Return False    'if no piece on LogicSquare then returns false
            Return True    'otherwise there is a piece so return true
        End Get
    End Property

    'when a new LogicSquare is created, it is given the type of piece occupying it, and both of its indexes for the element it occupies in GV_LogicBoard
    Sub New(Type As Char, X As Integer, Y As Integer)
        _Type = Type
        Me.X = X
        Me.Y = Y
    End Sub

End Class