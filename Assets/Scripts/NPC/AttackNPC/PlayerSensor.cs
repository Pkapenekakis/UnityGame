using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class PlayerSensor : MonoBehaviour
{
    public delegate void PlayerEnterEvent(Transform player);
    public delegate void PlayerExitEvent(Vector3 lastKnownPos);
    public event PlayerEnterEvent OnPlayerEnter;
    public event PlayerExitEvent OnPlayerExit;

    private void OnTriggerEnter(Collider other){
        if(other.CompareTag("Player")){ //other.CompareTag("Player")

            //Debug.Log("Player entered the sensor");
            OnPlayerEnter?.Invoke(other.transform);
        }else{
            //Debug.Log("Player not found in sensor");
        }
    }

    private void OnTriggerExit(Collider other){
        if(other.CompareTag("Player")){ //other.TryGetComponent(out Player player)
            //Debug.Log("Player exited the sensor");
            OnPlayerExit?.Invoke(other.transform.position);
        }
    }
}
