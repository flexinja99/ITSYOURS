
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
    [Header("Game Scene")]
    public string gameSceneName;

    [Header("Volume Settings")]
    public Slider musicSlider;
    public Slider sfxSlider;

    private void Start()
    {
        if (musicSlider != null)
        {
            musicSlider.minValue = 0f;
            musicSlider.maxValue = 1f;

            musicSlider.SetValueWithoutNotify(
                PlayerPrefs.GetFloat("MusicVolume", 0.25f)
            );

            musicSlider.onValueChanged.AddListener(
                SetMusicVolume
            );
        }

        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0f;
            sfxSlider.maxValue = 1f;

            sfxSlider.SetValueWithoutNotify(
                PlayerPrefs.GetFloat("SfxVolume", 0.8f)
            );

            sfxSlider.onValueChanged.AddListener(
                SetSfxVolume
            );
        }
    }

    public void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSfxVolume(float value)
    {
        PlayerPrefs.SetFloat("SfxVolume", value);
    }

    public void StartGame()
    {
        PlayerPrefs.Save();

        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogError("게임 씬 이름을 입력하세요!");
            return;
        }

        SceneManager.LoadScene(gameSceneName);
    }

    private void OnDestroy()
    {
        if (musicSlider != null)
            musicSlider.onValueChanged.RemoveListener(
                SetMusicVolume
            );

        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(
                SetSfxVolume
            );
    }
}
