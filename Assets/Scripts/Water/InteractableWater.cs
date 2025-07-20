using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(EdgeCollider2D))]
[RequireComponent(typeof(WaterTriggerHandler))]
public class InteractableWater : MonoBehaviour
{
    [Header("Springs")]
    [SerializeField] private float spriteConstant = 1.4f;
    [SerializeField] private float damping = 1.1f;
    [SerializeField] private float spread = 6.5f;
    [SerializeField, Range(1, 10)] private int wavePropogationIterations = 0;
    [SerializeField, Range(0f, 20f)] private float speedMult = 5.5f;

    [Header("Force")]
    public float ForceMultiplier = 0.2f;
    [Range(1f, 50f)] public float MaxForce = 5f;

    [Header("Collision")]
    [SerializeField, Range(1f, 10f)] private float playerCollisionRadiusMult = 4.15f;

    [Header("Mesh Generation")]
    [Range(2, 500)] 
    public int NumOfXVertices = 70;

    public float width = 10f;
    public float height = 4f;
    public Material waterMaterial;
    private const int NUM_OF_Y_VERTICES = 2;

    [Header("Gizmo")]
    public Color gizmoColor = Color.white;

    private Mesh mesh;
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Vector3[] vertices;
    private int[] topVerticesIndex;

    private EdgeCollider2D coll;

    private class WaterPoint
    {
        public float velocity, pos, targetHeight;
    }
    private List<WaterPoint> waterPoints = new List<WaterPoint>();

    private void Start()
    {
        GenerateMesh();

        GenerateMesh();
        CreateWaterPoints();
    }

    private void Reset()
    {
        coll = GetComponent<EdgeCollider2D>();
        coll.isTrigger = true;
    }

    private void FixedUpdate()
    {
        // Update all spring positions
        for (int i = 1; i < waterPoints.Count - 1; i++)
        {
            WaterPoint point = waterPoints[i];

            float x = point.pos - point.targetHeight;
            float acceleration = -spriteConstant * x - damping * point.velocity;
            point.pos += point.velocity * speedMult * Time.fixedDeltaTime;
            vertices[topVerticesIndex[i]].y = point.pos;
            point.velocity += acceleration * speedMult * Time.fixedDeltaTime;
        }

        // Handle wave propogation
        for (int j = 0; j < wavePropogationIterations; j++)
        {
            for (int i = 1; i < waterPoints.Count - 1; i++)
            {
                float leftDelta = spread * (waterPoints[i].pos - waterPoints[i - 1].pos) * speedMult * Time.fixedDeltaTime;
                waterPoints[i - 1].velocity += leftDelta;

                float rightDelta = spread * (waterPoints[i].pos - waterPoints[i + 1].pos) * speedMult * Time.fixedDeltaTime;
                waterPoints[i + 1].velocity += rightDelta;
            }
        }

        // update mesh
        mesh.vertices = vertices;
    }

    public void Splash(Collider2D collision, float force)
    {
        float radius = collision.bounds.extents.x * playerCollisionRadiusMult;
        Vector2 center = collision.transform.position;

        for (int i = 0; i < waterPoints.Count; i++)
        {
            Vector2 vertexWorldPos = transform.TransformPoint(vertices[topVerticesIndex[i]]);

            if (IsPointInsideCircle(vertexWorldPos, center, radius))
            {
                waterPoints[i].velocity = force;
            }
        }
    }

    private bool IsPointInsideCircle(Vector2 point, Vector2 center, float radius)
    {
        float distanceSquared = (point - center).sqrMagnitude;
        return distanceSquared <= radius * radius;
    }

    public void ResetEdgeCollider()
    {
        if (coll == null)
        {
            coll = GetComponent<EdgeCollider2D>();
        }

        if (vertices == null || topVerticesIndex == null || topVerticesIndex.Length < 2)
        {
            Debug.LogWarning("Cannot reset edge collider because vertices/topVertices are not initialized.");
            return;
        }

        Vector2[] newPoints = new Vector2[2];

        Vector2 firstPoint = new Vector2(vertices[topVerticesIndex[0]].x, vertices[topVerticesIndex[0]].y);
        Vector2 secondPoint = new Vector2(vertices[topVerticesIndex[topVerticesIndex.Length - 1]].x,
                                          vertices[topVerticesIndex[topVerticesIndex.Length - 1]].y);

        newPoints[0] = firstPoint;
        newPoints[1] = secondPoint;

        coll.offset = Vector2.zero;
        coll.points = newPoints;
    }


