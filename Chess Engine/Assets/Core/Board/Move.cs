namespace Core
{
    public class Move
    {
        readonly ushort moveValue;
        
        // Flags
        public const int NoFlag = 0b0000;
        public const int EnPassantCaptureFlag = 0b0001;
        public const int KingSideCastleFlag = 0b0010;
        public const int QueenSideCastleFlag = 0b0011;

        public const int PromoteToQueenFlag = 0b0100;
        public const int PromoteToKnightFlag = 0b0101;
        public const int PromoteToRookFlag = 0b0110;
        public const int PromoteToBishopFlag = 0b0111;
        
        public const int PawnTwoUpFlag = 0b1000;

        // Masks
        const ushort startSquareMask = 0b0000000000111111;
        const ushort targetSquareMask = 0b0000111111000000;
        const ushort flagMask = 0b1111000000000000;
        
        public Move(ushort moveValue)
        {
            this.moveValue = moveValue;
        }

        public Move(int startSquare, int targetSquare)
        {
            moveValue = (ushort)(startSquare | targetSquare << 6);
        }

        public Move(int startSquare, int targetSquare, int flag)
        {
            moveValue = (ushort)(startSquare | targetSquare << 6 | flag << 12);
        }

        public ushort Value => moveValue;
        public bool IsNull => moveValue == 0;

        public int StartSquare => moveValue & startSquareMask;
        public int TargetSquare => (moveValue & targetSquareMask) >> 6;
        public bool IsPromotion => MoveFlag >= PromoteToQueenFlag;
        public int MoveFlag => moveValue >> 12;

        public int PromotionPieceType
        {
            get
            {
                switch (MoveFlag)
                {
                    case PromoteToRookFlag:
                        return Piece.Rook;
                    case PromoteToKnightFlag:
                        return Piece.Knight;
                    case PromoteToBishopFlag:
                        return Piece.Bishop;
                    case PromoteToQueenFlag:
                        return Piece.Queen;
                    default:
                        return Piece.None;
                }
            }
        }
        
    }
}