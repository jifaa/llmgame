#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// CityBuilder Pro: Membuat kota modular yang rapi, terstruktur, tanpa jalan buntu,
/// persis seperti tata kota pada referensi (blok pulau dengan trotoar, variasi atap oranye/biru,
/// plaza taman terbuka, deretan toko, area parkir, zebra cross, dan lampu jalan).
/// </summary>
public class CityBuilder : EditorWindow
{
    [MenuItem("Tools/LLM Game/Build City Expansion")]
    public static void Open()
    {
        var win = GetWindow<CityBuilder>("City Builder Pro");
        win.minSize = new Vector2(440, 680);
        win.Show();
    }

    [Header("1. Dimensi & Grid Kota")]
    [Tooltip("Jumlah blok kota ke arah X")]
    public int gridX = 3;
    [Tooltip("Jumlah blok kota ke arah Z")]
    public int gridZ = 3;
    [Tooltip("Panjang tiap blok kota (sesuai panjang 2 segmen jalan = 44m)")]
    public float blockSize = 44f;
    [Tooltip("Lebar jalan raya (meter)")]
    public float roadWidth = 7.5f;

    [Header("2. Posisi Pusat Kota")]
    public float originX = 0f;
    public float originZ = 135f; // Aman dari area investigasi NPC awal (z = -8 s/d 42)
    public float roadY = 0.02f;

    [Header("3. Opsi Desain & Estetika")]
    public bool addZebraCrossings = true;
    public bool addStreetLightsAndSignals = true;
    public bool addTreesAndNature = true;
    public bool addParkedVehicles = true;
    public bool addSidewalkIslandPlates = true;
    public bool addAsphaltBase = true;

    [Header("4. Aset Prefab (Otomatis Dimuat)")]
    public GameObject roadStraight;
    public GameObject roadCrossroad;
    public GameObject roadCrosswalk;
    public GameObject streetLightPrefab;
    public GameObject trafficLightPrefab;

    public List<GameObject> buildingPrefabs = new List<GameObject>();
    public List<GameObject> treePrefabs = new List<GameObject>();
    public List<GameObject> propPrefabs = new List<GameObject>();
    public List<GameObject> vehiclePrefabs = new List<GameObject>();

    private Vector2 scrollPos;
    private SerializedObject so;

    const string URBAN_PATH = "Assets/Low Poly Simple Urban City 3D Asset Pack/Prefabs";
    const string SIMPLEPOLY_PATH = "Assets/SimplePoly City - Low Poly Assets/Prefab";

    void OnEnable()
    {
        so = new SerializedObject(this);
        LoadAssets();
        FixAllHDRPMaterials();
    }

