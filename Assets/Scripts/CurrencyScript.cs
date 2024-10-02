using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrencyScript : MonoBehaviour
{
    public int gold;
    GameObject currencyUI;
    // Start is called before the first frame update
    void Start()
    {
        currencyUI = GameObject.Find("Currency"); //The name of the gameobject  

        if (currencyUI == null)
        {
            Debug.LogError("Currency GameObject not found! Make sure it exists in the scene.");
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (currencyUI != null)
        {
            // Update the Text component with the player's gold amount
            if(currencyUI.GetComponent<TMPro.TextMeshProUGUI>().text != null){
                currencyUI.GetComponent<TMPro.TextMeshProUGUI>().text = "Gold: " + gold.ToString();
            }else{
                Debug.LogError("Cannot get text component");
            }
           
        }
        else
        {
            Debug.LogWarning("Currency GameObject is null. Unable to update UI.");
        }

        if(gold < 0){
            gold = 0;
        }    
    }
}