    public void GenerateMesh()
    {
        mesh = new Mesh();

        // add vertices
        vertices = new Vector3[NUM_OF_Y_VERTICES * NumOfXVertices];
        topVerticesIndex = new int[NumOfXVertices];
        for (int y = 0; y < NUM_OF_Y_VERTICES; y++)
        {
            for (int x = 0; x < NumOfXVertices; x++)
            {
                float xPos = (x / (float)(NumOfXVertices - 1)) * width - width / 2;
                float yPos = (y / (float)(NUM_OF_Y_VERTICES - 1)) * height - height / 2;
                vertices[y * NumOfXVertices + x] = new Vector3(xPos, yPos, 0f);

                if (y == NUM_OF_Y_VERTICES - 1)
                {
                    topVerticesIndex[x] = y * NumOfXVertices + x;
                }
            }
        }

        // construct triangles
        int[] triangles = new int[(NumOfXVertices - 1) * (NUM_OF_Y_VERTICES - 1) * 6];
        int index = 0;

        for (int y = 0; y < NUM_OF_Y_VERTICES - 1; y++)
        {
            for (int x = 0; x < NumOfXVertices - 1; x++)
            {
                int bottomLeft = y * NumOfXVertices + x;
                int bottomRight = bottomLeft + 1;
                int topLeft = bottomLeft + NumOfXVertices;
                int topRight = topLeft + 1;

                // First triangle
                triangles[index++] = bottomLeft;
                triangles[index++] = topLeft;
                triangles[index++] = bottomRight;

                // second triangle
                triangles[index++] = bottomRight;
                triangles[index++] = topLeft;
                triangles[index++] = topRight;
            }
        }

        // UVs
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            uvs[i] = new Vector2((vertices[i].x + width / 2) / width, (vertices[i].y + height / 2) / height);
        }

        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }
        if (meshFilter == null)
        {
            meshFilter = GetComponent<MeshFilter>();
        }

        meshRenderer.material = waterMaterial;

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
    }

    private void CreateWaterPoints()
    {
        waterPoints.Clear();

        for (int i = 0; i < topVerticesIndex.Length; i++)
        {
            waterPoints.Add(new WaterPoint
            {
                pos = vertices[topVerticesIndex[i]].y,
                targetHeight = vertices[topVerticesIndex[i]].y,
            });
        }
    }
}

[CustomEditor(typeof(InteractableWater))]
public class InteractableWaterEditor : Editor
{
    private InteractableWater water;

    private void OnEnable()
    {
        water = (InteractableWater)target;
    }

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new VisualElement();

        InspectorElement.FillDefaultInspector(root, serializedObject, this);

        root.Add(new VisualElement { style = { height = 10 } });

        Button generateMeshButton = new Button(() => water.GenerateMesh())
        {
            text = "Generate Mesh"
        };
        root.Add(generateMeshButton);

        Button placeEdgeColliderButton = new Button(() => water.ResetEdgeCollider())
        {
            text = "Place Edge Collider"
        };
        root.Add(placeEdgeColliderButton);

        return root;
    }

    private  void ChangeDimensions(ref float width, ref float height, float calculatedWidthMax, float calculatedHeightMax)
    {
        width = Mathf.Max(0.1f, calculatedWidthMax);
        height = Mathf.Max(0.1f, calculatedHeightMax);


    }

    private void OnSceneGUI()
    {
        // Draw the wireframe box
        Handles.color = water.gizmoColor;
        Vector3 center = water.transform.position;
        Vector3 size = new Vector3(water.width, water.height, 0.1f);
        Handles.DrawWireCube(center, size);

        // Handles for width and height
        float handleSize = HandleUtility.GetHandleSize(center) + 0.1f;
        Vector3 snap = Vector3.one * 0.1f;

        // Corner Handles
        Vector3[] corners = new Vector3[4];
        corners[0] = center + new Vector3(-water.width / 2, -water.height / 2, 0);  // Bottom-left
        corners[1] = center + new Vector3(water.width / 2, -water.height / 2, 0);  // Bottom-right
        corners[2] = center + new Vector3(-water.width / 2, water.height / 2, 0);  // Top-left
        corners[3] = center + new Vector3(water.width / 2, water.height / 2, 0);  // Top-right

        // Handle for each corner
        EditorGUI.BeginChangeCheck();
        Vector3 newBottomLeft = Handles.FreeMoveHandle(corners[0], handleSize, snap, Handles.CubeHandleCap);
        if (EditorGUI.EndChangeCheck())
        {
            ChangeDimensions(ref water.width, ref water.height, corners[1].x - newBottomLeft.x, corners[3].y - newBottomLeft.y);
            water.transform.position += new Vector3((newBottomLeft.x - corners[0].x) / 2, (newBottomLeft.y - corners[0].y) / 2, 0);
        }

        EditorGUI.BeginChangeCheck();
        Vector3 newBottomRight = Handles.FreeMoveHandle(corners[1], handleSize, snap, Handles.CubeHandleCap);
        if (EditorGUI.EndChangeCheck())
        {
            ChangeDimensions(ref water.width, ref water.height, newBottomRight.x - corners[0].x, corners[3].y - newBottomRight.y);
            water.transform.position += new Vector3((newBottomRight.x - corners[1].x) / 2, (newBottomRight.y - corners[1].y) / 2, 0);
        }

        EditorGUI.BeginChangeCheck();
        Vector3 newTopLeft = Handles.FreeMoveHandle(corners[2], handleSize, snap, Handles.CubeHandleCap);
        if (EditorGUI.EndChangeCheck())
        {
            ChangeDimensions(ref water.width, ref water.height, corners[3].x - newTopLeft.x, newTopLeft.y - corners[0].y);
            water.transform.position += new Vector3((newTopLeft.x - corners[2].x) / 2, (newTopLeft.y - corners[2].y) / 2, 0);
        }

        EditorGUI.BeginChangeCheck();
        Vector3 newTopRight = Handles.FreeMoveHandle(corners[3], handleSize, snap, Handles.CubeHandleCap);
        if (EditorGUI.EndChangeCheck())
        {
            ChangeDimensions(ref water.width, ref water.height, newTopRight.x - corners[2].x, newTopRight.y - corners[1].y);
            water.transform.position += new Vector3((newTopRight.x - corners[3].x) / 2, (newTopRight.y - corners[3].y) / 2, 0);
        }

        // Update the mesh if handles are moved
        if (GUI.changed)
        {
            water.GenerateMesh();
        }
    }
}