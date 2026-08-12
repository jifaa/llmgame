using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Text;
using System.Collections;

public class AIChatClient : MonoBehaviour
{
    public AIDataLoader aiDataLoader;

    [Header("AI Server Config")]
    public string serverUrl = "http://localhost:8000/generate";

    [Header("Player State")]
    [TextArea(3, 10)]
    public string playerEvidence = "Belum ada bukti yang ditemukan.";

    public IEnumerator AskNPC(
        NPCBrainTest npc,
        string playerMessage,
        string chatHistory,
        Action<string> onReply
    )
    {
        // 1. Safety Guard: Cek apakah AIDataLoader udah dipasang di Inspector
        if (aiDataLoader == null)
        {
            Debug.LogError("[FATAL] AIDataLoader belum dipasang di AIChatClient! NPC lu bakal bisu.");
            onReply?.Invoke("... (Aku seperti kehilangan arah)");
            yield break;
        }

        // 2. Safety Guard: Cek apakah komponen NPC atau ID-nya kosong
        if (npc == null || string.IsNullOrWhiteSpace(npc.npcId))
        {
            Debug.LogError("[FATAL] NPC GameObject tidak punya script NPCBrainTest atau npcId-nya kosong melompong!");
            onReply?.Invoke("... (Siapa aku? Aku tidak tahu namaku sendiri.)");
            yield break;
        }

        // 3. Build prompt pake loader
        string prompt = aiDataLoader.BuildPrompt(
            npc.npcId,
            playerMessage,
            playerEvidence,
            chatHistory
        );

        // 4. Safety Guard: Cek apakah profil NPC beneran ketemu di JSON
        // Ini buat nangkep bug "Raka" yang kemarin bikin NPC lu kena mental
        if (prompt.StartsWith("NPC profile tidak ditemukan"))
        {
            Debug.LogError($"[FATAL] Gagal bikin prompt! {prompt}. " +
                           $"Pastiin npcId '{npc.npcId}' di GameObject '{npc.gameObject.name}' " +
                           $"udah sama persis dengan ID yang ada di npc_profiles.json (case-sensitive!).");
            onReply?.Invoke("Aduh, kepalaku pusing banget... (Error: Profil NPC tidak sinkron)");
            yield break;
        }

        Debug.Log("=====================================");
        Debug.Log($"PLAYER MESSAGE TO [{npc.npcName.ToUpper()}]: " + playerMessage);
        Debug.Log("PROMPT TERKIRIM KE AI:\n" + prompt);
        Debug.Log("=====================================");

        // 5. Setup JSON Payload
        AIRequest requestData = new AIRequest
        {
            prompt = prompt
        };

        string jsonBody = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        UnityWebRequest request = new UnityWebRequest(serverUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // Kirim request ke FastAPI server
        yield return request.SendWebRequest();

        // 6. Safety Guard: Handler kalau server FastAPI tewas atau port salah
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[SERVER ERROR] Gagal konek ke backend AI: {request.error}. " +
                           $"Coba cek, server.py lu udah dijalankan di {serverUrl} belum?");
            onReply?.Invoke("Aku... tidak bisa menjawab sekarang. (Koneksi ke otak terputus)");
            yield break;
        }

        // 7. Parsing Response dengan aman
        try
        {
            string jsonResponse = request.downloadHandler.text;
            AIResponse response = JsonUtility.FromJson<AIResponse>(jsonResponse);

            if (response == null || string.IsNullOrWhiteSpace(response.reply))
            {
                Debug.LogWarning("[WARNING] Response dari AI kosong melompong, bjir.");
                onReply?.Invoke("Aku tidak tahu harus menjawab apa.");
            }
            else
            {
                onReply?.Invoke(response.reply);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[PARSE ERROR] Gagal parsing JSON dari server: {ex.Message}");
            onReply?.Invoke("Otakku mendadak blank... (Error parsing)");
        }
    }
}

[Serializable]
public class AIRequest
{
    public string prompt;
}

[Serializable]
public class AIResponse
{
    public string reply;
}