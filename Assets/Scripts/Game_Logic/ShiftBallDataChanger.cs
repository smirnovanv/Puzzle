using System.Collections.Generic;
using UnityEngine;

public class ShiftBallDataChanger
{
    private VirtualBoard _board;
    public ShiftBallDataChanger (VirtualBoard board)
    {
        _board = board;

    }
    public List<BallShiftData> Change () 
    {
        Debug.Log("=== НАЧАЛО СДВИГА ШАРИКОВ ВНИЗ ===");

        // 1. Собираем ВСЕ шарики, которые нужно сдвинуть, и сразу вычисляем их конечные позиции
        List<BallShiftData> ballMovements = new List<BallShiftData>();

        // Проходим по всем столбцам
        for (int x = 0; x < _board._width; x++)
        {
            int emptyCellsBelow = 0;

            // Проходим снизу вверх
            for (int y = 0; y < _board._height; y++)
            {
                if (_board._gameBoard[x, y].ball == null) // todo добавить проверку на тип клетки
                {
                    // Нашли пустую клетку - увеличиваем счетчик пустых клеток ниже
                    emptyCellsBelow++;
                }
                else if (emptyCellsBelow > 0)
                {
                    // Нашли клетку с шариком над пустыми клетками - нужно сдвинуть
                    CellData cell = _board._gameBoard[x, y];
                    int targetY = y - emptyCellsBelow; // Позиция после сдвига

                    // Сохраняем информацию о движении
                    ballMovements.Add(new BallShiftData
                    {
                        originalCell = cell,
                        startX = x,
                        startY = y,
                        targetX = x,
                        targetY = targetY,
                        distance = emptyCellsBelow
                    });
                }
            }
        }

        if (ballMovements.Count == 0)
        {
            Debug.Log("Нет шариков для сдвига");
           
        }

        Debug.Log($"Найдено {ballMovements.Count} шариков для сдвига");

        foreach (BallShiftData movement in ballMovements)
        {
            // Устанавливаем новую позицию
            _board._gameBoard[movement.targetX, movement.targetY].ball = movement.originalCell.ball;

            // Очищаем старую позицию
            _board._gameBoard[movement.startX, movement.startY].ball = null;
            // Обновляем словарь
            _board.ballLookup[_board._gameBoard[movement.targetX, movement.targetY].ball.visualObject] = _board._gameBoard[movement.targetX, movement.targetY];
        }

        return ballMovements;
    }
    
}
