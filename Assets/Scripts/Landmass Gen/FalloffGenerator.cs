using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FalloffGenerator
{
    public static float[,] GenerateFalloffMap(int size){
        float[,] map = new float[size,size];

        //populate the map with values
        //i and j are coordinates of a point inside the square map
        for(int i=0; i<size; i++){
            for(int j=0; j<size; j++){
                float x = i / (float)size * 2 -1; //gives values from -1 to 1
                float y = j / (float)size * 2 -1; //gives values from -1 to 1

                //find if x or y are closest to the edge of the square
                float value = MathF.Max(MathF.Abs(x),Mathf.Abs(y));
                map[i,j] = Evaluate(value);
            }
        }
        return map;
    }


    //Function that creates a mathematical function that expands our falloff map in order for the "land" to be stronger
    static float Evaluate(float value){
        float a = 3; //trial and error
        float b = 2.2f; //trial and error

        return MathF.Pow(value,a) / ( (MathF.Pow(value,a) + MathF.Pow(b-b*value, a)) );
    }
    
}
