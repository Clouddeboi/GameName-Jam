using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimplePlayerMovement : MonoBehaviour
{
    public float baseMoveSpeed = 5f;
    public float verticalSpeed = 5f;
    public float sizeSlowFactor = 0.05f;

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(horizontal, 0f, vertical);

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