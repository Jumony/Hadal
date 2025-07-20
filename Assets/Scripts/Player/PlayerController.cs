using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerStatsSO playerStats;

    public Rigidbody2D rb;
    public Vector2 input;

    private float moveForce;
    private float maxSpeed;
    private float waterDrag;
    private float buoyancyForce;

    private void Awake()
    {
        moveForce = playerStats.moveForce;
        maxSpeed = playerStats.maxSpeed;
        waterDrag = playerStats.waterDrag;
        buoyancyForce = playerStats.buoyancyForce;
    }

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
