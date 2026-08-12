using UnityEngine;
using System;
using System.IO;

public class AIDataLoader : MonoBehaviour
{
    public string worldLore;
    public string promptTemplate;
    public NPCDatabase npcDatabase;

    void Awake()
    {
        LoadAIData();
    }

    void LoadAIData()
    {
        string aiPath = Path.Combine(Application.streamingAssetsPath, "AI");

        string lorePath = Path.Combine(aiPath, "world_lore.txt");
        string templatePath = Path.Combine(aiPath, "prompt_template.txt");
        string jsonPath = Path.Combine(aiPath, "npc_profiles.json");

        // 1. Safety Guard: Cek apakah folder AI beneran ada
        if (!Directory.Exists(aiPath))
        {
            Debug.LogError($"[FATAL] Folder 'AI' gak ketemu! Bikin dulu foldernya di path ini biar bisa dibaca: {aiPath}");
            return;
        }

        // 2. Safety Guard: Cek kelengkapan file asset
        if (!File.Exists(lorePath) || !File.Exists(templatePath) || !File.Exists(jsonPath))
        {
            Debug.LogError("[FATAL] Ada file asset AI yang hilang di StreamingAssets/AI! " +
                           "Pastiin file 'world_lore.txt', 'prompt_template.txt', dan 'npc_profiles.json' ada di sana.");
            return;
        }

        try
        {
            worldLore = File.ReadAllText(lorePath);
            promptTemplate = File.ReadAllText(templatePath);

            string npcJson = File.ReadAllText(jsonPath);
            npcDatabase = JsonUtility.FromJson<NPCDatabase>(npcJson);

            if (npcDatabase == null || npcDatabase.npcs == null || npcDatabase.npcs.Length == 0)
            {
                Debug.LogError("[FATAL] Gagal membaca data dari npc_profiles.json! Struktur JSON lu mungkin ada yang salah/corrupt.");
            }
            else
            {
                Debug.Log($"[SUCCESS] Berhasil memuat {npcDatabase.npcs.Length} profil NPC dari database.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FATAL] Terjadi error saat membaca file AI: {ex.Message}");
        }
    }

    public NPCProfile GetNPCProfile(string npcId)
    {
        if (npcDatabase == null || npcDatabase.npcs == null) return null;

        foreach (NPCProfile npc in npcDatabase.npcs)
        {
            if (string.Equals(npc.id, npcId, StringComparison.OrdinalIgnoreCase))
                return npc;
        }

        return null;
    }

    public string BuildPrompt(string npcId, string playerMessage, string playerEvidence, string chatHistory)
    {
        NPCProfile npc = GetNPCProfile(npcId);

        if (npc == null)
        {
            return "NPC profile tidak ditemukan untuk id: " + npcId;
        }

        // 3. Konversi array dari JSON jadi teks poin-poin biar AI cilik gak gampang pusing
        string publicKnowledgeText = npc.public_knowledge != null && npc.public_knowledge.Length > 0 
            ? "- " + string.Join("\n- ", npc.public_knowledge) 
            : "Tidak ada.";

        string privateKnowledgeText = npc.private_knowledge != null && npc.private_knowledge.Length > 0 
            ? "- " + string.Join("\n- ", npc.private_knowledge) 
            : "Tidak ada rahasia.";

        string forbiddenText = npc.forbidden != null && npc.forbidden.Length > 0 
            ? "- " + string.Join("\n- ", npc.forbidden) 
            : "Tidak ada larangan khusus.";

        // 4. Susun profil deskriptif super rapi & to-the-point
        string formattedProfile = $"NAMA: {npc.name}\n" +
                                  $"PERAN/PEKERJAAN: {npc.role}\n" +
                                  $"UMUR: {npc.age} tahun\n" +
                                  $"KEPRIBADIAN (PERSONALITY): {npc.personality}\n" +
                                  $"GAYA BICARA (SPEECH STYLE): {npc.speech_style}\n" +
                                  $"INFORMASI UMUM (PUBLIC KNOWLEDGE):\n{publicKnowledgeText}\n" +
                                  $"RAHASIA PRIBADI (PRIVATE KNOWLEDGE):\n{privateKnowledgeText}\n" +
                                  $"SANGKALAN/CARA BERBOHONG (LIE BEHAVIOR): {npc.lie_behavior}\n" +
                                  $"HAL YANG DIHARAMKAN (FORBIDDEN):\n{forbiddenText}";

        // 5. Inject semua variabel ke prompt template
        string prompt = promptTemplate;
        prompt = prompt.Replace("{WORLD_LORE}", worldLore);
        prompt = prompt.Replace("{NPC_PROFILE}", formattedProfile);
        prompt = prompt.Replace("{PLAYER_EVIDENCE}", playerEvidence);
        prompt = prompt.Replace("{CHAT_HISTORY}", string.IsNullOrWhiteSpace(chatHistory) ? "Belum ada riwayat percakapan." : chatHistory);
        prompt = prompt.Replace("{PLAYER_MESSAGE}", playerMessage);

        return prompt;
    }
}

[Serializable]
public class NPCDatabase
{
    public NPCProfile[] npcs;
}

[Serializable]
public class NPCProfile
{
    public string id;
    public string name;
    public string role;
    public int age;
    public string status;
    public string personality;
    public string speech_style;
    public string[] public_knowledge;
    public string[] private_knowledge;
    public string lie_behavior;
    public string[] forbidden;
}