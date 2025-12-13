using UnityEditorInternal;
using UnityEngine;

public class BallData
{
    public int x;
    public int y;
    public int type;
    public bool isMatched = false;
    public GameObject visualObject; // ссылка на визуальный объект
    public BonusType bonus = BonusType.None;
    public enum BonusType
    {
        None,
        HorizontalRocket,   // Очищает всю строку
        VerticalRocket,     // Очищает весь столбец
        Bomb,              // Взрывает соседние шарики (3x3 область)
        ColorBomb,         // Уничтожает все шарики одного цвета
        Rainbow            // Специальный бонус (по желанию)
    }
    public BallData(int x, int y, int type)
    {
        this.x = x;
        this.y = y;
        this.type = type;
    }
}
