
'handles graphics for board and some pieces
Public Class Square

    'the point on the form where a piece will be located if it occupies this square
    Public Property Point As Point

    'the first index of GV_RealBoard, representing the file
    Public ReadOnly Property X As Integer
        Get
            Return (Point.X / GV_scalar) - 1    'converts point on form to index bewteen 0 and 7
        End Get
    End Property

    'the first index of GV_RealBoard, representing the rank
    Public ReadOnly Property Y As Integer
        Get
            Return 8 - (Point.Y / GV_scalar)    'converts point on form to index bewteen 0 and 7
        End Get
    End Property

    'determines whether a square is marked or not
    Private _marked As Boolean
    Public Property Marked As Boolean
        Get
            Return _marked
        End Get
        Set(value As Boolean)
            _marked = value

            'sets graphics variable to interact with bitmap for board
            GV_gfx = Graphics.FromImage(GV_boardBitmap)

            'always set standard background colour for square and piece (if there is a piece on the square)

            If (X + Y) Mod 2 = 0 Then 'if black square
                'set square to black colour
                GV_gfx.FillRectangle(GlobalVariables.GV_blackBrush, New Rectangle(New Point(GV_scalar * X, GV_scalar * (7 - Y)), New Size(GV_scalar, GV_scalar)))
            Else 'else (if white square)
                'set square to white colour
                GV_gfx.FillRectangle(GlobalVariables.GV_whiteBrush, New Rectangle(New Point(GV_scalar * X, GV_scalar * (7 - Y)), New Size(GV_scalar, GV_scalar)))
            End If

            'if there is a piece on the square then change piece's background colour
            If Occupied Then
                Piece.SetBackColour()
            End If


            'if square is marked then highlight piece grey or draw dark circle on square
            If value = True Then

                'if there is a piece on the square then change the piece's 
                If Occupied Then
                    Piece.PBX.BackColor = Color.Gray
                Else

                    If (X + Y) Mod 2 = 0 Then 'if black square
                        'draw darker circle on square
                        GV_gfx.FillEllipse(New SolidBrush(GlobalVariables.GV_darkBlackColour), New Rectangle(New Point(GV_scalar * X + (GV_scalar / 3), GV_scalar * (7 - Y) + (GV_scalar / 3)), New Size(GV_scalar / 3, GV_scalar / 3)))   'draws circle on square
                    Else 'else (if white square)
                        'draw darker circle on square
                        GV_gfx.FillEllipse(New SolidBrush(GlobalVariables.GV_darkWhiteColour), New Rectangle(New Point(GV_scalar * X + (GV_scalar / 3), GV_scalar * (7 - Y) + (GV_scalar / 3)), New Size(GV_scalar / 3, GV_scalar / 3)))   'draws circle on square
                    End If

                End If

            End If

        End Set
    End Property

    'returns if there is a piece occupying the square
    Public ReadOnly Property Occupied As Boolean
        Get
            Return GV_logicBoard(X, Y).Occupied
        End Get
    End Property

    'returns the piece that is occupying the square
    Public ReadOnly Property Piece As Piece
        Get
            For Each p In GV_piecesOnBoard
                If p.PBX.Location = Point Then
                    Return p
                End If
            Next
            Return Nothing
        End Get
    End Property

    Sub New(point As Point, marked As Boolean)
        Me.Point = point
        Me.Marked = marked
    End Sub

End Class
