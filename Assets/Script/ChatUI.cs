using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ChatUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject chatPanel;
    public TMP_Text npcNameText;
    public TMP_Text chatText;
    public TMP_InputField inputField;

    [Header("AI")]
    public AIChatClient aiChatClient;

    [Header("Player")]
    public FirstPerson playerController;

    [Header("Typing Effect")]
    public float typeSpeed = 0.02f;

    private NPCBrainTest currentNPC;
    private Coroutine typingCoroutine;

    private string chatHistory = "";
    private bool isWaitingReply = false;

    private readonly HashSet<string> greetingWords = new HashSet<string>
    {
        "halo", "hallo", "hai", "hei", "hello",
        "permisi", "pagi", "siang", "sore", "malam",
        "yo", "woy", "oy"
    };

    void Awake()
    {
        EnsureEventSystem();
    }

    void Start()
    {
        AutoFindReferences();
        if (chatPanel != null)
            chatPanel.SetActive(false);
    }

    void Update()
    {
        if (!IsOpen()) return;

        // Tekan Enter untuk langsung kirim pesan
        if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && !isWaitingReply)
        {
            if (inputField != null && !string.IsNullOrWhiteSpace(inputField.text))
            {
                SendMessageToNPC();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseChat();
        }
    }

    private void EnsureEventSystem()
    {
        if (EventSystem.current == null)
        {
            var es = Object.FindAnyObjectByType<EventSystem>(FindObjectsInactive.Include);
            if (es == null)
            {
                GameObject esGo = new GameObject("EventSystem");
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<StandaloneInputModule>();
            }
        }
    }

    private void AutoFindReferences()
    {
        if (chatPanel == null)
        {
            GameObject p = GameObject.Find("ChatPanel");
            chatPanel = p != null ? p : gameObject;
        }

        if (chatPanel == null) return;

        // 1. Cari ChatText
        if (chatText == null)
        {
            Transform tChat = chatPanel.transform.Find("ChatText") ?? chatPanel.transform.Find("Chat_Text") ?? chatPanel.transform.Find("Dialog_Text");
            if (tChat != null)
                chatText = tChat.GetComponent<TMP_Text>();

            if (chatText == null)
            {
                foreach (var t in chatPanel.GetComponentsInChildren<TMP_Text>(true))
                {
                    if (t.GetComponentInParent<TMP_InputField>() == null && t.GetComponentInParent<Button>() == null)
                    {
                        chatText = t;
                        break;
                    }
                }
            }
        }

        // 2. Cari InputField
        if (inputField == null)
        {
            inputField = chatPanel.GetComponentInChildren<TMP_InputField>(true);
        }

        if (inputField != null)
        {
            inputField.interactable = true;
            inputField.onSubmit.RemoveListener(OnInputSubmit);
            inputField.onSubmit.AddListener(OnInputSubmit);
        }

        // 3. Cari Header NameText jika ada
        if (npcNameText == null)
        {
            Transform tName = chatPanel.transform.Find("NPC_Name_Text") ?? chatPanel.transform.Find("NameText") ?? chatPanel.transform.Find("Title_Text");
            if (tName != null)
                npcNameText = tName.GetComponent<TMP_Text>();
        }

        // 4. Cari AIChatClient
        if (aiChatClient == null)
        {
            aiChatClient = Object.FindAnyObjectByType<AIChatClient>();
            if (aiChatClient == null)
            {
                GameObject clientGo = new GameObject("AIChatClient");
                aiChatClient = clientGo.AddComponent<AIChatClient>();
            }
        }

        // 5. Auto-hook SendButton
        Button btn = chatPanel.GetComponentInChildren<Button>(true);
        if (btn != null)
        {
            btn.onClick.RemoveListener(SendMessageToNPC);
            btn.onClick.AddListener(SendMessageToNPC);
        }
    }

    public bool IsOpen()
    {
        return chatPanel != null && chatPanel.activeSelf;
    }

    private string GetDisplayName(NPCBrainTest npc)
    {
        if (npc == null) return "Saksi";
        if (!string.IsNullOrEmpty(npc.npcName)) return npc.npcName;

        string n = npc.gameObject.name.ToLower();
        if (n.Contains("normal-man-a") || n.Contains("bima")) return "Bima Santoso";
        if (n.Contains("normal-man-b") || n.Contains("ardi") || n.Contains("maya")) return "Ardi Adrian";
        if (n.Contains("normal-man-c") || n.Contains("dito")) return "Dito Pradana";

        return npc.gameObject.name;
    }

    private string FormatNPCDialogue(string name, string text)
    {
        return $"<color=#E0A838><b>[{name}]</b></color>\n{text}";
    }

    public void OpenChat(NPCBrainTest npc)
    {
        currentNPC = npc;
        chatHistory = "";
        isWaitingReply = false;

        AutoFindReferences();

        string displayName = GetDisplayName(npc);
        if (npc != null && string.IsNullOrEmpty(npc.npcName)) npc.npcName = displayName;

        if (npcNameText != null)
            npcNameText.text = displayName;

        if (chatPanel != null)
            chatPanel.SetActive(true);

        if (playerController != null)
        {
            playerController.enabled = true;
            playerController.canControl = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (inputField != null)
        {
            inputField.text = "";
            inputField.Select();
            inputField.ActivateInputField();
        }

        ShowNPCText(FormatNPCDialogue(displayName, "Ada yang ingin kamu tanyakan?"));
    }

    public void SendMessageToNPC()
    {
        if (currentNPC == null) return;
        if (isWaitingReply) return;

        string playerMessage = inputField != null ? inputField.text.Trim() : "";

        if (string.IsNullOrWhiteSpace(playerMessage))
            return;

        if (inputField != null)
        {
            inputField.text = "";
            inputField.Select();
            inputField.ActivateInputField();
        }

        string displayName = GetDisplayName(currentNPC);

        // Sapaan pendek jangan dikirim ke AI, biar NPC nggak overthinking
        if (IsGreeting(playerMessage))
        {
            string reply = GetGreetingReply(currentNPC);

            ShowNPCText(FormatNPCDialogue(displayName, reply));

            chatHistory += "\nDetektif: " + playerMessage;
            chatHistory += "\n" + displayName + ": " + reply;

            return;
        }

        if (aiChatClient == null)
        {
            Debug.LogError("AIChatClient belum diisi di Inspector ChatUI!");
            ShowNPCText(FormatNPCDialogue(displayName, "Sistem AI belum tersambung."));
            return;
        }

        StartCoroutine(SendMessageRoutine(playerMessage));
    }

    private void OnInputSubmit(string text)
    {
        if (!isWaitingReply && !string.IsNullOrWhiteSpace(text))
        {
            SendMessageToNPC();
        }
    }

    IEnumerator SendMessageRoutine(string playerMessage)
    {
        isWaitingReply = true;

        NPCBrainTest npc = currentNPC;
        string displayName = GetDisplayName(npc);

        ShowNPCText(FormatNPCDialogue(displayName, "..."));

        yield return StartCoroutine(aiChatClient.AskNPC(
            npc,
            playerMessage,
            chatHistory,
            reply =>
            {
                if (!IsOpen()) return;
                if (currentNPC != npc) return;

                string cleanReply = CleanNPCReply(reply, npc);

                ShowNPCText(FormatNPCDialogue(displayName, cleanReply));

                chatHistory += "\nDetektif: " + playerMessage;
                chatHistory += "\n" + displayName + ": " + cleanReply;

                // Biar history nggak kegedean dan bikin AI mabok
                if (chatHistory.Length > 1200)
                {
                    chatHistory = chatHistory.Substring(chatHistory.Length - 1200);
                }
            }
        ));

        isWaitingReply = false;

        if (IsOpen() && inputField != null)
        {
            inputField.Select();
            inputField.ActivateInputField();
        }
    }

    bool IsGreeting(string message)
    {
        string msg = message.ToLower().Trim();
        msg = Regex.Replace(msg, @"[^\p{L}\p{N}\s]", "");
        msg = Regex.Replace(msg, @"\s+", " ").Trim();

        string[] words = msg.Split(' ');

        // "halo", "hai", "permisi"
        if (words.Length == 1 && greetingWords.Contains(words[0]))
            return true;

        // "halo pak", "hai dito", "permisi mas"
        if (words.Length <= 2 && greetingWords.Contains(words[0]))
            return true;

        return false;
    }

    string GetGreetingReply(NPCBrainTest npc)
    {
        switch (npc.npcId.ToLower())
        {
            case "bima":
                return "Iya. Mau tanya apa?";

            case "maya":
            case "ardi":
                return "Halo. Ada yang ingin kamu tanyakan?";

            case "dito":
                return "Oh, hai. Ada perlu apa?";

            default:
                return "Iya... ada apa?";
        }
    }

    string CleanNPCReply(string reply, NPCBrainTest npc)
{
    if (string.IsNullOrWhiteSpace(reply))
        return "Aku belum bisa jawab itu.";

    string clean = reply.Trim();

    // 1. SAFETY GUARD: Kalau AI kepotong pas lagi mikir (gak ada tag penutup </think>)
    if (clean.Contains("<think>") && !clean.Contains("</think>"))
    {
        return "..."; // NPC mode gugup/mikir karena teksnya kepotong
    }

    // 2. Hapus thinking tag yang lengkap beserta isinya
    clean = Regex.Replace(clean, @"<think>.*?</think>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);

    // 3. Hapus format label standar
    clean = Regex.Replace(clean, @"^(NPC|Pemain|Detektif|Jawaban NPC)\s*:\s*", "", RegexOptions.IgnoreCase);

    // 4. Hapus nama NPC di awal respon biar gak dobel
    string fullName = npc.npcName;
    string firstName = fullName.Split(' ')[0];
    clean = Regex.Replace(clean, @"^" + Regex.Escape(fullName) + @"\s*:\s*", "", RegexOptions.IgnoreCase);
    clean = Regex.Replace(clean, @"^" + Regex.Escape(firstName) + @"\s*:\s*", "", RegexOptions.IgnoreCase);

    // 5. Bersihkan tanda kutip luar
    clean = clean.Trim('"', '“', '”', '\'', ' ');

    // 6. Konversi istilah
    clean = clean.Replace("aku ini", "aku").Replace("Aku ini", "Aku");
    clean = clean.Replace("aku adalah", "aku").Replace("Aku adalah", "Aku");
    clean = clean.Replace("pemain", "detektif").Replace("Pemain", "Detektif");

    // 7. Rapikan spasi & huruf kapital di awal
    clean = Regex.Replace(clean, @"\s+", " ").Trim();
    if (string.IsNullOrWhiteSpace(clean))
        return "Aku belum bisa jawab itu.";

    clean = char.ToUpper(clean[0]) + clean.Substring(1);

    return clean;
}

    void ShowNPCText(string message)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeText(message));
    }

    IEnumerator TypeText(string message)
    {
        if (chatText == null) yield break;

        chatText.text = message;
        chatText.maxVisibleCharacters = 0;
        chatText.ForceMeshUpdate();

        int totalCharacters = chatText.textInfo.characterCount;
        for (int i = 1; i <= totalCharacters; i++)
        {
            chatText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(typeSpeed);
        }
    }

    public void CloseChat()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isWaitingReply = false;

        if (chatPanel != null)
            chatPanel.SetActive(false);

        if (playerController != null)
        {
            playerController.enabled = true;
            playerController.canControl = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentNPC = null;
    }
}