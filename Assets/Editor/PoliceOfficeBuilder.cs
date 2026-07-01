using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class PoliceOfficeBuilder
{
    [MenuItem("Tools/Build Police Office Scene")]
    public static void Build()
    {
        // ==================== MATERIALS ====================
        Material wallMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        wallMat.color = new Color(0.22f, 0.23f, 0.24f);
        AssetDatabase.CreateAsset(wallMat, "Assets/Models/WallMaterial.mat");

        Material floorMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        floorMat.color = new Color(0.12f, 0.1f, 0.08f);
        AssetDatabase.CreateAsset(floorMat, "Assets/Models/FloorMaterial.mat");

        // ==================== ROOM STRUCTURE ====================
        GameObject room = new GameObject("Room");

        // --- SAN ---
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.SetParent(room.transform);
        floor.transform.localScale = new Vector3(8f, 0.1f, 8f);
        floor.transform.localPosition = new Vector3(0, -0.05f, 0);
        floor.GetComponent<Renderer>().sharedMaterial = floorMat;

        // --- TRAN ---
        GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ceiling.name = "Ceiling";
        ceiling.transform.SetParent(room.transform);
        ceiling.transform.localScale = new Vector3(8f, 0.1f, 8f);
        ceiling.transform.localPosition = new Vector3(0, 3.05f, 0);
        ceiling.GetComponent<Renderer>().sharedMaterial = wallMat;

        // --- TUONG ---
        string[] wallNames = { "Wall_Back", "Wall_Front", "Wall_Left", "Wall_Right" };
        Vector3[] wallScales = {
            new Vector3(8f, 3.1f, 0.15f),
            new Vector3(8f, 3.1f, 0.15f),
            new Vector3(0.15f, 3.1f, 8f),
            new Vector3(0.15f, 3.1f, 8f)
        };
        Vector3[] wallPositions = {
            new Vector3(0, 1.5f, -4f),
            new Vector3(0, 1.5f, 4f),
            new Vector3(-4f, 1.5f, 0),
            new Vector3(4f, 1.5f, 0)
        };
        for (int i = 0; i < 4; i++)
        {
            GameObject w = GameObject.CreatePrimitive(PrimitiveType.Cube);
            w.name = wallNames[i];
            w.transform.SetParent(room.transform);
            w.transform.localScale = wallScales[i];
            w.transform.localPosition = wallPositions[i];
            w.GetComponent<Renderer>().sharedMaterial = wallMat;
        }

        // ==================== LIGHTING ====================
        // Xoa directional light mac dinh
        Light[] existingLights = Object.FindObjectsByType<Light>();
        foreach (var l in existingLights)
        {
            if (l.type == LightType.Directional)
                GameObject.DestroyImmediate(l.gameObject);
        }

        // Den chinh: Spot light tren ban - anh sang gat huong xuong
        GameObject spotObj = new GameObject("SpotLight_Interrogation");
        Light spot = spotObj.AddComponent<Light>();
        spot.type = LightType.Spot;
        spot.color = new Color(1f, 0.92f, 0.72f);
        spot.intensity = 8f;
        spot.range = 6f;
        spot.spotAngle = 55f;
        spot.shadows = LightShadows.Hard;
        spotObj.transform.position = new Vector3(0, 2.9f, 0.1f);
        spotObj.transform.rotation = Quaternion.Euler(90f, 0, 0);

        // Den mau xanh lanh o goc phong (ambient u am)
        GameObject ambientObj = new GameObject("AmbientLight_Cold");
        Light ambient = ambientObj.AddComponent<Light>();
        ambient.type = LightType.Point;
        ambient.color = new Color(0.3f, 0.4f, 0.6f);
        ambient.intensity = 0.35f;
        ambient.range = 9f;
        ambientObj.transform.position = new Vector3(-3.2f, 2.5f, -3f);

        // ==================== CAMERA ====================
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            cam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
            camObj.AddComponent<AudioListener>();
        }
        cam.transform.position = new Vector3(2.8f, 1.75f, 3.5f);
        cam.transform.rotation = Quaternion.Euler(12f, -148f, 0);
        cam.backgroundColor = new Color(0.02f, 0.02f, 0.03f);
        cam.clearFlags = CameraClearFlags.SolidColor;

        // ==================== FURNITURE (FBX) ====================
        string fbxPath = "Assets/Models/PoliceOfficeFurniture.fbx";
        GameObject furniturePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        if (furniturePrefab != null)
        {
            GameObject furniture = (GameObject)PrefabUtility.InstantiatePrefab(furniturePrefab);
            furniture.name = "Furniture";
            furniture.transform.position = Vector3.zero;
        }
        else
        {
            Debug.LogWarning("[PoliceOffice] FBX furniture not found at: " + fbxPath);
        }

        // ==================== VILLAIN1 (Nghi Pham) ====================
        string villain1Path = "Assets/Villain1/Villain1.obj";
        GameObject villain1Asset = AssetDatabase.LoadAssetAtPath<GameObject>(villain1Path);
        if (villain1Asset != null)
        {
            GameObject villain = (GameObject)PrefabUtility.InstantiatePrefab(villain1Asset);
            villain.name = "Villain1_Suspect";
            villain.transform.position = new Vector3(0f, 0f, -0.9f);
            villain.transform.rotation = Quaternion.Euler(0, 180f, 0);
        }
        else
        {
            Debug.LogWarning("[PoliceOffice] Villain1 not found at: " + villain1Path);
        }

        // ==================== SAVE ====================
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        AssetDatabase.Refresh();
        Debug.Log("[PoliceOffice] Scene built successfully!");
    }
}
