using System.Collections.Generic;
using UnityEngine;

public class MatchInfo
{
    public List<BallData> matchedBalls = new List<BallData>();
    public List<Vector2Int> positions = new List<Vector2Int>();
    public bool isHorizontal = false;
    public bool isVertical = false;
    public int matchLength = 0;

    // Для определения бонусов
    public Vector2Int startPos;
    public Vector2Int endPos;

    public void AddBall(BallData ball)
    {
        if (!matchedBalls.Contains(ball))
        {
            matchedBalls.Add(ball);
            positions.Add(new Vector2Int(ball.x, ball.y));
        }
    }

    public bool ContainsPosition(Vector2Int pos)
    {
        return positions.Contains(pos);
    }
}
