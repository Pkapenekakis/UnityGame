using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour{
    private static ShopUI instance;
    private Transform container;
    private Transform shopItemTemplate;
    private bool shopEnabled = false;
    private InventoryManager inventoryManager;
    private CurrencyScript currencyScript;

    public static ShopUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ShopUI>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("ShopUI");
                    instance = obj.AddComponent<ShopUI>();
                }
            }
            return instance;
        }
    }

    private void Awake() {
        container = transform.Find("ShopContainer");
        shopItemTemplate = container.Find("ShopItemTemplate");
        shopItemTemplate.gameObject.SetActive(false);
    }

    private void Start() {
        currencyScript = GameObject.FindWithTag("GameController").GetComponent<CurrencyScript>();
        inventoryManager = InventoryManager.Instance; //used for purchasing items
        CreateItemButton(Item.ItemType.HealthBooster, Item.GetSprite(Item.ItemType.HealthBooster), "Health Booster", Item.GetCost(Item.ItemType.HealthBooster), 0);
        CreateItemButton(Item.ItemType.SpeedBooster, Item.GetSprite(Item.ItemType.SpeedBooster), "Speed Booster", Item.GetCost(Item.ItemType.SpeedBooster), 1);
        CreateItemButton(Item.ItemType.Key, Item.GetSprite(Item.ItemType.Key), "Magic Key", Item.GetCost(Item.ItemType.Key), 2);
    }

    public void EnableShop(){
        Time.timeScale = 0; //Pause the game while opening the menu
        shopEnabled = true;
        Utils.Instance.CursorManagement(false); //lock the cursor
        foreach (Transform shopItem in container)
        {
            if (shopItem.name != "ShopItemTemplate"){
                shopItem.gameObject.SetActive(true);
            }
        }
    }

    public void DisableShop(){
        shopEnabled = false;
        Time.timeScale = 1; //GameContinues
        Utils.Instance.CursorManagement(true); //unlock the cursor
        foreach (Transform shopItem in container)
        {
            if (shopItem.name != "ShopItemTemplate")
            {
                shopItem.gameObject.SetActive(false);
            }
        }
          
    }

    private void CreateItemButton(Item.ItemType itemType, Sprite itemSprite, string itemName, int itemCost, int posIndex){

        Transform shopItemTransform = Instantiate(shopItemTemplate, container);
        RectTransform shopItemRectTransform = shopItemTransform.GetComponent<RectTransform>();
        float shopItemHeight = 30f;
        shopItemRectTransform.anchoredPosition = new Vector2(0, -shopItemHeight * posIndex*2);

        shopItemTransform.name = itemName;

        shopItemTransform.Find("ItemName").GetComponent<TextMeshProUGUI>().SetText(itemName);
        shopItemTransform.Find("ItemPrice").GetComponent<TextMeshProUGUI>().SetText(itemCost.ToString());
        shopItemTransform.Find("ItemIcon").GetComponent<Image>().sprite = itemSprite;

        Transform backgroundTransform = shopItemTransform.Find("Background");
        UnityEngine.UI.Button button = backgroundTransform.Find("Button").GetComponent<Button>(); //NEEDS TO BE UnityEngine.UI.Button OR ELSE UNITY BREAKS
        if (button != null){
            button.onClick.AddListener(() => OnShopItemClicked(itemType,itemName, itemCost));
        }
    }

    private void OnShopItemClicked(Item.ItemType itemType, string itemName, int itemCost){
        TryBuyItem(itemType, itemName);
    }

    private void TryBuyItem(Item.ItemType itemType,string itemName){
        int itemCost = Item.GetCost(itemType);
        int playerGold = currencyScript.gold; //Get the player Gold
        if(playerGold >= itemCost){ //if the player can afford that
            String addName = itemName.Replace(" ", "");
            inventoryManager.AddItem(addName,1,Item.GetSprite(itemType),Item.GetItemDescription(itemType),Item.GetAudioSource(itemType)); //string itemName, int quant, Sprite itemSprite, string itemDescription, AudioSource audioSource)
            currencyScript.gold -= itemCost;
        }else{
            Debug.Log("Not Enough Gold!");
        }
        
    }

    public bool IsShopEnabled(){
        return shopEnabled;
    }
}
