using System;
using UnityEngine;

public class Dot : MonoBehaviour
{
    public int column;
    public int row;
    public int targetX;
    public int targetY;
    public Boolean isMatched = false;
    private Board board;
    private GameObject otherDot;
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private Vector2 tempPosition;
    public float swipeAngle = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        board = FindAnyObjectByType<Board>();
        targetX = (int)transform.position.x;
        targetY = (int)transform.position.y;
        row = targetY;
        column = targetX;
    }

    // Update is called once per frame
    void Update()
    {
        findMatches();

        if (isMatched) {
            SpriteRenderer mySprite = GetComponent<SpriteRenderer>();
            mySprite.color = new Color(1f, 1f, 1f, .2f);
        }

        targetX = column;
        targetY = row;

        if (Mathf.Abs(targetX - transform.position.x) > .1)
        {
            // move towards the target
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .1f);
        } else
        {
            // directly set
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = tempPosition;
            board.allDots[column, row] = this.gameObject;
        }

        if (Mathf.Abs(targetY - transform.position.y) > .1)
        {
            // move towards the target
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .1f);
        }
        else
        {
            // directly set
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = tempPosition;
            board.allDots[column, row] = this.gameObject;
        }
    }

    private void OnMouseDown()
    {
        firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Debug.Log(firstTouchPosition);
    }

    private void OnMouseUp()
    {
        finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Debug.Log(finalTouchPosition);
        CalculateAngle();
        
    }

    void CalculateAngle()
    {
        swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
        Debug.Log(swipeAngle);
        MovePieces();
    }

    void MovePieces()
    {
        if(swipeAngle > -45 && swipeAngle <= 45 && column < board.width)
        {
            Debug.Log("right");
            otherDot = board.allDots[column + 1, row];
            otherDot.GetComponent<Dot>().column -= 1;
            column += 1;
        } else if(swipeAngle > 45 && swipeAngle <= 135 && row < board.height) {
            Debug.Log("up");
            otherDot = board.allDots[column, row + 1];
            otherDot.GetComponent<Dot>().row -= 1;
            row += 1;
        } else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0)
        {
            Debug.Log("left");
            otherDot = board.allDots[column - 1, row];
            otherDot.GetComponent<Dot>().column += 1;
            column -= 1;
        } else if(swipeAngle <= -45 && swipeAngle > -135 && row > 0)
        {
            Debug.Log("down");
            otherDot = board.allDots[column, row - 1];
            otherDot.GetComponent<Dot>().row += 1;
            row -= 1;
        }
    }

    void findMatches()
    {
        if (column > 0 && column < board.width - 1) {
            GameObject leftDot1 = board.allDots[column - 1, row];
            GameObject rightDot1 = board.allDots[column + 1, row];
            if (leftDot1.tag == this.gameObject.tag && rightDot1.tag == this.gameObject.tag) {
                leftDot1.GetComponent<Dot>().isMatched = true;
                rightDot1.GetComponent<Dot>().isMatched = true;
                isMatched = true;

            }
        }

        if (row > 0 && row < board.height - 1)
        {
            GameObject upDot1 = board.allDots[column, row + 1];
            GameObject downDot1 = board.allDots[column, row - 1];
            if (upDot1.tag == this.gameObject.tag && downDot1.tag == this.gameObject.tag)
            {
                upDot1.GetComponent<Dot>().isMatched = true;
                downDot1.GetComponent<Dot>().isMatched = true;
                isMatched = true;

            }
        }
    }
}
