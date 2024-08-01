using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimpleCharacterController : MonoBehaviour
{
    public float moveSpeed = 5f;

    private CharacterController characterController;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        Move();
    }

    private void Move()
    {
        // W, A, S, D tuþlarýndan gelen inputlarý al
        float moveDirectionX = Input.GetAxis("Horizontal"); // A ve D tuþlarý
        float moveDirectionZ = Input.GetAxis("Vertical");   // W ve S tuþlarý

        // Hareket yönünü belirle
        Vector3 move = transform.right * moveDirectionX + transform.forward * moveDirectionZ;

        // Karakteri hareket ettir
        characterController.Move(move * moveSpeed * Time.deltaTime);
    }
}