using System;
using System.Collections.Generic;
using Core;
using Core.Movegenerator;
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
    [SerializeField] private Color validMoveColor = Color.green;
    [SerializeField] private Color captureSquareColor = Color.red;
    
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
    private Material _activeSquareMaterial;
    private Material _validMoveMaterial;
    private Material _captureSquareMaterial;
    private Dictionary<int, Sprite> _pieceToSprite;
    private Board _board;
    private MoveGenerator _moveGenerator;

    // Input handling
    private InputHandler _inputHandler;

    // Valid moves visualization
    private List<GameObject> _highlightedSquares = new List<GameObject>();
    private Dictionary<GameObject, Material> _originalMaterials = new Dictionary<GameObject, Material>();

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
        _moveGenerator = new MoveGenerator(_board);
        _inputHandler = new InputHandler(_camera, this);
        GenerateBoard();
        SetupBoard(FEN);
    }

    private void Update()
    {
        UpdateMaterialColors();
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
        _activeSquareMaterial = CreateSquareMaterial(activeSquareColor);
        _validMoveMaterial = CreateSquareMaterial(validMoveColor);
        _captureSquareMaterial = CreateSquareMaterial(captureSquareColor);
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
        
        if (_activeSquareMaterial.color != activeSquareColor)
            _activeSquareMaterial.color = activeSquareColor;
            
        if (_validMoveMaterial.color != validMoveColor)
            _validMoveMaterial.color = validMoveColor;
            
        if (_captureSquareMaterial.color != captureSquareColor)
            _captureSquareMaterial.color = captureSquareColor;
    }

    private void CleanupMaterials()
    {
        if (_lightSquareMaterial != null) DestroyImmediate(_lightSquareMaterial);
        if (_darkSquareMaterial != null) DestroyImmediate(_darkSquareMaterial);
        if (_activeSquareMaterial != null) DestroyImmediate(_activeSquareMaterial);
        if (_validMoveMaterial != null) DestroyImmediate(_validMoveMaterial);
        if (_captureSquareMaterial != null) DestroyImmediate(_captureSquareMaterial);
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
        
        ConfigureSquare(square, x+y*8);
        _squares[x, y] = square;
    }

    private void ConfigureSquare(GameObject square, int square_count)
    {
        square.name = $"Square_{square_count}";
        
        var chessSquare = square.GetComponent<ChessSquare>();
        chessSquare.SetSquare(square_count);
        
        var isLightSquare = (square_count % 8 + square_count / 8) % 2 != 0;
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
        RefreshBoardFromSource();
    }

    /// <summary>
    /// Refreshes the entire visual board from the underlying board state
    /// </summary>
    public void RefreshBoardFromSource()
    {
        if (_board == null || _squares == null) return;
        
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                RefreshSquareFromSource(x, y);
            }
        }
        
        Debug.Log("Board refreshed from source");
    }

    private void RefreshSquareFromSource(int x, int y)
    {
        int squareIndex = x + y * 8;
        int pieceCode = _board.board[squareIndex];
        var square = _squares[x, y];
        var chessSquare = square.GetComponent<ChessSquare>();
        
        // Update the square's piece data
        chessSquare.SetPiece(pieceCode);
        
        // Clear any existing piece visuals
        ClearPieceVisualsFromSquare(square);
        
        // Create new piece visual if there's a piece
        if (pieceCode != Piece.None)
        {
            var pieceObject = CreatePieceGameObject(x, y, pieceCode);
            AttachPieceToSquare(pieceObject, square);
        }
    }

    private void ClearPieceVisualsFromSquare(GameObject square)
    {
        // Remove all child objects (pieces) from the square
        for (int i = square.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(square.transform.GetChild(i).gameObject);
        }
    }

    private void CreatePieceAtSquare(int x, int y)
    {
        int pieceCode = _board.board[x+y*8];
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

    #region Valid Moves Visualization
    public void ShowValidMovesForSquare(int squareIndex)
    {
        ClearHighlightedSquares();
        Debug.Log($"Square Index: {squareIndex}");
        var validMoves = _moveGenerator.GenerateMovesOfPiece(squareIndex);
        
        // Highlight the selected square
        var selectedSquare = GetSquareFromIndex(squareIndex);
        if (selectedSquare != null)
        {
            HighlightSquare(selectedSquare, _activeSquareMaterial);
        }
        
        // Highlight valid move squares
        foreach (var move in validMoves)
        {
            var targetSquare = GetSquareFromIndex(move.TargetSquare);
            if (targetSquare != null)
            {
                // Check if it's a capture move
                bool isCapture = _board.board[move.TargetSquare] != 0;
                var material = isCapture ? _captureSquareMaterial : _validMoveMaterial;
                HighlightSquare(targetSquare, material);
            }
        }
        
        Debug.Log($"Showing {validMoves.Count} valid moves for square {squareIndex}");
    }

    private GameObject GetSquareFromIndex(int squareIndex)
    {
        int x = squareIndex % 8;
        int y = squareIndex / 8;
        
        if (x >= 0 && x < 8 && y >= 0 && y < 8)
        {
            return _squares[x, y];
        }
        
        return null;
    }

    private void HighlightSquare(GameObject square, Material highlightMaterial)
    {
        var renderer = square.GetComponent<Renderer>();
        if (renderer != null)
        {
            // Store original material if not already stored
            if (!_originalMaterials.ContainsKey(square))
            {
                _originalMaterials[square] = renderer.material;
            }
            
            // Apply highlight material
            renderer.material = highlightMaterial;
            
            // Add to highlighted squares list
            if (!_highlightedSquares.Contains(square))
            {
                _highlightedSquares.Add(square);
            }
        }
    }

    public void ClearHighlightedSquares()
    {
        foreach (var square in _highlightedSquares)
        {
            if (square != null && _originalMaterials.ContainsKey(square))
            {
                var renderer = square.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = _originalMaterials[square];
                }
                _originalMaterials.Remove(square);
            }
        }
        
        _highlightedSquares.Clear();
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
        
        Debug.Log($"{_board.board[fromSquare.GetSquare()]}, {fromSquare.GetSquare()}, {toSquare.GetSquare()}");

        // Check if the move is valid
        var validMoves = _moveGenerator.GenerateMovesOfPiece(fromSquare.GetSquare());
        bool isValidMove = false;
        Move validMove = new Move(-1, -1);
        
        foreach (var move in validMoves)
        {
            if (move.TargetSquare == toSquare.GetSquare())
            {
                isValidMove = true;
                validMove = move;
                break;
            }
        }

        if (isValidMove)
        {
            // Execute the move in the underlying board
            _board.MakeMove(validMove, false);
            
            // Refresh the entire visual board from the true source
            RefreshBoardFromSource();
            
            ClearHighlightedSquares(); // Clear highlights after move
            OnPieceMoved?.Invoke(from, to);
            return true;
        }
        
        return false;
    }

    public bool IsValidSquare(Vector2Int coords)
    {
        return coords.x >= 0 && coords.x < 8 && coords.y >= 0 && coords.y < 8;
    }

    public GameObject GetSquareAt(Vector2Int coords)
    {
        return IsValidSquare(coords) ? _squares[coords.x, coords.y] : null;
    }
    
    public void OnSquareClicked(int squareIndex)
    {
        ShowValidMovesForSquare(squareIndex);
    }
    #endregion
}

