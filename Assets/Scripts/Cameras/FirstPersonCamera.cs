using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    float yaw; //X axis
    float pitch; //Y axis
    [SerializeField] public float mouseSensitivity = 5;
    public Vector2 pitchMinMax = new Vector2(-50,55);
    public float rotationSmoothTime = 0.12f; //Changing to lower values makes camera a lot more responsive
    Vector3 rotationSmoothVelocity;
    Vector3 currentRotation;

    // Start is called before the first frame update
    void Start()
    {
        //We hide the cursor and lock it to the character
        Utils.Instance.CursorManagement(true);  
    }

    // Update is called once per frame
    void LateUpdate()
    {
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y")* mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y); //applies the min-max Values

        //Used to smooth the camera movement
        currentRotation = Vector3.SmoothDamp(currentRotation,new Vector3(pitch, yaw),ref rotationSmoothVelocity, rotationSmoothTime);
        transform.eulerAngles = currentRotation;

    }
}
