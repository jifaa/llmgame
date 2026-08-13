#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.IO;

/// <summary>
/// InterrogationRoomBuilder: Generator otomatis untuk Ruang Interogasi Kantor Polisi.
/// Membangun arsitektur, properti, pencahayaan sinematik noir, NPC Tersangka (Dito),
/// Detective Player MC, dan sistem Chat UI AI yang langsung berfungsi 100%.
/// </summary>
public class InterrogationRoomBuilder : EditorWindow
{
    [MenuItem("Tools/LLM Game/Build Interrogation Room")]
    public static void Open()
    {
        var win = GetWindow<InterrogationRoomBuilder>("Interrogation Room Builder");
        win.minSize = new Vector2(400, 360);
        win.Show();
    }

    [MenuItem("Tools/LLM Game/Quick Build Interrogation Room in PoliceIndoor Scene")]
    public static void QuickBuild()
    {
        BuildRoomInScene("Assets/PoliceIndoor.unity");
    }

    private string scenePath = "Assets/PoliceIndoor.unity";

    void OnGUI()
    {
        EditorGUILayout.Space(10);
        GUILayout.Label("POLICE INTERROGATION ROOM BUILDER", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Tool ini akan membangun Ruang Interogasi Kantor Polisi secara lengkap: " +
            "dinding, lantai, kaca dua arah (one-way mirror), meja & kursi interogasi, " +
            "pencahayaan sinematik noir, MC Player, dan Chat UI AI.", 
            MessageType.Info
        );

        EditorGUILayout.Space(10);
        scenePath = EditorGUILayout.TextField("Target Scene", scenePath);

        EditorGUILayout.Space(15);
        GUI.backgroundColor = new Color(0.2f, 0.8f, 0.4f);
        if (GUILayout.Button("GENERATE RUANG INTEROGASI SEKARANG", GUILayout.Height(45)))
        {
            BuildRoomInScene(scenePath);
        }
        GUI.backgroundColor = Color.white;
    }

