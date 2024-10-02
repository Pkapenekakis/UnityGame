using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk{
        const float colliderGenerationDistanceThreshold = 6; //how close is the player to the end of the chunk before it creates the collider

        public event System.Action<TerrainChunk, bool> onVisibilityChanged;
        public Vector2 coord;

        Vector2 sampleCenter;
        GameObject meshObject;
        Bounds bounds;
        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;

        LODInfo[] detailLevels;
        LODMesh[] lodMeshes;
        int colliderLODIndex;

        public HeightMap heightMap;
        public bool heightMapReceived;
        int previousLODIndex = -1; //Used to prevent updating anything if things did not change
        bool hasSetCollider;
        float maxViewDist;

        HeightMapSettings heightMapSettings;
        MeshSettings meshSettings;
        Transform viewer;
        public GeneratePrefabs genPref;
        Transform parent;

        //Constructor
        public TerrainChunk(Vector2 coord,HeightMapSettings heightMapSettings ,MeshSettings meshSettings,LODInfo[] detailLevels, int colliderLODIndex, Transform parent, Transform viewer ,Material material){ 
            this.coord = coord;
            this.detailLevels = detailLevels;
            this.colliderLODIndex = colliderLODIndex;
            this.heightMapSettings = heightMapSettings;
            this.meshSettings = meshSettings;
            this.viewer = viewer;
            this.parent = parent;
            sampleCenter = coord * meshSettings.meshWorldSize / meshSettings.meshScale;
            Vector2 pos = this.coord * meshSettings.meshWorldSize;

            bounds = new Bounds(sampleCenter,Vector2.one * meshSettings.meshWorldSize);
            

            //meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer.material = material;
            meshCollider = meshObject.AddComponent<MeshCollider>();

            meshObject.transform.position =new Vector3(pos.x, 0, pos.y);
            meshObject.transform.parent = parent; //transform parent is used in order to make the generated planes(chunks) child of the mapGenerator for easier debugging
            SetVisible(false);
            meshObject.tag = ColliderController.groundTag; //used for item collider need to match with the name on ColliderController

            lodMeshes = new LODMesh[detailLevels.Length];
            for(int i = 0; i < detailLevels.Length; i++){
                lodMeshes[i] = new LODMesh(detailLevels[i].lod);
                lodMeshes[i].updateCallback += UpdateTerrainChunk;
                if(i == colliderLODIndex){
                    lodMeshes[i].updateCallback += UpdateCollisionMesh;
                }
                
            }

            maxViewDist = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
        }

        public void Load(){ 
            ThreadedDataRequester.RequestData(() => HeightMapGenerator.GenerateHeightMap(meshSettings.numberOfVertsPerLine,meshSettings.numberOfVertsPerLine, heightMapSettings, sampleCenter),OnHeightMapReceived);
        }

        Vector2 viewerPosition{
            get{
                return new Vector2(viewer.position.x, viewer.position.z);
            }
        }

        /*
        The chunk finds the point on its perimeter that is the closest on the viewers pos and it will find tthe distance between that
        point and the viewer and if that distance is less than the maxViewDist it will make sure the mesh object is enabled, otherwise it will disable it
        */
        public void UpdateTerrainChunk(){

            if(heightMapReceived){
                float viewDistFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool wasVisible = IsVisible();
                bool visible = viewDistFromNearestEdge <= maxViewDist;

                if(visible){
                    int lodIndex = 0;
                    for(int i =0;i<detailLevels.Length-1;i++){ //we do not need to look at the last one because visible will be false 
                        if(viewDistFromNearestEdge > detailLevels[i].visibleDstThreshold){
                            lodIndex = i+1;
                        }else{
                            break;
                        }
                    }

                    if(lodIndex != previousLODIndex){
                        LODMesh lodMesh = lodMeshes[lodIndex];
                        if(lodMesh.hasMesh){ //We receive the mesh
                            previousLODIndex = lodIndex;
                            meshFilter.mesh = lodMesh.mesh; //set the current mesh to the lodMesh
                        }else if(!lodMesh.hasRequestedMesh){
                            lodMesh.RequestMesh(heightMap,meshSettings);
                        }
                    }
                    
                }

                if(wasVisible != visible){
                    SetVisible(visible);
                    if(onVisibilityChanged != null){
                        onVisibilityChanged(this,visible);
                    } 
                }
                
            }      
        }

        public void UpdateCollisionMesh(){
            if(!hasSetCollider){
                float sqrDstFromViewerToEdge = bounds.SqrDistance(viewerPosition);

                if(sqrDstFromViewerToEdge < detailLevels[colliderLODIndex].sqrVisibleDstThreshold){
                    if(!lodMeshes[colliderLODIndex].hasRequestedMesh){
                        lodMeshes[colliderLODIndex].RequestMesh(heightMap,meshSettings);
                    }
                }

                if(sqrDstFromViewerToEdge < colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold){
                    if(lodMeshes[colliderLODIndex].hasMesh){
                        meshCollider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
                        hasSetCollider = true;
                        GenerateBoosters(); //We only care about the boosters when the chunk has a mesh (and a collider) else they fall through the ground
                    }
                }
            }
            
        }

        public void SetVisible(bool visible){
            meshObject.SetActive(visible);
        }

        //method hat finds out if a mesh is visible
        public bool IsVisible(){
            return meshObject.activeSelf;
        }

        void OnHeightMapReceived(object heightMapObj){
            this.heightMap = (HeightMap)heightMapObj;
            heightMapReceived = true; 
            UpdateTerrainChunk();
            genPref = new GeneratePrefabs(heightMap,bounds);

            //Generate Trees
            GameObject treePrefab = Resources.Load<GameObject>("Prefabs/Tree2");
            genPref.Generate(treePrefab,75,parent,0);

            GeneratePillars();
        }

        public void GenerateBoosters(){
            GameObject speedBooPref = Resources.Load<GameObject>("Prefabs/Potions/SpeedBooster");
            GameObject maxHBooPref = Resources.Load<GameObject>("Prefabs/Potions/MaxHealthBooster");
            GameObject healthBooPref = Resources.Load<GameObject>("Prefabs/Potions/HealthBooster");
            GameObject goldBag = Resources.Load<GameObject>("Prefabs/GoldBagPref");

            genPref.Generate(speedBooPref,30,parent,10);
            genPref.Generate(healthBooPref,30,parent,10);
            genPref.Generate(maxHBooPref,30,parent,10);
            genPref.Generate(goldBag,25,parent,10);

            GenerateNPC();
        }

        public void GenerateNPC(){
            GameObject swordNPC = Resources.Load<GameObject>("Prefabs/SwordNPC");
            GameObject vendorNPC = Resources.Load<GameObject>("Prefabs/VendorNPC");

            genPref.Generate(swordNPC,35,parent,6);
            genPref.Generate(vendorNPC,10,parent,6);
        }

        //Generates one of the 3 pillars in that chunk
        public void GeneratePillars(){
            System.Random random = new System.Random();
            int randomNumber = random.Next(0, 3); // Upper bound is exclusive
            GameObject pillar = GetPillarObj(randomNumber);

            if(pillar != null){
                genPref.Generate(pillar,6,parent,0);
            }else{
                Debug.Log("Invalid pillar");
            }
            
        }

        public GameObject GetPillarObj(int type){
            GameObject pillar; 
            if(type == 0){
                return pillar = Resources.Load<GameObject>("Prefabs/BluePillar");
            }else if(type ==1){
                return pillar = Resources.Load<GameObject>("Prefabs/YellowPillar");
            }else if(type == 2){
                return pillar = Resources.Load<GameObject>("Prefabs/GreenPillar");
            }

            return null;
        }
        /* WAS USED FOR TESTING THAT MESH GENERATION WORKS ON THREADING
        void OnMapDataReceived(MapData mapData){
            //Debug.Log("Map Data Received");
            mapGen.RequestMeshData(mapData, OnMeshDataReceived);
        }

        void OnMeshDataReceived(MeshData meshData){
            //Debug.Log("Map Data Received");
            meshFilter.mesh  = meshData.CreateMesh();
        } */
}

    //class is responsible for fetching its own mesh from the map generator
    class LODMesh{

        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;
        public event System.Action updateCallback;

        public LODMesh(int lod){
            this.lod = lod;
        }

        void OnMeshDataReceived(object meshDataObj){
            mesh = ((MeshData)meshDataObj).CreateMesh();
            hasMesh = true;

            updateCallback();
        }

        public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings){
            hasRequestedMesh = true;
            ThreadedDataRequester.RequestData(() => MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings,lod),OnMeshDataReceived);
        }

    }
