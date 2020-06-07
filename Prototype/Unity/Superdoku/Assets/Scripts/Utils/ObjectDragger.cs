using UnityEngine;

public class ObjectDragger : MonoBehaviour
{
    private Vector3 mOffset;
    private float mZCoord;

    private void OnMouseDown()
    {
        mZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
        mOffset = gameObject.transform.position - GetMouseWorldPos();
    }

    private Vector3 GetMouseWorldPos()
    {
        // Pixel coordinates (x, y)
        Vector3 mousePoint = Input.mousePosition;

        // Z coordinate of game object on screen
        mousePoint.z = mZCoord;

        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    private void OnMouseDrag()
    {
        Vector3 nextPos = GetMouseWorldPos() + mOffset;
        transform.position = nextPos;

        // Normalize the move position to make it valid 



    }
}