// Updated InputHandler with valid move visualization
public class InputHandler
{
    private Camera _camera;
    private ChessBoardGenerator _boardGenerator;
    private GameObject _activePiece;
    private Vector2Int _originalSquare;
    private bool _isDragging = false;

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
        else if (Input.GetMouseButtonUp(0))
        {
            HandleMouseUp();
        }
    }

    private void HandleMouseDown()
    {
        var worldPos = GetMouseWorldPosition();
        var hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null)
        {
            var chessSquare = hit.collider.GetComponent<ChessSquare>();
            if (chessSquare != null)
            {
                // Show valid moves for the clicked square
                _boardGenerator.OnSquareClicked(chessSquare.GetSquare());
                
                // If there's a piece on this square, prepare for dragging
                if (hit.collider.transform.childCount > 0)
                {
                    TrySelectPiece(hit.collider.gameObject);
                }
            }
        }
    }

    private void HandleMouseDrag()
    {
        if (!_isDragging) return;
        
        var worldPos = GetMouseWorldPosition();
        _activePiece.transform.position = worldPos;
    }

    private void HandleMouseUp()
    {
        if (_activePiece != null)
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
            _isDragging = false;
        }
    }

    private void TrySelectPiece(GameObject hitObject)
    {
        var pieceTransform = hitObject.transform.GetChild(0);
        if (pieceTransform != null)
        {
            _activePiece = pieceTransform.gameObject;
            var chessSquare = hitObject.GetComponent<ChessSquare>();
            _originalSquare = new Vector2Int(chessSquare.GetSquare() % 8, chessSquare.GetSquare() / 8);
            _isDragging = true;
            
            Debug.Log($"Selected piece at {_originalSquare}");
        }
    }

    private void TryDropPiece(GameObject targetSquare)
    {
        var chessSquare = targetSquare.GetComponent<ChessSquare>();
        if (chessSquare != null)
        {
            var targetCoords = new Vector2Int(chessSquare.GetSquare() % 8, chessSquare.GetSquare() / 8);
            
            if (targetCoords == _originalSquare)
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
                Debug.Log("Invalid move attempted");
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