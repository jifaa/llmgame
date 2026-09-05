#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class SceneExitButtonTool
{
    [MenuItem("Tools/LLM Game/Add Outdoor Exit Button to UI Canvas", false, 30)]
    public static void CreateExitButtonInCurrentScene()
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

        // Hapus duplikat tombol lama yang mungkin kosong atau rusak
        for (int i = canvas.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = canvas.transform.GetChild(i);
            if (child.name == "Btn_Keluar" || child.name == "Btn_KeluarOutdoor")
            {
                Object.DestroyImmediate(child.gameObject);
            }
        }

        // 1. Buat Container Button di Bawah Layar
        GameObject btnGo = new GameObject("Btn_KeluarOutdoor");
        btnGo.transform.SetParent(canvas.transform, false);

        RectTransform rect = btnGo.AddComponent<RectTransform>();
        // Posisi di Bawah Tengah (Bottom-Center)
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0, 30f); // 30px di atas batas bawah
        rect.sizeDelta = new Vector2(260f, 48f);

        // 2. CanvasGroup agar transisi auto-hide halus
        CanvasGroup cg = btnGo.AddComponent<CanvasGroup>();
        cg.alpha = 1f;

        // 3. Background Image (Dark Noir Slate)
        Image img = btnGo.AddComponent<Image>();
        img.color = new Color(0.12f, 0.15f, 0.20f, 0.95f);

        // 4. Button Component
        Button btn = btnGo.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
        cb.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        btn.colors = cb;

        // Hover Effect
        btnGo.AddComponent<ButtonHoverEffect>();

        // 5. TextMeshPro Label
        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(btnGo.transform, false);

        RectTransform textRect = textGo.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        TMP_Text tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = "<b>KELUAR KE TKP LUAR</b>";
        tmp.fontSize = 15;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.95f, 0.85f, 0.45f, 1f); // Gold Noir Accent

        // 6. SceneTeleporter Script
        SceneTeleporter teleporter = btnGo.AddComponent<SceneTeleporter>();
        teleporter.targetSceneName = "OutdoorsScene";
        teleporter.uiButtonRoot = btnGo;
        teleporter.hideWhenInteracting = true;

        // Hook Button OnClick
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, teleporter.ChangeScene);

        // Mark dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Undo.RegisterCreatedObjectUndo(btnGo, "Create Outdoor Exit Button");
        Selection.activeGameObject = btnGo;

        Debug.Log("[SUCCESS] Tombol 'Btn_KeluarOutdoor' berhasil dibuat di bagian bawah layar!");
    }
}
#endif
