using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts {
    public class PlayerControl : MonoBehaviour {
        [SerializeField] private float m_movementSpeed = 2.5f;
        private float m_xMovement;

        [Range(0, .3f)] [SerializeField] private float m_movementSmoothing = .05f; // How much to smooth out the movement

        [SerializeField] private float m_jumpForce = 20f;

        [SerializeField] private float m_slopeCheckDistance = 0.5f;
        [SerializeField] private float m_groundedRadius = .2f; // Radius of the overlap circle to determine if grounded

        private float m_slopeDownAngle;


        [SerializeField] private int m_maxHealth = 50;
        public float m_currentHealth;

        private bool m_canRegainHealth;
        [Range(0, 20f)] [SerializeField] private float m_regeneration = 5f;
        GameMaster gm;
        private bool m_canGetDamage = true;
        private float m_cantGetDamageTimer = 0.75f;
        //private int m_gold = 0;

        private bool m_isJumping;
        private bool m_isOnGround;
        private bool m_isOnSlope;
        private bool m_canJump;
        private bool m_nearLever;
        private bool m_nearChest;
        private bool m_facingRight = true;

        private bool m_hasControls = true;

        private Vector3 m_velocity = Vector3.zero;

        private Vector2 m_newVelocity;
        private Vector2 m_newForce;
        private Vector2 m_slopeNormalPerp;


        private Rigidbody2D m_rigidbody2D;

        [SerializeField] private PhysicsMaterial2D m_noFriction;
        [SerializeField] private PhysicsMaterial2D m_fullFriction;

        [SerializeField] private LayerMask m_whatIsGround = new LayerMask(); // A mask determining what is ground to the character

        [SerializeField] private Animator m_animator;
        [SerializeField] private Transform m_groundCheck = null; // A position marking where to check if the player is grounded.

        [SerializeField] private Transform m_firepoint;
        [SerializeField] private GameObject m_projectile;

        [SerializeField] private HealthBar m_healthBar;

        private enum State {
            Idle,
            Walk,
            Jump,
            Attack,
            Hit,
            Die
        };

        private State m_state = State.Idle;

        private LeverInteraction m_lever;
        private ChestInteraction m_chest;

        private void Start()
        {
            Physics2D.IgnoreLayerCollision(9, 10);
            Physics2D.IgnoreLayerCollision(10, 10);
            m_currentHealth = m_maxHealth;
            m_healthBar.SetMaxHealth(m_maxHealth);
            StartCoroutine(RegainHealthOverTime());
            StartCoroutine(CantTakeDamageWait());
            gm = GameObject.FindGameObjectWithTag("GameMaster").GetComponent<GameMaster>();
            transform.position = gm.m_lastCheckPoint;
        }

        private void Awake() {

            m_rigidbody2D = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate() {
            CheckGround();
            SlopeCheck();
            Move();
        }

        private void Update() {
            UpdateState();
            CheckInput();
            Interactions();
        }

        private void CheckInput() {
            m_animator.SetBool("OnGround", m_isOnGround);
            m_hasControls = true;
            if (m_state == State.Hit || m_state == State.Attack || m_state == State.Die)
            {
                m_xMovement = 0;
                m_hasControls = false;
            }

            if (m_hasControls) {
                m_xMovement = Input.GetAxisRaw("Horizontal") * m_movementSpeed;
                m_animator.SetFloat("Speed", Mathf.Abs(m_xMovement));

                if (m_xMovement > 0 && !m_facingRight) Flip();
                else if (m_xMovement < 0 && m_facingRight) Flip();

                if (Input.GetKey("space")) Jump();
                if (Input.GetKeyDown("mouse 0") && m_state != State.Jump && m_state != State.Attack && m_state != State.Die) {
                    m_xMovement = 0;
                    m_animator.SetTrigger("Attacking");
                }
            }
        }

        private IEnumerator RegainHealthOverTime()
        {
            m_canRegainHealth = true;
            while (true)
            {
                yield return new WaitForSeconds(0.1f);
                if (!m_canRegainHealth)
                {
                    yield return new WaitForSeconds(5);
                    m_canRegainHealth = true;
                }
                AdjustHealth((m_maxHealth * m_regeneration) / 1000);
            }
        }

        private void AdjustHealth(float adj)
        {
            //Debug.Log("Adjusting " + adj + " health point");
            m_currentHealth += adj;
            m_healthBar.SetHealth((int)m_currentHealth);
            if (m_currentHealth > m_maxHealth)
            {
                m_currentHealth = m_maxHealth;
            }
            if(m_currentHealth < 0)
            {
                if (m_state != State.Die)
                {
                    m_animator.SetTrigger("Die");
                    Destroy(gameObject, 5f / 6f);
                }
            }
        }

        private void Jump() {
            if (!m_canJump) return;
            m_animator.SetTrigger("Jump");
            m_animator.SetBool("Jumping", true);
            m_canJump = false;
            m_isJumping = true;
            m_newVelocity.Set(0.0f, 0.0f);
            m_rigidbody2D.velocity = m_newVelocity;
            m_newForce.Set(0.0f, m_jumpForce);
            m_rigidbody2D.AddForce(m_newForce, ForceMode2D.Impulse);
        }

        private void CheckGround() {
            m_isOnGround = Physics2D.OverlapCircle(m_groundCheck.position, m_groundedRadius, m_whatIsGround);

            if (m_rigidbody2D.velocity.y <= 0.0f) {
                m_animator.SetBool("Jumping", false);
                m_isJumping = false;
            }

            if (m_isOnGround && !m_isJumping) m_canJump = true;
        }

        private void Move() {

            if (m_isOnGround && !m_isOnSlope && !m_isJumping) { // On Ground
                m_newVelocity.Set(m_movementSpeed * m_xMovement, 0.0f);
                m_rigidbody2D.velocity = Vector3.SmoothDamp(m_rigidbody2D.velocity, m_newVelocity, ref m_velocity,
                    m_movementSmoothing);
            }
            else if (m_isOnGround && m_isOnSlope && !m_isJumping) { // On slope
                m_newVelocity.Set(m_movementSpeed * m_slopeNormalPerp.x * -m_xMovement,
                    m_xMovement * m_slopeNormalPerp.y * -m_xMovement);
                m_rigidbody2D.velocity = Vector3.SmoothDamp(m_rigidbody2D.velocity, m_newVelocity, ref m_velocity,
                    m_movementSmoothing);
            }
            else if (!m_isOnGround) { // In the air
                m_newVelocity.Set(m_movementSpeed * m_xMovement, m_rigidbody2D.velocity.y);
                m_rigidbody2D.velocity = Vector3.SmoothDamp(m_rigidbody2D.velocity, m_newVelocity, ref m_velocity,
                    m_movementSmoothing);
            }
        }

        private void SlopeCheck() {
            Vector2 checkPos = m_groundCheck.position;
            SlopeCheckHorizontal(checkPos);
            SlopeCheckVertical(checkPos);
        }

        private void SlopeCheckHorizontal(Vector2 checkPos) {
            RaycastHit2D slopeHitFront =
                Physics2D.Raycast(checkPos, transform.right, m_slopeCheckDistance, m_whatIsGround);
            RaycastHit2D slopeHitBack =
                Physics2D.Raycast(checkPos, -transform.right, m_slopeCheckDistance, m_whatIsGround);

            if (slopeHitFront || slopeHitBack) m_isOnSlope = true;
            else m_isOnSlope = false;

        }

        private void SlopeCheckVertical(Vector2 checkPos) {
            RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, m_slopeCheckDistance, m_whatIsGround);
            if (hit) {
                m_slopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;

                m_slopeDownAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (Math.Abs(m_slopeDownAngle) > 0.00001) {
                    m_isOnSlope = true;
                }

                Debug.DrawRay(hit.point, hit.normal, Color.green);

                Debug.DrawRay(hit.point, m_slopeNormalPerp, Color.red);

            }

            if (m_isOnSlope && m_xMovement == 0.0f) m_rigidbody2D.sharedMaterial = m_fullFriction;
            else m_rigidbody2D.sharedMaterial = m_noFriction;
        }
        
        private void Interactions() {
            if (Input.GetKeyDown(KeyCode.E)) {
                if (m_nearChest) {
                    m_chest.Open();

                    //m_gold += m_chest.getGold();
                }

                if (m_nearLever) {
                    m_lever.HandleLever();
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.tag == "Lever" && !m_nearLever) {
                m_nearLever = true;
                m_lever = other.GetComponent<LeverInteraction>();
                m_lever.ChangeMaterial();
            }

            if (other.tag == "Chest" && !m_nearChest) {
                m_nearChest = true;
                m_chest = other.GetComponent<ChestInteraction>();
                m_chest.ChangeMaterial();
            }
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (other.tag == "Lever" && m_lever != null) {
                m_lever.ChangeMaterial();
                m_nearLever = false;
                m_lever = null;
            }

            if (other.tag == "Chest" && m_chest != null) {
                m_chest.ChangeMaterial();
                m_nearChest = false;
                m_chest = null;
            }
        }

        public void TakeDamage(int damage) {
            if(!m_canGetDamage)
            {
                return;
            }
            m_currentHealth -= damage;
            m_canRegainHealth = false;
            m_canGetDamage = false;
            Debug.Log("Current health: " + m_currentHealth);
            m_healthBar.SetHealth((int)m_currentHealth);

            if (m_currentHealth <= 0) {
                m_currentHealth = m_maxHealth;

                //transform.position = gm.m_lastCheckPoint;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                //if (m_state != State.Die) {
                //    m_animator.SetTrigger("Die");
                //    Destroy(gameObject, 5f / 6f);
                //}
            }
        }

        public void Shoot()
        {
            m_projectile.GetComponent<Projectile>().SetParent(Projectile.Parent.Player);
            Instantiate(m_projectile, m_firepoint.position, m_firepoint.rotation);
        }

        private IEnumerator CantTakeDamageWait()
        {
            m_canGetDamage = true;
            while (true)
            {
                yield return new WaitForSeconds(0.01f);
                if (!m_canGetDamage)
                {
                    yield return new WaitForSeconds(m_cantGetDamageTimer);
                    m_canGetDamage = true;
                }
            }
        }

        private void UpdateState() {
            AnimatorClipInfo[] currentState = m_animator.GetCurrentAnimatorClipInfo(0);
            string state = currentState[0].clip.name;
            switch (state) {
                case "run":
                    m_state = State.Walk;
                    break;
                case "jump":
                    m_state = State.Jump;
                    break;
                case "fall":
                    m_state = State.Jump;
                    break;
                case "attack":
                    m_state = State.Attack;
                    break;
                case "die":
                    m_state = State.Die;
                    break;
                default:
                    m_state = State.Idle;
                    break;
            }
        }
        
        private void OnDrawGizmos() {
            Gizmos.DrawWireSphere(m_groundCheck.position, m_groundedRadius);
        }

        private void Flip() {
            m_facingRight = !m_facingRight;
            transform.Rotate(0f, 180f, 0f);
        }
    }
}
   