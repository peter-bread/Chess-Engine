
'stores data about pieces
'allows player to interact with pieces (click/drag to move)

'imports images of pieces
Imports Chess_Project_v3._08.My.Resources

Public Class Piece

    'picturebox to display piece
    'allows user to interact with piece by clicking or dragging the picturebox
    Public Property PBX As PictureBox

    'stores the location of the cursor relative to the picturebox
    Public xCursor, yCursor As Single

    'stores whether the piece is a promotion option when a player is promoting a pawn
    Public Property IsPromotionPiece As Boolean

    'stores whether a piece's valid moves have been calculated
    Private movesCalculated As Boolean

    'stores all valid moves that a piece can make
    Public validMoves As New List(Of Move)

    'first index of the square in board array that the piece occupies before it starts to move
    Public startX As Integer

    'second index of the square in board array that the piece occupies before it starts to move
    Public startY As Integer

    'stores whether a piece's moves are currently being displayed on the board
    Private movesVisible As Boolean

    'stores the move being made if move is a pawn promotion
    Public nextMove As Move

    'stores the type of piece
    Private _type As Char
    Public Property Type As Char
        Get
            Return _type
        End Get
        Set(value As Char)
            _type = value

            'set size of picturebox and piece
            PBX.Size = GV_standardSize

            'if Type is set to "Z" (piece has no type) then exit
            If value = GV_NULL Then Exit Property

            With PBX

                'loads image of piece that is associated with the type
                Select Case value
                    Case "B"
                        .Image = New Bitmap(WhiteBishop, GV_standardSize)
                    Case "b"
                        .Image = New Bitmap(BlackBishop, GV_standardSize)
                    Case "K"
                        .Image = New Bitmap(WhiteKing, GV_standardSize)
                    Case "k"
                        .Image = New Bitmap(BlackKing, GV_standardSize)
                    Case "N"
                        .Image = New Bitmap(WhiteKnight, GV_standardSize)
                    Case "n"
                        .Image = New Bitmap(BlackKnight, GV_standardSize)
                    Case "P"
                        .Image = New Bitmap(WhitePawn, GV_standardSize)
                    Case "p"
                        .Image = New Bitmap(BlackPawn, GV_standardSize)
                    Case "Q"
                        .Image = New Bitmap(WhiteQueen, GV_standardSize)
                    Case "q"
                        .Image = New Bitmap(BlackQueen, GV_standardSize)
                    Case "R"
                        .Image = New Bitmap(WhiteRook, GV_standardSize)
                    Case "r"
                        .Image = New Bitmap(BlackRook, GV_standardSize)
                End Select

            End With

        End Set

    End Property

    'gets first index of the square in board array that the piece is occupying
    Public ReadOnly Property X As Integer
        Get
            Return (PBX.Location.X / GV_scalar) - 1
        End Get
    End Property

    'gets second index of the square in board array that the piece is occupying
    Public ReadOnly Property Y As Integer
        Get
            Return 8 - (PBX.Location.Y / GV_scalar)
        End Get
    End Property

    'gets the colour of the piece
    Public ReadOnly Property Colour As Char
        Get
            If Asc(Type) < 90 Then Return "w"
            Return "b"
        End Get
    End Property

    Public Sub New(type As Char, IsPromotionPiece As Boolean)

        'initialises new picturebox
        PBX = New PictureBox

        'set type of piece and whether it is a promotion piece
        Me.Type = type
        Me.IsPromotionPiece = IsPromotionPiece

        'add mouse down event handler to all pieces (so they can be clicked on)
        AddHandler PBX.MouseDown, AddressOf MouseDown

        'if piece is not a promotion piece then add mouse up and mouse move events
        'this is so promotion pieces can't be dragged around, only clicked on
        If Not IsPromotionPiece Then
            AddHandler PBX.MouseMove, AddressOf MouseMove
            AddHandler PBX.MouseUp, AddressOf MouseUp
        End If

        'add event handlers for when the cursor enters/leaves the picturebox
        AddHandler PBX.MouseEnter, AddressOf Enter
        AddHandler PBX.MouseLeave, AddressOf Leave

    End Sub

    'when cursor is over piece, it changes to a hand to show it is an object that can be interacted with
    Private Sub Enter(sender As PictureBox, e As EventArgs)
        Form1.Cursor = Cursors.Hand
    End Sub

    'when cursor is not over a piece, it is reverted to default
    Private Sub Leave(sender As PictureBox, e As EventArgs)
        Form1.Cursor = Cursors.Default
    End Sub

    'when the user clicks down on a piece, they select it, deselect it, choose to capture it or choose the type of piece to promote a pawn to
    Private Sub MouseDown(sender As PictureBox, e As MouseEventArgs)

        'pieces can only be interacted with using left mouse button
        If e.Button <> MouseButtons.Left Then Return

        'stores the move that is going to be made if the user is clicking on a piece to capture
        Dim moveToMake As Move = Nothing

        'if piece is not a promotion piece
        If Not IsPromotionPiece Then

            'if the piece is the player's colour and it is the player's turn
            If GV_turn = Colour And GV_player1.Colour = Colour Then

                'if piece being clicked is not the currently selected piece
                If Me IsNot GV_selectedPiece Then

                    'if there is a selected piece then reset its background colour
                    If GV_selectedPiece IsNot Nothing Then GV_selectedPiece.SetBackColour()

                    'hide valid moves for previously selected piece
                    If GV_selectedPiece IsNot Nothing Then
                        GV_selectedPiece.HideValidMoves()
                    End If

                    'piece being clicked becomes the selected piece
                    GV_selectedPiece = Me

                    'piece being clicked is highlighted yellow to indicate it has been selected
                    PBX.BackColor = Color.Yellow

                    'when a piece is clicked its starting coordinates are stored
                    startX = X
                    startY = Y

                    'valid moves for piece that has being clicked on are displayed
                    ShowValidMoves()
                    PBX.BringToFront()

                Else 'if piece being clicked is already the currently selected piece

                    'deselects piece, resets its background colour and hides its valid moves
                    GV_selectedPiece = Nothing
                    SetBackColour()
                    HideValidMoves()

                End If

            Else  'if the piece clicked is not the player's colour

                'if there is a selected piece and you click on an opposing piece to capture it
                If GV_selectedPiece IsNot Nothing Then

                    'search through selected piece's valid moves until it finds the one that's target square is the square that the piece being clicked on is occupying
                    'stores this move as moveToMake
                    For Each move In GV_selectedPiece.validMoves
                        If X = move.TargetSquare.X And Y = move.TargetSquare.Y Then
                            moveToMake = move
                            Exit For
                        End If
                    Next

                End If

            End If

            'if there is a move to make
            If moveToMake IsNot Nothing Then

                'make the move
                Form1.MakeActualMove(moveToMake, True, True)

                'if move is not a pawn promotion
                If moveToMake.Promotion = GV_NULL Then

                    GV_selectedPiece.HideValidMoves()   'hide valid moves
                    GV_selectedPiece.validMoves.Clear() 'clear list of valid moves
                    GV_selectedPiece.startX = X     'stores the new first index of the square that the piece is on
                    GV_selectedPiece.startY = Y     'stores the new second index of the square that the piece is on
                    GV_selectedPiece = Nothing      'now there is no selected piece

                    'reset all pieces on the board so they are ready for the next move
                    For Each piece In GV_piecesOnBoard
                        piece.ResetForNextMove()
                    Next

                    'forces events to occur, updating the board and pieces more quickly
                    Application.DoEvents()

                    'makes the computer play its response
                    Form1.AutoPlay()

                Else 'if move is a pawn promotion

                    'store the move that the selected piece partially made
                    'this is because for promotion moves are done in 2 parts
                    'select square to move to and make some of the move; choose piece to promote to; make rest of move
                    GV_selectedPiece.nextMove = moveToMake

                End If

            End If

        Else  'if the piece is a promotion piece

            'iterates through the 4 promotion pieces to find the promotion piece that has been clicked
            For Each p In GV_promotionPieces
                If p.PBX Is sender Then

                    'stores what type of piece the selected pawn will promote to
                    GV_promotionSelected = p.Type

                    'alters the move being made to store what type of piece the pawn will promote to
                    GV_selectedPiece.nextMove.Promotion = GV_promotionSelected

                    Exit For

                End If
            Next

            'removes promotion pieces from the form
            Form1.DespawnPromotionPieces()

            'forces events to occur, updating the board and pieces more quickly
            Application.DoEvents()

            'changes the pawn to the type of piece it is promoting to
            GV_selectedPiece.Type = GV_promotionSelected

            'finish making the move
            Form1.MakeActualMove(GV_selectedPiece.nextMove, True, True)

            'ensures that no code will be skipped when making the next move
            GV_promotionToHappen = False

            GV_selectedPiece.HideValidMoves()   'hide valid moves
            GV_selectedPiece.validMoves.Clear() 'clear list of valid moves
            GV_selectedPiece.startX = X     'stores the new first index of the square that the piece is on
            GV_selectedPiece.startY = Y     'stores the new second index of the square that the piece is on
            GV_selectedPiece = Nothing      'now there is no selected piece

            'reset all pieces on the board so they are ready for the next move
            For Each piece In GV_piecesOnBoard
                piece.ResetForNextMove()
            Next

            'forces events to occur, updating the board and pieces more quickly
            Application.DoEvents()

            'makes the computer play its response
            Form1.AutoPlay()

        End If

    End Sub

    'handles dragging pieces
    Private Sub MouseMove(sender As PictureBox, e As MouseEventArgs)

        'if dragging is not enabled then exit sub so 
        If Not GV_draggingEnabled Then Return

        'if the piece is the player's colour and it is the player's turn to move
        If GV_turn = Colour And GV_player1.Colour = Colour Then

            'if no mouse button is being held
            If e.Button = MouseButtons.None Then

                'store the cursor's location relative to the piece that it is moving on
                xCursor = e.X
                yCursor = e.Y

            ElseIf e.Button = MouseButtons.Left Then 'the user is holding the left mouse button

                'if the piece being moved doesn't have its move displayed on the form then they are displayed
                If Not movesVisible Then
                    ShowValidMoves()
                    PBX.BringToFront()
                    PBX.BackColor = Color.Yellow
                    GV_selectedPiece = Me  'if mouse goes down on selected piece then it is deselected, so moving the piece reselects it
                End If

                'moves picturebox with mouse
                sender.Left += e.X - xCursor
                sender.Top += e.Y - yCursor

            End If

        End If

    End Sub

    'when the left mouse button goes up on a target piece, the selected piece is moved to the target piece's location
    Private Sub MouseUp(sender As PictureBox, e As MouseEventArgs)

        'pieces can only be interacted with using left mouse button
        If e.Button <> MouseButtons.Left Then Return

        'stores the move that is going to be made if the user is clicking on a piece to capture
        Dim moveToMake As Move = Nothing

        'if the piece is the player's colour and it is the player's turn to move
        If GV_turn = Colour And GV_player1.Colour = Colour Then

            'gets the cursor location relative to the form
            Dim xPos As Single = sender.Location.X + xCursor
            Dim yPos As Single = sender.Location.Y + yCursor

            'if the piece can't make any moves then it is moved back to its start location and the subroutine can be exited early
            If validMoves.Count = 0 Then
                sender.Location = GV_realBoard(startX, startY).Point
                Return
            End If

            'iterate through each move in the piece's valid moves
            For Each move In validMoves

                'if the target square of the move is the square that the cursor is on when the left mouse button goes up then that move is stored
                If move.TargetSquare.X = xPos \ GV_scalar - 1 And move.TargetSquare.Y = 8 - yPos \ GV_scalar Then
                    moveToMake = move
                    Exit For
                End If

            Next

            'if the left mouse button goes up on a square that is a valid move
            If moveToMake IsNot Nothing Then

                'make the move
                Form1.MakeActualMove(moveToMake, False, True)

                'snap the piece into the correct position so it fits in the square exactly
                sender.Location = GV_realBoard(xPos \ GV_scalar - 1, 8 - yPos \ GV_scalar).Point

                'if the move is not a pawn promotion
                If moveToMake.Promotion = GV_NULL Then

                    HideValidMoves()    'hide valid moves
                    validMoves.Clear()  'clear list of valid moves
                    startX = X          'stores the new first index of the square that the piece is on
                    startY = Y          'stores the new second index of the square that the piece is on
                    GV_selectedPiece = Nothing 'now there is no selected piece

                    'resets all pieces on the board for the next move
                    For Each piece In GV_piecesOnBoard
                        piece.ResetForNextMove()
                    Next

                Else 'if the move is a pawn promotion

                    'store the move that the selected piece partially made
                    'this is because for promotion moves are done in 2 parts
                    'select square to move to and make some of the move; choose piece to promote to; make rest of move
                    GV_selectedPiece.nextMove = moveToMake

                End If

            Else 'if there is no move

                'piece is moved back to where it started
                sender.Location = GV_realBoard(startX, startY).Point

            End If

        End If

        'makes the computer play its response
        If moveToMake IsNot Nothing Then
            If moveToMake.Promotion = GV_NULL Then
                Application.DoEvents()
                Form1.AutoPlay()
            End If
        End If

    End Sub

    'shows a piece's legal moves on the form
    Public Sub ShowValidMoves()

        'iterate through target squares on the board for a specific piece and mark those squares
        For Each target In validMoves
            GV_realBoard(target.TargetSquare.X, target.TargetSquare.Y).Marked = True
        Next

        'refresh the board picturebox
        GV_boardPBX.Refresh()

        'stores that the piece's move are currently being displayed
        movesVisible = True

    End Sub

    'hides a piece's legal moves on the form
    Public Sub HideValidMoves()

        'iterate through target squares on the board for a specific piece and unmark those squares
        For Each target In validMoves
            GV_realBoard(target.TargetSquare.X, target.TargetSquare.Y).Marked = False
        Next

        'refresh the board picturebox
        GV_boardPBX.Refresh()

        'stores that the piece's move are currently not being displayed
        movesVisible = False

    End Sub

    'sets the background colour of a piece
    Public Sub SetBackColour()

        'finds whether it is on a black or white square and sets the colour accordingly
        If (X + Y) Mod 2 = 0 Then
            PBX.BackColor = GV_blackColour
        Else
            PBX.BackColor = GV_whiteColour
        End If

        'if the piece is a king and it is in check then it is highlighted red to indicate this clearly to the user
        If GV_kingInCheck And ((Type = "k" And GV_turn = "b") Or (Type = "K" And GV_turn = "w")) Then
            PBX.BackColor = Color.Red
        End If

    End Sub

    'resets a piece raedy for the next turn
    Public Sub ResetForNextMove()

        'deletes all moves from the piece's list of legal moves as this will likely be different on the next turn
        movesCalculated = False
        validMoves.Clear()

    End Sub

End Class