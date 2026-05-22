using System;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    private VirtualBoard _virtualBoard;
    private LayerMask ballLayer;

    private bool _isInputEnabled = true; // todo
    public CellData selectedCell = null;

    public Vector2 dragStartPosition;

    private float minDragDistance = 0.5f;

    public event Action<CellData, CellData> OnSwipe; // (клетка, клетка) 
    public event Action OnDragCanceled;

    public void Initialize(VirtualBoard board, LayerMask layer)
    {
        _virtualBoard = board;
        ballLayer = layer;
    }

    void Update()
    {
        //if (!_isInputEnabled) return; // todo

        if (Input.GetMouseButtonDown(0))
        {
            StartDrag();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndDrag();
        }
    }

    public void EnableInput() => _isInputEnabled = true;
    public void DisableInput() => _isInputEnabled = false;

    private void StartDrag() {
        Debug.Log($"=== start ===");

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, ballLayer))
        {
            GameObject clickedBall = hit.collider.gameObject;

            if (_virtualBoard.ballLookup.TryGetValue(clickedBall, out CellData cellData))
            {
                selectedCell = cellData;
                dragStartPosition = Input.mousePosition;
                Debug.Log($"Start drag at ({cellData.x}, {cellData.y})");

                return;
            }
        }
    }

    private void EndDrag() {

        Debug.Log($"=== end ===");

        if (selectedCell == null) return;

        Vector2 dragEndPosition = Input.mousePosition;
        Vector2 dragVector = dragEndPosition - dragStartPosition;

        // ѕровер€ем, достаточно ли большое движение
        if (dragVector.magnitude >= minDragDistance)
        {
            // ќпредел€ем направление
            Direction direction = GetDragDirection(dragVector);

            Debug.Log($"Swipe detected: {direction}");

            // ѕолучаем координаты целевой клетки
            Vector2Int targetPos = GetTargetPosition(selectedCell.x, selectedCell.y, direction);

            Debug.Log($"End drag at ({targetPos.x}, {targetPos.y})");

            //ѕровер€ем, есть ли шарик в целевой клетке
            if (IsValidPosition(targetPos.x, targetPos.y))
            {
                CellData targetCell = _virtualBoard._gameBoard[targetPos.x, targetPos.y];

                if (targetCell.ball != null)
                {
                    OnSwipe?.Invoke(selectedCell, targetCell);
                }
            }
        }
        else
        {
            OnDragCanceled?.Invoke(); // todo пересмотреть дл€ случаев нажати€, напр. буста
            Debug.Log("Swipe too short, canceled");
        }

        selectedCell = null;
    }

    private Direction GetDragDirection(Vector2 dragVector)
    {
        // ќпредел€ем основное направление по наибольшей компоненте
        if (Mathf.Abs(dragVector.x) > Mathf.Abs(dragVector.y))
        {
            return dragVector.x > 0 ? Direction.Right : Direction.Left;
        }
        else
        {
            return dragVector.y > 0 ? Direction.Up : Direction.Down;
        }
    }

    private Vector2Int GetTargetPosition(int x, int y, Direction direction)
    {
        return direction switch
        {
            Direction.Up => new Vector2Int(x, y + 1),
            Direction.Down => new Vector2Int(x, y - 1),
            Direction.Left => new Vector2Int(x - 1, y),
            Direction.Right => new Vector2Int(x + 1, y),
            _ => new Vector2Int(x, y)
        };
    }

    private bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < _virtualBoard._width && y >= 0 && y < _virtualBoard._height;
    }

}
