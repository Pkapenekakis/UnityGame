using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public GameObject InventoryMenu;
    private bool MenuActivated;  //keeps track if menu is open or not
    public ItemSlot[] itemSlot;
    public ItemSO[] itemSOs;
    private PlayerStats stats;
    private static InventoryManager instance;

    public static InventoryManager Instance
    {
        get
        {
            if (instance == null)
            {
                // Find the InventoryManager in the scene if it exists
                instance = FindObjectOfType<InventoryManager>();

                // If it doesn't exist, create a new GameObject and add the InventoryManager component
                if (instance == null)
                {
                    GameObject inventoryManager = new GameObject("InventoryManager");
                    instance = inventoryManager.AddComponent<InventoryManager>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Ensure this instance persists across scene loads
        }
        else
        {
            Destroy(gameObject); // Destroy any duplicate instances
        }
    }
    

    // Start is called before the first frame update
    void Start()
    {
        stats = GameObject.Find("PlayerCharacter").GetComponent<PlayerStats>();    
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Inventory") && MenuActivated){ //Inventory button is defined inside the project manager, Its default is tab
            Time.timeScale = 1;
            InventoryMenu.SetActive(false);
            MenuActivated = false;
            Utils.Instance.CursorManagement(true); //unlock the cursor
            DeselectAllSlots(); //Deselect the current item slot  

        }else if (Input.GetButtonDown("Inventory") && !MenuActivated){ //Inventory button is defined inside the project manager, Its default is tab
            Time.timeScale = 0; //Pause the game while opening the menu -- Can cause trouble with animated inventory
            InventoryMenu.SetActive(true);  
            MenuActivated = true;
            Utils.Instance.CursorManagement(false); //lock the cursor
        }

    }

    public bool useItem(string itemName){
        for(int i=0; i<itemSOs.Length;i++){
            if(itemSOs[i].itemName == itemName){
                bool usable = itemSOs[i].UseItem(stats);
                return usable;           
            }
        }
        return false;
    }

    public int AddItem(string itemName, int quant, Sprite itemSprite, string itemDescription, AudioSource audioSource){ 

        //loop through item slots until find an empty one and add the item there
        for(int i=0;i<itemSlot.Length;i++){
            if(itemSlot[i].isFull == false && itemSlot[i].itemName == itemName || itemSlot[i].quantity == 0){
                int leftOverItems = itemSlot[i].AddItem(itemName,quant,itemSprite, itemDescription,audioSource);
                if(leftOverItems > 0){ //if we have leftover items
                    leftOverItems = AddItem(itemName, leftOverItems, itemSprite, itemDescription,audioSource);//run add item again so we add the items on a different slot
                }
                return leftOverItems;
            }  
        }
        return quant;

    }

    //Iterates over invSlots and if we removed a key returns true else false
    public bool RemoveKeyItem(string itemName){
        for (int i = 0; i < itemSlot.Length; i++){ //loop through invSlots
            if(itemSlot[i].RemoveKeyItem(itemName)){ //if we removed a key
                return true;
            }
        }
        return false;
    }

    //Deselects the item slot in the inventory
    public void DeselectAllSlots(){
        for(int i =0;i<itemSlot.Length;i++){
            itemSlot[i].selectedShader.SetActive(false);
            itemSlot[i].isItemSelected = false;
        }
    }
}
