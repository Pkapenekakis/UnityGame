using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderController : MonoBehaviour
{

    public static String groundTag = "Terrain";
    private void OnTriggerEnter(Collider collider)
    {
        // Check if the Object collided with the ground
        if (collider.CompareTag("Terrain"))
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.useGravity = false;
            }
        }
    }

    
}
