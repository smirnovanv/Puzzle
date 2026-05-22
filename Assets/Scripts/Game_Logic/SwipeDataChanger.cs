using UnityEngine;

public class SwipeDataChanger
{
    private VirtualBoard _board;
    public SwipeDataChanger(VirtualBoard board)
    {
        _board = board;
    }
    public void SwapBalls(CellData selectedCell, CellData targetCell)
    {
        BallData2 tempSelectedCellBall = selectedCell.ball;

        selectedCell.ball = targetCell.ball;
        targetCell.ball = tempSelectedCellBall;

        _board.ballLookup[selectedCell.ball.visualObject] = selectedCell;
        _board.ballLookup[targetCell.ball.visualObject] = targetCell;
    }

}
