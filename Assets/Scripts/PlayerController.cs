using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float movementInputDirection;

    private Rigidbody2D rb;
    private Animator anim;

    private int amountOfJumpsLeft;

    private bool isFacingRight = true;
    private bool isWalking = false;
    private bool isGrounded = true;
    private bool canJump = true;
    private bool isTouchingWall;
    private bool isSlidingWall;

    public int amountOfJumps = 1;

    public float movementSpeed = 10.0f;
    public float wallSlidingSpeed;
    public float jumpForce = 16.0f;
    public float variableJumpHeightMultiplier = 0.5f;
    public float airDragMultiplier;
    public float movementForceInAir;
    public float groundCheckRadius;
    public float wallCheckDistance;

    public Transform groundCheck;
    public Transform wallCheck;
    public LayerMask whatIsGround;

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        amountOfJumpsLeft = amountOfJumps;
    }

    private void Update() {
        CheckInput();
        CheckMovementDirection();
        UpdateAnimations();
        CheckIfCanJump();
        CheckIfWallSliding();
    }

    private void FixedUpdate() {
        ApplyMovement();
        CheckSurroundings();
    }

    private void CheckIfWallSliding() {
        if(isTouchingWall && !isGrounded && rb.velocity.y < 0) {
            isSlidingWall = true;
        } else {
            isSlidingWall = false;
        }
    }

    private void CheckMovementDirection() {
        if(isFacingRight && movementInputDirection < 0) {
            Flip();
        } else if(!isFacingRight && movementInputDirection > 0) {
            Flip();
        }

        if(Mathf.Abs(rb.velocity.x) >= 0.01f) {
            isWalking = true;
        } else {
            isWalking = false;
        }
    }

    private void UpdateAnimations() {
        anim.SetBool("isWalking", isWalking);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetBool("isSlidingWall", isSlidingWall);
    }

    private void CheckInput() {
        movementInputDirection = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump")) {
            Jump();
        } 

        if(Input.GetButtonUp("Jump")) {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * variableJumpHeightMultiplier);
        }
    }

    private void CheckIfCanJump() {
        if(isGrounded && rb.velocity.y <= 0) {
            amountOfJumpsLeft = amountOfJumps;
        } 

        if(amountOfJumpsLeft <= 0) {
            canJump = false;
        } else {
            canJump = true;
        }
    }

    private void CheckSurroundings() {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);
        isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround);
    }

    private void ApplyMovement() {
        if(isGrounded) {
            rb.velocity = new Vector2(movementSpeed * movementInputDirection, rb.velocity.y);
        } else if(!isGrounded && !isSlidingWall && movementInputDirection != 0) {
            Vector2 forceToAdd = new Vector2(movementForceInAir * movementInputDirection, 0);
            rb.AddForce(forceToAdd);

            if(Mathf.Abs(rb.velocity.x) > movementSpeed) {
                rb.velocity = new Vector2(movementSpeed * movementInputDirection, rb.velocity.y);
            }
        } else if(!isGrounded && !isSlidingWall && movementInputDirection == 0) {
            rb.velocity = new Vector2(rb.velocity.x * airDragMultiplier, rb.velocity.y);
        }

        if(isSlidingWall) {
            if (rb.velocity.y < -wallSlidingSpeed) {
                rb.velocity = new Vector2(rb.velocity.x, -wallSlidingSpeed);
            }
        }
    }

    private void Jump() {
        if(canJump) {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            amountOfJumpsLeft--;
        }
    }

    private void Flip() {
        if(!isSlidingWall) {
            isFacingRight = !isFacingRight;
            transform.Rotate(0.0f, 180.0f, 0.0f);
        }
    }

    private void OnDrawGizmos() {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));
    }
}
