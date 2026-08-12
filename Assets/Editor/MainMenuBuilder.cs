#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEditor.SceneManagement;

public class MainMenuBuilder : EditorWindow
{
    [MenuItem("Tools/LLM Game/Create Main Menu Scene")]
    public static void CreateMainMenuScene()
    {
        GenerateSpritesIfNotExist();

        // 1. Buat Scene Baru
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 2. Setup Camera
        GameObject camGo = new GameObject("Main Camera");
        Camera cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = HexToColor("FFF8EE"); // Warna background cream
        camGo.transform.position = new Vector3(0, 0, -10);
        camGo.tag = "MainCamera";

        // 3. Setup Canvas & EventSystem
        GameObject canvasGo = new GameObject("MainMenuCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGo.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject eventSystemGo = new GameObject("EventSystem");
        eventSystemGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystemGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // 4. Setup Manager Script
        MainMenuManager menuManager = canvasGo.AddComponent<MainMenuManager>();
        menuManager.targetGameScene = "OutdoorsScene";

        // Load Sprites
        Sprite circleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/circle_solid.png");
        Sprite ringSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/circle_ring.png");
        Sprite starSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/sparkle_star.png");
        Sprite roundedSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/rounded_rect.png");
        Sprite searchSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/search_icon.png");

        // --- LAYER 0: BACKGROUND & DECORATIONS ---
        GameObject bgPanel = CreateUIObject("BackgroundPanel", canvasGo.transform);
        StretchFull(bgPanel);
        Image bgImg = bgPanel.AddComponent<Image>();
        bgImg.color = HexToColor("FFF8EE");

        // Top-Right Large Pastel Peach Circle
        GameObject peachCircle = CreateUIObject("Deco_PeachCircle", bgPanel.transform);
        RectTransform rtPeach = peachCircle.GetComponent<RectTransform>();
        rtPeach.anchorMin = new Vector2(1, 1);
        rtPeach.anchorMax = new Vector2(1, 1);
        rtPeach.pivot = new Vector2(0.5f, 0.5f);
        rtPeach.anchoredPosition = new Vector2(-150, 50);
        rtPeach.sizeDelta = new Vector2(850, 850);
        Image imgPeach = peachCircle.AddComponent<Image>();
        imgPeach.sprite = circleSprite;
        imgPeach.color = HexToColor("F8D4BE");

        // Bottom-Left Large Pastel Mint Circle
        GameObject mintCircle = CreateUIObject("Deco_MintCircle", bgPanel.transform);
        RectTransform rtMint = mintCircle.GetComponent<RectTransform>();
        rtMint.anchorMin = new Vector2(0, 0);
        rtMint.anchorMax = new Vector2(0, 0);
        rtMint.pivot = new Vector2(0.5f, 0.5f);
        rtMint.anchoredPosition = new Vector2(150, -50);
        rtMint.sizeDelta = new Vector2(850, 850);
        Image imgMint = mintCircle.AddComponent<Image>();
        imgMint.sprite = circleSprite;
        imgMint.color = HexToColor("D2EBD8");

        // Middle-Left Yellow Outline Ring
        GameObject yellowRing = CreateUIObject("Deco_YellowRing", bgPanel.transform);
        RectTransform rtRing = yellowRing.GetComponent<RectTransform>();
        rtRing.anchorMin = new Vector2(0, 0.5f);
        rtRing.anchorMax = new Vector2(0, 0.5f);
        rtRing.pivot = new Vector2(0.5f, 0.5f);
        rtRing.anchoredPosition = new Vector2(150, 180);
        rtRing.sizeDelta = new Vector2(110, 110);
        Image imgRing = yellowRing.AddComponent<Image>();
        imgRing.sprite = ringSprite;
        imgRing.color = HexToColor("E8BE48");

        // Top-Right Orange Sparkle Star
        GameObject starTop = CreateUIObject("Deco_StarTop", bgPanel.transform);
        RectTransform rtStarTop = starTop.GetComponent<RectTransform>();
        rtStarTop.anchorMin = new Vector2(1, 1);
        rtStarTop.anchorMax = new Vector2(1, 1);
        rtStarTop.pivot = new Vector2(0.5f, 0.5f);
        rtStarTop.anchoredPosition = new Vector2(-280, -280);
        rtStarTop.sizeDelta = new Vector2(48, 48);
        Image imgStarTop = starTop.AddComponent<Image>();
        imgStarTop.sprite = starSprite;
        imgStarTop.color = HexToColor("F2A93B");

        // Bottom-Right Green Sparkle Star
        GameObject starBottom = CreateUIObject("Deco_StarBottom", bgPanel.transform);
        RectTransform rtStarBottom = starBottom.GetComponent<RectTransform>();
        rtStarBottom.anchorMin = new Vector2(1, 0);
        rtStarBottom.anchorMax = new Vector2(1, 0);
        rtStarBottom.pivot = new Vector2(0.5f, 0.5f);
        rtStarBottom.anchoredPosition = new Vector2(-200, 220);
        rtStarBottom.sizeDelta = new Vector2(55, 55);
        Image imgStarBottom = starBottom.AddComponent<Image>();
        imgStarBottom.sprite = starSprite;
        imgStarBottom.color = HexToColor("3D8E74");

        // --- LAYER 1: TOP BAR ---
        GameObject topBar = CreateUIObject("TopBar", canvasGo.transform);
        RectTransform rtTopBar = topBar.GetComponent<RectTransform>();
        rtTopBar.anchorMin = new Vector2(0, 1);
        rtTopBar.anchorMax = new Vector2(1, 1);
        rtTopBar.pivot = new Vector2(0.5f, 1);
        rtTopBar.anchoredPosition = new Vector2(0, -60);
        rtTopBar.sizeDelta = new Vector2(-160, 80);

        // Detective Club Widget (Left)
        GameObject detClub = CreateUIObject("DetectiveClubWidget", topBar.transform);
        RectTransform rtDet = detClub.GetComponent<RectTransform>();
        rtDet.anchorMin = new Vector2(0, 0.5f);
        rtDet.anchorMax = new Vector2(0, 0.5f);
        rtDet.pivot = new Vector2(0, 0.5f);
        rtDet.anchoredPosition = new Vector2(0, 0);
        rtDet.sizeDelta = new Vector2(300, 60);

        // Icon Box
        GameObject iconBox = CreateUIObject("IconBox", detClub.transform);
        RectTransform rtIconBox = iconBox.GetComponent<RectTransform>();
        rtIconBox.anchorMin = new Vector2(0, 0.5f);
        rtIconBox.anchorMax = new Vector2(0, 0.5f);
        rtIconBox.pivot = new Vector2(0, 0.5f);
        rtIconBox.anchoredPosition = new Vector2(0, 0);
        rtIconBox.sizeDelta = new Vector2(54, 54);
        Image imgIconBox = iconBox.AddComponent<Image>();
        imgIconBox.sprite = roundedSprite;
        imgIconBox.color = HexToColor("1D2D2E");

        GameObject searchIcon = CreateUIObject("SearchIcon", iconBox.transform);
        RectTransform rtSearch = searchIcon.GetComponent<RectTransform>();
        rtSearch.anchorMin = new Vector2(0.5f, 0.5f);
        rtSearch.anchorMax = new Vector2(0.5f, 0.5f);
        rtSearch.sizeDelta = new Vector2(28, 28);
        Image imgSearch = searchIcon.AddComponent<Image>();
        imgSearch.sprite = searchSprite;
        imgSearch.color = HexToColor("FFF8EE");

        // Detective Club Text
        GameObject detTextGo = CreateUIObject("DetText", detClub.transform);
        RectTransform rtDetText = detTextGo.GetComponent<RectTransform>();
        rtDetText.anchorMin = new Vector2(0, 0.5f);
        rtDetText.anchorMax = new Vector2(0, 0.5f);
        rtDetText.pivot = new Vector2(0, 0.5f);
        rtDetText.anchoredPosition = new Vector2(68, 0);
        rtDetText.sizeDelta = new Vector2(250, 50);
        TMP_Text tmpDet = detTextGo.AddComponent<TextMeshProUGUI>();
        tmpDet.text = "<size=11><color=#8E9B9C><b>DETECTIVE CLUB</b></color></size>\n<size=16><color=#1D2D2E><b>Temukan petunjuknya!</b></color></size>";
        tmpDet.lineSpacing = 10;

        // Language Button (Right)
        GameObject langBtnGo = CreateUIObject("LanguageButton", topBar.transform);
        RectTransform rtLang = langBtnGo.GetComponent<RectTransform>();
        rtLang.anchorMin = new Vector2(1, 0.5f);
        rtLang.anchorMax = new Vector2(1, 0.5f);
        rtLang.pivot = new Vector2(1, 0.5f);
        rtLang.anchoredPosition = new Vector2(0, 0);
        rtLang.sizeDelta = new Vector2(130, 42);
        Image imgLang = langBtnGo.AddComponent<Image>();
        imgLang.sprite = roundedSprite;
        imgLang.color = HexToColor("F4EFE6");
        Button btnLang = langBtnGo.AddComponent<Button>();
        langBtnGo.AddComponent<ButtonHoverEffect>();

        GameObject langTextGo = CreateUIObject("LangText", langBtnGo.transform);
        StretchFull(langTextGo);
        TMP_Text tmpLang = langTextGo.AddComponent<TextMeshProUGUI>();
        tmpLang.text = "<b>BAHASA ID</b>";
        tmpLang.fontSize = 12;
        tmpLang.alignment = TextAlignmentOptions.Center;
        tmpLang.color = HexToColor("5A6667");
        menuManager.languageButton = btnLang;
        menuManager.languageText = tmpLang;

        // --- LAYER 2: CENTER CONTENT (TITLE, SUBTITLE, BUTTON) ---
        GameObject centerContainer = CreateUIObject("CenterContent", canvasGo.transform);
        RectTransform rtCenter = centerContainer.GetComponent<RectTransform>();
        rtCenter.anchorMin = new Vector2(0.5f, 0.5f);
        rtCenter.anchorMax = new Vector2(0.5f, 0.5f);
        rtCenter.pivot = new Vector2(0.5f, 0.5f);
        rtCenter.anchoredPosition = new Vector2(0, 30);
        rtCenter.sizeDelta = new Vector2(1100, 500);

        // Big Title
        GameObject titleGo = CreateUIObject("GameTitle", centerContainer.transform);
        RectTransform rtTitle = titleGo.GetComponent<RectTransform>();
        rtTitle.anchorMin = new Vector2(0.5f, 1);
        rtTitle.anchorMax = new Vector2(0.5f, 1);
        rtTitle.pivot = new Vector2(0.5f, 1);
        rtTitle.anchoredPosition = new Vector2(0, 0);
        rtTitle.sizeDelta = new Vector2(1000, 160);
        TMP_Text tmpTitle = titleGo.AddComponent<TextMeshProUGUI>();
        tmpTitle.text = "<b><color=#1E3A3C>GANG</color> <color=#DE674B>MELATI</color></b>";
        tmpTitle.fontSize = 110;
        tmpTitle.alignment = TextAlignmentOptions.Center;
        tmpTitle.characterSpacing = 2f;
        menuManager.titleText = tmpTitle;

        // Subtitle
        GameObject subGo = CreateUIObject("Subtitle", centerContainer.transform);
        RectTransform rtSub = subGo.GetComponent<RectTransform>();
        rtSub.anchorMin = new Vector2(0.5f, 1);
        rtSub.anchorMax = new Vector2(0.5f, 1);
        rtSub.pivot = new Vector2(0.5f, 1);
        rtSub.anchoredPosition = new Vector2(0, -170);
        rtSub.sizeDelta = new Vector2(700, 80);
        TMP_Text tmpSub = subGo.AddComponent<TextMeshProUGUI>();
        tmpSub.text = "Ada cerita kecil yang menunggu untuk kamu\npecahkan. Yuk, mulai petualangannya!";
        tmpSub.fontSize = 21;
        tmpSub.alignment = TextAlignmentOptions.Center;
        tmpSub.color = HexToColor("526263");
        tmpSub.lineSpacing = 15f;
        menuManager.subtitleText = tmpSub;

        // Start Button ("MULAI ->")
        GameObject startBtnGo = CreateUIObject("StartButton", centerContainer.transform);
        RectTransform rtStart = startBtnGo.GetComponent<RectTransform>();
        rtStart.anchorMin = new Vector2(0.5f, 0);
        rtStart.anchorMax = new Vector2(0.5f, 0);
        rtStart.pivot = new Vector2(0.5f, 0);
        rtStart.anchoredPosition = new Vector2(0, 70);
        rtStart.sizeDelta = new Vector2(270, 74);

        // Shadow Button Layer
        GameObject btnShadow = CreateUIObject("Shadow", startBtnGo.transform);
        RectTransform rtShadow = btnShadow.GetComponent<RectTransform>();
        rtShadow.anchorMin = Vector2.zero;
        rtShadow.anchorMax = Vector2.one;
        rtShadow.anchoredPosition = new Vector2(0, -6);
        rtShadow.sizeDelta = Vector2.zero;
        Image imgShadow = btnShadow.AddComponent<Image>();
        imgShadow.sprite = roundedSprite;
        imgShadow.color = HexToColor("B94C33");

        // Main Button Surface
        GameObject btnSurface = CreateUIObject("Surface", startBtnGo.transform);
        StretchFull(btnSurface);
        Image imgStart = btnSurface.AddComponent<Image>();
        imgStart.sprite = roundedSprite;
        imgStart.color = HexToColor("DE674B");

        // Mini Search Badge inside Button
        GameObject btnBadge = CreateUIObject("Badge", btnSurface.transform);
        RectTransform rtBadge = btnBadge.GetComponent<RectTransform>();
        rtBadge.anchorMin = new Vector2(0, 0.5f);
        rtBadge.anchorMax = new Vector2(0, 0.5f);
        rtBadge.pivot = new Vector2(0, 0.5f);
        rtBadge.anchoredPosition = new Vector2(20, 0);
        rtBadge.sizeDelta = new Vector2(46, 46);
        Image imgBadge = btnBadge.AddComponent<Image>();
        imgBadge.sprite = circleSprite;
        imgBadge.color = HexToColor("E87C63");

        GameObject badgeIcon = CreateUIObject("Icon", btnBadge.transform);
        RectTransform rtBadgeIcon = badgeIcon.GetComponent<RectTransform>();
        rtBadgeIcon.anchorMin = new Vector2(0.5f, 0.5f);
        rtBadgeIcon.anchorMax = new Vector2(0.5f, 0.5f);
        rtBadgeIcon.sizeDelta = new Vector2(22, 22);
        Image imgBadgeIcon = badgeIcon.AddComponent<Image>();
        imgBadgeIcon.sprite = searchSprite;
        imgBadgeIcon.color = Color.white;

        // Button Text
        GameObject btnTextGo = CreateUIObject("BtnText", btnSurface.transform);
        RectTransform rtBtnText = btnTextGo.GetComponent<RectTransform>();
        rtBtnText.anchorMin = Vector2.zero;
        rtBtnText.anchorMax = Vector2.one;
        rtBtnText.anchoredPosition = new Vector2(25, 0);
        rtBtnText.sizeDelta = Vector2.zero;
        TMP_Text tmpBtn = btnTextGo.AddComponent<TextMeshProUGUI>();
        tmpBtn.text = "<b>MULAI   \u2192</b>";
        tmpBtn.fontSize = 22;
        tmpBtn.characterSpacing = 4f;
        tmpBtn.alignment = TextAlignmentOptions.Center;
        tmpBtn.color = Color.white;

        Button btnStart = startBtnGo.AddComponent<Button>();
        btnStart.targetGraphic = imgStart;
        startBtnGo.AddComponent<ButtonHoverEffect>();
        menuManager.startButton = btnStart;

        // --- LAYER 3: FOOTER INFO ---
        GameObject footerGo = CreateUIObject("FooterInfo", canvasGo.transform);
        RectTransform rtFooter = footerGo.GetComponent<RectTransform>();
        rtFooter.anchorMin = new Vector2(0.5f, 0);
        rtFooter.anchorMax = new Vector2(0.5f, 0);
        rtFooter.pivot = new Vector2(0.5f, 0);
        rtFooter.anchoredPosition = new Vector2(0, 70);
        rtFooter.sizeDelta = new Vector2(800, 100);

        // Tagline Text
        GameObject tagGo = CreateUIObject("Tagline", footerGo.transform);
        RectTransform rtTag = tagGo.GetComponent<RectTransform>();
        rtTag.anchorMin = new Vector2(0.5f, 1);
        rtTag.anchorMax = new Vector2(0.5f, 1);
        rtTag.pivot = new Vector2(0.5f, 1);
        rtTag.anchoredPosition = new Vector2(0, 0);
        rtTag.sizeDelta = new Vector2(800, 30);
        TMP_Text tmpTag = tagGo.AddComponent<TextMeshProUGUI>();
        tmpTag.text = "<b>SANTAI SAJA  \u2014  MAIN DENGAN RITMEMU SENDIRI</b>";
        tmpTag.fontSize = 13;
        tmpTag.characterSpacing = 3f;
        tmpTag.alignment = TextAlignmentOptions.Center;
        tmpTag.color = HexToColor("4A595B");

        // Episode Line Text
        GameObject epGo = CreateUIObject("Episode", footerGo.transform);
        RectTransform rtEp = epGo.GetComponent<RectTransform>();
        rtEp.anchorMin = new Vector2(0.5f, 0);
        rtEp.anchorMax = new Vector2(0.5f, 0);
        rtEp.pivot = new Vector2(0.5f, 0);
        rtEp.anchoredPosition = new Vector2(0, 0);
        rtEp.sizeDelta = new Vector2(800, 30);
        TMP_Text tmpEp = epGo.AddComponent<TextMeshProUGUI>();
        tmpEp.text = "<color=#8B989A>\u2014\u2014\u2014   EPISODE 01  \u00B7  JEJAK YANG HILANG   \u2014\u2014\u2014</color>";
        tmpEp.fontSize = 13;
        tmpEp.characterSpacing = 2f;
        tmpEp.alignment = TextAlignmentOptions.Center;
        menuManager.episodeText = tmpEp;

        // --- LAYER 4: FADE OVERLAY ---
        GameObject fadeOverlayGo = CreateUIObject("FadeOverlay", canvasGo.transform);
        StretchFull(fadeOverlayGo);
        Image imgFade = fadeOverlayGo.AddComponent<Image>();
        imgFade.color = HexToColor("FFF8EE");
        CanvasGroup cg = fadeOverlayGo.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.blocksRaycasts = false;
        menuManager.fadeOverlay = cg;

        // 5. Simpan Scene ke Assets/MainMenuScene.unity
        string scenePath = "Assets/MainMenuScene.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log($"<color=green>[MainMenuBuilder] Berhasil membuat scene Main Menu di: {scenePath}</color>");

        // Daftarkan ke Build Settings jika belum ada
        AddSceneToBuildSettings(scenePath);
        AddSceneToBuildSettings("Assets/OutdoorsScene.unity");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void StretchFull(GameObject go)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    private static Color HexToColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString("#" + hex, out Color color))
            return color;
        return Color.white;
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = EditorBuildSettings.scenes;
        foreach (var s in scenes)
        {
            if (s.path == scenePath) return;
        }

        var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
        for (int i = 0; i < scenes.Length; i++)
            newScenes[i] = scenes[i];
        newScenes[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
        EditorBuildSettings.scenes = newScenes;
    }

    private static void GenerateSpritesIfNotExist()
    {
        string dir = "Assets/Sprites/UI";
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        CreateAndSaveTexture(Path.Combine(dir, "circle_solid.png"), 256, 256, (x, y, w, h) =>
        {
            float cx = w / 2f, cy = h / 2f, r = w / 2f - 2f;
            float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
            float alpha = Mathf.Clamp01(r - dist);
            return new Color(1, 1, 1, alpha);
        });

        CreateAndSaveTexture(Path.Combine(dir, "circle_ring.png"), 256, 256, (x, y, w, h) =>
        {
            float cx = w / 2f, cy = h / 2f, outerR = w / 2f - 4f, innerR = outerR - 28f;
            float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
            float alpha = Mathf.Clamp01(outerR - dist) * Mathf.Clamp01(dist - innerR);
            return new Color(1, 1, 1, alpha);
        });

        CreateAndSaveTexture(Path.Combine(dir, "sparkle_star.png"), 256, 256, (x, y, w, h) =>
        {
            float nx = Mathf.Abs((x - w / 2f) / (w / 2f));
            float ny = Mathf.Abs((y - h / 2f) / (h / 2f));
            // 4-point astroid star shape: x^0.5 + y^0.5 <= 1
            float val = Mathf.Pow(nx, 0.45f) + Mathf.Pow(ny, 0.45f);
            float alpha = Mathf.Clamp01((1f - val) * 8f);
            return new Color(1, 1, 1, alpha);
        });

        CreateAndSaveTexture(Path.Combine(dir, "rounded_rect.png"), 256, 256, (x, y, w, h) =>
        {
            float radius = 50f;
            float dx = Mathf.Max(0, Mathf.Abs(x - w / 2f) - (w / 2f - radius));
            float dy = Mathf.Max(0, Mathf.Abs(y - h / 2f) - (h / 2f - radius));
            float dist = Mathf.Sqrt(dx * dx + dy * dy);
            float alpha = Mathf.Clamp01(radius - dist);
            return new Color(1, 1, 1, alpha);
        });

        CreateAndSaveTexture(Path.Combine(dir, "search_icon.png"), 128, 128, (x, y, w, h) =>
        {
            // Simple crisp magnifying glass
            Vector2 p = new Vector2(x, y);
            Vector2 c = new Vector2(50, 78);
            float rOuter = 34f, rInner = 24f;
            float distC = Vector2.Distance(p, c);
            float circleAlpha = Mathf.Clamp01(rOuter - distC) * Mathf.Clamp01(distC - rInner);

            // Handle: Line from (72, 56) to (105, 23)
            Vector2 hStart = new Vector2(70, 58);
            Vector2 hEnd = new Vector2(104, 24);
            float lineDist = DistanceToSegment(p, hStart, hEnd);
            float handleAlpha = Mathf.Clamp01(5.5f - lineDist);

            float combined = Mathf.Clamp01(circleAlpha + handleAlpha);
            return new Color(1, 1, 1, combined);
        });

        AssetDatabase.Refresh();

        // Konfigurasi Texture Importer ke Sprite UI
        SetAsUISprite(Path.Combine(dir, "circle_solid.png"));
        SetAsUISprite(Path.Combine(dir, "circle_ring.png"));
        SetAsUISprite(Path.Combine(dir, "sparkle_star.png"));
        SetAsUISprite(Path.Combine(dir, "rounded_rect.png"));
        SetAsUISprite(Path.Combine(dir, "search_icon.png"));
    }

    private static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
        return Vector2.Distance(p, a + t * ab);
    }

    private static void CreateAndSaveTexture(string path, int width, int height, System.Func<int, int, int, int, Color> pixelFunc)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                tex.SetPixel(x, y, pixelFunc(x, y, width, height));
            }
        }
        tex.Apply();
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        Object.DestroyImmediate(tex);
    }

    private static void SetAsUISprite(string assetPath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }
    }
}
#endif
