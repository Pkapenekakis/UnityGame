using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;


public class Item : MonoBehaviour
{
    [SerializeField] public string itemName;
    [SerializeField] public int quantity;
    [SerializeField] public Sprite sprite;

    [TextArea] //gives a box to type the item description
    [SerializeField] public string itemDescription;
    public AudioSource audioSource;
    //public ItemSO itemData;

    private InventoryManager inventoryManager;
    Animator anim;
    private static readonly int hashPickup = Animator.StringToHash("Pickup"); //converts the trigger to int for better perfomance 
    private bool isPickingUp = false;
    public bool item;

    // Start is called before the first frame update
    void Start()
    {
        inventoryManager = InventoryManager.Instance; //GameObject.Find("InventoryManager").GetComponent<InventoryManager>();
        audioSource = GetComponent<AudioSource>(); 
        StartCoroutine("TryFindPlayer");
    }

    private void OnTriggerStay(Collider player) {
        if(player.tag == "Player" && !isPickingUp){ //check if we are not already picking up 
            if( item && Input.GetKeyDown(KeyCode.F)){
                Utils.Instance.PlayPickupSound();
                String path= GetItemPrefabPath(GetItemType(itemName)) + itemName;
                audioSource = Resources.Load<AudioSource>(path);  
                StartCoroutine("PickupAnim", audioSource);
            }
        }      
    }

     IEnumerator TryFindPlayer(){
        while (anim == null){
            try{
                anim = GameObject.FindWithTag("Player").GetComponent<Animator>();
            }catch{
            }
            if (anim == null){
                yield return new WaitForSeconds(1);
            }
        }
    }

    IEnumerator PickupAnim(AudioSource audioSource){
        isPickingUp = true; //set the flag to true while picking up 
        anim.SetTrigger(hashPickup);
        yield return new WaitForSeconds(0.6f);
        if(item){
            int leftOverItems = inventoryManager.AddItem(itemName,quantity,sprite, itemDescription,audioSource);
            if(leftOverItems <= 0){
                Destroy(this.gameObject);
            }else{
                quantity = leftOverItems;    
            }
           
        }  
        isPickingUp = false; //Reset the flag
    }

    public static int GetCost(ItemType itemType){
        switch(itemType){
            default:
            case ItemType.HealthBooster: return 10;
            case ItemType.MaxHealthBooster: return 10;
            case ItemType.SpeedBooster: return 10;
            case ItemType.Key: return 30;
        }
    }

    
    public static Sprite GetSprite(ItemType itemType){
        string spriteName = "";

        switch(itemType){
            default:
            case ItemType.HealthBooster: 
                spriteName = "HealthBooster" ;
                break;
            case ItemType.MaxHealthBooster:
                spriteName = "MaxHealthBooster" ;
                break;
            case ItemType.SpeedBooster:
                spriteName = "SpeedBooster" ;
                break;
            case ItemType.Key:
                spriteName = "MagicKey" ;
                break;
        }

        return Resources.Load<Sprite>("Sprites/" + spriteName);
    }

    public static AudioSource GetAudioSource(ItemType itemType){
        string itemName = "";
        AudioSource audioSource;

        String path= GetItemPrefabPath(itemType) + itemName;
        try{
            audioSource = Resources.Load<AudioSource>(path);
            return audioSource;
        }catch{
            Debug.Log("Item: " + itemName + "has no Audio Source");
        }
        return null;  
    }

    public static String GetItemDescription(ItemType itemType){
        string itemDesc = "";

        switch(itemType){
            default:
            case ItemType.HealthBooster: 
                itemDesc = "Heals you for 5 hp.";
                break;
            case ItemType.MaxHealthBooster:
                itemDesc = "Increases MaxHealth by 7. Heals for the same amount.";
                break;
            case ItemType.SpeedBooster:
                itemDesc = "Increases Speed by 0.2";
                break;
            case ItemType.Key:
                itemDesc = "A key used to unlock the magic pillars";
                break;
        }
        return itemDesc;  
    }

    public static String GetItemPrefabPath(ItemType itemType){
        String prefabPath;
        switch(itemType){
            default:
            case ItemType.HealthBooster: 
                prefabPath = "Prefabs/Potions/";
                break;
            case ItemType.MaxHealthBooster:
                prefabPath = "Prefabs/Potions/";
                break;
            case ItemType.SpeedBooster:
                prefabPath = "Prefabs/Potions/";
                break;
            case ItemType.Key:
                prefabPath = "Prefabs/";
                break;
        }
        return prefabPath;  
    }

    public static ItemType GetItemType(String name){
        ItemType itemType;
        switch(name){
            default:
            case "HealthBooster": 
                itemType = ItemType.HealthBooster;
                break;
            case "MaxHealthBooster": 
                itemType = ItemType.MaxHealthBooster;
                break;
            case "SpeedBooster": 
                itemType = ItemType.SpeedBooster;
                break;
            case "MagicKey": 
                itemType = ItemType.Key;
                break;
        }
        return itemType;
    }

    public enum ItemType{
        HealthBooster,
        MaxHealthBooster,
        SpeedBooster,
        Key
    }

}


 