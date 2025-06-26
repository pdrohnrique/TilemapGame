using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float acceleration = 15f;
    [SerializeField] private float deceleration = 20f;
    [SerializeField] private float friction = 5f;
    
    [Header("Pulo")]
    [SerializeField] private float jumpSpeed = 10f;
    [SerializeField] private float jumpShortSpeed = 5f;
    
    [Header("Configurações")]
    [SerializeField] private LayerMask chaoLayer;
    [SerializeField] private Transform peCheck;
    [SerializeField] private float raioChecagem = 0.2f;

    [Header("Animação")]
    [SerializeField] private Animator animator;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool noChao;
    private bool jump;
    private bool jumpCancel;
    private float movimentoX;
    private bool facingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (peCheck == null)
            Debug.LogError("Atribua o transform 'peCheck' no Inspector!");
    }

    void Update()
    {
        movimentoX = Input.GetAxisRaw("Horizontal");
    
        animator.SetFloat("xVelocity", Mathf.Abs(rb.velocity.x));
        animator.SetBool("IsJumping", !noChao);
        
        UpdateSpriteDirection();

        noChao = Physics2D.OverlapCircle(peCheck.position, raioChecagem, chaoLayer);

        if (Input.GetButtonDown("Jump") && noChao) jump = true;
        if (Input.GetButtonUp("Jump")) jumpCancel = true;
    }

    void FixedUpdate()
    {
        ApplyHorizontalMovement();
        HandleJump();
    }

    void ApplyHorizontalMovement()
    {
        float targetSpeed = movimentoX * maxSpeed;
        float speedDiff = targetSpeed - rb.velocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;

        float movement = Mathf.Pow(Mathf.Abs(speedDiff) * accelRate, 0.96f) * Mathf.Sign(speedDiff);
        rb.AddForce(movement * Vector2.right);

        if (Mathf.Abs(movimentoX) < 0.01f)
        {
            float frictionAmount = Mathf.Min(Mathf.Abs(rb.velocity.x), friction);
            frictionAmount *= Mathf.Sign(rb.velocity.x);
            rb.AddForce(-frictionAmount * Vector2.right, ForceMode2D.Impulse);
        }
    }

    void UpdateSpriteDirection()
    {
        if (movimentoX != 0)
            facingRight = movimentoX > 0;
        
        spriteRenderer.flipX = !facingRight;
    }

    void HandleJump()
    {
        if (jump)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
            jump = false;
        }

        if (jumpCancel)
        {
            if (rb.velocity.y > jumpShortSpeed)
                rb.velocity = new Vector2(rb.velocity.x, jumpShortSpeed);
            jumpCancel = false;
        }
    }

    public void ReiniciarDoCheckpoint()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnDrawGizmosSelected()
    {
        if (peCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(peCheck.position, raioChecagem);
        }
    }
}