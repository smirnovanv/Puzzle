using NUnit.Framework;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class BaseBoard : MonoBehaviour
{
    // ширина и высота поля
    public int width;
    public int height;

    // префабы
    public GameObject tilePrefab;
    public GameObject[] ballsPrefabs;

    // слой с шарами для регистрации нажатия
    public LayerMask ballLayer;

 
    // переменные для обработки перетаскивания шара
    private BallData selectedBall = null;
    private BallData targetBall = null;
    private Vector2 dragStartPosition;
    private bool isDragging = false;
    private float minDragDistance = 0.5f;
    private bool isInputEnabled = true;

    // основная структура для отрисовки доски и поиска совпадений
    private BallData[,] gameBoard;

    // словарь для быстрого поиска при нажатии на шар
    private Dictionary<GameObject, BallData> ballLookup;

    // структуры для поиска совпадений
    private List<MatchInfo> horizontalMatches = new List<MatchInfo>();
    private List<MatchInfo> verticalMatches = new List<MatchInfo>();
    private HashSet<BallData> allMatchedBalls = new HashSet<BallData>();

    void Start()
    {
        gameBoard = new BallData[width, height];
        ballLookup = new Dictionary<GameObject, BallData>();
        CreateBoard();
        VisualizeBoard();
    }
    
    void Update()
    {
        if (!isInputEnabled) return;

        if (Input.GetMouseButtonDown(0))
        {
            StartDrag();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndDrag();
            // сценарий успешного хода
            // EndDrag -> SwapBalls -> CheckForMatches
        }
    }

    // метод создания сетки со случайным наполнением шарами
    // TODO: доработать для создания доски по сценарию для уровней
    private void CreateBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                int ballTypeToUse;
                do
                {
                    // Пробуем случайный тип
                    ballTypeToUse = Random.Range(0, ballsPrefabs.Length);
                }
                // Проверяем, не создаст ли это 3 в ряд
                while (HasMatchAt(i, j, ballTypeToUse));

                gameBoard[i, j] = new BallData(i, j, ballTypeToUse);
            }
        }
    }

    private bool HasMatchAt(int x, int y, int ballType)
    {
        // Проверка по горизонтали (влево)
        if (x >= 2)
        {
            BallData left1 = gameBoard[x - 1, y];
            BallData left2 = gameBoard[x - 2, y];

            if (left1 != null && left2 != null &&
                left1.type == ballType && left2.type == ballType)
            {
                return true;
            }
        }

        // Проверка по вертикали (вниз/вверх - зависит от порядка заполнения)
        if (y >= 2)
        {
            BallData down1 = gameBoard[x, y - 1];
            BallData down2 = gameBoard[x, y - 2];

            if (down1 != null && down2 != null &&
                down1.type == ballType && down2.type == ballType)
            {
                return true;
            }
        }

        return false;
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
                    Vector3 tempPosition = new Vector3(x, y, 0);

                    // рендер плиток
                    GameObject backgroundTile = Instantiate(tilePrefab, tempPosition, Quaternion.identity);
                    backgroundTile.transform.parent = this.transform;
                    backgroundTile.name = $"Tile ({x}, {y})";

                    // рендер шаров
                    GameObject ballPrefab = ballsPrefabs[ballData.type];
                    GameObject ballVisual = Instantiate(ballPrefab, tempPosition, Quaternion.identity);
                    ballVisual.transform.parent = this.transform;
                    ballVisual.name = $"Ball ({x}, {y}) - {ballData.type}";

                    int ballsLayerIndex = LayerMask.NameToLayer("Balls");

                    if (ballsLayerIndex != -1)
                    {
                        ballVisual.layer = ballsLayerIndex;
                    }

                    ballData.visualObject = ballVisual;

                    // Сохраняем в словарь для быстрого поиска
                    ballLookup[ballVisual] = ballData;
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

            if (ballLookup.TryGetValue(clickedBall, out BallData ballData))
            {
                selectedBall = ballData;
                dragStartPosition = Input.mousePosition;
                isDragging = true;

                Debug.Log($"Start drag at ({ballData.x}, {ballData.y})");
                return;
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
                targetBall = gameBoard[targetPos.x, targetPos.y];

                if (targetBall != null)
                {
                    // Меняем шарики местами
                    StartCoroutine(SwapBalls(selectedBall, targetBall));
                }
            }
        }

        // Снимаем выделение
        // selectedBall = null;
        isDragging = false;
    }

    // Вспом. методы для определения направления перемещения шарика (EndDrag): НАЧАЛО
    private bool IsValidPosition(int x, int y)
    {
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
    // Вспом. методы для определения направления перемещения шарика (EndDrag): КОНЕЦ

    // изменение позиции шаров визуально и в модели доски
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

        // не вносим изменения в словарь ballLookup, т.к. при совпадении удалим объекты (тогда фиксируем в словарь)
        // либо шары вернутся назад

        // Проверяем совпадения
        CheckForMatches();

        isInputEnabled = true;
    }

    // Вспом. методы для поиска совпадений CheckForMatches: НАЧАЛО
    public List<MatchInfo> FindAllMatches()
    {
        ClearMatchData();

        FindHorizontalMatches();
        FindVerticalMatches();

        // Объединяем все совпадения
        List<MatchInfo> allMatches = new List<MatchInfo>();
        allMatches.AddRange(horizontalMatches);
        allMatches.AddRange(verticalMatches);

        LogMatchesInfo(allMatches);

        // Отмечаем шарики как совпавшие
        MarkMatchedBalls(allMatches);

        // Определяем бонусы на основе совпадений
       // todo DetermineBonuses(allMatches);

        return allMatches;
    }

    private void LogMatchesInfo(List<MatchInfo> allMatches)
    {
        Debug.Log($"=== Всего совпадений: {allMatches.Count} ===");

        for (int i = 0; i < allMatches.Count; i++)
        {
            MatchInfo match = allMatches[i];

            Debug.Log($"Совпадение #{i + 1}:");
            Debug.Log($"  Тип: {(match.isHorizontal ? "Горизонтальное" : "Вертикальное")}");
            Debug.Log($"  Длина: {match.matchLength} шариков");
            Debug.Log($"  Начало: ({match.startPos.x}, {match.startPos.y})");
            Debug.Log($"  Конец: ({match.endPos.x}, {match.endPos.y})");
            Debug.Log($"  Тип шарика: {match.matchedBalls[0].type}");
            Debug.Log($"  Количество шариков: {match.matchedBalls.Count}");

            // Выводим координаты всех шариков в совпадении
            string positions = "  Позиции: ";
            foreach (BallData ball in match.matchedBalls)
            {
                positions += $"({ball.x},{ball.y}) ";
            }
            Debug.Log(positions);

            Debug.Log("---");
        }
    }

    private void ClearMatchData()
    {
        horizontalMatches.Clear();
        verticalMatches.Clear();
        allMatchedBalls.Clear();

        // Сбрасываем флаги совпадений у всех шариков
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (gameBoard[x, y] != null)
                {
                    gameBoard[x, y].isMatched = false;
                }
            }
        }
    }

    private void FindHorizontalMatches()
    {
        for (int y = 0; y < height; y++)
        {
            int currentType = -1;
            int matchStart = 0;
            int matchLength = 0;

            for (int x = 0; x < width; x++)
            {
                BallData ball = gameBoard[x, y];

                if (ball != null && ball.type == currentType)
                {
                    matchLength++;
                }
                else
                {
                    // Сохраняем предыдущее совпадение, если оно было
                    if (matchLength >= 3)
                    {
                        SaveHorizontalMatch(matchStart, y, matchLength, currentType);
                    }

                    // Начинаем новое потенциальное совпадение
                    currentType = ball?.type ?? -1;
                    matchStart = x;
                    matchLength = ball != null ? 1 : 0;
                }
            }

            // Проверяем совпадение в конце строки
            if (matchLength >= 3)
            {
                SaveHorizontalMatch(matchStart, y, matchLength, currentType);
            }
        }
    }

    private void SaveHorizontalMatch(int startX, int y, int length, int type)
    {
        MatchInfo match = new MatchInfo
        {
            isHorizontal = true,
            matchLength = length,
            startPos = new Vector2Int(startX, y),
            endPos = new Vector2Int(startX + length - 1, y)
        };

        for (int x = startX; x < startX + length; x++)
        {
            BallData ball = gameBoard[x, y];
            if (ball != null && ball.type == type)
            {
                match.AddBall(ball);
            }
        }

        horizontalMatches.Add(match);
    }

    private void FindVerticalMatches()
    {
        for (int x = 0; x < width; x++)
        {
            int currentType = -1;
            int matchStart = 0;
            int matchLength = 0;

            for (int y = 0; y < height; y++)
            {
                BallData ball = gameBoard[x, y];

                if (ball != null && ball.type == currentType)
                {
                    matchLength++;
                }
                else
                {
                    if (matchLength >= 3)
                    {
                        SaveVerticalMatch(x, matchStart, matchLength, currentType);
                    }

                    currentType = ball?.type ?? -1;
                    matchStart = y;
                    matchLength = ball != null ? 1 : 0;
                }
            }

            if (matchLength >= 3)
            {
                SaveVerticalMatch(x, matchStart, matchLength, currentType);
            }
        }
    }

    private void SaveVerticalMatch(int x, int startY, int length, int type)
    {
        MatchInfo match = new MatchInfo
        {
            isVertical = true,
            matchLength = length,
            startPos = new Vector2Int(x, startY),
            endPos = new Vector2Int(x, startY + length - 1)
        };

        for (int y = startY; y < startY + length; y++)
        {
            BallData ball = gameBoard[x, y];
            if (ball != null && ball.type == type)
            {
                match.AddBall(ball);
            }
        }

        verticalMatches.Add(match);
    }

    private void MarkMatchedBalls(List<MatchInfo> allMatches)
    {
        foreach (MatchInfo match in allMatches)
        {
            foreach (BallData ball in match.matchedBalls)
            {
                ball.isMatched = true;
                allMatchedBalls.Add(ball);

                // Визуально помечаем шарик (подсветка на короткое время)
                if (ball.visualObject != null)
                {
                    StartCoroutine(HighlightMatchedBall(ball.visualObject));
                }
            }
        }
    }

    private System.Collections.IEnumerator HighlightMatchedBall(GameObject ballVisual)
    {
        MeshRenderer meshRenderer = ballVisual.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            Material originalMaterial = meshRenderer.material;
            Color originalColor = originalMaterial.color;

            // Меняем цвет материала
            meshRenderer.material.color = Color.white;
            yield return new WaitForSeconds(0.5f);

            // Возвращаем оригинальный цвет
            meshRenderer.material.color = originalColor;
        }
        else
        {
            Debug.LogWarning($"No MeshRenderer found on {ballVisual.name}");
        }
    }
    // Вспом. методы для поиска совпадений CheckForMatches: КОНЕЦ

    // Основной метод проверки совпадений после свапа
    private bool CheckForMatches()
    {
        List<MatchInfo> allMatches = FindAllMatches();

        BallData ball1 = selectedBall;
        BallData ball2 = targetBall;

        ClearBallSelection();

        if (allMatches.Count > 0)
        {
          // Удаляем совпавшие шарики
            StartCoroutine(RemoveMatchedBalls());
            return true;
        }

        // todo меняем местами на доске визуально и в модели доски
        StartCoroutine(UnswapBalls(ball1, ball2));

        return false;
    }

    private bool CheckForCascadeMatches()
    {
        List<MatchInfo> allMatches = FindAllMatches();

        if (allMatches.Count > 0)
        {
            StartCoroutine(RemoveMatchedBalls());
            return true;
        }
        return false;
    }

    private void ClearBallSelection() {
        selectedBall = null;
        targetBall = null;
    }

    private System.Collections.IEnumerator UnswapBalls(BallData ball1, BallData ball2) {
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

        // не вносим изменения в словарь ballLookup, т.к. при совпадении удалим объекты (тогда фиксируем в словарь)
        // либо шары вернутся назад

        isInputEnabled = true;
    }

    private System.Collections.IEnumerator RemoveMatchedBalls()
    {
        // Сначала показываем анимацию совпадения
        yield return new WaitForSeconds(0.5f);

        foreach (BallData ball in allMatchedBalls)
        {
            if (ball.visualObject != null)
            {
                // очищаем эти объекты в словаре
                if (ballLookup.ContainsKey(ball.visualObject))
                {
                    ballLookup.Remove(ball.visualObject);
                }

                // удаляем сами предметы
                Destroy(ball.visualObject);
                ball.visualObject = null;
            }

            // Очищаем клетку в модели сетки
            gameBoard[ball.x, ball.y] = null;
            Debug.Log($"Удален шарик на позиции ({ball.x}, {ball.y})");
        }

        // Сдвигаем шарики вниз
        yield return StartCoroutine(ShiftBallsDown());

        // Заполняем пустые клетки
        yield return StartCoroutine(FillEmptySpaces());

        // Проверяем новые совпадения (каскадные совпадения)
        bool newMatches = CheckForCascadeMatches();

        // Если есть новые совпадения, повторяем процесс
        while (newMatches)
        {
            yield return new WaitForSeconds(0.5f); // Пауза между каскадами
            newMatches = CheckForCascadeMatches();
        }
    }

    private System.Collections.IEnumerator ShiftBallsDown()
    {
        Debug.Log("=== НАЧАЛО СДВИГА ШАРИКОВ ВНИЗ ===");

        // 1. Собираем ВСЕ шарики, которые нужно сдвинуть, и сразу вычисляем их конечные позиции
        List<BallMovement> ballMovements = new List<BallMovement>();

        // Проходим по всем столбцам
        for (int x = 0; x < width; x++)
        {
            int emptyCellsBelow = 0;

            // Проходим снизу вверх
            for (int y = 0; y < height; y++)
            {
                if (gameBoard[x, y] == null)
                {
                    // Нашли пустую клетку - увеличиваем счетчик пустых клеток ниже
                    emptyCellsBelow++;
                }
                else if (emptyCellsBelow > 0)
                {
                    // Нашли шарик над пустыми клетками - нужно сдвинуть
                    BallData ball = gameBoard[x, y];
                    int targetY = y - emptyCellsBelow; // Позиция после сдвига

                    // Сохраняем информацию о движении
                    ballMovements.Add(new BallMovement
                    {
                        ball = ball,
                        startX = x,
                        startY = y,
                        targetX = x,
                        targetY = targetY,
                        distance = emptyCellsBelow
                    });
                }
            }
        }

        // Если нечего двигать - выходим
        if (ballMovements.Count == 0)
        {
            Debug.Log("Нет шариков для сдвига");
            yield break;
        }

        Debug.Log($"Найдено {ballMovements.Count} шариков для сдвига");

        // 2. СНАЧАЛА обновляем игровую модель (чтобы не было коллизий)
        foreach (BallMovement movement in ballMovements)
        {
            // Очищаем старую позицию
            gameBoard[movement.startX, movement.startY] = null;

            // Устанавливаем новую позицию
            movement.ball.x = movement.targetX;
            movement.ball.y = movement.targetY;
            gameBoard[movement.targetX, movement.targetY] = movement.ball;
        }

        // 3. Запускаем ВСЕ анимации одновременно
        List<Coroutine> animations = new List<Coroutine>();

        foreach (BallMovement movement in ballMovements)
        {
            if (movement.ball.visualObject != null)
            {
                Vector3 targetPosition = new Vector3(movement.targetX, movement.targetY, 0);
                float duration = CalculateFallDuration(movement.distance); // Длительность зависит от расстояния

                Coroutine animation = StartCoroutine(
                    MoveBallWithGravity(movement.ball.visualObject, targetPosition, duration));
                animations.Add(animation);
            }
        }

        // 4. Ждем завершения ВСЕХ анимаций
        foreach (Coroutine animation in animations)
        {
            yield return animation;
        }

        Debug.Log($"=== СДВИГ ЗАВЕРШЕН ===");
        Debug.Log($"Всего сдвинуто {ballMovements.Count} шариков");
    }

    // Структура для хранения информации о движении шарика
    private class BallMovement
    {
        public BallData ball;
        public int startX;
        public int startY;
        public int targetX;
        public int targetY;
        public int distance; // На сколько клеток вниз сдвигается
    }

    // Метод для расчета длительности падения в зависимости от расстояния
    private float CalculateFallDuration(float distance)
    {
        // Базовое время + дополнительное время за каждую клетку
        return 0.1f + (distance * 0.08f);
    }

    // анимация с эффектом гравитации
    private System.Collections.IEnumerator MoveBallWithGravity(GameObject ballVisual, Vector3 targetPosition, float duration)
    {
        if (ballVisual == null) yield break;

        Vector3 startPosition = ballVisual.transform.position;
        float distance = Vector3.Distance(startPosition, targetPosition);

        // Добавляем небольшую случайную задержку для эффекта "каскада"
        float randomDelay = Random.Range(0f, 0.05f);
        yield return new WaitForSeconds(randomDelay);

        float elapsed = 0;

        while (elapsed < duration)
        {
            float progress = elapsed / duration;

            // Используем физическую формулу для свободного падения с отскоком
            // y = -a*t² + v₀*t + y₀
            float t = progress;

            // Параболическая траектория (ускорение вниз)
            float verticalProgress = t * t; // Ускорение

            // Горизонтальное движение - линейное
            float horizontalProgress = t;

            // Вычисляем промежуточную позицию
            Vector3 currentPos = new Vector3(
                Mathf.Lerp(startPosition.x, targetPosition.x, horizontalProgress),
                Mathf.Lerp(startPosition.y, targetPosition.y, verticalProgress),
                0
            );

            // Добавляем легкое раскачивание (опционально)
            float swing = Mathf.Sin(progress * Mathf.PI * 2) * 0.05f * (1 - progress);
            currentPos.x += swing;

            ballVisual.transform.position = currentPos;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Гарантируем точное позиционирование в конце
        ballVisual.transform.position = targetPosition;

        // Небольшой "отскок" в конце (опционально)
        yield return StartCoroutine(BounceEffect(ballVisual, targetPosition));
    }

    // Эффект отскока при приземлении
    private System.Collections.IEnumerator BounceEffect(GameObject ballVisual, Vector3 basePosition)
    {
        float bounceHeight = 0.1f;
        float bounceDuration = 0.1f;
        float elapsed = 0;

        Vector3 bouncePosition = basePosition + Vector3.up * bounceHeight;

        // Вверх
        while (elapsed < bounceDuration / 2)
        {
            float progress = elapsed / (bounceDuration / 2);
            ballVisual.transform.position = Vector3.Lerp(basePosition, bouncePosition, progress);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Вниз
        elapsed = 0;
        while (elapsed < bounceDuration / 2)
        {
            float progress = elapsed / (bounceDuration / 2);
            ballVisual.transform.position = Vector3.Lerp(bouncePosition, basePosition, progress);
            elapsed += Time.deltaTime;
            yield return null;
        }

        ballVisual.transform.position = basePosition;
    }

    private System.Collections.IEnumerator FillEmptySpaces()
    {
        Debug.Log("=== ЗАПОЛНЕНИЕ ПУСТЫХ КЛЕТОК ===");

        // Собираем все пустые клетки, которые нужно заполнить
        List<Vector2Int> emptyCells = new List<Vector2Int>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (gameBoard[x, y] == null)
                {
                    emptyCells.Add(new Vector2Int(x, y));
                }
            }
        }

        Debug.Log($"Найдено {emptyCells.Count} пустых клеток для заполнения");

        // Генерируем новые шарики для каждой пустой клетки
        List<BallGeneration> newBalls = new List<BallGeneration>();

        foreach (Vector2Int cell in emptyCells)
        {
            int newType;
            do
            {
                newType = Random.Range(0, ballsPrefabs.Length);
            }
            while (HasMatchAt(cell.x, cell.y, newType));

            BallData newBall = new BallData(cell.x, cell.y, newType);
            gameBoard[cell.x, cell.y] = newBall;

            // Создаем визуальный объект выше доски
            Vector3 startPosition = new Vector3(cell.x, height + 1, 0);
            GameObject ballPrefab = ballsPrefabs[newType];
            GameObject ballVisual = Instantiate(ballPrefab, startPosition, Quaternion.identity);
            ballVisual.transform.parent = this.transform;
            ballVisual.name = $"New Ball ({cell.x}, {cell.y}) - {newType}";

            // Устанавливаем слой
            int ballsLayerIndex = LayerMask.NameToLayer("Balls");
            if (ballsLayerIndex != -1)
            {
                ballVisual.layer = ballsLayerIndex;
            }

            newBall.visualObject = ballVisual;
            ballLookup[ballVisual] = newBall;

            newBalls.Add(new BallGeneration
            {
                ball = newBall,
                targetPosition = new Vector3(cell.x, cell.y, 0),
                fallDistance = height + 1 - cell.y
            });
        }

        // Запускаем анимации падения всех новых шариков одновременно
        List<Coroutine> animations = new List<Coroutine>();

        foreach (BallGeneration generation in newBalls)
        {
            float duration = CalculateFallDuration(generation.fallDistance);
            Coroutine animation = StartCoroutine(
                DropNewBall(generation.ball.visualObject, generation.targetPosition, duration));
            animations.Add(animation);
        }

        // Ждем завершения всех анимаций
        foreach (Coroutine animation in animations)
        {
            yield return animation;
        }

        Debug.Log($"=== ЗАПОЛНЕНИЕ ЗАВЕРШЕНО ===");
    }

    private class BallGeneration
    {
        public BallData ball;
        public Vector3 targetPosition;
        public float fallDistance;
    }

    private System.Collections.IEnumerator DropNewBall(GameObject ballVisual, Vector3 targetPosition, float duration)
    {
        if (ballVisual == null) yield break;

        Vector3 startPosition = ballVisual.transform.position;
        float elapsed = 0;

        // Небольшая случайная задержка для эффекта каскада
        float randomDelay = Random.Range(0f, 0.1f);
        yield return new WaitForSeconds(randomDelay);

        while (elapsed < duration)
        {
            float progress = elapsed / duration;
            float t = progress;

            // Параболическая траектория с ускорением
            float verticalProgress = t * t;
            float horizontalProgress = t;

            Vector3 currentPos = new Vector3(
                Mathf.Lerp(startPosition.x, targetPosition.x, horizontalProgress),
                Mathf.Lerp(startPosition.y, targetPosition.y, verticalProgress),
                0
            );

            ballVisual.transform.position = currentPos;
            elapsed += Time.deltaTime;
            yield return null;
        }

        ballVisual.transform.position = targetPosition;

        // Эффект отскока при приземлении
        yield return StartCoroutine(BounceEffect(ballVisual, targetPosition));
    }

}
