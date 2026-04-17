using UnityEngine;
using UnityEngine.InputSystem;

public class ItemPickUp : MonoBehaviour
{
    [SerializeField] InputActionReference playerPickUpInput;
    [SerializeField] private Gun currentGun;
    [SerializeField] private Gun pendingGun;

    void OnEnable()
    {
        playerPickUpInput.action.Enable();
    }

    void OnDisable()
    {
        playerPickUpInput.action.Disable();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (playerPickUpInput.action.WasPressedThisFrame())
        {
            if (pendingGun != null)
            {
                if (currentGun != null)
                {
                    DropGun();
                }
                PickUpGun();
            }
            else if (currentGun != null)
            {
                DropGun();
            }
        }
    }

    void PickUpGun()
    {
        pendingGun.PickedUp(transform);
        currentGun = pendingGun;
        pendingGun = null;
        GameEvent.OnGunPicked?.Invoke(currentGun);
    }
    void DropGun()
    {
        currentGun.Dropped(transform);
        currentGun = null;
        GameEvent.OnGunDropped?.Invoke();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(ValueStorer.gunTag))
        {
            Gun gun = collision.GetComponent<Gun>();
            if (gun != null)
            {
                pendingGun = gun;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(ValueStorer.gunTag))
        {
            Gun gun = collision.GetComponent<Gun>();
            if (gun != null)
            {
                pendingGun = null;
            }
        }
    }
}
