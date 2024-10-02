using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{

    public float maxHealth = 100;
    public float moveSpeed = 3.5f;

    public float currentHealth;

    public bool isDead = false;

    // Start is called before the first frame update
    void Start()
    {
        //set health and healthbar
        currentHealth = maxHealth;   
    }

    public void TakeDamage(float dmg){
        currentHealth -= dmg;

        if(currentHealth > 0){
            //healthBar.setHealthBar(currentHealth);
        }else if(currentHealth == 0){
            Die();
        }else{ //if health falls below 0 set health to 0 for the healthbar and die
            currentHealth = 0;
            //healthBar.setHealthBar(currentHealth);
            Die();
        }
    }

    void Die(){
        isDead = true;
        //Handle death
        Debug.Log("NPC died!");
    }
}
