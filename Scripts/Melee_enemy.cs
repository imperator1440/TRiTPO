using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee_enemy : MonoBehaviour
{
    [SerializeField] private int m_health = 100;  // Wrong naming ('m_'), private field must start with "_"
    [SerializeField] private int m_moveRange = 20;  // Wrong naming ('m_'), private field must start with "_"
    [SerializeField] private float m_attackRange = 5;  // Wrong naming ('m_'), private field must start with "_" and .f
    [SerializeField] private int m_damage = 5; // Wrong naming ('m_'), private field must start with "_"
    [Range(0, .3f)] [SerializeField] private float m_movementSmoothing = .05f; // Wrong naming ('m_'), private field must start with "_"
    [SerializeField] private float m_walkSpeed = 10f; // Wrong naming ('m_'), private field must start with "_"
    public float m_moveCoordinates; // Wrong naming ('m_')
    [SerializeField] private Rigidbody2D m_rigidBody; // Wrong naming ('m_')
    [SerializeField] private Animator m_animator; // Wrong naming ('m_')
    [SerializeField] private Transform m_firepointRight; // Wrong naming ('m_')
    [SerializeField] private Transform m_firepointLeft; // Wrong naming ('m_')
    [SerializeField] private LayerMask m_layersToHit; // Wrong naming ('m_')
    private Vector3 m_velocity = Vector3.zero; // Wrong Vector3 is not velosity, ('m_') private field must start with "_"

    private bool m_facingRight = true; // Wrong naming ('m_'),You should name 'State' like '_isFacingRight'.  _isFacingRight
    private bool m_moving; // Wrong naming ('m_'), You should name 'State' like '_isMoving'. 
    private bool m_started = true;  // Wrong naming ('m_'), You should name 'State' like '_isStarted'. 

    private enum State // Wrong naming, You should name 'State' like 'AnimationState'. 
    {
        Idle,
        Walk,
        Attack,
        Hurt,
        Die
    };

    private State m_state = State.Idle;

    public void TakeDamage(int damage) // Wrong naming ('m_')
    {
        m_health -= damage;
        if (m_health < 0 && m_state != State.Die)
        {
            m_walkSpeed = 0;
            m_animator.SetTrigger("Die");
        }
    }

    private void Move(float move)
    {
        Vector3 targetVelocity = new Vector2(move * 10f, m_rigidBody.velocity.y); // Wrong Vector3 is not velosity
        m_rigidBody.velocity =
            Vector3.SmoothDamp(m_rigidBody.velocity, targetVelocity, ref m_velocity, m_movementSmoothing);
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }

    private void UpdateState()
    {
        AnimatorClipInfo[] currentState = m_animator.GetCurrentAnimatorClipInfo(0);
        string state = currentState[0].clip.name;
        switch (state)
        {
            case "run": //First 'r' is extra letter
                m_state = State.Walk;
                break;
            case "death": //First 'd' is extra letter
                m_state = State.Die;
                break;
            case "attack": //First 'a' is extra letter
                m_state = State.Attack;
                break;
            default:
                m_state = State.Idle;
                break;
        }
    }

    private void Attack()
    {
        if (Math.Abs(Math.Abs(m_moveCoordinates) - Math.Abs(transform.position.x)) < m_attackRange)
        {
            if (m_state != State.Die || m_state != State.Attack)
            {
                m_animator.SetTrigger("Attack");
            }
        }
    }

    private void Eye() //Wrong naming (Name of method talking nothing).
    {
        var angle = Mathf.Sin(Time.time * 100) * 360; //tweak this to change frequency
        RaycastHit2D hitRight = Physics2D.Raycast(m_firepointRight.position, m_firepointRight.right, m_moveRange);
        m_firepointRight.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        RaycastHit2D hitLeft = Physics2D.Raycast(m_firepointLeft.position, m_firepointLeft.right, m_moveRange);
        m_firepointLeft.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        if (hitRight)
            if (hitRight.collider.CompareTag("Player"))
            {
                m_moveCoordinates = hitRight.collider.transform.position.x;
                m_started = false;
                return;
            }

        if (hitLeft)
        {
            if (hitLeft.collider.CompareTag("Player"))
            {
                m_moveCoordinates = hitLeft.collider.transform.position.x;
                float distance = transform.position.x - m_moveCoordinates;

                if (!m_facingRight && distance < 0)
                {
                    Flip();
                }

                if (m_facingRight && distance > 0)
                {
                    Flip();
                }
                m_started = false;
            }
        }
    }

    private void Update()
    {
        Eye();
        UpdateState();
        RaycastHit2D hitOnFace =
            Physics2D.Raycast(m_firepointRight.position, Vector3.right * (m_facingRight ? 1 : -1));
        RaycastHit2D hitNotOnFace =
            Physics2D.Raycast(m_firepointLeft.position, Vector3.left * (m_facingRight ? 1 : -1));

        if (hitOnFace)
        {
            if (hitOnFace.collider.CompareTag("Player") && m_state != State.Attack)
            {
                Attack();
            }
        }

        if (hitNotOnFace)
        {
            if (hitNotOnFace.collider.CompareTag("Player") && m_state != State.Attack)
            {
                Attack();
            }
        }
        float distance = Math.Abs(Math.Abs(m_moveCoordinates) - Math.Abs(transform.position.x));
        if (!m_started)
        {
            if (distance < 0.1f && m_moving)
            {
                if (m_state != State.Die && m_state != State.Attack)
                {
                    m_animator.SetBool("Walk", false);
                    m_moving = false;
                }
            }

            if (distance > 0.1f && !m_moving)
            {
                if (m_state != State.Die && m_state != State.Attack && distance > m_attackRange)
                {
                    m_animator.SetBool("Walk", true);
                    m_moving = true;
                }
            }
        }

        if (m_moving && m_state != State.Die && m_state != State.Attack && distance > m_attackRange)
        {
            Move(m_walkSpeed * Time.fixedDeltaTime * (m_facingRight ? 1 : -1));
        }

    }

    private void Flip()
    {
        transform.Rotate(0f, 180f, 0f); //flipping
        m_facingRight = !m_facingRight;
    }

    public void HitCall()
    {
        StartCoroutine(Hit());
    }

    private IEnumerator Hit()
    {
        while (m_state == State.Attack)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(m_firepointRight.position, m_attackRange, m_layersToHit);

            foreach (Collider2D collider in colliders)
            {
                PlayerControl player = collider.GetComponent<PlayerControl>();
                player.TakeDamage(m_damage);
            }
            yield return new WaitForSeconds(0.01f);
        }
    }

    public void setMaterial(PhysicsMaterial2D material)
    {
        m_rigidBody.sharedMaterial = material;
    }
}
