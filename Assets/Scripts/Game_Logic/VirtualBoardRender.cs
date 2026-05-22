using System.Collections.Generic;
using UnityEngine;

public class VirtualBoardRender : MonoBehaviour
{
    private VirtualBoard _virtualBoard;
    private GameObject _tilePrefab;

    public void Initialize(VirtualBoard virtualBoard, GameObject tilePrefab)
    {
        _virtualBoard = virtualBoard;
        _tilePrefab = tilePrefab;
        Render();
    }

    private void Render()
    {
        for (int x = 0; x < _virtualBoard._width; x++)
        {
            for (int y = 0; y < _virtualBoard._height; y++)
            {
                CellData cellData = _virtualBoard._gameBoard[x, y];

                if (cellData != null)
                {
                    Vector3 tempPosition = new Vector3(x, y, 0);

                    // рендер плиток
                    GameObject backgroundTile = Instantiate(_tilePrefab, tempPosition, Quaternion.identity);
                    backgroundTile.transform.parent = this.transform;
                    backgroundTile.name = $"Tile ({x}, {y})";

                    // рендер шаров
                    GameObject ballPrefab = _virtualBoard._ballsPrefabs[cellData.ball.type];
                    GameObject ballVisual = Instantiate(ballPrefab, tempPosition, Quaternion.identity);
                    ballVisual.transform.parent = this.transform;
                    ballVisual.name = $"Ball ({x}, {y}) - {cellData.ball.type}";

                    int ballsLayerIndex = LayerMask.NameToLayer("Balls");

                    if (ballsLayerIndex != -1)
                    {
                        ballVisual.layer = ballsLayerIndex;
                    }

                    cellData.ball.visualObject = ballVisual;

                    // Сохраняем в словарь для быстрого поиска
                    _virtualBoard.ballLookup[ballVisual] = cellData;

                }
            }
        }
    }

    public void RenderNewBall(CellData virtualCell)
    {
        GameObject ballPrefab = _virtualBoard._ballsPrefabs[virtualCell.ball.type];
        Vector3 tempPosition = new Vector3(virtualCell.x, _virtualBoard._height + 1, 0);
        GameObject ballVisual = Instantiate(ballPrefab, tempPosition, Quaternion.identity);
        ballVisual.transform.parent = this.transform;

        int ballsLayerIndex = LayerMask.NameToLayer("Balls");
        if (ballsLayerIndex != -1)
            ballVisual.layer = ballsLayerIndex;

        // сохранили в структуры
        _virtualBoard._gameBoard[virtualCell.x, virtualCell.y].ball.visualObject = ballVisual;
        _virtualBoard.ballLookup[ballVisual] = _virtualBoard._gameBoard[virtualCell.x, virtualCell.y];
    }
}
