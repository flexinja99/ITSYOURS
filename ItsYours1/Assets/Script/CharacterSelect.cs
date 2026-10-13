
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

        if (selectedCharacter != null)
            selectedCharacter.SetSelected(false);

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

    private void OnDisable()
    {
        if (selectedCharacter == this)
            selectedCharacter = null;
    }
}
