using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TerrainGenerator : MonoBehaviour
{

    
    const float playerMoveThreshHoldForChunkUpdate = 25f;
    const float sqrtMoveThreshHoldForChunkUpdate = playerMoveThreshHoldForChunkUpdate*playerMoveThreshHoldForChunkUpdate;

    public int colliderLODIndex;
    public LODInfo[] detailLevels;

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureData textureSettings;

    public Transform viewer;
    Vector2 viewerPosition;
    Vector2 viewerPositionOld;
    public Material terrainMaterial;
    float meshWorldSize;
    int chunksVisible; //based on chunk size and view distance

    private bool firstSpawn = true; //used for spawning the player

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    List<Vector2> SpawnPointList = new List<Vector2>(); // List to store spawn points
    GeneratePrefabs genPre;
    //This start method "Generates" the entire world
    private void Start() {
        heightMapSettings.noiseSettings.randomSeed();
        textureSettings.ApplyToMaterial(terrainMaterial);
        textureSettings.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight); //needs to be called on the main thread

        float maxViewDist = detailLevels[detailLevels.Length-1].visibleDstThreshold;
        meshWorldSize = meshSettings.meshWorldSize;
        chunksVisible = Mathf.RoundToInt(maxViewDist / meshWorldSize) ;
        UpdateVisibleChunks();
    }

    private void Update() {
        //since everything is based on viewerPosition we just need to scale that instead of every value used
        viewerPosition = new Vector2(viewer.position.x,viewer.position.z); 
        
        //called when the player has moved
        if(viewerPosition != viewerPositionOld){
            foreach(TerrainChunk chunk in visibleTerrainChunks){
                chunk.UpdateCollisionMesh();
                UpdateSpawnPointList(chunk);
            }
            
        }

        if((viewerPositionOld-viewerPosition).sqrMagnitude > sqrtMoveThreshHoldForChunkUpdate){
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
        //if its the first time the game runs spawn the player after we setupt spawn points
        if(firstSpawn){
            foreach(TerrainChunk chunk in visibleTerrainChunks){
                chunk.UpdateCollisionMesh();
                UpdateSpawnPointList(chunk);
            }
            if(SpawnPointList.Count > 0){
                SpawnPlayerCharacter();
                firstSpawn = false;
                //Required for the restart case
                Time.timeScale = 1;
                Utils.Instance.CursorManagement(true); //unlock the cursor
            }   
        }

        if(Input.GetKeyDown(KeyCode.R)){
            SpawnPlayerCharacter();
        }      
    }

    void UpdateVisibleChunks(){
        HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();
        //disable chunks that are not visible anymore
        for(int i = visibleTerrainChunks.Count-1;i >= 0; i--){ //start at the end of the list so if a chunk removes itself we do not iterate over it
            alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].coord);
            visibleTerrainChunks[i].UpdateTerrainChunk();
        }

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x/meshWorldSize); //get the x coordinate the viewer is standing on
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y/meshWorldSize); //get the y coordinate the viewer is standing on

        for(int yOffset=-chunksVisible; yOffset <= chunksVisible; yOffset++){
            for(int xOffset=-chunksVisible; xOffset <= chunksVisible; xOffset++){
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                if(!alreadyUpdatedChunkCoords.Contains(viewedChunkCoord)){ //goes inside only if we do not have already updated the chunk
                    //if we only need to instantiate a new terrain chunk at this coordinate if one does not exist we need to maintain a dicionary of all the coordinates/terrain chunk
                    //in order to prevent duplicates
                    if(terrainChunkDictionary.ContainsKey(viewedChunkCoord)){
                        terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();

                    }else{  //instantiate new terrain chunk
                        TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, heightMapSettings, meshSettings, detailLevels, colliderLODIndex,transform,viewer,terrainMaterial);
                        terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
                        newChunk.onVisibilityChanged += onTerrainChunkVisibilityChanged;
                        newChunk.Load();
                    }
                }      
            }
        }
    }

    void UpdateSpawnPointList(TerrainChunk chunk){
        // Check if the chunk is visible
        if (chunk.IsVisible()){         
            // Check if the chunk has a flat area
            if (IsFlatArea(chunk)){
                // Add the center position of the chunk to the SpawnPointList
                SpawnPointList.Add(chunk.coord * meshSettings.meshWorldSize + Vector2.one * meshSettings.meshWorldSize / 2f);
            }
        }else{
            RemoveSpawnPoints(chunk);
        }
    }

    void RemoveSpawnPoints(TerrainChunk chunk){
        // Iterate through the SpawnPointList to find and remove spawn points corresponding to the chunk
        for (int i = SpawnPointList.Count - 1; i >= 0; i--){
            Vector2 spawnPoint = SpawnPointList[i];
            Vector2 chunkCoord = new Vector2(Mathf.RoundToInt(spawnPoint.x / meshSettings.meshWorldSize), Mathf.RoundToInt(spawnPoint.y / meshSettings.meshWorldSize));
            
            // Check if the spawn point corresponds to the chunk
            if (chunkCoord == chunk.coord){
                // Remove the spawn point from the SpawnPointList
                SpawnPointList.RemoveAt(i);
            }
        }
    }

    void onTerrainChunkVisibilityChanged(TerrainChunk chunk, bool isVisible){
        if(isVisible){
            visibleTerrainChunks.Add(chunk);
        }else{
            visibleTerrainChunks.Remove(chunk);
        }
    }

    void SpawnPlayerCharacter(){
        // Check if there are any available spawn points
        if (SpawnPointList.Count > 0){
            // Choose a random spawn point from the list
            Vector3 spawnPoint = GetRandomSpawnPoint();

            GameObject playerObject = GameObject.Find("PlayerCharacter");
            // Instantiate the player character at the chosen spawn point
            
            if (playerObject == null){
                // Instantiate a new player character at the chosen spawn point
                Instantiate(GameObject.Find("PlayerCharacter"), spawnPoint, Quaternion.identity);
            }else{
                // Move the existing player character to the chosen spawn point location
                StartCoroutine(MovePlayerToSpawnPosWithOffset(playerObject, spawnPoint));
                //MovePlayerToSpawnPos(playerObject, spawnPoint);
            }
        }else{
            Debug.LogError("No spawn points available.");
        }
    }

    IEnumerator MovePlayerToSpawnPosWithOffset(GameObject playerObject, Vector3 spawnPoint){
        //playerObject.SetActive(false);
        // Move the player to the spawn point with an initial offset
        playerObject.transform.position = new Vector3(spawnPoint.x, spawnPoint.y + 5, spawnPoint.z);
        Rigidbody rb = playerObject.GetComponent<Rigidbody>();
        if (rb != null){
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        }

        // Wait for 1 seconds before checking the player's y position
        yield return new WaitForSeconds(1f);

        // Check if the player's y position is less than 0
        if (playerObject.transform.position.y < 0){
            // Offset the spawn point and try again         
            float xOffset = UnityEngine.Random.Range(-15f, 15f);
            float zOffset = UnityEngine.Random.Range(-15f, 15f);
            spawnPoint.x += xOffset;
            spawnPoint.z += zOffset;
            yield return MovePlayerToSpawnPosWithOffset(playerObject, spawnPoint); // Recursively call the coroutine with the updated spawn point
        }else{
            // Player is successfully spawned at a suitable position
            //playerObject.SetActive(true);
        }
    }

    Vector3 GetRandomSpawnPoint(){
        // Choose a random index within the range of the SpawnPointList
        int randomIndex = UnityEngine.Random.Range(0, SpawnPointList.Count);

        // Retrieve the random spawn point from the list
        Vector2 spawnPoint2D = SpawnPointList[randomIndex];

        // Convert the 2D spawn point to a 3D position (assuming Y coordinate is the height of the terrain)
        Vector3 spawnPoint = new Vector3(spawnPoint2D.x, SampleHeightAtPoint(spawnPoint2D.x, spawnPoint2D.y), spawnPoint2D.y);

        return spawnPoint;
    }

    float SampleHeightAtPoint(float x, float z){
        RaycastHit hit;
        float terrainHeight = 0.0f;

        // Cast a ray downward from a point above the terrain
        Ray ray = new Ray(new Vector3(x, 1000f, z), Vector3.down);

        // Check if the ray intersects with the terrain
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Terrain")))
        {
            // If intersection detected, get the y-coordinate of the intersection point
            terrainHeight = hit.point.y;
        }

        return terrainHeight;
    }
    bool IsFlatArea(TerrainChunk chunk){
        // Calculate the heightmap for the chunk
        if(chunk.heightMapReceived){
            float[,] heights = chunk.heightMap.values;
            // Calculate the size of the spawn area
            int spawnAreaSize = Mathf.RoundToInt(meshSettings.meshWorldSize / 2);

            // Calculate the starting index for the spawn area
            int startX = heights.GetLength(0) / 2 - spawnAreaSize / 2;
            int startZ = heights.GetLength(1) / 2 - spawnAreaSize / 2;

            // Check if the area within the spawn area size is flat-ish
            float minHeight = float.MaxValue;
            float maxHeight = float.MinValue;

            for (int x = startX; x < startX + spawnAreaSize; x++){
                for (int z = startZ; z < startZ + spawnAreaSize; z++){
                    float height = heights[x, z];
                    minHeight = Mathf.Min(minHeight, height);
                    maxHeight = Mathf.Max(maxHeight, height);
                }
            }

            // Adjust the threshold according to your requirement
            float flatnessThreshold = 7f;
            //Debug.Log("Min Height: " + minHeight + ", Max Height: " + maxHeight); // Debug statement
            return (maxHeight - minHeight) < flatnessThreshold;
        }
            Debug.Log("Height map was not received");
            return false;
    }

}

[System.Serializable]
    public struct LODInfo{

        [Range(0,MeshSettings.numSupportedLODs -1)]
        public int lod;
        public float visibleDstThreshold;
        public float sqrVisibleDstThreshold{
            get{
                return visibleDstThreshold * visibleDstThreshold;
            }
        }
    }



