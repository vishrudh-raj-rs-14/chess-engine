using System;
using UnityEngine;
using System.Collections.Generic;

namespace Core
{
    public class Board
    {

        public int[] board;
        
        public Stack<GameState> gameStateHistory;

        public Board()
        {
            board = new int[64];
            gameStateHistory = new Stack<GameState>();
            gameStateHistory.Push(new GameState());
        }
        public void LoadBoard(string fen)
        {
            board = FenToBoard(fen);
            // Add gamestate as well from fen
        }

        // Move validity must be checked before making the move to prevent inefficiencies where we already know a move
        // is valid and want to look through the board to check if it is valid again. can also be updated to never is if
        // a move is valid in search
        public int MakeMove(Move move, bool inSearch=false)
        {
            GameState gameState = (gameStateHistory.Count>0)? gameStateHistory.Peek() : new GameState();
            int flag = move.MoveFlag;
            int startSquare = move.StartSquare;
            int endSquare = move.TargetSquare;
            int newEnPassFile = -1;
            int newCastlingRights = gameState.castlingRights;
            bool isPawnMove = Piece.GetPieceType(board[startSquare]) == Piece.Pawn;
            bool isCapture = board[endSquare] != 0 || flag == Move.EnPassantCaptureFlag;
            int currentCapturedPiece = board[endSquare];
            int movedPiece = board[startSquare];
            if (Piece.GetPieceType(movedPiece) == Piece.King)
            {
                newCastlingRights &= (gameState.PlayerTurn) ? 0b0011 : 0b1100;
                if (flag == Move.KingSideCastleFlag)
                {
                    board[endSquare - 1] = board[endSquare + 1];
                    board[endSquare + 1] = 0;
                }
                else if (flag == Move.QueenSideCastleFlag)
                {
                    board[endSquare + 1] = board[endSquare - 2];
                    board[endSquare - 2] = 0;
                }
            }
            if (flag == Move.EnPassantCaptureFlag)
            {
                int sqr =  endSquare + (gameState.PlayerTurn ? 8 : -8);
                currentCapturedPiece = board[sqr];
                board[sqr] = 0;
            }else if (flag == Move.PawnTwoUpFlag)
            {
                newEnPassFile = endSquare % 8;
            }

            if (Piece.GetPieceType(movedPiece) == Piece.Rook)
            {
                int curState = (gameState.castlingRights);
                if (startSquare % 8 == 0)
                {
                    newCastlingRights = curState & ((gameState.PlayerTurn) ? 0b1011 : 0b1110);
                }else if (startSquare % 8 == 7)
                {
                    newCastlingRights = curState & ((gameState.PlayerTurn) ? 0b0111 : 0b1101);
                }
            }
            if (Piece.GetPieceType(board[endSquare]) == Piece.Rook)
            {
                int curState = (gameState.castlingRights);
                if (endSquare % 8 == 0)
                {
                    newCastlingRights = curState & ((!gameState.PlayerTurn) ? 0b1011 : 0b1110);
                }else if (endSquare % 8 == 7)
                {
                    newCastlingRights = curState & ((!gameState.PlayerTurn) ? 0b0111 : 0b1101);
                }
            }
            if (Piece.GetPieceType(movedPiece) == Piece.Pawn && (endSquare / 8 == 0 || endSquare / 8 == 7))
            {
                int pieceType = Piece.None;
                switch (flag)
                {
                    case Move.PromoteToBishopFlag:
                        pieceType = Piece.Bishop;
                        break;
                    case Move.PromoteToQueenFlag:
                        pieceType = Piece.Queen;
                        break;
                    case Move.PromoteToRookFlag:
                        pieceType = Piece.Rook;
                        break;
                    case Move.PromoteToKnightFlag:
                        pieceType = Piece.Knight;
                        break;
                    default:
                        pieceType = Piece.Queen;
                        break;
                }

                int color = gameState.PlayerTurn ? Piece.White : Piece.Black;
                int piece = Piece.MakePice(color, pieceType);
                movedPiece = piece;
            }

            board[endSquare] = movedPiece;
            board[startSquare] = 0;
            GameState newGameState = new GameState();
            if (isPawnMove || isCapture)
            {
                newGameState.fiftyMoveCounter = 0;
            }
            else
            {
                newGameState.fiftyMoveCounter = gameState.fiftyMoveCounter+1;
            }
            newGameState.castlingRights = newCastlingRights;
            newGameState.enPassantFile = newEnPassFile;
            newGameState.PlayerTurn = !gameState.PlayerTurn;
            newGameState.CurrentCapturedPiece = currentCapturedPiece;
            gameStateHistory.Push(newGameState);
            return 0;
        }
        
