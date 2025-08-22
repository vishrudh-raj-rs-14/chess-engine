using System.Collections.Generic;
using UnityEngine;

namespace Core.Movegenerator
{
    public class MoveGenerator
    {
        private Board _board;

        public MoveGenerator(Board board)
        {
            _board = board;
        }
    
        public List<Move> GenerateMoves(Board board)
        {
            List<Move> moves = new List<Move>();
            return moves;
        }

        
        
        public List<Move> GenerateMovesOfPiece(int square)
        {
            List<Move> moves = new List<Move>();
            int pieceCode = Piece.GetPieceType(_board.board[square]);
            int color = Piece.GetPieceColor(_board.board[square]);
            GameState state = _board.gameStateHistory.Peek();
            // Debug.Log($"{color}, {pieceCode}, {state.PlayerTurn}");
            if ((color == (Piece.White>>3)) != state.PlayerTurn)
            {
                return moves;
            }
            if (pieceCode == 0)
            {
                return moves;
            }
            switch (pieceCode)
            {
                case Piece.Pawn:
                    GeneratePawnMoves(moves, square);
                    break;
                case Piece.King:
                    GenerateKingMoves(moves, square);
                    break;
                case Piece.Knight:
                    GenerateKnightMoves(moves, square);
                    break;
                default:
                    GenerateSlidingPiecesMoves(moves, square);
                    break;
            }
            
            return moves;
        }

        public void GenerateKingMoves(List<Move> moves, int square)
        {
            int[,] dir =
            {
                { 1, 1 },
                { 1, 0 },
                { 1, -1 },
                { 0, -1 },
                { -1, -1 },
                { -1, 0 },
                { -1, 1 },
                { 0, 1 }
            };
            int currentPiece = _board.board[square];
            int currentPieceColor = Piece.GetPieceColor(currentPiece);
            GameState state = _board.gameStateHistory.Peek();
            if ((currentPieceColor == (Piece.White>>3)) != state.PlayerTurn)
            {
                return;
            }
            
            for (int i = 0; i < 8; i++)
            {
                int row = square / 8;
                int col = square % 8;
                int newRow = row + dir[i, 0];
                int newCol = col + dir[i, 1];
                if(newRow < 0 || newRow >= 8 || newCol < 0 || newCol >= 8) continue;
                int newSqr = row*8 + col;
                int targetPiece = _board.board[newSqr];
                if (Piece.GetPieceColor(targetPiece) == currentPieceColor) continue;

                Move move = new Move(square, newSqr);
                _board.MakeMove(move);
                
                if(!_board.inCheck(currentPieceColor))
                {
                    moves.Add(move);
                }
                
                _board.UnMakeMove(move);
            }

            if (_board.inCheck(currentPieceColor))
            {
                return;
            }

            if (state.CanCastleKingSide(currentPieceColor))
            {
                if (_board.board[square + 1] == 0 && _board.board[square + 2] == 0)
                {
                    // Check so that those squares are not controlled as well
                    Move move = new Move(square, square + 2, Move.KingSideCastleFlag);
                    _board.MakeMove(move);
                
                    if(!_board.inCheck(currentPieceColor))
                    {
                        moves.Add(move);
                    }
                
                    _board.UnMakeMove(move);
                }
            }

            if (state.CanQueenSideCastle(currentPieceColor))
            {
                if (_board.board[square - 1] == 0 && _board.board[square - 2] == 0 && _board.board[square - 3] == 0)
                {
                    // Check so that those squares are not controlled as well
                    Move move = new Move(square, square - 2, Move.QueenSideCastleFlag);
                    _board.MakeMove(move);
                
                    if(!_board.inCheck(currentPieceColor))
                    {
                        moves.Add(move);
                    }
                
                    _board.UnMakeMove(move);
                }
            }
            
        }

