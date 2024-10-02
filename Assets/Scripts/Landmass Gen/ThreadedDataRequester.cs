using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

public class ThreadedDataRequester : MonoBehaviour
{

    static ThreadedDataRequester instance;

    void Awake(){
        instance = FindObjectOfType<ThreadedDataRequester>();
    }
    Queue<ThreadInfo> dataQueue = new Queue<ThreadInfo>();
    //Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    public static void RequestData(Func<object> generateData, Action<object> callback){
        ThreadStart threadStart = delegate { //Delegates can be thought as containers for functions that can be passed around like variables
            instance.DataThread(generateData, callback);
        };

        new Thread(threadStart).Start();
    }

    void DataThread(Func<object> generateData, Action<object> callback){
        object data = generateData(); //gets executed on the thread that called it
        lock(dataQueue){ //locks the mapDataThreadInfoQueue to prevent multiple threads accessing it at the same time
            dataQueue.Enqueue(new ThreadInfo(callback, data));
        }
    }

    /*
    public void RequestMeshData(HeightMap mapData,int lod,Action<MeshData> callback){
        ThreadStart threadStart = delegate {
			MeshDataThread(mapData, lod, callback);
		};

		new Thread (threadStart).Start ();
    }

    void MeshDataThread(HeightMap heightMap, int lod, Action<MeshData> callback){
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, lod);
        lock(meshDataThreadInfoQueue){ //locks the mapDataThreadInfoQueue to prevent multiple threads accessing it at the same time
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback,meshData));
        }
    } */


    private void Update() {
        if(dataQueue.Count > 0){
            for(int i=0;i<dataQueue.Count;i++){
                ThreadInfo threadInfo = dataQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            } 
        }

        /*
        if(meshDataThreadInfoQueue.Count > 0){
            for(int i=0;i<meshDataThreadInfoQueue.Count;i++){
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            } 
        } */
    }


    struct ThreadInfo{
        public readonly Action<object> callback; //readonly because structs are immutable
        public readonly object parameter; //readonly because structs are immutable

        public ThreadInfo(Action<object> callback, object parameter){
            this.callback = callback;
            this.parameter = parameter;
        }


    }


}
