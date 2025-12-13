using UnityEngine;

public class BaseMove : MonoBehaviour
{
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private DashBoard board;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        
    }

    // Update is called once per frame
    void Update()
    {
        
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
        }
    }
}