        public int UnMakeMove(Move move, bool inSearch=false)
        {
            GameState gameState = gameStateHistory.Peek();
            int flag = move.MoveFlag;
            int startSquare = move.StartSquare;
            int endSquare = move.TargetSquare;
            bool enPassant = false;
            int movedPiece = board[endSquare];
            // In castle the move only indicate king move
            if (Piece.GetPieceType(movedPiece) == Piece.King)
            {
                if (flag == Move.KingSideCastleFlag)
                {
                    board[endSquare + 1] = board[endSquare - 1];
                    board[endSquare - 1] = 0;
                }
                else if (flag == Move.QueenSideCastleFlag)
                {
                    board[endSquare - 2] = board[endSquare + 1];
                    board[endSquare + 1] = 0;
                }
            }
            if (flag == Move.EnPassantCaptureFlag)
            {
                enPassant = true;
                int sqr =  endSquare + (!gameState.PlayerTurn ? 8 : -8);
                board[sqr] = gameState.CurrentCapturedPiece;
            }
            
            if ((flag >= Move.PromoteToQueenFlag && flag <= Move.PromoteToBishopFlag) &&
                (endSquare / 8 == 0 || endSquare / 8 == 7))
            {
                int pieceType = Piece.Pawn;
                int color = !gameState.PlayerTurn ? Piece.White : Piece.Black;
                int piece = Piece.MakePice(color, pieceType);
                movedPiece = piece;
                
            }

            board[startSquare] = movedPiece;
            if (!enPassant)
            {
                board[endSquare] = gameState.CurrentCapturedPiece;
            }
            else
            {
                board[endSquare] = 0;
            }

            gameStateHistory.Pop();
            return 0;
        }

