using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    private PlayerStats playerStats;
    [SerializeField] float damage = 10f;

    void Start()
    {
        StartCoroutine("TryFindPlayer");    
        
    }
    
    private void OnTriggerEnter(Collider other){
        // Check if the collided object is the player
        if (other.CompareTag("Player")){
            StartCoroutine(ApplyDamageAfterDelay());
        }
    }

    private IEnumerator ApplyDamageAfterDelay()
    {
        // Wait for the duration of the attack animation
        yield return new WaitForSeconds(1.2f);

        // If the player has a PlayerStats component, reduce its health
        if (playerStats != null)
        {
            playerStats.TakeDamage(damage);
        }
    }

    IEnumerator TryFindPlayer(){
        while (playerStats == null){
            try{
                playerStats = GameObject.FindWithTag("Player").GetComponent<PlayerStats>();
            }catch{
            }
            if (playerStats == null){
                yield return new WaitForSeconds(1);
            }
        }
    }
}
