using UnityEngine;

public class TestCript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ComputeAndLog();
    }

    // Update is called once per frame
    void Update()
    {
        // press Space to recompute and log values at runtime
        if (Input.GetKeyDown(KeyCode.Space))
            ComputeAndLog();
    }

    [Header("Demo Vectors")]
    public Vector3 vectorA = new Vector3(1f, 1f, 0f); // vector to be projected / reflected
    public Vector3 vectorB = new Vector3(1f, 0f, 0f); // axis / projection target (not necessarily normalized)
    public Vector3 normal = new Vector3(0f, 1f, 0f);  // normal for reflection (will be normalized)

    [Header("Visualization")]
    public bool drawGizmos = true;
    public float gizmoScale = 1f;

    private void ComputeAndLog()
    {
        // Using Unity built-in methods
        Vector3 projBuiltIn = Vector3.Project(vectorA, vectorB);
        Vector3 reflBuiltIn = Vector3.Reflect(vectorA, normal.normalized);

        // Using manual implementations (same math)
        Vector3 projManual = VectorUtils.Project(vectorA, vectorB);
        Vector3 reflManual = VectorUtils.Reflect(vectorA, normal);

        Debug.Log($"Project (built-in): {projBuiltIn}");
        Debug.Log($"Project (manual)  : {projManual}");
        Debug.Log($"Reflect (built-in): {reflBuiltIn}");
        Debug.Log($"Reflect (manual)  : {reflManual}");
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        Vector3 origin = transform.position;

        // Original vector (white)
        Gizmos.color = Color.white;
        Gizmos.DrawLine(origin, origin + vectorA * gizmoScale);
        Gizmos.DrawSphere(origin + vectorA * gizmoScale, 0.02f * gizmoScale);

        // Projection onto vectorB (green)
        Vector3 proj = VectorUtils.Project(vectorA, vectorB);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(origin, origin + proj * gizmoScale);
        Gizmos.DrawSphere(origin + proj * gizmoScale, 0.02f * gizmoScale);

        // Reflection across normal (cyan)
        Vector3 refl = VectorUtils.Reflect(vectorA, normal);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(origin, origin + refl * gizmoScale);
        Gizmos.DrawSphere(origin + refl * gizmoScale, 0.02f * gizmoScale);

        // Draw the projection axis (vectorB) and normal direction for clarity
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, origin + vectorB.normalized * gizmoScale);
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(origin, origin + normal.normalized * gizmoScale);
    }
}

/// <summary>
/// Utility implementations for vector projection and reflection.
/// These reproduce the behavior of Vector3.Project and Vector3.Reflect.
/// </summary>
public static class VectorUtils
{
    /// <summary>
    /// Projects vector 'a' onto vector 'b'. Returns zero if 'b' is zero.
    /// Formula: (dot(a,b) / dot(b,b)) * b
    /// </summary>
    public static Vector3 Project(Vector3 a, Vector3 b)
    {
        float denom = Vector3.Dot(b, b);
        if (denom <= Mathf.Epsilon) return Vector3.zero;
        return (Vector3.Dot(a, b) / denom) * b;
    }

    /// <summary>
    /// Reflects vector 'v' around the supplied normal 'n'. 'n' is normalized internally.
    /// Formula: v - 2 * dot(v, n) * n
    /// </summary>
    public static Vector3 Reflect(Vector3 v, Vector3 n)
    {
        Vector3 nn = n.normalized;
        return v - 2f * Vector3.Dot(v, nn) * nn;
    }
}
