using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator{

    public static Texture2D TextureFromColourMap(Color[] colormap, int width, int height){
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point; //fixes the blurriness
        texture.wrapMode = TextureWrapMode.Clamp; //fixes seeing small amounts of  one side of the map on the other side
        texture.SetPixels(colormap);
        texture.Apply();
        return texture;
    }

    public static Texture2D TextureFromHeightMap(HeightMap heightMap){
        int width = heightMap.values.GetLength(0);
        int height = heightMap.values.GetLength(1);

        Color[] colourMap = new Color[width*height]; //generate a colour map from our height map

        for(int y = 0; y < height; y++){
            for(int x = 0; x < width; x++){
                colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, Mathf.InverseLerp(heightMap.minValue,heightMap.maxValue,heightMap.values[x,y]));
            }
        }
        return TextureFromColourMap(colourMap, width,height);
    }

}
