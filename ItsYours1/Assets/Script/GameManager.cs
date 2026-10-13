
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Characters")]
    public GameObject player;
    public GameObject[] aiPlayers;
    public GameObject bomb;

    [Header("Random Explosion Time")]
    public float minExplosionTime = 20f;
    public float maxExplosionTime = 35f;

    [Header("Optional UI")]
    public TMP_Text survivorText;
    public GameObject resultPanel;
    public TMP_Text resultText;

    private float remainingTime;
    private bool gameEnded;

    public float RemainingTime => remainingTime;
    public bool GameEnded => gameEnded;

    private GameObject[] allCharacters;
    private bool[] eliminated;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (player == null || bomb == null ||
            aiPlayers == null || aiPlayers.Length != 3)
        {
            Debug.LogError(
                "GameManager: Player, Bomb, AI 3명을 연결하세요.");
            enabled = false;
            return;
        }

        allCharacters = new GameObject[4];
        allCharacters[0] = player;

        for (int i = 0; i < aiPlayers.Length; i++)
            allCharacters[i + 1] = aiPlayers[i];

        eliminated = new bool[allCharacters.Length];

        if (resultPanel != null)
            resultPanel.SetActive(false);

        remainingTime = Random.Range(
            minExplosionTime, maxExplosionTime);

        BombController bombController =
            bomb.GetComponent<BombController>();

        if (bombController == null)
        {
            Debug.LogError("Bomb에 BombController가 없습니다.");
            enabled = false;
            return;
        }

        bombController.Initialize(player);

        UpdateSurvivorUI();

        Debug.Log("이거 네 거야! 게임 시작!");
    }

    private void Update()
    {
        if (gameEnded)
            return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;

            BombController bombController =
                bomb.GetComponent<BombController>();

            if (bombController != null)
                bombController.Explode();
        }
    }

    public bool IsAlive(GameObject character)
    {
        if (character == null || allCharacters == null)
            return false;

        for (int i = 0; i < allCharacters.Length; i++)
        {
            if (allCharacters[i] == character)
                return !eliminated[i];
        }

        return false;
    }

    public GameObject GetRandomAliveCharacterExcept(
        params GameObject[] excluded)
    {
        System.Collections.Generic.List<GameObject> candidates =
            new System.Collections.Generic.List<GameObject>();

        foreach (GameObject character in allCharacters)
        {
            if (!IsAlive(character))
                continue;

            bool skip = false;

            if (excluded != null)
            {
                foreach (GameObject item in excluded)
                {
                    if (character == item)
                    {
                        skip = true;
                        break;
                    }
                }
            }

            if (!skip)
                candidates.Add(character);
        }

        if (candidates.Count == 0)
            return null;

        return candidates[Random.Range(0, candidates.Count)];
    }

    public void EliminateCharacter(GameObject character)
    {
        if (gameEnded || !IsAlive(character))
            return;

        for (int i = 0; i < allCharacters.Length; i++)
        {
            if (allCharacters[i] != character)
                continue;

            eliminated[i] = true;
            character.SetActive(false);
            Debug.Log(character.name + " 탈락!");

            break;
        }

        UpdateSurvivorUI();
        CheckWinner();
    }

    private void UpdateSurvivorUI()
    {
        if (survivorText == null || allCharacters == null)
            return;

        int count = 0;

        foreach (GameObject character in allCharacters)
        {
            if (IsAlive(character))
                count++;
        }

        survivorText.text = "생존 " + count + "명";
    }

    private void CheckWinner()
    {
        GameObject lastAlive = null;
        int count = 0;

        foreach (GameObject character in allCharacters)
        {
            if (IsAlive(character))
            {
                lastAlive = character;
                count++;
            }
        }

        if (count <= 1)
        {
            gameEnded = true;

            string message = lastAlive == player
                ? "승리!"
                : "패배!";

            if (lastAlive == null)
                message = "무승부!";

            Debug.Log(message);

            if (resultPanel != null)
                resultPanel.SetActive(true);

            if (resultText != null)
                resultText.text = message;
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex);
    }
}
