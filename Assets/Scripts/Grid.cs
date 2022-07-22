using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
public class Grid : MonoBehaviour
{
    public GameObject[] pineTreePrefabs;
    public GameObject[] roundTreePrefabs;
    public GameObject[] rockPrefabs;
    public GameObject[] grassPrefabs;
    public GameObject[] mushroomPrefabs;
    public GameObject[] flowerPrefabs;
    public GameObject[] forestLitterPrefabs;


    public Material terrainMaterial;
    public Material edgeMaterial;
    public float waterLevel = 4f;
    public float mountainLevel = 7f;
    public float sandLevel = 4f;
    public float grassLevel = 5f;
    public float scale = .1f;
    public float treeNoiseScale = .05f;
    public float treeDensity = .5f;
    public int size = 100;
    public List<string> w, g, s, m, v; // Five different terrain types which have unique relationships with each other

    public GameObject cube;
    public float heightScaling = 1.5f;
    public float noiseMapScaling = 11;

    public Material GrassMat;
    public Material SandMat;
    public Material MountainMat;

    Cell[,] grid;

    float[,] noiseMap;
    GameObject[,] cubeMap;

    void Start()
    {
        noiseMap = new float[size, size];
        cubeMap = new GameObject[size, size];


        //noise map populates an arbitrary map with perlin noise values
        //Used as a context for mesh placement in the grid
        // float[,] noiseMap = new float[size, size];
        (float xOffset, float yOffset) = (Random.Range(-10000f, 10000f), Random.Range(-10000f, 10000f));
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float noiseValue = Mathf.PerlinNoise(x * scale + xOffset, y * scale + yOffset);
                noiseMap[x, y] = noiseValue * noiseMapScaling;
            }
        }

        //fallout map populates an arbitrary map with random floats
        //Subtracted with noise map to generate islands
        float[,] falloffMap = new float[size, size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float xv = x / (float)size * 2 - 1;
                float yv = y / (float)size * 2 - 1;
                float v = Mathf.Max(Mathf.Abs(xv), Mathf.Abs(yv));
                falloffMap[x, y] = Mathf.Pow(v, 3f) / (Mathf.Pow(v, 3f) + Mathf.Pow(2.2f - 2.2f * v, 3f)) * noiseMapScaling;
            }
        }

        //Grid where the nature of the mesh is decided based upon the difference between the noisemap and falloff map
        grid = new Cell[size, size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float noiseValue = noiseMap[x, y];
                float u = Random.Range(-1f, 1.1f);
                if(u == 1.0) noiseValue -= falloffMap[x, y];
                string type;
                if (noiseValue <= waterLevel) type = "water";
                else if (noiseValue <= sandLevel) type = "sand";
                else if (noiseValue <= grassLevel) type = "grass";
                else type = "mountain";
                //bool isWater = noiseValue < waterLevel;
                Cell cell = new Cell(type);
                grid[x, y] = cell;
            }
        }
        
        //Hash map to maintain the adjacency lists for autotiling amongst different terrain elements
        Dictionary<string, List<string>> hash = new Dictionary<string, List<string>>();
        hash.Add("Water", w);
        hash.Add("Grass", g);
        hash.Add("Sand", s);
        hash.Add("Mountain", m);

        foreach (KeyValuePair<string, List<string>> pair in hash)
        {
            Debug.Log("KEY: " + pair.Key);
            Debug.Log("VALUE: ");
            foreach (string val in pair.Value) Debug.Log(val + " ");
            Debug.Log("\n");
        }

        DrawTerrainMesh(grid);
        DrawEdgeMesh(grid);
        DrawTexture(grid);
        bool[] tempArray = {false, false, true};
        GenerateObjects(grid, pineTreePrefabs, tempArray, treeDensity, 0.4f);

        tempArray[0] = false;
        tempArray[1] = true;
        tempArray[2] = false;
        GenerateObjects(grid, roundTreePrefabs, tempArray, treeDensity, 0.4f);

        tempArray[0] = true;
        tempArray[1] = true;
        tempArray[2] = true;
        GenerateObjects(grid, rockPrefabs, tempArray, 0.4f, 0.35f);

        tempArray[0] = true;
        tempArray[1] = true;
        tempArray[2] = true;
        // GenerateObjects(grid, forestLitterPrefabs, tempArray, 0.4f, 0.4f);

        tempArray[0] = false;
        tempArray[1] = true;
        tempArray[2] = false;
        GenerateObjects(grid, grassPrefabs, tempArray, 0.9f, 4f);

        tempArray[0] = false;
        tempArray[1] = true;
        tempArray[2] = true;
        GenerateObjects(grid, mushroomPrefabs, tempArray, 0.25f, 2f);

        tempArray[0] = true;
        tempArray[1] = true;
        tempArray[2] = true;
        GenerateObjects(grid, flowerPrefabs, tempArray, 0.4f, 2f);
    }





    void DrawTerrainMesh(Cell[,] grid)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Cell cell = grid[x, y];

                
                if (!cell.isWater)
                {
                    // making Terrain Cubes
                    cubeMap[x,y] = Instantiate(cube, new Vector3(x, 0, y), Quaternion.identity);
                    cubeMap[x,y].transform.localScale = new Vector3(1, heightScaling * noiseMap[x, y], 1);

                    // Changing Cube Color
                    Material TempMat =  null;
                    if (grid[x, y].isSand) TempMat = SandMat;
                    else if (grid[x, y].isGrass) TempMat = GrassMat;
                    else if (grid[x, y].isMountain) TempMat = MountainMat;
                    cubeMap[x, y].transform.GetChild(0).GetComponent<MeshRenderer>().material = TempMat;

                }
            }
        }

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
    }

    void DrawEdgeMesh(Cell[,] grid)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Cell cell = grid[x, y];
                if (!cell.isWater)
                {
                    if (x > 0)
                    {
                        Cell left = grid[x - 1, y];
                        if (left.isWater)
                        {
                            Vector3 a = new Vector3(x - .5f, 0, y + .5f);
                            Vector3 b = new Vector3(x - .5f, 0, y - .5f);
                            Vector3 c = new Vector3(x - .5f, -1, y + .5f);
                            Vector3 d = new Vector3(x - .5f, -1, y - .5f);
                            Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                            for (int k = 0; k < 6; k++)
                            {
                                vertices.Add(v[k]);
                                triangles.Add(triangles.Count);
                            }
                        }
                    }
                    if (x < size - 1)
                    {
                        Cell right = grid[x + 1, y];
                        if (right.isWater)
                        {
                            Vector3 a = new Vector3(x + .5f, 0, y - .5f);
                            Vector3 b = new Vector3(x + .5f, 0, y + .5f);
                            Vector3 c = new Vector3(x + .5f, -1, y - .5f);
                            Vector3 d = new Vector3(x + .5f, -1, y + .5f);
                            Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                            for (int k = 0; k < 6; k++)
                            {
                                vertices.Add(v[k]);
                                triangles.Add(triangles.Count);
                            }
                        }
                    }
                    if (y > 0)
                    {
                        Cell down = grid[x, y - 1];
                        if (down.isWater)
                        {
                            Vector3 a = new Vector3(x - .5f, 0, y - .5f);
                            Vector3 b = new Vector3(x + .5f, 0, y - .5f);
                            Vector3 c = new Vector3(x - .5f, -1, y - .5f);
                            Vector3 d = new Vector3(x + .5f, -1, y - .5f);
                            Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                            for (int k = 0; k < 6; k++)
                            {
                                vertices.Add(v[k]);
                                triangles.Add(triangles.Count);
                            }
                        }
                    }
                    if (y < size - 1)
                    {
                        Cell up = grid[x, y + 1];
                        if (up.isWater)
                        {
                            Vector3 a = new Vector3(x + .5f, 0, y + .5f);
                            Vector3 b = new Vector3(x - .5f, 0, y + .5f);
                            Vector3 c = new Vector3(x + .5f, -1, y + .5f);
                            Vector3 d = new Vector3(x - .5f, -1, y + .5f);
                            Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                            for (int k = 0; k < 6; k++)
                            {
                                vertices.Add(v[k]);
                                triangles.Add(triangles.Count);
                            }
                        }
                    }
                }
            }
        }
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        GameObject edgeObj = new GameObject("Edge");
        edgeObj.transform.SetParent(transform);

        MeshFilter meshFilter = edgeObj.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        MeshRenderer meshRenderer = edgeObj.AddComponent<MeshRenderer>();
        meshRenderer.material = edgeMaterial;
    }

    void DrawTexture(Cell[,] grid)
    {
        Texture2D texture = new Texture2D(size, size);
        Color[] colorMap = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Cell cell = grid[x, y];
                if (cell.isWater)
                    colorMap[y * size + x] = Color.blue;
                else
                    colorMap[y * size + x] = Color.green;
            }
        }
        texture.filterMode = FilterMode.Point;
        texture.SetPixels(colorMap);
        texture.Apply();

        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        meshRenderer.material = terrainMaterial;
        meshRenderer.material.mainTexture = texture;
    }

    void GenerateObjects(Cell[,] grid, GameObject[] PrefabList, bool[] possibleTerrain, float density, float scale)
    {
        float[,]noiseMap = new float[size, size];
        (float xOffset, float yOffset) = (Random.Range(-10000f, 10000f), Random.Range(-10000f, 10000f));
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float noiseValue = Mathf.PerlinNoise(x * treeNoiseScale + xOffset, y * treeNoiseScale + yOffset);
                noiseMap[x, y] = noiseValue;
            }
        }


        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Cell cell = grid[x, y];
                if (!cell.hasObject && !cell.isWater)
                {
                    if (cell.isSand && possibleTerrain[0])
                    {
                        float v = Random.Range(0f, density);
                        if (noiseMap[x, y] < v)
                        {
                            cell.hasObject = true;
                            GameObject prefab = PrefabList[Random.Range(0, PrefabList.Length)];
                            Debug.Log(prefab.name);
                            GameObject obj = Instantiate(prefab, cubeMap[x, y].transform.GetChild(1).transform.position, Quaternion.identity);
                            // tree.transform.position = new Vector3(x, 0, y);
                            obj.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
                            obj.transform.localScale = Vector3.one * Random.Range(.8f, 1.2f) * scale;
                        }
                    } else if (cell.isGrass && possibleTerrain[1])
                    {
                        float v = Random.Range(0f, density);
                        if (noiseMap[x, y] < v)
                        {
                            cell.hasObject = true;
                            GameObject prefab = PrefabList[Random.Range(0, PrefabList.Length)];
                            Debug.Log(prefab.name);
                            GameObject obj = Instantiate(prefab, cubeMap[x, y].transform.GetChild(1).transform.position, Quaternion.identity);
                            // tree.transform.position = new Vector3(x, 0, y);
                            obj.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
                            obj.transform.localScale = Vector3.one * Random.Range(.8f, 1.2f) * scale;
                        }
                    } else if (cell.isMountain && possibleTerrain[2])
                    {
                        float v = Random.Range(0f, density);
                        if (noiseMap[x, y] < v)
                        {
                            cell.hasObject = true;
                            GameObject prefab = PrefabList[Random.Range(0, PrefabList.Length)];
                            Debug.Log(prefab.name);
                            GameObject obj = Instantiate(prefab, cubeMap[x, y].transform.GetChild(1).transform.position, Quaternion.identity);
                            // tree.transform.position = new Vector3(x, 0, y);
                            obj.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
                            obj.transform.localScale = Vector3.one * Random.Range(.8f, 1.2f) * scale;
                        }
                    }
                }
            } // I'm so fucking retarted
        }
    }


    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Cell cell = grid[x, y];
                if (cell.isWater)
                    Gizmos.color = Color.blue;
                else
                    Gizmos.color = Color.green;
                Vector3 pos = new Vector3(x, 0, y);
                Gizmos.DrawCube(pos, Vector3.one);
            }
        }
    }

    public void Reload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}