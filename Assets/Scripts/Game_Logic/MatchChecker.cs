using System;
using System.Collections.Generic;
using UnityEngine;

public class MatchChecker
{
    private VirtualBoard _board;
    // структуры для поиска совпадений
    private List<MatchData> horizontalMatches = new List<MatchData>();
    private List<MatchData> verticalMatches = new List<MatchData>();
    private HashSet<CellData> allMatchedCells = new HashSet<CellData>();
    public MatchChecker(VirtualBoard board)
    {
        _board = board;
    }
    //public Boolean Check(VirtualBoard board) {
    //    return false;
    //}

    public List<MatchData> FindAllMatches()
    {
        ClearMatchData();

        FindHorizontalMatches();
        FindVerticalMatches();

        // Объединяем все совпадения
        List<MatchData> allMatches = new List<MatchData>();
        allMatches.AddRange(horizontalMatches);
        allMatches.AddRange(verticalMatches);

        LogMatchesInfo(allMatches);

        // Отмечаем шарики как совпавшие
        MarkMatchedBalls(allMatches);

        // Определяем бонусы на основе совпадений
        // todo DetermineBonuses(allMatches);

        return allMatches;
    }

    private void ClearMatchData()
    {
        horizontalMatches.Clear();
        verticalMatches.Clear();
        allMatchedCells.Clear();

        // Сбрасываем флаги совпадений у всех шариков
        //for (int x = 0; x < _board._width; x++)  // todo пересмотреть, шарики должны быть удалены
        //{
        //    for (int y = 0; y < _board._height; y++)
        //    {
        //        if (_board._gameBoard[x, y] != null)
        //        {
        //            _board._gameBoard[x, y].ball.isMatched = false;
        //        }
        //    }
        //}
    }

    private void LogMatchesInfo(List<MatchData> allMatches)
    {
        Debug.Log($"=== Всего совпадений: {allMatches.Count} ===");

        for (int i = 0; i < allMatches.Count; i++)
        {
            MatchData match = allMatches[i];

            Debug.Log($"Совпадение #{i + 1}:");
            Debug.Log($"  Тип: {(match.isHorizontal ? "Горизонтальное" : "Вертикальное")}");
            Debug.Log($"  Длина: {match.matchLength} шариков");
            Debug.Log($"  Начало: ({match.startPos.x}, {match.startPos.y})");
            Debug.Log($"  Конец: ({match.endPos.x}, {match.endPos.y})");
            Debug.Log($"  Тип шарика: {match.matchedCells[0].ball.type}");
            Debug.Log($"  Количество шариков: {match.matchedCells.Count}");

            // Выводим координаты всех шариков в совпадении
            string positions = "  Позиции: ";
            foreach (CellData cell in match.matchedCells)
            {
                positions += $"({cell.x},{cell.y}) ";
            }
            Debug.Log(positions);

            Debug.Log("---");
        }
    }

    private void FindHorizontalMatches()
    {
        for (int y = 0; y < _board._height; y++)
        {
            int currentType = -1; // todo check -1 reason
            int matchStart = 0;
            int matchLength = 0;

            for (int x = 0; x < _board._width; x++)
            {
                CellData cell = _board._gameBoard[x, y];

                if (cell.ball != null && cell.ball.type == currentType)
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
                    currentType = cell.ball?.type ?? -1;
                    matchStart = x;
                    matchLength = cell.ball != null ? 1 : 0;
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
        MatchData match = new MatchData
        {
            isHorizontal = true,
            matchLength = length,
            startPos = new Vector2Int(startX, y),
            endPos = new Vector2Int(startX + length - 1, y)
        };

        for (int x = startX; x < startX + length; x++)
        {
            CellData cell = _board._gameBoard[x, y];
            if (cell.ball != null && cell.ball.type == type)
            {
                match.AddCell(cell);
            }
        }

        horizontalMatches.Add(match);
    }

    private void FindVerticalMatches()
    {
        for (int x = 0; x < _board._width; x++)
        {
            int currentType = -1;
            int matchStart = 0;
            int matchLength = 0;

            for (int y = 0; y < _board._height; y++)
            {
                CellData cell = _board._gameBoard[x, y];

                if (cell.ball != null && cell.ball.type == currentType)
                {
                    matchLength++;
                }
                else
                {
                    if (matchLength >= 3)
                    {
                        SaveVerticalMatch(x, matchStart, matchLength, currentType);
                    }

                    currentType = cell.ball?.type ?? -1;
                    matchStart = y;
                    matchLength = cell.ball != null ? 1 : 0;
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
        MatchData match = new MatchData
        {
            isVertical = true,
            matchLength = length,
            startPos = new Vector2Int(x, startY),
            endPos = new Vector2Int(x, startY + length - 1)
        };

        for (int y = startY; y < startY + length; y++)
        {
            CellData cell = _board._gameBoard[x, y];
            if (cell.ball != null && cell.ball.type == type)
            {
                match.AddCell(cell);
            }
        }

        verticalMatches.Add(match);
    }

    private void MarkMatchedBalls(List<MatchData> allMatches)
    {
        foreach (MatchData match in allMatches)
        {
            foreach (CellData cell in match.matchedCells)
            {
                cell.ball.isMatched = true;
                allMatchedCells.Add(cell);

                //Визуально помечаем шарик(подсветка на короткое время)
                //if (ball.visualObject != null)
                //{
                //    StartCoroutine(HighlightMatchedBall(ball.visualObject));
                //}
            }
        }
    }

}
