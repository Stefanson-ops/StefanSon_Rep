using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Controller : MonoBehaviour
{
    public float LookSpeed;
    public float LookXLimit;
    public Transform Body;
    public Transform CameraHolder;
    public Transform Shaker;
    public bool CanLook;
    public float HorizontalCamRotation = 0;
    public float VerticalCamRotation = 0;
    public float TiltMod;
    public float TiltAngle;
    public float TiltSpeed;
    float RotationZ;
    Character_Controller Controller;
    private void Start()
    {
        Controller = GetComponentInParent<Character_Controller>();
        Cursor.lockState = CursorLockMode.Locked;
        CanLook = true;
        
    }
    private void Update()
    {
        CameraShake();
        if (CanLook)
            BodyAndCameraRotation();
        CameraTilting();
    }
    public void ResetCameraRotation()
    {
        HorizontalCamRotation = -CameraHolder.eulerAngles.x;
        VerticalCamRotation = CameraHolder.eulerAngles.y;
    }
    void BodyAndCameraRotation()
    {
        float MouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * LookSpeed;
        float MouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * LookSpeed;

        HorizontalCamRotation += MouseY;
        VerticalCamRotation += MouseX;

        HorizontalCamRotation = Mathf.Clamp(HorizontalCamRotation, -LookXLimit, LookXLimit);

        CameraHolder.rotation = Quaternion.Euler(-HorizontalCamRotation, VerticalCamRotation, RotationZ);
        Body.rotation = Quaternion.Euler(0, VerticalCamRotation, 0);
    }
    public void LookAtPoint(Transform Point)
    {
        Quaternion rotTarget = Quaternion.LookRotation(Point.position - transform.position);
        CameraHolder.rotation = Quaternion.RotateTowards(CameraHolder.rotation, rotTarget, 500 * Time.deltaTime);
    }
    public void CameraTilting()
    {
        RotationZ = Mathf.Lerp(RotationZ, TiltAngle * TiltMod, TiltSpeed * Time.deltaTime);
    }
    void CameraShake()
    {
        if (Controller.rb.velocity.magnitude > 10)
        {


        }
        else
        {

        }
    }
}
