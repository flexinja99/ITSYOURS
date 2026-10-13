
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ThrowButtonController : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler
{
    public BombController bombController;
    public Slider chargeSlider;

    private bool isCharging;

    private void Start()
    {
        if (chargeSlider != null)
            chargeSlider.value = 0f;
    }

    private void Update()
    {
        if (!isCharging || bombController == null)
            return;

        if (chargeSlider != null)
            chargeSlider.value = bombController.GetChargeAmount();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (bombController == null ||
            GameManager.Instance == null ||
            GameManager.Instance.GameEnded ||
            bombController.CurrentOwner != GameManager.Instance.player ||
            bombController.IsFlying)
        {
            return;
        }

        bombController.BeginCharge();
        isCharging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isCharging)
            return;

        isCharging = false;

        if (bombController != null)
            bombController.ReleaseCharge();

        if (chargeSlider != null)
            chargeSlider.value = 0f;
    }

    private void OnDisable()
    {
        isCharging = false;

        if (chargeSlider != null)
            chargeSlider.value = 0f;
    }
}
