using UnityEngine;
using UnityEngine.InputSystem;
using static Gun;

public class PlayerController : MonoBehaviour
{
    [SerializeField] InputActionReference playerRunInput;
    [SerializeField] InputActionReference playerJumpInput;

    void OnEnable()
    {
        playerRunInput.action.Enable();
        playerJumpInput.action.Enable();
    }

    void OnDisable()
    {
        playerRunInput.action.Disable();
        playerJumpInput.action.Disable();
    }

    [SerializeField] Rigidbody2D rb;
    [SerializeField] BoxCollider2D bc;
    private int jumpCount = 2;
    private bool wasGrounded = false;

    private float moveRate = 0f;

    //Landing Detection
    [SerializeField] private float maxDistance = 0.1f;
    [SerializeField] Collider2D boxCol;
    private RaycastHit2D hit;
    private bool hitDetected;
    private bool leaveDetected;

    //Buffer
    private float jumpBufferTime;
    private float coyoteTime;

    void ManageMove()
    {
        float confirmedXVelocity = moveRate * ValueStorer.moveSpeed;
        rb.linearVelocityX = confirmedXVelocity;

        ChangeColliderShape(moveRate);
    }

    void ManageLeave()
    {
        if (!hitDetected && !leaveDetected)
        {
            transform.SetParent(null);
            rb.gravityScale = ValueStorer.gravityGround;
            coyoteTime -= Time.deltaTime;
            if (coyoteTime <= 0)
            {
                jumpCount -= 1;
                leaveDetected = true;
            }
        }
    }

    void ManageJump()
    {
        if (playerJumpInput.action.WasPressedThisFrame())
        {
            jumpBufferTime = ValueStorer.bufferTime;
        }
        else
        {
            jumpBufferTime -= Time.deltaTime;
        }

        if (jumpBufferTime >= 0 && jumpCount > 0)
        {
            rb.gravityScale = ValueStorer.gravityGround;
            transform.SetParent(null);

            rb.linearVelocityY = 0;
            rb.AddForceY(ValueStorer.jumpHeight, ForceMode2D.Impulse);
            jumpCount -= 1;
            leaveDetected = true;

            jumpBufferTime = 0;
        }
        else if (playerJumpInput.action.WasReleasedThisFrame() && rb.linearVelocityY > 0)
        {
            rb.AddForceY(-ValueStorer.lightPush, ForceMode2D.Impulse);
        }
    }

    void ManageLand()
    {
        Vector2 halfExtents = boxCol.bounds.extents;
        Vector2 origin = boxCol.bounds.center;
        Vector2 direction = Vector2.down;

        hit = Physics2D.BoxCast(origin, halfExtents, 0f, Vector2.down, maxDistance, LayerMask.GetMask(ValueStorer.groundLM));
        hitDetected = hit.collider != null;

        if (hitDetected && !wasGrounded)
        {
            coyoteTime = ValueStorer.coyoteTime;

            leaveDetected = false;
            jumpCount = ValueStorer.maxJumpCount;
        }

        wasGrounded = hitDetected;
    }

    void ChangeColliderShape(float moveRate)
    {
        if (moveRate != 0f)
        {
            bc.edgeRadius = ValueStorer.sizeRadiusMoving;
            bc.size = ValueStorer.colliderSizeMoving;
        }
        else
        {
            bc.edgeRadius = ValueStorer.sizeRadiusStill;
            bc.size = ValueStorer.colliderSizeStill;
        }
    }

    void Awake()
    {
        jumpCount = ValueStorer.maxJumpCount;

        jumpBufferTime = 0;
        coyoteTime = ValueStorer.coyoteTime;
    }

    [SerializeField] bool isGunEquipped = false;
    [SerializeField] GunStats gunStats;

    [SerializeField] GunType gunType;
    [SerializeField] float shootSpeed;
    [SerializeField] float shootRate;
    [SerializeField] GameObject bulletPrefab;

    void Start()
    {

    }

    [SerializeField] Transform sideBarrel;
    [SerializeField] Transform upBarrel;
    [SerializeField] Transform downBarrel;

    private float directionRotation = 0f;
    private float totalShootingTime = 0f;

    void Update()
    {
        moveRate = playerRunInput.action.ReadValue<float>();
        if (playerRunInput.action.IsPressed())
        {
            if (moveRate >= 0f) directionRotation = 0f;
            else directionRotation = 180f;
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, directionRotation, transform.eulerAngles.z);
        }

        if (isGunEquipped)
        {
            totalShootingTime += Time.deltaTime;
            if (totalShootingTime >= shootRate)
            {
                totalShootingTime -= shootRate;
                Transform barrel = GetBarrel();
                GameObject bulletObject = Instantiate(bulletPrefab, barrel.position, Quaternion.identity);
                Bullet bullet = bulletObject.GetComponent<Bullet>();
                if (bullet != null)
                {
                    bullet.speed = shootSpeed;
                    bullet.direction = (barrel.position - transform.position).normalized;
                }
            }
        }

        ManageJump();

        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            DestroyPlayer();
        }
    }

    Transform GetBarrel()
    {
        if (!gunStats)
        {
            return sideBarrel;
        }

        switch (gunStats.gunType)
        {
            case GunType.GUN_SIDE: return sideBarrel;
            case GunType.GUN_DOWN: return downBarrel;
            case GunType.GUN_UP: return upBarrel;
        }
        return sideBarrel;
    }

    void FixedUpdate()
    {
        ManageMove();
        ManageLeave();
        ManageLand();
    }

    void DestroyPlayer()
    {

    }

    void BeginShooting()
    {
        if (!gunStats)
        {
            return;
        }

        isGunEquipped = true;
        gunType = gunStats.gunType;
        shootSpeed = gunStats.shootSpeed;
        shootRate = gunStats.shootRate;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(ValueStorer.gunTag))
        {
            Gun gun = collision.GetComponent<Gun>();
            if (gun != null)
            {
                gunStats = gun.GetStats();
                BeginShooting();
                Destroy(gun.gameObject);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {

    }

    private void OnTriggerExit2D(Collider2D collision)
    {

    }
}