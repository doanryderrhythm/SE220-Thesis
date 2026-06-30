using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using static Gun;

public class PlayerController : MonoBehaviour
{
    private Animator animator;

    [SerializeField] InputActionReference playerRunInput;
    [SerializeField] InputActionReference playerJumpInput;

    [SerializeField] ParticleSystem hurtParticles;
    [SerializeField] ParticleSystem deadParticles;

    void OnEnable()
    {
        playerRunInput.action.Enable();
        playerJumpInput.action.Enable();

        GameEvent.OnGunPicked += PickUpGun;
        GameEvent.OnGunDropped += DropGun;
        GameEvent.OnGunPowerUpCollected += CollectGunPowerUp;
        GameEvent.OnPlayerPowerUpCollected += CollectPlayerPowerUp;
    }

    void OnDisable()
    {
        playerRunInput.action.Disable();
        playerJumpInput.action.Disable();

        GameEvent.OnGunPicked -= PickUpGun;
        GameEvent.OnGunDropped -= DropGun;
        GameEvent.OnGunPowerUpCollected -= CollectGunPowerUp;
        GameEvent.OnPlayerPowerUpCollected -= CollectPlayerPowerUp;
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

   private TowerBuilder towerBuilder;
    private bool IsInBuildMode => towerBuilder != null && towerBuilder.IsBuildMode;

    private bool isOnPlacementPoint = false;
    private PlacementPoint placementPoint = null;

    void ManageMove()
    {
          if (IsInBuildMode)
        {
            rb.linearVelocityX = 0f;
            ChangeColliderShape(0f);
            return;
        }
        float confirmedXVelocity = moveRate * ValueStorer.moveSpeed;
        rb.linearVelocityX = confirmedXVelocity;

        if (confirmedXVelocity != 0f && wasGrounded)
            AudioManager.Instance.PlayLoopSFX(AudioManager.Instance.platformerPlayerMoveSound, 0.25f);
        else
            AudioManager.Instance.StopLoopSFX();

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
          if (IsInBuildMode)
        {
            rb.linearVelocityX = 0f;
            ChangeColliderShape(0f);
            return;
        }
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

            AudioManager.Instance.InstantiateSFX(AudioManager.Instance.platformerPlayerJumpSound, 0.4f);

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

        bool legacyGrounded = wasGrounded;
        wasGrounded = hitDetected;

        if (wasGrounded && legacyGrounded != wasGrounded)
            AudioManager.Instance.InstantiateSFX(AudioManager.Instance.platformerPlayerLandSound, 2f);
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
          towerBuilder = GetComponent<TowerBuilder>();
    }

    [SerializeField] bool isGunEquipped = false;
    [SerializeField] GunStats gunStats;

    [SerializeField] GunType gunType;
    [SerializeField] float shootSpeed;
    [SerializeField] float shootRate;
    [SerializeField] GameObject bulletPrefab;

    void Start()
    {
        GameManager.Instance.player = this;

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
        if (GameManager.Instance.isLevelFinished)
            return;

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

        //if (IsInBuildMode && isOnPlacementPoint)
        //{
        //    if (Keyboard.current != null)
        //    {
        //        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        //            towerBuilder.PlaceTower(TowerType.Normal, placementPoint);
        //        else if (Keyboard.current.digit2Key.wasPressedThisFrame)
        //            towerBuilder.PlaceTower(TowerType.Piercing, placementPoint);
        //        else if (Keyboard.current.digit3Key.wasPressedThisFrame)
        //            towerBuilder.PlaceTower(TowerType.Sniper, placementPoint);
        //    }
        //}

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
if (IsInBuildMode)
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
                    ShootBullet();
                }
            }

        }
        ManageJump();

        if (Keyboard.current != null)
        {   
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
                GameEvent.OnPaused.Invoke();
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
                InstantiateBullet(barrel.position - new Vector3(-0.05f, 0, 0));
                InstantiateBullet(barrel.position - new Vector3(0.05f, 0, 0));
            }
            else if (direction.y >= -0.2f && direction.y <= 0.2f)
            {
                InstantiateBullet(barrel.position - new Vector3(0, -0.05f, 0));
                InstantiateBullet(barrel.position - new Vector3(0, 0.05f, 0));
            }
        }

        AudioManager.Instance.InstantiateSFX(AudioManager.Instance.platformerPlayerShootSound, 0.25f);
    }

    void InstantiateBullet(Vector3 pos)
    {
        GameObject bulletObject = Instantiate(bulletPrefab, pos, Quaternion.identity);
        Bullet bullet = bulletObject.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.speed = shootSpeed;
            bullet.direction = (pos - transform.position).normalized;
            bullet.damage += damageBonus;
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

    [SerializeField] private float health = ValueStorer.defaultPlayerHealth;

   public void HurtPlayer(float value)
    {
        if (isInvulnerable || GameManager.Instance.isLevelFinished)
            return;

        isInvulnerable = true;

        health -= value;
        if (health > 0)
            Instantiate(hurtParticles.gameObject, transform.position, Quaternion.identity);

        animator.Play("player_hurt");
        Debug.Log(health);

        if (health <= 0)
        {
            DestroyPlayer();
            return;
        }

        AudioManager.Instance.InstantiateSFX(AudioManager.Instance.platformerPlayerDamageSound);
    }

    public void DestroyPlayer()
    {
        isDead = true;
        Instantiate(deadParticles.gameObject, transform.position, Quaternion.identity);
        AudioManager.Instance.InstantiateSFX(AudioManager.Instance.platformerPlayerDeadSound);
        GameEvent.OnGameLost.Invoke();
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
        UpdateGunStats();

        Debug.Log($"Gun Type: {gunType}, Shoot Speed: {shootSpeed}, Shoot Rate: {shootRate}");
    }

    bool isDead = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (LayerMask.LayerToName(collision.gameObject.layer) == ValueStorer.placementPointLM)
        {
            isOnPlacementPoint = true;
            if (collision.TryGetComponent<PlacementPoint>(out PlacementPoint point))
            {
                towerBuilder.SetPlacementPoint(point);
            }
        }
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
        if (LayerMask.LayerToName(collision.gameObject.layer) == ValueStorer.placementPointLM)
        {
            isOnPlacementPoint = false;
            if (collision.TryGetComponent<PlacementPoint>(out PlacementPoint point))
            {
                towerBuilder.SetPlacementPoint(null);
            }
        }
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
    [SerializeField] float shootSpeedBonus = 0f;
    [SerializeField] float shootRateBonus = 0f;
    [SerializeField] float damageBonus = 0f;
    void CollectGunPowerUp(float value)
    {
        shootSpeedBonus += value;
        shootRateBonus += value;
        damageBonus += value;

        UpdateGunStats();
        Debug.Log($"Shoot Speed: {shootSpeed}; Shoot Rate: {shootRate}");
    }

    void CollectPlayerPowerUp(float value)
    {
        health += value;
        Debug.Log($"Health: {health}");
    } 

    void UpdateGunStats()
    {
        if (gunStats == null)
        {
            return;
        }
        shootSpeed = gunStats.shootSpeed + shootSpeedBonus;

        shootRate = Mathf.Max(
            0.05f,
            gunStats.shootRate - shootRateBonus
        );
    }
}