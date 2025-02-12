using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class playerController : MonoBehaviour
{
    private Rigidbody2D rb;
    public float maxSpeed;
    public float accelSpeed;
    public float jumpForce;

    private InputAction horizontalMoveAction;
    private InputAction jumpAction;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        horizontalMoveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalMoveValue = horizontalMoveAction.ReadValue<float>();

        if (horizontalMoveValue < 0)
        {
            if(rb.linearVelocity.x <= -maxSpeed)
            {
                rb.linearVelocity = new Vector2(Mathf.Clamp(rb.linearVelocity.x, -1000, -maxSpeed), rb.linearVelocity.y);
            }
            else
            {
                rb.linearVelocity += new Vector2(-accelSpeed * Time.deltaTime, 0);
            }
        }
        if (horizontalMoveValue > 0)
        {
            if (rb.linearVelocity.x >= maxSpeed)
            {
                rb.linearVelocity = new Vector2(Mathf.Clamp(rb.linearVelocity.x, maxSpeed, 1000), rb.linearVelocity.y);
            }
            else
            {
                rb.linearVelocity += new Vector2(accelSpeed * Time.deltaTime, 0);
            }
        }


        bool jumped = jumpAction.WasPressedThisFrame();
        bool jumping = jumpAction.IsPressed();

        bool grounded = Physics2D.BoxCast(transform.position,new Vector2(1,1),0,Vector2.down,0.4f, 64).collider != null;

        if (jumped && grounded)
        {
            rb.AddForceY(jumpForce);
        }

        if (rb.linearVelocity.y < 0 || !jumping)
        {
            rb.gravityScale = 2f;
        }
        else if(jumping && rb.linearVelocity.y > 0)
        {
            rb.gravityScale = 0.5f;
        }
        else
        {
            rb.gravityScale = 1;
        }

    }
}
