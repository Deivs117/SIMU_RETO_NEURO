using UnityEngine;

public class Agent : MonoBehaviour
{
    private Rigidbody rb;
    float speed = 5.0f;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(moveHorizontal*speed, rb.linearVelocity.y, moveVertical*speed);
        rb.linearVelocity = movement;
    }
}
