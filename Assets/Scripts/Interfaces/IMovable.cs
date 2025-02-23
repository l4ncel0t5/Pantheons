using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMoveable
{
    void Move();
    void Flip();
    Rigidbody2D RigidBody { get; }
}
