#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using TMPro;

/// <summary>
/// OfficeRoomBuilder: Generator Khusus 1 Ruang Interogasi 3 Orang (Fixed Camera Ready).
/// Menghasilkan 1 ruangan interogasi polisi yang rapi, padat dekorasi, dan berwarna penuh (100% HDRP/URP Compatible).
/// </summary>
public class OfficeRoomBuilder : EditorWindow
{
    [MenuItem("Tools/LLM Game/AUTO-FIX & SETUP POLICE INDOOR (ChatUI, NPC Brains & Camera)")]
    public static void AutoFixPoliceIndoor()
    {
        string scenePath = "Assets/PoliceIndoor.unity";
        if (File.Exists(scenePath))
        {
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }

        // 1. Setup / Perbaiki Camera
        Camera mainCam = Camera.main;
        GameObject camGo = mainCam != null ? mainCam.gameObject : GameObject.Find("Main_Interrogation_Camera");
        if (camGo != null)
        {
            var camCtrl = camGo.GetComponent<InterrogationCameraController>();
            if (camCtrl == null) camCtrl = camGo.AddComponent<InterrogationCameraController>();
            camCtrl.targetCamera = camGo.transform;
        }

        // 2. Bersihkan InterrogationCameraController yang salah pasang di tubuh NPC
        foreach (var wrongCtrl in Object.FindObjectsByType<InterrogationCameraController>(FindObjectsInactive.Include))
        {
            if (wrongCtrl.gameObject != camGo)
            {
                DestroyImmediate(wrongCtrl);
            }
        }

        // 3. Setup / Perbaiki NPC Brain & Animasi
        var sitController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animation/Controller/sit.controller");
        
        var allObjs = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include);
        foreach (var t in allObjs)
        {
            string n = t.name.ToLower();
            if (n.Contains("normal-man-a") || (n.Contains("bima") && t.parent != null && t.parent.name.Contains("Table")))
            {
                SetupNPC(t.gameObject, "bima", "Bima Santoso", sitController);
            }
            else if (n.Contains("normal-man-b") || ((n.Contains("ardi") || n.Contains("maya")) && t.parent != null && t.parent.name.Contains("Table")))
            {
                SetupNPC(t.gameObject, "ardi", "Ardi Adrian", sitController);
            }
            else if (n.Contains("normal-man-c") || (n.Contains("dito") && t.parent != null && t.parent.name.Contains("Table")))
            {
                SetupNPC(t.gameObject, "dito", "Dito Pradana", sitController);
            }
        }

        // 4. Setup AIChatClient
        AIChatClient chatClient = Object.FindAnyObjectByType<AIChatClient>();
        if (chatClient == null)
        {
            GameObject clientGo = new GameObject("AIChatClient");
            chatClient = clientGo.AddComponent<AIChatClient>();
        }

        // 5. Setup Canvas & ChatUI
        ChatUI chatUI = Object.FindAnyObjectByType<ChatUI>(FindObjectsInactive.Include);
        if (chatUI == null)
        {
            chatUI = BuildInterrogationChatUI(chatClient);
        }