        public void GeneratePawnMoves(List<Move> moves, int square)
        {
            GameState state = _board.gameStateHistory.Peek();
            int color = Piece.GetPieceColor(_board.board[square]);
            if ((color == (Piece.White>>3)) != state.PlayerTurn)
            {
                return;
            }
            int dir = state.PlayerTurn ? -1 : 1;
            if (_board.board[square + dir * 8] == 0)
            {
                Move move = new Move(square, square + dir * 8);
                _board.MakeMove(move);
                if (!_board.inCheck(color))
                {
                    moves.Add(move);
                }
                _board.UnMakeMove(move);
            }

            if ((state.PlayerTurn && square / 8 == 6) || (!state.PlayerTurn && square / 8 == 1))
            {
                if (_board.board[square + dir * 16] == 0)
                {
                    Move move = new Move(square, square + dir * 16, Move.PawnTwoUpFlag);
                    _board.MakeMove(move);
                    if (!_board.inCheck(color))
                    {
                        moves.Add(move);
                    }
                    _board.UnMakeMove(move);
                }
            }

            int leftSqr = square  - 1;
            int rightSqr = square +  1;

            if (_board.board[leftSqr + dir * 8] != 0 &&
                Piece.GetPieceColor(_board.board[leftSqr + dir * 8]) != color)
            {
                Move move = new Move(square, leftSqr+dir * 8);
                _board.MakeMove(move);
                if (!_board.inCheck(color))
                {
                    moves.Add(move);
                }
                _board.UnMakeMove(move);
            }
            
            if (_board.board[rightSqr + dir * 8] != 0 &&
                Piece.GetPieceColor(_board.board[rightSqr + dir * 8]) != color)
            {
                Move move = new Move(square, rightSqr+dir * 8);
                _board.MakeMove(move);
                if (!_board.inCheck(color))
                {
                    moves.Add(move);
                }
                _board.UnMakeMove(move);
            }

            bool leftEnPassCondition = EnPassCaptureCondition(square, leftSqr, color, state);
            bool rightEnPassCondition = EnPassCaptureCondition(square, rightSqr, color, state);
            Debug.Log("En Pass Condition: " + leftEnPassCondition + ", " + rightEnPassCondition);
            Debug.Log($"{state.enPassantFile}, {square%8}, {leftSqr%8}, {rightSqr%8}");
            if (leftEnPassCondition)
            {
                Move move = new Move(square, leftSqr+dir * 8, Move.EnPassantCaptureFlag);
                _board.MakeMove(move);
                if (!_board.inCheck(color))
                {
                    moves.Add(move);
                }
                _board.UnMakeMove(move);
            }
            if (rightEnPassCondition)
            {
                Move move = new Move(square, rightSqr+dir * 8, Move.EnPassantCaptureFlag);
                _board.MakeMove(move);
                if (!_board.inCheck(color))
                {
                    moves.Add(move);
                }
                _board.UnMakeMove(move);
            }
        }

        public bool EnPassCaptureCondition(int square, int adjacentSquare, int color, GameState state)
        {
            return _board.board[adjacentSquare] != 0 &&
                Piece.GetPieceColor(_board.board[adjacentSquare]) != color &&
                state.enPassantFile == (adjacentSquare % 8) &&
                Piece.GetPieceType(_board.board[adjacentSquare]) == Piece.Pawn &&
                adjacentSquare/8 == square/8 &&
                ((color == Piece.White)
                    ? (square / 8) == 3
                    : (square / 8) == 4);
        }

