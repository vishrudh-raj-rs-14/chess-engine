using UnityEngine;

public class ChessSquare : MonoBehaviour
{
    private int _x;
    private int _y;
    

    private int _piece;

    public Vector2Int GetCoords()
    {
        return new Vector2Int(_x,_y);
    }

    public void SetCoords(int x, int y)
    {
        _x = x;
        _y = y;
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
