
using UnityEngine;
using UnityEngine.UI;

public class BombTimerBar : MonoBehaviour
{
    [Header("UI References")]
    public Canvas mainCanvas;
    public Image timerFill;

    [Header("Follow Settings")]
    public float heightOffset = 0.7f;

    [Header("Timer Size")]
    public float barWidth = 70f;
    public float barHeight = 14f;
    public float fillWidth = 62f;
    public float fillHeight = 8f;

    [Header("Colors")]
    public Color safeColor =
        new Color(0.2f, 0.8f, 0.4f);

    public Color warningColor =
        new Color(1f, 0.7f, 0.2f);

    public Color dangerColor =
        new Color(1f, 0.2f, 0.2f);

    private Transform bombTarget;
    private RectTransform barRect;
    private RectTransform fillRect;
    private RectTransform canvasRect;

    private CanvasGroup canvasGroup;
    private Camera gameCamera;

    private void Start()
    {
        barRect = GetComponent<RectTransform>();

        if (barRect == null ||
            mainCanvas == null ||
            timerFill == null)
        {
            Debug.LogError(
                "BombTimerBar: Main Canvas 또는 Timer Fill 연결 확인!"
            );

            enabled = false;
            return;
        }

        mainCanvas = mainCanvas.rootCanvas;

        if (mainCanvas.renderMode == RenderMode.WorldSpace)
        {
            Debug.LogError(
                "BombTimerBar: Screen Space Canvas를 연결하세요."
            );

            enabled = false;
            return;
        }

        if (GameManager.Instance == null ||
            GameManager.Instance.bomb == null)
        {
            Debug.LogError(
                "BombTimerBar: Bomb를 찾을 수 없습니다."
            );

            enabled = false;
            return;
        }

        bombTarget = GameManager.Instance.bomb.transform;
        gameCamera = Camera.main;

        if (gameCamera == null)
        {
            Debug.LogError(
                "BombTimerBar: Main Camera를 찾을 수 없습니다."
            );

            enabled = false;
            return;
        }

        // 지정한 메인 Canvas 아래로 이동
        barRect.SetParent(mainCanvas.transform, false);
        barRect.SetAsLastSibling();

        canvasRect =
            mainCanvas.GetComponent<RectTransform>();

        fillRect =
            timerFill.GetComponent<RectTransform>();

        // 게이지 기본 설정
        timerFill.type = Image.Type.Filled;
        timerFill.fillMethod =
            Image.FillMethod.Horizontal;
        timerFill.fillOrigin = 0;
        timerFill.raycastTarget = false;

        Image background = GetComponent<Image>();

        if (background != null)
            background.raycastTarget = false;

        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        ApplyFixedSize();

        Debug.Log("BombTimerBar: 고정 크기 타이머 시작!");
    }

    // 타이머의 크기와 Scale을 항상 동일하게 설정
    private void ApplyFixedSize()
    {
        if (barRect == null || fillRect == null)
            return;

        // 바깥 배경
        barRect.anchorMin =
            new Vector2(0.5f, 0.5f);
        barRect.anchorMax =
            new Vector2(0.5f, 0.5f);
        barRect.pivot =
            new Vector2(0.5f, 0.5f);

        barRect.localScale = Vector3.one;
        barRect.localRotation = Quaternion.identity;

        barRect.sizeDelta = new Vector2(
            Mathf.Max(1f, barWidth),
            Mathf.Max(1f, barHeight)
        );

        // 안쪽 게이지
        fillRect.anchorMin =
            new Vector2(0.5f, 0.5f);
        fillRect.anchorMax =
            new Vector2(0.5f, 0.5f);
        fillRect.pivot =
            new Vector2(0.5f, 0.5f);

        fillRect.anchoredPosition = Vector2.zero;
        fillRect.localScale = Vector3.one;
        fillRect.localRotation = Quaternion.identity;

        fillRect.sizeDelta = new Vector2(
            Mathf.Max(1f, fillWidth),
            Mathf.Max(1f, fillHeight)
        );
    }

    private void LateUpdate()
    {
        if (GameManager.Instance == null ||
            bombTarget == null ||
            gameCamera == null ||
            canvasRect == null)
        {
            return;
        }

        // 매 프레임 크기 유지
        ApplyFixedSize();

        // 게임 종료 시 숨기기
        if (GameManager.Instance.GameEnded ||
            !bombTarget.gameObject.activeInHierarchy)
        {
            canvasGroup.alpha = 0f;
            return;
        }

        // 폭탄 월드 좌표
        Vector3 worldPosition =
            bombTarget.position +
            Vector3.up * heightOffset;

        // 화면 좌표로 변환
        Vector3 screenPosition =
            gameCamera.WorldToScreenPoint(worldPosition);

        if (screenPosition.z <= 0f)
        {
            canvasGroup.alpha = 0f;
            return;
        }

        Camera uiCamera = null;

        if (mainCanvas.renderMode ==
            RenderMode.ScreenSpaceCamera)
        {
            uiCamera = mainCanvas.worldCamera;

            if (uiCamera == null)
                uiCamera = gameCamera;
        }

        Vector2 localPosition;

        bool success =
            RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPosition,
                uiCamera,
                out localPosition
            );

        if (!success)
        {
            canvasGroup.alpha = 0f;
            return;
        }

        // 폭탄 위치 따라가기
        barRect.anchoredPosition = localPosition;

        canvasGroup.alpha = 1f;

        // 폭발 타이머 비율
        float progress =
            GameManager.Instance.ExplosionProgress;

        timerFill.fillAmount = progress;

        // 색상 변경
        if (progress > 0.5f)
        {
            timerFill.color = safeColor;
        }
        else if (progress > 0.2f)
        {
            timerFill.color = warningColor;
        }
        else
        {
            timerFill.color = dangerColor;
        }
    }
}
