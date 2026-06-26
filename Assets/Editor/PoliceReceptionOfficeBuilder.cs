using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PoliceReceptionOfficeBuilder
{
    static Material _wallMat;
    static Material _floorMat;
    static Material _ceilingMat;
    static Material _woodMat;
    static Material _darkWoodMat;
    static Material _redBannerMat;
    static Material _blueSignMat;
    static Material _metalMat;
    static Material _glassMat;
    static Material _plantMat;
    static Material _paperMat;
    static Material _screenMat;

    [MenuItem("Tools/Build Police Reception Office (Reference Style)")]
    public static void Build()
    {
        EnsureFolders();
        CreateMaterials();

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject root = new GameObject("Police_Reception_Office");
        Transform architecture = CreateGroup(root.transform, "Architecture");
        Transform reception = CreateGroup(root.transform, "ReceptionArea");
        Transform foreground = CreateGroup(root.transform, "ForegroundDesk");
        Transform decor = CreateGroup(root.transform, "Decorations");
        Transform lights = CreateGroup(root.transform, "Lighting");
        Transform cameraRig = CreateGroup(root.transform, "MainCameraRig");

        BuildArchitecture(architecture);
        BuildReceptionWall(reception);
        BuildFrontDesk(foreground);
        BuildDecorations(decor);
        BuildLighting(lights);
        BuildCamera(cameraRig);

        string scenePath = "Assets/Scenes/Police_Reception_Office_Day.unity";
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), scenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[PoliceReceptionOffice] Scene created: " + scenePath);
    }

    static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Materials")) AssetDatabase.CreateFolder("Assets", "Materials");
        if (!AssetDatabase.IsValidFolder("Assets/Materials/PoliceReceptionOffice")) AssetDatabase.CreateFolder("Assets/Materials", "PoliceReceptionOffice");
    }

    static Material LoadOrCreate(string name, Color color, float smoothness, float metallic)
    {
        string path = "Assets/Materials/PoliceReceptionOffice/" + name + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(mat, path);
        }

        mat.color = color;
        mat.SetFloat("_Smoothness", smoothness);
        mat.SetFloat("_Metallic", metallic);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static void CreateMaterials()
    {
        _wallMat = LoadOrCreate("WallPaint", new Color(0.82f, 0.82f, 0.80f), 0.08f, 0f);
        _floorMat = LoadOrCreate("FloorTile", new Color(0.68f, 0.66f, 0.62f), 0.45f, 0f);
        _ceilingMat = LoadOrCreate("Ceiling", new Color(0.86f, 0.86f, 0.84f), 0.05f, 0f);
        _woodMat = LoadOrCreate("Wood", new Color(0.25f, 0.16f, 0.10f), 0.3f, 0f);
        _darkWoodMat = LoadOrCreate("DarkWood", new Color(0.16f, 0.10f, 0.06f), 0.35f, 0f);
        _redBannerMat = LoadOrCreate("RedBanner", new Color(0.45f, 0.08f, 0.08f), 0.2f, 0f);
        _blueSignMat = LoadOrCreate("BlueSign", new Color(0.08f, 0.18f, 0.45f), 0.2f, 0f);
        _metalMat = LoadOrCreate("Metal", new Color(0.42f, 0.44f, 0.46f), 0.6f, 0.8f);
        _glassMat = LoadOrCreate("Glass", new Color(0.60f, 0.72f, 0.80f, 0.35f), 0.9f, 0f);
        _glassMat.SetFloat("_Surface", 1f);
        _glassMat.SetFloat("_Blend", 0f);
        _glassMat.SetOverrideTag("RenderType", "Transparent");
        _glassMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        _glassMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        _glassMat.SetInt("_ZWrite", 0);
        _glassMat.renderQueue = 3000;
        _plantMat = LoadOrCreate("Plant", new Color(0.23f, 0.45f, 0.20f), 0.2f, 0f);
        _paperMat = LoadOrCreate("Paper", new Color(0.92f, 0.90f, 0.84f), 0.05f, 0f);
        _screenMat = LoadOrCreate("Screen", new Color(0.06f, 0.13f, 0.16f), 0.8f, 0.1f);
        _screenMat.EnableKeyword("_EMISSION");
        _screenMat.SetColor("_EmissionColor", new Color(0.08f, 0.22f, 0.28f) * 1.2f);
    }

    static Transform CreateGroup(Transform parent, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.localPosition = Vector3.zero;
        return go.transform;
    }

    static GameObject CreateBox(Transform parent, string name, Vector3 scale, Vector3 localPos, Material mat)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.SetParent(parent);
        box.transform.localScale = scale;
        box.transform.localPosition = localPos;
        box.GetComponent<Renderer>().sharedMaterial = mat;
        return box;
    }

    static void BuildArchitecture(Transform parent)
    {
        float w = 14f;
        float d = 10f;
        float h = 3.2f;

        CreateBox(parent, "Floor", new Vector3(w, 0.12f, d), new Vector3(0f, -0.06f, 0f), _floorMat);
        CreateBox(parent, "Ceiling", new Vector3(w, 0.1f, d), new Vector3(0f, h + 0.05f, 0f), _ceilingMat);
        CreateBox(parent, "BackWall", new Vector3(w, h, 0.15f), new Vector3(0f, h * 0.5f, -d * 0.5f), _wallMat);
        CreateBox(parent, "FrontWall", new Vector3(w, h, 0.15f), new Vector3(0f, h * 0.5f, d * 0.5f), _wallMat);
        CreateBox(parent, "RightWall", new Vector3(0.15f, h, d), new Vector3(w * 0.5f, h * 0.5f, 0f), _wallMat);

        // Left wall split for windows
        CreateBox(parent, "LeftWall_Front", new Vector3(0.15f, h, 3.0f), new Vector3(-w * 0.5f, h * 0.5f, 3.5f), _wallMat);
        CreateBox(parent, "LeftWall_Back", new Vector3(0.15f, h, 3.0f), new Vector3(-w * 0.5f, h * 0.5f, -3.5f), _wallMat);
        CreateBox(parent, "LeftWall_MidTop", new Vector3(0.15f, 1.1f, 4.0f), new Vector3(-w * 0.5f, 2.65f, 0f), _wallMat);
        CreateBox(parent, "LeftWall_MidBottom", new Vector3(0.15f, 1.0f, 4.0f), new Vector3(-w * 0.5f, 0.5f, 0f), _wallMat);

        Transform windows = CreateGroup(parent, "Windows");
        CreateBox(windows, "WindowGlass_1", new Vector3(0.04f, 1.7f, 1.8f), new Vector3(-w * 0.5f + 0.02f, 1.55f, -1.1f), _glassMat);
        CreateBox(windows, "WindowGlass_2", new Vector3(0.04f, 1.7f, 1.8f), new Vector3(-w * 0.5f + 0.02f, 1.55f, 1.1f), _glassMat);

        Transform blinds = CreateGroup(windows, "Blinds");
        for (int i = 0; i < 8; i++)
        {
            float y = 2.2f - i * 0.18f;
            CreateBox(blinds, "Blind_" + i, new Vector3(0.03f, 0.05f, 3.8f), new Vector3(-w * 0.5f + 0.06f, y, 0f), _metalMat);
        }
    }

    static void BuildReceptionWall(Transform parent)
    {
        CreateBox(parent, "BackdropWood", new Vector3(5.6f, 2.8f, 0.08f), new Vector3(0f, 1.45f, -4.85f), _woodMat);
        CreateBox(parent, "ReceptionDesk", new Vector3(4.8f, 0.85f, 1.2f), new Vector3(0f, 0.42f, -3.95f), _darkWoodMat);
        CreateBox(parent, "DeskTop", new Vector3(5.0f, 0.08f, 1.25f), new Vector3(0f, 0.86f, -3.95f), _woodMat);

        CreateBox(parent, "BannerTop", new Vector3(6.4f, 0.48f, 0.05f), new Vector3(0f, 2.95f, -4.9f), _redBannerMat);
        CreateBox(parent, "BannerLeft", new Vector3(1.5f, 1.8f, 0.05f), new Vector3(-3.0f, 1.9f, -4.9f), _redBannerMat);
        CreateBox(parent, "BannerRight", new Vector3(1.5f, 1.8f, 0.05f), new Vector3(3.0f, 1.9f, -4.9f), _redBannerMat);

        CreateBox(parent, "BlueSign_Door", new Vector3(1.4f, 0.35f, 0.05f), new Vector3(5.7f, 2.8f, -4.9f), _blueSignMat);
        CreateBox(parent, "Door", new Vector3(1.2f, 2.2f, 0.08f), new Vector3(5.7f, 1.1f, -4.86f), _darkWoodMat);

        // Insignia plate
        GameObject insignia = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        insignia.name = "PoliceInsignia";
        insignia.transform.SetParent(parent);
        insignia.transform.localScale = new Vector3(0.9f, 0.02f, 0.9f);
        insignia.transform.localPosition = new Vector3(0f, 2.15f, -4.80f);
        insignia.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        insignia.GetComponent<Renderer>().sharedMaterial = _metalMat;

        CreateText(parent, "Text_Slogan", "VI AN NINH TO QUOC", 0.16f, new Vector3(0f, 1.72f, -4.80f), Color.yellow);
        CreateText(parent, "Text_Slogan2", "VI HANH PHUC NHAN DAN", 0.16f, new Vector3(0f, 1.46f, -4.80f), Color.yellow);
        CreateText(parent, "Text_BannerTop", "DANG CONG SAN VIET NAM QUANG VINH MUON NAM", 0.09f, new Vector3(0f, 2.95f, -4.78f), Color.yellow);
        CreateText(parent, "Text_DoorSign", "PHONG DIEU TRA", 0.08f, new Vector3(5.7f, 2.8f, -4.78f), Color.white);
    }

    static void BuildFrontDesk(Transform parent)
    {
        CreateBox(parent, "WorkDesk", new Vector3(3.0f, 0.78f, 1.5f), new Vector3(-3.2f, 0.39f, 2.7f), _darkWoodMat);
        CreateBox(parent, "Monitor", new Vector3(0.65f, 0.42f, 0.05f), new Vector3(-4.1f, 0.96f, 2.3f), _screenMat);
        CreateBox(parent, "Keyboard", new Vector3(0.45f, 0.02f, 0.15f), new Vector3(-4.1f, 0.79f, 2.6f), _metalMat);
        CreateBox(parent, "Mouse", new Vector3(0.05f, 0.02f, 0.07f), new Vector3(-3.8f, 0.79f, 2.6f), _metalMat);
        CreateBox(parent, "PaperStack", new Vector3(0.6f, 0.08f, 0.45f), new Vector3(-2.6f, 0.82f, 2.6f), _paperMat);
        CreateBox(parent, "DeskSign", new Vector3(0.9f, 0.22f, 0.05f), new Vector3(-3.3f, 0.92f, 1.95f), _blueSignMat);
        CreateText(parent, "Text_DeskSign", "CAN BO TIEP DAN", 0.06f, new Vector3(-3.3f, 0.92f, 1.98f), Color.white);

        CreateBox(parent, "CoffeeCup", new Vector3(0.08f, 0.1f, 0.08f), new Vector3(-4.8f, 0.84f, 2.4f), _paperMat);
        CreateBox(parent, "TeaTable", new Vector3(2.2f, 0.52f, 1.3f), new Vector3(1.8f, 0.26f, 3.0f), _darkWoodMat);
        CreateBox(parent, "TeaPot", new Vector3(0.16f, 0.12f, 0.16f), new Vector3(2.0f, 0.58f, 2.9f), _paperMat);

        CreateBox(parent, "VisitorBench_Left", new Vector3(1.1f, 0.45f, 0.55f), new Vector3(0.7f, 0.23f, 3.9f), _woodMat);
        CreateBox(parent, "VisitorBench_Right", new Vector3(1.1f, 0.45f, 0.55f), new Vector3(2.9f, 0.23f, 3.9f), _woodMat);
    }

    static void BuildDecorations(Transform parent)
    {
        // Plants
        CreateBox(parent, "PlantPot_Left", new Vector3(0.45f, 0.55f, 0.45f), new Vector3(-6.0f, 0.27f, -2.8f), _metalMat);
        CreateBox(parent, "PlantLeaf_Left", new Vector3(0.25f, 1.1f, 0.25f), new Vector3(-6.0f, 1.0f, -2.8f), _plantMat);
        CreateBox(parent, "PlantPot_Right", new Vector3(0.45f, 0.55f, 0.45f), new Vector3(6.2f, 0.27f, -2.8f), _metalMat);
        CreateBox(parent, "PlantLeaf_Right", new Vector3(0.25f, 1.1f, 0.25f), new Vector3(6.2f, 1.0f, -2.8f), _plantMat);

        // Water dispenser and cabinet
        CreateBox(parent, "WaterDispenserBody", new Vector3(0.55f, 1.2f, 0.55f), new Vector3(3.9f, 0.6f, -3.8f), _paperMat);
        CreateBox(parent, "WaterBottle", new Vector3(0.28f, 0.45f, 0.28f), new Vector3(3.9f, 1.45f, -3.8f), _glassMat);
        CreateBox(parent, "FileCabinet", new Vector3(0.9f, 1.8f, 0.55f), new Vector3(6.2f, 0.9f, -1.0f), _metalMat);

        // Wall frames and map
        CreateBox(parent, "Frame_1", new Vector3(1.2f, 1.6f, 0.04f), new Vector3(3.8f, 1.9f, -4.86f), _paperMat);
        CreateBox(parent, "Frame_2", new Vector3(1.2f, 1.6f, 0.04f), new Vector3(2.4f, 1.9f, -4.86f), _paperMat);
    }

    static void BuildLighting(Transform parent)
    {
        GameObject sunObj = new GameObject("Directional Sun");
        sunObj.transform.SetParent(parent);
        sunObj.transform.rotation = Quaternion.Euler(45f, -38f, 0f);
        Light sun = sunObj.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.color = new Color(1f, 0.95f, 0.85f);
        sun.intensity = 1.15f;
        sun.shadows = LightShadows.Soft;

        for (int i = 0; i < 4; i++)
        {
            GameObject tube = new GameObject("Fluorescent_" + i);
            tube.transform.SetParent(parent);
            tube.transform.position = new Vector3(-4.5f + i * 3f, 3.0f, 0f);
            Light l = tube.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(0.9f, 0.92f, 0.95f);
            l.intensity = 0.8f;
            l.range = 5f;
            l.shadows = LightShadows.None;
        }
    }

    static void BuildCamera(Transform parent)
    {
        GameObject camObj = new GameObject("Main Camera");
        camObj.transform.SetParent(parent);
        camObj.tag = "MainCamera";
        camObj.transform.position = new Vector3(-1.2f, 1.65f, 4.9f);
        camObj.transform.rotation = Quaternion.Euler(10f, -160f, 0f);

        Camera cam = camObj.AddComponent<Camera>();
        cam.fieldOfView = 56f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.70f, 0.76f, 0.85f);
        camObj.AddComponent<AudioListener>();
    }

    static void CreateText(Transform parent, string name, string text, float charSize, Vector3 localPos, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.localPosition = localPos;
        TextMesh tm = go.AddComponent<TextMesh>();
        tm.text = text;
        tm.characterSize = charSize;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.fontSize = 72;
        tm.color = color;
    }
}
