using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class VirtualBoard
{
    public readonly int _width;
    public readonly int _height;
    public readonly GameObject[] _ballsPrefabs;
    public CellData[,] _gameBoard;
    // словарь для быстрого поиска при нажатии на шар
    public Dictionary<GameObject, CellData> ballLookup;

    // todo: добавить layout фигурного поля
    public VirtualBoard(int width, int height, GameObject[] ballsPrefabs)
    {
        _width = width;
        _height = height;
        _ballsPrefabs = ballsPrefabs;
        ballLookup = new Dictionary<GameObject, CellData>();
        CreateBoard();
    }

    private void CreateBoard()
    {
        _gameBoard = new CellData[_width, _height];

        for (int i = 0; i < _width; i++)
        {
            for (int j = 0; j < _height; j++)
            {
                int ballTypeToUse;
                do
                {
                    // Пробуем случайный тип
                    ballTypeToUse = Random.Range(0, _ballsPrefabs.Length);
                }
                // Проверяем, не создаст ли это 3 в ряд
                while (HasMatchAt(i, j, ballTypeToUse));

                _gameBoard[i, j] = new CellData(i, j, CellType.Normal, new BallData2(ballTypeToUse));
            }
        }
    }

    private class BallGeneration
    {
        public BallData ball;
        public Vector3 targetPosition;
        public float fallDistance;
    }
    public List<CellData>[] GenerateExtraBalls()
    {
        Debug.Log("=== ЗАПОЛНЕНИЕ ПУСТЫХ КЛЕТОК ===");

        // Собираем все пустые клетки, которые нужно заполнить
        List<CellData>[] cellsToFill = new List<CellData>[_height];

        for (int x = 0; x < _width; x++)
        {
            int order = 0;
            

            for (int y = 0; y < _height; y++)
            {
                if (_gameBoard[x, y].ball == null)
                {
                    int newType;
                    do
                    {
                        newType = Random.Range(0, _ballsPrefabs.Length);
                    }
                    while (HasMatchAt(x, y, newType));

                    BallData2 newBall = new BallData2(newType);
                    CellData cell = new CellData(x, y, CellType.Normal, newBall);

                    if (cellsToFill[order] == null) {
                        cellsToFill[order] = new List<CellData>();
                    }
                    cellsToFill[order].Add(cell);
                    _gameBoard[cell.x, cell.y].ball = newBall;
                    order++;
                }
            }
        }

        Debug.Log($"Клетки созданы");
        return cellsToFill;

        // List<BallGeneration> newBalls = new List<BallGeneration>();

        //foreach (Vector2Int cell in emptyCells)
        //{
        //    int newType;
        //    do
        //    {
        //        newType = Random.Range(0, _ballsPrefabs.Length);
        //    }
        //    while (HasMatchAt(cell.x, cell.y, newType));

        //    BallData2 newBall = new BallData2(newType);
        //    _gameBoard[cell.x, cell.y].ball = newBall;

        // Создаем визуальный объект выше доски
        //Vector3 startPosition = new Vector3(cell.x, _height + 1, 0);
        //GameObject ballPrefab = _ballsPrefabs[newType];
        //GameObject ballVisual = Instantiate(ballPrefab, startPosition, Quaternion.identity);
        //ballVisual.transform.parent = this.transform;
        //ballVisual.name = $"New Ball ({cell.x}, {cell.y}) - {newType}";

        //// Устанавливаем слой
        //int ballsLayerIndex = LayerMask.NameToLayer("Balls");
        //if (ballsLayerIndex != -1)
        //{
        //    ballVisual.layer = ballsLayerIndex;
        //}

        //newBall.visualObject = ballVisual;
        //ballLookup[ballVisual] = newBall;

        //newBalls.Add(new BallGeneration
        //{
        //    ball = newBall,
        //    targetPosition = new Vector3(cell.x, cell.y, 0),
        //    fallDistance = height + 1 - cell.y
        //});
        //}
    }
    // проверка на валидность при генерации поля
    private bool HasMatchAt(int x, int y, int ballType)
    {
        // Проверка по горизонтали (влево)
        if (x >= 2)
        {
            BallData2 left1 = _gameBoard[x - 1, y].ball;
            BallData2 left2 = _gameBoard[x - 2, y].ball;

            if (left1 != null && left2 != null &&
                left1.type == ballType && left2.type == ballType)
            {
                return true;
            }
        }

        // Проверка по вертикали (вниз/вверх - зависит от порядка заполнения)
        if (y >= 2)
        {
            BallData2 down1 = _gameBoard[x, y - 1].ball;
            BallData2 down2 = _gameBoard[x, y - 2].ball;

            if (down1 != null && down2 != null &&
                down1.type == ballType && down2.type == ballType)
            {
                return true;
            }
        }

        return false;
    }

    public void DebugPrintBoard()
    {
        // Создаем строку с разделителем
        string separator = new string('-', _width * 4 + 1);

        Debug.Log($"=== Virtual Board {_width}x{_height} ===");
        Debug.Log(separator);

        for (int y = _height - 1; y >= 0; y--) // Обычно в играх Y=0 снизу, но здесь выводим сверху вниз
        {
            string row = "|";
            for (int x = 0; x < _width; x++)
            {
                if (_gameBoard[x, y] != null)
                {
                    // Выводим тип шара, форматируя под 2 символа
                    row += $"{_gameBoard[x, y].ball.type,2} |";
                }
                else
                {
                    row += " . |"; // . означает пустоту
                }
            }
            Debug.Log(row);
            Debug.Log(separator);
        }
    }
}
