using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public GameObject AssociedGO;
    public Vector2Int Position;
    public bool IsFilled;
    public List<Cell> Neighbors;
    public LineRenderer AssociedLineRenderer;

    public Cell()
    {
        IsFilled = false;
        Neighbors = new List<Cell>();
    }
}

public class Grid
{
    public int Width;
    public int Height;
    public Cell[,] Cells;

    private Grid() { }
    public Grid(int width, int height)
    {
        Width = width;
        Height = height;

        Cells = new Cell[Width, Height];

        // Inicializamos todas las celdas
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Cells[x, y] = new Cell();
            }
        }
    }
}

public class Node
{
    public int NodeId;
    public Color NodeColor;
    public SpriteRenderer SpriteRend;
    public Cell AssociedCell;
    public LineRenderer AssociedLineRenderer;
    public List<Cell> SolutionPath;
    //public List<CellCPR> PlayerPath;
    public Animator NodeAnim;
    public bool IsCorrectPath = false;

    // El nodo con el que hace match
    public Node NodePair;

    public Node()
    {
        SolutionPath = new List<Cell>();
        //PlayerPath = new List<CellCPR>();
    }

    public Node(int id, Color nodeColor, SpriteRenderer sprRend, Cell cell)
    {
        NodeId = id;
        NodeColor = nodeColor;
        SpriteRend = sprRend;
        AssociedCell = cell;

        SolutionPath = new List<Cell>();
        //PlayerPath = new List<CellCPR>();
    }

    public void ShowAnim()
    {
        BoardGenerator.Instance.StartCoroutine(AnimationCoroutine());
    }

    private IEnumerator AnimationCoroutine()
    {
        NodeAnim.SetBool("connected", true);

        yield return new WaitUntil(() =>
        NodeAnim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);


        NodeAnim.SetBool("connected", false);
    }

    public void AnimPairs()
    {
        NodeAnim.SetTrigger("size");
        NodePair.NodeAnim.SetTrigger("size");
    }
}

public class BoardGenerator : MonoBehaviour
{
    public Grid GridBoard;

    [SerializeField]
    [Tooltip("Camara a ajustar posición")]
    private Camera _boardCamera;

    [SerializeField]
    [Tooltip("Padre del tablero")]
    private Transform _fatherBoard;

    [SerializeField]
    [Tooltip("Prefab de las celdas")]
    private GameObject _prefabCell;

    [SerializeField]
    [Tooltip(@"Tiempo de espera entre pintar/despintar fila y
               pintar/despintar fila en la corrección")]
    private float _waitingColorHeightTime = 0.08f;

    public static BoardGenerator Instance;

    void Awake()
    {
        if (Instance != this && Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        GameController.Instance.OnReset.AddListener(OnReset);
        //GenerateBoard(4, 4, 4);
    }

    /// <summary>
    /// Función que genera el tablero, instanciando las celdas
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="nodes"></param>
    public void GenerateBoard(int width, int height, int nodes)
    {
        // Creamos un objeto de la clase tablero e inicializamos sus celdas
        GridBoard = new Grid(width, height);

        // Creamos la parte fisica del tablero
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject cellGO = Instantiate(_prefabCell, _fatherBoard);
                cellGO.name = $"Cell({x},{y})";
                cellGO.transform.position = new Vector2(x, y);
                GridBoard.Cells[x, y].AssociedGO = cellGO;
                GridBoard.Cells[x, y].Position = new Vector2Int(x, y);
            }
        }

        // Asociamos a las celdas cuáles son sus vecinos
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Vecino de arriba
                if (y < height - 1)
                    GridBoard.Cells[x, y].Neighbors.Add(GridBoard.Cells[x, y + 1]);

                // Vecino de abajo
                if (y > 0)
                    GridBoard.Cells[x, y].Neighbors.Add(GridBoard.Cells[x, y - 1]);

                // Vecino de la izquierda
                if (x > 0)
                    GridBoard.Cells[x, y].Neighbors.Add(GridBoard.Cells[x - 1, y]);

                // Vecino de la derecha
                if (x < width - 1)
                    GridBoard.Cells[x, y].Neighbors.Add(GridBoard.Cells[x + 1, y]);

            }
        }

        SetCameraToCenter();
        NodesGenerator.Instance.CreateCorrectBoard(nodes);
    }

    /// <summary>
    /// Función que pone la camara en el centro del tablero
    /// </summary>
    private void SetCameraToCenter()
    {
        _boardCamera.transform.position = GetCameraCenter();

        float maxValue = Mathf.Max(GridBoard.Width, GridBoard.Height);

        _boardCamera.orthographicSize = 0.5f * maxValue + 0.6f;
    }

    private Vector3 GetCameraCenter()
    {
        Vector3 center = new Vector3();

        foreach (Transform child in _fatherBoard)
        {
            center += child.position;
        }

        center = center / _fatherBoard.transform.childCount;

        center.z = Camera.main.transform.position.z;

        return center;
    }

    public void StartChangeColor()
    {
        for (int x = 0; x < GridBoard.Width; x++)
        {
            for (int y = 0; y < GridBoard.Height; y++)
            {
                GridBoard.Cells[x, y].AssociedGO.transform.GetChild(3).GetComponent<LoadingColor>().StartChangeColor();
            }
        }
    }

    public void StopChangeColor()
    {
        for (int x = 0; x < GridBoard.Width; x++)
        {
            for (int y = 0; y < GridBoard.Height; y++)
            {
                GridBoard.Cells[x, y].AssociedGO.transform.GetChild(3).GetComponent<LoadingColor>().StopChangeColor();
            }
        }
    }

    public IEnumerator ShowLoadingColor(bool show)
    {
        for (int y = (GridBoard.Height - 1); y >= 0; y--)
        {
            for (int x = 0; x < GridBoard.Width; x++)
                GridBoard.Cells[x, y].AssociedGO.transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = show ? 3 : 1;

            ControlLoadingColor(y, show);
            yield return new WaitForSeconds(_waitingColorHeightTime);
        }

    }

    private void ControlLoadingColor(int line, bool show)
    {
        for (int x = 0; x < GridBoard.Width; x++)
        {
            GridBoard.Cells[x, line].AssociedGO.transform.GetChild(3).GetComponent<SpriteRenderer>().enabled = show;
        }
    }

    private void OnReset()
    {
        StopAllCoroutines();

        // Si hay casillas pintadas las despintamos
        for (int x = 0; x < GridBoard.Width; x++)
        {
            for (int y = 0; y < GridBoard.Height; y++)
            {
                GameObject cell = GridBoard.Cells[x, y].AssociedGO;

                if (cell.transform.GetChild(1).gameObject.activeSelf)
                {
                    cell.transform.GetChild(1).gameObject.SetActive(false);
                }
            }

        }
    }
}
