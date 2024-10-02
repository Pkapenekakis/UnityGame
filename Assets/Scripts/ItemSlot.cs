using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
public class ItemSlot : MonoBehaviour, IPointerClickHandler
{
    private InventoryManager invManager;
    //TODO MAKE PRIVATE AFTER TESTING IS DONE
    //============Item Data ================// 
    public String itemName; 
    public int quantity;
    public Sprite itemSprite;
    public bool isFull; //check if item slot contains an item
    public String itemDescription;
    public Sprite emptySprite;
    

    [SerializeField] private int maxNumberOfItems;

    //============Item Slot Data ================//
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Image itemImage;
    [SerializeField] private AudioSource audioSource;
    //[SerializeField] private AudioSource audioSource;
    public GameObject selectedShader;
    public bool isItemSelected; // if the item is selected

    //============Item Description Slot ================//
    public Image itemDescriptionImage;
    public TMP_Text ItemDescriptionNameText;
    public TMP_Text ItemDescriptionText;
    

    private void Start(){
        invManager = InventoryManager.Instance; //GameObject.Find("InventoryManager").GetComponent<InventoryManager>();
        itemDescriptionImage.sprite = emptySprite;
    }

    public int AddItem(string itemName, int quant, Sprite itemSprite,string itemDescription, AudioSource audioSource){
        //check to see if the slot is full
        if(isFull){
            return quant;
        }

        //Update the Slot's data
        this.itemName = itemName;
        this.itemSprite = itemSprite;
        this.itemDescription = itemDescription;
        if(audioSource != null){
            this.audioSource.clip = audioSource.clip;
        }
        

        itemImage.sprite = itemSprite;
        itemImage.enabled = true;


        //Update Quantity of items
        this.quantity += quant;
        if(this.quantity >= maxNumberOfItems){
            quantityText.text = maxNumberOfItems.ToString();
            quantityText.enabled = true;
            isFull = true;
        
            //Return Leftover items
            int extraItems = this.quantity - maxNumberOfItems;
            this.quantity = maxNumberOfItems;
            return extraItems;    
        }

        //Update Quantity Text
        quantityText.text = this.quantity.ToString();
        quantityText.enabled = true;

        return 0;     

    }

    public bool RemoveKeyItem(string itemName){
        if(Item.GetItemType(itemName) == Item.ItemType.Key){ //check if the item is a key
            this.quantity -=1;
            quantityText.text = this.quantity.ToString(); //update the quantity
            if(this.quantity <= 0){
                EmptySlot();
            }
            return true;
        }
        return false;
    }



    //Called when the item is clicked
    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left){
            OnLeftClick();
        }
        if(eventData.button == PointerEventData.InputButton.Right){
            OnRightClick();    
        }
    }

    //Function Deselects the other selected item(s) and selects the new one
    public void OnLeftClick(){
        if(isItemSelected){
            bool usable = invManager.useItem(itemName); //if the item is usable returns true else it returns false
            if(usable){
                this.audioSource.Play(); //Due to how coroutines interact with unity it cannot be inside a coroutine   
                this.quantity -=1;
                quantityText.text = this.quantity.ToString(); //update the quantity
                if(this.quantity <= 0){
                    EmptySlot();
                }
            }         
        }else{
            invManager.DeselectAllSlots();
            selectedShader.SetActive(true);
            isItemSelected = true;
            ItemDescriptionNameText.text = itemName;
            ItemDescriptionText.text = itemDescription;
            itemDescriptionImage.sprite = itemSprite;
            if(itemDescriptionImage.sprite == null){
                itemDescriptionImage.sprite = emptySprite;
            }
        }
    }

    //Empties the itemSlot if the items are used
    private void EmptySlot()
    {
        quantityText.enabled= false;
        itemImage.sprite = emptySprite;

        itemDescription = "";
        itemName = "";
        ItemDescriptionNameText.text = itemName;
        ItemDescriptionText.text = itemDescription;
        itemDescriptionImage.sprite = emptySprite;
        itemSprite = emptySprite;
        isFull = false;
        //audioSource.clip = null; 

    }

    public void OnRightClick(){
        String path= Item.GetItemPrefabPath(Item.GetItemType(itemName)) + itemName;
        GameObject itemPrefab = Resources.Load<GameObject>(path);

        // Check if the prefab was found
        if (itemPrefab != null)
        {
            // Instantiate the item prefab
            GameObject itemToDrop = Instantiate(itemPrefab);

            // Set item properties (if needed)
            Item newItem = itemToDrop.GetComponent<Item>();
            if (newItem != null)
            {
                newItem.quantity = 1;
                newItem.itemName = itemName;
                newItem.itemDescription = itemDescription;
                newItem.audioSource = itemToDrop.GetComponent<AudioSource>();
            }
/*
            // Set the location in front of the player
            Transform playerTransform = GameObject.FindWithTag("Player").transform;
            Vector3 dropPosition = playerTransform.position + playerTransform.forward * 2f; // Adjust the distance as needed */
            
            Vector3 playerPosition = GameObject.FindWithTag("Player").transform.position;

            // Get the player's forward direction
            Vector3 playerForward = GameObject.FindWithTag("Player").transform.forward;

            // Define the offset distance in front of the player
            float dropDistance = 2f; 

            // Define the vertical offset above the ground
            float verticalOffset = 0.2f; 

            // Cast a ray downward to find the ground level
            RaycastHit hit;
            if (Physics.Raycast(playerPosition, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
            {
                // If the ray hits the ground, add the vertical offset to the hit point
                playerPosition = hit.point + Vector3.up * verticalOffset;
            }
            else
            {
                // If the ray doesn't hit anything, simply add the vertical offset to the player's position
                playerPosition += Vector3.up * verticalOffset;
            }

            // Calculate the drop position by adding the offset to the player's position
            Vector3 dropPosition = playerPosition + playerForward * dropDistance;

            //the final Drop Position
            itemToDrop.transform.position = dropPosition;

             // Add a collider based on the prefab's collider type
            Collider prefabCollider = itemPrefab.GetComponent<Collider>();
            Collider newCollider = null;
            if (prefabCollider != null){
                if(!prefabCollider.isTrigger){//check if the item already has a trigger collider
                    if (prefabCollider is SphereCollider){
                        newCollider = itemToDrop.AddComponent<SphereCollider>();
                    }else if (prefabCollider is CapsuleCollider){
                        newCollider = itemToDrop.AddComponent<CapsuleCollider>();
                    }

                if(newCollider != null)
                    newCollider.isTrigger = true;
                    Utils.Instance.CopyColliderProperties(prefabCollider, newCollider);
                }
            }


            this.quantity -=1;
            quantityText.text = this.quantity.ToString(); //update the quantity
            if(this.quantity <= 0){
                EmptySlot();
            }
        }else{
            Debug.Log("Item prefab not Found: " + path);        
        }
    }

}
