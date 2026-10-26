
using UnityEngine;

public class CharacterSelect : MonoBehaviour
{
    private static CharacterSelect selectedCharacter;

    private BombController bombController;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    public Color selectedColor = Color.cyan;

    private void Start()
    {
        bombController =
            FindFirstObjectByType<BombController>();

        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    private void OnMouseDown()
    {
        if (bombController == null ||
            GameManager.Instance == null ||
            GameManager.Instance.GameEnded)
        {
            return;
        }

        bombController.SetTarget(gameObject);

        if (bombController.Target != gameObject)
            return;

        // 이전 선택된 캐릭터 색상 복구
        if (selectedCharacter != null)
            selectedCharacter.SetSelected(false);

        // 새로운 캐릭터 선택
        selectedCharacter = this;
        SetSelected(true);
    }

    private void SetSelected(bool selected)
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.color = selected
            ? selectedColor
            : originalColor;
    }

    // NEW: 선택된 캐릭터를 원래 색으로 복구
    public static void ClearSelection()
    {
        if (selectedCharacter != null)
        {
            selectedCharacter.SetSelected(false);
        }

        selectedCharacter = null;
    }

    private void OnDisable()
    {
        if (selectedCharacter == this)
        {
            SetSelected(false);
            selectedCharacter = null;
        }
    }
}
