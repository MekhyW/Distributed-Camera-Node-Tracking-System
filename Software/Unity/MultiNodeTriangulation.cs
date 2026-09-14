using UnityEngine;
using System.Collections.Generic;

public class MultiNodeTriangulation : MonoBehaviour
{
    [Header("Triangulation Settings")]
    public NodeMode requiredMode = NodeMode.Tracking;
    public float maxRange = 500f;
    public float fieldOfViewCone = 30f;

    [Header("Visuals")]
    public float dotScale = 1.0f;
    public Material dotMaterial;

    private GameObject _triangulationDot;
    private Renderer _dotRenderer;
    private NodeData[] _allNodes;

    void Start()
    {
        _triangulationDot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _triangulationDot.name = "TriangulationPoint";
        Destroy(_triangulationDot.GetComponent<Collider>());
        _dotRenderer = _triangulationDot.GetComponent<Renderer>();
        if (dotMaterial == null) // Default Material
        {
            var shader = Shader.Find("Sprites/Default");
            dotMaterial = new Material(shader);
            dotMaterial.color = Color.cyan; 
        }
        _dotRenderer.material = dotMaterial;
        _triangulationDot.SetActive(false);
    }

    void Update()
    {
        _allNodes = FindObjectsOfType<NodeData>();
        
        List<NodeData> activeNodes = new List<NodeData>();
        foreach (var node in _allNodes)
        {
            if (node.currentMode == requiredMode || node.currentMode == NodeMode.Searching) activeNodes.Add(node);
            else ResetVisualizer(node);
        }
        if (activeNodes.Count < 2)
        {
            ClearAllVisuals(activeNodes);
            return;
        }

        Vector3 estimatedPoint;
        bool success = CalculateLinearLeastSquares(activeNodes, out estimatedPoint); 
        if (!success) { 
            ClearAllVisuals(activeNodes); 
            return; 
        }

        if (ValidatePoint(estimatedPoint, activeNodes)) // Cone check
        {
            _triangulationDot.SetActive(true);
            _triangulationDot.transform.position = estimatedPoint;
            _triangulationDot.transform.localScale = Vector3.one * dotScale;
            // --- CALCULATE PROJECTION FOR EACH CAMERA ---
            foreach (var node in activeNodes)
            {
                var viz = node.GetComponent<CameraVectorVisualizer>();
                if (viz != null)
                {
                    Vector3 origin = node.transform.position;
                    Vector3 dir = node.transform.forward;
                    Vector3 toTarget = estimatedPoint - origin;
                    float dist = Vector3.Dot(toTarget, dir);
                    if (dist < 0) dist = 0; //don't draw backwards
                    Vector3 pointOnLine = origin + dir * dist;
                    viz.SetTargetPoint(pointOnLine);
                }
            }
        }
        else ClearAllVisuals(activeNodes);
    }

    private void ClearAllVisuals(List<NodeData> nodes)
    {
        _triangulationDot.SetActive(false);
        foreach (var node in nodes) ResetVisualizer(node);
    }

    private void ResetVisualizer(NodeData node)
    {
        var viz = node.GetComponent<CameraVectorVisualizer>();
        if (viz != null) viz.SetTargetPoint(null); // Null = Infinite/Ground
    }

    private bool CalculateLinearLeastSquares(List<NodeData> nodes, out Vector3 result)
    {
        result = Vector3.zero;
        Matrix4x4 matSum = Matrix4x4.zero;
        Vector3 vecSum = Vector3.zero;
        foreach (var node in nodes)
        {
            Vector3 o = node.transform.position;
            Vector3 d = node.transform.forward; 
            float xx = 1 - d.x * d.x;
            float xy =   - d.x * d.y;
            float xz =   - d.x * d.z;
            float yy = 1 - d.y * d.y;
            float yz =   - d.y * d.z;
            float zz = 1 - d.z * d.z;
            Matrix4x4 m = Matrix4x4.identity;
            m.m00 = xx; m.m01 = xy; m.m02 = xz; m.m03 = 0;
            m.m10 = xy; m.m11 = yy; m.m12 = yz; m.m13 = 0;
            m.m20 = xz; m.m21 = yz; m.m22 = zz; m.m23 = 0;
            m.m33 = 1; 
            for(int i=0; i<3; i++)
                for(int j=0; j<3; j++)
                    matSum[i,j] += m[i,j];
            vecSum += m.MultiplyVector(o);
        }
        matSum.m33 = 1.0f;
        if (Mathf.Abs(matSum.determinant) < 0.0001f) return false;
        result = matSum.inverse.MultiplyVector(vecSum);
        return true;
    }

    private bool ValidatePoint(Vector3 point, List<NodeData> nodes)
    {
        if (nodes.Count > 0 && Vector3.Distance(nodes[0].transform.position, point) > maxRange) return false;
        foreach (var node in nodes)
        {
            Vector3 toPoint = point - node.transform.position;
            if (Vector3.Dot(node.transform.forward, toPoint) <= 0) return false;
            float angle = Vector3.Angle(node.transform.forward, toPoint);
            if (angle > fieldOfViewCone) return false;
        }
        return true;
    }
}