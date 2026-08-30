using UnityEngine;
using UnityEngine.EventSystems;

public class InterrogationCameraController : MonoBehaviour
{
    [Header("Target Kamera")]
    public Transform targetCamera;

    [Header("Pengaturan Fokus")]
    public float distanceFromNPC = 1.35f;
    public float lookHeight = 1.15f;
    public float transitionSpeed = 4.5f;

    [Header("Reset")]
    public KeyCode resetKey = KeyCode.Escape;
    public bool rightClickToReset = true;

    [Header("UI Chat")]
    public ChatUI chatUI;
    public GameObject chatPanel;

    private Vector3 defaultPosition;
    private Quaternion defaultRotation;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private Transform currentFocusedNPC;

    void Awake()
    {
        // Auto-find jika belum di-assign di Inspector
        if (chatUI == null)
            chatUI = Object.FindAnyObjectByType<ChatUI>(FindObjectsInactive.Include);

        if (chatUI == null)
        {
            Canvas cv = Object.FindAnyObjectByType<Canvas>(FindObjectsInactive.Include);
            if (cv != null)
                chatUI = cv.gameObject.AddComponent<ChatUI>();
            else
            {
                GameObject cp = GameObject.Find("ChatPanel");
                if (cp != null) chatUI = cp.AddComponent<ChatUI>();
            }
        }

        if (chatPanel == null && chatUI != null)
            chatPanel = chatUI.chatPanel;

        if (chatPanel == null)
        {
            GameObject found = GameObject.Find("ChatPanel");
            if (found == null) found = GameObject.Find("Chat_Panel");
            if (found == null) found = GameObject.Find("Panel");
            if (found != null) chatPanel = found;
        }

        // Paksa sembunyikan saat game mulai
        SetPanelActive(false);
    }

    void Start()
    {
        if (targetCamera == null)
        {
            Camera cam = GetComponent<Camera>();
            targetCamera = cam != null ? cam.transform : (Camera.main != null ? Camera.main.transform : Object.FindAnyObjectByType<Camera>()?.transform);
        }

        if (targetCamera != null)
        {
            defaultPosition = targetCamera.position;
            defaultRotation = targetCamera.rotation;
            targetPosition = defaultPosition;
            targetRotation = defaultRotation;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SetPanelActive(false);
    }

    void Update()
    {
        if (targetCamera == null) return;

        HandleClick();
        HandleReset();

        targetCamera.position = Vector3.Lerp(targetCamera.position, targetPosition, Time.deltaTime * transitionSpeed);
        targetCamera.rotation = Quaternion.Slerp(targetCamera.rotation, targetRotation, Time.deltaTime * transitionSpeed);
    }

    private void HandleClick()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        Camera cam = targetCamera != null ? targetCamera.GetComponent<Camera>() : Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 50f))
        {
            Transform npc = hit.transform;
            Transform rootChar = GetCharacterRoot(npc);

            // Cari NPCBrainTest di seluruh hierarki karakter yang diklik
            NPCBrainTest brain = hit.collider.GetComponentInParent<NPCBrainTest>();
            if (brain == null) brain = hit.transform.GetComponent<NPCBrainTest>();
            if (brain == null) brain = rootChar.GetComponent<NPCBrainTest>();
            if (brain == null) brain = rootChar.GetComponentInChildren<NPCBrainTest>();
            if (brain == null) brain = rootChar.GetComponentInParent<NPCBrainTest>();

            if (brain != null || IsCharacter(npc) || IsCharacter(rootChar))
            {
                Transform focusTarget = (brain != null) ? brain.transform : rootChar;
                FocusOnNPC(focusTarget);

                // Munculkan panel
                SetPanelActive(true);

                if (chatUI != null)
                {
                    if (brain == null)
                    {
                        brain = (rootChar != null ? rootChar.gameObject : npc.gameObject).AddComponent<NPCBrainTest>();
                    }

                    if (string.IsNullOrEmpty(brain.npcName))
                    {
                        string charName = (rootChar != null ? rootChar.name : npc.name).ToLower();
                        if (charName.Contains("normal-man-a") || charName.Contains("bima")) { brain.npcName = "Bima Santoso"; brain.npcId = "bima"; }
                        else if (charName.Contains("normal-man-b") || charName.Contains("ardi") || charName.Contains("maya")) { brain.npcName = "Ardi Adrian"; brain.npcId = "ardi"; }
                        else if (charName.Contains("normal-man-c") || charName.Contains("dito")) { brain.npcName = "Dito Pradana"; brain.npcId = "dito"; }
                        else { brain.npcName = rootChar != null ? rootChar.name : npc.name; brain.npcId = brain.npcName.ToLower(); }
                    }

                    chatUI.OpenChat(brain);
                }
            }
        }
    }

    private void HandleReset()
    {
        if (Input.GetKeyDown(resetKey) || (rightClickToReset && Input.GetMouseButtonDown(1)))
        {
            ResetToOverview();
        }
    }

    public void FocusOnNPC(Transform npc)
    {
        if (npc == null) return;
        currentFocusedNPC = npc;

        Vector3 lookTarget = npc.position + Vector3.up * lookHeight;
        targetPosition = lookTarget + (npc.forward * distanceFromNPC);

        Vector3 lookDir = lookTarget - targetPosition;
        if (lookDir.sqrMagnitude > 0.001f)
            targetRotation = Quaternion.LookRotation(lookDir);
    }

    public void ResetToOverview()
    {
        currentFocusedNPC = null;
        targetPosition = defaultPosition;
        targetRotation = defaultRotation;

        SetPanelActive(false);

        if (chatUI != null && chatUI.IsOpen())
            chatUI.CloseChat();
    }

    private void SetPanelActive(bool active)
    {
        if (chatPanel != null)
            chatPanel.SetActive(active);

        if (chatUI != null && chatUI.chatPanel != null && chatUI.chatPanel != chatPanel)
            chatUI.chatPanel.SetActive(active);
    }

    private bool IsCharacter(Transform t)
    {
        if (t == null) return false;
        string n = t.name.ToLower();
        return n.Contains("normal-man") || n.Contains("npc") || n.Contains("male") || n.Contains("female") || t.GetComponent<NPCBrainTest>() != null || t.GetComponentInParent<NPCBrainTest>() != null;
    }

    private Transform GetCharacterRoot(Transform t)
    {
        Transform curr = t;
        while (curr.parent != null && !curr.parent.name.StartsWith("---") && !curr.parent.name.Contains("Table_Area"))
        {
            if (IsCharacter(curr.parent))
                curr = curr.parent;
            else
                break;
        }
        return curr;
    }
}
