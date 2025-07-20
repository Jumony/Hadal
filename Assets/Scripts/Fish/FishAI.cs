using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum FishState
{
    Idle,
    Wander,
    Flee,
}

public class FishAI : MonoBehaviour
{
    public Transform player;
    public Rigidbody2D rb;
    public float fleeDistance = 3f;

    public float speed = 2f;
    public float fleeSpeed = 5f;
    public float fleeImpulseSpeed;
    public float idleDuration = 2f;
    public float wanderDuration = 3f;

    private FishState currentState;
    private float stateTimer;
    private Vector2 direction;

    // Start is called before the first frame update
    void Start()
    {
        TransitionToState(FishState.Wander);
    }

    // Update is called once per frame
    void Update()
    {
        float distanceFromPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceFromPlayer < fleeDistance)
        {
            TransitionToState(FishState.Flee);
        }

        stateTimer -= Time.deltaTime;
        switch (currentState)
        {
            case FishState.Idle:
                rb.drag = 5f;
                if (stateTimer <= 0)
                {
                    TransitionToState(FishState.Wander);
                }
                break;

            case FishState.Wander:
                rb.drag = 0f;
                rb.AddForce(direction * speed * Time.deltaTime, ForceMode2D.Force);
                if (stateTimer <= 0)
                {
                    TransitionToState(FishState.Idle);
                }
                break;

            case FishState.Flee:
                rb.drag = 0f;
                Vector2 fleeDir = (transform.position - player.position).normalized;
                rb.AddForce(fleeDir * fleeSpeed * Time.deltaTime);
                if (distanceFromPlayer >= fleeDistance)
                {
                    TransitionToState(FishState.Wander);
                }
                break;
        }
    }

    void TransitionToState(FishState newState)
    {
        Debug.Log("Switching to new state: " + newState);
        currentState = newState;
        switch (newState)
        {
            case FishState.Idle:
                stateTimer = idleDuration;
                break;

            case FishState.Wander:
                direction = Random.insideUnitCircle.normalized;
                SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
                if (direction.x < 0)
                {
                    spriteRenderer.flipX = true;
                }
                else
                {
                    spriteRenderer.flipX = false;
                }
                stateTimer = wanderDuration;
                break;

            case FishState.Flee:
                Vector2 fleeDir = (transform.position - player.position).normalized;
                rb.velocity = Vector2.zero;
                rb.AddForce(fleeDir * fleeImpulseSpeed, ForceMode2D.Impulse);
                break;
        }
    }
}
