using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Redresses the ReceptionArea inside Police_Reception_Office_Day to match the reference photo:
/// dark wood paneling, central police emblem, gold slogan, two red poster panels and a
/// long wooden counter with monitor / TIEP DAN sign / pen holder / folders / plant.
/// Keeps Door, spawn points and any player object untouched.
/// </summary>
public static class PoliceReceptionRedress
{
    const string ScenePath = "Assets/Scenes/Police_Reception_Office_Day.unity";
    const string MatFolder = "Assets/Materials/PoliceReceptionOffice";

    static Material _mahoganyPanel;
    static Material _deskWood;
    static Material _deskTop;
    static Material _gold;
    static Material _goldEmissive;
    static Material _emblemRed;
    static Material _posterRed;
    static Material _monitorBlack;
    static Material _screen;
    static Material _potWhite;
    static Material _leafGreen;
    static Material _folderBlue;
    static Material _folderOrange;
    static Material _folderDark;
    static Material _pen;
    static Material _penHolder;
    static Material _plateBaseGold;

    [MenuItem("Tools/Redress Police Reception (Match Reference)")]
    public static void Redress()
    {
        if (SceneManager.GetActiveScene().path != ScenePath)
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        var area = GameObject.Find("ReceptionArea");
        if (area == null)
        {
            Debug.LogError("[PoliceReceptionRedress] ReceptionArea not found.");
            return;
        }

        EnsureMaterials();

        RemoveOldVisuals(area.transform);

        // Wall panel (dark wood, full reception backdrop)
        var bgWall = CreateBox(area.transform, "BackdropWoodLight", new Vector3(7.4f, 2.85f, 0.06f), new Vector3(0f, 1.43f, -4.83f), _mahoganyPanel);
        // Central darker wood panel where emblem and slogan sit
        CreateBox(area.transform, "CentralPanel", new Vector3(5.4f, 2.55f, 0.04f), new Vector3(0f, 1.43f, -4.80f), _deskWood);

        // Police emblem (approximation: gold ring + red disc + gold star)
        var emblemRing = CreateCylinder(area.transform, "Emblem_Ring", new Vector3(0.95f, 0.025f, 0.95f), new Vector3(0f, 2.05f, -4.78f), Quaternion.Euler(90f, 0f, 0f), _gold);
        CreateCylinder(area.transform, "Emblem_Disc", new Vector3(0.75f, 0.03f, 0.75f), new Vector3(0f, 2.05f, -4.77f), Quaternion.Euler(90f, 0f, 0f), _emblemRed);
        CreateCylinder(area.transform, "Emblem_StarPad", new Vector3(0.40f, 0.04f, 0.40f), new Vector3(0f, 2.05f, -4.76f), Quaternion.Euler(90f, 0f, 0f), _goldEmissive);
        // Bottom gold strip below emblem (the "rice ear" wreath stylized as horizontal gold bar)
        CreateBox(area.transform, "Emblem_Wreath", new Vector3(1.20f, 0.06f, 0.03f), new Vector3(0f, 1.50f, -4.77f), _gold);

        // Gold slogan text
        CreateText(area.transform, "Text_Slogan_Big", "VI NHAN DAN PHUC VU", 0.022f, new Vector3(0f, 1.30f, -4.76f), new Color(1f, 0.85f, 0.35f), FontStyle.Bold);
        CreateText(area.transform, "Text_Slogan_Small", "CONG AN NHAN DAN", 0.012f, new Vector3(0f, 1.05f, -4.76f), new Color(1f, 0.85f, 0.35f), FontStyle.Normal);

        // Poster on visitor's LEFT (world +x, since camera faces -z so +x is screen left) -> "6 DIEU"
        CreateBox(area.transform, "PosterLeft_Frame", new Vector3(1.30f, 2.05f, 0.04f), new Vector3(2.80f, 1.50f, -4.80f), _gold);
        CreateBox(area.transform, "PosterLeft", new Vector3(1.15f, 1.90f, 0.05f), new Vector3(2.80f, 1.50f, -4.78f), _posterRed);
        CreateText(area.transform, "PosterLeft_Title", "6 DIEU", 0.020f, new Vector3(2.80f, 2.32f, -4.76f), new Color(1f, 0.88f, 0.30f), FontStyle.Bold);
        CreateText(area.transform, "PosterLeft_Sub", "BAC HO DAY", 0.011f, new Vector3(2.80f, 2.21f, -4.76f), new Color(1f, 0.88f, 0.30f), FontStyle.Bold);
        CreateText(area.transform, "PosterLeft_Sub2", "CONG AN NHAN DAN", 0.009f, new Vector3(2.80f, 2.13f, -4.76f), new Color(1f, 0.88f, 0.30f), FontStyle.Bold);
        CreateText(area.transform, "PosterLeft_Body",
            "1. Doi voi tu minh, phai\n   can, kiem, liem, chinh.\n" +
            "2. Doi voi dong su, phai\n   than ai giup do.\n" +
            "3. Doi voi chinh phu, phai\n   tuyet doi trung thanh.\n" +
            "4. Doi voi nhan dan, phai\n   kinh trong, le phep.\n" +
            "5. Doi voi cong viec, phai\n   tan tuy.\n" +
            "6. Doi voi dich, phai\n   cuong quyet, khon kheo.",
            0.006f, new Vector3(2.80f, 1.45f, -4.76f), new Color(0.98f, 0.94f, 0.85f), FontStyle.Normal);

        // Poster on visitor's RIGHT (world -x) -> "TU CACH"
        CreateBox(area.transform, "PosterRight_Frame", new Vector3(1.30f, 2.05f, 0.04f), new Vector3(-2.80f, 1.50f, -4.80f), _gold);
        CreateBox(area.transform, "PosterRight", new Vector3(1.15f, 1.90f, 0.05f), new Vector3(-2.80f, 1.50f, -4.78f), _posterRed);
        CreateText(area.transform, "PosterRight_Title", "TU CACH", 0.020f, new Vector3(-2.80f, 2.32f, -4.76f), new Color(1f, 0.88f, 0.30f), FontStyle.Bold);
        CreateText(area.transform, "PosterRight_Sub", "NGUOI CONG AN", 0.011f, new Vector3(-2.80f, 2.21f, -4.76f), new Color(1f, 0.88f, 0.30f), FontStyle.Bold);
        CreateText(area.transform, "PosterRight_Sub2", "CACH MENH LA", 0.010f, new Vector3(-2.80f, 2.13f, -4.76f), new Color(1f, 0.88f, 0.30f), FontStyle.Bold);
        CreateText(area.transform, "PosterRight_Body",
            "Doi voi tu minh, phai\n   CAN, KIEM, LIEM, CHINH,\n   THAN AI GIUP DO.\n" +
            "Doi voi Chinh phu, phai\n   TUYET DOI TRUNG THANH.\n" +
            "Doi voi nhan dan, phai\n   KINH TRONG, LE PHEP.\n" +
            "Doi voi cong viec, phai\n   TAN TUY.\n" +
            "Doi voi dich, phai\n   CUONG QUYET, KHON KHEO.",
            0.006f, new Vector3(-2.80f, 1.45f, -4.76f), new Color(0.98f, 0.94f, 0.85f), FontStyle.Normal);

        // Long counter desk
        CreateBox(area.transform, "CounterBody", new Vector3(6.0f, 0.85f, 1.1f), new Vector3(0f, 0.42f, -3.30f), _deskWood);
        CreateBox(area.transform, "CounterTop", new Vector3(6.2f, 0.08f, 1.2f), new Vector3(0f, 0.86f, -3.30f), _deskTop);
        // Decorative front panel insets
        CreateBox(area.transform, "CounterFront_PanelL", new Vector3(1.6f, 0.55f, 0.02f), new Vector3(-1.8f, 0.40f, -2.78f), _deskTop);
        CreateBox(area.transform, "CounterFront_PanelC", new Vector3(1.6f, 0.55f, 0.02f), new Vector3(0f, 0.40f, -2.78f), _deskTop);
        CreateBox(area.transform, "CounterFront_PanelR", new Vector3(1.6f, 0.55f, 0.02f), new Vector3(1.8f, 0.40f, -2.78f), _deskTop);

        // ---- Desk items, viewed from visitor (+z side) looking toward -z. World +x = visitor's LEFT. ----

        // TIEP DAN plate — visitor's LEFT edge of desk (world +x)
        CreateBox(area.transform, "TiepDan_Base", new Vector3(0.62f, 0.20f, 0.20f), new Vector3(1.45f, 1.00f, -3.05f), _posterRed);
        CreateBox(area.transform, "TiepDan_GoldBand", new Vector3(0.66f, 0.03f, 0.22f), new Vector3(1.45f, 1.115f, -3.05f), _gold);
        CreateBox(area.transform, "TiepDan_GoldBandLow", new Vector3(0.66f, 0.03f, 0.22f), new Vector3(1.45f, 0.895f, -3.05f), _gold);
        CreateCylinder(area.transform, "TiepDan_EmblemDot", new Vector3(0.07f, 0.01f, 0.07f), new Vector3(1.71f, 1.00f, -2.945f), Quaternion.Euler(90f, 0f, 0f), _goldEmissive);
        CreateText(area.transform, "TiepDan_Text", "TIEP DAN", 0.013f, new Vector3(1.42f, 1.00f, -2.93f), new Color(1f, 0.86f, 0.30f), FontStyle.Bold);

        // Monitor (with stand) — between TIEP DAN and center
        CreateBox(area.transform, "MonitorStand_Base", new Vector3(0.30f, 0.025f, 0.20f), new Vector3(0.60f, 0.915f, -3.40f), _monitorBlack);
        CreateBox(area.transform, "MonitorStand_Neck", new Vector3(0.05f, 0.18f, 0.06f), new Vector3(0.60f, 1.01f, -3.40f), _monitorBlack);
        CreateBox(area.transform, "MonitorFrame", new Vector3(0.75f, 0.50f, 0.05f), new Vector3(0.60f, 1.30f, -3.42f), _monitorBlack);
        CreateBox(area.transform, "MonitorScreen", new Vector3(0.69f, 0.43f, 0.02f), new Vector3(0.60f, 1.30f, -3.395f), _screen);

        // Pen holder + 3 pens — near center
        CreateCylinder(area.transform, "PenHolder", new Vector3(0.07f, 0.07f, 0.07f), new Vector3(0.05f, 0.97f, -3.15f), Quaternion.identity, _penHolder);
        CreateCylinder(area.transform, "Pen1", new Vector3(0.012f, 0.10f, 0.012f), new Vector3(0.03f, 1.07f, -3.15f), Quaternion.Euler(15f, 12f, 0f), _pen);
        CreateCylinder(area.transform, "Pen2", new Vector3(0.012f, 0.10f, 0.012f), new Vector3(0.05f, 1.07f, -3.13f), Quaternion.Euler(8f, -15f, 0f), _pen);
        CreateCylinder(area.transform, "Pen3", new Vector3(0.012f, 0.10f, 0.012f), new Vector3(0.07f, 1.07f, -3.17f), Quaternion.Euler(-5f, 0f, 0f), _pen);

        // Folder stack — visitor's right of center
        CreateBox(area.transform, "FolderBlue", new Vector3(0.50f, 0.05f, 0.36f), new Vector3(-0.55f, 0.92f, -3.25f), _folderBlue);
        CreateBox(area.transform, "FolderOrange", new Vector3(0.50f, 0.04f, 0.34f), new Vector3(-0.50f, 0.965f, -3.27f), _folderOrange);
        CreateBox(area.transform, "FolderBlue2", new Vector3(0.45f, 0.03f, 0.32f), new Vector3(-0.52f, 1.00f, -3.23f), _folderBlue);

        // Plant — right of folders
        CreateCylinder(area.transform, "PlantPot", new Vector3(0.12f, 0.10f, 0.12f), new Vector3(-1.05f, 0.99f, -3.20f), Quaternion.identity, _potWhite);
        CreateSphere(area.transform, "PlantLeaves", new Vector3(0.30f, 0.30f, 0.30f), new Vector3(-1.05f, 1.16f, -3.20f), _leafGreen);

        // File holders (vertical box files on visitor's right edge)
        CreateBox(area.transform, "FileHolder_Orange", new Vector3(0.10f, 0.40f, 0.30f), new Vector3(-1.45f, 1.10f, -3.30f), _folderOrange);
        CreateBox(area.transform, "FileHolder_Blue", new Vector3(0.10f, 0.40f, 0.30f), new Vector3(-1.57f, 1.10f, -3.30f), _folderBlue);
        CreateBox(area.transform, "FileHolder_Dark", new Vector3(0.10f, 0.40f, 0.30f), new Vector3(-1.69f, 1.10f, -3.30f), _folderDark);
        CreateBox(area.transform, "FileHolder_Orange2", new Vector3(0.10f, 0.40f, 0.30f), new Vector3(-1.81f, 1.10f, -3.30f), _folderOrange);

        // Wall trim (lower wood baseboard along bottom of central panel)
        CreateBox(area.transform, "WallTrim_Bottom", new Vector3(7.4f, 0.12f, 0.05f), new Vector3(0f, 0.06f, -4.815f), _deskWood);
        CreateBox(area.transform, "WallTrim_Top", new Vector3(7.4f, 0.08f, 0.06f), new Vector3(0f, 2.82f, -4.815f), _deskWood);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        Debug.Log("[PoliceReceptionRedress] Redress complete.");
    }

