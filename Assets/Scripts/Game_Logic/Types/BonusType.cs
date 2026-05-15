public enum BonusType
{
    None,
    HorizontalRocket,   // Очищает всю строку
    VerticalRocket,     // Очищает весь столбец
    Bomb,              // Взрывает соседние шарики (3x3 область)
    ColorBomb,         // Уничтожает все шарики одного цвета
}
