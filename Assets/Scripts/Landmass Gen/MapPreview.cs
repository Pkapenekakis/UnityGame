using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPreview : MonoBehaviour
{
    public Renderer textureRenderer;
    public MeshFilter meshfilter;
    public MeshRenderer meshRenderer;
    public enum DrawMode{
        noiseMap, mesh, FalloffMap
    }
    public DrawMode drawMode;

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureData textureData;
    public Material terrainMaterial;

    [Range(0,MeshSettings.numSupportedLODs -1)]
    public int EditorLOD;
    public bool autoUpdate; 

    public void DrawMapInEditor(){
        textureData.ApplyToMaterial(terrainMaterial);
        //needs to be called on the main thread
        textureData.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numberOfVertsPerLine, meshSettings.numberOfVertsPerLine, heightMapSettings, UnityEngine.Vector2.zero);

        if(drawMode == DrawMode.noiseMap){
            DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap));
        /*}else if(drawMode == DrawMode.colourMap){
            display.DrawTexture(TextureGenerator.TextureFromColourMap(mapData.colourMap,mapChunkSize,mapChunkSize)); */
        }else if (drawMode == DrawMode.mesh){
            DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, EditorLOD)/*, TextureGenerator.TextureFromColourMap(mapData.colourMap,mapChunkSize,mapChunkSize)*/);
        }else if(drawMode == DrawMode.FalloffMap){
            DrawTexture(TextureGenerator.TextureFromHeightMap(new HeightMap(FalloffGenerator.GenerateFalloffMap(meshSettings.numberOfVertsPerLine),0,1)));
        }   
    }

    public void DrawTexture(Texture2D texture){
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(texture.width,1,texture.height) / 10f; //appears better in editor

        textureRenderer.gameObject.SetActive(true);
        meshfilter.gameObject.SetActive(false);
    }

    public void DrawMesh(MeshData meshData){
        meshfilter.sharedMesh = meshData.CreateMesh(); //must be shared since we may be generating the mesh from outside gamemode
        textureRenderer.gameObject.SetActive(false);
        meshfilter.gameObject.SetActive(true);
    }

    //Checks and keeps the values within bounds in the inspector (and in general)
     void OnValidate() { //when a value on script is changed on the inspector
        
        if(meshSettings != null){
            //Combination of both ensures that the method is only run once every time data is updated
            meshSettings.OnValuesUpdated -= OnValuesUpdated;
            meshSettings.OnValuesUpdated += OnValuesUpdated;
        }
        if(heightMapSettings != null){
            //Combination of both ensures that the method is only run once every time data is updated
            heightMapSettings.OnValuesUpdated -= OnValuesUpdated;
            heightMapSettings.OnValuesUpdated += OnValuesUpdated;
        }
        if(textureData !=null){
            textureData.OnValuesUpdated -= OnValuesUpdated;
            textureData.OnValuesUpdated += OnValuesUpdated;
        }
        
    }

    void OnValuesUpdated(){
        textureData.ApplyToMaterial(terrainMaterial);
        if(!Application.isPlaying){
            DrawMapInEditor();
        }
    }

}
