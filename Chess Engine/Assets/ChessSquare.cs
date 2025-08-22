using UnityEngine;

public class ChessSquare : MonoBehaviour
{
    private int _square;
    

    private int _piece;

    public int GetSquare()
    {
        return _square;
    }

    public void SetSquare(int square)
    {
        _square = square;
    }
    public int GetPiece()
    {
        return _piece;
    }

    public void SetPiece(int piece)
    {
        _piece = piece;
    }
}
