
using UnityEngine;
using System.Collections;

public class BombController : MonoBehaviour
{
    [Header("Bomb Settings")]
    public float throwSpeed = 7f;
    public float maxChargeTime = 1f;
    public float heightOffset = 0.7f;

    private GameObject currentOwner;
    private GameObject target;

    private bool isFlying;
    private bool isCharging;
    private float chargeStartTime;
    private float chargeAmount;

    private Coroutine flightRoutine;

    public GameObject CurrentOwner => currentOwner;
    public GameObject Target => target;
    public bool IsFlying => isFlying;

    public void Initialize(GameObject startingOwner)
    {
        currentOwner = startingOwner;
        target = null;

        isFlying = false;
        isCharging = false;
        chargeAmount = 0f;

        if (flightRoutine != null)
        {
            StopCoroutine(flightRoutine);
            flightRoutine = null;
        }

        if (currentOwner != null)
        {
            transform.position =
                currentOwner.transform.position
                + Vector3.up * heightOffset;
        }
    }

    public void SetTarget(GameObject newTarget)
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.GameEnded ||
            isFlying || isCharging ||
            !GameManager.Instance.IsAlive(newTarget) ||
            newTarget == currentOwner)
        {
            return;
        }

        target = newTarget;

        Debug.Log("던질 상대 선택: " + target.name);
    }

    public void BeginCharge()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.GameEnded ||
            currentOwner == null ||
            currentOwner != GameManager.Instance.player ||
            isFlying || isCharging)
        {
            return;
        }

        isCharging = true;
        chargeStartTime = Time.time;
        chargeAmount = 0f;
    }

    public float GetChargeAmount()
    {
        if (!isCharging)
            return 0f;

        return Mathf.Clamp01(
            (Time.time - chargeStartTime) / maxChargeTime
        );
    }

    public void ReleaseCharge()
    {
        if (!isCharging)
            return;

        chargeAmount = GetChargeAmount();
        isCharging = false;

        if (target == null)
        {
            Debug.Log("먼저 던질 상대를 선택하세요!");
            return;
        }

        ThrowTo(target, chargeAmount);
    }

    public void ThrowTo(GameObject newTarget, float charge01)
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.GameEnded ||
            isFlying ||
            currentOwner == null ||
            !GameManager.Instance.IsAlive(currentOwner) ||
            !GameManager.Instance.IsAlive(newTarget) ||
            newTarget == currentOwner)
        {
            return;
        }

        target = newTarget;
        chargeAmount = Mathf.Clamp01(charge01);
        isCharging = false;
        isFlying = true;

        flightRoutine = StartCoroutine(FlyToTarget());
    }

    private IEnumerator FlyToTarget()
    {
        while (isFlying && target != null)
        {
            if (GameManager.Instance == null ||
                GameManager.Instance.GameEnded)
            {
                isFlying = false;
                flightRoutine = null;
                yield break;
            }

            Vector3 destination =
                target.transform.position
                + Vector3.up * heightOffset;

            float speed = throwSpeed *
                (1f + chargeAmount * 0.5f);

            transform.position = Vector3.MoveTowards(
                transform.position,
                destination,
                speed * Time.deltaTime
            );

            if (Vector3.Distance(
                transform.position, destination) < 0.05f)
            {
                transform.position = destination;

                currentOwner = target;
                target = null;

                isFlying = false;
                flightRoutine = null;

                Debug.Log("폭탄 소유자: " + currentOwner.name);
                yield break;
            }

            yield return null;
        }

        isFlying = false;
        flightRoutine = null;
    }

    public bool ParryFromPlayer()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.GameEnded ||
            !isFlying ||
            target != GameManager.Instance.player)
        {
            return false;
        }

        GameObject newTarget =
            GameManager.Instance.GetRandomAliveCharacterExcept(
                currentOwner,
                GameManager.Instance.player
            );

        if (newTarget == null)
            return false;

        if (flightRoutine != null)
        {
            StopCoroutine(flightRoutine);
            flightRoutine = null;
        }

        target = newTarget;
        isFlying = false;

        Debug.Log("받아치기 성공! → " + newTarget.name);

        ThrowTo(newTarget, 0.25f);

        return true;
    }

    public void Explode()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.GameEnded)
        {
            return;
        }

        isFlying = false;
        isCharging = false;
        target = null;

        if (flightRoutine != null)
        {
            StopCoroutine(flightRoutine);
            flightRoutine = null;
        }

        // 폭발 시점의 폭탄 소유자 한 명만 탈락
        GameObject victim = currentOwner;

        Debug.Log("폭탄 폭발!");

        if (victim != null &&
            GameManager.Instance.IsAlive(victim))
        {
            Debug.Log("폭탄 소유자 탈락: " + victim.name);

            GameManager.Instance.EliminateCharacter(victim);
        }
        else
        {
            Debug.LogWarning("폭발 시 유효한 폭탄 소유자가 없습니다.");
        }

        // 마지막 생존자 판정이 끝났으면 폭탄 비활성화
        if (GameManager.Instance.GameEnded)
        {
            gameObject.SetActive(false);
            return;
        }

        // 탈락하지 않은 생존자 중 다음 소유자 선택
        GameObject nextOwner =
            GameManager.Instance.GetRandomAliveCharacterExcept();

        if (nextOwner != null)
        {
            currentOwner = nextOwner;
            target = null;

            transform.position =
                currentOwner.transform.position
                + Vector3.up * heightOffset;

            Debug.Log("새로운 폭탄 소유자: " + currentOwner.name);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
