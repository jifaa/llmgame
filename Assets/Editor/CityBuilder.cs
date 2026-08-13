#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// CityBuilder Pro: Membuat kota modular yang presisi tanpa overlap, 
/// dengan sistem zonasi 6 distrik tematik, dan terstruktur sempurna.
/// </summary>
public class CityBuilder : EditorWindow
{
    [MenuItem("Tools/LLM Game/Build City Expansion")]
    public static void Open()
    {
        var win = GetWindow<CityBuilder>("City Builder Pro");
        win.minSize = new Vector2(480, 720);
        win.Show();
    }

    public enum CityPreset
    {
        MetropolisSeimbang,
        PusatBisnisKomersial,
        KawasanResidensial,
        PusatLayananPublik,
        KawasanIndustri
    }

    [Header("1. Dimensi & Grid Kota")]
    public CityPreset presetKota = CityPreset.MetropolisSeimbang;
    public int gridX = 3;
    public int gridZ = 3;
    
    [Tooltip("Panjang blok (44 = 1 segmen antar simpang, 66 = 2 segmen, 88 = 3 segmen)")]
    public float blockSize = 66f; 
    public float roadWidth = 7.5f;

    [Header("2. Posisi Pusat Kota")]
    public float originX = 0f;
    public float originZ = 135f; 
    public float roadY = 0.02f;
    public float curbHeight = 0.12f; // Tinggi trotoar

    [Header("3. Opsi Desain & Estetika")]
    public bool addZebraCrossings = true;
    public bool addStreetLightsAndSignals = true;
    public bool addTreesAndNature = true;
    public bool addParkedVehicles = true;
    public bool autoAddColliders = true;
    public bool addAsphaltBase = true;

    // --- ASET PREFABS ---
    private GameObject roadStraight, roadCrossroad, roadCrosswalk, roadManhole;
    private GameObject streetLightPrefab, trafficLightPrefab;
    
    private List<GameObject> commercialBuildings = new List<GameObject>();
    private List<GameObject> civicBuildings = new List<GameObject>();
    private List<GameObject> residentialBuildings = new List<GameObject>();
    private List<GameObject> industrialBuildings = new List<GameObject>();
    private List<GameObject> highRiseBuildings = new List<GameObject>();
    
    private List<GameObject> streetProps = new List<GameObject>();
    private List<GameObject> parkProps = new List<GameObject>();
    private List<GameObject> natureTrees = new List<GameObject>();
    private List<GameObject> natureBushes = new List<GameObject>();
    
    private List<GameObject> civilianVehicles = new List<GameObject>();
    private List<GameObject> civicVehicles = new List<GameObject>();

    private Vector2 scrollPos;
    private SerializedObject so;

    const string URBAN_PATH = "Assets/Low Poly Simple Urban City 3D Asset Pack/Prefabs";
    const string SIMPLEPOLY_PATH = "Assets/SimplePoly City - Low Poly Assets/Prefab";
    const string STUDIO_PATH = "Assets/Studio Horizon/Simple Building Generic Free/Prefabs";

    void OnEnable()
    {
        so = new SerializedObject(this);
        LoadAllAssets();
        FixAllHDRPMaterials();
    }