    public static void BuildRoomInScene(string targetScenePath)
    {
        // 1. Pastikan Scene Terbuka
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

        // 2. Setup Suasana & RenderSettings (Noir Detective Atmosphere)
        SetupAtmosphere();

        // 3. Load Materials & Prefabs
        Material wallMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Low Poly Furniture/Demos/Materials/Walls.mat");
        Material floorMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Low Poly Furniture/Demos/Materials/Ground_DemoScene.mat");
        if (wallMat == null) wallMat = CreateSimpleMaterial("Mat_InterrogationWall", new Color(0.22f, 0.24f, 0.28f));
        if (floorMat == null) floorMat = CreateSimpleMaterial("Mat_InterrogationFloor", new Color(0.12f, 0.13f, 0.16f));

        Material darkMetalMat = CreateSimpleMaterial("Mat_DarkMetal", new Color(0.15f, 0.16f, 0.18f));
        Material glassMat = CreateGlassMaterial("Mat_OneWayMirror", new Color(0.08f, 0.12f, 0.15f, 0.85f));
        Material ceilingMat = CreateSimpleMaterial("Mat_CeilingDark", new Color(0.14f, 0.15f, 0.18f));
        Material baseboardMat = CreateSimpleMaterial("Mat_Baseboard", new Color(0.1f, 0.1f, 0.12f));

        // 4. Root Container
        GameObject roomRoot = new GameObject("--- INTERROGATION ROOM ---");
        Undo.RegisterCreatedObjectUndo(roomRoot, "Create Interrogation Room");

        // Dimensi Ruangan
        float roomW = 7.0f; // Lebar X
        float roomL = 9.0f; // Panjang Z
        float roomH = 3.4f; // Tinggi Y

        // ==================== A. ARSITEKTUR RUANGAN ====================
        GameObject structRoot = new GameObject("Architecture");
        structRoot.transform.SetParent(roomRoot.transform);

        // Lantai
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.SetParent(structRoot.transform);
        floor.transform.position = new Vector3(0, -0.1f, 0);
        floor.transform.localScale = new Vector3(roomW, 0.2f, roomL);
        floor.GetComponent<Renderer>().sharedMaterial = floorMat;

        // Plafon (Ceiling)
        GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ceiling.name = "Ceiling";
        ceiling.transform.SetParent(structRoot.transform);
        ceiling.transform.position = new Vector3(0, roomH + 0.1f, 0);
        ceiling.transform.localScale = new Vector3(roomW, 0.2f, roomL);
        ceiling.GetComponent<Renderer>().sharedMaterial = ceilingMat;

        // Dinding Depan (Front Wall / Papan Tulis)
        GameObject wallFront = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallFront.name = "Wall_Front";
        wallFront.transform.SetParent(structRoot.transform);
        wallFront.transform.position = new Vector3(0, roomH * 0.5f, roomL * 0.5f);
        wallFront.transform.localScale = new Vector3(roomW, roomH, 0.2f);
        wallFront.GetComponent<Renderer>().sharedMaterial = wallMat;

        // Dinding Belakang (Back Wall / Pintu Masuk)
        // Dibuat 2 panel dinding kiri-kanan dengan celah pintu di tengah
        float doorWidth = 1.3f;
        float sideWallW = (roomW - doorWidth) * 0.5f;

        GameObject wallBackL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallBackL.name = "Wall_Back_Left";
        wallBackL.transform.SetParent(structRoot.transform);
        wallBackL.transform.position = new Vector3(-(roomW * 0.5f - sideWallW * 0.5f), roomH * 0.5f, -roomL * 0.5f);
        wallBackL.transform.localScale = new Vector3(sideWallW, roomH, 0.2f);
        wallBackL.GetComponent<Renderer>().sharedMaterial = wallMat;

        GameObject wallBackR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallBackR.name = "Wall_Back_Right";
        wallBackR.transform.SetParent(structRoot.transform);
        wallBackR.transform.position = new Vector3((roomW * 0.5f - sideWallW * 0.5f), roomH * 0.5f, -roomL * 0.5f);
        wallBackR.transform.localScale = new Vector3(sideWallW, roomH, 0.2f);
        wallBackR.GetComponent<Renderer>().sharedMaterial = wallMat;

        GameObject wallBackTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallBackTop.name = "Wall_Back_Top";
        wallBackTop.transform.SetParent(structRoot.transform);
        wallBackTop.transform.position = new Vector3(0, 2.7f + (roomH - 2.7f) * 0.5f, -roomL * 0.5f);
        wallBackTop.transform.localScale = new Vector3(doorWidth, roomH - 2.7f, 0.2f);
        wallBackTop.GetComponent<Renderer>().sharedMaterial = wallMat;

        // Dinding Kanan (Solid Wall)
        GameObject wallRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallRight.name = "Wall_Right";
        wallRight.transform.SetParent(structRoot.transform);
        wallRight.transform.position = new Vector3(roomW * 0.5f, roomH * 0.5f, 0);
        wallRight.transform.localScale = new Vector3(0.2f, roomH, roomL);
        wallRight.GetComponent<Renderer>().sharedMaterial = wallMat;

        // Dinding Kiri (Dengan One-Way Mirror / Kaca Observasi)
        float windowW = 3.2f;
        float windowH = 1.6f;
        float windowCenterY = 1.7f;
        float sideWallZ = (roomL - windowW) * 0.5f;

        GameObject wallLeftFront = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallLeftFront.name = "Wall_Left_Front";
        wallLeftFront.transform.SetParent(structRoot.transform);
        wallLeftFront.transform.position = new Vector3(-roomW * 0.5f, roomH * 0.5f, roomL * 0.5f - sideWallZ * 0.5f);
        wallLeftFront.transform.localScale = new Vector3(0.2f, roomH, sideWallZ);
        wallLeftFront.GetComponent<Renderer>().sharedMaterial = wallMat;

        GameObject wallLeftBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallLeftBack.name = "Wall_Left_Back";
        wallLeftBack.transform.SetParent(structRoot.transform);
        wallLeftBack.transform.position = new Vector3(-roomW * 0.5f, roomH * 0.5f, -roomL * 0.5f + sideWallZ * 0.5f);
        wallLeftBack.transform.localScale = new Vector3(0.2f, roomH, sideWallZ);
        wallLeftBack.GetComponent<Renderer>().sharedMaterial = wallMat;

        GameObject wallLeftBottom = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallLeftBottom.name = "Wall_Left_Bottom";
        wallLeftBottom.transform.SetParent(structRoot.transform);
        wallLeftBottom.transform.position = new Vector3(-roomW * 0.5f, (windowCenterY - windowH * 0.5f) * 0.5f, 0);
        wallLeftBottom.transform.localScale = new Vector3(0.2f, (windowCenterY - windowH * 0.5f), windowW);
        wallLeftBottom.GetComponent<Renderer>().sharedMaterial = wallMat;

        GameObject wallLeftTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallLeftTop.name = "Wall_Left_Top";
        wallLeftTop.transform.SetParent(structRoot.transform);
        float topH = roomH - (windowCenterY + windowH * 0.5f);
        wallLeftTop.transform.position = new Vector3(-roomW * 0.5f, (windowCenterY + windowH * 0.5f) + topH * 0.5f, 0);
        wallLeftTop.transform.localScale = new Vector3(0.2f, topH, windowW);
        wallLeftTop.GetComponent<Renderer>().sharedMaterial = wallMat;

        // Kaca Dua Arah (One-Way Mirror)
        GameObject mirror = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mirror.name = "OneWay_Mirror_Glass";
        mirror.transform.SetParent(structRoot.transform);
        mirror.transform.position = new Vector3(-roomW * 0.5f, windowCenterY, 0);
        mirror.transform.localScale = new Vector3(0.06f, windowH, windowW);
        mirror.GetComponent<Renderer>().sharedMaterial = glassMat;

        // Frame Kaca Dua Arah
        GameObject mirrorFrame = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mirrorFrame.name = "OneWay_Mirror_Frame";
        mirrorFrame.transform.SetParent(structRoot.transform);
        mirrorFrame.transform.position = new Vector3(-roomW * 0.5f + 0.04f, windowCenterY, 0);
        mirrorFrame.transform.localScale = new Vector3(0.04f, windowH + 0.12f, windowW + 0.12f);
        mirrorFrame.GetComponent<Renderer>().sharedMaterial = darkMetalMat;
        // Biar tembus pandang frame luarnya
        Collider frameCol = mirrorFrame.GetComponent<Collider>();
        if (frameCol != null) DestroyImmediate(frameCol);

        // --- POLISH ARCHITECTURE: Baseboards (Plin Lantai) ---
        float baseH = 0.15f;
        float baseD = 0.04f;
        
        GameObject plinFront = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plinFront.name = "Baseboard_Front";
        plinFront.transform.SetParent(structRoot.transform);
        plinFront.transform.position = new Vector3(0, baseH * 0.5f, roomL * 0.5f - baseD * 0.5f);
        plinFront.transform.localScale = new Vector3(roomW, baseH, baseD);
        plinFront.GetComponent<Renderer>().sharedMaterial = baseboardMat;

        GameObject plinBackL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plinBackL.name = "Baseboard_Back_L";
        plinBackL.transform.SetParent(structRoot.transform);
        plinBackL.transform.position = new Vector3(-(roomW * 0.5f - sideWallW * 0.5f), baseH * 0.5f, -roomL * 0.5f + baseD * 0.5f);
        plinBackL.transform.localScale = new Vector3(sideWallW, baseH, baseD);
        plinBackL.GetComponent<Renderer>().sharedMaterial = baseboardMat;

        GameObject plinBackR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plinBackR.name = "Baseboard_Back_R";
        plinBackR.transform.SetParent(structRoot.transform);
        plinBackR.transform.position = new Vector3((roomW * 0.5f - sideWallW * 0.5f), baseH * 0.5f, -roomL * 0.5f + baseD * 0.5f);
        plinBackR.transform.localScale = new Vector3(sideWallW, baseH, baseD);
        plinBackR.GetComponent<Renderer>().sharedMaterial = baseboardMat;

        GameObject plinRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plinRight.name = "Baseboard_Right";
        plinRight.transform.SetParent(structRoot.transform);
        plinRight.transform.position = new Vector3(roomW * 0.5f - baseD * 0.5f, baseH * 0.5f, 0);
        plinRight.transform.localScale = new Vector3(baseD, baseH, roomL);
        plinRight.GetComponent<Renderer>().sharedMaterial = baseboardMat;

        GameObject plinLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plinLeft.name = "Baseboard_Left";
        plinLeft.transform.SetParent(structRoot.transform);
        plinLeft.transform.position = new Vector3(-roomW * 0.5f + baseD * 0.5f, baseH * 0.5f, 0);
        plinLeft.transform.localScale = new Vector3(baseD, baseH, roomL);
        plinLeft.GetComponent<Renderer>().sharedMaterial = baseboardMat;

        // --- POLISH ARCHITECTURE: Corner Pillars ---
        float pillarSize = 0.3f;
        Vector3[] pillarPositions = new Vector3[] {
            new Vector3(-roomW * 0.5f + pillarSize * 0.5f, roomH * 0.5f, -roomL * 0.5f + pillarSize * 0.5f),
            new Vector3(roomW * 0.5f - pillarSize * 0.5f, roomH * 0.5f, -roomL * 0.5f + pillarSize * 0.5f),
            new Vector3(roomW * 0.5f - pillarSize * 0.5f, roomH * 0.5f, roomL * 0.5f - pillarSize * 0.5f),
            new Vector3(-roomW * 0.5f + pillarSize * 0.5f, roomH * 0.5f, roomL * 0.5f - pillarSize * 0.5f)
        };
        for (int i = 0; i < 4; i++)
        {
            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pillar.name = "Corner_Pillar_" + i;
            pillar.transform.SetParent(structRoot.transform);
            pillar.transform.position = pillarPositions[i];
            pillar.transform.localScale = new Vector3(pillarSize, roomH, pillarSize);
            pillar.GetComponent<Renderer>().sharedMaterial = baseboardMat;
        }

        // Pintu Masuk
        GameObject doorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Low Poly Furniture 2/Doors/Door A.fbx");
        if (doorPrefab != null)
        {
            GameObject doorObj = (GameObject)PrefabUtility.InstantiatePrefab(doorPrefab, structRoot.transform);
            doorObj.name = "Entrance_Door";
            doorObj.transform.position = new Vector3(0, 0, -roomL * 0.5f + 0.05f);
            doorObj.transform.rotation = Quaternion.Euler(0, 0, 0);
            doorObj.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        }

        // ==================== B. MEJA & KURSI INTEROGASI ====================
        GameObject furnitureRoot = new GameObject("Furniture");
        furnitureRoot.transform.SetParent(roomRoot.transform);

        // Meja Tengah Interogasi
        GameObject tablePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Low Poly Furniture/Prefabs/Table/Rectangle Table.prefab");
        if (tablePrefab == null) tablePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Low Poly Furniture/Prefabs/Table/Table.prefab");
        
        GameObject tableObj = null;
        if (tablePrefab != null)
        {
            tableObj = (GameObject)PrefabUtility.InstantiatePrefab(tablePrefab, furnitureRoot.transform);
            tableObj.name = "Interrogation_Table";
            tableObj.transform.position = new Vector3(0, 0, 0.2f);
            tableObj.transform.rotation = Quaternion.Euler(0, 90, 0);
            tableObj.transform.localScale = new Vector3(1.15f, 1.0f, 1.15f);
            EnsureMeshOrBoxCollider(tableObj);
        }

        // Kursi Detektif (Menghadap Tersangka ke arah Z+)
        GameObject chairPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Low Poly Furniture/Prefabs/Chair/Standart Chair.prefab");
        if (chairPrefab == null) chairPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Low Poly Furniture/Prefabs/Chair/Thin Chair.prefab");

        if (chairPrefab != null)
        {
            GameObject detChair = (GameObject)PrefabUtility.InstantiatePrefab(chairPrefab, furnitureRoot.transform);
            detChair.name = "Chair_Detective";
            detChair.transform.position = new Vector3(0, 0, -0.95f);
            detChair.transform.rotation = Quaternion.Euler(0, 0, 0);
            EnsureMeshOrBoxCollider(detChair);

            // Kursi Tersangka (Menghadap Detektif ke arah Z-)
            GameObject suspChair = (GameObject)PrefabUtility.InstantiatePrefab(chairPrefab, furnitureRoot.transform);
            suspChair.name = "Chair_Suspect";
            suspChair.transform.position = new Vector3(0, 0, 1.35f);
            suspChair.transform.rotation = Quaternion.Euler(0, 180, 0);
            EnsureMeshOrBoxCollider(suspChair);
        }

        // Properti Meja: Laptop & Lampu Meja & Berkas
        GameObject laptopPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Low Poly Furniture/Prefabs/Screen/Laptop.prefab");
        if (laptopPrefab != null)
        {
            GameObject laptop = (GameObject)PrefabUtility.InstantiatePrefab(laptopPrefab, furnitureRoot.transform);
            laptop.name = "Detective_Laptop";
            laptop.transform.position = new Vector3(-0.35f, 0.76f, -0.1f);
            laptop.transform.rotation = Quaternion.Euler(0, 15, 0);
        }

        GameObject lampPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Low Poly Furniture/Prefabs/Lamp/Lamp.prefab");
        if (lampPrefab != null)
        {
            GameObject lamp = (GameObject)PrefabUtility.InstantiatePrefab(lampPrefab, furnitureRoot.transform);
            lamp.name = "Desk_Lamp";
            lamp.transform.position = new Vector3(0.55f, 0.76f, 0.2f);
            lamp.transform.rotation = Quaternion.Euler(0, -135, 0);

            // Polish: Add Spotlight to Desk Lamp aiming at suspect
            GameObject lampSpot = new GameObject("Desk_Lamp_Spotlight");
            lampSpot.transform.SetParent(lamp.transform);
            lampSpot.transform.localPosition = new Vector3(0.12f, 0.3f, 0);
            lampSpot.transform.localRotation = Quaternion.Euler(30, -90, 0);

            Light dl = lampSpot.AddComponent<Light>();
            dl.type = LightType.Spot;
            dl.color = new Color(1.0f, 0.85f, 0.7f); // Warm tungsten
            dl.intensity = 3.5f;
            dl.range = 4.0f;
            dl.spotAngle = 50f;
            dl.innerSpotAngle = 30f;
            dl.shadows = LightShadows.Soft;
            dl.shadowStrength = 0.8f;
        }

        // Berkas Kasus (Books)
        GameObject bookYellow = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Low Poly Furniture/Prefabs/Book/Yellow Book.prefab");
        GameObject bookBlack = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Low Poly Furniture/Prefabs/Book/Black Book.prefab");
        if (bookYellow != null)
        {
            GameObject b1 = (GameObject)PrefabUtility.InstantiatePrefab(bookYellow, furnitureRoot.transform);
            b1.name = "Case_File_Folder";
            b1.transform.position = new Vector3(0.25f, 0.76f, -0.12f);
            b1.transform.rotation = Quaternion.Euler(0, 20, 0);
        }
        if (bookBlack != null)
        {
            GameObject b2 = (GameObject)PrefabUtility.InstantiatePrefab(bookBlack, furnitureRoot.transform);
            b2.name = "Detective_Notes";
            b2.transform.position = new Vector3(0.23f, 0.80f, -0.11f);
            b2.transform.rotation = Quaternion.Euler(0, 5, 0);
        }

        // ==================== C. PERABOT PENDUKUNG RUANGAN ====================
        // Lemari Arsip / Rak Buku Kasus di Sudut
        GameObject bookshelfPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Low Poly Furniture/Prefabs/Bookshelf/Standart Bookshelf.prefab");
        if (bookshelfPrefab != null)
        {
            GameObject shelf = (GameObject)PrefabUtility.InstantiatePrefab(bookshelfPrefab, furnitureRoot.transform);
            shelf.name = "Police_Archive_Shelf";
            shelf.transform.position = new Vector3(roomW * 0.5f - 0.5f, 0, 2.8f);
            shelf.transform.rotation = Quaternion.Euler(0, -90, 0);
            EnsureMeshOrBoxCollider(shelf);
        }

        // Lemari Laci
        GameObject drawerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Low Poly Furniture 2/Drawers/Drawer A.fbx");
        if (drawerPrefab != null)
        {
            GameObject drawer = (GameObject)PrefabUtility.InstantiatePrefab(drawerPrefab, furnitureRoot.transform);
            drawer.name = "Filing_Cabinet";
            drawer.transform.position = new Vector3(-roomW * 0.5f + 0.6f, 0, 3.2f);
            drawer.transform.rotation = Quaternion.Euler(0, 90, 0);
            drawer.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            EnsureMeshOrBoxCollider(drawer);
        }

        // Papan Tulis / Evidence Board di Dinding Depan
        GameObject boardObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        boardObj.name = "Police_Whiteboard";
        boardObj.transform.SetParent(furnitureRoot.transform);
        boardObj.transform.position = new Vector3(0, 1.9f, roomL * 0.5f - 0.08f);
        boardObj.transform.localScale = new Vector3(3.2f, 1.6f, 0.05f);
        boardObj.GetComponent<Renderer>().sharedMaterial = CreateSimpleMaterial("Mat_Whiteboard", new Color(0.9f, 0.92f, 0.95f));

        // Bingkai Papan Tulis
        GameObject boardFrame = GameObject.CreatePrimitive(PrimitiveType.Cube);
        boardFrame.name = "Board_Frame";
        boardFrame.transform.SetParent(boardObj.transform);
        boardFrame.transform.localPosition = Vector3.zero;
        boardFrame.transform.localScale = new Vector3(1.03f, 1.05f, 0.8f);
        boardFrame.GetComponent<Renderer>().sharedMaterial = darkMetalMat;
        Collider bfCol = boardFrame.GetComponent<Collider>();
        if (bfCol != null) DestroyImmediate(bfCol);

        // Tanaman di Sudut
        GameObject plantPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Low Poly Furniture/Prefabs/Flower/Green Plant.prefab");
        if (plantPrefab != null)
        {
            GameObject plant = (GameObject)PrefabUtility.InstantiatePrefab(plantPrefab, furnitureRoot.transform);
            plant.name = "Room_Plant";
            plant.transform.position = new Vector3(roomW * 0.5f - 0.7f, 0, -roomL * 0.5f + 0.8f);
            plant.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
        }

        // ==================== D. PENCAHAYAAN SINEMATIK NOIR ====================
        GameObject lightsRoot = new GameObject("Lights");
        lightsRoot.transform.SetParent(roomRoot.transform);

        // 1. Lampu Sorot Utama Menggantung Tepat di Atas Meja (Overhead Spotlight)
        GameObject spotGo = new GameObject("Overhead_Interrogation_Spotlight");
        spotGo.transform.SetParent(lightsRoot.transform);
        spotGo.transform.position = new Vector3(0, roomH - 0.1f, 0.2f);
        spotGo.transform.rotation = Quaternion.Euler(90, 0, 0);

        Light spotLight = spotGo.AddComponent<Light>();
        spotLight.type = LightType.Spot;
        spotLight.spotAngle = 68f;
        spotLight.innerSpotAngle = 45f;
        spotLight.color = new Color(1.0f, 0.94f, 0.82f); // Warm neutral amber
        spotLight.intensity = 4.2f;
        spotLight.range = 5.5f;
        spotLight.shadows = LightShadows.Soft;
        spotLight.shadowStrength = 0.9f;

        // Model gantung lampu plafon sederhana
        GameObject fixture = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        fixture.name = "Lamp_Fixture";
        fixture.transform.SetParent(spotGo.transform);
        fixture.transform.localPosition = new Vector3(0, 0, 0.15f);
        fixture.transform.localRotation = Quaternion.Euler(0, 0, 0);
        fixture.transform.localScale = new Vector3(0.45f, 0.08f, 0.45f);
        fixture.GetComponent<Renderer>().sharedMaterial = darkMetalMat;
        Collider fixCol = fixture.GetComponent<Collider>();
        if (fixCol != null) DestroyImmediate(fixCol);

        // 2. Ambient Fill Point Light (Redup untuk bayangan sudut)
        GameObject fillGo = new GameObject("Ambient_Fill_Light");
        fillGo.transform.SetParent(lightsRoot.transform);
        fillGo.transform.position = new Vector3(0, 2.5f, 0);

        Light fillLight = fillGo.AddComponent<Light>();
        fillLight.type = LightType.Point;
        fillLight.color = new Color(0.35f, 0.45f, 0.6f); // Cool moody detective blue
        fillLight.intensity = 0.45f;
        fillLight.range = 9.0f;
        fillLight.shadows = LightShadows.None;

        // 3. Polish: Observation Room Light (Cahaya dari balik kaca)
        GameObject obsLightGo = new GameObject("Observation_Room_Light");
        obsLightGo.transform.SetParent(lightsRoot.transform);
        obsLightGo.transform.position = new Vector3(-roomW * 0.5f - 1.5f, 1.8f, 0); // Di luar jendela

        Light obsLight = obsLightGo.AddComponent<Light>();
        obsLight.type = LightType.Point;
        obsLight.color = new Color(0.1f, 0.4f, 0.6f); // Cyan/Blue observation tint
        obsLight.intensity = 2.5f;
        obsLight.range = 6.0f;
        obsLight.shadows = LightShadows.None;

        // ==================== F. DETECTIVE PLAYER (MC) ====================
        GameObject playerGo = new GameObject("Player");
        playerGo.transform.SetParent(roomRoot.transform);
        playerGo.transform.position = new Vector3(0, 0.1f, -3.2f); // Berdiri di dekat pintu masuk
        playerGo.transform.rotation = Quaternion.Euler(0, 0, 0);    // Menghadap ke meja interogasi

        CharacterController charCtrl = playerGo.AddComponent<CharacterController>();
        charCtrl.height = 1.8f;
        charCtrl.radius = 0.3f;
        charCtrl.center = new Vector3(0, 0.9f, 0);

        FirstPerson fp = playerGo.AddComponent<FirstPerson>();
        fp.moveSpeed = 3.5f;
        fp.mouseSensitivity = 2.0f;
        fp.canControl = true;

        // Child Camera
        GameObject camGo = new GameObject("Camera");
        camGo.transform.SetParent(playerGo.transform);
        camGo.transform.localPosition = new Vector3(0, 1.6f, 0);
        camGo.transform.localRotation = Quaternion.identity;

        Camera cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.06f, 0.08f);
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 100f;
        camGo.AddComponent<AudioListener>();

