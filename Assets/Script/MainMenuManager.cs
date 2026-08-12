using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Config")]
    [Tooltip("Nama scene game yang akan dibuka saat klik MULAI")]
    public string targetGameScene = "OutdoorsScene";

    [Header("UI References")]
    public Button startButton;
    public Button languageButton;
    public TMP_Text languageText;
    public CanvasGroup fadeOverlay;

    [Header("Title Customization (Opsional)")]
    public TMP_Text titleText;
    public TMP_Text subtitleText;
    public TMP_Text episodeText;

    private bool isTransitioning = false;
    private bool isBahasaID = true;

    void Start()
    {
        // Pastikan kursor terlihat dan bebas bergerak di Main Menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);

        if (languageButton != null)
            languageButton.onClick.AddListener(OnLanguageButtonClicked);

        if (fadeOverlay != null)
        {
            fadeOverlay.alpha = 1f;
            StartCoroutine(FadeInRoutine());
        }
    }

    public void OnStartButtonClicked()
    {
        if (isTransitioning) return;
        StartCoroutine(StartGameRoutine());
    }

    public void OnLanguageButtonClicked()
    {
        isBahasaID = !isBahasaID;
        if (languageText != null)
        {
            languageText.text = isBahasaID ? "BAHASA ID" : "ENGLISH";
        }

        Debug.Log($"[MainMenu] Bahasa diubah ke: {(isBahasaID ? "Bahasa Indonesia" : "English")}");
    }

    private IEnumerator FadeInRoutine()
    {
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fadeOverlay.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            yield return null;
        }

        fadeOverlay.alpha = 0f;
        fadeOverlay.blocksRaycasts = false;
    }

    private IEnumerator StartGameRoutine()
    {
        isTransitioning = true;

        // Animasi button scale pulse
        if (startButton != null)
        {
            Vector3 origScale = startButton.transform.localScale;
            startButton.transform.localScale = origScale * 0.92f;
            yield return new WaitForSeconds(0.1f);
            startButton.transform.localScale = origScale;
        }

        // Fade out transition
        if (fadeOverlay != null)
        {
            fadeOverlay.blocksRaycasts = true;
            float duration = 0.6f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                fadeOverlay.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                yield return null;
            }
            fadeOverlay.alpha = 1f;
        }

        yield return new WaitForSeconds(0.1f);

        // Load scene permainan
        if (Application.CanStreamedLevelBeLoaded(targetGameScene))
        {
            SceneManager.LoadScene(targetGameScene);
        }
        else
        {
            Debug.LogWarning($"[MainMenu] Scene '{targetGameScene}' belum didaftarkan di Build Settings! Mencoba load langsung...");
            SceneManager.LoadScene(targetGameScene);
        }
    }

    public void QuitGame()
    {
        Debug.Log("[MainMenu] Keluar dari game.");
        Application.Quit();
    }
}
