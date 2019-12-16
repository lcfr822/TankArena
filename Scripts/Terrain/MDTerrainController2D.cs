using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MDTerrainController2D : MonoBehaviour
{
    private GameObject detailObject;

    public float startHeight, endHeight, roughness, resolution;
    public SpriteRenderer backgroundRenderer;
    public Texture terrainTexture, detailTexture;

    private void Awake()
    {

        float[] heightMap = GenerateHeightmap(startHeight, endHeight, Mathf.CeilToInt(backgroundRenderer.bounds.size.x) * Mathf.CeilToInt(resolution));
        Vector2[] terrainVertices = GenerateFillVertices(heightMap, resolution);
        Vector2[] terrainUVs = GenerateFillUVs(heightMap);
        int[] terrainTriangles = Triangulate(terrainVertices.Length);
        GenerateTerrainMesh(terrainVertices, terrainTriangles, terrainUVs, "Generated Terrain", terrainTexture, 0);

        Vector2[] grassVertices = GenerateDetailVertices(heightMap, resolution);
        Vector2[] grassUVs = GenerateDetailUVs(heightMap);
        int[] grassTriangles = Triangulate(terrainVertices.Length);
        CalculateCollider(GenerateTerrainMesh(grassVertices, grassTriangles, grassUVs, "Generated Terrain Detail", detailTexture, -2, out detailObject));
    }

    /// <summary>
    /// Generate a biplanar mesh facing the camera.
    /// </summary>
    /// <param name="vertices">Mesh vertices.</param>
    /// <param name="triangles">Mesh triangles.</param>
    /// <param name="uvs">Mesh UVs.</param>
    /// <param name="name">Name of the mesh.</param>
    /// <param name="texture">Texture of the mesh.</param>
    /// <param name="z">Z position value of the mesh.</param>
    /// <returns>Final terrain layer mesh.</returns>
    private Mesh GenerateTerrainMesh(Vector2[] vertices, int[] triangles, Vector2[] uvs, string name, Texture texture, int z)
    {
        List<Vector3> meshVertices = new List<Vector3>();

        foreach(Vector2 vertex in vertices)
        {
            meshVertices.Add(new Vector3(vertex.x, vertex.y, z));
        }

        Mesh mesh = new Mesh();
        mesh.vertices = meshVertices.ToArray();
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GameObject terrainObject = new GameObject(name);
        terrainObject.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = terrainObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        terrainObject.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", texture);
        terrainObject.transform.parent = transform;
        terrainObject.transform.localPosition = transform.InverseTransformPoint(transform.position);
        terrainObject.tag = "Ground";

        return mesh;
    }
    
    /// <summary>
    /// Generate a biplanar mesh facing the camera and assign it to a GameOject.
    /// </summary>
    /// <param name="vertices">Mesh vertices.</param>
    /// <param name="triangles">Mesh triangles.</param>
    /// <param name="uvs">Mesh UVs.</param>
    /// <param name="name">Name of the mesh.</param>
    /// <param name="texture">Texture of the mesh.</param>
    /// <param name="z">Z position value of the mesh.</param>
    /// <param name="meshObject">GameObject to render the mesh.</param>
    /// <returns>Final terrain layer mesh.</returns>
    private Mesh GenerateTerrainMesh(Vector2[] vertices, int[] triangles, Vector2[] uvs, string name, Texture texture, int z, out GameObject meshObject)
    {
        List<Vector3> meshVertices = new List<Vector3>();

        foreach(Vector2 vertex in vertices)
        {
            meshVertices.Add(new Vector3(vertex.x, vertex.y, z));
        }

        Mesh mesh = new Mesh();
        mesh.vertices = meshVertices.ToArray();
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GameObject terrainObject = new GameObject(name);
        terrainObject.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = terrainObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        terrainObject.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", texture);
        terrainObject.transform.parent = transform;
        terrainObject.transform.localPosition = transform.InverseTransformPoint(transform.position);
        terrainObject.tag = "Ground";

        meshObject = terrainObject;
        //meshObject.AddComponent(typeof(PolygonCollider2D));
        mesh.name = meshObject.name;

        return mesh;
    }

    /// <summary>
    /// Calculate height data for a mesh.
    /// </summary>
    /// <param name="startHeight">Height at the leftmost point of the mesh.</param>
    /// <param name="endHeight">Height at the rightmost point of the mesh.</param>
    /// <param name="length">Number of heights to generate for the mesh.</param>
    /// <returns>An array of height values with a count equal to <paramref name="length"/></returns>
    private float[] GenerateHeightmap(float startHeight, float endHeight, int length)
    {
        float[] heightmap = new float[length + 1];
        heightmap[0] = startHeight;
        heightmap[heightmap.Length - 1] = endHeight;

        GenerateMidpoint(0, heightmap.Length - 1, roughness, heightmap);
        return heightmap;
    }

    /// <summary>
    /// Generate midpoints recursively along a heightmap.
    /// </summary>
    /// <param name="start">Lowest value point.</param>
    /// <param name="end">Highest value point.</param>
    /// <param name="roughness">Modifies jaggedness of terrain mesh.</param>
    /// <param name="heightmap">Height data of the mesh</param>
    private void GenerateMidpoint(int start, int end, float roughness, float[] heightmap)
    {
        int midpoint = Mathf.FloorToInt((start + end) / 2);

        if(midpoint != start)
        {
            float midHeight = (heightmap[start] + heightmap[end]) / 2;
            heightmap[midpoint] = midHeight + Random.Range(-roughness, roughness);

            GenerateMidpoint(start, midpoint, roughness / 2, heightmap);
            GenerateMidpoint(midpoint, end, roughness / 2, heightmap);
        }
    }

    /// <summary>
    /// Generate vertices for the fill mesh.
    /// </summary>
    /// <param name="heightmap">Height data of the fill mesh.</param>
    /// <param name="resolution">Quality of the fill mesh. (Higher = bettera)</param>
    /// <returns></returns>
    private Vector2[] GenerateFillVertices(float[] heightmap, float resolution)
    {
        resolution = Mathf.Max(1, resolution);

        List<Vector2> vertices = new List<Vector2>();
        for(int i = 0; i < heightmap.Length; i++)
        {
            vertices.Add(new Vector2(i / resolution, heightmap[i]));
            vertices.Add(new Vector2(i / resolution, 0));
        }

        return vertices.ToArray();
    }    

    /// <summary>
    /// Generate vertices for the detail layer.
    /// </summary>
    /// <param name="heightmap">Height data of the detail mesh.</param>
    /// <param name="resolution">Quality of the detail mesh. (Higher = better)</param>
    /// <returns></returns>
    private Vector2[] GenerateDetailVertices(float[] heightmap, float resolution)
    {
        resolution = Mathf.Max(1, resolution);

        List<Vector2> vertices = new List<Vector2>();

        for(int i = 0; i < heightmap.Length; i++)
        {
            vertices.Add(new Vector2(i / resolution, heightmap[i]));
            vertices.Add(new Vector2(i / resolution, heightmap[i] - 1));
        }

        return vertices.ToArray();
    }

    /// <summary>
    /// Generates lighting data for the fill mesh.
    /// </summary>
    /// <param name="heightmap">Height data of the fill mesh.</param>
    /// <returns>UV coordinates for a biplanar mesh.</returns>
    private Vector2[] GenerateFillUVs(float[] heightmap)
    {
        List<Vector2> uvs = new List<Vector2>();
        float factor = 1.0f / heightmap.Length;

        for (int i = 0; i < heightmap.Length; i++)
        {
            uvs.Add(new Vector2(factor * i * 20, heightmap[i] / 20));
            uvs.Add(new Vector2(factor * i * 20, 0));
        }

        return uvs.ToArray();
    }

    /// <summary>
    /// Generates lighting data for the detail mesh.
    /// </summary>
    /// <param name="heightmap">Height data of the mesh.</param>
    /// <returns>UV coordinates for a biplanar mesh.</returns>
    private Vector2[] GenerateDetailUVs(float[] heightmap)
    {
        List<Vector2> uvs = new List<Vector2>();
        float factor = 1.0f / heightmap.Length;

        for(int i = 0; i < heightmap.Length; i++)
        {
            uvs.Add(new Vector2(i % 2, 1));
            uvs.Add(new Vector2(i % 2, 0));
        }

        return uvs.ToArray();
    }

    /// <summary>
    /// Duplicates the border of a biplanar mesh into a PolygonCollider2D path.
    /// </summary>
    /// <param name="mesh">Mesh path to duplicate.</param>
    private void CalculateCollider(Mesh mesh)
    {
        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;

        // Get just the outer edges from the mesh's triangles (ignore or remove any shared edges)
        Dictionary<string, KeyValuePair<int, int>> edges = new Dictionary<string, KeyValuePair<int, int>>();
        for (int i = 0; i < triangles.Length; i += 3)
        {
            for (int e = 0; e < 3; e++)
            {
                int vert1 = triangles[i + e];
                int vert2 = triangles[i + e + 1 > i + 2 ? i : i + e + 1];
                string edge = Mathf.Min(vert1, vert2) + ":" + Mathf.Max(vert1, vert2);
                if (edges.ContainsKey(edge))
                {
                    edges.Remove(edge);
                }
                else
                {
                    edges.Add(edge, new KeyValuePair<int, int>(vert1, vert2));
                }
            }
        }

        // Create edge lookup (Key is first vertex, Value is second vertex, of each edge)
        Dictionary<int, int> lookup = new Dictionary<int, int>();
        foreach (KeyValuePair<int, int> edge in edges.Values)
        {
            if (lookup.ContainsKey(edge.Key) == false)
            {
                lookup.Add(edge.Key, edge.Value);
            }
        }

        // Create empty polygon collider
        PolygonCollider2D polygonCollider = detailObject.AddComponent(typeof(PolygonCollider2D)) as PolygonCollider2D;
        polygonCollider.pathCount = 0;

        // Loop through edge vertices in order
        int startVert = 0;
        int nextVert = startVert;
        int highestVert = startVert;
        List<Vector2> colliderPath = new List<Vector2>();
        while (true)
        {

            // Add vertex to collider path
            colliderPath.Add(vertices[nextVert]);

            // Get next vertex
            nextVert = lookup[nextVert];

            // Store highest vertex (to know what shape to move to next)
            if (nextVert > highestVert)
            {
                highestVert = nextVert;
            }

            // Shape complete
            if (nextVert == startVert)
            {

                // Add path to polygon collider
                polygonCollider.pathCount++;
                polygonCollider.SetPath(polygonCollider.pathCount - 1, colliderPath.ToArray());
                colliderPath.Clear();

                // Go to next shape if one exists
                if (lookup.ContainsKey(highestVert + 1))
                {
                    startVert = highestVert + 1;
                    nextVert = startVert;

                    continue;
                }

                for(int i = 0; i < polygonCollider.pathCount; i++)
                {
                    detailObject.GetComponent<PolygonCollider2D>().SetPath(i, polygonCollider.GetPath(i));
                }

                detailObject.AddComponent<Rigidbody2D>().isKinematic = true;
                Rigidbody2D rb2d = detailObject.GetComponent<Rigidbody2D>();
                rb2d.useFullKinematicContacts = true;
                rb2d.mass = 99999;
                rb2d.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                rb2d.sleepMode = RigidbodySleepMode2D.NeverSleep;

                break;
            }
        }
    }

    /// <summary>
    /// Calculate the triangles that form a mesh.
    /// </summary>
    /// <param name="count">Number of indeces to process.</param>
    /// <returns>Int[] containing calculated triangles.</returns>
    private int[] Triangulate(int count)
    {
        List<int> indices = new List<int>();

        for(int i = 0; i <= count - 4; i += 2)
        {
            indices.Add(i);
            indices.Add(i + 3);
            indices.Add(i + 1);

            indices.Add(i + 3);
            indices.Add(i);
            indices.Add(i + 2);
        }

        return indices.ToArray();
    }
}
