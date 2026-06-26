using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static class DetectiveOfficeBuilder
{
    const float RoomWidth = 18f;
    const float RoomDepth = 14f;
    const float RoomHeight = 3.2f;
    const float WallThickness = 0.15f;
    const float HallwayDepth = 10f;

    static readonly Dictionary<string, Material> Mats = new Dictionary<string, Material>();

    [MenuItem("Tools/Build Detective Office Scene (AAA Night)")]
    public static void Build()
    {
        EnsureFolders();
        CreateMaterials();
        ResetScene();

        GameObject root = new GameObject("Police_Detective_Office");
        Transform architecture = CreateGroup(root.transform, "Architecture");
        Transform floor = CreateGroup(root.transform, "Floor");
        Transform ceiling = CreateGroup(root.transform, "Ceiling");
        Transform walls = CreateGroup(root.transform, "Walls");
        Transform windows = CreateGroup(root.transform, "Windows");
        Transform doors = CreateGroup(root.transform, "Doors");
        Transform reception = CreateGroup(root.transform, "Reception");
        Transform desks = CreateGroup(root.transform, "DetectiveDesks");
        Transform board = CreateGroup(root.transform, "InvestigationBoard");
        Transform cabinets = CreateGroup(root.transform, "Cabinets");
        Transform decorations = CreateGroup(root.transform, "Decorations");
        Transform lighting = CreateGroup(root.transform, "Lighting");
        Transform effects = CreateGroup(root.transform, "Effects");
        Transform mainCamera = CreateGroup(root.transform, "MainCamera");

        BuildArchitecture(architecture);
        BuildFloor(floor);
        BuildCeiling(ceiling);
        BuildWalls(walls);
        BuildWindows(windows);
        BuildDoorAndEntry(doors);
        BuildReception(reception);
        BuildDetectiveDesks(desks, lighting);
        BuildInvestigationCorner(board);
        BuildStorage(cabinets);
        BuildDecorations(decorations);
        BuildMainLighting(lighting);
        BuildEffects(effects);
        BuildMainCamera(mainCamera);
        ConfigureAtmosphere();
        MarkSceneStatic(root);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[DetectiveOffice] Scene Police_Detective_Office_Night rebuilt successfully.");
    }

    static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Models")) AssetDatabase.CreateFolder("Assets", "Models");
        if (!AssetDatabase.IsValidFolder("Assets/Models/DetectiveOffice")) AssetDatabase.CreateFolder("Assets/Models", "DetectiveOffice");
        if (!AssetDatabase.IsValidFolder("Assets/Models/DetectiveOffice/Materials")) AssetDatabase.CreateFolder("Assets/Models/DetectiveOffice", "Materials");
        if (!AssetDatabase.IsValidFolder("Assets/Settings")) AssetDatabase.CreateFolder("Assets", "Settings");
    }

    static void ResetScene()
    {
        GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject go in roots)
        {
            if (go == null) continue;
            Object.DestroyImmediate(go);
        }
    }

    static Transform CreateGroup(Transform parent, string name)
    {
        GameObject group = new GameObject(name);
        group.transform.SetParent(parent);
        group.transform.localPosition = Vector3.zero;
        group.transform.localRotation = Quaternion.identity;
        group.transform.localScale = Vector3.one;
        return group.transform;
    }

    static Material LoadOrCreateMat(string name, Color color, float smoothness = 0.2f, float metallic = 0f, bool transparent = false, bool emissive = false, Color? emission = null)
    {
        string path = "Assets/Models/DetectiveOffice/Materials/" + name + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(mat, path);
        }

        mat.color = color;
        mat.SetFloat("_Smoothness", smoothness);
        mat.SetFloat("_Metallic", metallic);
        if (transparent)
        {
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", 0f);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;
        }
        if (emissive)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emission ?? color * 1.5f);
        }
        else
        {
            mat.DisableKeyword("_EMISSION");
        }

        EditorUtility.SetDirty(mat);
        Mats[name] = mat;
        return mat;
    }

    static void CreateMaterials()
    {
        LoadOrCreateMat("ConcreteWall", new Color(0.76f, 0.76f, 0.74f), 0.05f, 0f);
        LoadOrCreateMat("WallStain", new Color(0.40f, 0.38f, 0.34f, 0.45f), 0.05f, 0f, true);
        LoadOrCreateMat("FloorCeramic", new Color(0.16f, 0.17f, 0.19f), 0.6f, 0f);
        LoadOrCreateMat("AcousticCeiling", new Color(0.66f, 0.66f, 0.64f), 0.08f, 0f);
        LoadOrCreateMat("DarkWood", new Color(0.20f, 0.13f, 0.08f), 0.28f, 0f);
        LoadOrCreateMat("BlackMetal", new Color(0.11f, 0.11f, 0.12f), 0.55f, 0.85f);
        LoadOrCreateMat("DustyGlass", new Color(0.16f, 0.20f, 0.24f, 0.35f), 0.9f, 0f, true);
        LoadOrCreateMat("RainGlass", new Color(0.25f, 0.30f, 0.36f, 0.22f), 0.9f, 0f, true, true, new Color(0.2f, 0.26f, 0.35f) * 0.8f);
        LoadOrCreateMat("MonitorOn", new Color(0.06f, 0.18f, 0.26f), 0.78f, 0f, false, true, new Color(0.10f, 0.45f, 0.60f) * 1.2f);
        LoadOrCreateMat("Paper", new Color(0.86f, 0.84f, 0.78f), 0.05f, 0f);
        LoadOrCreateMat("StickyNote", new Color(0.92f, 0.88f, 0.35f), 0.05f, 0f);
        LoadOrCreateMat("WarmLamp", new Color(0.95f, 0.72f, 0.42f), 0.2f, 0f, false, true, new Color(1f, 0.72f, 0.32f) * 2f);
        LoadOrCreateMat("Blinds", new Color(0.34f, 0.35f, 0.36f), 0.28f, 0.2f);
        LoadOrCreateMat("PlantLeaf", new Color(0.16f, 0.30f, 0.18f), 0.22f, 0f);
        LoadOrCreateMat("RedString", new Color(0.64f, 0.10f, 0.10f), 0.12f, 0f);
        LoadOrCreateMat("WhiteBoard", new Color(0.80f, 0.82f, 0.84f), 0.45f, 0f);
        LoadOrCreateMat("PlasticChair", new Color(0.20f, 0.20f, 0.22f), 0.22f, 0.05f);
        LoadOrCreateMat("DoorWood", new Color(0.30f, 0.20f, 0.10f), 0.26f, 0f);
        LoadOrCreateMat("ExitSign", new Color(0.12f, 0.42f, 0.18f), 0.15f, 0f, false, true, new Color(0.12f, 0.90f, 0.30f) * 0.8f);
    }

    static void BuildArchitecture(Transform parent)
    {
        CreateBox(parent, "HallwayFloor", new Vector3(3.4f, 0.1f, HallwayDepth), new Vector3(0f, -0.05f, RoomDepth * 0.5f + HallwayDepth * 0.5f), Mats["FloorCeramic"]);
        CreateBox(parent, "HallwayCeiling", new Vector3(3.4f, 0.08f, HallwayDepth), new Vector3(0f, RoomHeight + 0.04f, RoomDepth * 0.5f + HallwayDepth * 0.5f), Mats["AcousticCeiling"]);
        CreateBox(parent, "HallwayLeftWall", new Vector3(WallThickness, RoomHeight, HallwayDepth), new Vector3(-1.7f, RoomHeight * 0.5f, RoomDepth * 0.5f + HallwayDepth * 0.5f), Mats["ConcreteWall"]);
        CreateBox(parent, "HallwayRightWall", new Vector3(WallThickness, RoomHeight, HallwayDepth), new Vector3(1.7f, RoomHeight * 0.5f, RoomDepth * 0.5f + HallwayDepth * 0.5f), Mats["ConcreteWall"]);
        CreateBox(parent, "HallwayEndWall", new Vector3(3.4f, RoomHeight, WallThickness), new Vector3(0f, RoomHeight * 0.5f, RoomDepth * 0.5f + HallwayDepth), Mats["ConcreteWall"]);
    }

    static void BuildFloor(Transform parent)
    {
        CreateBox(parent, "OfficeFloor", new Vector3(RoomWidth, 0.1f, RoomDepth), new Vector3(0f, -0.05f, 0f), Mats["FloorCeramic"]);
        for (int i = 0; i < 4; i++)
        {
            float x = -6f + i * 4f;
            CreateBox(parent, "WetReflection_" + i, new Vector3(2.8f, 0.002f, 1.6f), new Vector3(x, 0.001f, -4.8f), Mats["RainGlass"]);
        }
    }

    static void BuildCeiling(Transform parent)
    {
        CreateBox(parent, "OfficeCeiling", new Vector3(RoomWidth, 0.08f, RoomDepth), new Vector3(0f, RoomHeight + 0.04f, 0f), Mats["AcousticCeiling"]);
        for (int i = 0; i < 3; i++)
        {
            float x = -5f + i * 5f;
            CreateBox(parent, "CeilingFixture_" + i, new Vector3(2.2f, 0.05f, 0.36f), new Vector3(x, RoomHeight - 0.02f, 0.2f), Mats["BlackMetal"]);
            CreateBox(parent, "FluorescentTube_" + i, new Vector3(1.8f, 0.03f, 0.16f), new Vector3(x, RoomHeight - 0.05f, 0.2f), Mats["WarmLamp"]);
        }
    }

    static void BuildWalls(Transform parent)
    {
        CreateBox(parent, "BackWall", new Vector3(RoomWidth, RoomHeight, WallThickness), new Vector3(0f, RoomHeight * 0.5f, -RoomDepth * 0.5f), Mats["ConcreteWall"]);
        CreateBox(parent, "LeftWall", new Vector3(WallThickness, RoomHeight, RoomDepth), new Vector3(-RoomWidth * 0.5f, RoomHeight * 0.5f, 0f), Mats["ConcreteWall"]);
        CreateBox(parent, "RightWall", new Vector3(WallThickness, RoomHeight, RoomDepth), new Vector3(RoomWidth * 0.5f, RoomHeight * 0.5f, 0f), Mats["ConcreteWall"]);

        // Front wall segmented for the single wooden door.
        CreateBox(parent, "FrontWall_Left", new Vector3(7f, RoomHeight, WallThickness), new Vector3(-5.5f, RoomHeight * 0.5f, RoomDepth * 0.5f), Mats["ConcreteWall"]);
        CreateBox(parent, "FrontWall_Right", new Vector3(7f, RoomHeight, WallThickness), new Vector3(5.5f, RoomHeight * 0.5f, RoomDepth * 0.5f), Mats["ConcreteWall"]);
        CreateBox(parent, "FrontWall_Header", new Vector3(2.2f, 0.7f, WallThickness), new Vector3(0f, 2.85f, RoomDepth * 0.5f), Mats["ConcreteWall"]);

        // Worn paint / water stain strips.
        CreateBox(parent, "BackWallStain", new Vector3(8f, 0.9f, 0.01f), new Vector3(-2f, 1.1f, -RoomDepth * 0.5f + 0.08f), Mats["WallStain"]);
        CreateBox(parent, "LeftWallStain", new Vector3(0.01f, 1.0f, 4.5f), new Vector3(-RoomWidth * 0.5f + 0.08f, 1.0f, 1.5f), Mats["WallStain"]);
        CreateBox(parent, "RightWallStain", new Vector3(0.01f, 0.8f, 3.6f), new Vector3(RoomWidth * 0.5f - 0.08f, 1.2f, -1.8f), Mats["WallStain"]);
    }

    static void BuildWindows(Transform parent)
    {
        float[] windowXs = { -5.0f, 0f, 5.0f };
        for (int i = 0; i < windowXs.Length; i++)
        {
            float x = windowXs[i];
            Transform window = CreateGroup(parent, "Window_" + (i + 1));
            window.localPosition = new Vector3(x, 1.8f, -RoomDepth * 0.5f + 0.02f);

            CreateBox(window, "FrameTop", new Vector3(2.8f, 0.08f, 0.08f), new Vector3(0f, 0.95f, 0f), Mats["BlackMetal"]);
            CreateBox(window, "FrameBottom", new Vector3(2.8f, 0.08f, 0.08f), new Vector3(0f, -0.95f, 0f), Mats["BlackMetal"]);
            CreateBox(window, "FrameLeft", new Vector3(0.08f, 1.9f, 0.08f), new Vector3(-1.36f, 0f, 0f), Mats["BlackMetal"]);
            CreateBox(window, "FrameRight", new Vector3(0.08f, 1.9f, 0.08f), new Vector3(1.36f, 0f, 0f), Mats["BlackMetal"]);
            CreateBox(window, "Glass", new Vector3(2.64f, 1.8f, 0.02f), Vector3.zero, Mats["DustyGlass"]);

            for (int b = 0; b < 5; b++)
            {
                float bx = -1f + b * 0.5f;
                CreateBox(window, "BarV_" + b, new Vector3(0.04f, 1.72f, 0.04f), new Vector3(bx, 0f, 0.02f), Mats["BlackMetal"]);
            }
            CreateBox(window, "BarH", new Vector3(2.55f, 0.04f, 0.04f), new Vector3(0f, 0f, 0.02f), Mats["BlackMetal"]);

            for (int s = 0; s < 6; s++)
            {
                float sy = 0.75f - s * 0.15f;
                float zOffset = (s < 3) ? 0f : 0.02f;
                CreateBox(window, "BlindSlat_" + s, new Vector3(2.46f, 0.05f, 0.07f), new Vector3(0f, sy, zOffset), Mats["Blinds"]);
            }

            // Rain planes outside the window.
            CreateBox(window, "RainLayerNear", new Vector3(2.7f, 1.9f, 0.02f), new Vector3(0f, 0f, -0.38f), Mats["RainGlass"]);
            CreateBox(window, "RainLayerFar", new Vector3(2.7f, 1.9f, 0.02f), new Vector3(0.05f, 0f, -0.62f), Mats["RainGlass"]);
        }
    }

    static void BuildDoorAndEntry(Transform parent)
    {
        CreateBox(parent, "WoodenDoor", new Vector3(1.15f, 2.3f, 0.06f), new Vector3(0f, 1.15f, RoomDepth * 0.5f - 0.03f), Mats["DoorWood"]);
        CreateBox(parent, "DoorFrameLeft", new Vector3(0.08f, 2.4f, 0.10f), new Vector3(-0.6f, 1.2f, RoomDepth * 0.5f - 0.03f), Mats["BlackMetal"]);
        CreateBox(parent, "DoorFrameRight", new Vector3(0.08f, 2.4f, 0.10f), new Vector3(0.6f, 1.2f, RoomDepth * 0.5f - 0.03f), Mats["BlackMetal"]);
        CreateBox(parent, "DoorFrameTop", new Vector3(1.25f, 0.08f, 0.10f), new Vector3(0f, 2.38f, RoomDepth * 0.5f - 0.03f), Mats["BlackMetal"]);
    }

    static void BuildReception(Transform parent)
    {
        CreateDesk(parent, "ReceptionDesk", new Vector3(0f, 0f, 4.6f), 180f, false);
        BuildVisitorChair(parent, "VisitorChair_A", new Vector3(-0.9f, 0f, 3.1f));
        BuildVisitorChair(parent, "VisitorChair_B", new Vector3(0.9f, 0f, 3.1f));
        CreateBox(parent, "ReceptionPhone", new Vector3(0.18f, 0.05f, 0.16f), new Vector3(-0.35f, 0.77f, 4.62f), Mats["BlackMetal"]);
    }

    static void BuildDetectiveDesks(Transform parent, Transform lightingParent)
    {
        float startX = -5.2f;
        float startZ = 1.8f;
        int index = 0;
        for (int row = 0; row < 2; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                float x = startX + col * 5.2f;
                float z = startZ - row * 4.4f;
                BuildDeskCluster(parent, lightingParent, index, new Vector3(x, 0f, z), row == 0 ? 0f : 180f);
                index++;
            }
        }
    }

    static void BuildDeskCluster(Transform parent, Transform lightingParent, int index, Vector3 pos, float rotY)
    {
        Transform desk = CreateGroup(parent, "DeskCluster_" + index);
        desk.localPosition = pos;
        desk.localRotation = Quaternion.Euler(0f, rotY, 0f);

        CreateDesk(desk, "Desk", Vector3.zero, 0f, true);
        BuildOfficeChair(desk, "Chair", new Vector3(0f, 0f, rotY < 90f ? -0.9f : 0.9f));
        CreateBox(desk, "ComputerMonitor", new Vector3(0.50f, 0.30f, 0.04f), new Vector3(-0.2f, 0.96f, 0f), Mats["MonitorOn"]);
        CreateBox(desk, "Keyboard", new Vector3(0.36f, 0.02f, 0.13f), new Vector3(0.05f, 0.78f, 0.12f), Mats["BlackMetal"]);
        CreateBox(desk, "Mouse", new Vector3(0.05f, 0.02f, 0.07f), new Vector3(0.28f, 0.78f, 0.12f), Mats["BlackMetal"]);
        CreateBox(desk, "DeskLamp", new Vector3(0.14f, 0.12f, 0.14f), new Vector3(0.58f, 0.98f, -0.24f), Mats["WarmLamp"]);
        CreateBox(desk, "CoffeeMug", new Vector3(0.08f, 0.09f, 0.08f), new Vector3(0.55f, 0.80f, 0.24f), Mats["Paper"]);
        CreateBox(desk, "PoliceRadio", new Vector3(0.16f, 0.05f, 0.11f), new Vector3(-0.55f, 0.79f, 0.2f), Mats["BlackMetal"]);
        CreateBox(desk, "Telephone", new Vector3(0.20f, 0.05f, 0.15f), new Vector3(-0.55f, 0.79f, -0.2f), Mats["BlackMetal"]);
        CreatePaperScatter(desk, "Documents", new Vector3(-0.10f, 0.782f, -0.2f));
        CreateBox(desk, "Folder", new Vector3(0.30f, 0.02f, 0.24f), new Vector3(0.18f, 0.79f, -0.2f), Mats["StickyNote"]);

        // Electrical details.
        CreateBox(desk, "PowerStrip", new Vector3(0.24f, 0.03f, 0.08f), new Vector3(0f, 0.05f, -0.26f), Mats["BlackMetal"]);
        CreateBox(desk, "CableBundle", new Vector3(0.02f, 0.46f, 0.02f), new Vector3(-0.2f, 0.46f, 0f), Mats["BlackMetal"]);
        CreateBox(desk, "TrashBin", new Vector3(0.20f, 0.28f, 0.20f), new Vector3(0.62f, 0.14f, 0.36f), Mats["BlackMetal"]);

        // Desk lamp light.
        GameObject lampLightObj = new GameObject("DeskLampLight_" + index);
        lampLightObj.transform.SetParent(lightingParent);
        lampLightObj.transform.position = desk.TransformPoint(new Vector3(0.55f, 1.1f, -0.2f));
        Light lampLight = lampLightObj.AddComponent<Light>();
        lampLight.type = LightType.Point;
        lampLight.color = new Color(1f, 0.79f, 0.54f);
        lampLight.intensity = 0.9f;
        lampLight.range = 2.8f;
        lampLight.shadows = LightShadows.Soft;
    }

    static void CreateDesk(Transform parent, string name, Vector3 pos, float rotY, bool includeDrawer)
    {
        Transform desk = CreateGroup(parent, name);
        desk.localPosition = pos;
        desk.localRotation = Quaternion.Euler(0f, rotY, 0f);

        CreateBox(desk, "Top", new Vector3(1.7f, 0.06f, 0.85f), new Vector3(0f, 0.76f, 0f), Mats["DarkWood"]);
        Vector3[] legs =
        {
            new Vector3(-0.78f, 0.38f, -0.35f), new Vector3(0.78f, 0.38f, -0.35f),
            new Vector3(-0.78f, 0.38f, 0.35f), new Vector3(0.78f, 0.38f, 0.35f)
        };
        for (int i = 0; i < legs.Length; i++) CreateBox(desk, "Leg_" + i, new Vector3(0.06f, 0.76f, 0.06f), legs[i], Mats["BlackMetal"]);
        if (includeDrawer) CreateBox(desk, "Drawer", new Vector3(0.66f, 0.30f, 0.50f), new Vector3(0.45f, 0.55f, 0f), Mats["DarkWood"]);
    }

    static void BuildVisitorChair(Transform parent, string name, Vector3 pos)
    {
        Transform chair = CreateGroup(parent, name);
        chair.localPosition = pos;
        CreateBox(chair, "Seat", new Vector3(0.54f, 0.07f, 0.54f), new Vector3(0f, 0.47f, 0f), Mats["PlasticChair"]);
        CreateBox(chair, "Back", new Vector3(0.54f, 0.62f, 0.06f), new Vector3(0f, 0.83f, -0.24f), Mats["PlasticChair"]);
        CreateBox(chair, "LegA", new Vector3(0.05f, 0.46f, 0.05f), new Vector3(-0.22f, 0.23f, -0.22f), Mats["BlackMetal"]);
        CreateBox(chair, "LegB", new Vector3(0.05f, 0.46f, 0.05f), new Vector3(0.22f, 0.23f, -0.22f), Mats["BlackMetal"]);
        CreateBox(chair, "LegC", new Vector3(0.05f, 0.46f, 0.05f), new Vector3(-0.22f, 0.23f, 0.22f), Mats["BlackMetal"]);
        CreateBox(chair, "LegD", new Vector3(0.05f, 0.46f, 0.05f), new Vector3(0.22f, 0.23f, 0.22f), Mats["BlackMetal"]);
    }

    static void BuildOfficeChair(Transform parent, string name, Vector3 pos)
    {
        Transform chair = CreateGroup(parent, name);
        chair.localPosition = pos;
        CreateBox(chair, "Seat", new Vector3(0.52f, 0.08f, 0.52f), new Vector3(0f, 0.50f, 0f), Mats["PlasticChair"]);
        CreateBox(chair, "Back", new Vector3(0.52f, 0.72f, 0.06f), new Vector3(0f, 0.90f, -0.24f), Mats["PlasticChair"]);
        CreateBox(chair, "Stem", new Vector3(0.07f, 0.35f, 0.07f), new Vector3(0f, 0.20f, 0f), Mats["BlackMetal"]);
        CreateBox(chair, "Base", new Vector3(0.52f, 0.03f, 0.52f), new Vector3(0f, 0.06f, 0f), Mats["BlackMetal"]);
    }

    static void CreatePaperScatter(Transform parent, string name, Vector3 center)
    {
        Transform scatter = CreateGroup(parent, name);
        scatter.localPosition = center;
        for (int i = 0; i < 5; i++)
        {
            GameObject sheet = CreateBox(scatter, "Paper_" + i, new Vector3(0.20f, 0.002f, 0.28f), new Vector3((i % 3) * 0.05f, i * 0.002f, (i / 3) * -0.08f), Mats["Paper"]);
            sheet.transform.localRotation = Quaternion.Euler(0f, 12f * i, 0f);
        }
    }

    static void BuildInvestigationCorner(Transform parent)
    {
        Transform board = CreateGroup(parent, "EvidenceBoard");
        board.localPosition = new Vector3(8.35f, 1.6f, -1.8f);
        board.localRotation = Quaternion.Euler(0f, -90f, 0f);
        CreateBox(board, "Cork", new Vector3(0.08f, 1.7f, 3.2f), Vector3.zero, Mats["DarkWood"]);
        CreateBox(board, "CorkSurface", new Vector3(0.03f, 1.6f, 3.0f), new Vector3(0.04f, 0f, 0f), Mats["Paper"]);

        for (int i = 0; i < 8; i++)
        {
            float y = 0.65f - i * 0.18f;
            float z = -1.2f + (i % 4) * 0.8f;
            Material noteMat = (i % 2 == 0) ? Mats["Paper"] : Mats["StickyNote"];
            CreateBox(board, "PinItem_" + i, new Vector3(0.01f, 0.22f, 0.28f), new Vector3(0.06f, y, z), noteMat);
        }

        CreateBox(board, "StringA", new Vector3(0.01f, 0.01f, 1.6f), new Vector3(0.07f, 0.25f, -0.35f), Mats["RedString"]);
        CreateBox(board, "StringB", new Vector3(0.01f, 0.62f, 0.01f), new Vector3(0.07f, -0.08f, 0.45f), Mats["RedString"]);

        Transform whiteboard = CreateGroup(parent, "WhiteBoard");
        whiteboard.localPosition = new Vector3(8.2f, 1.5f, 2.2f);
        whiteboard.localRotation = Quaternion.Euler(0f, -90f, 0f);
        CreateBox(whiteboard, "Board", new Vector3(0.06f, 1.4f, 2.4f), Vector3.zero, Mats["WhiteBoard"]);
        CreateBox(whiteboard, "MarkerTray", new Vector3(0.12f, 0.03f, 1.4f), new Vector3(0f, -0.68f, 0f), Mats["BlackMetal"]);
    }

    static void BuildStorage(Transform parent)
    {
        for (int i = 0; i < 4; i++)
        {
            float x = -7.7f + i * 0.75f;
            CreateFilingCabinet(parent, "FilingCabinet_" + i, new Vector3(x, 0f, -5.8f));
        }

        for (int i = 0; i < 3; i++)
        {
            float x = 6.9f + i * 0.65f;
            CreateBox(parent, "Locker_" + i, new Vector3(0.56f, 1.95f, 0.56f), new Vector3(x, 0.98f, 5.3f), Mats["BlackMetal"]);
        }

        Transform shelf = CreateGroup(parent, "Bookshelf");
        shelf.localPosition = new Vector3(-6.4f, 0f, 5.6f);
        CreateBox(shelf, "Body", new Vector3(1.6f, 2.1f, 0.42f), new Vector3(0f, 1.05f, 0f), Mats["DarkWood"]);
        for (int i = 0; i < 4; i++) CreateBox(shelf, "Shelf_" + i, new Vector3(1.5f, 0.03f, 0.38f), new Vector3(0f, 0.36f + i * 0.45f, 0f), Mats["DarkWood"]);
        for (int i = 0; i < 12; i++)
        {
            float bx = -0.62f + (i % 6) * 0.24f;
            float by = 0.48f + (i / 6) * 0.45f;
            CreateBox(shelf, "Binder_" + i, new Vector3(0.12f, 0.28f, 0.24f), new Vector3(bx, by, 0f), Mats["Paper"]);
        }

        for (int i = 0; i < 5; i++)
        {
            float x = -2.6f + i * 0.65f;
            CreateBox(parent, "ArchiveBox_" + i, new Vector3(0.56f, 0.32f, 0.42f), new Vector3(x, 0.16f, -6.2f), Mats["Paper"]);
        }

        CreateBox(parent, "Printer", new Vector3(0.62f, 0.32f, 0.42f), new Vector3(6.2f, 0.90f, -5.7f), Mats["Paper"]);
        CreateBox(parent, "PrinterStand", new Vector3(0.72f, 0.75f, 0.50f), new Vector3(6.2f, 0.37f, -5.7f), Mats["BlackMetal"]);

        Transform dispenser = CreateGroup(parent, "WaterDispenser");
        dispenser.localPosition = new Vector3(7.2f, 0f, -4.5f);
        CreateBox(dispenser, "Body", new Vector3(0.45f, 1.1f, 0.45f), new Vector3(0f, 0.55f, 0f), Mats["Paper"]);
        CreateBox(dispenser, "Bottle", new Vector3(0.24f, 0.42f, 0.24f), new Vector3(0f, 1.34f, 0f), Mats["DustyGlass"]);
    }

    static void CreateFilingCabinet(Transform parent, string name, Vector3 pos)
    {
        Transform cab = CreateGroup(parent, name);
        cab.localPosition = pos;
        CreateBox(cab, "Body", new Vector3(0.56f, 1.35f, 0.62f), new Vector3(0f, 0.68f, 0f), Mats["BlackMetal"]);
        for (int i = 0; i < 3; i++)
        {
            CreateBox(cab, "Drawer_" + i, new Vector3(0.52f, 0.34f, 0.03f), new Vector3(0f, 0.25f + i * 0.4f, 0.32f), Mats["DarkWood"]);
            CreateBox(cab, "Handle_" + i, new Vector3(0.14f, 0.03f, 0.02f), new Vector3(0f, 0.25f + i * 0.4f, 0.35f), Mats["BlackMetal"]);
        }
    }

    static void BuildDecorations(Transform parent)
    {
        CreateBox(parent, "WallClock", new Vector3(0.45f, 0.45f, 0.04f), new Vector3(-7.9f, 2.5f, 1.8f), Mats["WhiteBoard"]);
        CreateBox(parent, "GovNoticeBoard", new Vector3(1.6f, 1.1f, 0.06f), new Vector3(-8.2f, 1.8f, 4.0f), Mats["DarkWood"]);
        CreateBox(parent, "Calendar", new Vector3(0.75f, 0.52f, 0.02f), new Vector3(8.2f, 2.2f, 0.5f), Mats["Paper"]);
        CreateBox(parent, "EmergencyExitSign", new Vector3(1.0f, 0.20f, 0.02f), new Vector3(0f, 2.8f, RoomDepth * 0.5f - 0.02f), Mats["ExitSign"]);
        CreateBox(parent, "NoticeBoardSmall", new Vector3(1.1f, 0.75f, 0.06f), new Vector3(7.8f, 1.9f, -4.5f), Mats["DarkWood"]);

        Transform plantA = CreateGroup(parent, "Plant_A");
        plantA.localPosition = new Vector3(-8.0f, 0f, -5.4f);
        CreateBox(plantA, "Pot", new Vector3(0.30f, 0.34f, 0.30f), new Vector3(0f, 0.17f, 0f), Mats["BlackMetal"]);
        CreateBox(plantA, "Leaf", new Vector3(0.22f, 0.58f, 0.12f), new Vector3(0f, 0.58f, 0f), Mats["PlantLeaf"]);

        Transform plantB = CreateGroup(parent, "Plant_B");
        plantB.localPosition = new Vector3(8.0f, 0f, 5.1f);
        CreateBox(plantB, "Pot", new Vector3(0.30f, 0.34f, 0.30f), new Vector3(0f, 0.17f, 0f), Mats["BlackMetal"]);
        CreateBox(plantB, "Leaf", new Vector3(0.22f, 0.58f, 0.12f), new Vector3(0f, 0.58f, 0f), Mats["PlantLeaf"]);
    }

    static void BuildMainLighting(Transform parent)
    {
        for (int i = 0; i < 3; i++)
        {
            float x = -5f + i * 5f;
            GameObject lightObj = new GameObject("FluorescentLight_" + i);
            lightObj.transform.SetParent(parent);
            lightObj.transform.position = new Vector3(x, RoomHeight - 0.05f, 0.2f);
            lightObj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Spot;
            light.color = new Color(0.62f, 0.72f, 0.90f);
            light.intensity = 1.15f;
            light.range = 11f;
            light.spotAngle = 90f;
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.92f;
        }

        float[] moonXs = { -5f, 0f, 5f };
        for (int i = 0; i < moonXs.Length; i++)
        {
            GameObject moon = new GameObject("Moonlight_" + i);
            moon.transform.SetParent(parent);
            moon.transform.position = new Vector3(moonXs[i], RoomHeight + 0.2f, -RoomDepth * 0.5f + 0.2f);
            moon.transform.rotation = Quaternion.Euler(56f, 0f, 0f);
            Light l = moon.AddComponent<Light>();
            l.type = LightType.Spot;
            l.color = new Color(0.38f, 0.48f, 0.82f);
            l.intensity = 2.3f;
            l.range = 9f;
            l.spotAngle = 34f;
            l.shadows = LightShadows.Soft;
            l.shadowStrength = 0.96f;
        }

        GameObject rainReflection = new GameObject("RainReflectionLight");
        rainReflection.transform.SetParent(parent);
        rainReflection.transform.position = new Vector3(0f, 0.25f, -4.8f);
        Light rainLight = rainReflection.AddComponent<Light>();
        rainLight.type = LightType.Point;
        rainLight.color = new Color(0.30f, 0.40f, 0.65f);
        rainLight.intensity = 0.55f;
        rainLight.range = 7f;
        rainLight.shadows = LightShadows.None;
    }

    static void BuildEffects(Transform parent)
    {
        string profilePath = "Assets/Settings/Police_Detective_Office_Night_Profile.asset";
        VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, profilePath);
        }

        Bloom bloom = profile.TryGet(out Bloom b) ? b : profile.Add<Bloom>(true);
        bloom.active = true;
        bloom.intensity.Override(0.2f);
        bloom.threshold.Override(1.0f);
        bloom.scatter.Override(0.55f);

        ColorAdjustments color = profile.TryGet(out ColorAdjustments ca) ? ca : profile.Add<ColorAdjustments>(true);
        color.active = true;
        color.postExposure.Override(-0.35f);
        color.contrast.Override(22f);
        color.saturation.Override(-24f);
        color.colorFilter.Override(new Color(0.88f, 0.92f, 1f));

        DepthOfField dof = profile.TryGet(out DepthOfField depth) ? depth : profile.Add<DepthOfField>(true);
        dof.active = true;
        dof.mode.Override(DepthOfFieldMode.Gaussian);
        dof.gaussianStart.Override(2.8f);
        dof.gaussianEnd.Override(13.5f);
        dof.highQualitySampling.Override(true);

        Vignette vignette = profile.TryGet(out Vignette vig) ? vig : profile.Add<Vignette>(true);
        vignette.active = true;
        vignette.intensity.Override(0.28f);
        vignette.smoothness.Override(0.45f);

        EditorUtility.SetDirty(profile);

        GameObject volumeGo = new GameObject("GlobalPostProcessVolume");
        volumeGo.transform.SetParent(parent);
        Volume volume = volumeGo.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 10f;
        volume.profile = profile;

        GameObject reflection = new GameObject("ReflectionProbe_Main");
        reflection.transform.SetParent(parent);
        reflection.transform.position = new Vector3(0f, 1.6f, 0f);
        ReflectionProbe probe = reflection.AddComponent<ReflectionProbe>();
        probe.size = new Vector3(RoomWidth, RoomHeight, RoomDepth);
        probe.mode = ReflectionProbeMode.Baked;
        probe.boxProjection = true;
        probe.intensity = 1f;

        GameObject lightProbeRoot = new GameObject("LightProbeGroup_Main");
        lightProbeRoot.transform.SetParent(parent);
        LightProbeGroup lightProbeGroup = lightProbeRoot.AddComponent<LightProbeGroup>();
        lightProbeGroup.probePositions = BuildProbeGrid(new Vector3(-7f, 0.5f, -5f), 5, 3, 4, new Vector3(3.5f, 1.1f, 3f));
    }

    static Vector3[] BuildProbeGrid(Vector3 start, int countX, int countY, int countZ, Vector3 step)
    {
        List<Vector3> probes = new List<Vector3>();
        for (int y = 0; y < countY; y++)
        {
            for (int z = 0; z < countZ; z++)
            {
                for (int x = 0; x < countX; x++)
                {
                    probes.Add(new Vector3(start.x + x * step.x, start.y + y * step.y, start.z + z * step.z));
                }
            }
        }
        return probes.ToArray();
    }

    static void BuildMainCamera(Transform parent)
    {
        GameObject camObj = new GameObject("Main Camera");
        camObj.transform.SetParent(parent);
        camObj.tag = "MainCamera";
        camObj.transform.position = new Vector3(0f, 1.7f, 6.2f);
        camObj.transform.rotation = Quaternion.Euler(8f, 188f, 0f);

        Camera cam = camObj.AddComponent<Camera>();
        cam.fieldOfView = 60f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.01f, 0.015f, 0.03f);
        camObj.AddComponent<AudioListener>();

        UniversalAdditionalCameraData urpData = camObj.AddComponent<UniversalAdditionalCameraData>();
        urpData.renderPostProcessing = true;
        urpData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        urpData.antialiasingQuality = AntialiasingQuality.High;
    }

    static void ConfigureAtmosphere()
    {
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.035f, 0.04f, 0.06f);
        RenderSettings.ambientEquatorColor = new Color(0.02f, 0.025f, 0.038f);
        RenderSettings.ambientGroundColor = new Color(0.01f, 0.012f, 0.018f);

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.028f, 0.035f, 0.05f);
        RenderSettings.fogDensity = 0.012f;
        RenderSettings.reflectionIntensity = 0.62f;
    }

    static void MarkSceneStatic(GameObject root)
    {
        Transform[] allTransforms = root.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in allTransforms)
        {
            GameObjectUtility.SetStaticEditorFlags(
                t.gameObject,
                StaticEditorFlags.BatchingStatic |
                StaticEditorFlags.ContributeGI |
                StaticEditorFlags.OccluderStatic |
                StaticEditorFlags.OccludeeStatic |
                StaticEditorFlags.ReflectionProbeStatic
            );
        }
    }

    static GameObject CreateBox(Transform parent, string name, Vector3 scale, Vector3 localPos, Material mat)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.SetParent(parent);
        box.transform.localScale = scale;
        box.transform.localPosition = localPos;
        box.transform.localRotation = Quaternion.identity;
        Renderer renderer = box.GetComponent<Renderer>();
        renderer.sharedMaterial = mat;
        return box;
    }
}
