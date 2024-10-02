using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public static class HeightMapGenerator{
    public static HeightMap GenerateHeightMap(int width, int height, HeightMapSettings settings, UnityEngine.Vector2 sampleCenter){
        float [,] values = Noise.GenerateNoiseMap(width,height,settings.noiseSettings,sampleCenter);

        AnimationCurve heightCurve_threadsafe = new AnimationCurve(settings.heightCurve.keys); //creates a local copy to ensure threading does not break this

        float minVal = float.MaxValue;
        float maxVal = float.MinValue;

        for(int i=0; i < width; i++){
           for(int j=0; j < height; j++){
                values[i,j] *= heightCurve_threadsafe.Evaluate(values[i,j]) * settings.heightMult;

                if(values[i,j] > maxVal){
                    maxVal = values[i,j];
                }
                if(values[i,j] < minVal){
                    minVal = values[i,j];
                }
            } 
        }

        return new HeightMap(values, minVal, maxVal);
    }
}

public struct HeightMap{
    public readonly float[,] values; //readonly because structs are immutable
    public readonly float minValue;
    public readonly float maxValue;

    public HeightMap(float[,] values, float minVal, float maxVal){
        this.values = values;
        this.minValue = minVal;
        this.maxValue = maxVal;
    }
}