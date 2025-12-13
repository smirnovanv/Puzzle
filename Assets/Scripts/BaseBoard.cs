using NUnit.Framework;
using UnityEditorInternal;
using UnityEngine;

public class BaseBoard : MonoBehaviour
{
    public int width;
    public int height;
    public GameObject tilePrefab;
    private BackgroundTile[,] allTiles;
    public GameObject[] ballsPrefabs;
    private BallData[,] gameBoard;
    private BallData selectedBall = null;
    private Vector2 dragStartPosition;
    private bool isDragging = false;
    private float minDragDistance = 0.5f;
    public LayerMask ballLayer;
    private bool isInputEnabled = true;
    void Start()
    {
        allTiles = new BackgroundTile[width, height];
        gameBoard = new BallData[width, height];
        CreateBoard();
        VisualizeBoard();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isInputEnabled) return;

        if (Input.GetMouseButtonDown(0))
        {
            StartDrag();
            Debug.Log("StartDrag");
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndDrag();
            Debug.Log("EndDrag");
        }
    }
    private void CreateBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vector3 tempPosition = new Vector3(i, j, 0);
                GameObject backgroundTile = Instantiate(tilePrefab, tempPosition, Quaternion.identity) as GameObject;
                backgroundTile.transform.parent = this.transform;
                backgroundTile.name = "( " + i + ", " + j + ", " + 0 + " )";

                int ballTypeToUse = Random.Range(0, ballsPrefabs.Length);
                gameBoard[i, j] = new BallData(i, j, ballTypeToUse);
            }

        }
    }
    private void VisualizeBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                BallData ballData = gameBoard[x, y];

                if (ballData != null)
                {

                        GameObject prefab = ballsPrefabs[ballData.type];
                        Vector3 position = new Vector3(x, y, 0);
                        GameObject ballVisual = Instantiate(prefab, position, Quaternion.identity);
                        ballVisual.transform.parent = this.transform;
                        ballVisual.name = $"Ball ({x}, {y}) - {ballData.type}";
                    int ballsLayerIndex = LayerMask.NameToLayer("Balls");
                    if (ballsLayerIndex != -1)
                    {
                        ballVisual.layer = ballsLayerIndex;
                    }

                    ballData.visualObject = ballVisual;
                }
            }
        }
    }
    private void StartDrag()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, ballLayer))
        {
            GameObject clickedBall = hit.collider.gameObject;

            // Находим BallData
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    BallData ballData = gameBoard[x, y];
                    if (ballData != null && ballData.visualObject == clickedBall)
                    {
                        selectedBall = ballData;
                        dragStartPosition = Input.mousePosition;
                        isDragging = true;

                        Debug.Log($"Start drag at ({ballData.x}, {ballData.y})");
                        return;
                    }
                }
            }
        }
    }
    private void EndDrag()
    {
        if (!isDragging || selectedBall == null) return;

        Vector2 dragEndPosition = Input.mousePosition;
        Vector2 dragVector = dragEndPosition - dragStartPosition;

        // Проверяем, достаточно ли большое движение
        if (dragVector.magnitude >= minDragDistance)
        {
            // Определяем направление
            Direction direction = GetDragDirection(dragVector);

            // Получаем координаты целевой клетки
            Vector2Int targetPos = GetTargetPosition(selectedBall.x, selectedBall.y, direction);

            Debug.Log($"End drag at ({targetPos.x}, {targetPos.y})");

            //Проверяем, есть ли шарик в целевой клетке
            if (IsValidPosition(targetPos.x, targetPos.y))
            {
                BallData targetBall = gameBoard[targetPos.x, targetPos.y];
                if (targetBall != null)
                {
                    // Меняем шарики местами
                    StartCoroutine(SwapBalls(selectedBall, targetBall));
                }
            }
        }

        // Снимаем выделение
        selectedBall = null;
        isDragging = false;
    }
    private bool IsValidPosition(int x, int y)
    {
        // Проверяем, находится ли позиция в пределах игрового поля
        return x >= 0 && x < width && y >= 0 && y < height;
    }
    private enum Direction { Up, Down, Left, Right, None }
    private Direction GetDragDirection(Vector2 dragVector)
    {
        // Определяем основное направление по наибольшей компоненте
        if (Mathf.Abs(dragVector.x) > Mathf.Abs(dragVector.y))
        {
            return dragVector.x > 0 ? Direction.Right : Direction.Left;
        }
        else
        {
            return dragVector.y > 0 ? Direction.Up : Direction.Down;
        }
    }
    private Vector2Int GetTargetPosition(int x, int y, Direction direction)
    {
        return direction switch
        {
            Direction.Up => new Vector2Int(x, y + 1),
            Direction.Down => new Vector2Int(x, y - 1),
            Direction.Left => new Vector2Int(x - 1, y),
            Direction.Right => new Vector2Int(x + 1, y),
            _ => new Vector2Int(x, y)
        };
    }
    private System.Collections.IEnumerator SwapBalls(BallData ball1, BallData ball2)
    {
        isInputEnabled = false;

        Debug.Log($"Swapping ({ball1.x},{ball1.y}) with ({ball2.x},{ball2.y})");

        // Визуальная анимация обмена
        Vector3 pos1 = ball1.visualObject.transform.position;
        Vector3 pos2 = ball2.visualObject.transform.position;

        float duration = 0.3f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            ball1.visualObject.transform.position = Vector3.Lerp(pos1, pos2, elapsed / duration);
            ball2.visualObject.transform.position = Vector3.Lerp(pos2, pos1, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Обновляем позиции
        ball1.visualObject.transform.position = pos2;
        ball2.visualObject.transform.position = pos1;

        // Меняем данные в игровой модели
        int tempX = ball1.x;
        int tempY = ball1.y;

        // Обновляем координаты в BallData
        ball1.x = ball2.x;
        ball1.y = ball2.y;
        ball2.x = tempX;
        ball2.y = tempY;

        // Меняем местами в массиве gameBoard
        gameBoard[ball1.x, ball1.y] = ball1;
        gameBoard[ball2.x, ball2.y] = ball2;

        // Проверяем совпадения
        CheckForMatches();

        isInputEnabled = true;
    }
    private void CheckForMatches()
    {
        // TODO: Реализуйте проверку совпадений
        Debug.Log("Checking for matches...");

        // После проверки совпадений:
        // 1. Удалить совпадающие шарики
        // 2. Сдвинуть шарики вниз
        // 3. Заполнить пустые клетки новыми шариками
    }
}
