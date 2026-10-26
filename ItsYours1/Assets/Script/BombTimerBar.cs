
using UnityEngine;
using UnityEngine.UI;

public class BombTimerBar : MonoBehaviour
{
    [Header("Timer UI")]
    public Image timerFill;

    [Header("Follow Settings")]
    public float heightOffset = 0.7f;

    [Header("Colors")]
    public Color safeColor = new Color(0.2f, 0.8f, 0.4f);
    public Color warningColor = new Color(1f, 0.7f, 0.2f);
    public Color dangerColor = new Color(1f, 0.2f, 0.2f);

    private Transform bombTarget;
    private RectTransform barRect;
    private RectTransform canvasRect;

    private Canvas mainCanvas;
    private Camera gameCamera;
    private CanvasGroup canvasGroup;

    private void Start()
    {
        barRect = GetComponent<RectTransform>();
        gameCamera = Camera.main;

        if (GameManager.Instance == null ||
            GameManager.Instance.bomb == null)
        {
            Debug.LogError("BombTimerBar: Bomb를 찾을 수 없습니다.");
            enabled = false;
            return;
        }

        bombTarget = GameManager.Instance.bomb.transform;

        // Screen Space Canvas 자동 검색
        Canvas[] canvases =
            FindObjectsByType<Canvas>(FindObjectsSortMode.None);

        foreach (Canvas canvas in canvases)
        {
            if (canvas.renderMode != RenderMode.WorldSpace &&
                canvas.isRootCanvas)
            {
                mainCanvas = canvas;
                break;
            }
        }

        if (mainCanvas == null)
        {
            Debug.LogError(
                "BombTimerBar: Screen Space Canvas가 없습니다."
            );

            enabled = false;
            return;
        }

        // 기존 World Space UI를 일반 Canvas로 자동 이동
        barRect.SetParent(mainCanvas.transform, false);

        canvasRect = mainCanvas.GetComponent<RectTransform>();

        // 타이머 바 크기 및 기준점 자동 설정
        barRect.anchorMin = new Vector2(0.5f, 0.5f);
        barRect.anchorMax = new Vector2(0.5f, 0.5f);
        barRect.pivot = new Vector2(0.5f, 0.5f);

        barRect.localScale = Vector3.one;
        barRect.localRotation = Quaternion.identity;
        barRect.sizeDelta = new Vector2(140f, 22f);

        // 클릭 방지
        Image background = GetComponent<Image>();

        if (background != null)
            background.raycastTarget = false;

        if (timerFill != null)
        {
            timerFill.type = Image.Type.Filled;
            timerFill.fillMethod =
                Image.FillMethod.Horizontal;
            timerFill.fillOrigin = 0;
            timerFill.raycastTarget = false;

            RectTransform fillRect =
                timerFill.GetComponent<RectTransform>();

            fillRect.anchorMin = new Vector2(0.5f, 0.5f);
            fillRect.anchorMax = new Vector2(0.5f, 0.5f);
            fillRect.pivot = new Vector2(0.5f, 0.5f);

            fillRect.anchoredPosition = Vector2.zero;
            fillRect.localScale = Vector3.one;
            fillRect.localRotation = Quaternion.identity;
            fillRect.sizeDelta = new Vector2(132f, 14f);
        }

        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        Debug.Log("BombTimerBar: 폭탄 추적 시작!");
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

        // 게임 종료 시 타이머 숨기기
        if (GameManager.Instance.GameEnded)
        {
            canvasGroup.alpha = 0f;
            return;
        }

        canvasGroup.alpha = 1f;

        // 폭탄의 월드 위치를 화면 위치로 변환
        Vector3 worldPosition =
            bombTarget.position + Vector3.up * heightOffset;

        Vector3 screenPosition =
            gameCamera.WorldToScreenPoint(worldPosition);

        // 화면 뒤에 있으면 숨기기
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

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            uiCamera,
            out localPosition
        );

        // 타이머 바가 폭탄을 따라 이동
        barRect.anchoredPosition = localPosition;

        if (timerFill == null)
            return;

        // 폭발까지 남은 비율 표시
        float progress =
            GameManager.Instance.ExplosionProgress;

        timerFill.fillAmount = progress;

        // 남은 시간에 따라 색상 변경
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
