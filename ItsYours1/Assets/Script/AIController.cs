
using UnityEngine;

public class AIController : MonoBehaviour
{
    public float minWaitTime = 0.5f;
    public float maxWaitTime = 1.5f;

    private BombController bombController;
    private float nextActionTime;
    private bool wasOwner;

    private void Start()
    {
        bombController =
            FindFirstObjectByType<BombController>();
    }

    private void Update()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.GameEnded ||
            bombController == null ||
            !GameManager.Instance.IsAlive(gameObject))
            return;

        bool isOwner =
            bombController.CurrentOwner == gameObject;

        if (!isOwner)
        {
            wasOwner = false;
            return;
        }

        if (!wasOwner)
        {
            wasOwner = true;
            nextActionTime = Time.time +
                Random.Range(minWaitTime, maxWaitTime);
        }

        if (bombController.IsFlying ||
            Time.time < nextActionTime)
            return;

        GameObject target =
            GameManager.Instance.GetRandomAliveCharacterExcept(
                gameObject);

        if (target != null)
            bombController.ThrowTo(target, Random.Range(0.1f, 0.8f));
    }
}
