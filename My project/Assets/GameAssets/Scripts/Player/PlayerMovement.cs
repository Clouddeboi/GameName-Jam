using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimplePlayerMovement : MonoBehaviour
{
    public float baseMoveSpeed = 5f;
    public float verticalSpeed = 5f;
    public float sizeSlowFactor = 0.05f;
    public Transform cameraTransform;

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (!cameraTransform)
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        //Camera relative directions (ignores vertical tilt)
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 move = forward * vertical + right * horizontal;

        float upDown = 0f;
        if (Input.GetKey(KeyCode.Q)) upDown = -1f;
        if (Input.GetKey(KeyCode.E)) upDown = 1f;

        move.y = upDown;

        //Movement slowdown based on the black holes size
        float size = transform.localScale.magnitude;
        float adjustedSpeed = baseMoveSpeed / Mathf.Log(size + 1f);

        Vector3 finalMove = new Vector3(move.x * adjustedSpeed, move.y * verticalSpeed, move.z * adjustedSpeed);

        controller.Move(finalMove * Time.deltaTime);
    }
}