        fp.cameraTransform = camGo.transform;

        // Interaction Script
        NPCInteraction interaction = camGo.AddComponent<NPCInteraction>();
        interaction.interactDistance = 3.8f;
        interaction.playerBody = playerGo.transform;
        interaction.cameraTransform = camGo.transform;
        interaction.playerController = fp;
        interaction.npcLookHeight = 1.4f;

        // ==================== G. AI MANAGERS & CHAT UI CANVAS ====================
        GameObject aiManagerGo = new GameObject("AIChatManager");
        aiManagerGo.transform.SetParent(roomRoot.transform);

        AIDataLoader dataLoader = aiManagerGo.AddComponent<AIDataLoader>();
        AIChatClient chatClient = aiManagerGo.AddComponent<AIChatClient>();
        chatClient.aiDataLoader = dataLoader;
        chatClient.serverUrl = "http://localhost:8000/generate";
        chatClient.playerEvidence = "Belum ada bukti yang ditemukan.";

        EvidenceManager evidenceManager = aiManagerGo.AddComponent<EvidenceManager>();
        evidenceManager.aiChatClient = chatClient;

        interaction.evidenceManager = evidenceManager;

        // Apply URP Material Fix for all generated furniture
        FixURPMaterials(roomRoot);

        // Canvas & UI
        ChatUI chatUI = BuildInterrogationChatUI(roomRoot.transform, chatClient, fp);
        interaction.chatUI = chatUI;

