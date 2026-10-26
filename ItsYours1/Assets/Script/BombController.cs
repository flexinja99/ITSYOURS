
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

    // 게임 시작 시 폭탄 초기화
    public void Initialize(GameObject startingOwner)
    {
        CharacterSelect.ClearSelection();

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

        UpdateBombPosition();
    }

    // NEW: 폭탄을 가지고 있는 캐릭터를 계속 따라감
    private void LateUpdate()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.GameEnded)
        {
            return;
        }

        // 폭탄이 날아가는 동안에는 위치 고정 안 함
        if (isFlying)
            return;

        // 폭탄 소유자가 없으면 실행 안 함
        if (currentOwner == null)
            return;

        // 탈락한 캐릭터는 따라가지 않음
        if (!GameManager.Instance.IsAlive(currentOwner))
            return;

        UpdateBombPosition();
    }

    // NEW: 폭탄 위치를 소유자 위로 맞추는 함수
    private void UpdateBombPosition()
    {
        if (currentOwner == null)
            return;

        transform.position =
            currentOwner.transform.position
            + Vector3.up * heightOffset;
    }

    // 던질 대상 선택
    public void SetTarget(GameObject newTarget)
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.GameEnded ||
            isFlying ||
            isCharging ||
            !GameManager.Instance.IsAlive(newTarget) ||
            newTarget == currentOwner)
        {
            return;
        }

        target = newTarget;

        Debug.Log("던질 상대 선택: " + target.name);
    }

    // 폭탄 충전 시작
    public void BeginCharge()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.GameEnded ||
            currentOwner == null ||
            currentOwner != GameManager.Instance.player ||
            isFlying ||
            isCharging)
        {
            return;
        }

        isCharging = true;
        chargeStartTime = Time.time;
        chargeAmount = 0f;
    }

    // 현재 충전량 반환
    public float GetChargeAmount()
    {
        if (!isCharging)
            return 0f;

        return Mathf.Clamp01(
            (Time.time - chargeStartTime) /
            Mathf.Max(0.01f, maxChargeTime)
        );
    }

    // 충전 종료 후 폭탄 던지기
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

    // 실제 폭탄 투척
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

        // 던지는 순간 늑대 선택 색상 초기화
        CharacterSelect.ClearSelection();

        Debug.Log("폭탄 투척! → " + target.name);

        flightRoutine = StartCoroutine(FlyToTarget());
    }

    // 폭탄 비행 처리
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

            float speed =
                throwSpeed * (1f + chargeAmount * 0.5f);

            transform.position = Vector3.MoveTowards(
                transform.position,
                destination,
                speed * Time.deltaTime
            );

            if (Vector3.Distance(
                transform.position,
                destination) < 0.05f)
            {
                transform.position = destination;

                currentOwner = target;
                target = null;

                isFlying = false;
                flightRoutine = null;

                // 도착 후 소유자 위치에 정확하게 고정
                UpdateBombPosition();

                Debug.Log(
                    "폭탄 소유자: " + currentOwner.name
                );

                yield break;
            }

            yield return null;
        }

        isFlying = false;
        flightRoutine = null;
    }

    // 플레이어 패링
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

        Debug.Log(
            "받아치기 성공! → " + newTarget.name
        );

        ThrowTo(newTarget, 0.25f);

        return true;
    }

    // 폭탄 폭발
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

        CharacterSelect.ClearSelection();

        if (flightRoutine != null)
        {
            StopCoroutine(flightRoutine);
            flightRoutine = null;
        }

        // 현재 폭탄 소유자만 탈락
        GameObject victim = currentOwner;

        Debug.Log("폭탄 폭발!");

        if (victim != null &&
            GameManager.Instance.IsAlive(victim))
        {
            Debug.Log(
                "폭탄 소유자 탈락: " + victim.name
            );

            GameManager.Instance.EliminateCharacter(victim);
        }
        else
        {
            Debug.LogWarning(
                "폭발 시 유효한 폭탄 소유자가 없습니다."
            );
        }

        // 승리 또는 패배 결정 시 폭탄 비활성화
        if (GameManager.Instance.GameEnded)
        {
            gameObject.SetActive(false);
            return;
        }

        // 다음 폭탄 소유자 선택
        GameObject nextOwner =
            GameManager.Instance.GetRandomAliveCharacterExcept();

        if (nextOwner != null)
        {
            currentOwner = nextOwner;
            target = null;

            UpdateBombPosition();

            Debug.Log(
                "새로운 폭탄 소유자: " + currentOwner.name
            );
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