    void LoadAllAssets()
    {
        // 1. Roads
        roadStraight = LoadPrefab($"{URBAN_PATH}/Roads/Road_1.prefab");
        roadCrossroad = LoadPrefab($"{URBAN_PATH}/Roads/Road_Crossroads_1.prefab");
        roadCrosswalk = LoadPrefab($"{URBAN_PATH}/Roads/Road_Crosswalk.prefab");
        roadManhole = LoadPrefab($"{URBAN_PATH}/Roads/Road_Manhole_1.prefab");

        // 2. Lights
        streetLightPrefab = LoadPrefab($"{URBAN_PATH}/Props/Road_Signs/Light.prefab");
        trafficLightPrefab = LoadPrefab($"{URBAN_PATH}/Props/Road_Signs/Traffic_Light_1.prefab");

        // 3. Buildings Categorized
        LoadPrefabsIntoList(commercialBuildings, new string[] {
            $"{SIMPLEPOLY_PATH}/Buildings/Building_Bakery.prefab",
            $"{SIMPLEPOLY_PATH}/Buildings/Building_Coffee Shop.prefab",
            $"{SIMPLEPOLY_PATH}/Buildings/Building_Fast Food.prefab",
            $"{SIMPLEPOLY_PATH}/Buildings/Building_Restaurant.prefab",
            $"{SIMPLEPOLY_PATH}/Buildings/Building_Super Market.prefab",
            $"{SIMPLEPOLY_PATH}/Buildings/Building_Books Shop.prefab",
            $"{SIMPLEPOLY_PATH}/Buildings/Building_Clothing.prefab",
            $"{URBAN_PATH}/Buildings/Building_2.prefab",
            $"{URBAN_PATH}/Buildings/Building_4.prefab",
            $"{URBAN_PATH}/Buildings/Building_5.prefab",
            $"{URBAN_PATH}/Buildings/Building_6.prefab"
        });

        LoadPrefabsIntoList(civicBuildings, new string[] {
            $"{STUDIO_PATH}/Police_Station.prefab",
            $"{STUDIO_PATH}/Hospital.prefab",
            $"{STUDIO_PATH}/Fire_Station.prefab",
            $"{STUDIO_PATH}/School.prefab"
        });

        LoadPrefabsIntoList(residentialBuildings, new string[] {
            $"{SIMPLEPOLY_PATH}/Buildings/Building_House_01_color01.prefab",
            $"{SIMPLEPOLY_PATH}/Buildings/Building_House_02_color01.prefab",
            $"{SIMPLEPOLY_PATH}/Buildings/Building_Residential_color01.prefab",
            $"{URBAN_PATH}/Buildings/Building_1.prefab",
            $"{URBAN_PATH}/Buildings/Building_3.prefab",
            $"{URBAN_PATH}/Buildings/Building_7.prefab"
        });

        LoadPrefabsIntoList(industrialBuildings, new string[] {
            $"{SIMPLEPOLY_PATH}/Buildings/Building_Factory.prefab",
            $"{SIMPLEPOLY_PATH}/Buildings/Building_Auto Service.prefab",
            $"{SIMPLEPOLY_PATH}/Buildings/Building_Gas Station.prefab",
            $"{STUDIO_PATH}/Workshop.prefab"
        });

        LoadPrefabsIntoList(highRiseBuildings, new string[] {
            $"{SIMPLEPOLY_PATH}/Buildings/Building Sky_big_color01.prefab",
            $"{SIMPLEPOLY_PATH}/Buildings/Building Sky_small_color01.prefab",
            $"{URBAN_PATH}/Buildings/Building_8.prefab",
            $"{URBAN_PATH}/Buildings/Building_9.prefab"
        });

        // 4. Props
        LoadPrefabsIntoList(streetProps, new string[] {
            $"{URBAN_PATH}/Props/Other/Hydrant.prefab",
            $"{URBAN_PATH}/Props/Other/Trash_Big.prefab",
            $"{URBAN_PATH}/Props/Other/Newspaper_Stand.prefab",
            $"{URBAN_PATH}/Props/Other/Hot_Dog.prefab",
            $"{SIMPLEPOLY_PATH}/Props/Props_Coffee shop chair.prefab",
            $"{SIMPLEPOLY_PATH}/Props/Props_BillBoard_small.prefab"
        });
        
        LoadPrefabsIntoList(parkProps, new string[] {
            $"{SIMPLEPOLY_PATH}/Props/Props_Bench_1.prefab",
            $"{SIMPLEPOLY_PATH}/Props/Props_Dustbin.prefab"
        });

        // 5. Nature
        LoadPrefabsIntoList(natureTrees, new string[] {
            $"{URBAN_PATH}/Props/Other/Tree_1.prefab",
            $"{URBAN_PATH}/Props/Other/Tree_2.prefab",
            $"{SIMPLEPOLY_PATH}/Natures/Natures_Big Tree.prefab",
            $"{SIMPLEPOLY_PATH}/Natures/Natures_Fir Tree.prefab"
        });
        
        LoadPrefabsIntoList(natureBushes, new string[] {
            $"{SIMPLEPOLY_PATH}/Natures/Natures_Bush_01.prefab",
            $"{SIMPLEPOLY_PATH}/Natures/Natures_Pot Bush_big.prefab"
        });

        // 6. Vehicles
        LoadPrefabsIntoList(civilianVehicles, new string[] {
            $"{URBAN_PATH}/Vehicles/Cars/Car_1_1.prefab",
            $"{URBAN_PATH}/Vehicles/Cars/Car_2_1.prefab",
            $"{URBAN_PATH}/Vehicles/Cars/Car_3_1.prefab",
            $"{URBAN_PATH}/Vehicles/Pickups/Pickup_1_1.prefab"
        });

        LoadPrefabsIntoList(civicVehicles, new string[] {
            $"{URBAN_PATH}/Vehicles/Emergency_Vehicles/Police_Car.prefab"
        });
    }

