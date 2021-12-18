using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FollowCamera : MonoBehaviour
{
    [SerializeField] Transform target;
    public Vector3 offset;
    Vector3 localOffset;
    float moveSpeed = 10f;
    [SerializeField] float height = 5f;
    [SerializeField] float distance = 10;
    Vector3 wantedPos;
    Quaternion wantedRot;
    (float Height, float Distance) lowAngle = (12f, 44f);
    (float Height, float Distance) normalAngle = (7f, 10f);
    (float Height, float Distance) angle
    {
        get
        {
            if (isNormalAngle)
            {
                return normalAngle;
            }
            else
            {
                return lowAngle;
            }
        }
    }
    bool isNormalAngle = true;
    void LateUpdate()
    {
        wantedPos = target.TransformPoint(0, angle.Height, -angle.Distance);
        transform.position = Vector3.Lerp(transform.position, wantedPos, 5f * Time.deltaTime);

        Quaternion temp = transform.rotation;
        wantedRot = Quaternion.LookRotation((target.TransformPoint(offset)) - transform.position);

        transform.rotation = Quaternion.Slerp(transform.rotation, wantedRot, 5f * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Y))
        {
            if (isNormalAngle)
            {
                isNormalAngle = false;
            }
            else
            {
                isNormalAngle = true;
            }
        }
    }
}


public class TankController : MonoBehaviour
{
    public FollowCamera followCamera;
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float rotateSpeed = 50f;
    float xAxis;
    float zAxis;

    public Transform head;
    public Transform cannon;
    public Transform firePivot;
    public GameObject shell;
    // Start is called before the first frame update
    void Start()
    {
        cameraOffsetY = followCamera.offset.y;
        cameraOffsetX = followCamera.offset.x;
    }

    // Update is called once per frame
    void Update()
    {
        xAxis = Input.GetAxis("Horizontal");
        zAxis = Input.GetAxis("Vertical");

        transform.Translate(transform.forward * moveSpeed * zAxis * Time.deltaTime, Space.World);
        transform.Rotate(0, rotateSpeed * xAxis * Time.deltaTime, 0);
        RotateHead();
        RotateCannon();
        Shot();
    }

    float headRotationY;
    void RotateHead()
    {
        if (Input.GetKey(KeyCode.J))
        {
            headRotationY += -50 * Time.deltaTime;
            cameraOffsetX += -(50f / 65f * 7f) * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.L))
        {
            headRotationY += 50 * Time.deltaTime;
            cameraOffsetX += 50f / 65f * 7f * Time.deltaTime;
        }
        headRotationY = Mathf.Clamp(headRotationY, -30, 30);
        head.localEulerAngles = new Vector3(0, headRotationY, 0);

        cameraOffsetX = Mathf.Clamp(cameraOffsetX, -3.5f, 3.5f);
        followCamera.offset.x = cameraOffsetX;

#if false
        rotationX += Input.GetAxis("Vertical") * speed * Time.deltaTime;
        rotationX = Mathf.Clamp(rotationX, min, max);

        //cannon.localRotation = Quaternion.Euler(rotationX, 0, 0);
        cannon.localEulerAngles = new Vector3(rotationX, rotationY, 0); 
#endif
    }
    float cannonRotationX;
    float cameraOffsetY;
    float cameraOffsetX;
    void RotateCannon()
    {
        if (Input.GetKey(KeyCode.I))
        {
            cannonRotationX += -50 * Time.deltaTime;
            cameraOffsetY += 50f / 65f * 5f * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.K))
        {
            cannonRotationX += 50 * Time.deltaTime;
            cameraOffsetY += -(50f / 65f * 5f) * Time.deltaTime;
        }
        cannonRotationX = Mathf.Clamp(cannonRotationX, -45, 20); //65 , 50
        cannon.localEulerAngles = new Vector3(cannonRotationX, 0, 0);

        cameraOffsetY = Mathf.Clamp(cameraOffsetY, 1, 6); // 5  , 50 / 65 * 5
        followCamera.offset.y = cameraOffsetY;
    }
    public GameObject tempShell;
    public void Shot()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            tempShell = Instantiate(shell, firePivot.position, firePivot.rotation);
        }
    }
}



