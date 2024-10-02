using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public Camera firstPersonCamera;
    public Camera thirdPersonCamera;

    void Start()
    {
        // Ensure only one camera is enabled at the start
        firstPersonCamera.gameObject.SetActive(false);
        thirdPersonCamera.gameObject.SetActive(true);
    }

    void Update(){
        if (Input.GetKeyDown(KeyCode.C)){
            SwitchCamera();
        }
    }

    void SwitchCamera(){
        // Toggle the enabled state of both cameras2
        if(firstPersonCamera.isActiveAndEnabled){
            firstPersonCamera.gameObject.SetActive(false);
        }else{
            firstPersonCamera.gameObject.SetActive(true);
        }
        if(thirdPersonCamera.isActiveAndEnabled){
            thirdPersonCamera.gameObject.SetActive(false);
        }else{
            thirdPersonCamera.gameObject.SetActive(true);
        }
    }
}
