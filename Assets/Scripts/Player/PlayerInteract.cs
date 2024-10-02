using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour{   
    public static float interactRange = 2f; 
    void Update(){
        if(Input.GetKeyDown(KeyCode.E)){
            IInteractable interactable = GetInteractableObject();
            if(interactable != null){
                interactable.Interact();
            }
        }
    }

    public IInteractable GetInteractableObject(){
        List<IInteractable> interactableList = new List<IInteractable>();
        Collider[] colliderArray = Physics.OverlapSphere(transform.position, interactRange);
        //Go through every collider available
        foreach (Collider collider in colliderArray){
            if(collider.TryGetComponent(out IInteractable interactable)){
                interactableList.Add(interactable);
            }
        }

        //Get the closest NPC to the player
        IInteractable closestInteractable = null;
        foreach (IInteractable interactable in interactableList){
            if(closestInteractable == null){ //if its the first one it is the closest
                closestInteractable = interactable;
            }else{
                //Check if it needs to be optimized with .sqrMagnitude
                if(Vector3.Distance(transform.position, interactable.GetTransform().position) < Vector3.Distance(transform.position, closestInteractable.GetTransform().position)){
                    closestInteractable = interactable;
                }
            }
        }
        return closestInteractable;
    }

}
