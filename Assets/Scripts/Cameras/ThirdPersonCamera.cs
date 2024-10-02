using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    float yaw; //X axis
    float pitch; //Y axis
    [SerializeField] public float mouseSensitivity = 5;
    public Transform target; // the target which the camera rotates around
    [SerializeField] public float distanceFromTarget = 3; //Camera distance from the target
    public Vector2 pitchMinMax = new Vector2(-17,85); //These values need to be changed if distanceFromTarget is changed
    public float rotationSmoothTime = 0.12f; //Changing to lower values makes camera a lot more responsive
    Vector3 rotationSmoothVelocity;
    Vector3 currentRotation;
    

    void Start()
    {
        //We hide the cursor and lock it to the character
        Utils.Instance.CursorManagement(true);  
    }

    //LateUpdate is called after all the other update methods
    //The advantage of that is that the target.position will have been set so we set camera to the correct location
    void LateUpdate()
    {
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y")* mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y); //applies the min-max Values

        //Used to smooth the camera movement
        currentRotation = Vector3.SmoothDamp(currentRotation,new Vector3(pitch, yaw),ref rotationSmoothVelocity, rotationSmoothTime);
        transform.eulerAngles = currentRotation;

        transform.position = target.position - transform.forward * distanceFromTarget;

    }
}
