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
    [SerializeField] private Color activeSquareColor = Color.yellow;
    
    [Header("Game Settings")]
    [SerializeField] private string FEN;
    [SerializeField] private float pieceScale = 1f;

    [Header("Piece Sprites")]
    [SerializeField] private Sprite whitePawn, blackPawn;
    [SerializeField] private Sprite whiteKnight, blackKnight;
    [SerializeField] private Sprite whiteBishop, blackBishop;
    [SerializeField] private Sprite whiteRook, blackRook;
    [SerializeField] private Sprite whiteQueen, blackQueen;
    [SerializeField] private Sprite whiteKing, blackKing;

    // Cached references
    private Camera _camera;
    private GameObject[,] _squares;
    private Material _darkSquareMaterial;
    private Material _lightSquareMaterial;
    private Dictionary<int, Sprite> _pieceToSprite;
    private Board _board;

    // Input handling
    private InputHandler _inputHandler;

    // Events for decoupling
    public static event System.Action<Vector2Int, Vector2Int> OnPieceMoved;
    public static event System.Action<Vector2Int> OnPieceSelected;

    private void Awake()
    {
        _camera = Camera.main;
        InitializePieceSprites();
        InitializeMaterials();
    }

    private void Start()
    {
        _board = new Board();
        _inputHandler = new InputHandler(_camera, this);
        GenerateBoard();
        SetupBoard(FEN);
    }

    private void Update()
    {
        UpdateMaterialColors(); // Only updates if colors changed
        _inputHandler.HandleInput();
    }

    private void OnDestroy()
    {
        CleanupMaterials();
    }

    #region Initialization
    private void InitializePieceSprites()
    {
        _pieceToSprite = new Dictionary<int, Sprite>
        {
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
    }

    private void InitializeMaterials()
    {
        _lightSquareMaterial = CreateSquareMaterial(lightSquareColor);
        _darkSquareMaterial = CreateSquareMaterial(darkSquareColor);
    }

    private Material CreateSquareMaterial(Color color)
    {
        var material = new Material(Shader.Find("Unlit/Color"));
        material.color = color;
        return material;
    }

    private void UpdateMaterialColors()
    {
        if (_lightSquareMaterial.color != lightSquareColor)
            _lightSquareMaterial.color = lightSquareColor;
        
        if (_darkSquareMaterial.color != darkSquareColor)
            _darkSquareMaterial.color = darkSquareColor;
    }

    private void CleanupMaterials()
    {
        if (_lightSquareMaterial != null) DestroyImmediate(_lightSquareMaterial);
        if (_darkSquareMaterial != null) DestroyImmediate(_darkSquareMaterial);
    }
    #endregion

    #region Board Generation
    [ContextMenu("Generate Board")]
    public void GenerateBoard()
    {
        ClearBoard();
        CreateBoard();
    }

    private void CreateBoard()
    {
        if (!ValidateSetup()) return;

        _squares = new GameObject[8, 8];
        var boardOffset = CalculateBoardOffset();
        
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                CreateSquare(x, y, boardOffset);
            }
        }
        
        Debug.Log("Chess board generated successfully");
    }

    private void CreateSquare(int x, int y, Vector2 boardOffset)
    {
        var position = boardOffset + new Vector2(x * squareSize, -y * squareSize);
        var square = Instantiate(squarePrefab, position, Quaternion.identity, transform);
        
        ConfigureSquare(square, x, y);
        _squares[x, y] = square;
    }

    private void ConfigureSquare(GameObject square, int x, int y)
    {
        square.name = $"Square_{x}_{y}";
        
        var chessSquare = square.GetComponent<ChessSquare>();
        chessSquare.SetCoords(x, y);
        
        var isLightSquare = (x + y) % 2 != 0;
        var material = isLightSquare ? _lightSquareMaterial : _darkSquareMaterial;
        
        ApplyMaterialToSquare(square, material);
    }

    private Vector2 CalculateBoardOffset()
    {
        return new Vector2(-(8 - 1) * squareSize * 0.5f, (8 - 1) * squareSize * 0.5f);
    }

    private bool ValidateSetup()
    {
        if (squarePrefab == null)
        {
            Debug.LogError("Square prefab is not assigned!");
            return false;
        }
        return true;
    }

    private void ApplyMaterialToSquare(GameObject square, Material material)
    {
        var renderer = square.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = material;
        }
        else
        {
            Debug.LogWarning($"No renderer found on square {square.name}");
        }
    }

    [ContextMenu("Clear Board")]
    public void ClearBoard()
    {
        if (_squares == null) return;

        DestroySquares();
        DestroyRemainingChildren();
        _squares = null;
    }

    private void DestroySquares()
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

    private void DestroyRemainingChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
    #endregion

    #region Board Setup
    public void SetupBoard(string fen)
    {
        if (_board == null || _squares == null) return;

        _board.LoadBoard(fen);
        
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                CreatePieceAtSquare(x, y);
            }
        }
    }

    private void CreatePieceAtSquare(int x, int y)
    {
        int pieceCode = _board.board[x, y];
        var square = _squares[x, y];
        var chessSquare = square.GetComponent<ChessSquare>();
        
        chessSquare.SetPiece(pieceCode);
        
        if (pieceCode == Piece.None) return;

        var pieceObject = CreatePieceGameObject(x, y, pieceCode);
        AttachPieceToSquare(pieceObject, square);
    }

    private GameObject CreatePieceGameObject(int x, int y, int pieceCode)
    {
        var pieceObj = new GameObject($"Piece_{x}_{y}");
        var spriteRenderer = pieceObj.AddComponent<SpriteRenderer>();
        
        spriteRenderer.sprite = _pieceToSprite[pieceCode];
        pieceObj.transform.localScale = Vector3.one * pieceScale;
        
        return pieceObj;
    }

    private void AttachPieceToSquare(GameObject pieceObject, GameObject square)
    {
        pieceObject.transform.SetParent(square.transform, false);
        pieceObject.transform.position = square.transform.position - Vector3.back;
    }
    #endregion

    #region Public Interface
    public bool TryMovePiece(Vector2Int from, Vector2Int to)
    {
        if (!IsValidSquare(from) || !IsValidSquare(to)) return false;

        var fromSquare = _squares[from.x, from.y].GetComponent<ChessSquare>();
        var toSquare = _squares[to.x, to.y].GetComponent<ChessSquare>();
        
        // Move piece in game logic
        var piece = fromSquare.GetPiece();
        if (piece == Piece.None) return false;

        ExecuteMove(fromSquare, toSquare, from, to, piece);
        OnPieceMoved?.Invoke(from, to);
        
        return true;
    }

    private void ExecuteMove(ChessSquare fromSquare, ChessSquare toSquare, Vector2Int from, Vector2Int to, int piece)
    {
        // Update game logic
        _board.board[from.x, from.y] = 0;
        _board.board[to.x, to.y] = piece;
        
        // Update UI
        fromSquare.SetPiece(0);
        toSquare.SetPiece(piece);
        
        // Move piece visual
        var fromSquareObj = _squares[from.x, from.y];
        var toSquareObj = _squares[to.x, to.y];
        
        if (fromSquareObj.transform.childCount > 0)
        {
            var pieceTransform = fromSquareObj.transform.GetChild(0);
            
            // Remove any existing piece at destination
            if (toSquareObj.transform.childCount > 0)
            {
                DestroyImmediate(toSquareObj.transform.GetChild(0).gameObject);
            }
            
            pieceTransform.SetParent(toSquareObj.transform);
            pieceTransform.position = toSquareObj.transform.position - Vector3.back;
        }
    }

    public bool IsValidSquare(Vector2Int coords)
    {
        return coords.x >= 0 && coords.x < 8 && coords.y >= 0 && coords.y < 8;
    }

    public GameObject GetSquareAt(Vector2Int coords)
    {
        return IsValidSquare(coords) ? _squares[coords.x, coords.y] : null;
    }
    #endregion
}

