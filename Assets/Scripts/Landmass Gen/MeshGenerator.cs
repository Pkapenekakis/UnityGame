using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public static class MeshGenerator {


    public static MeshData GenerateTerrainMesh(float[,] heightMap, MeshSettings meshSettings, int levelOfDetail){
        int meshSimplifIncr = (levelOfDetail == 0)?1:levelOfDetail * 2;
        int borderedSize = heightMap.GetLength(0);
        int meshSize = borderedSize - 2*meshSimplifIncr;
        int meshSizeUnsimplified = borderedSize - 2;

        //division keeps them floats and prevents ints
        float topLeftX = (meshSizeUnsimplified-1) / -2f; 
        float topLeftZ = (meshSizeUnsimplified-1) / 2f;

        
        int verticesPerLine = (meshSize-1) / meshSimplifIncr +1;

        MeshData meshData = new MeshData(verticesPerLine,meshSettings.useFlatShading);
        int[,] vertexIndicesMap = new int[borderedSize,borderedSize];
        int meshVertexIndex = 0;
        int borderVertexIndex= -1;

         for(int y=0; y<borderedSize; y+= meshSimplifIncr){
            for(int x=0; x<borderedSize; x+= meshSimplifIncr){
                bool isBorderVertex = y == 0 || y == borderedSize - 1 || x==0 || x == borderedSize - 1;
                if(isBorderVertex){
                    vertexIndicesMap[x,y] = borderVertexIndex;
                    borderVertexIndex-- ;
                }else{
                    vertexIndicesMap[x,y] = meshVertexIndex;
                    meshVertexIndex++;
                }
            }
        }

        for(int y=0; y<borderedSize; y+= meshSimplifIncr){
            for(int x=0; x<borderedSize; x+= meshSimplifIncr){
                int vertexIndex = vertexIndicesMap[x,y];

                // tell each vertex where it is in relation to the rest of the map as a % of x,y axis. The % has range 0-1
                Vector2 percent = new Vector2((x-meshSimplifIncr) / (float)meshSize, (y-meshSimplifIncr) / (float)meshSize); 
                float height = heightMap[x,y];
                //topLeft variables help keep it centered in the middle of the screen
                Vector3 vertexPos= new Vector3((topLeftX + percent.x * meshSizeUnsimplified) * meshSettings.meshScale, height, (topLeftZ - percent.y * meshSizeUnsimplified) * meshSettings.meshScale);

                meshData.addVertex(vertexPos,percent,vertexIndex);

                if(x < borderedSize - 1 && y < borderedSize - 1){ //ignores right and bottom vertices of the map since we cannot generate there
                    int a = vertexIndicesMap[x,y];
                    int b = vertexIndicesMap[x+meshSimplifIncr,y];
                    int c = vertexIndicesMap[x,y+meshSimplifIncr];
                    int d = vertexIndicesMap[x+meshSimplifIncr,y+meshSimplifIncr];
                    meshData.AddTriangle(a,d,c);
                    meshData.AddTriangle(d,a,b);
                }
                
                vertexIndex++; //keep track where we are in the array
            }
        }

        meshData.ProcessMesh();

        return meshData;
    }
    
}

public class MeshData{
    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;

    Vector3[] bakedNormals;
    
    Vector3[] borderVertices;
    int[] borderTriangles;
    int borderTriangleIndex;
    int triangleIndex;
    bool useFlatShading;

    public MeshData(int verticesPerLine, bool useFlatShading){
        this.useFlatShading = useFlatShading;
        vertices = new Vector3[verticesPerLine * verticesPerLine]; //calculates the number of vertices in the array
        triangles = new int[(verticesPerLine-1)*(verticesPerLine-1)*6]; //calculates the number of trianlges in the array
        uvs = new Vector2[verticesPerLine* verticesPerLine];

        borderVertices = new Vector3[verticesPerLine*4 +4];
        borderTriangles = new int[24 * verticesPerLine];
    }

    public void addVertex(Vector3 vertexPos, Vector2 uv, int vertexIndex){
        if(vertexIndex < 0){ //border vertex
            borderVertices[-vertexIndex-1] = vertexPos;
        }else{
            vertices[vertexIndex] = vertexPos;
            uvs[vertexIndex] = uv;
        }
    }

    public void AddTriangle(int a, int b, int c){
        if(a < 0 || b<0 || c<0){ //check if it is a border triangle
            borderTriangles[borderTriangleIndex] = a;
            borderTriangles[borderTriangleIndex+1] = b;
            borderTriangles[borderTriangleIndex+2] = c;
            borderTriangleIndex += 3;
        }else{
            triangles[triangleIndex] = a;
            triangles[triangleIndex+1] = b;
            triangles[triangleIndex+2] = c;
            triangleIndex += 3;
        }      
    }

    Vector3 [] CalculateNormals(){
        Vector3[] vertexNormals = new Vector3[vertices.Length];
        int triangleCount = triangles.Length / 3;
        for(int i=0; i < triangleCount; i++){
            int normalTriangleIndex = i * 3;
            int vertexIndexA = triangles[normalTriangleIndex];
            int vertexIndexB = triangles[normalTriangleIndex +1];
            int vertexIndexC = triangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA,vertexIndexB,vertexIndexC);
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }

        //loop through border triangles
        int borderTriangleCount = borderTriangles.Length / 3;
        for(int i=0; i < borderTriangleCount; i++){
            int normalTriangleIndex = i * 3;
            int vertexIndexA = borderTriangles[normalTriangleIndex];
            int vertexIndexB = borderTriangles[normalTriangleIndex +1];
            int vertexIndexC = borderTriangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA,vertexIndexB,vertexIndexC);
            if(vertexIndexA >= 0){
                 vertexNormals[vertexIndexA] += triangleNormal;
            }
            if(vertexIndexB >= 0){
                 vertexNormals[vertexIndexB] += triangleNormal;
            }
            if(vertexIndexC >= 0){
                vertexNormals[vertexIndexC] += triangleNormal;
            }       
        }

        //normalise each value in the array
        for(int i=0; i < vertexNormals.Length; i++){
            vertexNormals[i].Normalize();
        }

        return vertexNormals;
    }

    Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC){
        Vector3 pointA = (indexA < 0)?borderVertices[-indexA-1] : vertices[indexA];
        Vector3 pointB = (indexB < 0)?borderVertices[-indexB-1] : vertices[indexB];
        Vector3 pointC = (indexC < 0)?borderVertices[-indexC-1] :vertices[indexC];

        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;
        return Vector3.Cross(sideAB,sideAC).normalized;
    }

    //this function is used in order for the normals to be calculated to the individual threads instead of the main game thread
    void BakeNormals(){
        bakedNormals = CalculateNormals();
    }

    void FlatShading(){
        Vector3[] flatShadedVertices = new Vector3[triangles.Length];
        Vector2[] flatShadedUvs = new Vector2[triangles.Length];

        for(int i=0; i< triangles.Length; i ++){
            flatShadedVertices[i] = vertices[triangles[i]];
            flatShadedUvs[i] = uvs[triangles[i]];
            triangles[i] = i;
        }

        vertices = flatShadedVertices;
        uvs = flatShadedUvs;
    }

    public void ProcessMesh(){
        if(useFlatShading){
            FlatShading();
        }else{
            BakeNormals();
        }
    }

    public Mesh CreateMesh(){
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        if(useFlatShading){
            mesh.RecalculateNormals();
        }else{
            mesh.normals = bakedNormals; //makes lighting work correctly
        }
        return mesh; 
    }


}
