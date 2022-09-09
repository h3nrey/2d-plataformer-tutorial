using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float movementInputDirection;
    private float jumpTimer;
    private float turnTimer;
    private float wallJumpTimer;

    private Rigidbody2D rb;
    private Animator anim;

    private int amountOfJumpsLeft;
    private int facingDirection = 1;
    private int lastWallJumpDirection;

    private bool isFacingRight = true;
    private bool isWalking = false;
    private bool isGrounded = true;
    private bool canNormalJump;
    private bool canWallJump;
    private bool isTouchingWall;
    private bool isSlidingWall;
    private bool isAttemptingToJump;
    private bool checkJumpMultiplier;
    private bool canMove;
    private bool canFlip;
    private bool hasWallJumped;

    public int amountOfJumps = 1;

    public float movementSpeed = 10.0f;
    public float wallSlidingSpeed;
    public float jumpForce = 16.0f;
    public float variableJumpHeightMultiplier = 0.5f;
    public float airDragMultiplier;
    public float movementForceInAir;
    public float groundCheckRadius;
    public float wallCheckDistance;
    public float wallHopForce;
    public float wallJumpForce;
    public float jumpTimerSet = 0.15f;
    public float turnTimerSet = 0.1f;
    public float wallJumpTimerSet = 0.5f;

    public Vector2 wallHopDirection;
    public Vector2 wallJumpDirection;

    public Transform groundCheck;
    public Transform wallCheck;
    public LayerMask whatIsGround;

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        amountOfJumpsLeft = amountOfJumps;
        wallHopDirection.Normalize();
        wallJumpDirection.Normalize();
    }

    private void Update() {
        CheckInput();
        CheckMovementDirection();
        UpdateAnimations();
        CheckIfCanJump();
        CheckJump();
        CheckIfWallSliding();
    }

    private void FixedUpdate() {
        ApplyMovement();
        CheckSurroundings();
    }

    private void CheckIfWallSliding() {
        if(isTouchingWall && movementInputDirection == facingDirection && rb.velocity.y < 0) {
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

        if(Input.GetButtonDown("Horizontal")  && isTouchingWall) {
            if(!isGrounded && movementInputDirection != facingDirection) {
                canMove = false;
                canFlip = false;

                turnTimer = turnTimerSet;
            }
        }
            
        if(!canMove) {
            turnTimer -= Time.deltaTime;

            if(turnTimer  <= 0) {
                canMove = true;
                canFlip = true;
            }
        }

        if (Input.GetButtonDown("Jump")) {
            
            if(isGrounded || (amountOfJumpsLeft > 0 && isTouchingWall)) {
                NormalJump();
            } else {
                jumpTimer = jumpTimerSet;
                isAttemptingToJump = true;
            }
        } 

        if(checkJumpMultiplier && !Input.GetButton("Jump")) {
            checkJumpMultiplier = false;
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * variableJumpHeightMultiplier);
        }
    }

    private void CheckIfCanJump() {
        if(isGrounded && rb.velocity.y <= 0.01f) {
            amountOfJumpsLeft = amountOfJumps;
        } 

        if(isTouchingWall) {
            canWallJump = true;
        } else {
            canWallJump = false;
        }

        if(amountOfJumpsLeft <= 0) {
            canNormalJump = false;
        } else {
            canNormalJump = true;
        }
    }

    private void CheckSurroundings() {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);
        isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround);
    }

    private void ApplyMovement() {
        if (!isGrounded && !isSlidingWall && movementInputDirection == 0) {
            rb.velocity = new Vector2(rb.velocity.x * airDragMultiplier, rb.velocity.y);
        } else if(canMove){
            rb.velocity = new Vector2(movementSpeed * movementInputDirection, rb.velocity.y);
        } 

        if(isSlidingWall) {
            if (rb.velocity.y < -wallSlidingSpeed) {
                rb.velocity = new Vector2(rb.velocity.x, -wallSlidingSpeed);
            }
        }
    }

    private void CheckJump() {
         if(jumpTimer  > 0) {
            //Wall Jump
            if(!isGrounded && isTouchingWall && movementInputDirection != 0 && movementInputDirection != facingDirection) {
                WallJump();
            }  else if(isGrounded) {
                NormalJump();
            } 
         }

         if (isAttemptingToJump) {
            jumpTimer -= Time.deltaTime;
         }

         if(wallJumpTimer > 0) {
            if(hasWallJumped && movementInputDirection  == -lastWallJumpDirection) {
                rb.velocity = new Vector2(rb.velocity.x, 0.0f);
                hasWallJumped = false;
            } else if(wallJumpTimer <= 0) {
                hasWallJumped = false;
            } else {
                wallJumpTimer = Time.deltaTime;
            }
        }
    }

    private void NormalJump() {
        if (canNormalJump) {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            amountOfJumpsLeft--;
            jumpTimer = 0;
            isAttemptingToJump = false;
            checkJumpMultiplier = true;
        }
    }

    private void WallJump() {
        if (canWallJump) {
            rb.velocity = new Vector2(rb.velocity.x, 0.0f);
            isSlidingWall = false;
            amountOfJumpsLeft = amountOfJumps;
            amountOfJumpsLeft--;
            Vector2 forceToAdd = new Vector2(wallJumpForce * wallJumpDirection.x * movementInputDirection, wallJumpForce * wallJumpDirection.y);
            rb.AddForce(forceToAdd, ForceMode2D.Impulse);
            jumpTimer = 0;
            isAttemptingToJump = false;
            checkJumpMultiplier = true;
            turnTimer = 0;
            canMove = true;
            canFlip = true;
            hasWallJumped = true;
            wallJumpTimer = wallJumpTimerSet;
            lastWallJumpDirection = -facingDirection;
        }
    }

    private void Flip() {
        if(!isSlidingWall && canFlip) {
            facingDirection *= -1;
            isFacingRight = !isFacingRight;
            transform.Rotate(0.0f, 180.0f, 0.0f);
        }
    }

    private void OnDrawGizmos() {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));
    }
}