    GameObject LoadPrefab(string path)
    {
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }

    void LoadPrefabsIntoList(List<GameObject> list, string[] paths)
    {
        list.Clear();
        foreach (var p in paths)
        {
            var go = LoadPrefab(p);
            if (go != null) list.Add(go);
        }
    }

    void OnGUI()
    {
        so.Update();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("🏙️ City Builder Pro — Advanced Generation", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Sistem tata kota berarsitektur modular yang presisi tanpa mesh-overlap (z-fighting) dan memiliki 6 Arketipe Distrik Tematik.", MessageType.Info);
        EditorGUILayout.Space(6);

        // Preset
        EditorGUILayout.LabelField("0. Tema & Gaya Kota", EditorStyles.boldLabel);
        presetKota = (CityPreset)EditorGUILayout.EnumPopup("Preset Distrik", presetKota);

        EditorGUILayout.Space(6);

        // Group 1: Ukuran & Posisi
        EditorGUILayout.LabelField("1. Pengaturan Grid & Matematika", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        gridX = EditorGUILayout.IntSlider("Jumlah Blok X", gridX, 2, 8);
        gridZ = EditorGUILayout.IntSlider("Jumlah Blok Z", gridZ, 2, 8);
        
        // Snapped Block Size Slider (44, 66, 88)
        int blockSegments = Mathf.RoundToInt((blockSize - 22f) / 22f);
        blockSegments = EditorGUILayout.IntSlider("Jarak Antar Simpang (Segmen)", blockSegments, 1, 4);
        blockSize = 22f + (blockSegments * 22f);
        EditorGUILayout.LabelField($"Ukuran Blok Aktual: {blockSize} meter", EditorStyles.miniLabel);

        originX = EditorGUILayout.FloatField("Pusat Kota X", originX);
        originZ = EditorGUILayout.FloatField("Pusat Kota Z", originZ);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(8);

        // Group 2: Opsi Fitur
        EditorGUILayout.LabelField("2. Fitur & Komponen Kota", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        addZebraCrossings = EditorGUILayout.Toggle("Zebra Cross Presisi", addZebraCrossings);
        addStreetLightsAndSignals = EditorGUILayout.Toggle("Lampu Jalan & Traffic Light", addStreetLightsAndSignals);
        addTreesAndNature = EditorGUILayout.Toggle("Vegetasi & Pohon", addTreesAndNature);
        addParkedVehicles = EditorGUILayout.Toggle("Mobil Parkir", addParkedVehicles);
        autoAddColliders = EditorGUILayout.Toggle("Auto-Fit Physics Colliders", autoAddColliders);
        addAsphaltBase = EditorGUILayout.Toggle("Lantai Aspal Dasar (Ground)", addAsphaltBase);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(12);

        // Tombol Aksi
        GUI.backgroundColor = new Color(0.25f, 0.8f, 0.45f);
        if (GUILayout.Button("🚀 BUILD STRUKTUR KOTA", GUILayout.Height(44)))
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
        
        // Status Aset
        EditorGUILayout.LabelField($"Katalog: {commercialBuildings.Count} Komersial, {civicBuildings.Count} Layanan Publik, {residentialBuildings.Count} Residensial", EditorStyles.miniLabel);

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

        if (roadStraight == null)
        {
            Debug.LogError("[CityBuilder] Error: Road prefabs not found! Pastikan Low Poly Simple Urban City terinstal.");
            return;
        }

        GameObject root = new GameObject("CityExpansion");
        Undo.RegisterCreatedObjectUndo(root, "Build Layout City");

        GameObject roadsParent = new GameObject("01_Roads_Network");
        roadsParent.transform.SetParent(root.transform);

        GameObject blocksParent = new GameObject("02_Districts");
        blocksParent.transform.SetParent(root.transform);

        GameObject propsParent = new GameObject("03_Street_Furniture_&_Lighting");
        propsParent.transform.SetParent(root.transform);

        GameObject vehiclesParent = new GameObject("04_Vehicles");
        vehiclesParent.transform.SetParent(root.transform);
        
        GameObject natureParent = new GameObject("05_Vegetation_&_Parks");
        natureParent.transform.SetParent(root.transform);

        float totalW = gridX * blockSize;
        float totalH = gridZ * blockSize;
        float halfW = totalW * 0.5f;
        float halfH = totalH * 0.5f;
        Vector3 center = new Vector3(originX, roadY, originZ);

        float roadSegLength = 22f;
        int segmentsPerBlock = Mathf.RoundToInt((blockSize - 22f) / 22f);
        if (segmentsPerBlock < 1) segmentsPerBlock = 1;

        // =========================================================================
        // 1. JARINGAN JALAN PRESISI TANPA OVERLAP (Zero Z-Fighting)
        // =========================================================================

        for (int zi = 0; zi <= gridZ; zi++)
        {
            float zPos = center.z - halfH + zi * blockSize;
            for (int xi = 0; xi <= gridX; xi++)
            {
                float xPos = center.x - halfW + xi * blockSize;
                Vector3 crossPos = new Vector3(xPos, roadY, zPos);

                // A. Persimpangan (Crossroads)
                SpawnPrefabDirect(roadCrossroad != null ? roadCrossroad : roadStraight, crossPos, Quaternion.identity, roadsParent.transform);

                // Lampu Lalu Lintas 4 Sudut (Oriented to face incoming traffic lanes)
                if (addStreetLightsAndSignals && trafficLightPrefab != null)
                {
                    float offset = roadWidth * 0.65f;
                    SpawnPrefabDirect(trafficLightPrefab, crossPos + new Vector3(offset, 0, offset), Quaternion.Euler(0, 225, 0), propsParent.transform);
                    SpawnPrefabDirect(trafficLightPrefab, crossPos + new Vector3(-offset, 0, offset), Quaternion.Euler(0, 135, 0), propsParent.transform);
                    SpawnPrefabDirect(trafficLightPrefab, crossPos + new Vector3(offset, 0, -offset), Quaternion.Euler(0, 315, 0), propsParent.transform);
                    SpawnPrefabDirect(trafficLightPrefab, crossPos + new Vector3(-offset, 0, -offset), Quaternion.Euler(0, 45, 0), propsParent.transform);
                }

                // B. Segmen Jalan Horizontal (Sumbu X)
                if (xi < gridX)
                {
                    for (int s = 0; s < segmentsPerBlock; s++)
                    {
                        float segX = xPos + 11f + (s * roadSegLength) + (roadSegLength * 0.5f);
                        Vector3 segPos = new Vector3(segX, roadY, zPos);
                        
                        GameObject prefabToUse = roadStraight;
                        if (addZebraCrossings && roadCrosswalk != null) {
                            if (s == 0 || s == segmentsPerBlock - 1) prefabToUse = roadCrosswalk;
                        } else if (s == segmentsPerBlock / 2 && roadManhole != null) {
                            prefabToUse = roadManhole;
                        }

                        SpawnPrefabDirect(prefabToUse, segPos, Quaternion.Euler(0, 90, 0), roadsParent.transform);

                        // Lampu Jalan di pinggir trotoar jalan horizontal
                        if (addStreetLightsAndSignals && streetLightPrefab != null)
                        {
                            SpawnPrefabDirect(streetLightPrefab, segPos + new Vector3(0, 0, roadWidth * 0.55f), Quaternion.Euler(0, 180, 0), propsParent.transform);
                            SpawnPrefabDirect(streetLightPrefab, segPos + new Vector3(0, 0, -roadWidth * 0.55f), Quaternion.Euler(0, 0, 0), propsParent.transform);
                        }
                    }
                }

                // C. Segmen Jalan Vertikal (Sumbu Z)
                if (zi < gridZ)
                {
                    for (int s = 0; s < segmentsPerBlock; s++)
                    {
                        float segZ = zPos + 11f + (s * roadSegLength) + (roadSegLength * 0.5f);
                        Vector3 segPos = new Vector3(xPos, roadY, segZ);
                        
                        GameObject prefabToUse = roadStraight;
                        if (addZebraCrossings && roadCrosswalk != null) {
                            if (s == 0 || s == segmentsPerBlock - 1) prefabToUse = roadCrosswalk;
                        } else if (s == segmentsPerBlock / 2 && roadManhole != null) {
                            prefabToUse = roadManhole;
                        }

                        SpawnPrefabDirect(prefabToUse, segPos, Quaternion.identity, roadsParent.transform);

                        // Lampu Jalan di pinggir trotoar jalan vertikal
                        if (addStreetLightsAndSignals && streetLightPrefab != null)
                        {
                            SpawnPrefabDirect(streetLightPrefab, segPos + new Vector3(roadWidth * 0.55f, 0, 0), Quaternion.Euler(0, -90, 0), propsParent.transform);
                            SpawnPrefabDirect(streetLightPrefab, segPos + new Vector3(-roadWidth * 0.55f, 0, 0), Quaternion.Euler(0, 90, 0), propsParent.transform);
                        }
                    }
                }
            }
        }

        // =========================================================================
        // 2. SISTEM ZONASI 6 DISTRIK TEMATIK
        // =========================================================================
        int blockCount = 0;

        for (int gx = 0; gx < gridX; gx++)
        {
            for (int gz = 0; gz < gridZ; gz++)
            {
                float bCenterX = center.x - halfW + (gx + 0.5f) * blockSize;
                float bCenterZ = center.z - halfH + (gz + 0.5f) * blockSize;
                Vector3 bCenter = new Vector3(bCenterX, roadY, bCenterZ);
                float innerSize = blockSize - roadWidth; // misal 44 - 7.5 = 36.5m

                // Penentuan Arketipe Distrik
                int archetype = 0; // 0=Commercial, 1=Park, 2=Civic, 3=Residential, 4=Industrial, 5=HighRise
                
                if (presetKota == CityPreset.MetropolisSeimbang) {
                    if (gx == gridX / 2 && gz == gridZ / 2) archetype = 1; // Center Park
                    else if ((gx + gz) % 3 == 0) archetype = 3; // Residential
                    else if ((gx + gz) % 5 == 0) archetype = 2; // Civic
                    else archetype = 0; // Commercial
                }
                else if (presetKota == CityPreset.PusatBisnisKomersial) {
                    archetype = ((gx + gz) % 2 == 0) ? 0 : 5; // Campuran Komersial & HighRise
                }
                else if (presetKota == CityPreset.KawasanResidensial) {
                    archetype = ((gx + gz) % 4 == 0) ? 1 : 3; // Mayoritas perumahan & taman
                }
                else if (presetKota == CityPreset.PusatLayananPublik) {
                    archetype = ((gx + gz) % 2 == 0) ? 2 : 0;
                }
                else if (presetKota == CityPreset.KawasanIndustri) {
                    archetype = ((gx + gz) % 3 == 0) ? 4 : 0;
                }

                GameObject blockIsland = new GameObject($"District_{gx}_{gz}_Type{archetype}");
                blockIsland.transform.SetParent(blocksParent.transform);
                blockIsland.transform.position = bCenter;

                // Alas Pulau Trotoar 3D (Solid Sidewalk Base)
                GameObject sidewalk = GameObject.CreatePrimitive(PrimitiveType.Cube);
                sidewalk.name = "Sidewalk_Curb_Plate";
                sidewalk.transform.SetParent(blockIsland.transform);
                sidewalk.transform.position = bCenter + new Vector3(0, curbHeight * 0.5f, 0);
                sidewalk.transform.localScale = new Vector3(innerSize, curbHeight, innerSize);
                
                Material sidewalkMat = AssetDatabase.LoadAssetAtPath<Material>($"{URBAN_PATH}/../Materials/tex.mat");
                if (sidewalkMat != null) sidewalk.GetComponent<Renderer>().sharedMaterial = sidewalkMat;

                // Bangun Distrik sesuai tipe
                switch (archetype)
                {
                    case 0: BuildCommercialDistrict(bCenter, innerSize, blockIsland.transform, propsParent.transform, vehiclesParent.transform); break;
                    case 1: BuildCentralPark(bCenter, innerSize, blockIsland.transform, natureParent.transform, propsParent.transform); break;
                    case 2: BuildCivicDistrict(bCenter, innerSize, blockIsland.transform, vehiclesParent.transform); break;
                    case 3: BuildResidentialDistrict(bCenter, innerSize, blockIsland.transform, natureParent.transform, vehiclesParent.transform); break;
                    case 4: BuildIndustrialDistrict(bCenter, innerSize, blockIsland.transform, vehiclesParent.transform); break;
                    case 5: BuildHighRiseDistrict(bCenter, innerSize, blockIsland.transform, natureParent.transform); break;
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
            ground.transform.position = new Vector3(center.x, -0.01f, center.z); // sedikit di bawah jalan
            ground.transform.localScale = new Vector3((totalW + 40f) / 10f, 1f, (totalH + 40f) / 10f);

            Material groundMat = AssetDatabase.LoadAssetAtPath<Material>($"{URBAN_PATH}/../Materials/tex.mat");
            if (groundMat != null) ground.GetComponent<Renderer>().sharedMaterial = groundMat;

            // Pastikan ground punya collider
            EnsureCollider(ground);
            Undo.RegisterCreatedObjectUndo(ground, "ground");
        }

        Debug.Log($"<color=cyan>[CityBuilder Pro] Sukses membangun kota presisi! Ukuran: {gridX}x{gridZ} ({blockCount} distrik). Segmen Jalan: {segmentsPerBlock}.</color>");
    }

    // --- ARSITEKTUR ZONASI ---

    void BuildCommercialDistrict(Vector3 center, float innerSize, Transform parent, Transform propsParent, Transform vehiclesParent)
    {
        float edge = innerSize * 0.5f;
        float setback = 1.0f; // Mundur 1 meter dari tepi curb
        
        // Frontages (4 sisi menghadap jalan)
        SpawnBuildingAtEdge(commercialBuildings, center + new Vector3(0, 0, edge - setback), 180f, parent); // North edge, faces North
        SpawnBuildingAtEdge(commercialBuildings, center + new Vector3(0, 0, -edge + setback), 0f, parent); // South edge, faces South
        SpawnBuildingAtEdge(commercialBuildings, center + new Vector3(edge - setback, 0, 0), -90f, parent); // East edge, faces East
        SpawnBuildingAtEdge(commercialBuildings, center + new Vector3(-edge + setback, 0, 0), 90f, parent); // West edge, faces West

        if (addParkedVehicles)
        {
            SpawnPrefabList(civilianVehicles, center + new Vector3(edge + 2f, 0, -edge * 0.5f), Quaternion.Euler(0, 90, 0), vehiclesParent);
            SpawnPrefabList(civilianVehicles, center + new Vector3(-edge - 2f, 0, edge * 0.5f), Quaternion.Euler(0, -90, 0), vehiclesParent);
        }
        
        // Props (Meja Kafe / Kios)
        SpawnPrefabList(streetProps, center + new Vector3(edge - 4f, curbHeight, edge - 4f), Quaternion.identity, propsParent);
    }

    void BuildCentralPark(Vector3 center, float innerSize, Transform parent, Transform natureParent, Transform propsParent)
    {
        // Inner grass patch
        GameObject grass = GameObject.CreatePrimitive(PrimitiveType.Plane);
        grass.name = "Park_Grass_Area";
        grass.transform.SetParent(parent);
        grass.transform.position = center + new Vector3(0, curbHeight + 0.02f, 0);
        grass.transform.localScale = new Vector3((innerSize - 4f) / 10f, 1f, (innerSize - 4f) / 10f);
        grass.GetComponent<Renderer>().material.color = new Color(0.3f, 0.6f, 0.2f); // Warna hijau fallback
        EnsureCollider(grass);

        if (addTreesAndNature)
        {
            // Pohon besar di tengah
            SpawnPrefabList(natureTrees, center + new Vector3(0, curbHeight, 0), Quaternion.identity, natureParent);
            // Pohon melingkar
            float d = innerSize * 0.35f;
            Vector3[] pos = { new Vector3(d,0,d), new Vector3(-d,0,d), new Vector3(d,0,-d), new Vector3(-d,0,-d) };
            foreach (var p in pos) {
                SpawnPrefabList(natureTrees, center + p + new Vector3(0, curbHeight, 0), Quaternion.identity, natureParent);
                SpawnPrefabList(parkProps, center + p * 0.8f + new Vector3(0, curbHeight, 0), Quaternion.identity, propsParent); // Bangku
            }
        }
    }

    void BuildCivicDistrict(Vector3 center, float innerSize, Transform parent, Transform vehiclesParent)
    {
        float edge = innerSize * 0.5f;
        // 1 Bangunan layanan publik besar di utara
        SpawnBuildingAtEdge(civicBuildings, center + new Vector3(0, 0, edge - 2f), 180f, parent);
        
        // Area parkir luas di depan (selatan)
        if (addParkedVehicles)
        {
            SpawnPrefabList(civicVehicles, center + new Vector3(-4f, 0, -edge + 4f), Quaternion.Euler(0, 180, 0), vehiclesParent);
            SpawnPrefabList(civicVehicles, center + new Vector3(4f, 0, -edge + 4f), Quaternion.Euler(0, 180, 0), vehiclesParent);
        }
    }

    void BuildResidentialDistrict(Vector3 center, float innerSize, Transform parent, Transform natureParent, Transform vehiclesParent)
    {
        float edge = innerSize * 0.5f;
        float d = innerSize * 0.25f;

        // 4 Rumah di 4 kuadran, menghadap jalan luar
        SpawnBuildingAtEdge(residentialBuildings, center + new Vector3(-d, 0, edge - 1f), 180f, parent); // Kiri-Atas hadap Utara
        SpawnBuildingAtEdge(residentialBuildings, center + new Vector3(d, 0, edge - 1f), 180f, parent);  // Kanan-Atas hadap Utara
        SpawnBuildingAtEdge(residentialBuildings, center + new Vector3(-d, 0, -edge + 1f), 0f, parent);  // Kiri-Bawah hadap Selatan
        SpawnBuildingAtEdge(residentialBuildings, center + new Vector3(d, 0, -edge + 1f), 0f, parent);   // Kanan-Bawah hadap Selatan

        if (addTreesAndNature)
        {
            SpawnPrefabList(natureTrees, center + new Vector3(-d, curbHeight, 0), Quaternion.identity, natureParent);
            SpawnPrefabList(natureTrees, center + new Vector3(d, curbHeight, 0), Quaternion.identity, natureParent);
        }

        if (addParkedVehicles)
        {
            // Mobil parkir di driveway rumah (halaman depan)
            SpawnPrefabList(civilianVehicles, center + new Vector3(-d + 2f, 0, edge - 6f), Quaternion.identity, vehiclesParent);
            SpawnPrefabList(civilianVehicles, center + new Vector3(d + 2f, 0, -edge + 6f), Quaternion.Euler(0, 180, 0), vehiclesParent);
        }
    }

    void BuildIndustrialDistrict(Vector3 center, float innerSize, Transform parent, Transform vehiclesParent)
    {
        float edge = innerSize * 0.5f;
        SpawnBuildingAtEdge(industrialBuildings, center + new Vector3(0, 0, -edge + 2f), 0f, parent); // Pabrik menghadap selatan
        
        // Tumpukan boks logistik
        SpawnPrefabList(streetProps, center + new Vector3(edge - 6f, curbHeight, edge - 6f), Quaternion.Euler(0, 45, 0), parent);
        
        if (addParkedVehicles)
        {
            SpawnPrefabList(civilianVehicles, center + new Vector3(edge + 2f, 0, 0), Quaternion.Euler(0, 90, 0), vehiclesParent);
        }
    }

    void BuildHighRiseDistrict(Vector3 center, float innerSize, Transform parent, Transform natureParent)
    {
        // 1-2 Gedung pencakar langit
        SpawnBuildingAtEdge(highRiseBuildings, center, 0f, parent);
        
        if (addTreesAndNature)
        {
            float d = innerSize * 0.4f;
            SpawnPrefabList(natureBushes, center + new Vector3(d, curbHeight, d), Quaternion.identity, natureParent);
            SpawnPrefabList(natureBushes, center + new Vector3(-d, curbHeight, d), Quaternion.identity, natureParent);
            SpawnPrefabList(natureBushes, center + new Vector3(d, curbHeight, -d), Quaternion.identity, natureParent);
            SpawnPrefabList(natureBushes, center + new Vector3(-d, curbHeight, -d), Quaternion.identity, natureParent);
        }
    }

    // --- HELPER METHODS ---

    void SpawnPrefabDirect(GameObject prefab, Vector3 pos, Quaternion rot, Transform parent)
    {
        if (prefab == null) return;
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        if (go == null) return;
        
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.rotation = rot;
        if (autoAddColliders) EnsureCollider(go);
        Undo.RegisterCreatedObjectUndo(go, go.name);
    }

    void SpawnBuildingAtEdge(List<GameObject> list, Vector3 pos, float yRotation, Transform parent)
    {
        if (list == null || list.Count == 0) return;
        var prefab = list[Random.Range(0, list.Count)];
        
        // Posisi Y diatur agar base bangunan sejajar dengan tinggi trotoar (curbHeight)
        Vector3 finalPos = new Vector3(pos.x, curbHeight, pos.z);
        SpawnPrefabDirect(prefab, finalPos, Quaternion.Euler(0, yRotation, 0), parent);
    }

    void SpawnPrefabList(List<GameObject> list, Vector3 pos, Quaternion rot, Transform parent)
    {
        if (list == null || list.Count == 0) return;
        var p = list[Random.Range(0, list.Count)];
        SpawnPrefabDirect(p, pos, rot, parent);
    }

    void EnsureCollider(GameObject go)
    {
        // Jika object punya mesh renderer tapi tidak punya collider sama sekali
        MeshRenderer[] renderers = go.GetComponentsInChildren<MeshRenderer>();
        foreach (var rend in renderers)
        {
            Collider col = rend.GetComponent<Collider>();
            if (col == null)
            {
                // Pasang BoxCollider yang fit dengan mesh
                rend.gameObject.AddComponent<BoxCollider>();
            }
        }
    }
}
#endif
