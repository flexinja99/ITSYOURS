
using UnityEngine;
using UnityEngine.EventSystems;

public class SwipeParryController : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler
{
    public BombController bombController;
    public float minimumSwipeDistance = 50f;

    private Vector2 startPosition;
    private bool isPointerDown;

    public void OnPointerDown(PointerEventData eventData)
    {
        startPosition = eventData.position;
        isPointerDown = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isPointerDown)
            return;

        isPointerDown = false;

        Vector2 endPosition = eventData.position;
        float distance = Vector2.Distance(
            startPosition, endPosition);

        if (distance < minimumSwipeDistance)
            return;

        float horizontal = endPosition.x - startPosition.x;
        float vertical = endPosition.y - startPosition.y;

        // 좌우 이동이 위아래 이동보다 클 때만 받아치기
        if (Mathf.Abs(horizontal) <= Mathf.Abs(vertical))
            return;

        if (bombController != null)
            bombController.ParryFromPlayer();
    }
}
