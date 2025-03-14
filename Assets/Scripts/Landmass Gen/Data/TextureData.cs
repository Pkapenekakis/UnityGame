using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu()]
public class TextureData : UpdatableData
{

   const int textureSize = 512;
   const TextureFormat textureFormat = TextureFormat.RGB565;

   public Layer[] layers;
   float savedMinHeight;
   float savedMaxHeight;
   public void ApplyToMaterial(Material material){

      material.SetInt("layerCount", layers.Length);
      material.SetColorArray("baseColours", layers.Select(x => x.tint).ToArray());
      material.SetFloatArray("baseStartHeights", layers.Select(x => x.startHeight).ToArray());
      material.SetFloatArray("baseBlends", layers.Select(x => x.blendStrength).ToArray());
      material.SetFloatArray("baseColourStrength", layers.Select(x => x.tintStrenght).ToArray());
      material.SetFloatArray("baseTextureScales", layers.Select(x => x.textureScale).ToArray());
      Texture2DArray texturesArray = GenerateTextureArray(layers.Select(x => x.texture).ToArray());
      material.SetTexture("baseTextures", texturesArray);

      UpdateMeshHeights(material, savedMinHeight,savedMaxHeight);
   }

   public void UpdateMeshHeights(Material material, float minHeight, float maxHeight){
      //Debug.Log("heights updated");
      savedMaxHeight = maxHeight;
      savedMinHeight = minHeight;

      material.SetFloat("minHeight", minHeight); //exactly same name as in shader
      material.SetFloat("maxHeight", maxHeight); //exactly same name as in shader
   }

   Texture2DArray GenerateTextureArray(Texture2D[] textures){
      Texture2DArray textureArray = new Texture2DArray(textureSize,textureSize,textures.Length,textureFormat,true);
      for(int i=0; i < textures.Length; i++){
         textureArray.SetPixels(textures[i].GetPixels(), i);
      }
      textureArray.Apply();
      return textureArray;
   }

   [System.Serializable]
   public class Layer{
      public Texture2D texture;
      public Color tint;
      [Range(0,1)]
      public float tintStrenght;
      [Range(0,1)]
      public float startHeight;
      [Range(0,1)]
      public float blendStrength;
      public float textureScale;
   }
}
