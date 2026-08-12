using UnityEngine;
using System.Collections.Generic;

public class EvidenceManager : MonoBehaviour
{
    public AIChatClient aiChatClient;

    private List<string> collectedEvidence = new List<string>();

    public void CollectEvidence(EvidenceItem evidence)
    {
        if (evidence == null) return;
        if (evidence.collected) return;

        evidence.collected = true;

        string evidenceText = "- " + evidence.evidenceName + ": " + evidence.evidenceDescription;
        collectedEvidence.Add(evidenceText);

        UpdateAIPlayerEvidence();

        Debug.Log("Evidence collected: " + evidence.evidenceName);
    }

    void UpdateAIPlayerEvidence()
    {
        if (aiChatClient == null) return;

        if (collectedEvidence.Count == 0)
        {
            aiChatClient.playerEvidence = "Belum ada bukti.";
            return;
        }

        aiChatClient.playerEvidence = string.Join("\n", collectedEvidence);
    }
}