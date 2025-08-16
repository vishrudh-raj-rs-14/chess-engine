namespace Core
{
    public class Piece
    {

        public const int None = 0;
        public const int Pawn = 1;
        public const int Rook = 2;
        public const int Bishop = 3;
        public const int Knight = 4;
        public const int Queen = 5;
        public const int King = 6;

        public const int White = 0b0000;
        public const int Black = 0b1000;
        
        
        public const int WhitePawn = White | Pawn;
        public const int WhiteRook = White | Rook;
        public const int WhiteBishop = White | Bishop;
        public const int WhiteKnight = White | Knight;
        public const int WhiteQueen = White | Queen;
        public const int WhiteKing = White | King;
        
        public const int BlackPawn = Black | Pawn;
        public const int BlackRook = Black | Rook;
        public const int BlackBishop = Black | Bishop;
        public const int BlackKnight = Black | Knight;
        public const int BlackQueen = Black | Queen;
        public const int BlackKing = Black | King;

        public const int PieceMask = 0b0111;
        public const int ColorMask = 0b1000;

        public static int MakePice(int pieceColor, int pieceType) { return (pieceColor | pieceType); }
        public static int GetPieceColor(int piece) { return (piece & PieceMask); }
        public static int GetPieceType(int piece) { return (piece & PieceMask); }

        public static int GetPieceFromSymbol(char symbol)
        {
            int pieceColor = char.IsUpper(symbol) ? White : Black;
            char pieceSymbol = char.ToLower(symbol);
            if (pieceSymbol == 'r') return pieceColor | Rook;
            if (pieceSymbol == 'b') return pieceColor | Bishop;
            if (pieceSymbol == 'n') return pieceColor | Knight;
            if (pieceSymbol == 'q') return pieceColor | Queen;
            if (pieceSymbol == 'k') return pieceColor | King;
            if (pieceSymbol == 'p') return pieceColor | Pawn;
            return None;
        }
    }
}