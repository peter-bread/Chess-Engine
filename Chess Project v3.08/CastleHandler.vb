
'handles pieces that are used in castling (rooks and kings)
'used for handling castling availability when simulated moves are unmade
Public Class CastleHandler

    'reference to the first index in GV_CastleHandling
    Public Property X As Integer

    'reference to the second index in GV_CastleHandling
    Public Property Y As Integer

    'How many moves has the piece simulated. +1 for a move made, -1 for a move unmade
    'can also be used to determine if the real piece has actually moved
    Public Property SimulatedMoveCounter As Integer

    'Current location of the piece being handled
    Public Property CurrentSquare As LogicSquare

    'The types of castling that are controlled by the piece
    Public Property CastlingControl As List(Of String)

    Sub New(ByVal CurrentSquare As LogicSquare, ByVal CastlingControl As List(Of String))

        Me.CastlingControl = New List(Of String)    'initialises new list for CastlingControl
        Me.CastlingControl.AddRange(CastlingControl)    'populates list with the types of castling that will be controlled by the pieces
        SimulatedMoveCounter = 0    'no moves has been made or simulated so set to 0
        Me.CurrentSquare = CurrentSquare    'sets start location of piece being handled

    End Sub

    'used when unmaking moves to check if a king or rook is still at its start location and yet to move
    Public Function IsAtStart() As Boolean

        Return If(SimulatedMoveCounter = 0, True, False)    'if it has made no moves (or any moves that have been made have also been unmade) then it is at the start

    End Function

End Class