        public bool inCheck(int color)
{
    // Find the king of the specified color
    int kingSquare = -1;
    int kingPiece = Piece.MakePice(color, Piece.King);
    
    for (int i = 0; i < 64; i++)
    {
        if (board[i] == kingPiece)
        {
            kingSquare = i;
            break;
        }
    }
    
    if (kingSquare == -1)
        return false; // No king found (shouldn't happen in valid game)
    
    // Check if any enemy piece can attack the king
    int enemyColor = (color == Piece.White) ? Piece.Black : Piece.White;
    
    for (int square = 0; square < 64; square++)
    {
        int piece = board[square];
        if (piece == 0 || Piece.GetPieceColor(piece) != enemyColor)
            continue;
        
        if (IsSquareAttackedByPieceAt(kingSquare, square))
            return true;
    }
    
    return false;
}

public HashSet<int> getAllControllingSquares(int colour)
{
    HashSet<int> controlledSquares = new HashSet<int>();
    
    for (int square = 0; square < 64; square++)
    {
        int piece = board[square];
        if (piece == 0 || Piece.GetPieceColor(piece) != colour)
            continue;
        
        // Get all squares this piece controls
        HashSet<int> pieceControlledSquares = GetSquaresControlledByPieceAt(square);
        controlledSquares.UnionWith(pieceControlledSquares);
    }
    
    return controlledSquares;
}

private bool IsSquareAttackedByPieceAt(int targetSquare, int attackerSquare)
{
    int piece = board[attackerSquare];
    if (piece == 0)
        return false;
    
    int pieceType = Piece.GetPieceType(piece);
    int pieceColor = Piece.GetPieceColor(piece);
    
    int targetRow = targetSquare / 8;
    int targetCol = targetSquare % 8;
    int attackerRow = attackerSquare / 8;
    int attackerCol = attackerSquare % 8;
    
    switch (pieceType)
    {
        case Piece.Pawn:
            return IsPawnAttacking(attackerSquare, targetSquare, pieceColor);
        case Piece.King:
            return IsKingAttacking(attackerRow, attackerCol, targetRow, targetCol);
        case Piece.Knight:
            return IsKnightAttacking(attackerRow, attackerCol, targetRow, targetCol);
        case Piece.Rook:
            return IsRookAttacking(attackerRow, attackerCol, targetRow, targetCol);
        case Piece.Bishop:
            return IsBishopAttacking(attackerRow, attackerCol, targetRow, targetCol);
        case Piece.Queen:
            return IsQueenAttacking(attackerRow, attackerCol, targetRow, targetCol);
        default:
            return false;
    }
}

private HashSet<int> GetSquaresControlledByPieceAt(int square)
{
    HashSet<int> controlledSquares = new HashSet<int>();
    int piece = board[square];
    if (piece == 0)
        return controlledSquares;
    
    int pieceType = Piece.GetPieceType(piece);
    int pieceColor = Piece.GetPieceColor(piece);
    
    int row = square / 8;
    int col = square % 8;
    
    switch (pieceType)
    {
        case Piece.Pawn:
            GetPawnControlledSquares(square, pieceColor, controlledSquares);
            break;
        case Piece.King:
            GetKingControlledSquares(row, col, controlledSquares);
            break;
        case Piece.Knight:
            GetKnightControlledSquares(row, col, controlledSquares);
            break;
        case Piece.Rook:
            GetSlidingControlledSquares(row, col, controlledSquares, true, false);
            break;
        case Piece.Bishop:
            GetSlidingControlledSquares(row, col, controlledSquares, false, true);
            break;
        case Piece.Queen:
            GetSlidingControlledSquares(row, col, controlledSquares, true, true);
            break;
    }
    
    return controlledSquares;
}

private bool IsPawnAttacking(int pawnSquare, int targetSquare, int pawnColor)
{
    int pawnRow = pawnSquare / 8;
    int pawnCol = pawnSquare % 8;
    int targetRow = targetSquare / 8;
    int targetCol = targetSquare % 8;
    
    int direction = (pawnColor == Piece.White) ? -1 : 1;
    
    // Pawns attack diagonally one square forward
    return (targetRow == pawnRow + direction) && 
           (Math.Abs(targetCol - pawnCol) == 1);
}

private void GetPawnControlledSquares(int pawnSquare, int pawnColor, HashSet<int> controlledSquares)
{
    int row = pawnSquare / 8;
    int col = pawnSquare % 8;
    int direction = (pawnColor == Piece.White) ? -1 : 1;
    
    // Pawn controls diagonal squares (regardless of whether there's a piece there)
    int newRow = row + direction;
    if (newRow >= 0 && newRow < 8)
    {
        if (col > 0)
            controlledSquares.Add(newRow * 8 + col - 1);
        if (col < 7)
            controlledSquares.Add(newRow * 8 + col + 1);
    }
}

private bool IsKingAttacking(int kingRow, int kingCol, int targetRow, int targetCol)
{
    int rowDiff = Math.Abs(targetRow - kingRow);
    int colDiff = Math.Abs(targetCol - kingCol);
    return rowDiff <= 1 && colDiff <= 1 && (rowDiff != 0 || colDiff != 0);
}

private void GetKingControlledSquares(int row, int col, HashSet<int> controlledSquares)
{
    int[,] directions = { {-1,-1}, {-1,0}, {-1,1}, {0,-1}, {0,1}, {1,-1}, {1,0}, {1,1} };
    
    for (int i = 0; i < 8; i++)
    {
        int newRow = row + directions[i, 0];
        int newCol = col + directions[i, 1];
        
        if (newRow >= 0 && newRow < 8 && newCol >= 0 && newCol < 8)
        {
            controlledSquares.Add(newRow * 8 + newCol);
        }
    }
}

private bool IsKnightAttacking(int knightRow, int knightCol, int targetRow, int targetCol)
{
    int rowDiff = Math.Abs(targetRow - knightRow);
    int colDiff = Math.Abs(targetCol - knightCol);
    return (rowDiff == 2 && colDiff == 1) || (rowDiff == 1 && colDiff == 2);
}

private void GetKnightControlledSquares(int row, int col, HashSet<int> controlledSquares)
{
    int[,] knightMoves = { {2,1}, {2,-1}, {-2,1}, {-2,-1}, {1,2}, {1,-2}, {-1,2}, {-1,-2} };
    
    for (int i = 0; i < 8; i++)
    {
        int newRow = row + knightMoves[i, 0];
        int newCol = col + knightMoves[i, 1];
        
        if (newRow >= 0 && newRow < 8 && newCol >= 0 && newCol < 8)
        {
            controlledSquares.Add(newRow * 8 + newCol);
        }
    }
}

private bool IsRookAttacking(int rookRow, int rookCol, int targetRow, int targetCol)
{
    // Must be on same rank or file
    if (rookRow != targetRow && rookCol != targetCol)
        return false;
    
    return HasClearPath(rookRow, rookCol, targetRow, targetCol);
}

private bool IsBishopAttacking(int bishopRow, int bishopCol, int targetRow, int targetCol)
{
    // Must be on same diagonal
    if (Math.Abs(targetRow - bishopRow) != Math.Abs(targetCol - bishopCol))
        return false;
    
    return HasClearPath(bishopRow, bishopCol, targetRow, targetCol);
}

private bool IsQueenAttacking(int queenRow, int queenCol, int targetRow, int targetCol)
{
    // Queen combines rook and bishop attacks
    return IsRookAttacking(queenRow, queenCol, targetRow, targetCol) ||
           IsBishopAttacking(queenRow, queenCol, targetRow, targetCol);
}

private void GetSlidingControlledSquares(int row, int col, HashSet<int> controlledSquares, bool includeRookMoves, bool includeBishopMoves)
{
    List<int[]> directions = new List<int[]>();
    
    if (includeRookMoves)
    {
        directions.Add(new int[] {0, 1});   // right
        directions.Add(new int[] {0, -1});  // left
        directions.Add(new int[] {1, 0});   // down
        directions.Add(new int[] {-1, 0});  // up
    }
    
    if (includeBishopMoves)
    {
        directions.Add(new int[] {1, 1});   // down-right
        directions.Add(new int[] {1, -1});  // down-left
        directions.Add(new int[] {-1, 1});  // up-right
        directions.Add(new int[] {-1, -1}); // up-left
    }
    
    foreach (var direction in directions)
    {
        for (int distance = 1; distance < 8; distance++)
        {
            int newRow = row + direction[0] * distance;
            int newCol = col + direction[1] * distance;
            
            if (newRow < 0 || newRow >= 8 || newCol < 0 || newCol >= 8)
                break;
            
            int newSquare = newRow * 8 + newCol;
            controlledSquares.Add(newSquare);
            
            // If there's a piece here, we can't continue further in this direction
            if (board[newSquare] != 0)
                break;
        }
    }
}

private bool HasClearPath(int fromRow, int fromCol, int toRow, int toCol)
{
    int rowDirection = (toRow > fromRow) ? 1 : (toRow < fromRow) ? -1 : 0;
    int colDirection = (toCol > fromCol) ? 1 : (toCol < fromCol) ? -1 : 0;
    
    int currentRow = fromRow + rowDirection;
    int currentCol = fromCol + colDirection;
    
    while (currentRow != toRow || currentCol != toCol)
    {
        if (board[currentRow * 8 + currentCol] != 0)
            return false; // Path is blocked
        
        currentRow += rowDirection;
        currentCol += colDirection;
    }
    
    return true;
}

        public int[] FenToBoard(string fen)
        {
            int [] newBoard = new int[64];
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
                newBoard[y*8+x] = Piece.GetPieceFromSymbol(fen[i]);
                x++;
            }

            return newBoard;
        }
    }
}