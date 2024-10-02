using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpScript : MonoBehaviour
{

    Animator anim;
    public bool gold;
    public int goldAmount;
    CurrencyScript currencyScript;
    private bool isPickingUp = false;
    public AudioSource audioSource;

    private static readonly int hashPickup = Animator.StringToHash("Pickup"); //converts the trigger to int for better perfomance 

    

    // Start is called before the first frame update
    void Start()
    {
        //anim = GameObject.FindWithTag("Player").GetComponent<Animator>();
        StartCoroutine("TryFindPlayer");
        currencyScript = GameObject.FindWithTag("GameController").GetComponent<CurrencyScript>(); 
        audioSource = GetComponent<AudioSource>(); 
    }

    //when we stay in the trigger area (Stay close to gold/item)
    void OnTriggerStay(Collider player)
    {
        if(player.tag == "Player" && !isPickingUp){ //check if we are not already picking up 
            if( gold && Input.GetKeyDown(KeyCode.F)){
                try{
                    audioSource.Play();
                }catch{
                    audioSource.GetComponent<AudioSource>();
                    audioSource.Play();
                }
                //audioSource.Play(); //Due to how coroutines interact with unity it cannot be inside a coroutine              
                StartCoroutine("PickupAnim");
            }
        }    
    }

    IEnumerator PickupAnim(){
        isPickingUp = true; //set the flag to true while picking up 
        anim.SetTrigger(hashPickup);
        yield return new WaitForSeconds(0.6f);
        if(gold){
            currencyScript.gold += goldAmount;
            gameObject.SetActive(false);
            Destroy(this.gameObject, audioSource.clip.length + 1);
        }  
        isPickingUp = false; //Reset the flag
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

}
