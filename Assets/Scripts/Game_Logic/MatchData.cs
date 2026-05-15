using System.Collections.Generic;
using UnityEngine;

public class MatchData
{
    public List<CellData> matchedCells = new List<CellData>();
    public List<Vector2Int> positions = new List<Vector2Int>();
    public bool isHorizontal = false;
    public bool isVertical = false;
    public int matchLength = 0;

    // Для определения бонусов
    public Vector2Int startPos;
    public Vector2Int endPos;

    public void AddCell(CellData cell)
    {
        if (!matchedCells.Contains(cell))
        {
            matchedCells.Add(cell);
            positions.Add(new Vector2Int(cell.x, cell.y));
        }
    }

    public bool ContainsPosition(Vector2Int pos)
    {
        return positions.Contains(pos);
    }
}
