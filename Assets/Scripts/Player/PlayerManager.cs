using Assets.Scripts.Player;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour, IDamageable
{
    public Rigidbody2D RigidBody => GetComponent<Rigidbody2D>();
    //public CapsuleCollider2D Collider => GetComponent<CapsuleCollider2D>();
    public static PlayerManager Instance;
    public AudioManager Audio;
    private Collision Collision;

    #region General
    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); }
        if (Instance == null) { Instance = this; }
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        Audio = AudioManager.Instance;
        ResetPlayer();
        RigidBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        SetInput();
    }
    private void FixedUpdate() //Executing physics simulation
    {
        //Movement
        HandleCollisions();
        //HandleY();
        Move();
        HandleGravity();
        ApplyMovement();

        //Attack
        //HandleAttack();

        //Health
        HealthChecks();
        //HandleInvincibility();
        //HandleRespawnPoint();
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
    //private PlayerInput AttackInput;
    private void OnCollisionStay(Collision collision)
    {
        Collision = collision;
    }

    public void ResetPlayer()
    {
        alive = true;
        ResetBoosts();
        hp = maxHp;
        //def = maxDef;
        mana = maxMana;
        invincible = false;
        movementDisable = false;
        maxCharms = baseCharms + unlockedCharms;
    }
    public void ResetBoosts()
    {
        maxHp = baseHp + hpAdd;
        //maxDef = baseDef + defAdd;
        maxMana = baseMana + manaAdd;
    }
    private void SetInput()
    {
        JumpInput = new(KeyCode.Space, MaxJumpTime);

        JumpInput.OnDown.AddListener(Jump);
        JumpInput.OnHold.AddListener(JumpSustain);
        JumpInput.OnUp.AddListener(() => endedJump = true);
        JumpInput.OnUp.AddListener(() => jumpSustainable = false);
        JumpInput.OnMaxReached.AddListener(() => jumpSustainable = false);
        JumpInput.OnMaxReached.AddListener(() => endedJump = true);

        InteractInput = new(KeyCode.DownArrow, MaxJumpTime);

        InteractInput.OnDown.AddListener(Interact);

        PauseInput = new(KeyCode.Escape, 0.1f);

        PauseInput.OnDown.AddListener(Pause);
    }
    private void UpdateInput()
    {
        JumpInput.Update();
        InteractInput.Update();
        if (IsStanding && HasBufferedJump) { Jump(); }
        MoveDirection = Input.GetKey(KeyCode.RightArrow) ? 1 : Input.GetKey(KeyCode.LeftArrow) ? -1 : 0;
    }
    #endregion

    #region Health
    [Header("HEALTH")]
    public int maxHp = 5;
    private int baseHp;
    //[HideInInspector]
    //public int maxDef;
    //[HideInInspector]
    //private int baseDef;
    public bool alive = true;
    public float invincibleDuration;

    public int hp;
    [HideInInspector]
    //public int def;
    private bool invincible;
    private float damageTakenTime;

    [Header("HEALTH GRAPHICS")]
    private int hpDisplayed;
    public Image[] hpImages;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    [HideInInspector]
    [Header("DEF GRAPHICS")]
    private int defDisplayed;
    [HideInInspector]
    public Sprite fullShield;
    [HideInInspector]
    public Sprite emptyShield;

    private GameObject respawnPoint;
    [HideInInspector] public int respawnSceneIndex;
    private bool respawnPointOverlap;
    private bool IsSitting;
    [HideInInspector] public bool canChangeScenes;

    public void Die()
    {
        movementDisable = true;
        alive = false;
    }
    public void TakeDamage(int damage)
    {
        if (invincible) return;
        //Deathward deathward = new Deathward();
        //if (equippedCharms.Contains(deathward)) damage = deathward.DeathSave(damage);

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
        hp -= damage;
        if (damage > 0 && hp > 0)
        {
            damageTakenTime = Time.time;
        }
        if (hp <= 0) { Die(); }
    }
    //private void HandleInvincibility()
    //{
    //    if (damageTakenTime + invincibleDuration <= Time.time)
    //    {
    //        invincible = false;
    //        damageTakenTime = 0;
    //    }
    //}
    private void HealthChecks()
    {
        CheckForHpGraphics();
        CheckForMaxValues();

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
            //if (def > maxDef) { def = maxDef; }
        }
    }
    //private void HandleRespawnPoint()
    //{
    //    if (!Input.GetKeyUp(KeyCode.DownArrow)) return;
    //    if (respawnPointOverlap && !IsSitting)
    //    {
    //        respawnSceneIndex = SceneManager.GetActiveScene().buildIndex;
    //        Mathf.MoveTowards(transform.position.x, respawnPoint.transform.position.x, previousVelocity.x);
    //        IsSitting = true;
    //        movementDisable = true;
    //    }
    //    if (IsSitting)
    //    {
    //        IsSitting = false;
    //        movementDisable = false;
    //    }
    //}

    public void Respawn()
    {
        //if (respawnSceneIndex < 2) return;
        if (!canChangeScenes)
        {
            Debug.LogError("Cannot change scenes. Respawn is impossible.");
            return;
        }

        //Loader.LoadScene(respawnSceneIndex);
        GameObject[] objects = SceneManager.GetSceneByBuildIndex(respawnSceneIndex).GetRootGameObjects();
        foreach (GameObject obj in objects)
        {
            if (obj != null && obj.CompareTag("Respawn")) respawnPoint = obj;
        }
        transform.position = respawnPoint.transform.position;
        ResetPlayer();
        IsSitting = true;
    }

    #endregion

    #region Movement
    [Header("MOVEMENT")]
    public float MaxSpeed = 12;
    public float Acceleration = 140;
    public float GroundDeceleration = 80;
    public float AirDeceleration = 50;
    public float GroundingForce = -1f;
    public float GrounderDistance = .1f;

    [Header("JUMP")]
    public int JumpPower = 18;
    public int JumpSustainPower = 16;
    public float MaxJumpTime = 0.2f;
    public float MaxFallSpeed = 30;
    public float FallAcceleration = 40;
    public float JumpEndGModifier = 5f;
    public float CoyoteTime = .15f;
    public float JumpBuffer = .2f;
    public float ApexSpeedModifier = 3f;

    private int MoveDirection;
    private bool groundHit, ceilingHit;
    private bool IsStanding, IsJumping;
    private Vector2 tempVelocity;
    private LayerMask Ground => LayerMask.GetMask("Ground");
    public bool movementDisable;

    private bool bufferedJumpUsable;
    private bool endedJump;
    private bool coyoteUsable;
    private bool jumpSustainable;
    private bool ApexHit;
    private float timeGroundLeft;
    //private Vector2 previousVelocity;

    private bool HasBufferedJump => bufferedJumpUsable && Time.time < JumpInput.timePressed + JumpBuffer;
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
            bufferedJumpUsable = true;
            jumpSustainable = true;
            endedJump = false;
        }

        if (IsStanding && !groundHit)
        {
            IsStanding = false;
            timeGroundLeft = Time.time;
        }
    }

    private void Jump()
    {
        if (IsStanding || HasCoyoteTime)
        {
            coyoteUsable = false;
            IsJumping = true;
            tempVelocity.y = JumpPower;
        }
    }
    private void JumpSustain()
    {
        if (ceilingHit || !jumpSustainable || JumpInput.timePressed + MaxJumpTime < Time.time) return;
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
            if (ceilingHit) tempVelocity.y = Mathf.Min(0, tempVelocity.y);

            tempVelocity.y = Mathf.MoveTowards(tempVelocity.y, -MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
        }
    }
    void ApplyMovement()
    {
        //previousVelocity = tempVelocity;
        RigidBody.velocity = tempVelocity;
    }

    #endregion

    #region Interact
    private void Interact()
    {
        Collision.gameObject.GetComponent<IInteractible>()?.Interaction();
    }
    private void Pause()
    {
        GameStatemachine machine = GameManager.Instance.machine;
        GamePausedState pausedState = new(machine);
        if (machine.CurrentState != pausedState) machine.ChangeState(pausedState);
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
    public float healChargeTime = .7f;
    public float healManaCost = 30;

    //public void Heal()
    //{
    //    if (!(timeDownArrowUp - timeDownArrowDown < healChargeTime)) return;

    //    if (hp == maxHp) return;

    //    hp += 1;
    //    mana -= healManaCost;
    //}

    #endregion

    #region Magic
    [Header("MAGIC")]
    public float mana = 100;
    public float maxMana;
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
    public List<IItem> inventory;
    #region Charms
    public int baseCharms = 3;
    public int unlockedCharms = 0;
    public int maxCharms;
    //public List<Charm> equippedCharms;

    public int hpAdd;
    public int defAdd;
    public int manaAdd;
    public int speedAdd;
    #endregion
    #endregion
}

