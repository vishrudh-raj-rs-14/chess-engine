using System;

namespace Core
{
    public class GameState
    {
        // true if white turn false if black turn
        public bool PlayerTurn = true;
        public int castlingRights;
        public int enPassantFile;
        public int fiftyMoveCounter;
        public int CurrentCapturedPiece;

        public GameState()
        {
            PlayerTurn = true;
            castlingRights = 0b1111;
            enPassantFile = -1;
            fiftyMoveCounter = 0;
        }
        
        public bool isTurn(int colour)
        {
            return Convert.ToBoolean(colour) == PlayerTurn;
        }
        
        public bool CanCastleKingSide(int color)
        {
            int flag = castlingRights;
            if (color == Piece.White)
            {
                flag = flag >> 2;
            }

            return Convert.ToBoolean(flag & 1);
        }

        public bool CanQueenSideCastle(int color)
        {
            int flag = castlingRights;
            if (color == Piece.White)
            {
                flag = flag >> 2;
            }
            return Convert.ToBoolean((flag >> 1) & 1);
            
        }
        
    }
}