    void LoadAssets()
    {
        // 1. Roads
        if (roadStraight == null)
            roadStraight = AssetDatabase.LoadAssetAtPath<GameObject>($"{URBAN_PATH}/Roads/Road_1.prefab");
        if (roadCrossroad == null)
            roadCrossroad = AssetDatabase.LoadAssetAtPath<GameObject>($"{URBAN_PATH}/Roads/Road_Crossroads_1.prefab");
        if (roadCrosswalk == null)
            roadCrosswalk = AssetDatabase.LoadAssetAtPath<GameObject>($"{URBAN_PATH}/Roads/Road_Crosswalk.prefab");

        // 2. Lights
        if (streetLightPrefab == null)
            streetLightPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{URBAN_PATH}/Props/Road_Signs/Light.prefab");
        if (trafficLightPrefab == null)
            trafficLightPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{URBAN_PATH}/Props/Road_Signs/Traffic_Light_1.prefab");

        // 3. Buildings (Gedung atap terracotta, biru, toko, dll)
        if (buildingPrefabs.Count == 0)
        {
            for (int i = 1; i <= 9; i++)
            {
                var b = AssetDatabase.LoadAssetAtPath<GameObject>($"{URBAN_PATH}/Buildings/Building_{i}.prefab");
                if (b != null) buildingPrefabs.Add(b);
            }

            string[] extra = new string[] {
                "Assets/Studio Horizon/Simple Building Generic Free/Prefabs/Market.prefab",
                "Assets/Studio Horizon/Simple Building Generic Free/Prefabs/Hospital.prefab",
                "Assets/Studio Horizon/Simple Building Generic Free/Prefabs/Police_Station.prefab",
                "Assets/Studio Horizon/Simple Building Generic Free/Prefabs/School.prefab",
                "Assets/Studio Horizon/Simple Building Generic Free/Prefabs/Workshop.prefab",
                "Assets/Studio Horizon/Simple Building Generic Free/Prefabs/Fire_Station.prefab"
            };
            foreach (var p in extra)
            {
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(p);
                if (go != null) buildingPrefabs.Add(go);
            }
        }

        // 4. Trees
        if (treePrefabs.Count == 0)
        {
            var t1 = AssetDatabase.LoadAssetAtPath<GameObject>($"{URBAN_PATH}/Props/Other/Tree_1.prefab");
            var t2 = AssetDatabase.LoadAssetAtPath<GameObject>($"{URBAN_PATH}/Props/Other/Tree_2.prefab");
            if (t1 != null) treePrefabs.Add(t1);
            if (t2 != null) treePrefabs.Add(t2);
        }

        // 5. Props (Trotoar, Halte, Hidran, Bangku, Stan Makanan)
        if (propPrefabs.Count == 0)
        {
            string[] props = new string[] {
                $"{URBAN_PATH}/Props/Other/Hydrant.prefab",
                $"{URBAN_PATH}/Props/Other/Bus Stop.prefab",
                $"{URBAN_PATH}/Props/Other/Trash_Big.prefab",
                $"{URBAN_PATH}/Props/Other/Trash_Small.prefab",
                $"{URBAN_PATH}/Props/Other/Newspaper_Stand.prefab",
                $"{URBAN_PATH}/Props/Other/Hot_Dog.prefab",
                $"{URBAN_PATH}/Props/Other/Barrier.prefab",
                $"{URBAN_PATH}/Props/Boxes/Box_1.prefab"
            };
            foreach (var p in props)
            {
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(p);
                if (go != null) propPrefabs.Add(go);
            }
        }

        // 6. Vehicles
        if (vehiclePrefabs.Count == 0)
        {
            string[] vehicles = new string[] {
                $"{URBAN_PATH}/Vehicles/Cars/Car_1_1.prefab",
                $"{URBAN_PATH}/Vehicles/Cars/Car_1_2.prefab",
                $"{URBAN_PATH}/Vehicles/Cars/Car_2_1.prefab",
                $"{URBAN_PATH}/Vehicles/Cars/Car_3_1.prefab",
                $"{URBAN_PATH}/Vehicles/Cars/Car_3_2.prefab",
                $"{URBAN_PATH}/Vehicles/Pickups/Pickup_1_1.prefab",
                $"{URBAN_PATH}/Vehicles/Emergency_Vehicles/Police_Car.prefab"
            };
            foreach (var v in vehicles)
            {
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(v);
                if (go != null) vehiclePrefabs.Add(go);
            }
        }
    }

