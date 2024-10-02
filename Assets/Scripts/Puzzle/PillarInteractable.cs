using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class PillarInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactText = "Insert Key";
    [SerializeField] private string keyName = "MagicKey"; //Placeholder. In the feature accept specific key types/names
    float timer;
    float useKeyCooldown = 3f;

    // Start is called before the first frame update
    void Start(){
        
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
    }

    public void Interact()
    {
        if(UseKey()){
            UnlockPillar(); 
            Debug.Log("Key Used");
        }
    }

    public bool UseKey(){
        if(timer >= useKeyCooldown){
            if(InventoryManager.Instance.RemoveKeyItem(keyName)){ //try to use a key from Inventory
                timer = 0;
                return true;
            }  
        }else{
            //Popup that key cannot be used
            Debug.Log("Key Cooldown is not Over!");
        }
        return false;
    }

    public void UnlockPillar(){
        string pillarName = gameObject.name;

        if(pillarName.Contains("YellowPillar")){
            GameManager.Instance.setYellowPillar();
        }else if(pillarName.Contains("BluePillar")){
            GameManager.Instance.setBluePillar();
        }else if(pillarName.Contains("GreenPillar")){
            GameManager.Instance.setGreenPillar();
        }
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public string GetInteractText()
    {
        return interactText;
    }
}
