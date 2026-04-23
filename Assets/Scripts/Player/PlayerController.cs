using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using static Gun;

public class PlayerController : MonoBehaviour
{
    private Animator animator;

    [SerializeField] InputActionReference playerRunInput;
    [SerializeField] InputActionReference playerJumpInput;

    void OnEnable()
    {
        playerRunInput.action.Enable();
        playerJumpInput.action.Enable();

        GameEvent.OnGunPicked += PickUpGun;
        GameEvent.OnGunDropped += DropGun;
        GameEvent.OnCoinCollected += CollectCoin;
    }

    void OnDisable()
    {
        playerRunInput.action.Disable();
        playerJumpInput.action.Disable();

        GameEvent.OnGunPicked -= PickUpGun;
        GameEvent.OnGunDropped -= DropGun;
        GameEvent.OnCoinCollected -= CollectCoin;
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

            ShootBullet(true);
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
        animator = GetComponent<Animator>();
        animator.Play("player_normal");
    }

    [SerializeField] Transform sideBarrel;
    [SerializeField] Transform upBarrel;
    [SerializeField] Transform downBarrel;

    private float directionRotation = 0f;
    private float totalShootingTime = 0f;

    bool isInvulnerable = false;
    float invulnerableTime = ValueStorer.playerInvulnerableTime;

    void Update()
    {
        if (isInvulnerable)
        {
            invulnerableTime -= Time.deltaTime;
            if (invulnerableTime <= 0f)
            {
                isInvulnerable = false;
                invulnerableTime = ValueStorer.playerInvulnerableTime;

                animator.Play("player_normal");
            }
        }

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
                ShootBullet();
            }
        }

        ManageJump();

        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            DestroyPlayer();
        }
    }

    void ShootBullet(bool isMultiple = false)
    {
        if (gunStats == null)
            return;

        Transform barrel = GetBarrel();
        Vector2 direction = (barrel.position - transform.position).normalized;

        InstantiateBullet(barrel.position);
        if (isMultiple)
        {
            if (direction.x >= -0.2f && direction.x <= 0.2f)
            {
                InstantiateBullet(barrel.position - new Vector3(-0.2f, 0, 0));
                InstantiateBullet(barrel.position - new Vector3(-0.1f, 0, 0));
                InstantiateBullet(barrel.position - new Vector3(0.1f, 0, 0));
                InstantiateBullet(barrel.position - new Vector3(0.2f, 0, 0));
            }
            else if (direction.y >= -0.2f && direction.y <= 0.2f)
            {
                InstantiateBullet(barrel.position - new Vector3(0, -0.2f, 0));
                InstantiateBullet(barrel.position - new Vector3(0, -0.1f, 0));
                InstantiateBullet(barrel.position - new Vector3(0, 0.1f, 0));
                InstantiateBullet(barrel.position - new Vector3(0, 0.2f, 0));
            }
        }
    }

    void InstantiateBullet(Vector3 pos)
    {
        GameObject bulletObject = Instantiate(bulletPrefab, pos, Quaternion.identity);
        Bullet bullet = bulletObject.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.speed = shootSpeed;
            bullet.direction = (pos - transform.position).normalized;
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

    private float health = ValueStorer.defaultPlayerHealth;

    void HurtPlayer(float value)
    {
        if (isInvulnerable)
            return;

        isInvulnerable = true;

        health -= value;
        animator.Play("player_hurt");
        Debug.Log(health);

        if (health <= 0)
            DestroyPlayer();
    }

    void DestroyPlayer()
    {
        isDead = true;
        if (GameManager.Instance)
        {
            GameManager.Instance.StartCoroutine(GameManager.Instance.RespawnPlayer());
        }
        Destroy(gameObject);
    }

    void BeginShooting()
    {
        if (!gunStats)
        {
            return;
        }

        isGunEquipped = true;
        gunType = gunStats.gunType;
        shootSpeed = gunStats.shootSpeed + shootSpeedBonus;
        shootRate = gunStats.shootRate;

        Debug.Log($"Gun Type: {gunType}, Shoot Speed: {shootSpeed}, Shoot Rate: {shootRate}");
    }

    bool isDead = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        /*if (collision.gameObject.CompareTag(ValueStorer.gunTag))
        {
            Gun gun = collision.GetComponent<Gun>();
            if (gun != null)
            {
                gunStats = gun.GetStats();
                BeginShooting();
                Destroy(gun.gameObject);
            }
        }
        else*/ 
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (LayerMask.LayerToName(collision.gameObject.layer) == ValueStorer.enemyLM)
        {
            if (isDead || isInvulnerable)
                return;

            HarmData harmData = collision.GetComponent<HarmData>();
            if (!harmData)
                return;

            if (harmData.GetHarmStats().isInstaKill)
                DestroyPlayer();
            else
                HurtPlayer(harmData.GetHarmStats().damage);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {

    }

    void PickUpGun(Gun gun)
        {
            if (gunStats == null)
            {
                gunStats = gun.GetStats();
                BeginShooting();
            }
    }

    void DropGun()
    {
        isGunEquipped = false;
        gunStats = null;
        totalShootingTime = 0f;
    }

    [SerializeField] private float shootSpeedBonus = 0f;
    [SerializeField] private float damageBonus = 0f;

    void CollectCoin(float speed, float damage)
    {
        shootSpeedBonus += speed;
        damageBonus += damage;

        Debug.Log($"Collected Coin! Shoot Speed Bonus: {shootSpeedBonus}, Damage Bonus: {damageBonus}");
    }
}