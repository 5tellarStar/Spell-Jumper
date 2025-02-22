using Microsoft.Win32.SafeHandles;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
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

    private Vector3 castingPoint;

    List<dir> chant = new List<dir>();

    List<dir> blastChant = new List<dir>{dir.Right,dir.Up};
    public float blastMana = 40;

    List<dir> gravityChant = new List<dir> {dir.Down,dir.Down,dir.Up};
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

        bool grounded = Physics2D.BoxCast(transform.position,new Vector2(1,1),0,Vector2.down,0.4f, 64).collider != null;

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
            
            chant = new();
            Time.timeScale = 0.25f;
        }

        if(castingAction.WasReleasedThisFrame())
        {
            spellWheel.gameObject.SetActive(false);
            bool cast = chant.Count == blastChant.Count;
            if(cast)
            {
                for (int i = 0; i < chant.Count; i++)
                {
                    if (chant[i] != blastChant[i])
                    {
                        cast = false; break;
                    }
                }
                if(cast && mana >= blastMana)
                {
                    mana -= blastMana;
                    rb.AddForce((aim.position - transform.position).normalized * -500);

                    Collider2D hit = Physics2D.CircleCast(transform.position + (aim.position - transform.position).normalized, 1, Vector2.zero ,0, 64).collider;
                    if(hit != null)
                    {
                        rb.AddForce((aim.position - transform.position).normalized * -500);
                    }
                }
            }

            cast = chant.Count == gravityChant.Count;
            if(cast)
            {
                for (int i = 0; i < chant.Count; i++)
                {
                    if (chant[i] != gravityChant[i])
                    {
                        cast = false; break;
                    }
                }
                if (cast && mana >= gravityMana)
                {
                    mana -= gravityMana;
                    gravityTimer = 0;
                }
            }
           
            Time.timeScale = 1;
        }

        if (casting)
        {
            
            if((aim.position - castingPoint).magnitude > 0.5f)
            {
                spellWheel.gameObject.SetActive(false);
                if ((aim.position - castingPoint).x > 0)
                {
                    if((aim.position - castingPoint).y/(aim.position - castingPoint).x < -1)
                    {
                        Debug.Log("4");
                    }
                    else if((aim.position - castingPoint).y / (aim.position - castingPoint).x < 0)
                    {
                        Debug.Log("3");
                    }
                    else if((aim.position - castingPoint).y / (aim.position - castingPoint).x < 1)
                    {
                        Debug.Log("2");
                    }
                    else
                    {
                        Debug.Log("1");
                    }
                }
                else
                {
                    if ((aim.position - castingPoint).y / (aim.position - castingPoint).x < -1)
                    {
                        Debug.Log("8");
                    }
                    else if ((aim.position - castingPoint).y / (aim.position - castingPoint).x < 0)
                    {
                        Debug.Log("7");
                    }
                    else if ((aim.position - castingPoint).y / (aim.position - castingPoint).x < 1)
                    {
                        Debug.Log("6");
                    }
                    else
                    {
                        Debug.Log("5");
                    }
                }
            }

            if (up)
            {
                chant.Add(dir.Up);
            }
            else if(down)
            {
                chant.Add(dir.Down);
            }
            else if(left)
            {
                chant.Add(dir.Left);
            }
            else if(right)
            {
                chant.Add(dir.Right);
            }
            horizontalMoveValue = 0;
            firing = false;
            jumped = false;
            jumping = false;
        }



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



        jumpTime += Time.deltaTime;

        if (jumped && grounded)
        {
            rb.AddForceY(jumpForce);
            jumpTime = 0;
        }

        if (rb.linearVelocity.y < 0 || !jumping)
        {
            rb.gravityScale = 2f * gravityMod;
        }
        else if(jumping && rb.linearVelocity.y > 0 && jumpTime < 0.78f) 
        {
            rb.gravityScale = 0.5f * gravityMod;
        }
        else
        {
            rb.gravityScale = 1 * gravityMod;
        }


        if (aim.position.x < transform.position.x)
        {
            BookSprite.rotation = Quaternion.Euler(0, 0, MathF.Atan((aim.position.y - transform.position.y) / (aim.position.x - transform.position.x)) * Mathf.Rad2Deg - 180);
        }
        else
        {
            BookSprite.rotation = Quaternion.Euler(0, 0, MathF.Atan((aim.position.y - transform.position.y)/ (aim.position.x - transform.position.x)) * Mathf.Rad2Deg);
        }

        if(grounded)
        {
            rb.linearDamping = 1;
        }
        else
        {
            rb.linearDamping = 0;
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
public enum dir
{
    Up,
    Down,
    Left,
    Right
}
public enum spells
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