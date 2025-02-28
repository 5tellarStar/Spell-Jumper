using Microsoft.Win32.SafeHandles;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class playerController : MonoBehaviour
{
    private Rigidbody2D rb;
    public float maxSpeed;
    public float accelSpeed;
    public float jumpForce;

    public float projectileSpeed;
    public float fireRate;

    public float maxMana = 100;
    public float mana = 100;
    public float manaRecharge = 5;

    private float fireTime = 0;

    private float jumpTime = 0;

    private InputAction horizontalMoveAction;
    private InputAction jumpAction;
    private InputAction fireAction;
    private InputAction downAction;
    private InputAction castingAction;

    [SerializeField] private GameObject projectile;

    [SerializeField] private Transform aim;
    [SerializeField] private Transform spellWheel;
    [SerializeField] private Transform BookSprite;
    [SerializeField] private Transform spawnPoint;

    [SerializeField] private Camera camera;

    private Spell currentSpell;
    private Spell prevSpell;
    private Vector3 castingPoint;

    public float blastMana = 40;

    public float gravityMana = 100;
    public float gravityTime = 5;
    private float gravityTimer = 5;
    private float gravityMod = 1;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        horizontalMoveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        fireAction = InputSystem.actions.FindAction("Fire");
        downAction = InputSystem.actions.FindAction("Down");
        castingAction = InputSystem.actions.FindAction("Casting");
    }

    // Update is called once per frame
    void Update()
    {
        gravityTimer += Time.deltaTime;
        if(gravityTimer < gravityTime)
        {
            gravityMod = 0.5f;
        }
        else
        {
            gravityMod = 1;
        }

        aim.position = Input.mousePosition * ((camera.orthographicSize * 2)/Screen.height) - new Vector3(camera.orthographicSize * camera.aspect,camera.orthographicSize);

        bool grounded = Physics2D.BoxCast(transform.position - new Vector3(0,-0.9f,0),new Vector2(0.7f,0.3f * 0.75f),0,Vector2.down,1.6f,64).collider != null && Physics2D.BoxCast(transform.position - new Vector3(0, -0.9f, 0), new Vector2(0.7f, 0.3f * 0.75f), 0, Vector2.down, 1.6f, 64).collider.transform.position.y < transform.position.y - 1;


        float horizontalMoveValue = horizontalMoveAction.ReadValue<float>();

        bool casting = castingAction.IsPressed();

        bool up = jumpAction.WasPressedThisFrame();
        bool down = downAction.WasPressedThisFrame();
        bool left = horizontalMoveAction.WasPressedThisFrame() && horizontalMoveValue < 0;
        bool right = horizontalMoveAction.WasPressedThisFrame() && horizontalMoveValue > 0;

        bool firing = fireAction.IsPressed();
        bool jumped = jumpAction.WasPressedThisFrame();
        bool jumping = jumpAction.IsPressed();


        if(grounded)
        {
            mana = Math.Clamp(mana + manaRecharge * Time.deltaTime, 0, maxMana);
        }


        if(castingAction.WasPressedThisFrame())
        {
            castingPoint = aim.position;
            spellWheel.gameObject.SetActive(true);
            spellWheel.position = castingPoint;
            
            Time.timeScale = 0.25f;

            currentSpell = Spell.None;
        }

        if(castingAction.WasReleasedThisFrame())
        {
            spellWheel.gameObject.SetActive(false);
            if(currentSpell == Spell.None)
            {
                currentSpell = prevSpell;
            }

            prevSpell = currentSpell;

            switch (currentSpell)
            {
            case Spell.Blast:
                if(mana >= blastMana)
                {
                    mana -= blastMana;
                    rb.AddForce((aim.position - transform.position).normalized * -500);

                    Collider2D hit = Physics2D.CircleCast(transform.position + (aim.position - transform.position).normalized, 1, Vector2.zero ,0, 64).collider;
                    if(hit != null)
                    {
                        rb.AddForce((aim.position - transform.position).normalized * -500);
                    }
                }
                break;
                case Spell.Gravity:
                if (mana >= gravityMana)
                {
                    mana -= gravityMana;
                    gravityTimer = 0;
                }
                break;
                default:
                break;
            }
           
            Time.timeScale = 1;
        }

        if (casting)
        {
            
            if((aim.position - castingPoint).magnitude > 0.5f && currentSpell == Spell.None)
            {
                spellWheel.gameObject.SetActive(false);
                if ((aim.position - castingPoint).x > 0)
                {
                    if((aim.position - castingPoint).y/(aim.position - castingPoint).x < -1)
                    {
                        Debug.Log("4");
                        currentSpell = Spell.Temp1;
                    }
                    else if((aim.position - castingPoint).y / (aim.position - castingPoint).x < 0)
                    {
                        Debug.Log("3");
                        currentSpell = Spell.Temp1;
                    }
                    else if((aim.position - castingPoint).y / (aim.position - castingPoint).x < 1)
                    {
                        Debug.Log("2");
                        currentSpell = Spell.Gravity;
                    }
                    else
                    {
                        Debug.Log("1");
                        currentSpell = Spell.Blast;
                    }
                }
                else
                {
                    if ((aim.position - castingPoint).y / (aim.position - castingPoint).x < -1)
                    {
                        Debug.Log("8");
                        currentSpell = Spell.Temp1;
                    }
                    else if ((aim.position - castingPoint).y / (aim.position - castingPoint).x < 0)
                    {
                        Debug.Log("7");
                        currentSpell = Spell.Temp1;
                    }
                    else if ((aim.position - castingPoint).y / (aim.position - castingPoint).x < 1)
                    {
                        Debug.Log("6");
                        currentSpell = Spell.Temp1;
                    }
                    else
                    {
                        Debug.Log("5");
                        currentSpell = Spell.Temp1;
                    }
                }
            }
        }


        if(grounded)
        {
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
        }
        else
        {
            if (horizontalMoveValue < 0)
            {
                if (rb.linearVelocity.x <= -maxSpeed/2)
                {
                    rb.linearVelocity = new Vector2(Mathf.Clamp(rb.linearVelocity.x, -1000, -maxSpeed /2), rb.linearVelocity.y);
                }
                else
                {
                    rb.linearVelocity += new Vector2(-accelSpeed/2 * Time.deltaTime, 0);
                }
            }
            if (horizontalMoveValue > 0)
            {
                if (rb.linearVelocity.x >= maxSpeed/2)
                {
                    rb.linearVelocity = new Vector2(Mathf.Clamp(rb.linearVelocity.x, maxSpeed/2, 1000), rb.linearVelocity.y);
                }
                else
                {
                    rb.linearVelocity += new Vector2(accelSpeed/2 * Time.deltaTime, 0);
                }
            }
        }



        jumpTime += Time.deltaTime;

        if (jumped && grounded)
        {
            rb.AddForceY(jumpForce);
            jumpTime = 0;
        }

        if (rb.linearVelocity.y < 0 || !jumping)
        {
            rb.gravityScale = 3 * gravityMod;
        }
        else if(jumping && rb.linearVelocity.y > 0 && jumpTime < 0.78f) 
        {
            rb.gravityScale = 1 * gravityMod;
        }
        else
        {
            rb.gravityScale = 2 * gravityMod;
        }


        if (aim.position.x < transform.position.x)
        {
            BookSprite.rotation = Quaternion.Euler(0, 0, MathF.Atan((aim.position.y - transform.position.y) / (aim.position.x - transform.position.x)) * Mathf.Rad2Deg - 180);
        }
        else
        {
            BookSprite.rotation = Quaternion.Euler(0, 0, MathF.Atan((aim.position.y - transform.position.y)/ (aim.position.x - transform.position.x)) * Mathf.Rad2Deg);
        }



        fireTime += Time.deltaTime;

        if (firing && fireTime > fireRate && !casting)
        {
            fireTime = 0;
            GameObject shot = Instantiate(projectile);
            shot.transform.rotation = BookSprite.rotation;
            shot.GetComponent<projectile>().speed = projectileSpeed;
            shot.transform.position = spawnPoint.position;
        }
        
    }
}

public enum Spell
{
    None,
    Blast,
    Gravity,
    Shield,
    Temp1,
    Temp2,
    Temp3,
    Temp4,
    Temp5
}