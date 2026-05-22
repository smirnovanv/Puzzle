using UnityEngine;
using TMPro;

public class Level1 : MonoBehaviour
{
    public int movesCounter = 0;
    private int totalMoves = 30;

    public Goal goal1;
    public BaseBoard3 baseBoard;

    // ширина и высота поля
    public int width;
    public int height;

    // префабы
    public GameObject tilePrefab;
    public GameObject[] ballsPrefabs;

    // слой с шарами для регистрации нажатия
    public LayerMask ballLayer;

    [Header("UI")]
    [SerializeField] private GameObject movesTextPrefab; // Префаб текста
    private TextMeshProUGUI movesText;

    private MovesDisplay movesDisplay;

    public class Goal
    {
        public int collectBallType;
        public int numberToCollect;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        baseBoard = gameObject.AddComponent<BaseBoard3>();
        baseBoard.Initialize(width, height, ballsPrefabs, tilePrefab, ballLayer);
        baseBoard.OnMove += HandleMove;

        CreateMovesUI();
    }

    private void CreateMovesUI()
    {
        // Создаём отдельный GameObject для управления UI
        GameObject uiManager = new GameObject("UIManager");
        movesDisplay = uiManager.AddComponent<MovesDisplay>();

        // Настраиваем начальные значения
        movesDisplay.Initialize(movesTextPrefab, totalMoves);
        movesDisplay.UpdateMoves(movesCounter, totalMoves);
    }


    void HandleMove()
    {
        movesCounter++;

        // Обновляем UI
        if (movesDisplay != null)
        {
            movesDisplay.UpdateMoves(movesCounter, totalMoves);
        }

        // Проверяем, не закончились ли ходы
        if (movesCounter >= totalMoves)
        {
            Debug.Log("Ходы закончились!");
            // Здесь можно добавить логику окончания игры
        }

        Debug.Log($"Сделано ходов: {movesCounter}/{totalMoves}");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        // Отписываемся от ВСЕХ событий, на которые подписывались
        if (baseBoard != null)
        {
            baseBoard.OnMove -= HandleMove;
            // Если есть другие события - отписываемся и от них
        }

        // Обновляем UI
        if (movesDisplay != null)
        {
            movesDisplay.UpdateMoves(movesCounter, totalMoves);
        }

        // Проверяем, не закончились ли ходы
        if (movesCounter >= totalMoves)
        {
            Debug.Log("Ходы закончились!");
            // Здесь можно добавить логику окончания игры
        }

        Debug.Log($"Сделано ходов: {movesCounter}/{totalMoves}");
    }
}
