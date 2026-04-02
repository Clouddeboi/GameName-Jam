using UnityEngine;

public class DynamicThirdPersonCamera : MonoBehaviour
{
    public Transform target;

    [Header("Rotation")]
    public float mouseSensitivity = 3f;
    public float minYAngle = -30f;
    public float maxYAngle = 80f;

    [Header("Distance")]
    public float distanceMultiplier = 2.5f;
    public float minDistance = 3f;
    public float maxDistance = 50f;
    public float distanceSmoothSpeed = 10f;

    [Header("Follow")]
    public float followSmoothSpeed = 10f;

    private float yaw;
    private float pitch;
    private float currentDistance;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        currentDistance = minDistance;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (!target) return;

        yaw += Input.GetAxis("Mouse X") * mouseSensitivity * 100f * Time.deltaTime;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity * 100f * Time.deltaTime;

        pitch = Mathf.Clamp(pitch, minYAngle, maxYAngle);

        //Calculate player "size"
        float playerSize = GetTargetSize();

        //Dynamic distance based on size
        float targetDistance = Mathf.Clamp(playerSize * distanceMultiplier, minDistance, maxDistance);
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, distanceSmoothSpeed * Time.deltaTime);

        //Rotation and Position
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        Vector3 desiredPosition = target.position - rotation * Vector3.forward * currentDistance;

        //Smooth follow
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSmoothSpeed * Time.deltaTime);

        //Always look at player
        transform.LookAt(target);
    }

    float GetTargetSize()
    {
        //Try renderer bounds first or fallback to scale
        Renderer r = target.GetComponentInChildren<Renderer>();
        if (r != null)
            return r.bounds.size.magnitude;

        return target.localScale.magnitude;
    }
}