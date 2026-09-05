using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;

public class AIChatClient : MonoBehaviour
{
    public AIDataLoader aiDataLoader;

    [Header("9Router / AI Endpoint Config")]
    public string endpointUrl = "https://api.linstore.my.id/v1/chat/completions";
    public string modelName = "qwen3.8-flash-free";
    public string apiKey = "sk-scOmNEQX3RO1Z22jb5PaBTfPKtfYXZ5lJL30Ksj7sU8w0Pah";


    [Header("Player State")]
    [TextArea(3, 10)]
    public string playerEvidence = "Belum ada bukti yang ditemukan.";

    void Awake()
    {
        MigrateOldConfig();
        EnsureDataLoader();
    }

    void OnValidate()
    {
        MigrateOldConfig();
    }

    private void MigrateOldConfig()
    {
        if (endpointUrl != null && (endpointUrl.Contains("router.juan.web.id") || endpointUrl.Contains("localhost:8000")))
        {
            endpointUrl = "https://api.linstore.my.id/v1/chat/completions";
            modelName = "qwen3.8-flash-free";
            apiKey = "sk-scOmNEQX3RO1Z22jb5PaBTfPKtfYXZ5lJL30Ksj7sU8w0Pah";
        }
        else if (modelName == "deepseek-v4-flash-free")
        {
            modelName = "qwen3.8-flash-free";
        }
    }

    private void EnsureDataLoader()
    {
        if (aiDataLoader == null)
        {
            aiDataLoader = GetComponent<AIDataLoader>();
            if (aiDataLoader == null)
                aiDataLoader = FindAnyObjectByType<AIDataLoader>(FindObjectsInactive.Include);
            if (aiDataLoader == null)
                aiDataLoader = gameObject.AddComponent<AIDataLoader>();
        }
    }

    public IEnumerator AskNPC(
        NPCBrainTest npc,
        string playerMessage,
        string chatHistory,
        Action<string> onReply
    )
    {
        EnsureDataLoader();

        if (aiDataLoader == null)
        {
            Debug.LogError("[FATAL] AIDataLoader belum dipasang di AIChatClient!");
            onReply?.Invoke("... (Aku seperti kehilangan arah)");
            yield break;
        }

        if (npc == null || string.IsNullOrWhiteSpace(npc.npcId))
        {
            Debug.LogError("[FATAL] NPC tidak memiliki npcId!");
            onReply?.Invoke("... (Siapa aku? Aku tidak tahu namaku sendiri.)");
            yield break;
        }

        string prompt = aiDataLoader.BuildPrompt(
            npc.npcId,
            playerMessage,
            playerEvidence,
            chatHistory
        );

        if (prompt.StartsWith("NPC profile tidak ditemukan"))
        {
            Debug.LogError($"[FATAL] Gagal bikin prompt! {prompt}.");
            onReply?.Invoke("Aduh, kepalaku pusing banget... (Error: Profil NPC tidak sinkron)");
            yield break;
        }

        Debug.Log($"[AIChatClient] Mengirim prompt ke AI untuk [{npc.npcName}]...");

        string targetUrl = string.IsNullOrWhiteSpace(endpointUrl) ? "https://api.linstore.my.id/v1/chat/completions" : endpointUrl.Trim();
        if (targetUrl.EndsWith("/v1") || targetUrl.EndsWith("/v1/"))
        {
            targetUrl = targetUrl.TrimEnd('/') + "/chat/completions";
        }
        // Format prompt jadi system context + user question untuk model AI
        string systemPrompt = prompt;
        systemPrompt = Regex.Replace(systemPrompt, @"\[PROSES ANALISIS INTERNAL NPC\].*?\[ATURAN OUTPUT DIALOG \(MUTLAK\)\]", "[ATURAN OUTPUT DIALOG (MUTLAK)]", RegexOptions.Singleline);
        systemPrompt = Regex.Replace(systemPrompt, @"Sebelum merespons.*?tag <think>.*?</think>.*?merespons\?[)\s]*", "", RegexOptions.Singleline);
        systemPrompt += "\n\nPENTING: Langsung tulis HANYA dialog responmu. JANGAN tulis analisis, pemikiran, atau penjelasan apa pun. Cukup 1-2 kalimat dialog saja.";

        var openAIPayload = new OpenAIChatRequest
        {
            model = string.IsNullOrWhiteSpace(modelName) ? "qwen3.8-flash-free" : modelName,
            temperature = 0.4f,
            max_tokens = 300,
            stream = false,
            messages = new OpenAIMessage[]
            {
                new OpenAIMessage { role = "system", content = systemPrompt },
                new OpenAIMessage { role = "user", content = playerMessage }
            }
        };
        string jsonBody = JsonUtility.ToJson(openAIPayload);

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        UnityWebRequest request = new UnityWebRequest(targetUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.timeout = 120;

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            request.SetRequestHeader("Authorization", $"Bearer {apiKey.Trim()}");
        }

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[AI ERROR] Gagal konek ke AI ({targetUrl}): {request.error}\n{request.downloadHandler.text}");
            onReply?.Invoke("Aku... tidak bisa menjawab sekarang. (Koneksi ke otak terputus/timeout)");
            yield break;
        }

