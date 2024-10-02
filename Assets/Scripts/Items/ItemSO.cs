using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ItemSO : ScriptableObject
{
    public string itemName;
    public float amountToChange;
    public StatToChange stc = new StatToChange();
    
    

    public bool UseItem(PlayerStats stats){
        if(stc == StatToChange.health){
            if(stats.ReplenishHealth(amountToChange)){
                return true;
            }            
            return false;  
        }
        if(stc == StatToChange.speed){
            if(stats.ChangeMovementSpeed(amountToChange)){  
                return true;
            }
            return false;     
        }
        if(stc == StatToChange.maxHealth){
            if(stats.IncreaseMaxHealth(amountToChange)){  
                return true;
            }
            return false;     
        }

        return false;
    }
    public enum StatToChange{
        none,
        health,
        maxHealth,
        speed
    };    
}
