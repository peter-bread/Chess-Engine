
'stores information b=about a given board at a moment in a game.
'used to create a large tree when simulating future moves
Public Class BoardState

    'stores the result of EvaluateBoard
    Public evalNumber As Double

    'gamestate data for specific board state
    Public turn As Char
    Public castlingAvailability As String
    Public enPassantTargetSquare As LogicSquare
    Public halfmoveClock As Integer
    Public fullmoveCounter As Integer

    Public gameState As String  'describes if a game is in progress, checkmate, stalemate, draw
    Public layout(7, 7) As LogicSquare  'how the pieces are layed out on the board
    Public parent As BoardState 'board state that is one simulated move prior to this one
    Public children As List(Of BoardState)  'list of board states that can be reached from this one with a single move
    Public isLeaf As Boolean    'stores whether this board state is a leaf on GV_BoardStateTree

    Public moveToGetThisBoard As Move   'the move that was made to go from parent board state to this board state

    'stores the base values of each type of piece
    Private Enum PieceValue As Integer

        Pawn = 1
        BishopOrKnight = 3
        Rook = 5
        Queen = 9
        King = 200

    End Enum

    'Calculates a score to give value to a boardstate, (white is positive)
    Public ReadOnly Property EvaluateBoard As Double
        Get

            Dim finalEvaluation As Double = 0       'the evaluation that wil be returned at the end of the property
            Dim numberOfCaptures As Integer = 0     'the number of captures that have been made to reach this board

            Dim opponentKing As LogicSquare = Nothing
            Dim friendlyKing As LogicSquare = Nothing

            Dim materialScore As Double = 0     'score from the material (pieces) on the board

            For Each square In layout   'iterates through board layout
                Select Case square.Type 'selects the type of piece (if any) on a sqaure and adds the associated score
                    'if the piece is a king then checks if it is a friendly king or opposing king
                    Case "K"
                        materialScore += PieceValue.King
                        If turn = "w" Then
                            friendlyKing = square
                        Else
                            opponentKing = square
                        End If
                    Case "k"
                        materialScore -= PieceValue.King
                        If turn = "b" Then
                            friendlyKing = square
                        Else
                            opponentKing = square
                        End If
                    Case "Q"
                        materialScore += PieceValue.Queen * EvalCoefficient(square)
                    Case "q"
                        materialScore -= PieceValue.Queen * EvalCoefficient(square)
                    Case "R"
                        materialScore += PieceValue.Rook * EvalCoefficient(square)
                    Case "r"
                        materialScore -= PieceValue.Rook * EvalCoefficient(square)
                    Case "B", "N"
                        materialScore += PieceValue.BishopOrKnight * EvalCoefficient(square)
                    Case "b", "n"
                        materialScore -= PieceValue.BishopOrKnight * EvalCoefficient(square)
                    Case "P"
                        materialScore += PieceValue.Pawn * EvalCoefficient(square)
                    Case "p"
                        materialScore -= PieceValue.Pawn * EvalCoefficient(square)
                    Case Else
                        'counter for empty squares
                        numberOfCaptures += 1
                End Select
            Next
            'assumes white is positive

            'adds material score to final evaluation
            finalEvaluation += materialScore

            'subtracts 32 from empty squares to account for the squares that start empty
            numberOfCaptures -= 32

            'if the opponent king is in check then adds value (the value added increases the further into the game you get)
            If moveToGetThisBoard.CausesCheck Then
                finalEvaluation += (numberOfCaptures + fullmoveCounter) * 0.2
            End If

            'if the move is a capture then adds value to the move
            If moveToGetThisBoard.Capture <> GV_NULL Then
                finalEvaluation += 3
            End If

            'adds value if the opposing king is closer to the corner of the board. This value increases the further into the game you get
            finalEvaluation += ForceKingToCorner(friendlyKing, opponentKing, numberOfCaptures)

            Return finalEvaluation

        End Get
    End Property

    'adds value the closer the opposing king is to the corner of the board. Takes in 
    Private Function ForceKingToCorner(friendlyKing As LogicSquare, opponentKing As LogicSquare, weight As Integer) As Double
        'idea from internet

        Dim evaluation As Double = 0

        'force king to edge of board
        Dim distanceFromCentreX As Integer = Math.Max(3 - opponentKing.X, opponentKing.X - 4)   'gets horizontal distance from centre
        Dim distanceFromCentreY As Integer = Math.Max(3 - opponentKing.Y, opponentKing.Y - 4)   'gets vertical distance from centre
        Dim distanceFromCentre As Integer = distanceFromCentreX + distanceFromCentreY           'gets total distance from centre

        'force kings closer together
        Dim distanceFromKingX As Integer = Math.Abs(opponentKing.X - friendlyKing.X)    'gets horizontal distance bewteen kings
        Dim distanceFromKingY As Integer = Math.Abs(opponentKing.Y - friendlyKing.Y)    'gets vertical distance between kings
        Dim distanceFromKing As Integer = distanceFromKingX + distanceFromKingY         'gets total distance bewteen kings

        evaluation += distanceFromCentre + 2 - distanceFromKing     'overall evaluation from distance from centre and distance between kings

        Return evaluation * weight * 0.2    'weights the evaluation so it gets bigger as the game goes on

    End Function

    Private Function EvalCoefficient(square As LogicSquare) As Double
        'did not get a chance to do this

        Return 1

    End Function

    'when a new board state is created, takes in the current board layout and the state of the game (in progress, checkmate etc.)
    Sub New(layout(,) As LogicSquare, state As String)

        'stores layout of the board
        For y = 0 To 7
            For x = 0 To 7
                Me.layout(x, y) = New LogicSquare(layout(x, y).Type, layout(x, y).X, layout(x, y).Y)
            Next
        Next

        gameState = state   'stores state of game
        children = New List(Of BoardState)  'creates new list to store any potential children
        parent = Nothing    'assumes it has no parent (assumes it is the root node of GV_BoardStateTree)
        isLeaf = True       'assumes it is a leaf (assumes it is the only node in GV_BoardStateTree)

    End Sub

    'to add a child, takes in the board state of the child to be added
    Public Sub AddChild(child As BoardState)

        child.parent = Me   'sets the child's parent to the current node
        isLeaf = False      'since the current node now has a child, it is no longer a leaf
        children.Add(child) 'adds the child to the current node's list of children

    End Sub

End Class