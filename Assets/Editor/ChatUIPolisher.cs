#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class ChatUIPolisher
{
    [MenuItem("Tools/LLM Game/Polish & Rebuild Interrogation Chat UI", false, 10)]
    public static void PolishChatUIInCurrentScene()
    {
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGo = new GameObject("Canvas");
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();
        }

        // 1. Setup Canvas Scaler
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // 2. Setup EventSystem
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject esGo = new GameObject("EventSystem");
            esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // 3. Cari ChatUI
        ChatUI chatUI = canvas.GetComponent<ChatUI>();
        if (chatUI == null) chatUI = canvas.gameObject.AddComponent<ChatUI>();

        // Cari AIChatClient
        AIChatClient client = Object.FindAnyObjectByType<AIChatClient>();
        if (client != null) chatUI.aiChatClient = client;

        // 4. Hapus ChatPanel lama agar bersih
        for (int i = canvas.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = canvas.transform.GetChild(i);
            if (child.name == "ChatPanel" || child.name == "InterrogationPanel" || child.name == "Chat_Panel")
            {
                Object.DestroyImmediate(child.gameObject);
            }
        }

        // ==================== A. MAIN CHAT PANEL (FULL WIDTH BOTTOM DOCK) ====================
        GameObject panelGo = new GameObject("ChatPanel");
        panelGo.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = panelGo.AddComponent<RectTransform>();
        // Mentok Kiri Kanan di Bawah Layar (Stretch Full-Width)
        panelRect.anchorMin = new Vector2(0f, 0f);
        panelRect.anchorMax = new Vector2(1f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = Vector2.zero; // Menempel pas di bawah layar
        panelRect.sizeDelta = new Vector2(0f, 370f); // Tinggi 370px (sangat lega)

        // Background Slate Noir
        Image panelBg = panelGo.AddComponent<Image>();
        panelBg.color = new Color(0.04f, 0.06f, 0.10f, 0.98f); // Deep Noir Glass

        // CanvasGroup untuk smooth fade
        CanvasGroup cg = panelGo.AddComponent<CanvasGroup>();
        cg.alpha = 1f;

        // Top Gold Accent Line (Garis Emas Detektif di atas panel)
        GameObject accentLine = new GameObject("TopAccentLine");
        accentLine.transform.SetParent(panelGo.transform, false);
        RectTransform accentRect = accentLine.AddComponent<RectTransform>();
        accentRect.anchorMin = new Vector2(0f, 1f);
        accentRect.anchorMax = new Vector2(1f, 1f);
        accentRect.pivot = new Vector2(0.5f, 1f);
        accentRect.anchoredPosition = Vector2.zero;
        accentRect.sizeDelta = new Vector2(0, 5f); // Tebal 5px
        Image accentImg = accentLine.AddComponent<Image>();
        accentImg.color = new Color(0.94f, 0.72f, 0.22f, 1.0f); // Bright Gold #F0B838

        // ==================== B. HEADER BAR (BADGE & INFO) ====================
        GameObject headerGo = new GameObject("HeaderBar");
        headerGo.transform.SetParent(panelGo.transform, false);
        RectTransform headerRect = headerGo.AddComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0f, 1f);
        headerRect.anchorMax = new Vector2(1f, 1f);
        headerRect.pivot = new Vector2(0.5f, 1f);
        headerRect.anchoredPosition = new Vector2(0, -14f);
        headerRect.sizeDelta = new Vector2(-80f, 44f); // 40px margin kiri kanan

        // Header Text / Name Badge
        GameObject nameTextGo = new GameObject("NPC_Name_Text");
        nameTextGo.transform.SetParent(headerGo.transform, false);
        RectTransform nameRect = nameTextGo.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0f, 0f);
        nameRect.anchorMax = new Vector2(0.7f, 1f);
        nameRect.pivot = new Vector2(0f, 0.5f);
        nameRect.anchoredPosition = Vector2.zero;
        nameRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI nameTMP = nameTextGo.AddComponent<TextMeshProUGUI>();
        nameTMP.text = "<color=#F0B838><b>● RUANG INTEROGASI</b></color>  <color=#8FA0B5>│</color>  <color=#FFFFFF><b>Tersangka</b></color>";
        nameTMP.fontSize = 24; // Font Header Besar
        nameTMP.alignment = TextAlignmentOptions.MidlineLeft;
        nameTMP.color = Color.white;

        // Header Shortcuts (ESC / ENTER)
        GameObject hintGo = new GameObject("Hint_Text");
        hintGo.transform.SetParent(headerGo.transform, false);
        RectTransform hintRect = hintGo.AddComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0.45f, 0f);
        hintRect.anchorMax = new Vector2(1f, 1f);
        hintRect.pivot = new Vector2(1f, 0.5f);
        hintRect.anchoredPosition = Vector2.zero;
        hintRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI hintTMP = hintGo.AddComponent<TextMeshProUGUI>();
        hintTMP.text = "<color=#9BB0C7>[ENTER]</color> Kirim  <color=#5A6B80>│</color>  <color=#9BB0C7>[ESC]</color> Tutup Interogasi";
        hintTMP.fontSize = 20; // Shortcut text besar
        hintTMP.alignment = TextAlignmentOptions.MidlineRight;
        hintTMP.color = new Color(0.75f, 0.85f, 0.95f);

        // ==================== C. DIALOGUE SPEECH AREA ====================
        GameObject dialogGo = new GameObject("Dialog_Container");
        dialogGo.transform.SetParent(panelGo.transform, false);
        RectTransform dialogRect = dialogGo.AddComponent<RectTransform>();
        dialogRect.anchorMin = new Vector2(0f, 0.28f);
        dialogRect.anchorMax = new Vector2(1f, 1f);
        dialogRect.pivot = new Vector2(0.5f, 0.5f);
        dialogRect.anchoredPosition = new Vector2(0, -32f);
        dialogRect.sizeDelta = new Vector2(-80f, -68f); // 40px margin kiri kanan

        // Inner speech background (Subtle dark card)
        Image dialogBg = dialogGo.AddComponent<Image>();
        dialogBg.color = new Color(0.07f, 0.10f, 0.15f, 0.90f);

        // Chat Text Mesh
        GameObject chatTextGo = new GameObject("ChatText");
        chatTextGo.transform.SetParent(dialogGo.transform, false);
        RectTransform chatTextRect = chatTextGo.AddComponent<RectTransform>();
        chatTextRect.anchorMin = Vector2.zero;
        chatTextRect.anchorMax = Vector2.one;
        chatTextRect.pivot = new Vector2(0.5f, 0.5f);
        chatTextRect.anchoredPosition = Vector2.zero;
        chatTextRect.sizeDelta = new Vector2(-40f, -28f); // 20px padding

        TextMeshProUGUI chatTMP = chatTextGo.AddComponent<TextMeshProUGUI>();
        chatTMP.text = "<color=#F0B838><b>[Nama Tersangka]</b></color>\nAda apa? Ada yang ingin kamu tanyakan padaku?";
        chatTMP.fontSize = 28; // Teks Dialog Sangat Besar & Jelas (28px)
        chatTMP.lineSpacing = 22f;
        chatTMP.color = new Color(0.96f, 0.98f, 1.0f);
        chatTMP.alignment = TextAlignmentOptions.TopLeft;

        // ==================== D. INPUT BAR (BOTTOM ROW) ====================
        GameObject inputBarGo = new GameObject("InputBar");
        inputBarGo.transform.SetParent(panelGo.transform, false);
        RectTransform inputBarRect = inputBarGo.AddComponent<RectTransform>();
        inputBarRect.anchorMin = new Vector2(0f, 0f);
        inputBarRect.anchorMax = new Vector2(1f, 0.25f);
        inputBarRect.pivot = new Vector2(0.5f, 0f);
        inputBarRect.anchoredPosition = new Vector2(0, 18f);
        inputBarRect.sizeDelta = new Vector2(-80f, 0); // 40px margin kiri kanan

        // 1. Input Field Capsule
        GameObject inputFieldGo = new GameObject("InputField");
        inputFieldGo.transform.SetParent(inputBarGo.transform, false);
        RectTransform ifRect = inputFieldGo.AddComponent<RectTransform>();
        ifRect.anchorMin = new Vector2(0f, 0f);
        ifRect.anchorMax = new Vector2(1f, 1f);
        ifRect.pivot = new Vector2(0f, 0.5f);
        ifRect.anchoredPosition = Vector2.zero;
        ifRect.sizeDelta = new Vector2(-190f, 0); // Sisakan 190px untuk tombol Kirim

        Image ifBg = inputFieldGo.AddComponent<Image>();
        ifBg.color = new Color(0.10f, 0.14f, 0.21f, 1.0f); // Input Background

        // Text Area
        GameObject textAreaGo = new GameObject("Text Area");
        textAreaGo.transform.SetParent(inputFieldGo.transform, false);
        RectTransform taRect = textAreaGo.AddComponent<RectTransform>();
        taRect.anchorMin = Vector2.zero;
        taRect.anchorMax = Vector2.one;
        taRect.sizeDelta = new Vector2(-36f, -14f);

        // Placeholder
        GameObject phGo = new GameObject("Placeholder");
        phGo.transform.SetParent(textAreaGo.transform, false);
        RectTransform phRect = phGo.AddComponent<RectTransform>();
        phRect.anchorMin = Vector2.zero;
        phRect.anchorMax = Vector2.one;
        phRect.sizeDelta = Vector2.zero;
        TextMeshProUGUI phTMP = phGo.AddComponent<TextMeshProUGUI>();
        phTMP.text = "Tanyakan alibi, hubungan korban, keberadaan, atau bukti... (Tekan Enter)";
        phTMP.fontSize = 24; // Placeholder Besar
        phTMP.fontStyle = FontStyles.Italic;
        phTMP.color = new Color(0.55f, 0.64f, 0.75f);
        phTMP.alignment = TextAlignmentOptions.MidlineLeft;

        // Input Main Text
        GameObject mainTextGo = new GameObject("Text");
        mainTextGo.transform.SetParent(textAreaGo.transform, false);
        RectTransform mtRect = mainTextGo.AddComponent<RectTransform>();
        mtRect.anchorMin = Vector2.zero;
        mtRect.anchorMax = Vector2.one;
        mtRect.sizeDelta = Vector2.zero;
        TextMeshProUGUI mtTMP = mainTextGo.AddComponent<TextMeshProUGUI>();
        mtTMP.fontSize = 26; // Input Text Besar (26px)
        mtTMP.color = Color.white;
        mtTMP.alignment = TextAlignmentOptions.MidlineLeft;

        TMP_InputField tmpInputField = inputFieldGo.AddComponent<TMP_InputField>();
        tmpInputField.textViewport = taRect;
        tmpInputField.textComponent = mtTMP;
        tmpInputField.placeholder = phTMP;
        tmpInputField.fontAsset = mtTMP.font;

        // 2. Send Button (Tombol Kirim Emas/Noir)
        GameObject sendBtnGo = new GameObject("SendButton");
        sendBtnGo.transform.SetParent(inputBarGo.transform, false);
        RectTransform sendRect = sendBtnGo.AddComponent<RectTransform>();
        sendRect.anchorMin = new Vector2(1f, 0f);
        sendRect.anchorMax = new Vector2(1f, 1f);
        sendRect.pivot = new Vector2(1f, 0.5f);
        sendRect.anchoredPosition = Vector2.zero;
        sendRect.sizeDelta = new Vector2(180f, 0); // Tombol lebar 180px

        Image sendImg = sendBtnGo.AddComponent<Image>();
        sendImg.color = new Color(0.94f, 0.72f, 0.22f, 1.0f); // Bright Gold

        Button sendBtn = sendBtnGo.AddComponent<Button>();
        ColorBlock scb = sendBtn.colors;
        scb.normalColor = Color.white;
        scb.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
        scb.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        sendBtn.colors = scb;

        sendBtnGo.AddComponent<ButtonHoverEffect>();

        GameObject sendTextGo = new GameObject("Text");
        sendTextGo.transform.SetParent(sendBtnGo.transform, false);
        RectTransform sendTextRect = sendTextGo.AddComponent<RectTransform>();
        sendTextRect.anchorMin = Vector2.zero;
        sendTextRect.anchorMax = Vector2.one;
        sendTextRect.sizeDelta = Vector2.zero;
        TextMeshProUGUI sendTMP = sendTextGo.AddComponent<TextMeshProUGUI>();
        sendTMP.text = "<b>KIRIM</b>";
        sendTMP.fontSize = 22; // Tombol font besar 22px
        sendTMP.color = new Color(0.06f, 0.08f, 0.12f); // Dark text on gold button
        sendTMP.alignment = TextAlignmentOptions.Center;

        // ==================== E. WIRE CHATUI REFERENCES ====================
        chatUI.chatPanel = panelGo;
        chatUI.npcNameText = nameTMP;
        chatUI.chatText = chatTMP;
        chatUI.inputField = tmpInputField;
        chatUI.typeSpeed = 0.02f;

        UnityEditor.Events.UnityEventTools.AddPersistentListener(sendBtn.onClick, chatUI.SendMessageToNPC);

        // Hide panel by default
        panelGo.SetActive(false);

        // Mark Dirty & Select
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Undo.RegisterCreatedObjectUndo(panelGo, "Rebuild Polished Chat UI");
        Selection.activeGameObject = panelGo;

        Debug.Log("[SUCCESS] ChatPanel berhasil di-polish dengan desain Noir Interogasi modern!");
    }
}
#endif
