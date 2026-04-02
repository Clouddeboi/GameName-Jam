using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimplePlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float verticalSpeed = 5f;

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
        if (Input.GetKey(KeyCode.Q))
            upDown = -1f;
        if (Input.GetKey(KeyCode.E))
            upDown = 1f;

        move.y = upDown;

        controller.Move(move * moveSpeed * Time.deltaTime);
    }
}