        // Mark Scene Dirty & Save
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log($"[SUCCESS] Ruang Interogasi Polisi berhasil dibuat di '{targetScenePath}'! Buka dan tekan Play untuk mencoba.");
        Selection.activeGameObject = playerGo;
    }

    private static ChatUI BuildInterrogationChatUI(Transform parent, AIChatClient chatClient, FirstPerson player)
    {
        // 1. Canvas
        GameObject canvasGo = new GameObject("InterrogationCanvas");
        canvasGo.transform.SetParent(parent);

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
            eventSystemGo.transform.SetParent(parent);
            eventSystemGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // 2. ChatUI Component on Canvas
        ChatUI chatUI = canvasGo.AddComponent<ChatUI>();
        chatUI.aiChatClient = chatClient;
        chatUI.playerController = player;
        chatUI.typeSpeed = 0.02f;

        // 3. UI Panel Container (Bottom Noir Dialogue Box)
        GameObject panelGo = new GameObject("InterrogationPanel");
        panelGo.transform.SetParent(canvasGo.transform, false);

        RectTransform panelRect = panelGo.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0, 40);
        panelRect.sizeDelta = new Vector2(1000, 260);

        Image panelImg = panelGo.AddComponent<Image>();
        Sprite roundedSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/rounded_rect.png");
        if (roundedSprite != null) panelImg.sprite = roundedSprite;
        panelImg.color = new Color(0.08f, 0.10f, 0.14f, 0.94f); // Sleek Dark Slate Noir

        // Header Title (Badge / Interogasi)
        GameObject headerGo = new GameObject("Title_Text");
        headerGo.transform.SetParent(panelGo.transform, false);
        RectTransform headerRect = headerGo.AddComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0f, 1f);
        headerRect.anchorMax = new Vector2(1f, 1f);
        headerRect.pivot = new Vector2(0.5f, 1f);
        headerRect.anchoredPosition = new Vector2(0, -15);
        headerRect.sizeDelta = new Vector2(-40, 30);

        TextMeshProUGUI headerText = headerGo.AddComponent<TextMeshProUGUI>();
        headerText.text = "<b><color=#E0A838>● RUANG INTEROGASI</color></b> | <i>Tersangka: Dito</i>";
        headerText.fontSize = 20;
        headerText.color = Color.white;
        headerText.alignment = TextAlignmentOptions.Left;

        // Dialog Content Text
        GameObject dialogGo = new GameObject("Dialog_Text");
        dialogGo.transform.SetParent(panelGo.transform, false);
        RectTransform dialogRect = dialogGo.AddComponent<RectTransform>();
        dialogRect.anchorMin = new Vector2(0f, 0.35f);
        dialogRect.anchorMax = new Vector2(1f, 1f);
        dialogRect.pivot = new Vector2(0.5f, 0.5f);
        dialogRect.anchoredPosition = new Vector2(0, -25);
        dialogRect.sizeDelta = new Vector2(-50, -40);

        TextMeshProUGUI dialogText = dialogGo.AddComponent<TextMeshProUGUI>();
        dialogText.text = "Dito: Ada yang ingin kamu tanyakan?";
        dialogText.fontSize = 22;
        dialogText.color = new Color(0.92f, 0.94f, 0.96f);
        dialogText.alignment = TextAlignmentOptions.TopLeft;

        // Input Field Box
        GameObject inputGo = new GameObject("Input_Field");
        inputGo.transform.SetParent(panelGo.transform, false);
        RectTransform inputRect = inputGo.AddComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0f, 0f);
        inputRect.anchorMax = new Vector2(1f, 0.35f);
        inputRect.pivot = new Vector2(0.5f, 0f);
        inputRect.anchoredPosition = new Vector2(0, 15);
        inputRect.sizeDelta = new Vector2(-50, 0);

        Image inputBg = inputGo.AddComponent<Image>();
        if (roundedSprite != null) inputBg.sprite = roundedSprite;
        inputBg.color = new Color(0.14f, 0.17f, 0.22f, 1.0f);

        // Text Area for InputField
        GameObject textArea = new GameObject("Text Area");
        textArea.transform.SetParent(inputGo.transform, false);
        RectTransform taRect = textArea.AddComponent<RectTransform>();
        taRect.anchorMin = Vector2.zero;
        taRect.anchorMax = Vector2.one;
        taRect.sizeDelta = new Vector2(-20, -10);

        // Input Placeholder
        GameObject phGo = new GameObject("Placeholder");
        phGo.transform.SetParent(textArea.transform, false);
        RectTransform phRect = phGo.AddComponent<RectTransform>();
        phRect.anchorMin = Vector2.zero;
        phRect.anchorMax = Vector2.one;
        phRect.sizeDelta = Vector2.zero;
        TextMeshProUGUI phText = phGo.AddComponent<TextMeshProUGUI>();
        phText.text = "Ketik pertanyaan interogasimu... (Tekan Enter untuk kirim)";
        phText.fontSize = 19;
        phText.fontStyle = FontStyles.Italic;
        phText.color = new Color(0.55f, 0.60f, 0.68f);

        // Input Main Text
        GameObject inputTextGo = new GameObject("Text");
        inputTextGo.transform.SetParent(textArea.transform, false);
        RectTransform itRect = inputTextGo.AddComponent<RectTransform>();
        itRect.anchorMin = Vector2.zero;
        itRect.anchorMax = Vector2.one;
        itRect.sizeDelta = Vector2.zero;
        TextMeshProUGUI mainInputText = inputTextGo.AddComponent<TextMeshProUGUI>();
        mainInputText.fontSize = 19;
        mainInputText.color = Color.white;

        TMP_InputField inputField = inputGo.AddComponent<TMP_InputField>();
        inputField.textViewport = taRect;
        inputField.textComponent = mainInputText;
        inputField.placeholder = phText;
        inputField.fontAsset = mainInputText.font;

        // Wire references to ChatUI
        chatUI.chatPanel = panelGo;
        chatUI.chatText = dialogText;
        chatUI.inputField = inputField;

        panelGo.SetActive(false); // Sembunyikan saat mulai, aktif saat bicara (tombol E)

        return chatUI;
    }

    private static void SetupAtmosphere()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.12f, 0.14f, 0.18f); // Dark Slate Noir
        RenderSettings.fog = false;
        RenderSettings.skybox = null;
    }

    private static void EnsureMeshOrBoxCollider(GameObject target)
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

    private static Material CreateSimpleMaterial(string matName, Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");

        Material mat = new Material(shader);
        mat.name = matName;
        
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);
        else if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", color);

        if (mat.HasProperty("_Smoothness"))
            mat.SetFloat("_Smoothness", 0.15f);
        else if (mat.HasProperty("_Glossiness"))
            mat.SetFloat("_Glossiness", 0.15f);

        mat.SetFloat("_Metallic", 0.05f);
        return mat;
    }

    private static void FixURPMaterials(GameObject target)
    {
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null) return;

        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in renderers)
        {
            Material[] mats = r.sharedMaterials;
            bool changed = false;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] != null && mats[i].shader != null && (mats[i].shader.name == "Standard" || mats[i].shader.name == "Standard (Specular setup)"))
                {
                    Material newMat = new Material(mats[i]);
                    newMat.shader = urpLit;
                    // Transfer color if possible
                    if (mats[i].HasProperty("_Color") && newMat.HasProperty("_BaseColor"))
                    {
                        newMat.SetColor("_BaseColor", mats[i].GetColor("_Color"));
                    }
                    mats[i] = newMat;
                    changed = true;
                }
            }
            if (changed)
            {
                r.sharedMaterials = mats;
            }
        }
    }

    private static Material CreateGlassMaterial(string matName, Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");

        Material mat = new Material(shader);
        mat.name = matName;
        
        if (mat.HasProperty("_BaseColor"))
        {
            mat.SetColor("_BaseColor", color);
            mat.SetFloat("_Surface", 1); // Transparent in URP
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }
        else
        {
            mat.color = color;
            mat.SetFloat("_Mode", 3); // Transparent
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHABLEND_ON");
            mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
        }

        if (mat.HasProperty("_Smoothness"))
            mat.SetFloat("_Smoothness", 0.85f);
        else if (mat.HasProperty("_Glossiness"))
            mat.SetFloat("_Glossiness", 0.85f);

        mat.SetFloat("_Metallic", 0.4f);
        return mat;
    }
}
#endif
