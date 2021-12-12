using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldVariables : MonoBehaviour
{
    //camera obj vars
    public GameObject cameraobj;

    //mesh size
    public int meshx;
    public int meshz;
    public float globalheight;
    
    //noise vars
    public int randseed;
    public float noiseseed;
    public float noisescale;
    public int noiseoctaves;

    //world vars
    public int worldsize;
    public GameObject meshentity;
    public float meshgendistance;

    public void centercamera()
    {
        cameraobj = GameObject.FindGameObjectWithTag("MainCamera");
        cameraobj.transform.position = new Vector3((meshx * 1.5f) * (worldsize / 2), 2, (meshz * 1.5f) * (worldsize / 2));
    }

    private void Start()
    {
        centercamera();
        meshentity = GameObject.FindGameObjectWithTag("MeshEntity");
        if (worldsize % 2 == 0)
        {
            for (int currentx = 0, currentz = 0, x = 0; x < worldsize; x++)
            {
                for (int z = 0; z < worldsize; z++)
                {
                    Instantiate(meshentity, new Vector3(currentx, 0, currentz), Quaternion.identity);
                    currentz += meshx;
                }
                currentz = 0;
                currentx += meshz;
            }
        }
        else
        {
            Debug.Log("Worldsize must be divisible by 2...");
        }
        
    }
}
