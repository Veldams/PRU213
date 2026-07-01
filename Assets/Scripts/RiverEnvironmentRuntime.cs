using UnityEngine;

public static class RiverEnvironmentRuntime
{
    // Runtime river setup removed for performance on SampleScene.
}
[RequireComponent(typeof(MeshFilter))]
public class RiverWaveAnimator : MonoBehaviour
{
    [SerializeField] private float waveAmplitude = 0.08f;
    [SerializeField] private float waveFrequency = 1.4f;
    [SerializeField] private float waveSpeed = 1.2f;
    [SerializeField] private Vector2 uvPanSpeed = new Vector2(0.03f, 0.015f);

    private MeshFilter meshFilter;
    private Mesh runtimeMesh;
    private Vector3[] baseVertices;
    private Vector3[] animatedVertices;
    private Material runtimeMaterial;

    private void Awake()
    {
        enabled = false;
    }
}
