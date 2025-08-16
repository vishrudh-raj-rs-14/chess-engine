using System;
using System.Collections.Generic;
using Core;
using UnityEngine;

public class ChessBoardGenerator : MonoBehaviour
{
    [Header("Board Settings")]
    [SerializeField] private GameObject squarePrefab;
    [SerializeField] private float squareSize = 1f;
    
    [Header("Colors")]
    [SerializeField] private Color lightSquareColor = Color.white;
    [SerializeField] private Color darkSquareColor = Color.black;
    
    
    private GameObject[,] _squares;
    private Material _darkSquareMaterial;
    private Material _lightSquareMaterial;

    private Board _board;
    [SerializeField] private string FEN;
    [SerializeField] private float pieceScale = 1;

    [SerializeField] private Sprite whitePawn;
    [SerializeField] private Sprite blackPawn;
    [SerializeField] private Sprite whiteKnight;
    [SerializeField] private Sprite blackKnight;
    [SerializeField] private Sprite whiteBishop;
    [SerializeField] private Sprite blackBishop;
    [SerializeField] private Sprite whiteRook;
    [SerializeField] private Sprite blackRook;
    [SerializeField] private Sprite whiteQueen;
    [SerializeField] private Sprite blackQueen;
    [SerializeField] private Sprite whiteKing;
    [SerializeField] private Sprite blackKing;

    private Dictionary<int, Sprite> _pieceToSprite;
    
    void Start()
    {
        _pieceToSprite = new Dictionary<int, Sprite>{
                {Piece.White | Piece.Pawn, whitePawn},
                {Piece.Black | Piece.Pawn, blackPawn},
                {Piece.White | Piece.Knight, whiteKnight},
                {Piece.Black | Piece.Knight, blackKnight},
                {Piece.White | Piece.Bishop, whiteBishop},
                {Piece.Black | Piece.Bishop, blackBishop},
                {Piece.White | Piece.Rook, whiteRook},
                {Piece.Black | Piece.Rook, blackRook},
                {Piece.White | Piece.Queen, whiteQueen},
                {Piece.Black | Piece.Queen, blackQueen},
                {Piece.White | Piece.King, whiteKing},
                {Piece.Black | Piece.King, blackKing},
            };
        _board = new Board();
        GenerateBoard();
        SetupBoard(FEN);
    }

    private void Update()
    {
        _lightSquareMaterial.color = lightSquareColor;
        _darkSquareMaterial.color = darkSquareColor;
    }

    [ContextMenu("Generate Board")]
    public void GenerateBoard()
    {
        ClearBoard();
        CreateBoard();
    }

    public void SetupBoard(string FEN)
    {
        Debug.Log(FEN);
        _board.LoadBoard(FEN);
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Debug.Log($"{_board.board[x, y]}, {x}, {y}");
                int pieceCode = _board.board[x, y];
                if (pieceCode == Piece.None) continue;

                Sprite sprite = _pieceToSprite[pieceCode];

                GameObject square = _squares[x, y];
                Vector3 pos = square.transform.position + Vector3.up * 0.01f;

                // Create piece GameObject with SpriteRenderer
                GameObject pieceObj = new GameObject($"Piece_{x}_{y}");
                var sr = pieceObj.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;

                pieceObj.transform.SetParent(square.transform, false);
                pieceObj.transform.position = pos;
                pieceObj.transform.localScale *= pieceScale;
            }
        }
        
        
    }
    
    [ContextMenu("Clear Board")]
    public void ClearBoard()
    {
        if (_squares != null)
        {
            for (int x = 0; x < _squares.GetLength(0); x++)
            {
                for (int y = 0; y < _squares.GetLength(1); y++)
                {
                    if (_squares[x, y] != null)
                    {
                        DestroyImmediate(_squares[x, y]);
                    }
                }
            }
        }
        
        // Also clear any child objects in case some weren't tracked
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        _squares = null;
    }
    
    private void CreateBoard()
    {
        if (squarePrefab == null)
        {
            Debug.LogError("Square prefab is not assigned!");
            return;
        }
        if (_lightSquareMaterial == null)
        {
            _lightSquareMaterial = new Material(Shader.Find("Unlit/Color"));
            _lightSquareMaterial.color = lightSquareColor;
        }

        if (_darkSquareMaterial == null)
        {
            _darkSquareMaterial = new Material(Shader.Find("Unlit/Color"));
            _darkSquareMaterial.color = darkSquareColor;
        }
        
        _squares = new GameObject[8, 8];
        
        float startX = (8 - 1) * squareSize * 0.5f;
        float startY = (8 - 1) * squareSize * 0.5f;
        
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Vector2 position = new Vector2(
                    startX - x * squareSize,
                    startY - y * squareSize
                );
                
                GameObject square = Instantiate(squarePrefab, position, Quaternion.identity, transform);
                square.name = $"Square_{x}_{y}";
                
                bool isLightSquare = (x + y) % 2 != 0;
                Material squareColor = isLightSquare ? _lightSquareMaterial : _darkSquareMaterial;
                
                ApplyColorToSquare(square, squareColor, x, y);
                
                _squares[x, y] = square;
            }
        }
        
        Debug.Log($"Chess board generated with {8}x{8} squares");
    }
    
    
    private void ApplyColorToSquare(GameObject square, Material color, int x, int y)
    {
        Renderer renderer = square.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = color;
            return;
        }
        
        Debug.LogWarning($"No renderer found on square {square.name} to apply color to!");
    }
}