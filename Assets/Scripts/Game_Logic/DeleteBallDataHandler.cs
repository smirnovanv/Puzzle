using System.Collections.Generic;
using UnityEngine;

public class DeleteBallDataHandler: MonoBehaviour
{
    VirtualBoard _board;
    public void Initialize (VirtualBoard board)
    {
        _board = board;

    }

    public void RemoveMatchedBalls()
    {
        // Сначала показываем анимацию совпадения
        //yield return new WaitForSeconds(0.5f);

        foreach (CellData cell in _board._gameBoard)
        {
            if (cell.ball != null && cell.ball.isMatched &&  cell.ball.visualObject != null)
            {
                // очищаем эти объекты в словаре
                if (_board.ballLookup.ContainsKey(cell.ball.visualObject))
                {
                    _board.ballLookup.Remove(cell.ball.visualObject);
                }

                // удаляем сами предметы
                Destroy(cell.ball.visualObject);
                cell.ball = null;
                Debug.Log($"Удален шарик на позиции ({cell.x}, {cell.y})");
            }

            // Очищаем клетку в модели сетки
           // gameBoard[ball.x, ball.y] = null;
            
        }

        //// Сдвигаем шарики вниз
        //yield return StartCoroutine(ShiftBallsDown());

        //// Заполняем пустые клетки
        //yield return StartCoroutine(FillEmptySpaces());

        //// Проверяем новые совпадения (каскадные совпадения)
        //bool newMatches = CheckForCascadeMatches();

        //// Если есть новые совпадения, повторяем процесс
        //while (newMatches)
        //{
        //    yield return new WaitForSeconds(0.5f); // Пауза между каскадами
        //    newMatches = CheckForCascadeMatches();
        //}
    }
}
