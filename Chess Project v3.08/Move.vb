
'stores data about a move
Public Class Move

    'the type of piece making a move
    Public Property Type As Char

    'the square that the piece making the move is starting on
    Public Property StartSquare As LogicSquare

    'the square that the piece making the move will travel to
    Public Property TargetSquare As LogicSquare

    'Stores the type of piece being captured (if no capture then GV_NULL)
    Public Property Capture As Char

    'Stores whether the move is a pawn double advance
    Public Property DoubleAdvance As Boolean

    'stores whether the move is a pawn en passant capture
    Public Property EnPassant As Boolean

    'stores what type of piece, if any, a pawn is promoting to
    'Q = White Queen, R = White Rook, B = White Bishop, N = White Knight, q = Black Queen, r = Black Rook, b = Black Bishop, n = Black Knight, Z = No Promotion
    Public Property Promotion As Char

    'stores what type of castling, if any, a king is doing
    'K = White Kingside, Q = White Queenside, k = Black Kingside, q = Black Queenside, Z = No Castling
    Public Property Castling As Char

    'stores whether a move puts the opposing king in check
    Public Property CausesCheck As Boolean

    'when a move is found, it is given all its data immediately
    Sub New(Type As Char, StartSquare As LogicSquare, TargetSquare As LogicSquare, Capture As Char, DoubleAdvance As Boolean, EnPassant As Boolean, Promotion As Char, Castling As Char)

        With Me
            .Type = Type
            .StartSquare = StartSquare
            .TargetSquare = TargetSquare
            .Capture = Capture
            .DoubleAdvance = DoubleAdvance
            .EnPassant = EnPassant
            .Promotion = Promotion
            .Castling = Castling
        End With

        'CausesCheck is set to false as this is calcuated later when board states are being analysed
        CausesCheck = False

    End Sub

End Class