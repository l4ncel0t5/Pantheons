//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Events;

//public class PlayerJumpState : PlayerState
//{
//    private float maxJumpTime = 0.6f;
//    private float jumpTimer = 0f;
//    private float jumpForce;
//    private Rigidbody2D RigidBody;
//    private Animator animator;
//    private AudioManager audio;
//    private Transform transform;
//    private float speed;

//    public UnityEvent playerJump = new UnityEvent();

//    public PlayerJumpState(PlayerManager player, PlayerStatemachine machine) : base(player, machine)
//    {
//        this.player = player;
//        this.machine = machine;
//        jumpForce = player.JumpPower;
//        RigidBody = player.RigidBody;
//        animator = player.Animator;
//        transform = player.transform;
//        speed = player.Acceleration;
//        audio = AudioManager.instance;

//        playerJump.AddListener(audio.OnPlayerJump);
//    }

<<<<<<< HEAD
//    public override void EnterState()
//    {
//<<<<<<< Updated upstream
//        playerJump.Invoke();
//        animator.SetTrigger("Jump");
//=======
//        playerJump.Invoke(); 
//        animator.Play("jump");
//>>>>>>> Stashed changes
//    }
=======
    public override void EnterState()
    {
        playerJump.Invoke();
        animator.SetTrigger("Jump");
    }
>>>>>>> parent of 7a2af25 (13.8.24)

//    public override void ExitState()
//    {
//        jumpTimer = 0;
//    }

//    public override void PhysicsUpdate()
//    {
//        jumpTimer += Time.deltaTime;
//        if (jumpTimer > maxJumpTime) { jumpTimer = maxJumpTime; }
//        if (jumpTimer < maxJumpTime)
//        {
//            RigidBody.velocity = new Vector2(RigidBody.velocity.x, jumpForce - (jumpTimer / maxJumpTime * 10f));
//        }

//        if (Input.GetKey(KeyCode.LeftArrow))
//        {
//            transform.localRotation = Quaternion.Euler(transform.localRotation.x, 180f, transform.rotation.z);
//            RigidBody.velocity = new Vector2(-speed, RigidBody.velocity.y);
//        }
//        if (Input.GetKey(KeyCode.RightArrow))
//        {
//            transform.localRotation = Quaternion.Euler(transform.localRotation.x, 0f, transform.rotation.z);
//            RigidBody.velocity = new Vector2(speed, RigidBody.velocity.y);
//        }
//    }

//    public override void Update()
//    {
//        base.Update();
//    }
//}
