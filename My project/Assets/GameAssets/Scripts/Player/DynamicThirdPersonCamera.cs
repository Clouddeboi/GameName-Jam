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

    [Header("Collision")]
    public LayerMask collisionLayers;
    public float collisionRadius = 0.3f;
    public float collisionOffset = 0.2f;
    public float collisionSmoothSpeed = 15f;

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

        //Rotation
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        //Desired position
        Vector3 desiredPosition = target.position - rotation * Vector3.forward * currentDistance;

        //Collision check
        Vector3 direction = (desiredPosition - target.position).normalized;
        float adjustedDistance = currentDistance;
        collisionRadius = playerSize * 0.05f;

        RaycastHit hit;
        if (Physics.SphereCast(target.position, collisionRadius, direction, out hit, currentDistance, collisionLayers, QueryTriggerInteraction.Ignore))
        {
            adjustedDistance = hit.distance - collisionOffset;
        }

        currentDistance = Mathf.Lerp(currentDistance, adjustedDistance, collisionSmoothSpeed * Time.deltaTime);

        Vector3 finalPosition = target.position - rotation * Vector3.forward * currentDistance;

        //Smooth follow
        transform.position = Vector3.Lerp(transform.position, finalPosition, followSmoothSpeed * Time.deltaTime);

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