    void OnGUI()
    {
        so.Update();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("🏙️ City Builder Pro — Layout Referensi", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Membuat tata kota modular yang rapi seperti layout referensi:\n" +
            "• Sirkuit jalan tersambung penuh (100% tanpa jalan buntu)\n" +
            "• Blok pulau dengan pulau trotoar & variasi arsitektur\n" +
            "• Plaza terbuka dengan pohon rindang, zebra cross, dan mobil parkir", MessageType.Info);
        EditorGUILayout.Space(6);

        // Group 1: Ukuran & Posisi
        EditorGUILayout.LabelField("1. Pengaturan Grid & Posisi", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        gridX = EditorGUILayout.IntSlider("Jumlah Blok X", gridX, 2, 5);
        gridZ = EditorGUILayout.IntSlider("Jumlah Blok Z", gridZ, 2, 5);
        blockSize = EditorGUILayout.Slider("Ukuran Blok (Meter)", blockSize, 36f, 52f);
        originX = EditorGUILayout.FloatField("Pusat Kota X", originX);
        originZ = EditorGUILayout.FloatField("Pusat Kota Z", originZ);
        EditorGUILayout.HelpBox($"Pusat z={originZ} aman dari NPC & area TKP utama (z = -8 s/d 42).", MessageType.None);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(8);

        // Group 2: Opsi Fitur
        EditorGUILayout.LabelField("2. Fitur & Komponen Kota", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        addZebraCrossings = EditorGUILayout.Toggle("Zebra Cross di Persimpangan", addZebraCrossings);
        addStreetLightsAndSignals = EditorGUILayout.Toggle("Lampu Jalan & Traffic Light", addStreetLightsAndSignals);
        addTreesAndNature = EditorGUILayout.Toggle("Pohon Rindang & Taman", addTreesAndNature);
        addParkedVehicles = EditorGUILayout.Toggle("Mobil Parkir di Bahu Jalan", addParkedVehicles);
        addSidewalkIslandPlates = EditorGUILayout.Toggle("Alas Trotoar Tiap Blok (Islands)", addSidewalkIslandPlates);
        addAsphaltBase = EditorGUILayout.Toggle("Lantai Aspal Dasar (Ground Base)", addAsphaltBase);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(8);

        // Group 3: Prefabs
        EditorGUILayout.LabelField("3. Prefab Aset", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        roadStraight = (GameObject)EditorGUILayout.ObjectField("Jalan Lurus", roadStraight, typeof(GameObject), false);
        roadCrossroad = (GameObject)EditorGUILayout.ObjectField("Persimpangan (Crossroads)", roadCrossroad, typeof(GameObject), false);
        roadCrosswalk = (GameObject)EditorGUILayout.ObjectField("Zebra Cross", roadCrosswalk, typeof(GameObject), false);
        streetLightPrefab = (GameObject)EditorGUILayout.ObjectField("Lampu Jalan", streetLightPrefab, typeof(GameObject), false);
        trafficLightPrefab = (GameObject)EditorGUILayout.ObjectField("Lampu Lalu Lintas", trafficLightPrefab, typeof(GameObject), false);

        SerializedProperty bList = so.FindProperty("buildingPrefabs");
        EditorGUILayout.PropertyField(bList, new GUIContent($"Daftar Gedung ({buildingPrefabs.Count})"), true);

        SerializedProperty tList = so.FindProperty("treePrefabs");
        EditorGUILayout.PropertyField(tList, new GUIContent($"Daftar Pohon ({treePrefabs.Count})"), true);

        SerializedProperty pList = so.FindProperty("propPrefabs");
        EditorGUILayout.PropertyField(pList, new GUIContent($"Daftar Properti ({propPrefabs.Count})"), true);

        SerializedProperty vList = so.FindProperty("vehiclePrefabs");
        EditorGUILayout.PropertyField(vList, new GUIContent($"Daftar Kendaraan ({vehiclePrefabs.Count})"), true);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(12);

        // Tombol Aksi
        GUI.backgroundColor = new Color(0.25f, 0.8f, 0.45f);
        if (GUILayout.Button("🚀 BUILD KOTA METROPOLITAN", GUILayout.Height(44)))
        {
            BuildCity();
        }

        GUI.backgroundColor = new Color(0.9f, 0.4f, 0.4f);
        if (GUILayout.Button("🗑️ Hapus Kota (Clear CityExpansion)", GUILayout.Height(28)))
        {
            ClearCity();
        }

        GUI.backgroundColor = new Color(0.4f, 0.7f, 1f);
        if (GUILayout.Button("✨ Perbaiki Semua Tekstur (HDRP Auto-Fix)", GUILayout.Height(28)))
        {
            FixAllHDRPMaterials();
        }

        GUI.backgroundColor = Color.white;
        EditorGUILayout.Space(10);
        EditorGUILayout.EndScrollView();
        so.ApplyModifiedProperties();
    }

    public static void FixAllHDRPMaterials()
    {
        Shader hdrpLit = Shader.Find("HDRP/Lit");
        if (hdrpLit == null) return;

        string[] guids = AssetDatabase.FindAssets("t:Material", new[] {
            "Assets/Low Poly Simple Urban City 3D Asset Pack",
            "Assets/SimplePoly City - Low Poly Assets",
            "Assets/Studio Horizon"
        });

        int count = 0;
        foreach (string g in guids)
        {
            string p = AssetDatabase.GUIDToAssetPath(g);
            Material m = AssetDatabase.LoadAssetAtPath<Material>(p);
            if (m == null) continue;

            if (m.shader != hdrpLit && (m.shader.name.Contains("Standard") || m.shader.name.Contains("Legacy") || m.shader.name.Contains("Diffuse") || m.shader.name.Contains("Error")))
            {
                Texture tex = m.mainTexture ?? (m.HasProperty("_MainTex") ? m.GetTexture("_MainTex") : null);
                Color col = m.HasProperty("_Color") ? m.GetColor("_Color") : Color.white;

                m.shader = hdrpLit;
                if (tex != null) m.SetTexture("_BaseColorMap", tex);
                m.SetColor("_BaseColor", col);

                EditorUtility.SetDirty(m);
                count++;
            }
        }
        AssetDatabase.SaveAssets();
        if (count > 0)
            Debug.Log($"<color=green>[CityBuilder] Sukses mengonversi {count} material ke HDRP/Lit.</color>");
    }

    public static void ClearCity()
    {
        var old = GameObject.Find("CityExpansion");
        if (old != null)
        {
            Undo.DestroyObjectImmediate(old);
            Debug.Log("[CityBuilder] Objek 'CityExpansion' lama berhasil dibersihkan.");
        }
    }

    void BuildCity()
    {
        FixAllHDRPMaterials();
        ClearCity();

        if (roadStraight == null || buildingPrefabs.Count == 0)
        {
            Debug.LogError("[CityBuilder] Road atau Building Prefab belum terpasang!");
            return;
        }

        GameObject root = new GameObject("CityExpansion");
        Undo.RegisterCreatedObjectUndo(root, "Build Layout City");

        GameObject roadsParent = new GameObject("01_Roads_Network");
        roadsParent.transform.SetParent(root.transform);

        GameObject blocksParent = new GameObject("02_City_Blocks");
        blocksParent.transform.SetParent(root.transform);

        GameObject propsParent = new GameObject("03_Street_Props");
        propsParent.transform.SetParent(root.transform);

        GameObject vehiclesParent = new GameObject("04_Vehicles");
        vehiclesParent.transform.SetParent(root.transform);

        float totalW = gridX * blockSize;
        float totalH = gridZ * blockSize;
        float halfW = totalW * 0.5f;
        float halfH = totalH * 0.5f;
        Vector3 center = new Vector3(originX, roadY, originZ);

        // Panjang segmen jalan standar (22 meter)
        float roadUnit = 22f;

        // =========================================================================
        // 1. JARINGAN JALAN RAYA TERSAMBUNG PENUH (100% LOOP - TANPA JALAN BUNTU)
        // =========================================================================

        // A. Persimpangan Jalan (Crossroads) di setiap titik sudut grid
        for (int zi = 0; zi <= gridZ; zi++)
        {
            float zPos = center.z - halfH + zi * blockSize;
            for (int xi = 0; xi <= gridX; xi++)
            {
                float xPos = center.x - halfW + xi * blockSize;
                Vector3 crossPos = new Vector3(xPos, roadY, zPos);

                GameObject cross = (GameObject)PrefabUtility.InstantiatePrefab(roadCrossroad != null ? roadCrossroad : roadStraight);
                cross.name = $"Crossroad_{xi}_{zi}";
                cross.transform.SetParent(roadsParent.transform);
                cross.transform.position = crossPos;
                cross.transform.rotation = Quaternion.identity;
                Undo.RegisterCreatedObjectUndo(cross, "cross");

                // Lampu Lalu Lintas di Sudut Persimpangan
                if (addStreetLightsAndSignals && trafficLightPrefab != null)
                {
                    GameObject tl = (GameObject)PrefabUtility.InstantiatePrefab(trafficLightPrefab);
                    tl.transform.SetParent(propsParent.transform);
                    tl.transform.position = crossPos + new Vector3(roadWidth * 0.6f, 0, roadWidth * 0.6f);
                    tl.transform.rotation = Quaternion.Euler(0, 45, 0);
                    Undo.RegisterCreatedObjectUndo(tl, "traffic_light");
                }
            }
        }

        // B. Jalan Lurus & Zebra Cross Horizontal (Sumbu X)
        for (int zi = 0; zi <= gridZ; zi++)
        {
            float zPos = center.z - halfH + zi * blockSize;
            for (int xi = 0; xi < gridX; xi++)
            {
                float startX = center.x - halfW + xi * blockSize;
                float endX = center.x - halfW + (xi + 1) * blockSize;
                float midX = (startX + endX) * 0.5f;

                // Segmen Kiri (dengan Zebra Cross di dekat persimpangan)
                GameObject rLeft = (GameObject)PrefabUtility.InstantiatePrefab(addZebraCrossings && roadCrosswalk != null ? roadCrosswalk : roadStraight);
                rLeft.transform.SetParent(roadsParent.transform);
                rLeft.transform.position = new Vector3(startX + (blockSize * 0.25f), roadY, zPos);
                rLeft.transform.rotation = Quaternion.identity;
                Undo.RegisterCreatedObjectUndo(rLeft, "road_hx_left");

                // Segmen Kanan
                GameObject rRight = (GameObject)PrefabUtility.InstantiatePrefab(roadStraight);
                rRight.transform.SetParent(roadsParent.transform);
                rRight.transform.position = new Vector3(startX + (blockSize * 0.75f), roadY, zPos);
                rRight.transform.rotation = Quaternion.identity;
                Undo.RegisterCreatedObjectUndo(rRight, "road_hx_right");

                // Lampu Jalan di Tengah Blok
                if (addStreetLightsAndSignals && streetLightPrefab != null)
                {
                    GameObject light = (GameObject)PrefabUtility.InstantiatePrefab(streetLightPrefab);
                    light.transform.SetParent(propsParent.transform);
                    light.transform.position = new Vector3(midX, roadY, zPos + (roadWidth * 0.52f));
                    light.transform.rotation = Quaternion.Euler(0, 180, 0);
                    Undo.RegisterCreatedObjectUndo(light, "street_light_hx");
                }
            }
        }

        // C. Jalan Lurus & Zebra Cross Vertikal (Sumbu Z)
        for (int xi = 0; xi <= gridX; xi++)
        {
            float xPos = center.x - halfW + xi * blockSize;
            for (int zi = 0; zi < gridZ; zi++)
            {
                float startZ = center.z - halfH + zi * blockSize;
                float endZ = center.z - halfH + (zi + 1) * blockSize;
                float midZ = (startZ + endZ) * 0.5f;

                // Segmen Bawah
                GameObject rDown = (GameObject)PrefabUtility.InstantiatePrefab(addZebraCrossings && roadCrosswalk != null ? roadCrosswalk : roadStraight);
                rDown.transform.SetParent(roadsParent.transform);
                rDown.transform.position = new Vector3(xPos, roadY, startZ + (blockSize * 0.25f));
                rDown.transform.rotation = Quaternion.Euler(0, 90, 0);
                Undo.RegisterCreatedObjectUndo(rDown, "road_vz_down");

                // Segmen Atas
                GameObject rUp = (GameObject)PrefabUtility.InstantiatePrefab(roadStraight);
                rUp.transform.SetParent(roadsParent.transform);
                rUp.transform.position = new Vector3(xPos, roadY, startZ + (blockSize * 0.75f));
                rUp.transform.rotation = Quaternion.Euler(0, 90, 0);
                Undo.RegisterCreatedObjectUndo(rUp, "road_vz_up");

                // Lampu Jalan di Sisi Kanan Jalan
                if (addStreetLightsAndSignals && streetLightPrefab != null)
                {
                    GameObject light = (GameObject)PrefabUtility.InstantiatePrefab(streetLightPrefab);
                    light.transform.SetParent(propsParent.transform);
                    light.transform.position = new Vector3(xPos + (roadWidth * 0.52f), roadY, midZ);
                    light.transform.rotation = Quaternion.Euler(0, -90, 0);
                    Undo.RegisterCreatedObjectUndo(light, "street_light_vz");
                }
            }
        }

        // =========================================================================
        // 2. BLOK-BLOK KOTA TERSTRUKTUR (SESUAI REFERENSI GAMBAR)
        // =========================================================================
        int buildingIndex = 0;
        int blockCount = 0;

        for (int gx = 0; gx < gridX; gx++)
        {
            for (int gz = 0; gz < gridZ; gz++)
            {
                float bCenterX = center.x - halfW + (gx + 0.5f) * blockSize;
                float bCenterZ = center.z - halfH + (gz + 0.5f) * blockSize;
                Vector3 bCenter = new Vector3(bCenterX, roadY, bCenterZ);

                GameObject blockIsland = new GameObject($"CityBlock_{gx}_{gz}");
                blockIsland.transform.SetParent(blocksParent.transform);
                blockIsland.transform.position = bCenter;

                // Alas Pulau Trotoar (Sidewalk Island Base)
                if (addSidewalkIslandPlates)
                {
                    GameObject sidewalk = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    sidewalk.name = "Sidewalk_Curb_Plate";
                    sidewalk.transform.SetParent(blockIsland.transform);
                    sidewalk.transform.position = bCenter + new Vector3(0, 0.04f, 0);
                    float innerScale = (blockSize - roadWidth) / 10f;
                    sidewalk.transform.localScale = new Vector3(innerScale, 1f, innerScale);

                    Material sidewalkMat = AssetDatabase.LoadAssetAtPath<Material>($"{URBAN_PATH}/../Materials/tex.mat");
                    if (sidewalkMat != null)
                        sidewalk.GetComponent<Renderer>().sharedMaterial = sidewalkMat;
                }

                // Variasi Tipe Blok sesuai referensi gambar:
                int archetype = (gx + gz * 2) % 4;

                switch (archetype)
                {
                    case 0: // Tipe A: Komplek Rumah Residensial dengan Halaman Dalam & Mobil
                        BuildResidentialCourtyardBlock(bCenter, blockIsland.transform, ref buildingIndex);
                        break;

                    case 1: // Tipe B: Plaza Taman Terbuka dengan Pohon Rindang (Seperti di gambar tengah)
                        BuildPlazaParkBlock(bCenter, blockIsland.transform);
                        break;

                    case 2: // Tipe C: Deretan Toko Komersial L-Shape + Parkir Mobil Sisi Jalan
                        BuildCommercialShoppingBlock(bCenter, blockIsland.transform, ref buildingIndex);
                        break;

                    case 3: // Tipe D: Monumen / Paviliun Plaza Pusat & Pohon Sudut
                        BuildCivicSquareBlock(bCenter, blockIsland.transform, ref buildingIndex);
                        break;
                }

                blockCount++;
            }
        }

        // =========================================================================
        // 3. LANTAI ASPAL DASAR KOTA
        // =========================================================================
        if (addAsphaltBase)
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Asphalt_Ground_Base";
            ground.transform.SetParent(root.transform);
            ground.transform.position = new Vector3(center.x, -0.05f, center.z);
            ground.transform.localScale = new Vector3((totalW + 40f) / 10f, 1f, (totalH + 40f) / 10f);

            Material groundMat = AssetDatabase.LoadAssetAtPath<Material>($"{URBAN_PATH}/../Materials/tex.mat");
            if (groundMat != null)
                ground.GetComponent<Renderer>().sharedMaterial = groundMat;

            Undo.RegisterCreatedObjectUndo(ground, "ground");
        }

        Debug.Log($"<color=green>[CityBuilder Pro] Sukses membangun kota metropolitan {gridX}x{gridZ} blok ({blockCount} distrik)! Sirkuit jalan tersambung 100% tanpa jalan buntu.</color>");
    }

    // --- TIPE A: RESIDENTIAL COURTYARD BLOCK ---
    void BuildResidentialCourtyardBlock(Vector3 center, Transform parent, ref int bIndex)
    {
        float d = blockSize * 0.24f;

        // 3 Rumah menghadap jalan luar
        SpawnBuilding(center + new Vector3(-d, 0, -d), 0f, parent, ref bIndex);
        SpawnBuilding(center + new Vector3(d, 0, -d), 0f, parent, ref bIndex);
        SpawnBuilding(center + new Vector3(0, 0, d), 180f, parent, ref bIndex);

        // Pohon di halaman dalam
        if (addTreesAndNature && treePrefabs.Count > 0)
        {
            SpawnPrefab(treePrefabs, center + new Vector3(-d * 0.8f, 0, d * 0.5f), Quaternion.identity, parent);
            SpawnPrefab(treePrefabs, center + new Vector3(d * 0.8f, 0, d * 0.5f), Quaternion.identity, parent);
        }

        // Mobil Parkir di halaman samping
        if (addParkedVehicles && vehiclePrefabs.Count > 0)
        {
            SpawnPrefab(vehiclePrefabs, center + new Vector3(d * 0.7f, 0, 0), Quaternion.Euler(0, 90, 0), parent);
        }
    }

    // --- TIPE B: PLAZA PARK BLOCK (Taman Hijau & Pepohonan) ---
    void BuildPlazaParkBlock(Vector3 center, Transform parent)
    {
        float d = blockSize * 0.26f;

        // Lantai Rumput Taman
        GameObject grass = GameObject.CreatePrimitive(PrimitiveType.Plane);
        grass.name = "Park_Grass_Area";
        grass.transform.SetParent(parent);
        grass.transform.position = center + new Vector3(0, 0.05f, 0);
        grass.transform.localScale = new Vector3(blockSize * 0.065f, 1f, blockSize * 0.065f);

        // Klaster Pohon Rindang (4-6 Pohon melingkar)
        if (addTreesAndNature && treePrefabs.Count > 0)
        {
            Vector3[] treeOffsets = new Vector3[] {
                new Vector3(-d, 0, -d),
                new Vector3(d, 0, -d),
                new Vector3(-d, 0, d),
                new Vector3(d, 0, d),
                new Vector3(0, 0, d * 1.1f),
                new Vector3(0, 0, -d * 1.1f)
            };
            foreach (var offset in treeOffsets)
            {
                SpawnPrefab(treePrefabs, center + offset, Quaternion.Euler(0, Random.Range(0, 360), 0), parent);
            }
        }

        // Lampu Taman & Bangku / Halte di Sisi Taman
        if (streetLightPrefab != null)
        {
            GameObject lamp = (GameObject)PrefabUtility.InstantiatePrefab(streetLightPrefab);
            lamp.transform.SetParent(parent);
            lamp.transform.position = center + new Vector3(0, 0.05f, 0);
        }

        if (propPrefabs.Count > 0)
        {
            SpawnPrefab(propPrefabs, center + new Vector3(d * 1.1f, 0, 0), Quaternion.Euler(0, -90, 0), parent);
        }
    }

    // --- TIPE C: COMMERCIAL SHOPS & PARKING BAY ---
    void BuildCommercialShoppingBlock(Vector3 center, Transform parent, ref int bIndex)
    {
        float d = blockSize * 0.24f;

        // Pertokoan L-Shape (Menghadap Barat & Selatan)
        SpawnBuilding(center + new Vector3(-d, 0, -d), 0f, parent, ref bIndex);
        SpawnBuilding(center + new Vector3(-d, 0, d), 90f, parent, ref bIndex);
        SpawnBuilding(center + new Vector3(d, 0, d), 180f, parent, ref bIndex);

        // Area Parkir Mobil di Sisi Timur (seperti di gambar referensi)
        if (addParkedVehicles && vehiclePrefabs.Count > 0)
        {
            SpawnPrefab(vehiclePrefabs, center + new Vector3(d, 0, -d * 0.5f), Quaternion.Euler(0, 90, 0), parent);
            SpawnPrefab(vehiclePrefabs, center + new Vector3(d, 0, d * 0.3f), Quaternion.Euler(0, 90, 0), parent);
        }

        // Pohon di Sudut Trotoar
        if (addTreesAndNature && treePrefabs.Count > 0)
        {
            SpawnPrefab(treePrefabs, center + new Vector3(d * 0.9f, 0, -d * 1.1f), Quaternion.identity, parent);
        }

        // Stan Makanan / Hidran di Depan Toko
        if (propPrefabs.Count > 0)
        {
            SpawnPrefab(propPrefabs, center + new Vector3(-d * 1.2f, 0, 0), Quaternion.Euler(0, 90, 0), parent);
        }
    }

    // --- TIPE D: CIVIC SQUARE / PAVILION BLOCK ---
    void BuildCivicSquareBlock(Vector3 center, Transform parent, ref int bIndex)
    {
        float d = blockSize * 0.24f;

        // 2 Gedung Besar Berhadapan
        SpawnBuilding(center + new Vector3(0, 0, -d), 0f, parent, ref bIndex);
        SpawnBuilding(center + new Vector3(0, 0, d), 180f, parent, ref bIndex);

        // Barisan Pohon Hijau di Sisi Kiri & Kanan (Greenery Belts)
        if (addTreesAndNature && treePrefabs.Count > 0)
        {
            SpawnPrefab(treePrefabs, center + new Vector3(-d * 1.1f, 0, -d * 0.6f), Quaternion.identity, parent);
            SpawnPrefab(treePrefabs, center + new Vector3(-d * 1.1f, 0, d * 0.6f), Quaternion.identity, parent);
            SpawnPrefab(treePrefabs, center + new Vector3(d * 1.1f, 0, 0), Quaternion.identity, parent);
        }

        // Properti / Lampu di Tengah Plaza
        if (streetLightPrefab != null)
        {
            GameObject lamp = (GameObject)PrefabUtility.InstantiatePrefab(streetLightPrefab);
            lamp.transform.SetParent(parent);
            lamp.transform.position = center + new Vector3(d * 0.5f, 0, 0);
            lamp.transform.rotation = Quaternion.Euler(0, 90, 0);
        }
    }

    void SpawnBuilding(Vector3 pos, float yRotation, Transform parent, ref int index)
    {
        if (buildingPrefabs == null || buildingPrefabs.Count == 0) return;

        var prefab = buildingPrefabs[index % buildingPrefabs.Count];
        index++;

        GameObject b = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        b.transform.SetParent(parent);
        b.transform.position = pos;
        b.transform.rotation = Quaternion.Euler(0, yRotation, 0);
        Undo.RegisterCreatedObjectUndo(b, "Building");
    }

    void SpawnPrefab(List<GameObject> list, Vector3 pos, Quaternion rot, Transform parent)
    {
        if (list == null || list.Count == 0) return;
        var p = list[Random.Range(0, list.Count)];
        if (p == null) return;

        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(p);
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.rotation = rot;
        Undo.RegisterCreatedObjectUndo(go, go.name);
    }
}
#endif
