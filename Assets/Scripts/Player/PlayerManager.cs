using Assets.Scripts.Player;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour, IDamageable
{
    public Rigidbody2D RigidBody => GetComponent<Rigidbody2D>();
    public static PlayerManager Instance;
    //public AudioManager Audio;
    //public Collision2D lastCollision;

    #region General
    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); }
        if (Instance == null) { Instance = this; }
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        //Audio = AudioManager.Instance;
        Interact = new();
        ResetPlayer();
        RigidBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        SetInput();
    }
    private void FixedUpdate() //Executing physics simulation
    {
        //Movement
        HandleCollisions();
        Move();
        HandleGravity();
        ApplyMovement();

        //Attack
        //HandleAttack();

        //Health
        HealthChecks();
        HandleInvincibility();
    }
    private void Update() //Getting Player Input
    {
        UpdateInput();
    }
    #endregion

    #region PlayerManagement
    [Header("INPUT")]
    public float VerticalDeadZoneThreshold = .1f; //Determines the minimum velocity for the character to move vertically
    public float HorizontalDeadZoneThreshold = .1f; //Determines the minimum velocity for the character to move horizontaly
    public float TapThreshold = .35f; //Determines the difference between a tap and a hold of a button in time

    private PlayerInput JumpInput;
    private PlayerInput InteractInput;
    private PlayerInput PauseInput;
    private PlayerInput DebugInput;
    private PlayerInput HealInput;
    //private PlayerInput AttackInput;

    public void ResetPlayer()
    {
        //ResetBoosts();
        hp = maxHp;
        mana = maxMana;
        invincible = false;
        movementDisable = false;
        //maxCharms = baseCharms + unlockedCharms;
    }
    //public void ResetBoosts()
    //{
    //    maxHp = baseHp + hpAdd;
    //    maxMana = baseMana + manaAdd;
    //}
    private void SetInput()
    {
        JumpInput = new(KeyCode.Space, MaxJumpTime);
        JumpInput.OnDown.AddListener(Jump);
        JumpInput.OnHold.AddListener(JumpSustain);
        JumpInput.OnUp.AddListener(() => jumpSustainable = false);
        //JumpInput.OnMaxReached.AddListener(() => jumpSustainable = false);
        //JumpInput.OnMaxReached.AddListener(() => Debug.Log("Max reached"));
        JumpInput.OnUp.AddListener(() => endedJump = true);
        //JumpInput.OnMaxReached.AddListener(() => endedJump = true);

        InteractInput = new(KeyCode.DownArrow, MaxJumpTime);
        InteractInput.OnDown.AddListener(Interact.Invoke);

        PauseInput = new(KeyCode.Escape, 0.1f);
        PauseInput.OnDown.AddListener(Pause);

        DebugInput = new(KeyCode.O, 0.1f);
        DebugInput.OnDown.AddListener(() => Debug.Log("Getting debug input"));
        DebugInput.OnDown.AddListener(() => TakeDamage(1));

        HealInput = new(KeyCode.A, healChargeTime);
        HealInput.OnMaxReached.AddListener(HealAbility);
    }
    private void UpdateInput()
    {
        JumpInput.Update();
        InteractInput.Update();
        PauseInput.Update();
        DebugInput.Update();
        HealInput.Update();
        //if (IsStanding && HasBufferedJump) { Jump(); }
        MoveDirection = Input.GetKey(KeyCode.RightArrow) ? 1 : Input.GetKey(KeyCode.LeftArrow) ? -1 : 0;
    }
    #endregion

    #region Health
    [Header("HEALTH")]
    public int maxHp = 5;
    //private int baseHp = 5;
    public bool Alive => hp > 0;
    public int hp;

    public float invincibleDuration;
    private bool invincible;
    private float damageTakenTime;

    [Header("HEALTH GRAPHICS")]
    private int hpDisplayed;
    public Image[] hpImages;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    //[HideInInspector] public bool canChangeScenes;

    public void Die()
    {
        movementDisable = true;

        GameRespawningState respawningState = new(GameManager.Instance.machine);
        GameManager.Instance.machine.ChangeState(respawningState);
    }
    public void TakeDamage(int damage)
    {
        if (invincible) return; //Eventually add visual marker for user

        //if (def > 0)
        //{
        //    if (def <= damage)
        //    {
        //        damage -= def;
        //        def = 0;
        //    }
        //    if (def > damage)
        //    {
        //        def -= damage;
        //        damage = 0;
        //    }
        //}
        Debug.Log($"Previous Hp is {hp}");
        hp -= damage;
        Debug.Log($"Taking damage {damage}");
        Debug.Log($"Hp is {hp}");
        if (damage > 0 && hp > 0)
        {
            damageTakenTime = Time.time;
            invincible = true;
        }
    }
    private void HandleInvincibility()
    {
        if (invincible && damageTakenTime + invincibleDuration > Time.unscaledTime)
        {
            Time.timeScale = 0.5f;
        }
        else
        {
            invincible = false;
            Time.timeScale = 1.0f;
            damageTakenTime = 0;
        }
    }
    private void HealthChecks()
    {
        CheckForHpGraphics();
        CheckForMaxValues();
        if (hp <= 0) { Die(); }
        void CheckForHpGraphics()
        {
            for (int i = 0; i < hpImages.Length; i++)
            {
                if (i < hp) { hpImages[i].sprite = fullHeart; }
                else { hpImages[i].sprite = emptyHeart; }

                if (i < hpDisplayed) { hpImages[i].enabled = true; }
                else { hpImages[i].enabled = false; }
            }
            //for (int j = maxHp + 1; j < hpImages.Length; j++)
            //{
            //    if (j < def) { hpImages[j].sprite = fullShield; }
            //    else { hpImages[j].sprite = emptyShield; }

            //    if (j < defDisplayed) { hpImages[j].enabled = true; }
            //    else { hpImages[j].enabled = false; }
            //}
        }
        void CheckForMaxValues()
        {
            if (hp > maxHp) { hp = maxHp; }
            hpDisplayed = maxHp;
        }

    }
    public void Heal(int healAmount)
    {
        Debug.Log($"Healing by {healAmount}");
        if (hp >= maxHp) return;
        hp += healAmount;
    }
    #endregion

    #region Movement
    [Header("MOVEMENT")]
    public float MaxSpeed;
    public float Acceleration;
    public float GroundDeceleration;
    public float AirDeceleration;
    public float GroundingForce;

    [Header("JUMP")]
    public int JumpPower;
    public int JumpSustainPower;
    public float MaxJumpTime;
    public float MaxFallSpeed;
    public float FallAcceleration;
    public float JumpEndGModifier;
    public float CoyoteTime;
    public float JumpBuffer;
    public float ApexSpeedModifier;

    private int MoveDirection;
    private bool groundHit, ceilingHit;
    private bool IsStanding, IsJumping;
    private Vector2 tempVelocity;
    private LayerMask Ground => LayerMask.GetMask("Ground");
    public bool movementDisable;

    private bool endedJump;
    private bool coyoteUsable;
    private bool jumpSustainable;
    private bool ApexHit;
    private float timeGroundLeft;

    private bool HasCoyoteTime => coyoteUsable && !IsStanding && Time.time <= timeGroundLeft + CoyoteTime;

    void HandleCollisions()
    {
        Physics2D.queriesStartInColliders = false;

        BoxCollider2D groundcheck = transform.GetChild(0).gameObject.GetComponent<BoxCollider2D>();
        BoxCollider2D ceilingcheck = transform.GetChild(1).gameObject.GetComponent<BoxCollider2D>();
        groundHit = groundcheck.IsTouchingLayers(Ground);
        ceilingHit = ceilingcheck.IsTouchingLayers(Ground);

        if (!IsStanding && groundHit)
        {
            IsStanding = true;
            coyoteUsable = true;
        }
        if (IsStanding && !groundHit)
        {
            IsStanding = false;
            timeGroundLeft = Time.time;
        }
        if (ceilingHit)
        {
            tempVelocity.y = Mathf.Min(0, tempVelocity.y);
            jumpSustainable = false;
        }
    }

    private void Jump()
    {
        if (IsStanding || HasCoyoteTime)
        {
            coyoteUsable = false;
            IsJumping = true;
            tempVelocity.y = JumpPower;
            endedJump = false;
            jumpSustainable = true;
        }
    }
    private void JumpSustain()
    {
        if (!jumpSustainable || JumpInput.timePressed + MaxJumpTime < Time.time) return;
        coyoteUsable = false;
        IsJumping = true;
        if (tempVelocity.y <= JumpSustainPower) tempVelocity.y = JumpSustainPower;
    }
    void Move()
    {
        if (MoveDirection != 0 && (MoveDirection == 1 && transform.localRotation.y != 0) || (MoveDirection == -1 && transform.localRotation.y != 180))
        {
            int angle = MoveDirection == 1 ? 0 : 180;
            transform.localRotation = Quaternion.Euler(transform.localRotation.x, angle, transform.rotation.z);
        }

        if (MoveDirection == 0)
        {
            var deceleration = IsStanding ? GroundDeceleration : AirDeceleration;
            tempVelocity.x = Mathf.MoveTowards(tempVelocity.x, 0, deceleration * Time.fixedDeltaTime);
        }
        else
        {
            tempVelocity.x = Mathf.MoveTowards(tempVelocity.x, MoveDirection * MaxSpeed, Acceleration * Time.fixedDeltaTime);
        }

        if (IsJumping && tempVelocity.y < 1)
        {
            tempVelocity.x *= ApexSpeedModifier;
            IsJumping = false;
            endedJump = true;
            ApexHit = true;
        }
        if (!(ApexHit && (tempVelocity.x > MaxSpeed || tempVelocity.x < -MaxSpeed)))
        {
            tempVelocity.x = Mathf.Clamp(tempVelocity.x, -MaxSpeed, MaxSpeed);
            ApexHit = false;
        }
    }
    private void HandleGravity()
    {
        if (IsStanding && tempVelocity.y <= VerticalDeadZoneThreshold)
        {
            tempVelocity.y = GroundingForce;
        }
        else
        {
            var inAirGravity = FallAcceleration;
            if (endedJump) inAirGravity *= JumpEndGModifier;

            tempVelocity.y = Mathf.MoveTowards(tempVelocity.y, -MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
        }
    }
    void ApplyMovement()
    {
        RigidBody.velocity = tempVelocity;
    }

    #endregion

    #region Interact
    public UnityEvent Interact;

    private void Pause()
    {
        GameStatemachine machine = GameManager.Instance.machine;
        GamePausedState pausedState = new(machine);
        if (machine.CurrentState is Level) machine.ChangeState(pausedState);
        else machine.ChangeState(machine.PreviousState);
    }

    #endregion

    #region Attacking
    [Header("ATTACK")]
    public float attackDelay = .2f; //determines the time interval before attack can be used again
    public float attackReach; //determines how far the attack reaches (in case of adding other weapons)
    public int damage = 1;
    public float pushbackForce = 10; //by what force will the character be pushed back from the direction of the attack

    private bool attackUsable; //checks whether attack isn't blocked by other actions
    private bool attackDisable; //debug switch to disable attacks 

    private float attackTime; //the time when attack action happened
    private bool attackBuffer; //queues another attack if the button was pressed, but the player is doing another action
    //private bool attacking; //This is never used, but I'm keeping it here in the case I need it when blocking other actions and abilites during attack

    //private void HandleAttack()
    //{
    //    attackTime = Time.time;
    //    attackUsable = false;

    //    Vector2 boxOrigin = new(Collider.bounds.center.x + (Collider.bounds.max.x + attackReach) / 2 * MoveDirection, Collider.bounds.center.y);
    //    Vector2 boxSize = new(attackReach / 2, Collider.bounds.max.y / 2);
    //    Vector2 boxDirection = new(MoveDirection, 0);
    //    LayerMask player = LayerMask.GetMask("Player");
    //    Transform enemyTransform = null;

    //    RaycastHit2D[] hitObjects = Physics2D.BoxCastAll(boxOrigin, boxSize, 0f, boxDirection, 0.1f, ~player);
    //    foreach (RaycastHit2D hit in hitObjects)
    //    {
    //        if (hit.transform.gameObject.GetComponent<IDamageable>() != null)
    //        {
    //            IDamageable damaged = hit.transform.gameObject.GetComponent<IDamageable>();
    //            damaged.TakeDamage(damage);
    //            if (hit.transform.CompareTag("Enemy")) enemyTransform = hit.transform;
    //        }
    //    }
    //    if (enemyTransform != null)
    //    {
    //        Vector2 bounce = (transform.position - enemyTransform.position).normalized;
    //        RigidBody.AddForce(bounce * pushbackForce, ForceMode2D.Impulse);
    //    }

    //}



    #endregion

    #region Abilities
    [Header("HEAL")]
    public float healChargeTime = 1f;
    public float healManaCost = 30;

    public void HealAbility()
    {
        if (hp >= maxHp) return;

        Heal(1);
        mana -= healManaCost;
    }

    #endregion

    #region Magic
    [Header("MAGIC")]
    public float mana = 100;
    public float maxMana = 100;
    private int baseMana = 100;
    public float mAtk;

    //public void tmpRegainMana()
    //{
    //    if (Time.time >= timeDownArrowUp + 1 && mana != maxMana)
    //    {
    //        mana += 10;
    //    }
    //}

    #endregion

    #region Inventory
    public Inventory Inventory;

    #region Charms
    //public int baseCharms = 3;
    //public int unlockedCharms = 0;
    //public int maxCharms;
    ////public List<Charm> equippedCharms;

    //public int hpAdd;
    //public int defAdd;
    //public int manaAdd;
    //public int speedAdd;
    #endregion
    #endregion
}

