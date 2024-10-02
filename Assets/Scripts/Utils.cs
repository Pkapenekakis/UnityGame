using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Singleton Pattern is used to ensure only one instance of the script is used
//This ensures easy access to utility functions without the need for multiple instances and helps avoid initialization issues.
//In order to call the Functions inside the script the format is Utils.Instance.*FunctionName*
public class Utils : MonoBehaviour
{

    private static Utils instance;
    private bool lockCursor; //if true it is used to hide the cursor
    public AudioClip pickupSoundClip; // Reference to the AudioClip asset in the Inspector


    public static Utils Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<Utils>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("Utils");
                    instance = obj.AddComponent<Utils>();
                }
            }
            return instance;
        }
    }

    //True for locked cursor, false for free cursor
    public void CursorManagement(bool lockC){
        lockCursor = lockC;
        if(lockCursor){
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }else{
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;    
        }  
    }

    public void PlayPickupSound()
    {
        // Play the sound clip at the current position
        AudioSource.PlayClipAtPoint(pickupSoundClip, Camera.main.transform.position);
    }

    public void CopyColliderProperties(Collider source, Collider destination)
    {
        if (source.GetType() != destination.GetType())
        {
            Debug.LogWarning("Colliders are not of the same type, cannot copy properties.");
            return;
        }

        //Copy common collider properties
        destination.isTrigger = source.isTrigger;
        destination.sharedMaterial = source.sharedMaterial;

        //Copy specific properties based on collider type
        if (source is SphereCollider sourceSphere && destination is SphereCollider destSphere)
        {
            destSphere.center = sourceSphere.center;
            destSphere.radius = sourceSphere.radius;
        }
        else if (source is CapsuleCollider sourceCapsule && destination is CapsuleCollider destCapsule)
        {
            destCapsule.center = sourceCapsule.center;
            destCapsule.radius = sourceCapsule.radius;
            destCapsule.height = sourceCapsule.height;
            destCapsule.direction = sourceCapsule.direction;
        }
        /*
        else if (source is BoxCollider sourceBox && destination is BoxCollider destBox)
        {
            destBox.center = sourceBox.center;
            destBox.size = sourceBox.size;
        }
        else if (source is MeshCollider sourceMesh && destination is MeshCollider destMesh)
        {
            destMesh.sharedMesh = sourceMesh.sharedMesh;
            destMesh.convex = sourceMesh.convex;
            destMesh.cookingOptions = sourceMesh.cookingOptions;
        } */
    }
}
