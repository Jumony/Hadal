using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float moveForce = 10f;
    public float maxSpeed = 3f;
    public float waterDrag = 3f;
    public float buoyancyForce = 0.5f;

    public Rigidbody2D rb;
    public Vector2 input;

    // Start is called before the first frame update
    void Start()
    {
        rb.drag = waterDrag;
    }

    // Update is called once per frame
    void Update()
    {
        input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        Vector2 direction = rb.velocity.normalized;

        // test
        Debug.DrawRay(transform.position, direction * 2f, Color.green);
    }

    private void FixedUpdate()
    {
        if (input != Vector2.zero)
        {
            rb.AddForce(input.normalized * moveForce);

            if (rb.velocity.magnitude > maxSpeed)
            {
                rb.velocity = rb.velocity.normalized * maxSpeed;
            }
        }
        else
        {
            rb.AddForce(Vector2.up * buoyancyForce);
        }
    }

    private void LateUpdate()
    {
        if (rb.velocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, angle), Time.deltaTime * 2f);
        }
    }
}
