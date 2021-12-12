using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 };
    public RotationAxes axes = RotationAxes.MouseXAndY;
    public float sensitivityX = 15F;
    public float sensitivityY = 15F;
    public float minimumX = -360F;
    public float maximumX = 360F;
    public float minimumY = -60F;
    public float maximumY = 60F;
    float rotationX = 0F;
    float rotationY = 0F;
    public Vector3 targetPos;
    Quaternion originalRotation;
    Rigidbody rb;
    private Vector3 moveDirection;

    WorldVariables worldvars;
    int globalx;
    int globalz;
    float globalheight;
    int worldsize;


    void Update()
    {
        //Button and input handling        
        if (Input.GetKey(KeyCode.W))
        {
            rb.AddRelativeForce(Vector3.forward * 8, ForceMode.Force);
        }

        if (Input.GetKey(KeyCode.S))
        {
            rb.AddRelativeForce(-Vector3.forward * 8, ForceMode.Force);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddRelativeForce(Vector3.up, ForceMode.Impulse);
        }

        //world limit handling
        if (this.gameObject.transform.position.x <= 0 || this.gameObject.transform.position.x >= worldsize * globalx)
        {
            worldvars.centercamera();
            Debug.Log("hit x limit");
        }
        if (this.gameObject.transform.position.z <= 0 || this.gameObject.transform.position.z >= worldsize * globalx)
        {
            worldvars.centercamera();
            Debug.Log("hit z limit");
        }
        if (this.gameObject.transform.position.y <= -10 || this.gameObject.transform.position.y >= globalheight)
        {
            worldvars.centercamera();
            Debug.Log("Calm down icarus... hit y");
        }


        //mouselook handling
        if (axes == RotationAxes.MouseXAndY)
        {
            // Read the mouse input axis
            rotationX += Input.GetAxis("Mouse X") * sensitivityX;
            rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
            rotationX = ClampAngle(rotationX, minimumX, maximumX);
            rotationY = ClampAngle(rotationY, minimumY, maximumY);
            Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
            Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, -Vector3.right);
            transform.rotation = originalRotation * xQuaternion * yQuaternion;
        }
        else if (axes == RotationAxes.MouseX)
        {
            rotationX += Input.GetAxis("Mouse X") * sensitivityX;
            rotationX = ClampAngle(rotationX, minimumX, maximumX);
            Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
            transform.rotation = originalRotation * xQuaternion;
        }
        else
        {
            rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
            rotationY = ClampAngle(rotationY, minimumY, maximumY);

            Quaternion yQuaternion = Quaternion.AngleAxis(-rotationY, Vector3.right);
            transform.rotation = originalRotation * yQuaternion;
        }
    }

    void Start()
    {
        worldvars = GameObject.FindGameObjectWithTag("WorldController").GetComponent<WorldVariables>();
        globalx = worldvars.meshx;
        globalz = worldvars.meshz;
        globalheight = worldvars.globalheight;
        worldsize = worldvars.worldsize;

        // Make the rigid body not change rotation
        targetPos = transform.position;
        originalRotation = transform.localRotation;
        rb = GetComponent<Rigidbody>();
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}
