public class CellData
{
    public int x;
    public int y;
    public CellType type;
    public BallData2 ball;

    public CellData(int x, int y, CellType type, BallData2 ballType)
    {
        this.x = x;
        this.y = y;
        this.type = type;
        ball = ballType;
    }
}
