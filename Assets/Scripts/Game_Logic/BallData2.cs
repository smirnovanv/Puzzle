using UnityEngine;

public class BallData2
{
    public int type; // выбор из списка префабов, "цвет" шарика
    public bool isMatched = false; // помечаем для дальнейшего удаления с поля
    public GameObject visualObject; // ссылка на визуальный объект
    public BonusType bonus = BonusType.None; // todo

    public BallData2(int type)
    {
        this.type = type;
    }
}
