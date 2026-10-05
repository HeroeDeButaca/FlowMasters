using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class TouchDetector : MonoBehaviour
{
    [SerializeField] private LineRenderer _activeLineRenderer;
    private Node _actualNode;

    private int _pairsConnected;
    private int _totalPairs;

    public bool CanTouch = true;

    [Tooltip("Lista de LineRenderers existentes en el tablero")]
    private List<LineRenderer> _lineRenderers = new List<LineRenderer>();

    [SerializeField]
    [Tooltip("Prefab del LineRenderer")]
    private GameObject _prefabLineRenderer;

    [SerializeField]
    [Tooltip("Camera que renderiza el tablero")]
    private Camera _boardCamera;

    [SerializeField]
    [Tooltip("Padre del tablero")]
    private Transform _fatherBoard;

    [SerializeField]
    [Tooltip("Almacena las celdas por las que pasa el LineRenderer actual pero en formato de GameObject")]
    private List<GameObject> _cellsPath = new List<GameObject>();

    [HideInInspector]
    [Tooltip("Evento que se ejecutará cuando el tablero este lleno")]
    public UnityEvent OnTableFilled;

    [SerializeField]
    private AudioClip _bubblePopSfx;

    public static TouchDetector Instance;

    void Awake()
    {
        if (Instance != this && Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        GameController.Instance.OnReset.AddListener(OnReset);
        NodesGenerator.Instance.OnBoardReady.AddListener(delegate
        {
            CanTouch = true;
        });

        _totalPairs = PlayerData.Instance.SelectedMode.NodesPerBoard;
    }

    void Update()
    {
        if (!CanTouch)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            _cellsPath.Clear();

            GameObject clickedGO = GetMouseGameObjectCell();

            if (clickedGO != null)
            {
                int posIndex;

                if (IsAnActualLineRenderer(clickedGO, out _activeLineRenderer, out posIndex)) // Es un LineRenderer que ya existe
                {
                    // Si hay casillas pintadas las despintamos
                    for (int i = 0; i < _activeLineRenderer.positionCount; i++)
                    {
                        GameObject cell = GetCellGOByPosition(_activeLineRenderer.GetPosition(i));

                        if (cell.transform.GetChild(1).gameObject.activeSelf)
                        {
                            cell.transform.GetChild(1).gameObject.SetActive(false);
                        }
                    }

                    _activeLineRenderer.positionCount = posIndex + 1;

                    // Recoger _cellsPath del lineRenderer
                    for (int i = 0; i < _activeLineRenderer.positionCount; i++)
                    {
                        GameObject cell = GetCellGOByPosition(_activeLineRenderer.GetPosition(i));
                        _cellsPath.Add(cell);
                    }

                    // Averiguar el '_actualNode'
                    Node[] nodes = NodesGenerator.Instance.Nodes.ToArray();
                    for (int i = 0; i < nodes.Length; i++)
                    {
                        if (nodes[i].AssociedCell.AssociedGO.GetHashCode() == _cellsPath[0].GetHashCode())
                        {
                            _actualNode = nodes[i];
                        }
                    }

                    _actualNode.AnimPairs();
                }
                else if (IsThereNode(clickedGO, out _actualNode)) // Crea un nuevo LineRenderer
                {
                    if (_actualNode.NodePair.AssociedLineRenderer != null)
                    {
                        _lineRenderers.Remove(_actualNode.NodePair.AssociedLineRenderer);
                        Destroy(_actualNode.NodePair.AssociedLineRenderer.gameObject);
                    }

                    if (_actualNode.IsCorrectPath)
                    {
                        _pairsConnected--;
                        _actualNode.IsCorrectPath = false;
                        _actualNode.NodePair.IsCorrectPath = false;
                    }

                    _actualNode.AnimPairs();

                    _activeLineRenderer = Instantiate(_prefabLineRenderer, Vector3.zero,
                        Quaternion.identity, _fatherBoard).GetComponent<LineRenderer>();

                    _activeLineRenderer.SetPosition(0, clickedGO.transform.position);
                    _activeLineRenderer.colorGradient = CreateSimpleColorGradient(_actualNode.NodeColor);

                    _actualNode.AssociedLineRenderer = _activeLineRenderer;

                    _lineRenderers.Add(_activeLineRenderer);
                    _cellsPath.Add(clickedGO);
                }

            }

        }
        else if (Input.GetMouseButton(0))
        {
            if (_cellsPath.Count <= 0)
                return;

            GameObject clickedGO = GetMouseGameObjectCell();

            if (clickedGO != null)
            {
                if (clickedGO.tag == "Estimulo")
                {
                    if (_cellsPath.Last().GetHashCode() != clickedGO.GetHashCode())
                    {
                        int oldPosIndex = -1;

                        if (IsANewCell(clickedGO) && !ItBreaksSomeRule(clickedGO))
                        {
                            _cellsPath.Add(clickedGO);

                            CorrectLineRendererPath();
                            return;
                        }
                        else if (IsAnOldPos(clickedGO, out oldPosIndex))
                        {
                            GoToOldPosition(oldPosIndex);

                            CorrectLineRendererPath();
                            return;
                        }

                    }

                }

            }

        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (_actualNode != null && _activeLineRenderer != null)
            {
                if (IsLineRendererPathCorrect())
                {
                    float t = ((float)_pairsConnected / ((float)_totalPairs - 1));
                    float pitch = Mathf.Lerp(1f, 2f, t);
                    AudioManager.Instance.PlaySFX(_bubblePopSfx, pitch);

                    _pairsConnected++;

                    // Le decimos al nodo y a su pareja que ha hecho un camino correcto
                    _actualNode.IsCorrectPath = true;
                    _actualNode.NodePair.IsCorrectPath = true;

                    _actualNode.ShowAnim();
                    _actualNode.NodePair.ShowAnim();

                    // Poner todo el camino con el color del nodo en el background checker
                    foreach (GameObject cellGO in _cellsPath)
                    {
                        Color nodeColor = _actualNode.NodeColor;
                        nodeColor.a = 0.33f;

                        cellGO.transform.GetChild(1).GetComponent<SpriteRenderer>().color = nodeColor;
                        cellGO.transform.GetChild(1).gameObject.SetActive(true);
                    }
                }
                else
                {
                    // Le decimos al nodo y a su pareja que ha hecho un camino incorrecto
                    _actualNode.IsCorrectPath = false;
                    _actualNode.NodePair.IsCorrectPath = false;

                    if (_activeLineRenderer.positionCount <= 1)
                    {
                        _lineRenderers.Remove(_activeLineRenderer);
                        Destroy(_activeLineRenderer.gameObject);
                    }
                }

                // Desreferenciamos variables
                _actualNode = null;
                _activeLineRenderer = null;
            }

            int tableSquares = BoardGenerator.Instance.GridBoard.Width * BoardGenerator.Instance.GridBoard.Height;

            if (GetTotalPositionsFilled() >= tableSquares && IsAllCorrect())
            {
                Debug.Log("Tablero completado");
                CanTouch = false;
                OnTableFilled?.Invoke();
            }

        }

    }

    /// <summary>
    /// Lanza un raycast y te pasa en forma de GameObject que celda ha tocado
    /// </summary>
    /// <returns>La celda tocada en forma de GameObject</returns>
    private GameObject GetMouseGameObjectCell()
    {
        Vector2 mousePos = _boardCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);
        Collider2D topCollider = null;

        if (hits.Length > 0)
        {
            RaycastHit2D bestHit = hits[0];
            int bestOrder = int.MinValue;

            foreach (var h in hits)
            {
                var sr = h.collider.GetComponent<SpriteRenderer>();
                int order = sr != null ? sr.sortingOrder : 0;

                if (order > bestOrder)
                {
                    bestOrder = order;
                    bestHit = h;
                }
            }

            topCollider = bestHit.collider;
        }

        //Debug.Log($"Top collider tag: {topCollider.tag}");

        if (topCollider != null)
        {
            if (topCollider.CompareTag("Estimulo"))
            {
                //Debug.Log("Target Position: " + hit.collider.gameObject.transform.parent.gameObject.name);
                return topCollider.gameObject.transform.parent.gameObject;
            }
        }

        return null;
    }

    /// <summary>
    /// Pasando una posición te pasa una celda
    /// </summary>
    /// <param name="cellPos"></param>
    /// <returns></returns>
    private GameObject GetCellGOByPosition(Vector3 cellPos)
    {
        return GameObject.Find($"Cell({cellPos.x},{cellPos.y})");
    }

    /// <summary>
    /// Detecta si lo clickado es un LineRenderer, y devuelve si es así,
    /// el LineRenderer, y la posición del LineRenderer tocada
    /// </summary>
    /// <param name="cellGO"></param>
    /// <param name="actualLineRenderer"></param>
    /// <param name="posIndex"></param>
    /// <returns></returns>
    private bool IsAnActualLineRenderer(GameObject cellGO, out LineRenderer actualLineRenderer, out int posIndex)
    {
        bool isActualLineRenderer = false;
        actualLineRenderer = null;
        posIndex = -1;

        for (int i = 0; i < _lineRenderers.Count; i++)
        {
            for (int j = 0; j < _lineRenderers[i].positionCount; j++)
            {
                if (cellGO.transform.position == _lineRenderers[i].GetPosition(j))
                {
                    posIndex = j;
                    isActualLineRenderer = true;
                    actualLineRenderer = _lineRenderers[i];
                    break;
                }
            }

        }

        if (actualLineRenderer != null)
        {
            GameObject lastCell = GetCellGOByPosition(actualLineRenderer.GetPosition(actualLineRenderer.positionCount - 1));

            if (IsThereNode(lastCell, out Node node))
            {
                // Si hay casillas pintadas las despintamos
                for (int i = 0; i < actualLineRenderer.positionCount; i++)
                {
                    GameObject cell = GetCellGOByPosition(actualLineRenderer.GetPosition(i));

                    if (cell.transform.GetChild(1).gameObject.activeSelf)
                    {
                        cell.transform.GetChild(1).gameObject.SetActive(false);
                    }
                }

                _lineRenderers.Remove(actualLineRenderer);
                Destroy(actualLineRenderer.gameObject);

                isActualLineRenderer = false;
                actualLineRenderer = null;
                posIndex = -1;
            }
        }

        return isActualLineRenderer;
    }

    /// <summary>
    /// Te dice si ha tocado un nodo o no, y te devuelve que nodo ha tocado
    /// </summary>
    /// <param name="cellGO"></param>
    /// <param name="refNode"></param>
    /// <returns></returns>
    private bool IsThereNode(GameObject cellGO, out Node refNode)
    {
        List<Node> nodes = NodesGenerator.Instance.Nodes;
        Node detectedNode = null;

        bool isThereNode = false;

        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].AssociedCell.AssociedGO.GetHashCode() == cellGO.GetHashCode())
            {
                detectedNode = nodes[i];
                isThereNode = true;
                break;
            }
        }

        refNode = detectedNode;
        return isThereNode;
    }

    /// <summary>
    /// Comprueba si el movimiento desde la última celda hasta la celda indicada
    /// rompe alguna de las reglas del juego.
    ///
    /// También comprueba todas las celdas intermedias
    /// </summary>
    private bool ItBreaksSomeRule(GameObject targetCell)
    {
        if (_cellsPath.Count == 0)
            return false;

        Vector3 startPos = _cellsPath.Last().transform.position;
        Vector3 targetPos = targetCell.transform.position;

        if (_cellsPath.Count > 1 && IsThereNode(_cellsPath.Last(), out Node lastNode))
        {
            return true;
        }

        int xDif = Mathf.RoundToInt(targetPos.x - startPos.x);
        int yDif = Mathf.RoundToInt(targetPos.y - startPos.y);

        if (xDif != 0 && yDif != 0)
            return true;


        // Obtenemos todas las posiciones que se han atravesado
        List<Vector3> positionsToCheck = new List<Vector3>();

        int steps = Mathf.Max(Mathf.Abs(xDif), Mathf.Abs(yDif));

        if (steps > 1)
        {
            int xStep = xDif == 0 ? 0 : xDif / Mathf.Abs(xDif);
            int yStep = yDif == 0 ? 0 : yDif / Mathf.Abs(yDif);

            for (int i = 1; i < steps; i++)
            {
                Vector3 intermediatePos = new Vector3(
                    startPos.x + xStep * i,
                    startPos.y + yStep * i,
                    0f
                );

                positionsToCheck.Add(intermediatePos);
            }
        }


        // También comprobamos la celda destino
        positionsToCheck.Add(targetPos);

        // Comprobamos cada celda atravesada
        foreach (Vector3 position in positionsToCheck)
        {
            GameObject cell = GetCellGOByPosition(position);

            if (cell == null)
                continue;

            // Comprueba si la celda ya pertenece al camino actual
            if (_cellsPath.Contains(cell))
            {
                // La celda destino puede ser una posición antigua del camino.
                if (cell == targetCell)
                    continue;

                return true;
            }


            // Comprueba si hemos atravesado un nodo
            if (IsThereNode(cell, out Node node))
            {
                // El nodo ya está conectado por otro LineRenderer
                if (node.AssociedLineRenderer != null &&
                    node.AssociedLineRenderer != _activeLineRenderer)
                {
                    return true;
                }

                // Si es un nodo pero no es el destino, no debe
                // atravesarlo.
                if (cell != targetCell)
                {
                    return true;
                }

                continue;
            }

            // Comprueba si hemos atravesado una línea existente
            foreach (LineRenderer lr in _lineRenderers)
            {
                // No comprobamos nuestra propia línea
                if (lr == _activeLineRenderer)
                    continue;

                for (int i = 0; i < lr.positionCount; i++)
                {
                    if (lr.GetPosition(i) == position)
                        return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Pasando un Color hace un Gradiante que sea como un color plano
    /// </summary>
    /// <param name="basicColor"></param>
    /// <returns></returns>
    private Gradient CreateSimpleColorGradient(Color basicColor)
    {
        Gradient g = new Gradient();
        GradientColorKey[] gck = new GradientColorKey[2];
        GradientAlphaKey[] gak = new GradientAlphaKey[2];

        gck[0].color = basicColor;
        gck[0].time = 0.0f;
        gck[1].color = basicColor;
        gck[1].time = 1.0f;

        gak[0].alpha = 1.0f;
        gak[0].time = 0.0f;
        gak[1].alpha = 1.0f;
        gak[1].time = 1.0f;

        g.SetKeys(gck, gak);

        return g;
    }

    /// <summary>
    /// Averigua si se ha hecho bien el camino
    /// </summary>
    /// <returns></returns>
    private bool IsLineRendererPathCorrect()
    {
        bool isCorrectPath = false;

        if (_activeLineRenderer.positionCount > 0)
            isCorrectPath = _actualNode.NodePair.AssociedCell.AssociedGO.GetHashCode() == _cellsPath.Last().GetHashCode();

        return isCorrectPath;
    }

    /// <summary>
    /// Averigua si el ratón se ha movido a una celda nueva
    /// que no estuviera en el camino de celdas recorridas
    /// </summary>
    /// <param name="cellGO"></param>
    /// <returns></returns>
    private bool IsANewCell(GameObject cellGO)
    {
        bool isANewCell = true;

        for (int i = 0; i < _cellsPath.Count; i++)
        {
            if (cellGO.GetHashCode() == _cellsPath[i].GetHashCode())
            {
                isANewCell = false;
                break;
            }
        }

        return isANewCell;
    }

    /// <summary>
    /// Averigua si el ratón se ha movido a una celda antigua,
    /// y además devuelve la posición respecto al array de posiciones
    /// que tiene el LineRenderer
    /// </summary>
    /// <param name="cellGO"></param>
    /// <param name="oldPosIndex"></param>
    /// <returns></returns>
    private bool IsAnOldPos(GameObject cellGO, out int oldPosIndex)
    {
        bool isAnOldPos = false;
        oldPosIndex = -1;

        for (int i = 0; i < _activeLineRenderer.positionCount - 1; i++)
        {
            if (_activeLineRenderer.GetPosition(i) == cellGO.transform.position)
            {
                isAnOldPos = true;
                oldPosIndex = i;
                break;
            }
        }

        return isAnOldPos;
    }

    /// <summary>
    /// Corrige el camino del LineRenderer añadiendo las celdas que el ratón
    /// se haya saltado debido a un movimiento rápido.
    /// </summary>
    private void CorrectLineRendererPath()
    {
        if (_cellsPath.Count == 0 || _activeLineRenderer == null)
            return;


        List<GameObject> correctedCells = new List<GameObject>();

        correctedCells.Add(_cellsPath[0]);


        // Recorremos el camino actual y añadimos las celdas
        // intermedias que falten.
        for (int i = 1; i < _cellsPath.Count; i++)
        {
            GameObject previousCell = correctedCells.Last();
            GameObject currentCell = _cellsPath[i];

            Vector3 previousPos = previousCell.transform.position;
            Vector3 currentPos = currentCell.transform.position;

            int xDif = Mathf.RoundToInt(currentPos.x - previousPos.x);
            int yDif = Mathf.RoundToInt(currentPos.y - previousPos.y);

            int steps = Mathf.Max(Mathf.Abs(xDif), Mathf.Abs(yDif));

            int xStep = xDif == 0 ? 0 : xDif / Mathf.Abs(xDif);
            int yStep = yDif == 0 ? 0 : yDif / Mathf.Abs(yDif);


            // Añadimos las celdas intermedias
            for (int j = 1; j < steps; j++)
            {
                Vector3 intermediatePos = new Vector3(
                    previousPos.x + xStep * j,
                    previousPos.y + yStep * j,
                    0f
                );

                GameObject intermediateCell = GetCellGOByPosition(intermediatePos);

                if (intermediateCell != null)
                {
                    correctedCells.Add(intermediateCell);
                }
            }

            correctedCells.Add(currentCell);
        }

        _cellsPath.Clear();
        _cellsPath.AddRange(correctedCells);

        _activeLineRenderer.positionCount = _cellsPath.Count;

        for (int i = 0; i < _cellsPath.Count; i++)
        {
            _activeLineRenderer.SetPosition(
                i,
                _cellsPath[i].transform.position
            );
        }
    }

    /// <summary>
    /// Quita varias partes de camino de golpe para así retroceder
    /// varias celdas
    /// </summary>
    /// <param name="index"></param>
    private void GoToOldPosition(int index)
    {
        //_activeLineRenderer.positionCount = (index+1);
        _cellsPath.RemoveRange(index, _cellsPath.Count - index);
        Vector3[] newLineRendererPath = new Vector3[_cellsPath.Count];

        for (int i = 0; i < _cellsPath.Count; i++)
        {
            newLineRendererPath[i] = _cellsPath[i].transform.position;
        }

        _activeLineRenderer.positionCount = newLineRendererPath.Length;
        _activeLineRenderer.SetPositions(newLineRendererPath);
    }

    private int GetTotalPositionsFilled()
    {
        int totalPositionsFilled = 0;

        foreach (LineRenderer lr in _lineRenderers)
        {
            totalPositionsFilled += lr.positionCount;
        }

        return totalPositionsFilled;
    }

    private bool IsAllCorrect()
    {
        Node[] nodes = NodesGenerator.Instance.Nodes.ToArray();
        foreach (Node node in nodes)
        {
            if (!node.IsCorrectPath)
            {
                return false;
            }
        }

        return true;
    }

    public void ClearLineRenderers()
    {
        _lineRenderers.Clear();
    }

    public void AddLineRendererToList(LineRenderer lr)
    {
        _lineRenderers.Add(lr);
    }

    private void OnReset()
    {
        _pairsConnected = 0;

        CanTouch = false;
        _activeLineRenderer = null;
        _actualNode = null;

        foreach (LineRenderer lr in _lineRenderers)
        {
            Destroy(lr.gameObject);
        }

        _lineRenderers.Clear();
        _cellsPath.Clear();
    }
}
