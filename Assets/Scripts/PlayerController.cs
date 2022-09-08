using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private float movementInputDirection;
    private bool isFacingRight = true;
    [SerializeField] float movementSpeed = 10.0f;
    [SerializeField] float jumpForce = 16.0f;

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update() {
        CheckInput();
        CheckMovementDirection();
    }

    private void FixedUpdate() {
        ApplyMovement();
    }

    private void CheckMovementDirection() {
        if(isFacingRight && movementInputDirection < 0) {
            Flip();
        } else if(!isFacingRight && movementInputDirection > 0) {
            Flip();
        }
    }

    private void CheckInput() {
        movementInputDirection = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump")) {
            Jump();
        }
    }

    private void ApplyMovement() {
        rb.velocity = new Vector2(movementSpeed * movementInputDirection, rb.velocity.y);
    }

    private void Jump() {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }

    private void Flip() {
        isFacingRight = !isFacingRight;
        transform.Rotate(0.0f, 180.0f, 0.0f);
    }
}
