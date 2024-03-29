Public Class Form1

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        'sets the value that form, board, pieces and text will be scaled from
        GV_scalar = 110

        'stores the size that all the pieces will be
        GV_standardSize = New Size(GV_scalar, GV_scalar)

        'allows the user to drag pieces
        GV_draggingEnabled = True

        'sets background colour, size and title of the form
        BackColor = GV_formBackgroundColour
        Size = New Size((GV_scalar * 11) + 16, (GV_scalar * 10.1) + 36)
        Text = "Chess"

        'stops user from being able to change the size of the form
        FormBorderStyle = FormBorderStyle.Fixed3D

        'puts form in the centre of the screen by default
        CenterToScreen()

        InitialisePlayers(True) 'creates two players
        InitialiseLogicBoard()  'initialises elements of GV_LogicBoard
        InitialisePieces() 'creates 32 new pieces with no type, stores them in GV_piecesOffBoard
        InitialisePromotionPieces() 'creates 4 new pieces with no type, stores them in GV_promotionPieces
        LoadPieces()    'reads data from FEN string, moves pieces to GV_piecesOnBoard and gives them a type
        GV_kingInCheck = MoveThreatensKing(GV_logicBoard, If(GV_turn = "w", "b", "w"))  'stores if the king is in check
        SpawnPieces()   'adds piece pictureboxes (of pieces in GV_piecesOnBoard) onto the form
        LoadPromotionPieces()   'gives types to the four promotion pieces depending on the colour that the user is playing as
        InitialiseRealBoard()   'stores location of each square, draws the board image and loads the board picturebox onto the form
        InitialiseTree()    'creates root node of the board state tree
        InitialiseTurnIndictators() 'creates two labels and adds them to the form
        InitialiseLabels()  'creates 16 labels to show the ranks and files
        StartGame() 'starts the game

    End Sub

    'creates 16 labels to show the ranks and files
    Private Sub InitialiseLabels()

        'creates 16 labels
        For i = 0 To 7

            'initialises labels for ranks (numbers)
            GV_rankLabels(i) = New Label With {
                .Size = New Size(GV_scalar / 3, GV_scalar / 2),
                .Location = New Point(GV_scalar * 0.65, GV_scalar * (i + 1)),
                .Font = New Font("Calibri", GV_scalar / 4),
                .ForeColor = Color.White
            }

            'initialises labels for files (letters)
            GV_fileLabels(i) = New Label With {
                .Size = New Drawing.Size(GV_scalar / 3, GV_scalar / 2),
                .Location = New Point(GV_scalar * (i + 1) + GV_scalar * 0.65, GV_scalar * 9.02),
                .Font = New Font("Calibri", GV_scalar / 4),
                .ForeColor = Color.White
            }

            'puts text in labels
            If GV_player1.Colour = "w" Then
                GV_rankLabels(i).Text = (8 - i).ToString
                GV_fileLabels(i).Text = Chr(i + 97)
            Else
                GV_rankLabels(i).Text = (i + 1).ToString
                GV_fileLabels(i).Text = Chr(104 - i)
            End If

        Next

        'adds labels to the form
        Me.Controls.AddRange(GV_rankLabels)
        Me.Controls.AddRange(GV_fileLabels)

    End Sub

    'creates two labels and adds them to the form
    Private Sub InitialiseTurnIndictators()

        'sets label data
        With GV_player1.TurnIndicator
            .Size = New Size(GV_scalar * 1.5, GV_scalar * 0.47)
            .Location = New Point(9.25 * GV_scalar, 9 * GV_scalar - .Height)
            .Text = "PLAYER"
            .TextAlign = ContentAlignment.MiddleCenter
            .Font = New Font("Calibri", GV_scalar / 5)
            .ForeColor = Color.White
        End With

        'sets label data
        With GV_player2.TurnIndicator
            .Size = New Size(GV_scalar * 1.5, GV_scalar * 0.47)
            .Location = New Point(9.25 * GV_scalar, GV_scalar)
            .Text = "COMPUTER"
            .TextAlign = ContentAlignment.MiddleCenter
            .Font = New Font("Calibri", GV_scalar / 5)
            .ForeColor = Color.White
        End With

        'adds labels to form
        Me.Controls.AddRange({GV_player1.TurnIndicator, GV_player2.TurnIndicator})

    End Sub

    'switches turn and updates turn indicator labels
    Public Sub SwitchTurn()

        'switches turn
        GV_turn = If(GV_turn = "w", "b", "w")

        'highlights the label of the player whose turn it is
        If GV_player1.Colour = GV_turn Then
            GV_player2.IsNotYourTurn()
            GV_player1.IsYourTurn()
        Else
            GV_player1.IsNotYourTurn()
            GV_player2.IsYourTurn()
        End If

    End Sub

    'When a new board is loaded, either generates moves for the player or the computer makes a move
    Private Sub StartGame()

        'if it is the user's turn, generates moves for them
        If GV_turn = GV_player1.Colour Then
            GV_player1.IsYourTurn()
            GetMovesForSpecificColour(GV_player1.Colour)
        Else    'if it is the computer's turn, finds and makes its best move
            GV_player2.IsYourTurn()
            AutoPlay()
        End If

    End Sub

    'creates root node of the board state tree
    Private Sub InitialiseTree()

        'creates root node
        GV_boardStateTree = New BoardState(GV_logicBoard, "in progress")

        'assigns initial game state data
        With GV_boardStateTree
            .turn = GV_simulatedTurn
            .castlingAvailability = GV_simulatedCastlingAvailability
            .enPassantTargetSquare = If(GV_simulatedEnPassantTargetSquare IsNot Nothing, GV_logicBoard(GV_simulatedEnPassantTargetSquare.X, GV_simulatedEnPassantTargetSquare.Y), Nothing)
            .halfmoveClock = GV_simulatedHalfmoveClock
            .fullmoveCounter = GV_simulatedFullmoveCounter
        End With

    End Sub

    Sub AutoPlay()

        MP_startDepth = 6   'how deep the search will be
        MP_GenerateMoves(GV_boardStateTree, MP_startDepth, Double.NegativeInfinity, Double.PositiveInfinity, GV_turn <> GV_player1.Colour)
        'the computer will always be the maximising player, so maximisingPlayer = True if it is the computer's turn
        Dim bestMove As Move = FindBestMove(GV_boardStateTree)
        If bestMove IsNot Nothing Then  'if a move is found, make the move
            MakeActualMove(bestMove, False, False)
            GV_kingInCheck = bestMove.CausesCheck
        Else    'if no move, game ends
            GV_boardStateTree.gameState = CheckWhyGameIsOver(GV_boardStateTree.layout, True)
        End If

    End Sub

    'returns the best move
    Private Function FindBestMove(currentBoardState As BoardState)

        'if 50 move rule condition is met then no valid move
        If GV_halfmoveClock = 100 Then Return Nothing

        Dim bestMoves As New List(Of Move)

        'filters all moves that lead to best outcome
        For Each child In currentBoardState.children
            If child.evalNumber = currentBoardState.evalNumber Then
                bestMoves.Add(child.moveToGetThisBoard)
            End If
        Next

        'if no move leads to best outcome then return nothing
        If bestMoves.Count = 0 Then Return Nothing

        'if only one move that leads to best outcome then make that move
        If bestMoves.Count = 1 Then Return bestMoves.First

        Dim randomNumber As New Random
        Dim x As Integer = randomNumber.Next(0, bestMoves.Count)

        'if multiple moves that lead to best outcome, chooose one randomly
        Return bestMoves(x)

    End Function

    'updates logic board and moves pieces on real board
    Sub MakeActualMove(ByVal move As Move, clicked As Boolean, player As Boolean)

        'accounts for the direction pawns move relative to the colour that the user is playing as
        Dim accountForPerspective As Integer
        accountForPerspective = If(IsPlayerOneColour(move.Type), -1, 1)

        'this is the part of the code that is skipped if the subroutine is being called as the second part of a pawn promotion
        If Not GV_promotionToHappen Then

            'makes start square empty and changes target square to the type of piece moving (makes move on logic board)
            GV_logicBoard(move.StartSquare.X, move.StartSquare.Y).Type = GV_NULL
            GV_logicBoard(move.TargetSquare.X, move.TargetSquare.Y).Type = move.Type

            'if move is a capture
            If move.Capture <> GV_NULL Then

                'if move is not an en passant capture
                If Not move.EnPassant Then

                    'remove piece on target square from board and move piece from on board list to off board list
                    Me.Controls.Remove(GV_realBoard(move.TargetSquare.X, move.TargetSquare.Y).Piece.PBX)
                    GV_piecesOffBoard.Add(GV_realBoard(move.TargetSquare.X, move.TargetSquare.Y).Piece)
                    GV_piecesOnBoard.Remove(GV_realBoard(move.TargetSquare.X, move.TargetSquare.Y).Piece)

                End If

            End If

            'if move is being made either by the player clicking on the target square (not dragging) or by the computer
            If clicked Or Not player Then

                'move the piece picturebox to the target location
                If GV_realBoard(move.StartSquare.X, move.StartSquare.Y).Piece IsNot Nothing Then
                    GV_realBoard(move.StartSquare.X, move.StartSquare.Y).Piece.PBX.Location = GV_realBoard(move.TargetSquare.X, move.TargetSquare.Y).Point
                    GV_realBoard(move.TargetSquare.X, move.TargetSquare.Y).Piece.SetBackColour()
                End If

            End If

            'if it is an en passant move then remove the piece that has been captured from the board, and move it to the off board list
            If move.EnPassant Then
                GV_logicBoard(move.TargetSquare.X, move.TargetSquare.Y + accountForPerspective).Type = GV_NULL

                Me.Controls.Remove(GV_realBoard(move.TargetSquare.X, move.TargetSquare.Y + accountForPerspective).Piece.PBX)
                GV_piecesOffBoard.Add(GV_realBoard(move.TargetSquare.X, move.TargetSquare.Y + accountForPerspective).Piece)
                GV_piecesOnBoard.Remove(GV_realBoard(move.TargetSquare.X, move.TargetSquare.Y + accountForPerspective).Piece)

            End If

            'if the move is a promotion
            If move.Promotion <> GV_NULL Then

                'if it is the user's move, display promotion pieces and exit sub
                If GV_turn = GV_player1.Colour Then
                    SpawnPromotionPieces(move.TargetSquare.X)
                    GV_promotionToHappen = True
                    Return
                Else    'if it is the computer's move, it will change the pawn to the promotion piece automatically
                    GV_logicBoard(move.TargetSquare.X, move.TargetSquare.Y).Type = move.Promotion
                    GV_realBoard(move.TargetSquare.X, move.TargetSquare.Y).Piece.Type = move.Promotion
                End If

            End If

        Else    'if promotion is about to happen

            'pawn changes to the type of piece selected by the user
            GV_logicBoard(move.TargetSquare.X, move.TargetSquare.Y).Type = move.Promotion

        End If

        'if move is a double advance then store the en passant target square for the next move
        If move.DoubleAdvance Then
            GV_enPassantTargetSquare = GV_logicBoard(move.TargetSquare.X, move.TargetSquare.Y + accountForPerspective)
        Else
            GV_enPassantTargetSquare = Nothing
        End If

        'if piece being moved is a king or king then update castling rights
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
                        If GV_castlingAvailability.Contains(control) Then
                            GV_castlingAvailability = GV_castlingAvailability.Replace(control, "")
                        End If

                    Next

                    Exit For

                End If

            Next

        End If

        If GV_castlingAvailability = "" Then GV_castlingAvailability = "-"

        'if move is a castling move
        If move.Castling <> GV_NULL Then

            If GV_player1.Colour = "w" Then

                If move.TargetSquare.X = 6 Then
                    'kingside
                    GV_logicBoard(7, move.StartSquare.Y).Type = GV_NULL
                    GV_logicBoard(5, move.StartSquare.Y).Type = "R"
                    GV_realBoard(7, move.StartSquare.Y).Piece.PBX.Location = GV_realBoard(5, move.StartSquare.Y).Point
                ElseIf move.TargetSquare.X = 2 Then
                    'queenside
                    GV_logicBoard(0, move.StartSquare.Y).Type = GV_NULL
                    GV_logicBoard(3, move.StartSquare.Y).Type = "R"
                    GV_realBoard(0, move.StartSquare.Y).Piece.PBX.Location = GV_realBoard(3, move.StartSquare.Y).Point
                End If

            Else

                If move.TargetSquare.X = 1 Then
                    'kingside
                    GV_logicBoard(0, move.StartSquare.Y).Type = GV_NULL
                    GV_logicBoard(2, move.StartSquare.Y).Type = "R"
                    GV_realBoard(0, move.StartSquare.Y).Piece.PBX.Location = GV_realBoard(2, move.StartSquare.Y).Point
                ElseIf move.TargetSquare.X = 5 Then
                    'queenside
                    GV_logicBoard(7, move.StartSquare.Y).Type = GV_NULL
                    GV_logicBoard(4, move.StartSquare.Y).Type = "R"
                    GV_realBoard(7, move.StartSquare.Y).Piece.PBX.Location = GV_realBoard(4, move.StartSquare.Y).Point
                End If

            End If

        End If

        If move.Type.ToString.ToUpper = "P" Or move.Capture <> GV_NULL Then
            'if capture or pawn move
            GV_halfmoveClock = 0
        Else
            GV_halfmoveClock += 1
        End If

        'if black made move then increment fullmove counter
        If Asc(move.Type) > 90 Then GV_fullmoveCounter += 1

        'create new tree for the next move search
        GV_boardStateTree = New BoardState(GV_logicBoard, "in progress")

        'calculate if there is a king in check
        GV_kingInCheck = MoveThreatensKing(GV_logicBoard, GV_turn)

        'prepare game state information for next move
        SwitchTurn()
        ResetSimulatingVariables()

        'reset background colour of pieces
        For Each piece In GV_piecesOnBoard
            piece.SetBackColour()
        Next

        'find all legal moves for the next turn
        GV_movesForThisTurn = GetMovesForSpecificColour(GV_turn)

        'if there are no legal moves then check why game is over
        If GV_movesForThisTurn = 0 Then

            Select Case CheckWhyGameIsOver(GV_logicBoard, True)
                Case "checkmate"
                    If GV_turn = "b" Then
                        MsgBox("White wins!")
                    Else
                        MsgBox("Black wins!")
                    End If

                Case "stalemate", "50 move rule"
                    MsgBox("Draw!")

            End Select

        End If

    End Sub

    Private Sub InitialisePlayers(ByVal playerOneWhite As Boolean)

        GV_player1 = New Player(If(playerOneWhite, "w", "b"))
        GV_player2 = New Player(If(playerOneWhite, "b", "w"))

    End Sub

    Private Sub InitialiseRealBoard()

        GV_boardPBX = New PictureBox With {
            .Size = New Size(8 * GV_scalar, 8 * GV_scalar),
            .Location = New Point(GV_scalar, GV_scalar)
        }

        GV_boardBitmap = New Bitmap(8 * GV_scalar, 8 * GV_scalar)

        GV_gfx = Graphics.FromImage(GV_boardBitmap)

        For y = 0 To 7
            For x = 0 To 7
                GV_realBoard(x, y) = New Square(New Point(GV_scalar * (x + 1), GV_scalar * (8 - y)), False)
            Next x
        Next y

        GV_boardPBX.Image = GV_boardBitmap
        Me.Controls.Add(GV_boardPBX)

        AddHandler GV_boardPBX.Click, AddressOf BoardClick

    End Sub

    'initialises elements of logic board array
    Private Sub InitialiseLogicBoard()

        For y = 0 To 7
            For x = 0 To 7
                GV_logicBoard(x, y) = New LogicSquare(GV_NULL, x, y)
            Next
        Next

    End Sub

    'creates 32 new pieces with no type, stores them in GV_piecesOffBoard
    Private Sub InitialisePieces()

        For i = 0 To 31
            GV_piecesOffBoard.Add(New Piece(GV_NULL, False))
        Next

    End Sub

    'creates 4 new pieces with no type, stores them in GV_promotionPieces
    Private Sub InitialisePromotionPieces()

        For i = 0 To 3
            GV_promotionPieces.Add(New Piece(GV_NULL, True))
        Next

    End Sub

    'reads data from FEN string, moves pieces to GV_piecesOnBoard and gives them a type
    Private Sub LoadPieces()

        Dim FENstring As String = ""

        'sets FEN string depending on the colour that the user is playing as (user's pieces at the bottom of the board)
        If GV_player1.Colour = "w" Then
            FENstring = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
            'FENstring = "K7/6P1/2r5/8/4Q2Q/8/brp5/1k2n2Q w - - 0 1"
        Else
            FENstring = "RNBKQBNR/PPPPPPPP/8/8/8/8/pppppppp/rnbkqbnr w KQkq - 0 1"
        End If

        'splits FEN string into array, each element storing one piece of information
        Dim FEN() As String = FENstring.Split(" ")

        Dim FEN_Board() As String = FEN(0).Split("/")   'splits the board layout into each row on the board

        'stores game state data from FEN array
        GV_turn = FEN(1)
        GV_castlingAvailability = FEN(2)
        GV_enPassantTargetSquare = FormattingAlgorithms.AlphaNumericToSquare(FEN(3))
        GV_halfmoveClock = FEN(4)
        GV_fullmoveCounter = FEN(5)

        'sets the simulating variables to be the same as the actual game state information
        ResetSimulatingVariables()

        Dim fileCount As Integer = 0
        Dim rankCount As Integer = 7

        Dim counter As Integer = 0

        'iterates through layout data and assigns pieces types and locations
        For Each rank In FEN_Board
            For Each file In rank
                If Asc(file) >= 49 And Asc(file) <= 56 Then
                    For i = 1 To Asc(file) - 48
                        fileCount = (fileCount + 1) Mod 8
                        If fileCount = 0 Then
                            rankCount -= 1
                        End If
                    Next
                Else
                    GV_logicBoard(fileCount, rankCount).Type = file

                    With GV_piecesOffBoard(counter)
                        .Type = file
                        .PBX.Location = New Point((fileCount + 1) * GV_scalar, (8 - rankCount) * GV_scalar)
                    End With

                    counter += 1
                    fileCount = (fileCount + 1) Mod 8
                    If fileCount = 0 Then
                        rankCount -= 1
                    End If

                End If
            Next file
        Next rank

        'puts data in castle handler array
        For colour = 0 To 1 'white => black
            For type = 0 To 2   'rook => king => rook
                GV_castleHandling(colour, type) = New CastleHandler(GV_logicBoard(GetCastlingX(type), If(colour = 0, 0, 7)), GetCastlingControl(colour, type)) With {
                    .X = type,
                    .Y = colour
                }
                'if autosaving is implemented, this data will be stored and read from a .txt file
            Next
        Next

    End Sub

    Private Sub SpawnPieces()
        'moves pieces from OffBoard list to OnBoard list
        'spawns pieces from OnBoard into the board

        For i = 31 To 0 Step -1
            If GV_piecesOffBoard(i).Type <> GV_NULL Then
                GV_piecesOnBoard.Add(GV_piecesOffBoard(i))
                GV_piecesOffBoard.RemoveAt(i)
            End If
        Next

        For Each piece In GV_piecesOnBoard
            piece.SetBackColour()
            Me.Controls.Add(piece.PBX)
        Next

    End Sub

    Public Sub ResetSimulatingVariables()

        GV_simulatedTurn = GV_turn
        GV_simulatedCastlingAvailability = GV_castlingAvailability
        GV_simulatedEnPassantTargetSquare = GV_enPassantTargetSquare
        GV_simulatedHalfmoveClock = GV_halfmoveClock
        GV_simulatedFullmoveCounter = GV_fullmoveCounter

    End Sub

    'selects the type of pieces that the player will be able to promote to
    Private Sub LoadPromotionPieces()

        Dim possiblePromotionPiecesArray() As Char = {"Q", "N", "R", "B", "q", "n", "r", "b"}
        Dim v As Integer

        For i = 0 To 3
            v = If(GV_player1.Colour = "b", i + 4, i)
            GV_promotionPieces(i).Type = possiblePromotionPiecesArray(v)
        Next


    End Sub

    'add promotion pieces to the form
    Private Sub SpawnPromotionPieces(x As Integer)

        For i = 0 To 3

            With GV_promotionPieces(i)

                .PBX.Location = New Point(GV_scalar * (x + 1), GV_scalar * (i + 1))
                .PBX.BackColor = Color.LightGray
                .PBX.Refresh()
                Me.Controls.Add(.PBX)
                .PBX.BringToFront()

            End With

        Next

    End Sub

    'remove promotion pieces from the form
    Public Sub DespawnPromotionPieces()

        For i = 0 To 3
            With GV_promotionPieces(i)
                Me.Controls.Remove(.PBX)
            End With
        Next

    End Sub

    'handles when the user clicks the board
    Private Sub BoardClick(sender As PictureBox, e As MouseEventArgs)

        'the move that will be made by the user
        Dim moveToMake As Move = Nothing

        'gets the indexes of the square that is being clicked
        Dim x As Integer = e.X \ GV_scalar
        Dim y As Integer = 7 - e.Y \ GV_scalar

        'if there is a selected piece and you click on an opposing piece to capture it
        If GV_selectedPiece IsNot Nothing Then

            'find the selected piece's move that's target square is the one that has been clicked
            For Each possibleMove In GV_selectedPiece.validMoves
                If x = possibleMove.TargetSquare.X And y = possibleMove.TargetSquare.Y Then
                    moveToMake = possibleMove
                    Exit For
                End If
            Next

        End If

        'if there is a move to make
        If moveToMake IsNot Nothing Then

            'make move
            MakeActualMove(moveToMake, True, True)

            'if not a promotion
            If moveToMake.Promotion = GV_NULL Then

                'reset data for the selected piece for the next move
                GV_selectedPiece.HideValidMoves()
                GV_selectedPiece.validMoves.Clear()
                GV_selectedPiece.startX = x
                GV_selectedPiece.startY = y
                GV_selectedPiece = Nothing

                'reset all other pieces on the board for the next move
                For Each piece In GV_piecesOnBoard
                    piece.ResetForNextMove()
                Next

                'updates board and makes the computer play a move
                Application.DoEvents()
                AutoPlay()

            Else    'if the move is promotion then store the move
                GV_selectedPiece.nextMove = moveToMake
            End If

        End If

    End Sub

    'Gets the x index of a rook/castle for castle handling based on the type of piece it will be handling
    Private Function GetCastlingX(ByVal CastleHandlingYIndex As Integer) As Integer

        If CastleHandlingYIndex = 0 Then Return 0
        If CastleHandlingYIndex = 2 Then Return 7
        If GV_player1.Colour = "w" Then Return 4
        Return 3

    End Function

    'gets what types of castling a piece can control
    Private Function GetCastlingControl(ByVal x As Integer, ByVal y As Integer) As List(Of String)

        Dim castleControlList As New List(Of String)

        If x = 0 And y = 0 Then castleControlList.AddRange(If(GV_player1.Colour = "w", {"Q"}, {"k"}))
        If x = 0 And y = 1 Then castleControlList.AddRange(If(GV_player1.Colour = "w", {"K", "Q"}, {"k", "q"}))
        If x = 0 And y = 2 Then castleControlList.AddRange(If(GV_player1.Colour = "w", {"K"}, {"q"}))
        If x = 1 And y = 0 Then castleControlList.AddRange(If(GV_player1.Colour = "w", {"q"}, {"K"}))
        If x = 1 And y = 1 Then castleControlList.AddRange(If(GV_player1.Colour = "w", {"k", "q"}, {"K", "Q"}))
        If x = 1 And y = 2 Then castleControlList.AddRange(If(GV_player1.Colour = "w", {"k"}, {"Q"}))

        Return castleControlList

    End Function

    'gets the FEN string of a board
    Public Function GetFEN(ByVal board(,) As LogicSquare)

        Dim FENboard(7) As String
        Dim count As Integer = 0
        Dim FEN As String = ""
        Dim temp As String = ""
        Dim FENstring As String = ""

        'stores FEN by getting the types of pieces on the board e.g. rZZkqbZr
        For rank = 7 To 0 Step -1
            For file = 0 To 7
                temp += board(file, rank).Type
            Next
            FENboard(count) = temp
            count += 1
            temp = ""
        Next

        Dim z As New List(Of String)
        Dim zCount As Integer = 0

        'changes empty squares (Z's) to numbers
        'if consecutive empty squares then group into 1 number e.g rZZkqbZr => r2kqb1r
        For Each s In FENboard
            For Each letter In s
                If letter = GV_NULL Then
                    zCount += 1
                    If zCount = 8 Then
                        z.Add(zCount.ToString)
                    End If
                Else
                    If zCount <> 0 Then
                        z.Add(zCount.ToString)
                    End If
                    z.Add(letter)
                    zCount = 0
                End If
            Next letter
            If zCount <> 0 And zCount <> 8 Then
                z.Add(zCount.ToString)
            End If
            For Each term In z
                FEN += term
            Next term
            FEN += "/"
            z.Clear()
            zCount = 0
        Next s

        FEN = Mid(FEN, 1, FEN.Length - 1)

        Dim enPassantString As String

        If GV_enPassantTargetSquare IsNot Nothing Then
            enPassantString = FormattingAlgorithms.SquareToAlphaNumeric(GV_enPassantTargetSquare)
        Else
            enPassantString = "-"
        End If

        'adds additional game state data to the FEN string
        FENstring += FEN
        FENstring += " " + GV_turn
        FENstring += " " + GV_castlingAvailability
        FENstring += " " + enPassantString
        FENstring += " " + GV_halfmoveClock.ToString
        FENstring += " " + GV_fullmoveCounter.ToString

        Return FENstring

    End Function

    'handles what happens when the user presses a key
    Private Sub Form1_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown

        Select Case e.KeyCode

            Case Keys.M 'toggle between windowed and fullscreen

                If WindowState = FormWindowState.Normal Then
                    WindowState = FormWindowState.Maximized
                    FormBorderStyle = FormBorderStyle.None
                Else
                    WindowState = FormWindowState.Normal
                    FormBorderStyle = FormBorderStyle.Fixed3D
                End If

            Case Keys.Escape  'closes the form and thus the program

                Me.Close()

        End Select

    End Sub

End Class
