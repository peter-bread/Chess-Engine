
'Variables that are used across the entire project.
'Public variables from this module are denoted by the prefix MP.
Module GlobalVariables

    'Used to indicate that a square is not occupied by a piece.
    Public Const GV_NULL As Char = "Z"

    'the two players for the game, Player1 = User, Player2 = Computer
    Public GV_player1, GV_player2 As Player

    'describes if a king is in check
    Public GV_kingInCheck As Boolean

    'describes if a promotion piece is being clicked so part of MakeActualMove can be skipped
    Public GV_promotionToHappen As Boolean

    'describes what type of piece that the user has chosen for their pawn to promote to
    Public GV_promotionSelected As Char

    'The number of moves available for the current turn
    Public GV_movesForThisTurn As Integer

    'Value that the board, pieces and window are all scaled/proportional to
    Public GV_scalar As Integer

    'size of piece pictureboxes
    Public GV_standardSize As Size

    'labels to display files (a-h)
    Public GV_fileLabels(7) As Label

    'labels to display ranks (1-8)
    Public GV_rankLabels(7) As Label

    'describes whether the player has the option to drag-and-drop pieces
    Public GV_draggingEnabled As Boolean

    'Graphical board that the user can interact with
    Public GV_realBoard(7, 7) As Square

    'Abstract representation of RealBoard
    Public GV_logicBoard(7, 7) As LogicSquare

    'Abstract board used when generating future moves
    Public GV_temporaryBoard(7, 7) As LogicSquare

    'list of pieces that are on the board
    Public GV_piecesOnBoard As New List(Of Piece)

    'list of pieces that are not on the board (e.g. they have been captured)
    Public GV_piecesOffBoard As New List(Of Piece)

    'list of pieces that are displayed so the user can choose what type of piece they'd like their pawn to promote to
    Public GV_promotionPieces As New List(Of Piece)

    'the piece currently selected by the user
    Public GV_selectedPiece As Piece

    'bitmap image to represent the board
    Public GV_boardBitmap As Bitmap

    'picturebox to display the board and make it interactive (through click event)
    Public GV_boardPBX As PictureBox

    'graphics used to draw on BoardBitmap
    Public GV_gfx As Graphics

    'background colour of form
    Public GV_formBackgroundColour As Color = Color.FromArgb(255, 60, 60, 60)

    'colour for dark squares on the board
    Public GV_blackColour As Color = Color.FromArgb(255, 88, 168, 102)

    'colour for light squares on the board
    Public GV_whiteColour As Color = Color.FromArgb(255, 237, 237, 204)

    'colour used to draw on dark squares on the board that are a valid move for the player
    Public GV_darkBlackColour As Color = Color.FromArgb(255, 61, 117, 72)

    'colour used to draw on light squares on the board that are a valid move for the player
    Public GV_darkWhiteColour As Color = Color.FromArgb(255, 186, 186, 160)

    'brush used for drawing dark squares on BoardBitmap
    Public GV_blackBrush As Brush = New SolidBrush(GV_blackColour)

    'brush used for drawing dark squares on BoardBitmap
    Public GV_whiteBrush As Brush = New SolidBrush(GV_whiteColour)

    'Tree used to store and evaluate future moves
    Public GV_boardStateTree As BoardState

    'Gamestate information for the CURRENT state of the game
    Public GV_turn As Char
    Public GV_castlingAvailability As String
    Public GV_enPassantTargetSquare As LogicSquare
    Public GV_halfmoveClock As Integer
    Public GV_fullmoveCounter As Integer

    'Gamestate information for future/simulated state(s) of game
    Public GV_simulatedTurn As Char
    Public GV_simulatedCastlingAvailability As String
    Public GV_simulatedEnPassantTargetSquare As LogicSquare
    Public GV_simulatedHalfmoveClock As Integer
    Public GV_simulatedFullmoveCounter As Integer

    '2D array for handling what types of castling different pieces control
    Public GV_castleHandling(1, 2) As CastleHandler

    'describes if the game has reached the endgame
    Public GV_isEndgame As Boolean

End Module
