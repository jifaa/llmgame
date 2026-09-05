using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// SceneTeleporter: Komponen untuk berpindah antar scene di Unity.
/// Mendukung:
/// 1. Tombol UI di Layar (On Click Event) dengan fitur Auto-Hide saat interaksi NPC
/// 2. Dekati Pintu & Tekan tombol 'E' (Trigger Zone)
/// 3. Jalan menembus pintu (Instant Trigger)
/// </summary>
public class SceneTeleporter : MonoBehaviour
{
    [Header("Scene Destination")]
    [Tooltip("Nama file scene tujuan (contoh: OutdoorsScene atau PoliceIndoor)")]
    public string targetSceneName = "OutdoorsScene";

    [Header("UI Button & Auto-Hide Settings")]
    [Tooltip("Target GameObject Tombol/UI yang akan disembunyikan saat sedang interogasi/chat. Jika kosong, memakai gameObject ini.")]
    public GameObject uiButtonRoot;

    [Tooltip("Sembunyikan tombol ini saat pemain sedang berinteraksi/chat dengan NPC?")]
    public bool hideWhenInteracting = true;

    [Header("References (Auto-Found if Empty)")]
    public ChatUI chatUI;

    [Header("Trigger Settings (Jika dipasang di Objek Pintu Fisik)")]
    [Tooltip("Apakah player harus menekan tombol E untuk pindah? Jika false, langsung pindah saat masuk trigger.")]
    public bool requireButtonPress = true;
    public KeyCode interactKey = KeyCode.E;

    [Header("UI Prompt Trigger (Opsional)")]
    public GameObject promptUI;
    public TMP_Text promptText;
    [TextArea]
    public string customPromptMessage = "Tekan [E] untuk Keluar";

    private bool isPlayerInRange = false;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        if (uiButtonRoot == null)
            uiButtonRoot = this.gameObject;

        canvasGroup = uiButtonRoot.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = uiButtonRoot.AddComponent<CanvasGroup>();

        AutoFindReferences();
    }

    void Start()
    {
        if (promptUI != null)
            promptUI.SetActive(false);
    }

    void Update()
    {
        // 1. Logika Auto-Hide Tombol saat sedang ngobrol dengan NPC
        if (hideWhenInteracting && uiButtonRoot != null)
        {
            bool isInteracting = IsInteractingWithNPC();
            SetUIVisible(!isInteracting);
        }

        // 2. Logika Trigger Fisik (jika didekati player)
        if (isPlayerInRange && requireButtonPress)
        {
            if (Input.GetKeyDown(interactKey))
            {
                ChangeScene();
            }
        }
    }

    private void AutoFindReferences()
    {
        if (chatUI == null)
            chatUI = Object.FindAnyObjectByType<ChatUI>(FindObjectsInactive.Include);
    }

    public bool IsInteractingWithNPC()
    {
        if (chatUI == null)
            AutoFindReferences();

        if (chatUI != null)
        {
            if (chatUI.chatPanel != null && chatUI.chatPanel != chatUI.gameObject)
            {
                return chatUI.chatPanel.activeSelf;
            }
            return chatUI.IsOpen();
        }

        return false;
    }

    private void SetUIVisible(bool visible)
    {
        if (canvasGroup == null && uiButtonRoot != null)
            canvasGroup = uiButtonRoot.GetComponent<CanvasGroup>() ?? uiButtonRoot.AddComponent<CanvasGroup>();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsPlayer(other))
        {
            isPlayerInRange = true;

            if (promptUI != null)
            {
                promptUI.SetActive(true);
                if (promptText != null)
                    promptText.text = customPromptMessage;
            }

            if (!requireButtonPress)
            {
                ChangeScene();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsPlayer(other))
        {
            isPlayerInRange = false;

            if (promptUI != null)
                promptUI.SetActive(false);
        }
    }

    private bool IsPlayer(Collider col)
    {
        return col.CompareTag("Player") || 
               col.GetComponent<FirstPerson>() != null || 
               col.GetComponentInParent<FirstPerson>() != null ||
               col.name.ToLower().Contains("player") ||
               col.name.ToLower().Contains("detective");
    }

    /// <summary>
    /// Fungsi publik yang dipanggil oleh tombol UI (Button OnClick)
    /// </summary>
    public void ChangeScene()
    {
        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogError("[SceneTeleporter] Nama targetSceneName kosong!");
            return;
        }

        Debug.Log($"[SceneTeleporter] Berpindah ke scene: {targetSceneName}...");
        SceneManager.LoadScene(targetSceneName);
    }

    /// <summary>
    /// Fungsi alternatif dengan nama scene dinamis
    /// </summary>
    public void ChangeSceneTo(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName)) return;
        Debug.Log($"[SceneTeleporter] Berpindah ke scene: {sceneName}...");
        SceneManager.LoadScene(sceneName);
    }
}

