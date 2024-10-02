using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class AreaTerrainBaker : MonoBehaviour
{
    [SerializeField] private NavMeshSurface surface;
    GameObject playerObject;
    private float updateRate = 0.1f;
    [SerializeField] private float MovementThreshold = 10f; //how much the player moves without baking again
    [SerializeField] private Vector3 navMeshSize = new Vector3(20,20,20); 
    private Vector3 worldAnchor; //Last position we baked
    private NavMeshData navMeshData;
    private List<NavMeshBuildSource> Sources = new List<NavMeshBuildSource>();

    private void Start() {
        playerObject = GameObject.FindGameObjectWithTag("Player");
        navMeshData = new NavMeshData();
        NavMesh.AddNavMeshData(navMeshData);
        BuildNavMesh(false);
        StartCoroutine(CheckPlayerMovement());   
    }

    private IEnumerator CheckPlayerMovement(){
        WaitForSeconds wait = new WaitForSeconds(updateRate);

        while (true)
        {
            if(Vector3.Distance(worldAnchor, playerObject.transform.position) > MovementThreshold){
                BuildNavMesh(true); 
                worldAnchor = playerObject.transform.position;
            }
            yield return wait;
        }
    }

    private void BuildNavMesh(bool Async){
        Bounds navMeshBounds = new Bounds(playerObject.transform.position, navMeshSize);
        List<NavMeshBuildMarkup> markups = new List<NavMeshBuildMarkup>();
        List<NavMeshModifier> modifiers;

        if(surface.collectObjects == CollectObjects.Children){
            modifiers =  new List<NavMeshModifier>(surface.GetComponentsInChildren<NavMeshModifier>());
        }else{
            modifiers = NavMeshModifier.activeModifiers;
        }

        for (int i = 0; i < modifiers.Count; i++)
        {
           if( (surface.layerMask & (1 << modifiers[i].gameObject.layer)) == 1 && modifiers[i].AffectsAgentType(surface.agentTypeID)){
                markups.Add(new NavMeshBuildMarkup(){ //Data structure of the Navmesh
                    root = modifiers[i].transform,
                    overrideArea = modifiers[i].overrideArea,
                    area = modifiers[i].area,
                    ignoreFromBuild = modifiers[i].ignoreFromBuild
                });
           }
        }

        if(surface.collectObjects == CollectObjects.Children){
            //Populates the sources list
            NavMeshBuilder.CollectSources(surface.transform, surface.layerMask, surface.useGeometry, surface.defaultArea, markups, Sources);
        }else{
            NavMeshBuilder.CollectSources(navMeshBounds, surface.layerMask, surface.useGeometry, surface.defaultArea, markups, Sources);
        }

        Sources.RemoveAll(source => source.component != null && source.component.gameObject.GetComponent<NavMeshAgent>() != null); //fixes playermovement bug
        
        if(Async){
            NavMeshBuilder.UpdateNavMeshDataAsync(navMeshData, surface.GetBuildSettings(), Sources, new Bounds(playerObject.transform.position, navMeshSize));
        }else{
            NavMeshBuilder.UpdateNavMeshData(navMeshData, surface.GetBuildSettings(), Sources, new Bounds(playerObject.transform.position, navMeshSize));
        }

    }

}