        // 6. Hubungkan ke Camera Controller
        if (camGo != null)
        {
            var camCtrl = camGo.GetComponent<InterrogationCameraController>();
            if (camCtrl != null)
            {
                camCtrl.chatUI = chatUI;
                if (chatUI != null) camCtrl.chatPanel = chatUI.chatPanel;
            }
        }

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("<color=#00FF88>[SUCCESS] Scene PoliceIndoor berhasil diperbaiki! ChatUI, NPC Brains (Bima, Ardi, Dito), dan Kamera sudah terhubung 100%.</color>");
    }

    [MenuItem("Tools/LLM Game/Build 3-Person Interrogation Room (Single Room)")]
    public static void Open()
    {
        var win = GetWindow<OfficeRoomBuilder>("3-Person Interrogation Room");
        win.minSize = new Vector2(440, 520);
        win.Show();
    }

    [MenuItem("Tools/LLM Game/Quick Build Interrogation Room (test.unity)")]
    public static void QuickBuildTest()
    {
        BuildSingleInterrogationRoom("Assets/test.unity");
    }

    [MenuItem("Tools/LLM Game/Quick Build Interrogation Room (PoliceIndoor.unity)")]
    public static void QuickBuildPoliceIndoor()
    {
        BuildSingleInterrogationRoom("Assets/PoliceIndoor.unity");
    }

    private string scenePath = "Assets/test.unity";
    private bool spawnSuspectNPCs = true;
    private bool setupFixedCamera = true;
    private bool includeLighting = true;

    void OnGUI()
    {
        EditorGUILayout.Space(10);
        GUILayout.Label("3-PERSON INTERROGATION ROOM GENERATOR", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Membangun 1 Ruang Interogasi Sinematik Terfokus (Fixed Camera View):\n\n" +
            "• Meja Interogasi Presisi dengan 3 Kursi Tersangka (Bima, Maya, Dito) & 1 Kursi Detektif\n" +
            "• Seluruh Warna & Tekstur Ter-load 100% (Texture Palette & HDRP Materials Asli)\n" +
            "• Papan Bukti Kasus Pembunuhan Rian Gang Melati Berbingkai dengan Foto & Pin Bukti\n" +
            "• Kaca Pengawas Dua Arah (One-Way Mirror) dengan Ruang Observasi & Lampu Biru\n" +
            "• Rak Arsip Polisi Tinggi Penuh Berkas Kasus Warna-warni di Dinding Kanan\n" +
            "• Meja Samping CCTV: PC Tower, Dual Monitor CCTV, Telepon Polisi, Brankas Bukti\n" +
            "• Kamera Fix Sinematik dengan Komposisi Framing Sempurna.",
            MessageType.Info
        );

        EditorGUILayout.Space(10);
        scenePath = EditorGUILayout.TextField("Target Scene Path", scenePath);

        EditorGUILayout.Space(5);
        GUILayout.Label("Opsi Ruangan:", EditorStyles.boldLabel);
        spawnSuspectNPCs = EditorGUILayout.Toggle("Spawn 3D NPC di Kursi", spawnSuspectNPCs);
        setupFixedCamera = EditorGUILayout.Toggle("Setup Fixed Camera (Sudut Sinematik)", setupFixedCamera);
        includeLighting = EditorGUILayout.Toggle("Pencahayaan Sinematik Noir (Spotlights)", includeLighting);

        EditorGUILayout.Space(15);
        GUI.backgroundColor = new Color(0.15f, 0.75f, 1f);
        if (GUILayout.Button("GENERATE RUANGAN INTEROGASI SEKARANG", GUILayout.Height(48)))
        {
            BuildSingleInterrogationRoom(scenePath, spawnSuspectNPCs, setupFixedCamera, includeLighting);
        }
        GUI.backgroundColor = Color.white;
    }

    public static void BuildSingleInterrogationRoom(
        string targetScenePath,
        bool spawnNPCs = true,
        bool fixedCamera = true,
        bool addLighting = true)
    {
        // 1. Setup Scene
        if (!File.Exists(targetScenePath))
        {
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(newScene, targetScenePath);
        }
        else
        {
            EditorSceneManager.OpenScene(targetScenePath, OpenSceneMode.Single);
        }

        // Hapus objek lama di scene
        var rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var obj in rootObjects)
        {
            Undo.DestroyObjectImmediate(obj);
        }

        // 2. Setup Suasana
        SetupAtmosphere();

        // 3. Buat Material Khusus yang Pasti Berwarna & Kompatibel HDRP/URP
        Material floorMat = CreateSimpleMaterial("Mat_Room_Floor", new Color(0.16f, 0.17f, 0.20f), 0.35f, 0.1f);
        Material wallMat = CreateSimpleMaterial("Mat_Room_Wall", new Color(0.20f, 0.22f, 0.26f), 0.15f, 0.05f);
        Material wallAccentMat = CreateSimpleMaterial("Mat_Room_WallAccent", new Color(0.14f, 0.16f, 0.20f), 0.15f, 0.05f);
        Material ceilingMat = CreateSimpleMaterial("Mat_Room_Ceiling", new Color(0.10f, 0.11f, 0.13f), 0.1f, 0.05f);
        Material baseboardMat = CreateSimpleMaterial("Mat_Room_Baseboard", new Color(0.06f, 0.07f, 0.08f), 0.25f, 0.1f);
        Material darkMetalMat = CreateSimpleMaterial("Mat_Room_DarkMetal", new Color(0.12f, 0.12f, 0.14f), 0.6f, 0.85f);
        Material woodTableMat = CreateSimpleMaterial("Mat_Room_WoodTable", new Color(0.38f, 0.26f, 0.18f), 0.4f, 0.05f);
        Material corkBoardMat = CreateSimpleMaterial("Mat_Room_CorkBoard", new Color(0.26f, 0.20f, 0.14f), 0.1f, 0.0f);
        Material glassOneWayMat = CreateGlassMaterial("Mat_Room_OneWayMirror", new Color(0.06f, 0.12f, 0.18f, 0.85f));
        Material cardWhiteMat = CreateSimpleMaterial("Mat_Card_White", new Color(0.92f, 0.90f, 0.85f), 0.1f);
        Material cardRedMat = CreateSimpleMaterial("Mat_Card_Red", new Color(0.85f, 0.22f, 0.22f), 0.2f);
        Material cardYellowMat = CreateSimpleMaterial("Mat_Card_Yellow", new Color(0.92f, 0.75f, 0.20f), 0.2f);

        // Palette Material untuk seluruh Low Poly Furniture
        Material lowPolyPaletteMat = CreateLowPolyPaletteMaterial();

        // 4. Root Container
        GameObject roomRoot = new GameObject("--- 3-PERSON INTERROGATION ROOM ---");
        Undo.RegisterCreatedObjectUndo(roomRoot, "Create 3-Person Interrogation Room");

        // Dimensi Ruangan Interogasi (9.0m Lebar X × 7.5m Panjang Z × 3.6m Tinggi Y)
        float roomW = 9.0f; // -4.5f ke +4.5f
        float roomL = 7.5f; // -3.75f ke +3.75f
        float roomH = 3.6f;

        // ==================== A. ARSITEKTUR RUANGAN ====================
        GameObject structRoot = new GameObject("Architecture");
        structRoot.transform.SetParent(roomRoot.transform);

        // Lantai Beton Polisi
        CreateBox("Floor", structRoot.transform,
            new Vector3(0, -0.1f, 0), new Vector3(roomW, 0.2f, roomL), floorMat);

        // Plafon Ruangan
        CreateBox("Ceiling", structRoot.transform,
            new Vector3(0, roomH + 0.1f, 0), new Vector3(roomW, 0.2f, roomL), ceilingMat);

        // Dinding Belakang Tersangka (Z = +3.75f)
        CreateBox("Wall_Back", structRoot.transform,
            new Vector3(0, roomH * 0.5f, roomL * 0.5f), new Vector3(roomW, roomH, 0.2f), wallMat);

        // Dinding Kanan (X = +4.5f, Solid Wall)
        CreateBox("Wall_Right", structRoot.transform,
            new Vector3(roomW * 0.5f, roomH * 0.5f, 0), new Vector3(0.2f, roomH, roomL), wallAccentMat);

        // Dinding Depan Pintu Masuk (Z = -3.75f)
        float doorW = 1.3f;
        float doorH = 2.4f;
        float frontSideW = (roomW - doorW) * 0.5f;

        CreateBox("Wall_Front_L", structRoot.transform,
            new Vector3(-(roomW * 0.5f - frontSideW * 0.5f), roomH * 0.5f, -roomL * 0.5f), new Vector3(frontSideW, roomH, 0.2f), wallMat);
        CreateBox("Wall_Front_R", structRoot.transform,
            new Vector3((roomW * 0.5f - frontSideW * 0.5f), roomH * 0.5f, -roomL * 0.5f), new Vector3(frontSideW, roomH, 0.2f), wallMat);
        CreateBox("Wall_Front_Top", structRoot.transform,
            new Vector3(0, doorH + (roomH - doorH) * 0.5f, -roomL * 0.5f), new Vector3(doorW, roomH - doorH, 0.2f), wallMat);

        // Pintu Masuk
        GameObject doorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Low Poly Furniture 2/Doors/Door A.fbx");
        if (doorPrefab != null)
        {
            GameObject door = (GameObject)PrefabUtility.InstantiatePrefab(doorPrefab, structRoot.transform);
            door.name = "Entrance_Door";
            door.transform.position = new Vector3(0, 0, -roomL * 0.5f + 0.05f);
            door.transform.rotation = Quaternion.identity;
            door.transform.localScale = new Vector3(1.15f, 1.15f, 1.15f);
        }

        // Dinding Kiri dengan Kaca Pengawas Dua Arah (One-Way Mirror)
        float winW = 3.6f;
        float winH = 1.5f;
        float winCenterY = 1.8f;
        float winSideZ = (roomL - winW) * 0.5f;

        CreateBox("Wall_Left_Back", structRoot.transform,
            new Vector3(-roomW * 0.5f, roomH * 0.5f, roomL * 0.5f - winSideZ * 0.5f), new Vector3(0.2f, roomH, winSideZ), wallMat);
        CreateBox("Wall_Left_Front", structRoot.transform,
            new Vector3(-roomW * 0.5f, roomH * 0.5f, -roomL * 0.5f + winSideZ * 0.5f), new Vector3(0.2f, roomH, winSideZ), wallMat);
        CreateBox("Wall_Left_Bottom", structRoot.transform,
            new Vector3(-roomW * 0.5f, (winCenterY - winH * 0.5f) * 0.5f, 0), new Vector3(0.2f, winCenterY - winH * 0.5f, winW), wallMat);
        CreateBox("Wall_Left_Top", structRoot.transform,
            new Vector3(-roomW * 0.5f, (winCenterY + winH * 0.5f) + (roomH - (winCenterY + winH * 0.5f)) * 0.5f, 0), new Vector3(0.2f, roomH - (winCenterY + winH * 0.5f), winW), wallMat);

        // Kaca Dua Arah (One-Way Mirror)
        CreateBox("OneWay_Mirror_Glass", structRoot.transform,
            new Vector3(-roomW * 0.5f, winCenterY, 0), new Vector3(0.06f, winH, winW), glassOneWayMat);

        // Frame Kaca Metal Gelap
        CreateBox("OneWay_Mirror_Frame", structRoot.transform,
            new Vector3(-roomW * 0.5f + 0.03f, winCenterY, 0), new Vector3(0.04f, winH + 0.12f, winW + 0.12f), darkMetalMat, false);

        // Baseboards (Plin Lantai)
        CreateBaseboards(structRoot.transform, roomW, roomL, baseboardMat, doorW);

        // Corner Pillars
        float pSize = 0.3f;
        Vector3[] pillarPos = new Vector3[] {
            new Vector3(-roomW * 0.5f + pSize * 0.5f, roomH * 0.5f, -roomL * 0.5f + pSize * 0.5f),
            new Vector3(roomW * 0.5f - pSize * 0.5f, roomH * 0.5f, -roomL * 0.5f + pSize * 0.5f),
            new Vector3(roomW * 0.5f - pSize * 0.5f, roomH * 0.5f, roomL * 0.5f - pSize * 0.5f),
            new Vector3(-roomW * 0.5f + pSize * 0.5f, roomH * 0.5f, roomL * 0.5f - pSize * 0.5f)
        };
        for (int i = 0; i < pillarPos.Length; i++)
        {
            CreateBox($"Corner_Pillar_{i + 1}", structRoot.transform, pillarPos[i], new Vector3(pSize, roomH, pSize), baseboardMat);
        }

        // ==================== B. MEJA INTEROGASI UTAMA (3 KURSI TERSANGKA) ====================
        GameObject mainInterrogateRoot = new GameObject("Main_Interrogation_Table_Area");
        mainInterrogateRoot.transform.SetParent(roomRoot.transform);

        // Meja Interogasi Panjang (Pusat Ruangan di Z = 0.35f)
        Vector3 tableCenter = new Vector3(0, 0, 0.35f);
        float tableW = 3.6f;
        float tableD = 1.15f;
        float tableH = 0.76f;

        // Meja dibuat dari Top Board & Kaki Baja Presisi
        GameObject tableTop = CreateBox("Interrogation_Table_Top", mainInterrogateRoot.transform,
            tableCenter + new Vector3(0, tableH - 0.04f, 0), new Vector3(tableW, 0.08f, tableD), woodTableMat);

        // Kaki Meja Baja
        float legThick = 0.08f;
        Vector3[] legOffsets = new Vector3[] {
            new Vector3(-tableW * 0.5f + 0.15f, tableH * 0.5f - 0.04f, -tableD * 0.5f + 0.15f),
            new Vector3(tableW * 0.5f - 0.15f, tableH * 0.5f - 0.04f, -tableD * 0.5f + 0.15f),
            new Vector3(-tableW * 0.5f + 0.15f, tableH * 0.5f - 0.04f, tableD * 0.5f - 0.15f),
            new Vector3(tableW * 0.5f - 0.15f, tableH * 0.5f - 0.04f, tableD * 0.5f - 0.15f)
        };
        for (int i = 0; i < legOffsets.Length; i++)
        {
            CreateBox($"Table_Leg_{i + 1}", mainInterrogateRoot.transform, tableCenter + legOffsets[i], new Vector3(legThick, tableH - 0.08f, legThick), darkMetalMat);
        }

        // 1. Kursi Detektif (Menghadap Z+ ke arah 3 tersangka)
        Vector3 detChairPos = tableCenter + new Vector3(0, 0, -0.95f);
        LoadAndSpawnOfficeBuiltInProp("Chair", mainInterrogateRoot.transform, detChairPos, Quaternion.identity);

        // Laptop Detektif & Lampu Meja di Depan Detektif
        LoadAndSpawnOfficeBuiltInProp("Laptop", mainInterrogateRoot.transform, tableCenter + new Vector3(-0.35f, tableH, -0.2f), Quaternion.Euler(0, 15, 0));
        GameObject lamp = LoadAndSpawnOfficeBuiltInProp("DeskLamp", mainInterrogateRoot.transform, tableCenter + new Vector3(1.35f, tableH, -0.15f), Quaternion.Euler(0, -145, 0));
        if (lamp != null)
        {
            GameObject lampSpot = new GameObject("DeskLamp_Spot");
            lampSpot.transform.SetParent(lamp.transform);
            lampSpot.transform.localPosition = new Vector3(0.1f, 0.3f, 0);
            lampSpot.transform.localRotation = Quaternion.Euler(30, -90, 0);
            Light dl = lampSpot.AddComponent<Light>();
            dl.type = LightType.Spot;
            dl.color = new Color(1.0f, 0.88f, 0.72f);
            dl.intensity = 3.8f;
            dl.range = 4.0f;
            dl.spotAngle = 55f;
        }

        // 2. TIGA KURSI TERSANGKA (KIRI: BIMA, TENGAH: MAYA, KANAN: DITO - Menghadap Detektif ke Z-)
        float chairSpacingX = 1.15f;
        Vector3 chair1Pos = tableCenter + new Vector3(-chairSpacingX, 0, 0.95f); // BIMA SANTOSO
        Vector3 chair2Pos = tableCenter + new Vector3(0f, 0, 0.95f);             // MAYA KIRANA
        Vector3 chair3Pos = tableCenter + new Vector3(chairSpacingX, 0, 0.95f);  // DITO PRADANA

        LoadAndSpawnOfficeBuiltInProp("Chair", mainInterrogateRoot.transform, chair1Pos, Quaternion.Euler(0, 180, 0));
        LoadAndSpawnOfficeBuiltInProp("Chair", mainInterrogateRoot.transform, chair2Pos, Quaternion.Euler(0, 180, 0));
        LoadAndSpawnOfficeBuiltInProp("Chair", mainInterrogateRoot.transform, chair3Pos, Quaternion.Euler(0, 180, 0));

        // Properti Meja untuk Masing-masing Tersangka:
        // Posisi 1 (Bima - Satpam): Map BAP Kuning, Mug Kopi
        LoadAndSpawnFurnitureProp("Book/Yellow Book", mainInterrogateRoot.transform, tableCenter + new Vector3(-chairSpacingX, tableH, 0.15f), Quaternion.Euler(0, 10, 0), null, lowPolyPaletteMat);
        LoadAndSpawnOfficeBuiltInProp("CoffeeMug", mainInterrogateRoot.transform, tableCenter + new Vector3(-chairSpacingX - 0.35f, tableH, 0.25f), Quaternion.identity);

        // Posisi 2 (Maya - Kafe): Map Rekaman Hitam, Mug Kafe Senja
        LoadAndSpawnFurnitureProp("Book/Black Book", mainInterrogateRoot.transform, tableCenter + new Vector3(0f, tableH, 0.15f), Quaternion.Euler(0, -5, 0), null, lowPolyPaletteMat);
        LoadAndSpawnOfficeBuiltInProp("CoffeeMug", mainInterrogateRoot.transform, tableCenter + new Vector3(0.35f, tableH, 0.25f), Quaternion.identity);

        // Posisi 3 (Dito - Kurir / Tersangka Utama): Map Bukti Biru/Merah, Mug
        LoadAndSpawnFurnitureProp("Book/Blue Book", mainInterrogateRoot.transform, tableCenter + new Vector3(chairSpacingX, tableH, 0.15f), Quaternion.Euler(0, 15, 0), null, lowPolyPaletteMat);
        LoadAndSpawnOfficeBuiltInProp("CoffeeMug", mainInterrogateRoot.transform, tableCenter + new Vector3(chairSpacingX + 0.35f, tableH, 0.25f), Quaternion.identity);

        // 3D MODEL NPC TERSANGKA/SAKSI DUDUK DI KURSI
        if (spawnNPCs)
        {
            SpawnNPC("NPC_Bima_Santoso", "Assets/Character/New Folder/Male_Shirt.fbx", mainInterrogateRoot.transform, chair1Pos + new Vector3(0, 0, 0.05f));
            SpawnNPC("NPC_Maya_Kirana", "Assets/Character/New Folder/Male_Suit.fbx", mainInterrogateRoot.transform, chair2Pos + new Vector3(0, 0, 0.05f));
            SpawnNPC("NPC_Dito_Pradana", "Assets/Character/Male_Casual.fbx", mainInterrogateRoot.transform, chair3Pos + new Vector3(0, 0, 0.05f));
        }

        // ==================== C. DEKORASI DINDING BELAKANG (PAPAN BUKTI KASUS RAPI) ====================
        GameObject backDecorRoot = new GameObject("Decorations_Back_Wall");
        backDecorRoot.transform.SetParent(roomRoot.transform);

        // 1. Papan Bukti Pembunuhan Rian Gang Melati Berbingkai Elegan (Tengah Belakang)
        float boardW = 4.8f;
        float boardH = 1.6f;
        Vector3 boardPos = new Vector3(0, 2.1f, roomL * 0.5f - 0.06f);

        // Frame Papan
        CreateBox("Murder_Board_Frame", backDecorRoot.transform,
            boardPos, new Vector3(boardW + 0.12f, boardH + 0.12f, 0.05f), darkMetalMat, false);

        // Permukaan Papan Corkboard
        GameObject corkBoard = CreateBox("Murder_Board_Surface", backDecorRoot.transform,
            boardPos + new Vector3(0, 0, -0.02f), new Vector3(boardW, boardH, 0.04f), corkBoardMat);

        // Kartu-Kartu Bukti & Foto pada Papan
        // Kartu Saksi 1: Bima (Kuning)
        CreateBox("Card_Bima", corkBoard.transform, boardPos + new Vector3(-1.5f, 0.2f, -0.05f), new Vector3(0.9f, 0.65f, 0.02f), cardYellowMat, false);
        CreateTextSign(corkBoard.transform, boardPos + new Vector3(-1.5f, 0.2f, -0.07f), "<b>SAKSI 01</b>\nBima Santoso\n<i>(Satpam Ruko)</i>", Color.black, 9);

        // Kartu Saksi 2: Maya (Putih)
        CreateBox("Card_Maya", corkBoard.transform, boardPos + new Vector3(0f, 0.2f, -0.05f), new Vector3(0.9f, 0.65f, 0.02f), cardWhiteMat, false);
        CreateTextSign(corkBoard.transform, boardPos + new Vector3(0f, 0.2f, -0.07f), "<b>SAKSI 02</b>\nMaya Kirana\n<i>(Kafe Senja)</i>", Color.black, 9);

        // Kartu Terduga: Dito (Merah)
        CreateBox("Card_Dito", corkBoard.transform, boardPos + new Vector3(1.5f, 0.2f, -0.05f), new Vector3(0.9f, 0.65f, 0.02f), cardRedMat, false);
        CreateTextSign(corkBoard.transform, boardPos + new Vector3(1.5f, 0.2f, -0.07f), "<b>TERDUGA</b>\nDito Pradana\n<i>(Kurir Motor)</i>", Color.white, 9);

        // Header Papan Kasus
        CreateTextSign(corkBoard.transform, boardPos + new Vector3(0, 0.62f, -0.06f), "<b><color=#FFD700>KASUS PEMBUNUHAN: RIAN (GANG MELATI)</color></b>", Color.white, 13);
        // Garis Bukti Bawah
        CreateTextSign(corkBoard.transform, boardPos + new Vector3(0, -0.48f, -0.06f), "[BUKTI]: Sobekan Jaket Hitam  ●  Bekas Ban Motor  ●  Pisau Lipat  ●  Kunci Loker", Color.white, 10);

        // 2. Chart Pengukur Tinggi Badan (Suspect Lineup) di Kanan Papan
        Vector3 lineupPos = new Vector3(3.4f, 1.9f, roomL * 0.5f - 0.06f);
        CreateBox("Lineup_Ruler", backDecorRoot.transform, lineupPos, new Vector3(1.1f, 1.9f, 0.02f), cardWhiteMat);
        CreateTextSign(backDecorRoot.transform, lineupPos + new Vector3(0, 0.8f, -0.02f), "<b>LINEUP</b>", Color.darkGray, 11);
        CreateTextSign(backDecorRoot.transform, lineupPos + new Vector3(0, 0.1f, -0.02f), "- 190 cm -\n- 180 cm -\n- 170 cm -\n- 160 cm -\n- 150 cm -", Color.black, 9);

        // 3. Jam Dinding Polisi di Kiri Papan
        LoadAndSpawnModel("Assets/Low Poly Furniture 2/Miscellaneous/Clock.fbx", backDecorRoot.transform,
            new Vector3(-3.2f, 2.4f, roomL * 0.5f - 0.05f), Quaternion.Euler(0, 180, 0), Vector3.one * 1.3f);

        // ==================== D. DEKORASI DINDING KANAN (RAK ARSIP & CCTV STATION) ====================
        GameObject rightDecorRoot = new GameObject("Decorations_Right_Wall");
        rightDecorRoot.transform.SetParent(roomRoot.transform);

        // 1. Lemari Arsip Berkas Polisi Tinggi
        Vector3 shelfPos = new Vector3(roomW * 0.5f - 0.5f, 0, 1.4f);
        LoadAndSpawnFurnitureProp("Bookshelf/Standart Bookshelf", rightDecorRoot.transform,
            shelfPos, Quaternion.Euler(0, -90, 0), null, lowPolyPaletteMat);

        // Map & Buku Kasus Warna-warni di Rak (Menggunakan Palette Texture Asli)
        LoadAndSpawnFurnitureProp("Book/Blue Book", rightDecorRoot.transform, shelfPos + new Vector3(-0.2f, 0.95f, 0.3f), Quaternion.Euler(0, -90, 0), null, lowPolyPaletteMat);
        LoadAndSpawnFurnitureProp("Book/Black Book", rightDecorRoot.transform, shelfPos + new Vector3(-0.2f, 0.95f, 0f), Quaternion.Euler(0, -90, 0), null, lowPolyPaletteMat);
        LoadAndSpawnFurnitureProp("Book/Yellow Book", rightDecorRoot.transform, shelfPos + new Vector3(-0.2f, 1.55f, -0.2f), Quaternion.Euler(0, -90, 0), null, lowPolyPaletteMat);
        LoadAndSpawnFurnitureProp("Book/Green Book", rightDecorRoot.transform, shelfPos + new Vector3(-0.2f, 2.15f, 0.1f), Quaternion.Euler(0, -90, 0), null, lowPolyPaletteMat);

        // 2. Meja Samping CCTV / Telepon Investigasi
        Vector3 sideDeskPos = new Vector3(roomW * 0.5f - 0.6f, 0, -1.6f);
        LoadAndSpawnOfficeBuiltInProp("Desk", rightDecorRoot.transform, sideDeskPos, Quaternion.Euler(0, -90, 0));

        // Monitor CCTV, Telepon Polisi, PC Tower
        LoadAndSpawnOfficeBuiltInProp("Monitor", rightDecorRoot.transform, sideDeskPos + new Vector3(-0.1f, 0.75f, 0.35f), Quaternion.Euler(0, -90, 0));
        LoadAndSpawnModel("Assets/Low Poly Furniture 2/Electronics/Pc.fbx", rightDecorRoot.transform, sideDeskPos + new Vector3(-0.1f, 0, 0.85f), Quaternion.Euler(0, -90, 0), Vector3.one * 1.1f);
        LoadAndSpawnModel("Assets/Low Poly Furniture 2/Electronics/Phone.fbx", rightDecorRoot.transform, sideDeskPos + new Vector3(-0.1f, 0.76f, -0.35f), Quaternion.Euler(0, -90, 0), Vector3.one * 1.1f);
        LoadAndSpawnOfficeBuiltInProp("CoffeeMug", rightDecorRoot.transform, sideDeskPos + new Vector3(-0.1f, 0.76f, 0f), Quaternion.identity);

        // 3. Brankas Bukti (Safe) di Sudut Depan Kanan
        LoadAndSpawnModel("Assets/Low Poly Furniture 2/Miscellaneous/Safe A.fbx", rightDecorRoot.transform,
            new Vector3(roomW * 0.5f - 0.65f, 0, -roomL * 0.5f + 0.75f), Quaternion.Euler(0, -45, 0), Vector3.one * 1.2f);

        // 4. Tanaman Hias Sudut Belakang Kanan
        LoadAndSpawnOfficeBuiltInProp("PottedPlant", rightDecorRoot.transform,
            new Vector3(roomW * 0.5f - 0.65f, 0, roomL * 0.5f - 0.65f), Quaternion.identity, Vector3.one * 1.25f);

        // ==================== E. DEKORASI DINDING KIRI (OBSERVASI & LACI) ====================
        GameObject leftDecorRoot = new GameObject("Decorations_Left_Wall");
        leftDecorRoot.transform.SetParent(roomRoot.transform);

        // 1. Lemari Laci Dokumen Gelap di Sisi Kaca
        Vector3 drawerPos = new Vector3(-roomW * 0.5f + 0.6f, 0, 2.4f);
        LoadAndSpawnFurnitureProp("Case/Dark Case", leftDecorRoot.transform, drawerPos, Quaternion.Euler(0, 90, 0), null, lowPolyPaletteMat);

        // 2. Tanaman Hias Sudut Belakang Kiri
        LoadAndSpawnOfficeBuiltInProp("PottedPlant", leftDecorRoot.transform,
            new Vector3(-roomW * 0.5f + 0.65f, 0, roomL * 0.5f - 0.65f), Quaternion.identity, Vector3.one * 1.25f);

        // 3. Tempat Sampah Kantor di Sudut Depan Kiri
        LoadAndSpawnModel("Assets/Low Poly Furniture 2/Miscellaneous/Bin.fbx", leftDecorRoot.transform,
            new Vector3(-roomW * 0.5f + 0.65f, 0, -roomL * 0.5f + 0.75f), Quaternion.identity, Vector3.one * 1.2f);

        // ==================== F. DEKORASI DINDING DEPAN (SAKLAR & PIAGAM) ====================
        GameObject frontDecorRoot = new GameObject("Decorations_Front_Wall");
        frontDecorRoot.transform.SetParent(roomRoot.transform);

        // Saklar Lampu di samping pintu
        LoadAndSpawnModel("Assets/Low Poly Furniture 2/Miscellaneous/Light Switch.fbx", frontDecorRoot.transform,
            new Vector3(0.9f, 1.3f, -roomL * 0.5f + 0.06f), Quaternion.identity, Vector3.one * 1.2f);

        // Piagam Penghargaan Kepolisian di Dinding Depan
        LoadAndSpawnOfficeBuiltInProp("WallPicture", frontDecorRoot.transform,
            new Vector3(-2.4f, 2.0f, -roomL * 0.5f + 0.06f), Quaternion.identity, Vector3.one * 1.3f);

        // ==================== G. PENCAHAYAAN SINEMATIK NOIR ====================
        if (addLighting)
        {
            GameObject lightsRoot = new GameObject("Cinematic_Noir_Lighting");
            lightsRoot.transform.SetParent(roomRoot.transform);

            // 1. TIGA LAMPU PENDANT GANTUNG TEPAT DI ATAS KE-3 TERSANGKA
            Vector3[] suspectSpotPos = new Vector3[] {
                tableCenter + new Vector3(-chairSpacingX, roomH - 0.2f, 0.75f), // Spot Bima
                tableCenter + new Vector3(0f, roomH - 0.2f, 0.75f),             // Spot Maya
                tableCenter + new Vector3(chairSpacingX, roomH - 0.2f, 0.75f)   // Spot Dito
            };

            for (int i = 0; i < suspectSpotPos.Length; i++)
            {
                GameObject spotGo = new GameObject($"Overhead_Spotlight_{i + 1}");
                spotGo.transform.SetParent(lightsRoot.transform);
                spotGo.transform.position = suspectSpotPos[i];
                spotGo.transform.rotation = Quaternion.Euler(85, 0, 0);

                Light spot = spotGo.AddComponent<Light>();
                spot.type = LightType.Spot;
                spot.spotAngle = 55f;
                spot.innerSpotAngle = 35f;
                spot.color = (i == 2) ? new Color(1.0f, 0.88f, 0.72f) : new Color(0.95f, 0.92f, 0.85f);
                spot.intensity = (i == 2) ? 4.2f : 3.5f;
                spot.range = 5.0f;
                spot.shadows = LightShadows.Soft;

                // Kap Lampu Gantung Metal Hitam
                GameObject fixture = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                fixture.name = "Pendant_Shade";
                fixture.transform.SetParent(spotGo.transform);
                fixture.transform.localPosition = new Vector3(0, 0, 0.15f);
                fixture.transform.localScale = new Vector3(0.45f, 0.08f, 0.45f);
                fixture.GetComponent<Renderer>().sharedMaterial = darkMetalMat;
                Collider fc = fixture.GetComponent<Collider>();
                if (fc != null) DestroyImmediate(fc);

                // Kabel Gantung ke Plafon
                GameObject cord = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cord.name = "Pendant_Cord";
                cord.transform.SetParent(spotGo.transform);
                cord.transform.localPosition = new Vector3(0, 0, 0.05f);
                cord.transform.localScale = new Vector3(0.02f, 0.15f, 0.02f);
                cord.GetComponent<Renderer>().sharedMaterial = darkMetalMat;
                Collider cc = cord.GetComponent<Collider>();
                if (cc != null) DestroyImmediate(cc);
            }

            // 2. Cahaya Biru Observasi dari Balik Kaca Dua Arah
            GameObject obsLightGo = new GameObject("Observation_BlueLight");
            obsLightGo.transform.SetParent(lightsRoot.transform);
            obsLightGo.transform.position = new Vector3(-roomW * 0.5f - 1.2f, 1.8f, 0);
            Light obsL = obsLightGo.AddComponent<Light>();
            obsL.type = LightType.Point;
            obsL.color = new Color(0.15f, 0.45f, 0.85f);
            obsL.intensity = 2.5f;
            obsL.range = 6.0f;

            // 3. Ambient Fill Light Redup
            GameObject fillGo = new GameObject("Ambient_Room_Fill");
            fillGo.transform.SetParent(lightsRoot.transform);
            fillGo.transform.position = new Vector3(0, 2.5f, -1.0f);
            Light fill = fillGo.AddComponent<Light>();
            fill.type = LightType.Point;
            fill.color = new Color(0.45f, 0.50f, 0.60f);
            fill.intensity = 0.5f;
            fill.range = 8.5f;
        }

        // ==================== H. FIXED CAMERA SETUP (SINEMATIK) ====================
        if (fixedCamera)
        {
            GameObject camGo = new GameObject("Main_Interrogation_Camera");
            camGo.transform.SetParent(roomRoot.transform);
            // Posisi kamera sinematik tepat membingkai meja, 3 tersangka, papan kasus, dan kaca observasi
            camGo.transform.position = new Vector3(0f, 1.45f, -2.75f);
            camGo.transform.rotation = Quaternion.Euler(9.5f, 0f, 0f);
            camGo.tag = "MainCamera";

            Camera cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.05f, 0.07f);
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 50f;
            cam.fieldOfView = 56f; // FOV optimal yang tidak mendistorsi ruang
            camGo.AddComponent<AudioListener>();
            camGo.AddComponent<InterrogationCameraController>();

            Selection.activeGameObject = camGo;
        }

        // Mark Scene Dirty & Save
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log($"<color=#00FF88>[SUCCESS] 1 Ruangan Interogasi 3 Orang berhasil dibangun di '{targetScenePath}' dengan warna dan tekstur lengkap!</color>");
    }

    // ==================== HELPER METHODS ====================

    private static void SpawnNPC(string npcName, string modelPath, Transform parent, Vector3 pos)
    {
        GameObject charPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        if (charPrefab == null) charPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Character/Male_Casual.fbx");

        if (charPrefab != null)
        {
            GameObject npc = (GameObject)PrefabUtility.InstantiatePrefab(charPrefab, parent);
            npc.name = npcName;
            npc.transform.position = pos;
            npc.transform.rotation = Quaternion.Euler(0, 180, 0);
            EnsureCollider(npc);

            // Pasang Animator sit controller jika ada
            var animController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animation/Controller/sit.controller");
            if (animController != null)
            {
                Animator anim = npc.GetComponent<Animator>();
                if (anim == null) anim = npc.AddComponent<Animator>();
                anim.runtimeAnimatorController = animController;
            }

            // Pasang NPCBrainTest identitas
            NPCBrainTest brain = npc.GetComponent<NPCBrainTest>();
            if (brain == null) brain = npc.AddComponent<NPCBrainTest>();
            
            if (npcName.ToLower().Contains("bima"))
            {
                brain.npcId = "bima";
                brain.npcName = "Bima Santoso";
            }
            else if (npcName.ToLower().Contains("maya") || npcName.ToLower().Contains("ardi"))
            {
                brain.npcId = "ardi";
                brain.npcName = "Ardi Adrian";
            }
            else if (npcName.ToLower().Contains("dito"))
            {
                brain.npcId = "dito";
                brain.npcName = "Dito Pradana";
            }
        }
    }

    private static void SetupNPC(GameObject go, string id, string name, RuntimeAnimatorController animCtrl)
    {
        var brain = go.GetComponent<NPCBrainTest>();
        if (brain == null) brain = go.AddComponent<NPCBrainTest>();
        brain.npcId = id;
        brain.npcName = name;

        if (go.GetComponent<Collider>() == null)
        {
            var col = go.AddComponent<CapsuleCollider>();
            col.height = 1.7f;
            col.center = new Vector3(0, 0.85f, 0);
        }

        if (animCtrl != null)
        {
            var anim = go.GetComponent<Animator>();
            if (anim == null) anim = go.AddComponent<Animator>();
            anim.runtimeAnimatorController = animCtrl;
        }
    }

    private static void CreateBaseboards(Transform parent, float roomW, float roomL, Material mat, float doorW)
    {
        float baseH = 0.12f;
        float baseD = 0.04f;

        CreateBox("Baseboard_Back", parent, new Vector3(0, baseH * 0.5f, roomL * 0.5f - baseD * 0.5f), new Vector3(roomW, baseH, baseD), mat);

        float sideW = (roomW - doorW) * 0.5f;
        CreateBox("Baseboard_Front_L", parent, new Vector3(-(roomW * 0.5f - sideW * 0.5f), baseH * 0.5f, -roomL * 0.5f + baseD * 0.5f), new Vector3(sideW, baseH, baseD), mat);
        CreateBox("Baseboard_Front_R", parent, new Vector3((roomW * 0.5f - sideW * 0.5f), baseH * 0.5f, -roomL * 0.5f + baseD * 0.5f), new Vector3(sideW, baseH, baseD), mat);

        CreateBox("Baseboard_Left", parent, new Vector3(-roomW * 0.5f + baseD * 0.5f, baseH * 0.5f, 0), new Vector3(baseD, baseH, roomL), mat);
        CreateBox("Baseboard_Right", parent, new Vector3(roomW * 0.5f - baseD * 0.5f, baseH * 0.5f, 0), new Vector3(baseD, baseH, roomL), mat);
    }

    private static void CreateTextSign(Transform parent, Vector3 pos, string text, Color color, float fontSize = 14)
    {
        GameObject textObj = new GameObject("Sign_Text");
        textObj.transform.SetParent(parent);
        textObj.transform.position = pos;

        TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.rectTransform.sizeDelta = new Vector2(5.0f, 2.0f);
    }

    private static GameObject CreateBox(string name, Transform parent, Vector3 pos, Vector3 size, Material mat, bool addCollider = true)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.localScale = size;
        go.GetComponent<Renderer>().sharedMaterial = mat;

        if (!addCollider)
        {
            Collider c = go.GetComponent<Collider>();
            if (c != null) DestroyImmediate(c);
        }
        return go;
    }

    private static GameObject LoadAndSpawnOfficeBuiltInProp(string propName, Transform parent, Vector3 pos, Quaternion rot, Vector3? scale = null)
    {
        // Load BuiltIn prefabs (yang sudah dikonfigurasi dengan HDRP materials di project ini)
        string path = $"Assets/OfficePack/Prefabs/BuiltIn/{propName}.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

        if (prefab == null)
        {
            path = $"Assets/OfficePack/Prefabs/URP/{propName}.prefab";
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        if (prefab == null) return null;

        GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        inst.transform.position = pos;
        inst.transform.rotation = rot;
        if (scale.HasValue) inst.transform.localScale = scale.Value;
        EnsureCollider(inst);
        return inst;
    }

    private static GameObject LoadAndSpawnFurnitureProp(string relativePath, Transform parent, Vector3 pos, Quaternion rot, Material overrideMat)
    {
        return LoadAndSpawnFurnitureProp(relativePath, parent, pos, rot, null, overrideMat);
    }

    private static GameObject LoadAndSpawnFurnitureProp(string relativePath, Transform parent, Vector3 pos, Quaternion rot, Vector3? scale = null, Material overrideMat = null)
    {
        string fullPath = $"Assets/Low Poly Furniture/Prefabs/{relativePath}.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
        if (prefab == null) return null;

        GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        inst.transform.position = pos;
        inst.transform.rotation = rot;
        if (scale.HasValue) inst.transform.localScale = scale.Value;

        if (overrideMat != null)
        {
            Renderer[] renderers = inst.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers) r.sharedMaterial = overrideMat;
        }

        EnsureCollider(inst);
        return inst;
    }

    private static GameObject LoadAndSpawnModel(string assetPath, Transform parent, Vector3 pos, Quaternion rot, Vector3? scale = null)
    {
        GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (model == null) return null;

        GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(model, parent);
        inst.transform.position = pos;
        inst.transform.rotation = rot;
        if (scale.HasValue) inst.transform.localScale = scale.Value;
        EnsureCollider(inst);
        return inst;
    }

    private static void EnsureCollider(GameObject target)
    {
        if (target.GetComponentInChildren<Collider>() == null)
        {
            var box = target.AddComponent<BoxCollider>();
            var renderer = target.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                box.center = target.transform.InverseTransformPoint(renderer.bounds.center);
                box.size = renderer.bounds.size;
            }
        }
    }

    private static Shader GetActivePipelineLitShader()
    {
        Shader s = Shader.Find("HDRP/Lit");
        if (s != null && UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null && UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline.GetType().Name.Contains("HD"))
            return s;

        s = Shader.Find("Universal Render Pipeline/Lit");
        if (s != null && UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null && UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline.GetType().Name.Contains("Universal"))
            return s;

        s = Shader.Find("HDRP/Lit");
        if (s != null) return s;
        s = Shader.Find("Universal Render Pipeline/Lit");
        if (s != null) return s;
        return Shader.Find("Standard");
    }

    private static Material CreateLowPolyPaletteMaterial()
    {
        Shader shader = GetActivePipelineLitShader();
        Material mat = new Material(shader);
        mat.name = "Mat_LowPoly_Palette_HDRP";

        Texture2D paletteTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Low Poly Furniture/Textures/ty-shining-gems-24-32x.png");
        if (paletteTex != null)
        {
            if (mat.HasProperty("_BaseColorMap")) mat.SetTexture("_BaseColorMap", paletteTex);
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", paletteTex);
            if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", paletteTex);
        }

        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", Color.white);

        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.15f);
        if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.15f);
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.0f);

        return mat;
    }

    private static Material CreateSimpleMaterial(string matName, Color color, float smoothness = 0.2f, float metallic = 0.05f)
    {
        Shader shader = GetActivePipelineLitShader();
        Material mat = new Material(shader);
        mat.name = matName;

        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);

        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
        if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);

        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);

        return mat;
    }

    private static Material CreateGlassMaterial(string matName, Color color)
    {
        Shader shader = GetActivePipelineLitShader();
        Material mat = new Material(shader);
        mat.name = matName;

        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);

        if (mat.HasProperty("_Surface"))
        {
            mat.SetFloat("_Surface", 1);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }
        else
        {
            mat.color = color;
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;
        }

        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.95f);
        if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.95f);
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.1f);

        return mat;
    }

    private static ChatUI BuildInterrogationChatUI(AIChatClient chatClient)
    {
        // 1. Canvas
        GameObject canvasGo = new GameObject("InterrogationCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGo.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // EventSystem
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // 2. ChatUI Component on Canvas
        ChatUI chatUI = canvasGo.AddComponent<ChatUI>();
        chatUI.aiChatClient = chatClient;
        chatUI.typeSpeed = 0.02f;

        // 3. UI Panel Container (Bottom Noir Dialogue Box)
        GameObject panelGo = new GameObject("ChatPanel");
        panelGo.transform.SetParent(canvasGo.transform, false);

        RectTransform panelRect = panelGo.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0, 35);
        panelRect.sizeDelta = new Vector2(980, 240);

        Image panelImg = panelGo.AddComponent<Image>();
        panelImg.color = new Color(0.08f, 0.10f, 0.14f, 0.94f);

        // Header Title / NPC Name
        GameObject headerGo = new GameObject("NPC_Name_Text");
        headerGo.transform.SetParent(panelGo.transform, false);
        RectTransform headerRect = headerGo.AddComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0f, 1f);
        headerRect.anchorMax = new Vector2(1f, 1f);
        headerRect.pivot = new Vector2(0.5f, 1f);
        headerRect.anchoredPosition = new Vector2(0, -15);
        headerRect.sizeDelta = new Vector2(-40, 32);

        TextMeshProUGUI headerText = headerGo.AddComponent<TextMeshProUGUI>();
        headerText.text = "<b><color=#E0A838>● RUANG INTEROGASI</color></b> | <i>Saksi</i>";
        headerText.fontSize = 20;
        headerText.color = Color.white;
        headerText.alignment = TextAlignmentOptions.Left;

        // Dialog Content Text
        GameObject dialogGo = new GameObject("Chat_Text");
        dialogGo.transform.SetParent(panelGo.transform, false);
        RectTransform dialogRect = dialogGo.AddComponent<RectTransform>();
        dialogRect.anchorMin = new Vector2(0f, 0.35f);
        dialogRect.anchorMax = new Vector2(1f, 1f);
        dialogRect.pivot = new Vector2(0.5f, 0.5f);
        dialogRect.anchoredPosition = new Vector2(0, -25);
        dialogRect.sizeDelta = new Vector2(-50, -40);

        TextMeshProUGUI dialogText = dialogGo.AddComponent<TextMeshProUGUI>();
        dialogText.text = "Pilih karakter untuk memulai interogasi...";
        dialogText.fontSize = 21;
        dialogText.color = new Color(0.92f, 0.94f, 0.96f);
        dialogText.alignment = TextAlignmentOptions.TopLeft;

        // Input Field Box
        GameObject inputGo = new GameObject("Input_Field");
        inputGo.transform.SetParent(panelGo.transform, false);
        RectTransform inputRect = inputGo.AddComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0f, 0f);
        inputRect.anchorMax = new Vector2(1f, 0.35f);
        inputRect.pivot = new Vector2(0.5f, 0f);
        inputRect.anchoredPosition = new Vector2(-60, 15);
        inputRect.sizeDelta = new Vector2(-160, 0);

        Image inputBg = inputGo.AddComponent<Image>();
        inputBg.color = new Color(0.14f, 0.17f, 0.22f, 1.0f);

        // Text Area for InputField
        GameObject textArea = new GameObject("Text Area");
        textArea.transform.SetParent(inputGo.transform, false);
        RectTransform taRect = textArea.AddComponent<RectTransform>();
        taRect.anchorMin = Vector2.zero;
        taRect.anchorMax = Vector2.one;
        taRect.sizeDelta = new Vector2(-20, -10);

        // Input Text
        GameObject inputTextGo = new GameObject("Text");
        inputTextGo.transform.SetParent(textArea.transform, false);
        RectTransform textRect = inputTextGo.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        TextMeshProUGUI inputText = inputTextGo.AddComponent<TextMeshProUGUI>();
        inputText.fontSize = 18;
        inputText.color = Color.white;

        // Input Placeholder
        GameObject placeholderGo = new GameObject("Placeholder");
        placeholderGo.transform.SetParent(textArea.transform, false);
        RectTransform phRect = placeholderGo.AddComponent<RectTransform>();
        phRect.anchorMin = Vector2.zero;
        phRect.anchorMax = Vector2.one;
        phRect.sizeDelta = Vector2.zero;
        TextMeshProUGUI phText = placeholderGo.AddComponent<TextMeshProUGUI>();
        phText.text = "Ketik pertanyaanmu untuk tersangka di sini...";
        phText.fontSize = 18;
        phText.fontStyle = FontStyles.Italic;
        phText.color = new Color(0.6f, 0.65f, 0.7f, 0.7f);

        // TMP_InputField Setup
        TMP_InputField inputField = inputGo.AddComponent<TMP_InputField>();
        inputField.textViewport = taRect;
        inputField.textComponent = inputText;
        inputField.placeholder = phText;

        // Send Button
        GameObject sendBtnGo = new GameObject("SendButton");
        sendBtnGo.transform.SetParent(panelGo.transform, false);
        RectTransform btnRect = sendBtnGo.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(1f, 0f);
        btnRect.anchorMax = new Vector2(1f, 0.35f);
        btnRect.pivot = new Vector2(1f, 0f);
        btnRect.anchoredPosition = new Vector2(-25, 15);
        btnRect.sizeDelta = new Vector2(110, 0);

        Image btnImg = sendBtnGo.AddComponent<Image>();
        btnImg.color = new Color(0.18f, 0.60f, 0.95f);
        Button sendBtn = sendBtnGo.AddComponent<Button>();

        GameObject btnTextGo = new GameObject("Text");
        btnTextGo.transform.SetParent(sendBtnGo.transform, false);
        RectTransform btRect = btnTextGo.AddComponent<RectTransform>();
        btRect.anchorMin = Vector2.zero;
        btRect.anchorMax = Vector2.one;
        btRect.sizeDelta = Vector2.zero;
        TextMeshProUGUI btnText = btnTextGo.AddComponent<TextMeshProUGUI>();
        btnText.text = "<b>KIRIM</b>";
        btnText.fontSize = 18;
        btnText.color = Color.white;
        btnText.alignment = TextAlignmentOptions.Center;

        // Wire references to ChatUI
        chatUI.chatPanel = panelGo;
        chatUI.npcNameText = headerText;
        chatUI.chatText = dialogText;
        chatUI.inputField = inputField;

        sendBtn.onClick.AddListener(chatUI.SendMessageToNPC);

        return chatUI;
    }

    private static void SetupAtmosphere()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.20f, 0.25f, 0.35f);
        RenderSettings.ambientEquatorColor = new Color(0.12f, 0.15f, 0.20f);
        RenderSettings.ambientGroundColor = new Color(0.06f, 0.07f, 0.09f);
    }
}
#endif
