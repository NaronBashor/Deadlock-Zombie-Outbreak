using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public AnimatorOverrideController womanOverrideController;
    public Sprite maleImage;
    public Sprite femaleImage;

    public TextMeshProUGUI playerNameText;

    public Image sprite;
    public Image sprite2;

    public float raycastDistance;
    public LayerMask wallLayerMask;

    private Animator anim;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lookInput;

    public GameObject pauseMenu;

    public Camera cam;
    public Collider2D playerCollider;

    private bool isRightStickActive = false;
    private bool isInputEnabled = true;

    private WeaponManager weaponManager;
    private InventoryManager inventoryManager;

    private bool canAttack = true;

    PlayerControls controls;

    private void Awake()
    {
        playerNameText.text = GameSettings.playerName;
        anim = GetComponent<Animator>();
        if (GameSettings.characterChosen == 1) {
            //Debug.Log("Female chosen.");
            sprite.sprite = femaleImage;
            sprite2.sprite = femaleImage;
            anim.runtimeAnimatorController = womanOverrideController;

        } else {
            //Debug.Log("Male chosen.");
            sprite.sprite = maleImage;
            sprite2.sprite = maleImage;
        }

        rb = GetComponent<Rigidbody2D>();
        weaponManager = GetComponent<WeaponManager>();
        inventoryManager = GetComponentInChildren<InventoryManager>();

        controls = new PlayerControls();

        cam = FindAnyObjectByType<Camera>();
    }

    private void OnEnable()
    {
        controls.Player.Enable();
        controls.Player.Pause.performed += TogglePauseMenu;
    }
    private void OnDisable()
    {
        controls.Player.Disable();
        controls.Player.Pause.performed -= TogglePauseMenu;
    }

    private void Start()
    {
        inventoryManager.Initialize(this);
        pauseMenu.SetActive(false);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!isInputEnabled) return;

        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        if (!isInputEnabled) return;

        lookInput = context.ReadValue<Vector2>();
        isRightStickActive = lookInput.sqrMagnitude >= 0.1f;
    }


    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!isInputEnabled || inventoryManager.IsInventoryOpen || !canAttack) return;

        // Check if the attack button is pressed or released
        if (weaponManager != null) {
            if (context.phase == InputActionPhase.Performed) {
                // Button is pressed
                weaponManager.Attack(true);
            } else if (context.phase == InputActionPhase.Canceled) {
                // Button is released
                weaponManager.Attack(false);
            }
        }
    }


    public void SetInputEnabled(bool isEnabled)
    {
        isInputEnabled = isEnabled;

        if (!isEnabled) {
            moveInput = Vector2.zero;
            rb.velocity = Vector2.zero;
            anim.SetBool("isMoving", false);
        }
    }

    private void Update()
    {
        if (!isInputEnabled) return;

        // Update movement animations
        anim.SetBool("isMoving", moveInput.magnitude > 0);

        RaycastWallChecker();

        if (SoundManager.Instance != null && moveInput.magnitude > 0.1f) {
            SoundManager.Instance.StartWalking();
        } else {
            SoundManager.Instance.StopWalking();
        }

        // Camera follow player with clamping
        Vector3 targetPosition = new Vector3(transform.position.x, transform.position.y, -10);

        // Clamp the camera position to stay within the specified boundaries
        targetPosition.x = Mathf.Clamp(targetPosition.x, 3f, 242f);
        targetPosition.y = Mathf.Clamp(targetPosition.y, 76f, 184f);

        cam.transform.position = targetPosition;
    }


    private void FixedUpdate()
    {
        if (!isInputEnabled) return;

        // Move the player
        rb.velocity = moveInput * GameSettings.playerMoveSpeed;

        // Rotate the player based on the input type
        if (GameSettings.usingController && isRightStickActive) {
            RotateTowardsDirection(lookInput); // Rotate towards the right stick direction if it's being used
        } else if (!GameSettings.usingController) {
            RotateTowardsMouse(); // Rotate towards the mouse position if using keyboard/mouse
        } else {
            RotateTowardsDirection(moveInput); // Default to face the movement direction
        }
    }


    private void RotateTowardsDirection(Vector2 direction)
    {
        if (direction.magnitude <= 0.1f) return; // Ignore small inputs

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 270f;
        rb.rotation = Mathf.LerpAngle(rb.rotation, angle, Time.deltaTime * GameSettings.rotationSpeed);
    }

    private void RotateTowardsMouse()
    {
        // Get the mouse position in world space
        Vector3 mousePosition = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        // Calculate the direction from the player to the mouse
        Vector2 direction = (mousePosition - transform.position).normalized;

        RotateTowardsDirection(direction);
    }

    private void TogglePauseMenu(InputAction.CallbackContext context)
    {
        if (context.performed && pauseMenu.activeSelf) {
            pauseMenu.SetActive(false);
            Time.timeScale = 1f;
        } else {
            pauseMenu.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    private void RaycastWallChecker()
    {
        // Calculate the direction based on the player's rotation
        Vector2 direction = new Vector2(Mathf.Cos((rb.rotation + 270) * Mathf.Deg2Rad), Mathf.Sin((rb.rotation + 270) * Mathf.Deg2Rad));

        // Perform the raycast in the direction the player is facing
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, raycastDistance, wallLayerMask);

        // Debug draw for the raycast direction
        if (hit) {
            Debug.DrawRay(transform.position, direction * raycastDistance, Color.red);
            canAttack = false;
        } else {
            Debug.DrawRay(transform.position, direction * raycastDistance, Color.green);
            canAttack = true;
        }
    }

}