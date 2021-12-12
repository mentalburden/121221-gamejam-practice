using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGen : MonoBehaviour
{
    //outside vars
    public float currentx;
    public float currentz;
    public bool generated;
    public bool destroyed;

    //camera-controller tracking
    public GameObject cameraobj;
    public float distancetocamera;

    //inside vars
    Mesh mesh;
    Vector3[] vertices;
    Vector2[] uvs;
    float[,] thisnoise;
    Texture2D texture;
    Renderer rendy;        
    int globalx;    
    int globalz;
    float globalheight;
    int randseed;
    float noiseseed;
    float noisescale;
    int noiseoctaves;
    float meshgendistance;
    MeshCollider meshCollider;

    private void Awake()
    {
        cameraobj = GameObject.FindGameObjectWithTag("MainCamera");
        WorldVariables worldvars = GameObject.FindGameObjectWithTag("WorldController").GetComponent<WorldVariables>();
        globalx = worldvars.meshx;
        globalz = worldvars.meshz;
        globalheight = worldvars.globalheight;
        randseed = worldvars.randseed;
        noiseseed = worldvars.noiseseed;
        noisescale = worldvars.noisescale;
        noiseoctaves = worldvars.noiseoctaves;
        meshgendistance = worldvars.meshgendistance;
        meshCollider = gameObject.AddComponent<MeshCollider>();

        currentx = transform.position.x;
        currentz = transform.position.z;

        thisnoise = GenerateNoiseMap(globalx + 1, globalz + 1, randseed, noisescale, noiseoctaves, noiseseed, noiseseed, new Vector2(globalx, globalz));
        GenerateMesh(globalx, globalz);
        texture = GenerateTexture(globalx, globalz);
        rendy = GetComponent<Renderer>();
        rendy.material.EnableKeyword("_NORMALMAP");
        rendy.material.SetTexture("_MainTex", texture);
        meshCollider.sharedMesh = mesh;
        Debug.Log("mesh created");
        generated = false;
    }


    //void Update()
    //{
    //    distancetocamera = Vector3.Distance(transform.position, cameraobj.transform.position);
    //    bool generatedrunonce = true;
    //    bool destroyedrunonce = true;
    //    if (distancetocamera < meshgendistance && generatedrunonce)
    //    {
    //        generated = true;
    //        generatedrunonce = false;
    //    }
    //    else if (distancetocamera > meshgendistance && destroyedrunonce)
    //    {
    //        destroyed = true;
    //        destroyedrunonce = false;
    //    }
    //    if (generated)
    //    {

    //    }
    //    if (destroyed)
    //    {
    //        DestroyMesh();            
    //        destroyedrunonce = false;
    //        generatedrunonce = true;
    //        Debug.Log("mesh destroyed");
    //        destroyed = false;
    //    }
    //}
    public float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }
        if (scale <= 0)
        {
            scale = 0.0001f;
        }
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;
        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;
                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x + currentx - halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y + currentz - halfHeight) / scale * frequency + octaveOffsets[i].y;
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;
                    amplitude *= persistance;
                    frequency *= lacunarity;
                }
                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
            }
        }
        return noiseMap;
    }

    public void DestroyMesh()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.Clear();
    }

    public void GenerateMesh(int xSize, int zSize)
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        uvs = new Vector2[vertices.Length];
        
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++, i++)
            {
                float thisy = thisnoise[x, z];
                vertices[i] = new Vector3(x, thisy, z);
                //Instantiate(testcube, new Vector3(x, thisy, z), Quaternion.identity);
                uvs[i] = new Vector2((float)x / xSize, (float)z / zSize);
            }
        }
        mesh.vertices = vertices;
        mesh.uv = uvs;

        int[] triangles = new int[xSize * zSize * 6];
        for (int ti = 0, vi = 0, z = 0; z < zSize; z++, vi++)
        {
            for (int x = 0; x < xSize; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
                triangles[ti + 5] = vi + xSize + 2;
            }
        }
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    public Texture2D GenerateTexture(int xSize, int zSize)
    {
        Texture2D texture = new Texture2D(xSize, zSize);
        Color[] colours = new Color[xSize * zSize];
        for (int i = 0, z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                float noiseval = thisnoise[x, z];
                
                if (noiseval >= 0.9f)
                {
                    colours[i] = Color.white;
                }
                else
                {
                    //colours[i] = new Color(noiseval, noiseval, noiseval);
                    colours[i] = Color.black;
                }
                i++;
            }
        }
        texture.SetPixels(colours);
        texture.filterMode = FilterMode.Trilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();
        Debug.Log("texture generated");
        return texture;       
    }
}
