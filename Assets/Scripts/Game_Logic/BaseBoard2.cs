using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.Rendering.DebugUI;

public class BaseBoard2 : MonoBehaviour
{
    // ширина и высота поля
    public int width;
    public int height;

    // префабы
    public GameObject tilePrefab;
    public GameObject[] ballsPrefabs;

    // слой с шарами для регистрации нажатия
    public LayerMask ballLayer;

    private VirtualBoard board;
    private VirtualBoardRender boardRenderer;
    private InputHandler _inputHandler;

    private SwipeAnimationHandler _swipeAnimationHandler;

    private SwipeDataChanger _swipeDataChanger;
    private MatchChecker _matchChecker;
    private DeleteBallDataHandler _deleteBallDataHandler;

    private ShiftBallDataChanger _shiftBallDataChanger;
    private ShiftAnimationHandler _shiftAnimationHandler;



    void Start()
    {
        // 1. Создаем виртуальную доску
        board = new VirtualBoard(width, height, ballsPrefabs);
        board.DebugPrintBoard();

        // 2. Создаем рендерер и отрисовываем
        boardRenderer = gameObject.AddComponent<VirtualBoardRender>();
        boardRenderer.Initialize(board, tilePrefab);

        // 3. Создаем обработчик ввода
        _inputHandler = gameObject.AddComponent<InputHandler>();
        _inputHandler.Initialize(board, ballLayer);

        // 4. ПОДПИСЫВАЕМСЯ НА СОБЫТИЯ
        _inputHandler.OnSwipe += HandleSwipe; // todo добавить обработчик нажатия на шар/буст
        _inputHandler.OnDragCanceled += HandleDragCanceled;

        // 5. Создаем обработчик данных свайпа
        _swipeDataChanger = new SwipeDataChanger(board);

        // 6. Создаем обработчик анимации свайпа
        _swipeAnimationHandler = gameObject.AddComponent<SwipeAnimationHandler>();

        // 7. Создаем обработчик проверки на совпадения
        _matchChecker = new MatchChecker(board);

        // 8. Создаем обработчик удаления шаров
        _deleteBallDataHandler = gameObject.AddComponent<DeleteBallDataHandler>();
        _deleteBallDataHandler.Initialize(board);

        // 8. Создаем обработчик падения шаров
        _shiftBallDataChanger = new ShiftBallDataChanger(board);

        // 9. Создаем обработчик анимации падения шаров
        _shiftAnimationHandler = gameObject.AddComponent<ShiftAnimationHandler>();
    }

    private void HandleSwipe(CellData selectedCell, CellData targetCell)
    {
        Debug.Log("SWIPE"); // todo + virtualBoard

        StartCoroutine(ProcessSwipeCoroutine(selectedCell, targetCell));
    }

    private IEnumerator ProcessSwipeCoroutine(CellData selectedCell, CellData targetCell) {
        bool hasMatches = false;
        bool animationCompleted = false;

        // 1. Анимируем обмен
        BallData2 ball1 = selectedCell.ball;
        BallData2 ball2 = targetCell.ball;
        _swipeAnimationHandler.AnimateSwap(ball1, ball2, () => animationCompleted = true);

        yield return new WaitUntil(() => animationCompleted);

        // 2. Изменение данных доски
        _swipeDataChanger.SwapBalls(selectedCell, targetCell);

        List<MatchData> allMatches = _matchChecker.FindAllMatches();

        hasMatches = allMatches.Count > 0;

        if (hasMatches) 
        {
            Debug.Log("✅ Matches found! Move is valid.");
            _deleteBallDataHandler.RemoveMatchedBalls();

            // изменение данных доски
            List<BallShiftData> ballMovements = _shiftBallDataChanger.Change();
            List<CellData>[] cellsToFill = board.GenerateExtraBalls();
            // падение
            animationCompleted = false;
            _shiftAnimationHandler.AnimateShift(board, ballMovements, 1, () => animationCompleted = true);

            
            int levelAnimationCompleted = 0;

            for (int level = 0; level < cellsToFill.Length; level++)
            {
                List<BallShiftData> newBallMovements = new List<BallShiftData>();

                if (cellsToFill[level] == null) 
                {
                    levelAnimationCompleted++;
                } else
                {
                    foreach (CellData cell in cellsToFill[level])
                    {
                        boardRenderer.RenderNewBall(cell);
                        newBallMovements.Add(new BallShiftData
                        {
                            originalCell = cell,
                            startX = cell.x,
                            startY = board._height + 1,
                            targetX = cell.x,
                            targetY = cell.y,
                            distance = board._height + 1 - cell.y
                        });

                    }

                    _shiftAnimationHandler.AnimateShift(board, newBallMovements, level + 1, () => levelAnimationCompleted++);
                }
            }

            yield return new WaitUntil(() => animationCompleted && levelAnimationCompleted == cellsToFill.Length);
        }
        else {
            animationCompleted = false;

            _swipeAnimationHandler.AnimateSwap(ball1, ball2, () => animationCompleted = true);

            yield return new WaitUntil(() => animationCompleted);

            _swipeDataChanger.SwapBalls(selectedCell, targetCell);
        }
    }

    private void HandleDragCanceled()
    {
        Debug.Log("Drag was canceled");
    }

    private void OnDestroy()
    {
        // Отписываемся от ВСЕХ событий, на которые подписывались
        if (_inputHandler != null)
        {
            _inputHandler.OnSwipe -= HandleSwipe;
            _inputHandler.OnDragCanceled -= HandleDragCanceled;
                                                                // Если есть другие события - отписываемся и от них
        }
    }

}
