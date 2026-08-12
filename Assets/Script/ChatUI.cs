using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ChatUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject chatPanel;
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

    void Start()
    {
        if (chatPanel != null)
            chatPanel.SetActive(false);
    }

    public bool IsOpen()
    {
        return chatPanel != null && chatPanel.activeSelf;
    }

    public void OpenChat(NPCBrainTest npc)
    {
        currentNPC = npc;
        chatHistory = "";
        isWaitingReply = false;

        if (chatPanel != null)
            chatPanel.SetActive(true);

        if (playerController != null)
        {
            playerController.enabled = true;
            playerController.canControl = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        inputField.text = "";
        inputField.Select();
        inputField.ActivateInputField();

        ShowNPCText(npc.npcName + ": Ada yang ingin kamu tanyakan?");
    }

    public void SendMessageToNPC()
    {
        if (currentNPC == null) return;
        if (isWaitingReply) return;

        string playerMessage = inputField.text.Trim();

        if (string.IsNullOrWhiteSpace(playerMessage))
            return;

        inputField.text = "";
        inputField.Select();
        inputField.ActivateInputField();

        // Sapaan pendek jangan dikirim ke AI, biar NPC nggak overthinking
        if (IsGreeting(playerMessage))
        {
            string reply = GetGreetingReply(currentNPC);

            ShowNPCText(currentNPC.npcName + ": " + reply);

            chatHistory += "\nDetektif: " + playerMessage;
            chatHistory += "\n" + currentNPC.npcName + ": " + reply;

            return;
        }

        if (aiChatClient == null)
        {
            Debug.LogError("AIChatClient belum diisi di Inspector ChatUI!");
            ShowNPCText(currentNPC.npcName + ": Sistem AI belum tersambung.");
            return;
        }

        StartCoroutine(SendMessageRoutine(playerMessage));
    }

    IEnumerator SendMessageRoutine(string playerMessage)
    {
        isWaitingReply = true;

        NPCBrainTest npc = currentNPC;

        ShowNPCText(npc.npcName + ": ...");

        yield return StartCoroutine(aiChatClient.AskNPC(
            npc,
            playerMessage,
            chatHistory,
            reply =>
            {
                if (!IsOpen()) return;
                if (currentNPC != npc) return;

                string cleanReply = CleanNPCReply(reply, npc);

                ShowNPCText(npc.npcName + ": " + cleanReply);

                chatHistory += "\nDetektif: " + playerMessage;
                chatHistory += "\n" + npc.npcName + ": " + cleanReply;

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

        chatText.text = "";

        foreach (char letter in message)
        {
            chatText.text += letter;
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

    void Update()
    {
        if (!IsOpen()) return;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            SendMessageToNPC();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseChat();
        }
    }
}