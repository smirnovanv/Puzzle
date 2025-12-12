using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Sphere : MonoBehaviour
{
    public int column;
    public int row;
    public int targetX;
    public int targetY;
    private DashBoard board;
    private GameObject otherDot;
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private Vector2 tempPosition;
    public float swipeAngle = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        board = FindAnyObjectByType<DashBoard>();
        targetX = Mathf.RoundToInt(transform.position.x);
        targetY = Mathf.RoundToInt(transform.position.y);
        row = targetY;
        column = targetX;
    }

    // Update is called once per frame
    void Update()
    {
        targetX = column;
        targetY = row;

        if (Mathf.Abs(targetX - transform.position.x) > .1)
        {
            // move towards the target
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .1f);
        }
        else
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
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject == this.gameObject)
            {
                firstTouchPosition = hit.point;
                Debug.Log("Touch on sphere at: " + firstTouchPosition);
            }
        }
    }

    private void OnMouseUp()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            finalTouchPosition = hit.point;
            Debug.Log("Release on sphere at: " + finalTouchPosition);
            CalculateAngle();
        }
    }

    void CalculateAngle()
    {
        swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
        Debug.Log(swipeAngle);
        MovePieces();
    }

    void MovePieces()
    {
        if (swipeAngle > -45 && swipeAngle <= 45 && column < board.width)
        {
            Debug.Log("right");
            otherDot = board.allDots[column + 1, row];
            otherDot.GetComponent<Sphere>().column -= 1;
            column += 1;
        }
        else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.height)
        {
            Debug.Log("up");
            otherDot = board.allDots[column, row + 1];
            otherDot.GetComponent<Sphere>().row -= 1;
            row += 1;
        }
        else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0)
        {
            Debug.Log("left");
            otherDot = board.allDots[column - 1, row];
            otherDot.GetComponent<Sphere>().column += 1;
            column -= 1;
        }
        else if (swipeAngle <= -45 && swipeAngle > -135 && row > 0)
        {
            Debug.Log("down");
            otherDot = board.allDots[column, row - 1];
            otherDot.GetComponent<Sphere>().row += 1;
            row -= 1;
        }
    }
}
