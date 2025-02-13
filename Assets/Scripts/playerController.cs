using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class playerController : MonoBehaviour
{
    private Rigidbody2D rb;
    public float maxSpeed;
    public float accelSpeed;
    public float jumpForce;

    private float jumpTime = 0;

    private InputAction horizontalMoveAction;
    private InputAction jumpAction;
    private InputAction fireAction;


    [SerializeField] private Transform aim; 
    [SerializeField] private Transform BookSprite;

    [SerializeField] private Camera camera;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        horizontalMoveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        fireAction = InputSystem.actions.FindAction("Fire");
    }

    // Update is called once per frame
    void Update()
    {
        bool grounded = Physics2D.BoxCast(transform.position,new Vector2(1,1),0,Vector2.down,0.4f, 64).collider != null;

        float horizontalMoveValue = horizontalMoveAction.ReadValue<float>();

        if (horizontalMoveValue < 0 && grounded)
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
        if (horizontalMoveValue > 0 && grounded)
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

        jumpTime += Time.deltaTime;

        if (jumped && grounded)
        {
            rb.AddForceY(jumpForce);
            jumpTime = 0;
        }

        if (rb.linearVelocity.y < 0 || !jumping)
        {
            rb.gravityScale = 2f;
        }
        else if(jumping && rb.linearVelocity.y > 0 && jumpTime < 0.78f) 
        {
            rb.gravityScale = 0.5f;
        }
        else
        {
            rb.gravityScale = 1;
        }

        aim.position = Input.mousePosition * ((camera.orthographicSize * 2)/Screen.height) - new Vector3(camera.orthographicSize * camera.aspect,camera.orthographicSize);

        if (aim.position.x < transform.position.x)
        {
            BookSprite.rotation = Quaternion.Euler(0, 0, MathF.Atan((aim.position.y - transform.position.y) / (aim.position.x - transform.position.x)) * Mathf.Rad2Deg - 180);
        }
        else
        {
            BookSprite.rotation = Quaternion.Euler(0, 0, MathF.Atan((aim.position.y - transform.position.y)/ (aim.position.x - transform.position.x)) * Mathf.Rad2Deg);
        }

        bool fired = fireAction.WasPressedThisFrame();

        if (fired)
        {
            rb.AddForce((aim.position - transform.position).normalized * -300);
        }
        
    }
}
