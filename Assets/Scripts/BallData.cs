using UnityEngine;

public class BallData
{
    public int x;
    public int y;
    public int type;
    public bool isMatched = false;
    public GameObject visualObject; // ссылка на визуальный объект
    public BonusType bonus = BonusType.None;

    public BallData(int x, int y, int type)
    {
        this.x = x;
        this.y = y;
        this.type = type;
    }
}
