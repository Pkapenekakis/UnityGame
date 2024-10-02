using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class GeneratePrefabs{


    float minHeight = 0.015f; //same as the start Height of the grass "layer"
    float maxHeight = 0.29f; //-0.01 as the start Height of the stony ground "layer"
    Bounds bounds;
    HeightMap heightMap;

    [Header("Prefab Variation Settings")]
    Vector2 rotationRange = new Vector2(0,360);

    public GeneratePrefabs(HeightMap heightMap, Bounds bounds){
        this.heightMap = heightMap;
        this.bounds = bounds;
    }
    public void Generate(GameObject prefab, int density, Transform parent, float yOffset){

        //Clear();

        for(int i=0;i < density;i++){
            float sampleX = Random.Range(bounds.min.x, bounds.max.x);
            float sampleZ = Random.Range(bounds.min.y, bounds.max.y);
            float height = heightMap.values[(int)((sampleX - bounds.min.x) / bounds.size.x * (heightMap.values.GetLength(0) - 1)),
                                             (int)((sampleZ - bounds.min.y) / bounds.size.y * (heightMap.values.GetLength(1) - 1))];

            //Experimental
            /*
            int heightMapWidth = heightMap.values.GetLength(0);
            int heightMapHeight = heightMap.values.GetLength(1);

            // Ensure the sampled coordinates are within the bounds of the heightmap
            int xIndex = Mathf.Clamp((int)((sampleX - bounds.min.x) / bounds.size.x * (heightMapWidth - 1)), 0, heightMapWidth - 1);
            int zIndex = Mathf.Clamp((int)((sampleZ - bounds.min.y) / bounds.size.y * (heightMapHeight - 1)), 0, heightMapHeight - 1);
            float height = heightMap.values[xIndex, zIndex];
            //EXPPPPPPPPPPPPPPPPPPPPPPPPP */

            if (height >= minHeight && height <= maxHeight){
                Vector3 position = new Vector3(sampleX, height + yOffset, sampleZ);
                GameObject gameObj = GameObject.Instantiate(prefab, position, Quaternion.identity, parent);
            }
        }

        Physics.SyncTransforms();
    }

    /*
    public void Clear(){
        while(transform.childCount !=0){
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    } */
}
