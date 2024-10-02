using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float maxHealth = 100;
    public float moveSpeed = 5f;

    private float minMoveSpeed = 0.5f;
    private float maxMoveSpeed = 10f;
    public float currentHealth;

    public HealthBarScript healthBar;
    public GameManager gameManager;
    private bool isDead;
    

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        //set health and healthbar
        currentHealth = maxHealth;
        healthBar.setMaxHealth(maxHealth);
        healthBar.setHealthBar(currentHealth);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Function handles player taking damage
    public void TakeDamage(float dmg){
        currentHealth -= dmg;

        if(currentHealth > 0){
            healthBar.setHealthBar(currentHealth);
        }else if(currentHealth == 0){
            Die();
        }else{ //if health falls below 0 set health to 0 for the healthbar and die
            currentHealth = 0;
            healthBar.setHealthBar(currentHealth);
            Die();
        }
    }
    
    //Function handles Healing the player
    public bool ReplenishHealth(float amount){
        if(currentHealth < maxHealth){
            float temp = currentHealth + amount;
            if(temp >= maxHealth){ //prevents health overflow
                currentHealth = maxHealth;
            }else{
                currentHealth = temp;
            }
            healthBar.setHealthBar(currentHealth);
            return true; //Used to check for item usage
        }  
        return false; //Used to check for item usage
    }

    //Function handles max Health increases. When maxHealth increases the player is healed by that amount too
    public bool IncreaseMaxHealth(float amount){
        maxHealth += amount;
        healthBar.setMaxHealth(maxHealth);
        ReplenishHealth(amount);
        return true; //used to check for item usage
    }

    public bool ChangeMovementSpeed(float amount){
        if(moveSpeed == maxMoveSpeed || moveSpeed == minMoveSpeed){
            return false; //used to check for item usage
        }
        if(moveSpeed + amount <= minMoveSpeed){ //movement speed cannot be decreased below a specific threshold
            moveSpeed = minMoveSpeed;
        }else if(moveSpeed + amount >= maxMoveSpeed){ //movement speed cannot be increased above a specific threshold
            moveSpeed = maxMoveSpeed;
        }else{
            moveSpeed += amount;
        }
        return true; //used to check for item usage
    }

    void Die(){
        if(!isDead){
            isDead = true; //makes sure this is called only once
            gameManager.GameOver();
        }
        
    }
}
