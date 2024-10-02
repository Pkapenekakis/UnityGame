using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class MeshSettings : UpdatableData
{
    
    public const int numSupportedLODs = 5;
    public const int numOfSupportedChunkSizes = 9;
    public const int numOfSupportedFlatShadedChunkSizes = 3;

    //Acceptable Chunk sizes for our meshSimplifIncr (0 and 24 excluded since they are too small to be useful)
    public static readonly int[] supportedChunkSizes = {48, 72, 96, 120, 144, 168, 192, 216, 240};
    //public static readonly int[] supportedFlatShadedChunkSizes = {48, 72, 96};

    public bool useFlatShading;
    public float meshScale = 5f;

    [Range(0, numOfSupportedChunkSizes-1)]
    public int chunkSizeIndex;
    [Range(0, numOfSupportedFlatShadedChunkSizes-1)]
    public int flatShadedChunkSizeIndex;

    //num verts per line of mesh rendered at LOD = 0. Includes the 2 extra verts that are excluded from final mesh, but are used on normals calculation
    //Map chunk size when flat Shading is used needs to be a lot smaller since a lot more vertices are being generated (unity has a limit)
    public int numberOfVertsPerLine{
        get{
            return supportedChunkSizes[(useFlatShading)?flatShadedChunkSizeIndex:chunkSizeIndex] + 1; //for flatshading the first 3 values of chunkSize are the same
        }
    }

    //get how much space the mesh takes up
    public float meshWorldSize{
        get{
            return (numberOfVertsPerLine - 3) * meshScale;
        }
    }

    
}