    static void RemoveOldVisuals(Transform area)
    {
        var keep = new System.Collections.Generic.HashSet<string>
        {
            "Door", "BlueSign_Door",
            "PlayerSpawn", "ReceptionDoorSpawn", "DefaultPlayerSpawn"
        };
        var doomed = new System.Collections.Generic.List<Transform>();
        foreach (Transform c in area)
        {
            if (!keep.Contains(c.name))
                doomed.Add(c);
        }
        foreach (var t in doomed)
            Object.DestroyImmediate(t.gameObject);
    }

    static void EnsureMaterials()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");
        if (!AssetDatabase.IsValidFolder(MatFolder))
            AssetDatabase.CreateFolder("Assets/Materials", "PoliceReceptionOffice");

        _mahoganyPanel = MakeMat("MahoganyPanel", new Color(0.22f, 0.11f, 0.06f), 0.30f, 0f, Color.black);
        _deskWood = MakeMat("DeskWood", new Color(0.36f, 0.20f, 0.10f), 0.40f, 0f, Color.black);
        _deskTop = MakeMat("DeskTopWood", new Color(0.45f, 0.26f, 0.13f), 0.55f, 0f, Color.black);
        _gold = MakeMat("Gold", new Color(0.96f, 0.78f, 0.30f), 0.80f, 0.85f, new Color(0.20f, 0.15f, 0.04f));
        _goldEmissive = MakeMat("GoldEmissive", new Color(1f, 0.85f, 0.38f), 0.85f, 0.85f, new Color(0.45f, 0.32f, 0.10f));
        _emblemRed = MakeMat("EmblemRed", new Color(0.65f, 0.10f, 0.10f), 0.4f, 0f, new Color(0.18f, 0.02f, 0.02f));
        _posterRed = MakeMat("PosterRed", new Color(0.58f, 0.10f, 0.10f), 0.25f, 0f, Color.black);
        _monitorBlack = MakeMat("MonitorBlack", new Color(0.05f, 0.05f, 0.06f), 0.85f, 0.3f, Color.black);
        _screen = MakeMat("ScreenOff", new Color(0.05f, 0.06f, 0.09f), 0.95f, 0.2f, Color.black);
        _potWhite = MakeMat("PotWhite", new Color(0.94f, 0.93f, 0.90f), 0.20f, 0f, Color.black);
        _leafGreen = MakeMat("LeafGreen", new Color(0.18f, 0.40f, 0.18f), 0.20f, 0f, Color.black);
        _folderBlue = MakeMat("FolderBlue", new Color(0.10f, 0.20f, 0.50f), 0.20f, 0f, Color.black);
        _folderOrange = MakeMat("FolderOrange", new Color(0.84f, 0.48f, 0.18f), 0.22f, 0f, Color.black);
        _folderDark = MakeMat("FolderDark", new Color(0.20f, 0.13f, 0.10f), 0.20f, 0f, Color.black);
        _pen = MakeMat("PenDark", new Color(0.10f, 0.10f, 0.12f), 0.6f, 0.2f, Color.black);
        _penHolder = MakeMat("PenHolder", new Color(0.30f, 0.30f, 0.32f), 0.5f, 0.4f, Color.black);
        _plateBaseGold = MakeMat("PlateGold", new Color(0.95f, 0.78f, 0.30f), 0.7f, 0.5f, Color.black);
    }

    static Material MakeMat(string name, Color color, float smooth, float metal, Color emiss)
    {
        var path = MatFolder + "/" + name + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(m, path);
        }
        m.color = color;
        m.SetFloat("_Smoothness", smooth);
        m.SetFloat("_Metallic", metal);
        if (emiss.maxColorComponent > 0.01f)
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", emiss);
        }
        else
        {
            m.DisableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", Color.black);
        }
        EditorUtility.SetDirty(m);
        return m;
    }

    static GameObject CreateBox(Transform parent, string name, Vector3 scale, Vector3 localPos, Material mat)
    {
        var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.SetParent(parent);
        box.transform.localScale = scale;
        box.transform.localPosition = localPos;
        var col = box.GetComponent<Collider>();
        if (col != null) Object.DestroyImmediate(col);
        box.GetComponent<Renderer>().sharedMaterial = mat;
        return box;
    }

    static GameObject CreateCylinder(Transform parent, string name, Vector3 scale, Vector3 localPos, Quaternion rot, Material mat)
    {
        var c = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        c.name = name;
        c.transform.SetParent(parent);
        c.transform.localScale = scale;
        c.transform.localPosition = localPos;
        c.transform.localRotation = rot;
        var col = c.GetComponent<Collider>();
        if (col != null) Object.DestroyImmediate(col);
        c.GetComponent<Renderer>().sharedMaterial = mat;
        return c;
    }

    static GameObject CreateSphere(Transform parent, string name, Vector3 scale, Vector3 localPos, Material mat)
    {
        var s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        s.name = name;
        s.transform.SetParent(parent);
        s.transform.localScale = scale;
        s.transform.localPosition = localPos;
        var col = s.GetComponent<Collider>();
        if (col != null) Object.DestroyImmediate(col);
        s.GetComponent<Renderer>().sharedMaterial = mat;
        return s;
    }

    static void CreateText(Transform parent, string name, string text, float charSize, Vector3 localPos, Color color, FontStyle style)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.localPosition = localPos;
        // Text faces +Z (toward room interior) so viewer standing at +z can read it.
        go.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        var tm = go.AddComponent<TextMesh>();
        tm.text = text;
        tm.characterSize = charSize;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.fontSize = 96;
        tm.fontStyle = style;
        tm.color = color;
        var mr = go.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
        }
    }
}
