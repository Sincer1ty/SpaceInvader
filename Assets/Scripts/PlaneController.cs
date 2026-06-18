using UnityEngine;
using UnityEngine.InputSystem;

public class PlaneController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float forwardSpeed = 8f;

    private Vector2 moveInput;
    private Vector3 moveDirection;
    
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        moveDirection = new Vector3(moveInput.x, moveInput.y, 0f);
    }
    
    void Update()
    {
        Vector3 movement = transform.forward * forwardSpeed;
        movement += moveDirection * moveSpeed;

        transform.position += movement * Time.deltaTime;
    }
}
