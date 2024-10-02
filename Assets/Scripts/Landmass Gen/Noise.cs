using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise //only need one instance so static
{

    public enum NormalizeMode{
        local, //use local min and max for noise gen
        Global //for estimating a global min and max
    };

    //Method for generating a noise map
    //We want it to return a grid of values between 0 and 1 
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, NoiseSettings settings, Vector2 sampleCenter){
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random pseudoRng = new System.Random(settings.seed); //if we want to get the same map use the same seed
        Vector2[] octaveOffsets = new Vector2[settings.octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float freq = 1;

        for(int i=0;i<settings.octaves;i++){
            //Do not give perlinNoise a coordinate that is too high or it will return the same value, thats where that ranges comes from offset.x / y is so we can add our own offset to the random values
            float offsetX = pseudoRng.Next(-100000,100000) + settings.offset.x + sampleCenter.x; 
            float offsetY = pseudoRng.Next(-100000,100000) - settings.offset.y - sampleCenter.y; //we subtract in order for the offset to work correctly in the y coordinate
            octaveOffsets[i] = new Vector2(offsetX,offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= settings.persistance; //range 0-1
        }

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        //both are used so when playing with the noise scale value we "zoom in" in the center instead of top right. Not mandatory but convinient 
        float halfWidth = mapWidth / 2f; 
        float halfHeight = mapHeight / 2f;

        for (int y = 0; y < mapHeight; y++) {
			for (int x = 0; x < mapWidth; x++) {

                amplitude = 1;
                freq = 1;
                float noiseHeight = 0;

                for(int i=0; i< settings.octaves;i++){
                    //get non integer values, The higher the freq the further apart the sample points will be 
                    //that means height values change more rapidly
                    float sampleX = (x-halfWidth + octaveOffsets[i].x) / settings.scale * freq; // get non integer values
                    float sampleY = (y-halfHeight + octaveOffsets[i].y) / settings.scale * freq;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 -1; //Its better to get some negative values for the generation
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= settings.persistance; //range 0-1
                    freq *=  settings.lacunarity;
                }
                
                if(noiseHeight > maxLocalNoiseHeight){
                    maxLocalNoiseHeight = noiseHeight;
                }

                if(noiseHeight < minLocalNoiseHeight){
                    minLocalNoiseHeight = noiseHeight;
                }
				noiseMap[x,y] = noiseHeight;

                if(settings.normalizeMode == NormalizeMode.Global){
                    float normalizedHeight = (noiseMap[x,y] + 1) / (2f * maxPossibleHeight / 2.3f); //the 2.3f is used to reduce the maxPossible height in order to generate peaks etc
                    noiseMap[x,y] = Mathf.Clamp(normalizedHeight,0,int.MaxValue); 
                }
			}
		}

        if(settings.normalizeMode == NormalizeMode.local){
            for (int y = 0; y < mapHeight; y++) {
                for (int x = 0; x < mapWidth; x++) {
                    noiseMap[x,y] = Mathf.InverseLerp(minLocalNoiseHeight,maxLocalNoiseHeight,noiseMap[x,y]); //inverseLerp returns a value between 0-1, essentially normalizing the noiseMap
                }
            }
        }

        ApplyFlatSpots(noiseMap);

        return noiseMap;
    }

     public static void ApplyFlatSpots(float[,] noiseMap){
        int seed = 10;
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        // Define parameters for flat spot generation
        float flatSpotIntensity = 0.7f; // Adjust as needed
        float flatSpotWidth = 0.8f;     // Adjust as needed

         // Custom random number generator
        System.Random random = new System.Random(seed); // Initialize with seed
        // Calculate the number of mountains/hills
        int numMountains = random.Next(1, 3);

        // Calculate the width of each mountain/hill
        float mountainWidth = width / (float)numMountains;

        // Loop through each point in the noise map
        for (int y = 0; y < height; y++){
            for (int x = 0; x < width; x++){
                // Calculate the distance to the nearest mountain/hill
                float distanceToNearestMountain = Mathf.Min(Mathf.Abs(x % mountainWidth - mountainWidth / 2), Mathf.Abs(x % mountainWidth - mountainWidth / 2 - mountainWidth));

                // Determine the height modification based on distance
                float flatSpotHeight = Mathf.Lerp(1f, flatSpotIntensity, distanceToNearestMountain / (mountainWidth * flatSpotWidth));

                // Apply the flat spot height modification to the noise map
                noiseMap[x, y] *= flatSpotHeight;
            }
        }
    }
    
}

[Serializable]
public class NoiseSettings {
    public Noise.NormalizeMode normalizeMode;
    public float scale = 50;
    public int octaves = 6;
    [Range(0,1)] //sets the range of the persistance
    public float persistance = 0.6f;
    public float lacunarity = 1.5f;
    public int seed;
    public Vector2 offset;

    //Its called at the start of the Terrain Generation Process
    public void randomSeed(){
        System.Random random = new System.Random();
        this.seed = random.Next(1, 10001);
    }


    public void ValidateValues(){
        scale = Mathf.Max(scale,0.01f);
        octaves = Mathf.Max(octaves,1);
        lacunarity = Mathf.Max(lacunarity,1);
        persistance = Mathf.Clamp01(persistance);

    }
}