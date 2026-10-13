
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

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

    private GameObject[] allCharacters;
    private bool[] eliminated;

    public float RemainingTime => remainingTime;
    public bool GameEnded => gameEnded;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (player == null ||
            bomb == null ||
            aiPlayers == null ||
            aiPlayers.Length != 3)
        {
            Debug.LogError(
                "GameManager: Player, Bomb, AI 3명을 연결하세요."
            );

            enabled = false;
            return;
        }

        for (int i = 0; i < aiPlayers.Length; i++)
        {
            if (aiPlayers[i] == null)
            {
                Debug.LogError(
                    "GameManager: AI 슬롯 " + i + "이 비어 있습니다."
                );

                enabled = false;
                return;
            }
        }

        allCharacters = new GameObject[4];
        allCharacters[0] = player;

        for (int i = 0; i < aiPlayers.Length; i++)
        {
            allCharacters[i + 1] = aiPlayers[i];
        }

        eliminated = new bool[allCharacters.Length];

        gameEnded = false;

        if (resultPanel != null)
            resultPanel.SetActive(false);

        BombController bombController =
            bomb.GetComponent<BombController>();

        if (bombController == null)
        {
            Debug.LogError(
                "Bomb에 BombController가 없습니다."
            );

            enabled = false;
            return;
        }

        bomb.SetActive(true);

        bombController.Initialize(player);

        ResetExplosionTimer();
        UpdateSurvivorUI();

        Debug.Log("이거 네 거야! 게임 시작!");
        Debug.Log("초기 폭발 타이머: " + remainingTime + "초");
    }

    private void Update()
    {
        if (gameEnded)
            return;

        remainingTime -= Time.deltaTime;

        if (remainingTime > 0f)
            return;

        // 같은 프레임에 폭발 처리가 반복되지 않도록
        // 먼저 타이머를 정지 상태로 만든다.
        remainingTime = float.MaxValue;

        BombController bombController =
            bomb.GetComponent<BombController>();

        if (bombController == null)
        {
            Debug.LogError("BombController를 찾을 수 없습니다.");
            return;
        }

        bombController.Explode();

        // 게임이 끝나지 않았을 때만 다음 타이머 설정
        if (!gameEnded)
        {
            ResetExplosionTimer();

            Debug.Log(
                "다음 폭발 타이머: " + remainingTime + "초"
            );
        }
    }

    private void ResetExplosionTimer()
    {
        float minTime = Mathf.Max(0f, minExplosionTime);
        float maxTime = Mathf.Max(minTime, maxExplosionTime);

        remainingTime = Random.Range(minTime, maxTime);
    }

    public bool IsAlive(GameObject character)
    {
        if (character == null ||
            allCharacters == null ||
            eliminated == null)
        {
            return false;
        }

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
        List<GameObject> candidates =
            new List<GameObject>();

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

            // 이미 탈락 처리된 캐릭터는 다시 처리하지 않는다.
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
        if (survivorText == null ||
            allCharacters == null)
        {
            return;
        }

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

        if (count > 1)
            return;

        gameEnded = true;

        string message;

        if (lastAlive == null)
        {
            message = "무승부!";
        }
        else if (lastAlive == player)
        {
            message = "승리!";
        }
        else
        {
            message = "패배!";
        }

        Debug.Log(message);

        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (resultText != null)
            resultText.text = message;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}
