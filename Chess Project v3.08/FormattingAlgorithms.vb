
'Algorithms used for formatting
Module FormattingAlgorithms

    'Converts an element of LogicBoard to its alphanumeric representation
    Public Function SquareToAlphaNumeric(ByVal square As LogicSquare) As String

        If GV_player1.Colour = "w" Then 'accounts for perspective
            Return Chr(square.X + 97) + (square.Y + 1).ToString
        Else
            Return Chr(104 - square.X) + (8 - square.Y).ToString
        End If

    End Function

    'Converts an element of LogicBoard to its alphanumeric representation
    Public Function SquareToAlphaNumeric(x As Integer, y As Integer) As String

        If GV_player1.Colour = "w" Then 'accounts for perspective
            Return Chr(x + 97) + (y + 1).ToString
        Else
            Return Chr(104 - x) + (8 - y).ToString
        End If

    End Function

    'Converts alphanumeric square (e.g c6) to an element in LogicBoard
    Public Function AlphaNumericToSquare(ByVal alphaNumeric As String) As LogicSquare

        If alphaNumeric = "-" Then Return Nothing

        Dim file As String = Mid(alphaNumeric, 1, 1)      'file (letter)
        Dim rank As Integer = Mid(alphaNumeric, 2, 1)     'rank (number)

        If GV_player1.Colour = "w" Then 'accounts for perspective
            Return GV_logicBoard(Asc(file) - 97, rank - 1)
        Else
            Return GV_logicBoard(104 - Asc(file), 8 - rank)
        End If

    End Function

    'Takes a string, sorts it alphabetically, and returns the sorted string
    Public Function SortString(inputString As String) As String

        Dim sortingArray() As Char = inputString.ToCharArray    'converts input string to CharArray
        Array.Sort(sortingArray)    'sorts CharArray alphabetically
        Return New String(sortingArray) 'converts the CharArray back to a string and returns it

    End Function

End Module
