

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

    [Header("Victory / Defeat UI")]
    public GameObject victoryPanel;
    public GameObject defeatPanel;

    private float remainingTime;

    // NEW: 현재 폭탄 타이머의 전체 시간
    private float explosionDuration;

    private bool gameEnded;

    private GameObject[] allCharacters;
    private bool[] eliminated;

    public float RemainingTime => remainingTime;
    public bool GameEnded => gameEnded;

    // NEW: 폭탄 타이머 게이지 (1 = 가득 참, 0 = 폭발)
    public float ExplosionProgress
    {
        get
        {
            if (gameEnded || explosionDuration <= 0f)
                return 0f;

            return Mathf.Clamp01(
                remainingTime / explosionDuration
            );
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Time.timeScale = 1f;

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

        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        if (defeatPanel != null)
            defeatPanel.SetActive(false);

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
        Debug.Log(
            "초기 폭발 타이머: " + remainingTime + "초"
        );
    }

    private void Update()
    {
        if (gameEnded)
            return;

        remainingTime -= Time.deltaTime;

        if (remainingTime > 0f)
            return;

        // 폭발 중복 실행 방지
        remainingTime = float.MaxValue;

        BombController bombController =
            bomb.GetComponent<BombController>();

        if (bombController == null)
        {
            Debug.LogError(
                "BombController를 찾을 수 없습니다."
            );
            return;
        }

        bombController.Explode();

        // 게임이 계속되면 타이머 초기화
        if (!gameEnded)
        {
            ResetExplosionTimer();

            Debug.Log(
                "다음 폭발 타이머: " + remainingTime + "초"
            );
        }
    }

    // NEW: 전체 시간과 남은 시간을 함께 설정
    private void ResetExplosionTimer()
    {
        float minTime = Mathf.Max(
            0.01f,
            minExplosionTime
        );

        float maxTime = Mathf.Max(
            minTime,
            maxExplosionTime
        );

        explosionDuration = Random.Range(
            minTime,
            maxTime
        );

        remainingTime = explosionDuration;
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

        if (allCharacters == null)
            return null;

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

        return candidates[
            Random.Range(0, candidates.Count)
        ];
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

            Debug.Log(
                character.name + " 탈락!"
            );

            break;
        }

        UpdateSurvivorUI();

        // 플레이어가 탈락하면 즉시 패배
        if (character == player)
        {
            EndGame(false);
            return;
        }

        // AI 탈락 시 승리 조건 확인
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
        if (gameEnded)
            return;

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

        if (count == 1 && lastAlive == player)
        {
            EndGame(true);
        }
        else
        {
            EndGame(false);
        }
    }

    private void EndGame(bool victory)
    {
        if (gameEnded)
            return;

        gameEnded = true;

        string message = victory ? "승리!" : "패배!";

        Debug.Log(message);

        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (victoryPanel != null || defeatPanel != null)
        {
            if (victoryPanel != null)
                victoryPanel.SetActive(victory);

            if (defeatPanel != null)
                defeatPanel.SetActive(!victory);
        }
        else
        {
            if (resultPanel != null)
                resultPanel.SetActive(true);

            if (resultText != null)
                resultText.text = message;
        }

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
