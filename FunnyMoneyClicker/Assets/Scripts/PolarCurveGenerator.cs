using UnityEngine;

public class PolarCurveGenerator : MonoBehaviour
{
    public float a = 1f; // Parameter 'a' in the equation
    public float b = 2f; // Parameter 'b' in the equation
    public int numberOfPoints = 100; // Number of points to generate for the curve
    public float maxTheta = 2 * Mathf.PI; // Maximum angle for the curve (e.g., 2*PI for a full circle)

    private LineRenderer lineRenderer;

    void Update()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        GeneratePolarCurve();
    }

    void GeneratePolarCurve()
    {
        lineRenderer.positionCount = numberOfPoints;
        Vector3[] points = new Vector3[numberOfPoints];

        for (int i = 0; i < numberOfPoints; i++)
        {
            float theta = i * (maxTheta / (numberOfPoints - 1)); // Calculate theta for each point
            float r = Mathf.Sin((a / b) * theta); // Calculate 'r' using the given equation

            // Convert polar coordinates (r, theta) to Cartesian coordinates (x, y)
            float x = r * Mathf.Cos(theta);
            float y = r * Mathf.Sin(theta);

            points[i] = new Vector3(x, y, 0); // Create a Vector3 (z-coordinate is 0 for 2D)
        }

        lineRenderer.SetPositions(points);
    }

    // You can call this method to update the curve if 'a' or 'b' change at runtime
    public void UpdateCurve()
    {
        GeneratePolarCurve();
    }
}