        try
        {
            string rawText = request.downloadHandler.text;
            string reply = ParseAIResponse(rawText);

            if (string.IsNullOrWhiteSpace(reply))
            {
                onReply?.Invoke("Aku tidak tahu harus menjawab apa.");
            }
            else
            {
                onReply?.Invoke(reply);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[PARSE ERROR] Gagal parsing respons AI: {ex.Message}");
            onReply?.Invoke("Otakku mendadak blank... (Error parsing)");
        }
    }

    private string ParseAIResponse(string rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson)) return "";

        StringBuilder sbContent = new StringBuilder();

        // 1. Hapus semua kemunculan "reasoning_content":"..." dulu supaya regex content tidak menangkapnya
        string cleaned = Regex.Replace(rawJson, "\"reasoning_content\"\\s*:\\s*\"((?:[^\"\\\\]|\\\\.)*)\"", "\"reasoning_content\":\"\"");

        // 2. Ekstrak semua bagian "content": "..." dari response
        var matches = Regex.Matches(cleaned, "\"content\"\\s*:\\s*\"((?:[^\"\\\\]|\\\\.)*)\"");
        foreach (Match m in matches)
        {
            if (m.Success && m.Groups.Count > 1)
            {
                string val = m.Groups[1].Value;
                if (!string.IsNullOrEmpty(val))
                {
                    try { sbContent.Append(Regex.Unescape(val)); }
                    catch { sbContent.Append(val); }
                }
            }
        }

        if (sbContent.Length > 0)
        {
            Debug.Log($"[AIChatClient] Parsed dialog: {sbContent}");
            return CleanReply(sbContent.ToString());
        }

        // 3. Fallback — JANGAN pernah tampilkan reasoning_content atau raw JSON ke pemain
        Debug.LogWarning("[AIChatClient] Respon AI tidak mengandung teks dialog (content kosong).\nRaw: " + rawJson.Substring(0, Mathf.Min(rawJson.Length, 300)));
        return "Aku sedang tidak fokus... bisa ulangi pertanyaannya?";
    }

    private string CleanReply(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "";

        // Hapus reasoning tag <think>...</think>
        text = Regex.Replace(text, @"<think>.*?</think>", "", RegexOptions.Singleline);
        
        // Hapus analysis yang bocor tanpa tag <think> (pola: numbered analysis 0-6)
        // Cari pola "0. " atau "1. " dst di awal — strip semua sampai habis analysis
        text = Regex.Replace(text, @"^.*?\b[5-6]\.\s+.*?(?=\n[A-Z\u00C0-\u024F]|\n\n)", "", RegexOptions.Singleline);
        
        // Hapus pola analysis umum yang bocor
        text = Regex.Replace(text, @"(?:Let me (?:write |analyze |think ).*?\.|Topics? touched:.*?\.|Intent:.*?\.|Evidence\??:.*?\.|Emotion:.*?\.|CAN(?:NOT)? say:.*?\.)", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"(?:Mode obrolan.*?\.|Private knowledge.*?\.|Bagaimana cara.*?\?)", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        
        text = text.Trim('"', '\u201C', '\u201D', '\'', ' ', '\n', '\r');

        // Jika setelah cleaning kosong, kembalikan fallback
        if (string.IsNullOrWhiteSpace(text))
            return "...";

        return text.Trim();
    }
}

[Serializable]
public class OpenAIChatRequest
{
    public string model;
    public float temperature;
    public int max_tokens;
    public bool stream;
    public OpenAIMessage[] messages;
}

[Serializable]
public class OpenAIMessage
{
    public string role;
    public string content;
}