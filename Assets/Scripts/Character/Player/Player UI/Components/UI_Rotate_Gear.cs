using UnityEngine;

public class UI_Rotate_Gear : MonoBehaviour
{
    public float rotationSpeed = 5f;

    void Update()
    {
        float angle = rotationSpeed * Time.deltaTime;
        transform.Rotate(Vector3.back, angle);
    }
}
