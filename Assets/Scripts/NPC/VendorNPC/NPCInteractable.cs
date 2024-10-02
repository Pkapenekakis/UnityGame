using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactText = "Talk";
    public void Interact(){
        if(!ShopUI.Instance.IsShopEnabled()){
            ShopUI.Instance.EnableShop();
        }  
    }

    // Update is called once per frame
    void Update()
    {      
        if( ShopUI.Instance.IsShopEnabled() && (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Escape) ) ){ //CANNOT use the key that is used to Interact
            ShopUI.Instance.DisableShop();
        }
    }

    public string GetInteractText(){
        return interactText;
    }

    public Transform GetTransform()
    {
       return transform;
    }
}
