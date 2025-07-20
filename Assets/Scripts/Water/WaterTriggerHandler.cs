using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterTriggerHandler : MonoBehaviour
{
    [SerializeField] private LayerMask waterMask;
    [SerializeField] private GameObject splashParticles;

    private EdgeCollider2D edgeColl;

    private InteractableWater water;

    private void Awake()
    {
        edgeColl = GetComponent<EdgeCollider2D>();
        water = GetComponent<InteractableWater>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If our collision gameObject is within the watermask layermask
        if ((waterMask.value & (1 << collision.gameObject.layer)) > 0)
        {
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                Vector2 localPos = gameObject.transform.localPosition;
                Vector2 hitObjectPos = collision.transform.position;
                Bounds hitObjectBounds = collision.bounds;

                Vector3 spawnPos = Vector3.zero;
                if (collision.transform.position.y >= edgeColl.points[1].y + edgeColl.offset.y + localPos.y)
                {
                    // hit from above
                    spawnPos = hitObjectPos - new Vector2(0f, hitObjectBounds.extents.y);
                }
                else
                {
                    // hit from below
                    spawnPos = hitObjectPos + new Vector2(0f, hitObjectBounds.extents.y);
                }
                Instantiate(splashParticles, spawnPos, Quaternion.identity);

                // Clamp splash point to a max velocity
                int multiplier;
                if (rb.velocity.y < 0)
                {
                    multiplier = -1;
                }
                else
                {
                    multiplier = 1;
                }

                float vel = rb.velocity.y * water.ForceMultiplier;
                vel = Mathf.Clamp(Mathf.Abs(vel), 0f, water.MaxForce);
                vel *= multiplier;

                water.Splash(collision, vel);
                Debug.Log("Splash");
            }
        }
    }
}
