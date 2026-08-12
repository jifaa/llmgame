using UnityEngine;

public class EvidenceItem : MonoBehaviour
{
    public string evidenceId;
    public string evidenceName;

    [TextArea]
    public string evidenceDescription;

    public bool collected = false;
}