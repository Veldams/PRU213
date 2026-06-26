using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class PoliceOfficeFixer
{
    [MenuItem("Tools/Fix Police Office Materials & Furniture")]
    public static void FixScene()
    {
        // Tao folder Materials neu chua co
        if (!AssetDatabase.IsValidFolder("Assets/Models"))
            AssetDatabase.CreateFolder("Assets", "Models");
        if (!AssetDatabase.IsValidFolder("Assets/Models/Materials"))
            AssetDatabase.CreateFolder("Assets/Models", "Materials");

        // Wall material
        Material wallMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Models/Materials/WallMaterial.mat");
        if (wallMat == null)
        {
            wallMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            wallMat.color = new Color(0.22f, 0.23f, 0.24f);
            AssetDatabase.CreateAsset(wallMat, "Assets/Models/Materials/WallMaterial.mat");
        }

        // Floor material
        Material floorMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Models/Materials/FloorMaterial.mat");
        if (floorMat == null)
        {
            floorMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            floorMat.color = new Color(0.12f, 0.10f, 0.08f);
            AssetDatabase.CreateAsset(floorMat, "Assets/Models/Materials/FloorMaterial.mat");
        }

        // Ap dung material vao Room objects
        GameObject room = GameObject.Find("Room");
        if (room != null)
        {
            foreach (Transform child in room.transform)
            {
                var rend = child.GetComponent<Renderer>();
                if (rend == null) continue;
                if (child.name == "Floor")
                    rend.sharedMaterial = floorMat;
                else
                    rend.sharedMaterial = wallMat;
            }
        }
        else
        {
            Debug.LogWarning("[PoliceOffice] 'Room' GameObject not found. Run 'Tools/Build Police Office Scene' first.");
        }

        // Tao furniture neu chua co
        if (GameObject.Find("Furniture") == null)
        {
            CreatePrimitiveFurniture();
        }

        // Load Villain1
        if (GameObject.Find("Villain1_Suspect") == null)
        {
            string villain1Path = "Assets/Villain1/Villain1.obj";
            GameObject villain1Asset = AssetDatabase.LoadAssetAtPath<GameObject>(villain1Path);
            if (villain1Asset != null)
            {
                GameObject villain = (GameObject)PrefabUtility.InstantiatePrefab(villain1Asset);
                villain.name = "Villain1_Suspect";
                villain.transform.position = new Vector3(0f, 0f, -0.85f);
                villain.transform.rotation = Quaternion.Euler(0, 180f, 0);
                villain.transform.localScale = new Vector3(1f, 1f, 1f);
                Debug.Log("[PoliceOffice] Villain1 added as suspect.");
            }
            else
            {
                Debug.LogWarning("[PoliceOffice] Villain1.obj not found at: " + villain1Path);
            }
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        AssetDatabase.Refresh();
        Debug.Log("[PoliceOffice] Fix complete! Scene saved.");
    }

    static void CreatePrimitiveFurniture()
    {
        GameObject parent = new GameObject("Furniture");

        Material darkWood = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        darkWood.color = new Color(0.18f, 0.12f, 0.07f);

        Material darkMetal = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        darkMetal.color = new Color(0.15f, 0.15f, 0.15f);

        Material cabinetMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        cabinetMat.color = new Color(0.25f, 0.22f, 0.15f);

        // --- BAN HOI CUNG ---
        GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cube);
        table.name = "InterrogationTable";
        table.transform.SetParent(parent.transform);
        table.transform.localScale = new Vector3(1.6f, 0.08f, 0.8f);
        table.transform.localPosition = new Vector3(0, 0.74f, 0.1f);
        table.GetComponent<Renderer>().sharedMaterial = darkWood;

        // Chan ban
        Vector3[] legPos = { new Vector3(-0.7f,0.37f,-0.25f), new Vector3(0.7f,0.37f,-0.25f), new Vector3(-0.7f,0.37f,0.45f), new Vector3(0.7f,0.37f,0.45f) };
        foreach (var lp in legPos)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leg.transform.SetParent(parent.transform);
            leg.transform.localScale = new Vector3(0.07f, 0.74f, 0.07f);
            leg.transform.localPosition = lp;
            leg.GetComponent<Renderer>().sharedMaterial = darkMetal;
        }

        // --- GHE NGHI PHAM ---
        GameObject seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
        seat.name = "SuspectChair_Seat";
        seat.transform.SetParent(parent.transform);
        seat.transform.localScale = new Vector3(0.5f, 0.07f, 0.5f);
        seat.transform.localPosition = new Vector3(0, 0.44f, -0.75f);
        seat.GetComponent<Renderer>().sharedMaterial = darkMetal;

        GameObject seatBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
        seatBack.name = "SuspectChair_Back";
        seatBack.transform.SetParent(parent.transform);
        seatBack.transform.localScale = new Vector3(0.5f, 0.7f, 0.06f);
        seatBack.transform.localPosition = new Vector3(0, 0.82f, -1.0f);
        seatBack.GetComponent<Renderer>().sharedMaterial = darkMetal;

        // Chan ghe nghi pham
        Vector3[] slegPos = { new Vector3(-0.22f,0.22f,-0.52f), new Vector3(0.22f,0.22f,-0.52f), new Vector3(-0.22f,0.22f,-0.98f), new Vector3(0.22f,0.22f,-0.98f) };
        foreach (var lp in slegPos)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leg.transform.SetParent(parent.transform);
            leg.transform.localScale = new Vector3(0.05f, 0.44f, 0.05f);
            leg.transform.localPosition = lp;
            leg.GetComponent<Renderer>().sharedMaterial = darkMetal;
        }

        // --- GHE CONG AN ---
        GameObject seat2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        seat2.name = "OfficerChair_Seat";
        seat2.transform.SetParent(parent.transform);
        seat2.transform.localScale = new Vector3(0.55f, 0.07f, 0.55f);
        seat2.transform.localPosition = new Vector3(0, 0.46f, 1.0f);
        seat2.GetComponent<Renderer>().sharedMaterial = darkMetal;

        GameObject seatBack2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        seatBack2.name = "OfficerChair_Back";
        seatBack2.transform.SetParent(parent.transform);
        seatBack2.transform.localScale = new Vector3(0.55f, 0.8f, 0.06f);
        seatBack2.transform.localPosition = new Vector3(0, 0.86f, 1.28f);
        seatBack2.GetComponent<Renderer>().sharedMaterial = darkMetal;

        // Chan ghe cong an
        Vector3[] olegPos = { new Vector3(-0.24f,0.23f,0.74f), new Vector3(0.24f,0.23f,0.74f), new Vector3(-0.24f,0.23f,1.26f), new Vector3(0.24f,0.23f,1.26f) };
        foreach (var lp in olegPos)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leg.transform.SetParent(parent.transform);
            leg.transform.localScale = new Vector3(0.05f, 0.46f, 0.05f);
            leg.transform.localPosition = lp;
            leg.GetComponent<Renderer>().sharedMaterial = darkMetal;
        }

        // --- TU HO SO ---
        GameObject cabinet = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cabinet.name = "FileCabinet";
        cabinet.transform.SetParent(parent.transform);
        cabinet.transform.localScale = new Vector3(0.6f, 1.8f, 0.4f);
        cabinet.transform.localPosition = new Vector3(-3.2f, 0.9f, -3.2f);
        cabinet.GetComponent<Renderer>().sharedMaterial = cabinetMat;

        // --- MAY TINH DE BAN (box don gian) ---
        GameObject monitor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        monitor.name = "Monitor";
        monitor.transform.SetParent(parent.transform);
        monitor.transform.localScale = new Vector3(0.5f, 0.35f, 0.05f);
        monitor.transform.localPosition = new Vector3(0.4f, 0.97f, 0.55f);
        monitor.transform.rotation = Quaternion.Euler(0, 180f, 0);
        Material monitorMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        monitorMat.color = new Color(0.05f, 0.05f, 0.05f);
        monitor.GetComponent<Renderer>().sharedMaterial = monitorMat;

        Debug.Log("[PoliceOffice] Primitive furniture created.");
    }
}