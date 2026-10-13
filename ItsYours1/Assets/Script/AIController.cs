
using UnityEngine;

public class AIController : MonoBehaviour
{
    public float minWaitTime = 0.5f;
    public float maxWaitTime = 1.5f;

    private BombController bombController;
    private float nextActionTime;

    private void Start()
    {
        bombController =
            FindFirstObjectByType<BombController>();

        ScheduleNextAction();
    }

    private void Update()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.GameEnded ||
            bombController == null ||
            !GameManager.Instance.IsAlive(gameObject))
        {
            return;
        }

        if (bombController.CurrentOwner != gameObject ||
            bombController.IsFlying ||
            Time.time < nextActionTime)
        {
            return;
        }

        GameObject target =
            GameManager.Instance.GetRandomAliveCharacterExcept(
                gameObject);

        if (target == null)
            return;

        float charge = Random.Range(0.1f, 0.8f);

        bombController.ThrowTo(target, charge);
        ScheduleNextAction();
    }

    private void ScheduleNextAction()
    {
        nextActionTime = Time.time +
            Random.Range(minWaitTime, maxWaitTime);
    }
}