// Separate input handling for better organization
public class InputHandler
{
    private Camera _camera;
    private ChessBoardGenerator _boardGenerator;
    private GameObject _activePiece;
    private Vector2Int _originalSquare;

    public InputHandler(Camera camera, ChessBoardGenerator boardGenerator)
    {
        _camera = camera;
        _boardGenerator = boardGenerator;
    }

    public void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseDown();
        }
        else if (Input.GetMouseButton(0) && _activePiece != null)
        {
            HandleMouseDrag();
        }
        else if (Input.GetMouseButtonUp(0) && _activePiece != null)
        {
            HandleMouseUp();
        }
    }

    private void HandleMouseDown()
    {
        var worldPos = GetMouseWorldPosition();
        var hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null && _activePiece == null)
        {
            TrySelectPiece(hit.collider.gameObject);
        }
    }

    private void HandleMouseDrag()
    {
        var worldPos = GetMouseWorldPosition();
        _activePiece.transform.position = worldPos;
    }

    private void HandleMouseUp()
    {
        var worldPos = GetMouseWorldPosition();
        var hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null)
        {
            TryDropPiece(hit.collider.gameObject);
        }
        else
        {
            ReturnPieceToOriginal();
        }

        _activePiece = null;
    }

    private void TrySelectPiece(GameObject hitObject)
    {
        var pieceTransform = hitObject.transform.GetChild(0);
        if (pieceTransform != null)
        {
            _activePiece = pieceTransform.gameObject;
            var chessSquare = hitObject.GetComponent<ChessSquare>();
            _originalSquare = chessSquare.GetCoords();
            
            Debug.Log($"Selected piece at {_originalSquare}");
        }
    }

    private void TryDropPiece(GameObject targetSquare)
    {
        var chessSquare = targetSquare.GetComponent<ChessSquare>();
        if (chessSquare != null)
        {
            var targetCoords = chessSquare.GetCoords();
            if(targetCoords == _originalSquare)
            {
                 ReturnPieceToOriginal();
                 return;
            }
            
            if (_boardGenerator.TryMovePiece(_originalSquare, targetCoords))
            {
                Debug.Log($"Moved piece from {_originalSquare} to {targetCoords}");
            }
            else
            {
                ReturnPieceToOriginal();
            }
        }
        else
        {
            ReturnPieceToOriginal();
        }
    }

    private void ReturnPieceToOriginal()
    {
        var originalSquareObj = _boardGenerator.GetSquareAt(_originalSquare);
        if (originalSquareObj != null)
        {
            _activePiece.transform.position = originalSquareObj.transform.position - Vector3.back;
        }
    }

    private Vector2 GetMouseWorldPosition()
    {
        return _camera.ScreenToWorldPoint(Input.mousePosition);
    }
}