        public void GenerateSlidingPiecesMoves(List<Move> moves, int square)
        {
            int currentPiece = _board.board[square];
            int currentPieceColor = Piece.GetPieceColor(currentPiece);
            int pieceType = Piece.GetPieceType(currentPiece);
            GameState state = _board.gameStateHistory.Peek();
            if ((currentPieceColor == (Piece.White>>3)) != state.PlayerTurn)
            {
                return;
            }
            // Direction vectors for sliding pieces
            int[,] rookDirections = { {0, 1}, {0, -1}, {1, 0}, {-1, 0} }; // horizontal and vertical
            int[,] bishopDirections = { {1, 1}, {1, -1}, {-1, 1}, {-1, -1} }; // diagonal
            
            int[,] directions;
            int numDirections;
            
            // Determine which directions this piece can move
            if (pieceType == Piece.Rook)
            {
                directions = rookDirections;
                numDirections = 4;
            }
            else if (pieceType == Piece.Bishop)
            {
                directions = bishopDirections;
                numDirections = 4;
            }
            else if (pieceType == Piece.Queen)
            {
                // Queen combines rook and bishop moves - we'll handle both direction sets
                GenerateSlidingInDirections(moves, square, currentPieceColor, rookDirections, 4);
                GenerateSlidingInDirections(moves, square, currentPieceColor, bishopDirections, 4);
                return;
            }
            else
            {
                return; // Invalid piece type for sliding
            }
            
            GenerateSlidingInDirections(moves, square, currentPieceColor, directions, numDirections);
        }

        private void GenerateSlidingInDirections(List<Move> moves, int square, int currentPieceColor, int[,] directions, int numDirections)
        {
            int startRow = square / 8;
            int startCol = square % 8;
            
            
            
            for (int dirIndex = 0; dirIndex < numDirections; dirIndex++)
            {
                int deltaRow = directions[dirIndex, 0];
                int deltaCol = directions[dirIndex, 1];
                
                for (int distance = 1; distance < 8; distance++)
                {
                    int newRow = startRow + deltaRow * distance;
                    int newCol = startCol + deltaCol * distance;
                    
                    // Check if we're still on the board
                    if (newRow < 0 || newRow >= 8 || newCol < 0 || newCol >= 8)
                        break;
                    
                    int targetSquare = newRow * 8 + newCol;
                    int targetPiece = _board.board[targetSquare];
                    
                    // If there's a piece of our own color, we can't move there
                    if (targetPiece != 0 && Piece.GetPieceColor(targetPiece) == currentPieceColor)
                        break;
                    
                    // Create and test the move
                    Move move = new Move(square, targetSquare);
                    _board.MakeMove(move);
                    
                    if (!_board.inCheck(currentPieceColor))
                    {
                        moves.Add(move);
                    }
                    
                    _board.UnMakeMove(move);
                    
                    // If we captured an enemy piece, we can't continue in this direction
                    if (targetPiece != 0)
                        break;
                }
            }
        }

        public void GenerateKnightMoves(List<Move> moves, int square)
        {
            int currentPiece = _board.board[square];
            int currentPieceColor = Piece.GetPieceColor(currentPiece);
            GameState state = _board.gameStateHistory.Peek();
            if ((currentPieceColor == (Piece.White>>3)) != state.PlayerTurn)
            {
                return;
            }
            // Knight move offsets (L-shaped moves)
            int[,] knightMoves = {
                {2, 1},   // 2 up, 1 right
                {2, -1},  // 2 up, 1 left
                {-2, 1},  // 2 down, 1 right
                {-2, -1}, // 2 down, 1 left
                {1, 2},   // 1 up, 2 right
                {1, -2},  // 1 up, 2 left
                {-1, 2},  // 1 down, 2 right
                {-1, -2}  // 1 down, 2 left
            };
            
            int currentRow = square / 8;
            int currentCol = square % 8;
            
            for (int i = 0; i < 8; i++)
            {
                int newRow = currentRow + knightMoves[i, 0];
                int newCol = currentCol + knightMoves[i, 1];
                
                // Check if the new position is within board bounds
                if (newRow < 0 || newRow >= 8 || newCol < 0 || newCol >= 8)
                    continue;
                
                int targetSquare = newRow * 8 + newCol;
                int targetPiece = _board.board[targetSquare];
                
                // Skip if there's a piece of our own color on the target square
                if (targetPiece != 0 && Piece.GetPieceColor(targetPiece) == currentPieceColor)
                    continue;
                
                // Create and test the move
                Move move = new Move(square, targetSquare);
                _board.MakeMove(move);
                
                if (!_board.inCheck(currentPieceColor))
                {
                    moves.Add(move);
                }
                
                _board.UnMakeMove(move);
            }
        }

        
        
    }
}