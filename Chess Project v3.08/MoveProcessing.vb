'Handles the generation, validation and simulation of moves.
'Public subroutines and functions from this module are denoted by the prefix MP.
Module MoveProcessing

    Private moveStack As New Stack(Of Move) 'stores the sequence of moves that have been made to get to a given board state
    Public MP_startDepth As Integer 'the depth of the search

    Private halfmoveStack As New Stack(Of Integer)  'stores the sequence of halfmove clock values after each move leading up to the current board state
    Private validating As Boolean   'used for seeing if a king is put in check

    'simulates making moves into the future, evaluating board states, and adding this data to te the board state tree
    Public Function MP_GenerateMoves(ByVal currentBoardState As BoardState, ByVal depth As Integer, ByVal alpha As Double, ByVal beta As Double, ByVal maximisingPlayer As Boolean) As Double
        'currentBoardState: the board state that is having future moves analysed from
        'depth: how many moves into the future are being examined
        'alpha: the minimum score that the maximising player is assured of
        'beta : the maximum score that the minimising player is assured of

        'base case: if depth is 0 then end of search, return score for current board
        If depth = 0 Then
            currentBoardState.evalNumber = currentBoardState.EvaluateBoard
            Return currentBoardState.EvaluateBoard
        End If

        validating = False

        Dim pseudoLegalMoves As New List(Of Move)
        Dim legalMoves As New List(Of Move)

        Dim numberOfPieces As Integer = 0

        'finding all pseudo legal moves for the colour whose turn it is
        For Each square In currentBoardState.layout

            If square.Colour = GV_simulatedTurn Then

                Select Case square.Type.ToString.ToUpper

                    Case "R"
                        pseudoLegalMoves.AddRange(GenerateRookMoves(currentBoardState.layout, square.Type, square))
                        numberOfPieces += 1

                    Case "B"
                        pseudoLegalMoves.AddRange(GenerateBishopMoves(currentBoardState.layout, square.Type, square))
                        numberOfPieces += 1

                    Case "Q", "K"
                        pseudoLegalMoves.AddRange(GenerateQueenOrKingMoves(currentBoardState.layout, square.Type, square))
                        numberOfPieces += 1

                    Case "N"
                        pseudoLegalMoves.AddRange(GenerateKnightMoves(currentBoardState.layout, square.Type, square))
                        numberOfPieces += 1

                    Case "P"
                        pseudoLegalMoves.AddRange(GeneratePawnMoves(currentBoardState.layout, square.Type, square))
                        numberOfPieces += 1

                End Select

            End If

        Next

        If numberOfPieces < 8 Then GV_isEndgame = True

        'resets TemporaryBoard to the parent boardstate
        For y = 0 To 7
            For x = 0 To 7
                GV_temporaryBoard(x, y) = New LogicSquare(currentBoardState.layout(x, y).Type, currentBoardState.layout(x, y).X, currentBoardState.layout(x, y).Y)
            Next
        Next


        'filters out moves that would leave king threatened
        For i = 0 To pseudoLegalMoves.Count - 1

            MakeMove(GV_temporaryBoard, pseudoLegalMoves(i))
            If Not MoveThreatensKing(GV_temporaryBoard, If(GV_simulatedTurn = "w", "b", "w")) Then
                legalMoves.Add(pseudoLegalMoves(i))
            End If
            UnmakeMove(GV_temporaryBoard, pseudoLegalMoves(i))

        Next i

        'orders moves
        legalMoves = OrderMoves(legalMoves)

        'if no moves can be played then return +/- infinity as end of game is either best or worst case for one of the players
        If legalMoves.Count = 0 Then

            currentBoardState.gameState = CheckWhyGameIsOver(currentBoardState.layout, False)

            If maximisingPlayer Then
                currentBoardState.evalNumber = Double.NegativeInfinity
                Return Double.NegativeInfinity
            Else
                currentBoardState.evalNumber = Double.PositiveInfinity
                Return Double.PositiveInfinity
            End If

        End If


        Dim maxEval, minEval, eval As Double

        'adds legal moves to BoardStateTree and evaluates them
        For i = 0 To legalMoves.Count - 1
            GV_simulatedTurn = If(GV_simulatedTurn = "w", "b", "w")
            MakeMove(GV_temporaryBoard, legalMoves(i))  'simulates each move
            moveStack.Push(legalMoves(i))   'adds most recent move to moveStack
            currentBoardState.AddChild(New BoardState(GV_temporaryBoard, "in progress"))

            With currentBoardState.children.Last

                'if after simulating a move the halfmove clock = 100 then that move is illegal and doesn't need to be considered
                If GV_simulatedHalfmoveClock = 100 Then
                    .gameState = "50 move rule"
                    UnmakeMove(GV_temporaryBoard, legalMoves(i))
                    moveStack.Pop() 'removes most recent move from moveStack
                    GV_simulatedTurn = If(GV_simulatedTurn = "w", "b", "w")
                    Continue For    'since these board states are end of game, no evaluation needed so can move to next move
                End If

                'stores simulated game state information in the board state node
                .turn = GV_simulatedTurn
                .castlingAvailability = GV_simulatedCastlingAvailability
                .enPassantTargetSquare = If(GV_simulatedEnPassantTargetSquare IsNot Nothing, currentBoardState.layout(GV_simulatedEnPassantTargetSquare.X, GV_simulatedEnPassantTargetSquare.Y), Nothing)
                .halfmoveClock = GV_simulatedHalfmoveClock
                .fullmoveCounter = GV_simulatedFullmoveCounter
                .moveToGetThisBoard = legalMoves(i)

            End With

            'get maximising player's assured score
            If maximisingPlayer Then

                maxEval = Double.NegativeInfinity
                eval = MP_GenerateMoves(currentBoardState.children.Last, depth - 1, alpha, beta, False)
                maxEval = Math.Max(maxEval, eval)
                alpha = Math.Max(alpha, eval)
                If beta <= alpha Then
                    UnmakeMove(GV_temporaryBoard, legalMoves(i))
                    moveStack.Pop() 'removes most recent move from moveStack
                    GV_simulatedTurn = If(GV_simulatedTurn = "w", "b", "w")
                    Exit For
                End If

            Else    'get minimising player's assured score

                minEval = Double.PositiveInfinity
                eval = MP_GenerateMoves(currentBoardState.children.Last, depth - 1, alpha, beta, True)
                minEval = Math.Min(minEval, eval)
                beta = Math.Min(beta, eval)
                If beta <= alpha Then
                    UnmakeMove(GV_temporaryBoard, legalMoves(i))
                    moveStack.Pop() 'removes most recent move from moveStack
                    GV_simulatedTurn = If(GV_simulatedTurn = "w", "b", "w")
                    Exit For
                End If

            End If

            'unmake the move that was simulated
            UnmakeMove(GV_temporaryBoard, legalMoves(i))
            moveStack.Pop() 'removes most recent move from moveStack
            GV_simulatedTurn = If(GV_simulatedTurn = "w", "b", "w")

        Next i

        'return assured score of the player who the search is being perfomred in favour of
        If maximisingPlayer Then
            currentBoardState.evalNumber = maxEval
            Return maxEval
        Else
            currentBoardState.evalNumber = minEval
            Return minEval
        End If

    End Function

    'orders list of moves in an attempt to put the best moves first, meaning less boards need to be evaluated
    Private Function OrderMoves(moves As List(Of Move)) As List(Of Move)

        validating = True

        Dim orderedMoves As New List(Of Move)
        Dim capturesAndChecks As New List(Of Move)  'moves that make a capture and put the king in check
        'in future: Dim capturesAndPromotions As New List(Of Move)
        Dim captures As New List(Of Move)           'moves that make a capture (no check)
        'in future: Dim promotions As New List(Of Move)
        Dim checks As New List(Of Move)             'moves tha put king in check (no capture)
        Dim other As New List(Of Move)              'moves that are neither a capture nor a check

        'categorise moves and store them in lists
        For Each move In moves

            If PutsKingInCheck(GV_temporaryBoard, move.TargetSquare, move.Type) Then
                move.CausesCheck = True
                If move.Capture <> GV_NULL Then
                    capturesAndChecks.Add(move)
                Else
                    checks.Add(move)
                End If
            Else
                If move.Capture <> GV_NULL Then
                    captures.Add(move)
                Else
                    other.Add(move)
                End If
            End If

        Next

        'order capturing moves using MVV-LVA
        capturesAndChecks = MVV_LVA(capturesAndChecks)
        captures = MVV_LVA(captures)

        'add lists of moves to ordered move list
        orderedMoves.AddRange(capturesAndChecks)
        orderedMoves.AddRange(captures)
        orderedMoves.AddRange(checks)
        orderedMoves.AddRange(other)

        validating = False

        Return orderedMoves

    End Function

    'Most Valuable Victim - Least Valuable Aggressor
    'orders captures based on the piece making the capture and the piece being captured
    'best case: P x Q, worst case: K x P
    'This is only an estimate, in endgame this may not be accurate
    Private Function MVV_LVA(captures As List(Of Move)) As List(Of Move)

        Dim victimArray(4) As List(Of Move)

        For i = 0 To 4
            victimArray(i) = New List(Of Move)
        Next

        'sort moves by the type of piece being captured (victim)
        For Each move In captures
            Select Case move.Capture.ToString.ToUpper
                Case "Q"
                    victimArray(0).Add(move)
                Case "R"
                    victimArray(1).Add(move)
                Case "B"
                    victimArray(2).Add(move)
                Case "N"
                    victimArray(3).Add(move)
                Case "P"
                    victimArray(4).Add(move)
            End Select
        Next move

        Dim temp(5) As List(Of Move)

        Dim finalList As New List(Of Move)

        'for each list of moves (where all the victims are the same type of piece)
        'order moves by the type of piece making the capture (aggressor)
        'moves made by pawns go at the start, moves made by kings go at the end
        For Each sublist In victimArray

            For i = 0 To 5
                temp(i) = New List(Of Move)
            Next i

            For Each move In sublist

                Select Case move.Type.ToString.ToUpper
                    Case "P"
                        temp(0).Add(move)
                    Case "N"
                        temp(1).Add(move)
                    Case "B"
                        temp(2).Add(move)
                    Case "R"
                        temp(3).Add(move)
                    Case "Q"
                        temp(4).Add(move)
                    Case "K"
                        temp(5).Add(move)
                End Select

            Next move

            For i = 0 To 5
                finalList.AddRange(temp(i))
            Next i

        Next sublist

        Return finalList
        'queen victims (pawn => king aggressor) ==> pawn victims (pawn => king aggressor)


    End Function

    'finds moves for all pieces of one colour
    Public Function GetMovesForSpecificColour(colour As Char)

        Dim count As Integer = 0
        'counts how many moves are found
        'if 0 then end of game

        'find moves and add them to each piece's list of legal moves
        For Each piece In GV_piecesOnBoard

            If piece.Colour = colour Then
                piece.validMoves.AddRange(GetMovesForSpecificPiece(GV_boardStateTree, piece))
                count += piece.validMoves.Count
            End If

        Next

        Return count

    End Function

    'generates moves for a specific piece
    Public Function GetMovesForSpecificPiece(currentBoardState As BoardState, piece As Piece)

        Dim pseudoLegalMoves As New List(Of Move)
        Dim legalMoves As New List(Of Move)

        'finds moves based on the type of piece
        Select Case GV_logicBoard(piece.X, piece.Y).Type.ToString.ToUpper
            Case "R"
                pseudoLegalMoves.AddRange(GenerateRookMoves(currentBoardState.layout, piece.Type, GV_logicBoard(piece.X, piece.Y)))
            Case "B"
                pseudoLegalMoves.AddRange(GenerateBishopMoves(currentBoardState.layout, piece.Type, GV_logicBoard(piece.X, piece.Y)))
            Case "Q"
                pseudoLegalMoves.AddRange(GenerateQueenOrKingMoves(currentBoardState.layout, piece.Type, GV_logicBoard(piece.X, piece.Y)))
            Case "K"
                pseudoLegalMoves.AddRange(GenerateQueenOrKingMoves(currentBoardState.layout, piece.Type, GV_logicBoard(piece.X, piece.Y)))
            Case "N"
                pseudoLegalMoves.AddRange(GenerateKnightMoves(currentBoardState.layout, piece.Type, GV_logicBoard(piece.X, piece.Y)))
            Case "P"
                pseudoLegalMoves.AddRange(GeneratePawnMoves(currentBoardState.layout, piece.Type, GV_logicBoard(piece.X, piece.Y)))
        End Select

        'resets TemporaryBoard to the parent boardstate
        For y = 0 To 7
            For x = 0 To 7
                GV_temporaryBoard(x, y) = New LogicSquare(currentBoardState.layout(x, y).Type, currentBoardState.layout(x, y).X, currentBoardState.layout(x, y).Y)
            Next
        Next

        'filters out moves that would leave king threatened
        For i = 0 To pseudoLegalMoves.Count - 1

            MakeMove(GV_temporaryBoard, pseudoLegalMoves(i))
            If Not MoveThreatensKing(GV_temporaryBoard, If(GV_turn = "w", "b", "w")) Then
                legalMoves.Add(pseudoLegalMoves(i))
            End If
            UnmakeMove(GV_temporaryBoard, pseudoLegalMoves(i))

        Next i

        Return legalMoves

    End Function

    'checks if a move puts the opponent's king is in check
    Private Function PutsKingInCheck(board(,) As LogicSquare, attackingPiece As LogicSquare, type As Char) As Boolean

        Dim attackingPieceMoves As New List(Of Move)

        Dim temp As Char = attackingPiece.Type

        attackingPiece.Type = type

        Select Case attackingPiece.Type.ToString.ToUpper
            Case "R"
                attackingPieceMoves.AddRange(GenerateRookMoves(board, attackingPiece.Type, attackingPiece))
            Case "B"
                attackingPieceMoves.AddRange(GenerateBishopMoves(board, attackingPiece.Type, attackingPiece))
            Case "Q", "K"
                attackingPieceMoves.AddRange(GenerateQueenOrKingMoves(board, attackingPiece.Type, attackingPiece))
            Case "N"
                attackingPieceMoves.AddRange(GenerateKnightMoves(board, attackingPiece.Type, attackingPiece))
            Case "P"
                attackingPieceMoves.AddRange(GeneratePawnMoves(board, attackingPiece.Type, attackingPiece))
        End Select

        attackingPiece.Type = temp

        For Each move In attackingPieceMoves
            If move.Capture.ToString.ToUpper = "K" Then Return True
        Next
        Return False

    End Function

    'finds the reason why a game has ended
    Public Function CheckWhyGameIsOver(board(,) As LogicSquare, actual As Boolean)

        Dim turn As Char = If(actual, GV_turn, GV_simulatedTurn)
        Dim halfmoveClock As Integer = If(actual, GV_halfmoveClock, GV_simulatedHalfmoveClock)

        If MoveThreatensKing(board, If(turn = "w", "b", "w")) Then
            Return "checkmate"
        Else
            If halfmoveClock = 100 Then
                Return "50 move rule"
            Else
                Return "stalemate"
            End If
        End If

    End Function

    'checks if a move would put the friendly king in check, thusly invalidating the move
    Public Function MoveThreatensKing(ByVal board(,) As LogicSquare, ByVal turn As Char)

        validating = True

        Dim squaresWithPieces As New List(Of LogicSquare)
        Dim pseudoLegalMoves As New List(Of Move)

        'if black's turn, looking for white's king and vice versa
        Dim king As Char = If(turn = "b", "K", "k")

        'filters LogicSquares to those that contain a piece of the correct colour
        For Each square In board
            If square.Type <> GV_NULL And square.Colour = turn Then squaresWithPieces.Add(square)
        Next

        For Each square In squaresWithPieces

            Select Case square.Type.ToString.ToUpper

                Case "R"
                    pseudoLegalMoves.AddRange(GenerateRookMoves(board, square.Type, square))

                Case "B"
                    pseudoLegalMoves.AddRange(GenerateBishopMoves(board, square.Type, square))

                Case "Q", "K"
                    pseudoLegalMoves.AddRange(GenerateQueenOrKingMoves(board, square.Type, square))

                Case "N"
                    pseudoLegalMoves.AddRange(GenerateKnightMoves(board, square.Type, square))

                Case "P"
                    pseudoLegalMoves.AddRange(GeneratePawnMoves(board, square.Type, square))

            End Select

            'checks after each piece rather than after all pieces so less iterartions may be needed
            For Each possibleMove In pseudoLegalMoves
                If possibleMove.TargetSquare.Type = king Then Return True
            Next

            pseudoLegalMoves.Clear()

        Next

        validating = False

        Return False

    End Function

    'checks if a square is attacked by any opposing pieces
    Public Function SquareIsThreatened(ByVal currentBoard(,) As LogicSquare, ByVal turn As Char, ByVal squareBeingChecked As LogicSquare)

        Dim squaresWithPieces As New List(Of LogicSquare)
        Dim pseudoLegalMoves As New List(Of Move)

        'filters LogicSquares to those that contain a piece
        For Each square In currentBoard
            If square.Type <> GV_NULL And square.Colour = turn Then squaresWithPieces.Add(square)
        Next

        'finds pseudo legal moves for the opposition
        For Each square In squaresWithPieces

            Select Case square.Type.ToString.ToUpper

                Case "R"
                    pseudoLegalMoves.AddRange(GenerateRookMoves(currentBoard, square.Type, square))

                Case "B"
                    pseudoLegalMoves.AddRange(GenerateBishopMoves(currentBoard, square.Type, square))

                Case "Q", "K"
                    pseudoLegalMoves.AddRange(GenerateQueenOrKingMoves(currentBoard, square.Type, square))

                Case "N"
                    pseudoLegalMoves.AddRange(GenerateKnightMoves(currentBoard, square.Type, square))

                Case "P"
                    pseudoLegalMoves.AddRange(GeneratePawnMoves(currentBoard, square.Type, square))

            End Select

        Next

        'checks if the square being checked is the target square of any of the oppositions move
        For Each possibleMove In pseudoLegalMoves
            If possibleMove.TargetSquare Is squareBeingChecked Then Return True
        Next
        Return False

    End Function

    'generates pseudo legal moves for queens or kings
    Private Function GenerateQueenOrKingMoves(ByVal board(,) As LogicSquare, ByVal type As Char, ByVal startSquare As LogicSquare) As List(Of Move)

        Dim queenOrKingMoves As New List(Of Move)

        Dim x As Integer = startSquare.X
        Dim y As Integer = startSquare.Y

        If y <> 7 Then queenOrKingMoves.AddRange(GenerateLateralMoves(board, type, startSquare, 7, "x")) 'north
        If x <> 7 And y <> 7 Then queenOrKingMoves.AddRange(GenerateDiagonalMoves(board, type, startSquare, 7, 7, 1, 1)) 'north east
        If x <> 7 Then queenOrKingMoves.AddRange(GenerateLateralMoves(board, type, startSquare, 7, "y")) 'east
        If x <> 7 And y <> 0 Then queenOrKingMoves.AddRange(GenerateDiagonalMoves(board, type, startSquare, 7, 0, 1, -1)) 'south east
        If y <> 0 Then queenOrKingMoves.AddRange(GenerateLateralMoves(board, type, startSquare, 0, "x")) 'south
        If x <> 0 And y <> 0 Then queenOrKingMoves.AddRange(GenerateDiagonalMoves(board, type, startSquare, 0, 0, -1, -1)) 'south west
        If x <> 0 Then queenOrKingMoves.AddRange(GenerateLateralMoves(board, type, startSquare, 0, "y")) 'west
        If x <> 0 And y <> 7 Then queenOrKingMoves.AddRange(GenerateDiagonalMoves(board, type, startSquare, 0, 7, -1, 1)) 'north west

        Return queenOrKingMoves

    End Function

    'generates pseudo legal moves for rooks
    Private Function GenerateRookMoves(ByVal board(,) As LogicSquare, ByVal type As Char, ByVal startSquare As LogicSquare) As List(Of Move)

        Dim rookMoves As New List(Of Move)

        Dim x As Integer = startSquare.X
        Dim y As Integer = startSquare.Y

        If y <> 7 Then rookMoves.AddRange(GenerateLateralMoves(board, type, startSquare, 7, "x"))   'north
        If x <> 7 Then rookMoves.AddRange(GenerateLateralMoves(board, type, startSquare, 7, "y"))   'east
        If y <> 0 Then rookMoves.AddRange(GenerateLateralMoves(board, type, startSquare, 0, "x"))   'south
        If x <> 0 Then rookMoves.AddRange(GenerateLateralMoves(board, type, startSquare, 0, "y"))   'west

        Return rookMoves

    End Function

    'generate pseudo legal moves in a single lateral direction
    Private Function GenerateLateralMoves(ByVal board(,) As LogicSquare, ByVal type As Char, ByVal startSquare As LogicSquare, ByVal finish As Integer, ByVal constant As String) As List(Of Move)
        Dim lateralMoves As New List(Of Move)

        Dim change, a, b As Integer

        Dim start As Integer

        'sets the 
        If constant = "x" Then
            start = startSquare.Y
            a = startSquare.X
        Else
            start = startSquare.X
            b = startSquare.Y
        End If

        'determines if the loop should increment or decrement
        change = If(start < finish, 1, -1)

        'gets the variable index for the first square to be checked
        start += change

        'if piece is a king then only check adjacent squares
        If type.ToString.ToUpper = "K" Then
            finish = start
        End If

        'searches through squares in a given direction
        For i = start To finish Step change

            'sets the variable indexes
            If constant = "x" Then b = i
            If constant = "y" Then a = i

            If board(a, b).Occupied Then
                If board(a, b).Colour <> startSquare.Colour Then
                    lateralMoves.Add(New Move(type, startSquare, board(a, b), board(a, b).Type, False, False, GV_NULL, GV_NULL))
                    'capture
                End If
                Exit For
            Else
                lateralMoves.Add(New Move(type, startSquare, board(a, b), board(a, b).Type, False, False, GV_NULL, GV_NULL))
                'move
            End If

        Next

        'checks for castling (something does not work when user is playing as black)
        If constant = "y" And ((type = "K" And GV_simulatedTurn = "w") Or (type = "k" And GV_simulatedTurn = "b")) And Not validating Then

            Dim accountForPerspective As Integer = If(GV_player1.Colour = "w", 1, -1)

            If (GV_simulatedTurn = "w" And GV_simulatedCastlingAvailability.Contains("K") And finish = 5) Or (GV_simulatedTurn = "b" And GV_simulatedCastlingAvailability.Contains("k") And finish = 2) Then
                'check kingside

                If Not SquareIsThreatened(board, If(GV_simulatedTurn = "w", "b", "w"), board(startSquare.X, startSquare.Y)) _   'if king isn't in check
                   And Not SquareIsThreatened(board, If(GV_simulatedTurn = "w", "b", "w"), board(startSquare.X + accountForPerspective, startSquare.Y)) _ 'next square isnt theatened
                   And Not board(startSquare.X + accountForPerspective, startSquare.Y).Occupied _   'next square isnt occupied
                   And Not SquareIsThreatened(board, If(GV_simulatedTurn = "w", "b", "w"), board(startSquare.X + (2 * accountForPerspective), startSquare.Y)) _ 'target square isnt threatened
                   And Not board(startSquare.X + (2 * accountForPerspective), startSquare.Y).Occupied Then  'target square isnt occupied

                    lateralMoves.Add(New Move(type, startSquare, board(startSquare.X + (2 * accountForPerspective), startSquare.Y), board(startSquare.X + (2 * accountForPerspective), startSquare.Y).Type, False, False, GV_NULL, If(type = "K", "K", "k")))

                End If

            End If

            If (GV_simulatedTurn = "w" And GV_simulatedCastlingAvailability.Contains("Q") And finish = 3) Or (GV_simulatedTurn = "b" And GV_simulatedCastlingAvailability.Contains("q") And finish = 4) Then
                'check queenside

                If Not SquareIsThreatened(board, If(GV_simulatedTurn = "w", "b", "w"), board(startSquare.X, startSquare.Y)) _   'if king isn't in check
                   And Not SquareIsThreatened(board, If(GV_simulatedTurn = "w", "b", "w"), board(startSquare.X - accountForPerspective, startSquare.Y)) _ 'next square isn't theatened
                   And Not board(startSquare.X - accountForPerspective, startSquare.Y).Occupied _   'next square isnt occupied
                   And Not SquareIsThreatened(board, If(GV_simulatedTurn = "w", "b", "w"), board(startSquare.X - (2 * accountForPerspective), startSquare.Y)) _ 'target square isn't threatened
                   And Not board(startSquare.X - (2 * accountForPerspective), startSquare.Y).Occupied _  'target square isn't occupied
                   And Not board(startSquare.X - (3 * accountForPerspective), startSquare.Y).Occupied Then  'square next to rook isn't occupied

                    lateralMoves.Add(New Move(type, startSquare, board(startSquare.X - (2 * accountForPerspective), startSquare.Y), board(startSquare.X - (2 * accountForPerspective), startSquare.Y).Type, False, False, GV_NULL, If(type = "K", "Q", "q")))

                End If

            End If

        End If

        Return lateralMoves

    End Function

    'generates pseudo legal moves for a bishop
    Private Function GenerateBishopMoves(ByVal board(,) As LogicSquare, ByVal type As Char, ByVal startSquare As LogicSquare) As List(Of Move)

        Dim bishopMoves As New List(Of Move)

        Dim x As Integer = startSquare.X
        Dim y As Integer = startSquare.Y

        If x <> 7 And y <> 7 Then bishopMoves.AddRange(GenerateDiagonalMoves(board, type, startSquare, 7, 7, 1, 1))     'north east
        If x <> 7 And y <> 0 Then bishopMoves.AddRange(GenerateDiagonalMoves(board, type, startSquare, 7, 0, 1, -1))    'south east
        If x <> 0 And y <> 0 Then bishopMoves.AddRange(GenerateDiagonalMoves(board, type, startSquare, 0, 0, -1, -1))   'south west
        If x <> 0 And y <> 7 Then bishopMoves.AddRange(GenerateDiagonalMoves(board, type, startSquare, 0, 7, -1, 1))    'north west

        Return bishopMoves

    End Function

    'Generates a list of pseudolegal diagonal moves in a given direction (NE/SE/SW/NW)
    Private Function GenerateDiagonalMoves(ByVal board(,) As LogicSquare, ByVal type As Char, ByVal startSquare As LogicSquare, ByVal xLimit As Integer, ByVal yLimit As Integer, ByVal xSign As Integer, ByVal ySign As Integer) As List(Of Move)

        Dim diagonalMoves As New List(Of Move)

        'incrementing indexes
        Dim a As Integer = startSquare.X
        Dim b As Integer = startSquare.Y

        'loops until either of the counters have reached the edge of the board (can no longer travel in this direction)
        Do Until (a = xLimit) Or (b = yLimit)

            a += xSign
            b += ySign

            If board(a, b).Occupied Then
                If board(a, b).Colour <> startSquare.Colour Then
                    diagonalMoves.Add(New Move(type, startSquare, board(a, b), board(a, b).Type, False, False, GV_NULL, GV_NULL))
                End If
                Exit Do
            Else
                diagonalMoves.Add(New Move(type, startSquare, board(a, b), board(a, b).Type, False, False, GV_NULL, GV_NULL))
            End If

            If type.ToString.ToUpper = "K" Then Exit Do
            'if king then only one iteration

        Loop

        Return diagonalMoves

    End Function

    'Generates a list of pseudolegal knight moves
    Private Function GenerateKnightMoves(ByVal board(,) As LogicSquare, ByVal type As Char, ByVal startSquare As LogicSquare) As List(Of Move)

        Dim knightMoves As New List(Of Move)

        'loops through all combinations of the L shape moves that the knight can make
        For i = -2 To 2
            For j = -2 To 2
                If (Math.Abs(i Mod 2)) <> (Math.Abs(j Mod 2)) And (i <> 0 And j <> 0) Then
                    knightMoves.AddRange(GenerateKnightMove(board, type, startSquare, i, j))
                End If
            Next
        Next

        Return knightMoves

    End Function

    ''' <summary>
    ''' Generates a single pseudolegal knight move
    ''' </summary>
    ''' <param name="board">The boardstate that the knight move will be searched for on</param>
    ''' <param name="type">The type of piece (case determines if it is black or white)</param>
    ''' <param name="startSquare">The square that the knight being checked currently occupies</param>
    ''' <param name="horizontal">How many squares to the left or right the knight is trying to move</param>
    ''' <param name="vertical">How many squares up or down that the knight is trying to move</param>
    ''' <returns>A list of pseudolegal knight moves (will only ever contain 0 or 1 moves)</returns>
    Private Function GenerateKnightMove(ByVal board(,) As LogicSquare, ByVal type As Char, ByVal startSquare As LogicSquare, ByVal horizontal As Integer, ByVal vertical As Integer) As List(Of Move)

        Dim knightMove As New List(Of Move)

        Dim a As Integer = startSquare.X + horizontal
        Dim b As Integer = startSquare.Y + vertical

        If a >= 0 AndAlso a <= 7 AndAlso b >= 0 AndAlso b <= 7 Then
            If board(a, b).Occupied Then
                If board(a, b).Colour <> startSquare.Colour Then
                    knightMove.Add(New Move(type, startSquare, board(a, b), board(a, b).Type, False, False, GV_NULL, GV_NULL))
                End If
            Else
                knightMove.Add(New Move(type, startSquare, board(a, b), board(a, b).Type, False, False, GV_NULL, GV_NULL))
            End If
        End If

        Return knightMove

    End Function

    ''' <summary>
    ''' [UNFINISHED] Generates pseudolegal moves for a pawn
    ''' </summary>
    ''' <param name="board">The boardstate that pawn moves will be searched for on</param>
    ''' <param name="type">The type of piece (case determines if it is white or black)</param>
    ''' <param name="startSquare">The square that the pawn being checked is currently occupying</param>
    ''' <returns>List of pseudolegal pawn moves</returns>
    Private Function GeneratePawnMoves(ByVal board(,) As LogicSquare, ByVal type As Char, ByVal startSquare As LogicSquare) As List(Of Move)

        Dim pawnMoves As New List(Of Move)

        Dim x As Integer = startSquare.X
        Dim y As Integer = startSquare.Y

        Dim accountForPerspective, max, min As Integer

        accountForPerspective = If(IsPlayerOneColour(type), 1, -1)

        max = If(x = 7, 7, x + 1)

        min = If(x = 0, 0, x - 1)

        If Not ((IsPlayerOneColour(type) And y = 7) Or (Not IsPlayerOneColour(type) And y = 0)) Then

            For i = min To max

                If board(i, y + accountForPerspective).Occupied Then

                    If i <> x Then

                        If board(i, y + accountForPerspective).Colour <> startSquare.Colour Then

                            If (IsPlayerOneColour(type) And y + accountForPerspective = 7) Or (Not IsPlayerOneColour(type) And y + accountForPerspective = 0) Then

                                pawnMoves.Add(New Move(type, startSquare, board(i, y + accountForPerspective), board(i, y + accountForPerspective).Type, False, False, If(type = "P", "Q", "q"), GV_NULL))
                                pawnMoves.Add(New Move(type, startSquare, board(i, y + accountForPerspective), board(i, y + accountForPerspective).Type, False, False, If(type = "P", "R", "r"), GV_NULL))
                                pawnMoves.Add(New Move(type, startSquare, board(i, y + accountForPerspective), board(i, y + accountForPerspective).Type, False, False, If(type = "P", "B", "b"), GV_NULL))
                                pawnMoves.Add(New Move(type, startSquare, board(i, y + accountForPerspective), board(i, y + accountForPerspective).Type, False, False, If(type = "P", "N", "n"), GV_NULL))
                                'capture and promote (4 moves as it can promote into 4 different pieces)

                            Else

                                pawnMoves.Add(New Move(type, startSquare, board(i, y + accountForPerspective), board(i, y + accountForPerspective).Type, False, False, GV_NULL, GV_NULL))
                                'capture

                            End If


                        End If

                    End If

                Else

                    If i = x Then

                        If (IsPlayerOneColour(type) And y + accountForPerspective = 7) Or (Not IsPlayerOneColour(type) And y + accountForPerspective = 0) Then

                            pawnMoves.Add(New Move(type, startSquare, board(i, y + accountForPerspective), board(i, y + accountForPerspective).Type, False, False, If(type = "P", "Q", "q"), GV_NULL))
                            pawnMoves.Add(New Move(type, startSquare, board(i, y + accountForPerspective), board(i, y + accountForPerspective).Type, False, False, If(type = "P", "R", "r"), GV_NULL))
                            pawnMoves.Add(New Move(type, startSquare, board(i, y + accountForPerspective), board(i, y + accountForPerspective).Type, False, False, If(type = "P", "B", "b"), GV_NULL))
                            pawnMoves.Add(New Move(type, startSquare, board(i, y + accountForPerspective), board(i, y + accountForPerspective).Type, False, False, If(type = "P", "N", "n"), GV_NULL))
                            'advance and promote (4 moves as it can promote into 4 different pieces)

                        Else

                            pawnMoves.Add(New Move(type, startSquare, board(i, y + accountForPerspective), board(i, y + accountForPerspective).Type, False, False, GV_NULL, GV_NULL))
                            'advance

                        End If

                    Else

                        If GV_simulatedEnPassantTargetSquare IsNot Nothing Then

                            If i = GV_simulatedEnPassantTargetSquare.X And y + accountForPerspective = GV_simulatedEnPassantTargetSquare.Y Then
                                'If board(i, y + accountForPerspective) Is GV_simulatedEnPassantTargetSquare Then

                                pawnMoves.Add(New Move(type, startSquare, board(i, y + accountForPerspective), board(i, y).Type, False, True, GV_NULL, GV_NULL))
                                'en passant capture

                            End If

                        End If

                    End If

                End If

            Next

            If (IsPlayerOneColour(type) And y = 1) Or (Not IsPlayerOneColour(type) And y = 6) Then

                If Not board(x, y + accountForPerspective).Occupied AndAlso Not board(x, y + (2 * accountForPerspective)).Occupied Then

                    pawnMoves.Add(New Move(type, startSquare, board(x, y + (2 * accountForPerspective)), board(x, y + (2 * accountForPerspective)).Type, True, False, GV_NULL, GV_NULL))
                    'double advance

                End If

            End If

        End If

        Return pawnMoves

    End Function

    ''' <summary>
    ''' Checks if a piece belongs to Player1 (The human player).
    ''' </summary>
    ''' <param name="type">Type of piece (uppercase = White | lowercase = Black)</param>
    ''' <returns>True if piece belongs to PLayer1. False if piece does not belong to Player1.</returns>
    Public Function IsPlayerOneColour(ByVal type As Char) As Boolean

        If (Asc(type) <= 90 And GV_player1.Colour = "w") Or (Asc(type) >= 97 And GV_player1.Colour = "b") Then Return True
        Return False

    End Function

    ''' <summary>
    ''' [UNFINISHED] Simulates making a move on an abstract board
    ''' </summary>
    ''' <param name="board">Board that the move is being made on</param>
    ''' <param name="move">The move to be made</param>
    Private Sub MakeMove(ByVal board(,) As LogicSquare, ByVal move As Move)

        'get startSquare
        'find piece that has the same start square
        'move piece to the target square

        board(move.StartSquare.X, move.StartSquare.Y).Type = GV_NULL
        board(move.TargetSquare.X, move.TargetSquare.Y).Type = move.Type

        Dim accountForPerspective As Integer

        If move.EnPassant Then
            accountForPerspective = If(IsPlayerOneColour(move.Type), -1, 1)
            board(move.TargetSquare.X, move.TargetSquare.Y + accountForPerspective).Type = GV_NULL
        End If

        If move.Promotion <> GV_NULL Then
            board(move.TargetSquare.X, move.TargetSquare.Y).Type = move.Promotion
        End If

        If move.DoubleAdvance Then
            GV_simulatedEnPassantTargetSquare = board(move.TargetSquare.X, move.TargetSquare.Y + accountForPerspective)
        Else
            GV_simulatedEnPassantTargetSquare = Nothing
        End If

        'if move being simulated is a king or a rook
        If move.Type.ToString.ToUpper = "R" Or move.Type.ToString.ToUpper = "K" Then

            For Each piece In GV_castleHandling

                'finds the corresponding element in CastleHandling
                If piece.CurrentSquare.X = move.StartSquare.X And piece.CurrentSquare.Y = move.StartSquare.Y Then

                    'updates current location of piece
                    piece.CurrentSquare = move.TargetSquare

                    'increments simulated move counter
                    piece.SimulatedMoveCounter += 1

                    For Each control In piece.CastlingControl

                        'revokes castling rights (the rights that are lost change with each type of piece)
                        If GV_simulatedCastlingAvailability.Contains(control) Then
                            GV_simulatedCastlingAvailability = GV_simulatedCastlingAvailability.Replace(control, "")
                        End If

                    Next

                    Exit For

                End If

            Next

        End If

        If move.Castling <> GV_NULL Then

            If GV_player1.Colour = "w" Then

                If move.TargetSquare.X = 6 Then
                    'kingside
                    board(7, move.StartSquare.Y).Type = GV_NULL
                    board(5, move.StartSquare.Y).Type = "R"
                ElseIf move.TargetSquare.X = 2 Then
                    'queenside
                    board(0, move.StartSquare.Y).Type = GV_NULL
                    board(3, move.StartSquare.Y).Type = "R"
                End If

            Else

                If move.TargetSquare.X = 1 Then
                    'kingside
                    board(0, move.StartSquare.Y).Type = GV_NULL
                    board(2, move.StartSquare.Y).Type = "R"
                ElseIf move.TargetSquare.X = 5 Then
                    'queenside
                    board(7, move.StartSquare.Y).Type = GV_NULL
                    board(4, move.StartSquare.Y).Type = "R"
                End If

            End If

        End If

        If move.Type.ToString.ToUpper = "P" Or move.Capture <> GV_NULL Then
            'if capture or pawn move
            GV_simulatedHalfmoveClock = 0
        Else
            GV_simulatedHalfmoveClock += 1
        End If
        halfmoveStack.Push(GV_simulatedHalfmoveClock)

        If Asc(move.Type) > 90 Then GV_simulatedFullmoveCounter += 1

    End Sub

    ''' <summary>
    ''' Simulates unmaking a move on an abstract board
    ''' </summary>
    ''' <param name="board">Board that the move is being unmade on</param>
    ''' <param name="move">The move to be made</param>
    Private Sub UnmakeMove(ByVal board(,) As LogicSquare, ByVal move As Move)

        'moves piece back to start square, if a piece was captured then it is reinstated on the target square
        board(move.TargetSquare.X, move.TargetSquare.Y).Type = move.Capture
        board(move.StartSquare.X, move.StartSquare.Y).Type = move.Type

        Dim accountForPerspective As Integer

        'reinstate the en passant target square (if the move was an en passant capture)
        If move.EnPassant Then
            accountForPerspective = If(IsPlayerOneColour(move.Type), -1, 1)
            board(move.TargetSquare.X, move.TargetSquare.Y).Type = GV_NULL
            board(move.TargetSquare.X, move.TargetSquare.Y + accountForPerspective).Type = move.Capture
            GV_simulatedEnPassantTargetSquare = move.TargetSquare
        Else
            GV_simulatedEnPassantTargetSquare = Nothing
        End If

        'if piece is a castling piece then check if castling rights can be reinstated
        If move.Type.ToString.ToUpper = "R" Or move.Type.ToString.ToUpper = "K" Then

            'look through all castling pieces
            For Each piece In GV_castleHandling

                'check if the castling piece is the piece whose move is being unmade
                If piece.CurrentSquare.X = move.TargetSquare.X And piece.CurrentSquare.Y = move.TargetSquare.Y Then

                    'move piece back to start square
                    piece.CurrentSquare = move.StartSquare

                    'decrements the simulated move counter
                    piece.SimulatedMoveCounter -= 1

                    'gets indexes of the piece in castle handling array
                    Dim x As Integer = piece.X
                    Dim y As Integer = piece.Y

                    'if the piece is a king
                    If x = 1 Then

                        'check if the piece is in its initial starting position (from the beginning of the game)
                        If piece.IsAtStart() Then

                            'checks the two rooks of the same colour
                            For rookIndex = 0 To 2 Step 2

                                'if the rook is in its initial starting position
                                If GV_castleHandling(y, rookIndex).IsAtStart() Then

                                    'for all the types of castling that the pieces control
                                    For Each castleControl In GV_castleHandling(y, rookIndex).CastlingControl

                                        'if the control is not in castling availability then reinstate it
                                        If Not GV_simulatedCastlingAvailability.Contains(castleControl) And GV_castlingAvailability.Contains(castleControl) Then
                                            'adds a castle back, but not one that is gone permanently as an actual move has been made

                                            GV_simulatedCastlingAvailability += castleControl

                                        End If

                                    Next castleControl

                                End If

                            Next rookIndex

                        End If

                    Else    'of the piece is a rook

                        'if piece is in initial starting position
                        If piece.IsAtStart() Then

                            'if the king of the same colour is also in its starting position
                            If GV_castleHandling(y, 1).IsAtStart() Then

                                'loop through the types of castling that are controlled by the pieces
                                For Each castleControl In GV_castleHandling(y, x).CastlingControl

                                    'if control is not in castling availability
                                    If Not GV_simulatedCastlingAvailability.Contains(castleControl) And GV_castlingAvailability.Contains(castleControl) Then

                                        'castling is reinstated
                                        GV_simulatedCastlingAvailability += castleControl

                                    End If

                                Next castleControl

                            End If

                        End If

                    End If

                    Exit For

                End If

            Next piece

        End If

        'formats castling availability (in the form KQkq)
        GV_simulatedCastlingAvailability = FormattingAlgorithms.SortString(GV_simulatedCastlingAvailability)

        If move.Castling <> GV_NULL Then

            If GV_player1.Colour = "w" Then

                If move.TargetSquare.X = 6 Then
                    'kingside
                    board(7, move.StartSquare.Y).Type = "R"
                    board(5, move.StartSquare.Y).Type = GV_NULL
                ElseIf move.TargetSquare.X = 2 Then
                    'queenside
                    board(0, move.StartSquare.Y).Type = "R"
                    board(3, move.StartSquare.Y).Type = GV_NULL
                End If

            Else

                If move.TargetSquare.X = 1 Then
                    'kingside
                    board(0, move.StartSquare.Y).Type = "R"
                    board(2, move.StartSquare.Y).Type = GV_NULL
                ElseIf move.TargetSquare.X = 5 Then
                    'queenside
                    board(7, move.StartSquare.Y).Type = "R"
                    board(4, move.StartSquare.Y).Type = GV_NULL
                End If

            End If

        End If

        'gets simulated halfmove clock from halfmove stack
        If move.Type.ToString.ToUpper <> "P" And move.Capture <> GV_NULL Then
            GV_simulatedHalfmoveClock -= 1
        Else
            GV_simulatedHalfmoveClock = halfmoveStack.Pop()
        End If

        'if move being unmade is black, decrement simulated fullmove counter
        If Asc(move.Type) > 90 Then GV_simulatedFullmoveCounter -= 1

    End Sub

End Module