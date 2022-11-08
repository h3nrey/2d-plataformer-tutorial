using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float movementInputDirection;
    private float jumpTimer;
    private float turnTimer;
    private float wallJumpTimer;
    private float dashTimeLeft;
    private float lastImageXPos;
    private float lastDash = -100f;

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
    private bool isTouchingLedge;
    private bool canClimbLedge = false;
    private bool ledgeDetected;
    private bool isDashing;

    private Vector2 ledgePosBot;
    private Vector2 ledgePos1;
    private Vector2 ledgePos2;

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
    public float dashTime;
    public float dashSpeed;
    public float distanceBetweenImages;
    public float dashCooldown;


    public float ledgeClimbXOffset1 = 0f;
    public float ledgeClimbYOffset1 = 0f;
    public float ledgeClimbXOffset2 = 0f;
    public float ledgeClimbYOffset2 = 0f;

    public Vector2 wallHopDirection;
    public Vector2 wallJumpDirection;

    public Transform groundCheck;
    public Transform wallCheck;
    public Transform LedgeCheck;

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
        CheckLedgeClimb();
        CheckDash();
    }

    private void FixedUpdate() {
        ApplyMovement();
        CheckSurroundings();
    }

    private void CheckIfWallSliding() {
        if(isTouchingWall && movementInputDirection == facingDirection && rb.velocity.y < 0 && !canClimbLedge) {
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
            
        if(turnTimer >= 0) {
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

        if(Input.GetKeyDown(KeyCode.C)) {
            print("pressed dash button");
            if(Time.time >= (lastDash + dashCooldown))
            AttemptToDash();
        }
    }

    private void AttemptToDash() {
        isDashing = true;
        dashTimeLeft = dashTime;
        lastDash = Time.time;

        PlayerAfterImagePool.Instance.GetFromPool();
        lastImageXPos = transform.position.x;
    }

    private void CheckDash() {
        if(isDashing) {
            if(dashTimeLeft > 0) {
                canMove = false;
                canFlip = false;

                rb.velocity = new Vector2(dashSpeed * facingDirection, rb.velocity.y);
                dashTimeLeft -= Time.deltaTime;

                if (Mathf.Abs(transform.position.x - lastImageXPos) > distanceBetweenImages) {
                    PlayerAfterImagePool.Instance.GetFromPool();
                    lastImageXPos = transform.position.x;
                }
            }

            if(dashTimeLeft <= 0 || isTouchingWall) {
                isDashing = false;
                canMove = true;
                canFlip = true;
            }
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

    private void CheckLedgeClimb() {
        if(ledgeDetected && !canClimbLedge) {
            canClimbLedge = true;

            if(isFacingRight) {
                ledgePos1 = new Vector2(Mathf.Floor(ledgePosBot.x + wallCheckDistance) - ledgeClimbXOffset1, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset1);
                ledgePos2 = new Vector2(Mathf.Floor(ledgePosBot.x + wallCheckDistance) + ledgeClimbXOffset2, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset2);
            } else {
                ledgePos1 = new Vector2(Mathf.Ceil(ledgePosBot.x - wallCheckDistance) + ledgeClimbXOffset1, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset1);
                ledgePos2 = new Vector2(Mathf.Ceil(ledgePosBot.x - wallCheckDistance) - ledgeClimbYOffset2, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset2);
            }

            canMove = false;
            canFlip = false;
        }

        if (canClimbLedge) {
            transform.position = ledgePos1;
        }

        anim.SetBool("canClimbLedge", canClimbLedge);
    }

    public void FinishLedgeClimb() {
        print("teste");
        canClimbLedge = false;
        transform.position = ledgePos2;
        canMove = true;
        canFlip = true;
        ledgeDetected = false;
        anim.SetBool("canClimbLedge", canClimbLedge);
    }

    private void CheckSurroundings() {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);

        isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround);

        isTouchingLedge = Physics2D.Raycast(LedgeCheck.position, transform.right, wallCheckDistance, whatIsGround);

        if(isTouchingWall && !isTouchingLedge && !ledgeDetected) {
            ledgeDetected = true;
            ledgePosBot = wallCheck.position;
        }
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
