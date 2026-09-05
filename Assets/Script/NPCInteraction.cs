using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    public float interactDistance = 3f;
    public ChatUI chatUI;

    public Transform playerBody;
    public Transform cameraTransform;
    public FirstPerson playerController;
    public EvidenceManager evidenceManager;

    public float npcLookHeight = 1.5f;

    private Transform lockedNPC;

    void Update()
{
    if (chatUI != null && !chatUI.IsOpen())
    {
        lockedNPC = null;

        if (playerController != null)
            playerController.canControl = true;
    }

    if (chatUI != null && chatUI.IsOpen() && lockedNPC != null)
    {
        FaceNPC(lockedNPC);

        if (playerController != null)
            playerController.canControl = false;

        return;
    }

    if (Input.GetKeyDown(KeyCode.E))
    {
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            EvidenceItem evidence = hit.collider.GetComponentInParent<EvidenceItem>();

            if (evidence != null)
            {
                evidenceManager.CollectEvidence(evidence);
                return;
            }

            NPCBrainTest npc = hit.collider.GetComponentInParent<NPCBrainTest>();

            if (npc != null)
            {
                if (lockedNPC == npc.transform && chatUI != null && chatUI.IsOpen())
                {
                    return;
                }

                lockedNPC = npc.transform;
                FaceNPC(lockedNPC);
                chatUI.OpenChat(npc);
            }
        }
    }
}

    void FaceNPC(Transform npc)
    {
        Vector3 targetPos = npc.position + Vector3.up * npcLookHeight;

        Vector3 flatDirection = targetPos - playerBody.position;
        flatDirection.y = 0;

        if (flatDirection.sqrMagnitude > 0.001f)
        {
            playerBody.rotation = Quaternion.LookRotation(flatDirection);
        }

        Vector3 directionFromCamera = targetPos - cameraTransform.position;
        Vector3 localDirection = playerBody.InverseTransformDirection(directionFromCamera.normalized);

        float pitch = -Mathf.Atan2(localDirection.y, localDirection.z) * Mathf.Rad2Deg;
        pitch = Mathf.Clamp(pitch, -80f, 80f);

        if (playerController != null)
        {
            playerController.SetCameraPitch(pitch);
        }
    }
}