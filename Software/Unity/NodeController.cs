using UnityEngine;

public class NodeController : MonoBehaviour
{
    [Header("Status Dot Settings")]
    public float dotHeight = 1.5f; // How high above the node the dot sits
    public float dotScale = 0.3f;  // Size of the dot

    private GameObject statusDot;
    private Renderer dotRenderer;
    private NodeData nodeData;
    private Camera mainCam;

    void Start()
    {
        nodeData = GetComponent<NodeData>();
        mainCam = Camera.main;
        CreateStatusDot();
    }

    private void CreateStatusDot()
    {
        statusDot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        statusDot.name = "StatusDot";
        Destroy(statusDot.GetComponent<Collider>());  // Remove collider so it doesn't block mouse raycasts for the tooltip
        statusDot.transform.SetParent(transform);
        statusDot.transform.localPosition = Vector3.up * dotHeight;
        statusDot.transform.localScale = Vector3.one * dotScale;
        dotRenderer = statusDot.GetComponent<Renderer>();
        dotRenderer.material = new Material(Shader.Find("Sprites/Default")); // Use the Standard shader or Unlit if available. 
    }

    void Update()
    {
        UpdateDotColor();
        BillboardDot();
    }

    private void UpdateDotColor()
    {
        if (nodeData == null || dotRenderer == null) return;
        Color targetColor = Color.gray;
        switch (nodeData.currentMode)
        {
            case NodeMode.Disabled:
                targetColor = Color.red;
                break;
            case NodeMode.Searching:
                targetColor = new Color(1f, 0.5f, 0f); // Orange
                break;
            case NodeMode.Tracking:
                targetColor = Color.green;
                break;
        }
        dotRenderer.material.color = targetColor;
    }

    private void BillboardDot()
    {
        if (statusDot == null || mainCam == null) return;
        statusDot.transform.LookAt(statusDot.transform.position + mainCam.transform.rotation * Vector3.forward, mainCam.transform.rotation * Vector3.up); // Make the dot face the camera
    }

    public void Apply(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
    }
}