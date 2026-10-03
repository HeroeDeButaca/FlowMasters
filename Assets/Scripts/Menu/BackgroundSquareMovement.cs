using UnityEngine;

public class BackgroundSquareMovement : MonoBehaviour
{
    [SerializeField]
    private Vector3 _velocity = new Vector3(0f, 0f, -0.25f);
    private Vector3 _initialPos;

    private Rigidbody rb;

    void Start()
    {
        _initialPos = transform.position;
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + _velocity * Time.fixedDeltaTime);

        if (transform.position.z <= -1f)
            transform.position = _initialPos;
    }
}
