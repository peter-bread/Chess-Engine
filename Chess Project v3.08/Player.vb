'stores data about a player of the game
Public Class Player

    'stores what colour the player is playing as within the class
    Private _Colour As Char

    'enables other parts of the project to access the colour that a player is playing as
    'serparate field and property to make sure that it is always formatted correctly
    Public Property Colour As Char
        Get
            Return _Colour.ToString.ToLower     'colour always in lowercase
        End Get
        Set(value As Char)
            _Colour = value.ToString.ToLower    'colour always in lowercase
        End Set
    End Property

    'label used to indicate if it is a player's turn
    Public Property TurnIndicator As Label

    'when a new player is created, it takes in the colour it will be, stores it, and creates a new label for TurnIndicator
    Sub New(colour As Char)

        _Colour = colour
        TurnIndicator = New Label

    End Sub

    'changes TurnIndicator when it is the player's turn
    Public Sub IsYourTurn()

        TurnIndicator.BackColor = Color.Yellow  'sets background colour to yellow
        TurnIndicator.ForeColor = Color.Black   'sets text colour to black

    End Sub

    'changes TurnIndictator when it is no longer the player's turn
    Public Sub IsNotYourTurn()

        TurnIndicator.BackColor = Color.Transparent 'removes background colour
        TurnIndicator.ForeColor = Color.White       'sets text colour to white

    End Sub

End Class