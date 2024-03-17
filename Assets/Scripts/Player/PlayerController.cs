using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float turnSpeed = 10f;
    public float acceleration = 1000f;
    public float speedMultiplier = 2f;
    public float speedBoostCooldown = 10f;
    public float speedBoostDuration = 2f;

    private bool isSpeedBoostActive = false;
    public bool isSpeedBoostOnCooldown = false;
    private float speedBoostCooldownTimer = 0f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 currentVelocity;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction boostAction;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();

        // Assuming "PlayerControls" is the name of your action map
        var actionMap = playerInput.actions.FindActionMap("PlayerControls");
        moveAction = actionMap.FindAction("Move");
        boostAction = actionMap.FindAction("SpeedBoost");
    }

    public void EnableMovement()
    {
        moveAction.Enable();
    }

    public void DisableMovement()
    {
        moveAction.Disable();
    }

    private void Update()
    {
        // Handle cooldown timer in Update to ensure it decreases over time
        if (isSpeedBoostOnCooldown)
        {
            speedBoostCooldownTimer -= Time.deltaTime;
            if (speedBoostCooldownTimer <= 0)
            {
                isSpeedBoostOnCooldown = false;
                speedBoostCooldownTimer = 0; // Ensure the timer doesn't go negative
            }
        }
    }

    private void FixedUpdate()
    {
        // Read the movement value
        moveInput = moveAction.ReadValue<Vector2>();
        MoveSubmarine();
    }

    private Vector2 smoothDampVelocity;
    private void MoveSubmarine()
    {
        float currentSpeed = isSpeedBoostActive ? moveSpeed * speedMultiplier : moveSpeed;

        if (boostAction.ReadValue<float>() > 0 && !isSpeedBoostActive && !isSpeedBoostOnCooldown)
        {
            StartCoroutine(ActivateSpeedBoost(speedBoostDuration));
        }

        if (isSpeedBoostActive)
        {
            currentSpeed *= speedMultiplier;
        }

        Vector2 targetVelocity = moveInput * currentSpeed;
        currentVelocity = Vector2.SmoothDamp(rb.velocity, targetVelocity, ref smoothDampVelocity, 0.3f);
        rb.velocity = currentVelocity;

        if (moveInput != Vector2.zero)
        {
            float targetAngle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;
            rb.rotation = Mathf.LerpAngle(rb.rotation, targetAngle, turnSpeed * Time.fixedDeltaTime);
        }

        if (moveInput.x > 0)
        {
            // Moving right - ensure sprite faces right (assuming default sprite faces right)
            transform.localScale = new Vector3(transform.localScale.x, Mathf.Abs(transform.localScale.y), transform.localScale.z);
        }
        else if (moveInput.x < 0)
        {
            // Moving left - flip sprite to face left
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), -Mathf.Abs(transform.localScale.y), transform.localScale.z);
        }
    }

    IEnumerator ActivateSpeedBoost(float duration)
    {
        isSpeedBoostActive = true;
        yield return new WaitForSeconds(duration);
        isSpeedBoostActive = false;

        // Start cooldown after boost ends
        isSpeedBoostOnCooldown = true;
        speedBoostCooldownTimer = speedBoostCooldown;
    }
}
