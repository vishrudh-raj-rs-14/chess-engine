using UnityEngine;

namespace Core
{
    public class Board
    {

        public int[,] board;

        public Board()
        {
            board = new int[8, 8];
        }
        public void LoadBoard(string fen)
        {
            board = FenToBoard(fen);
        }

        public int[,] FenToBoard(string fen)
        {
            int [,] newBoard = new int[8, 8];
            int x = 0;
            int y = 0;
            for (int i = 0; i < fen.Length; i++)
            {
                
                if (fen[i] == '/')
                {
                    y++;
                    x = 0;
                    continue;
                }
                if (char.IsNumber(fen[i]))
                {
                    x+=(fen[i] - '0');
                    continue;
                }
                if(x>7 || y>7) break;
                newBoard[x, y] = Piece.GetPieceFromSymbol(fen[i]);
                x++;
            }

            return newBoard;
        }
    }
}