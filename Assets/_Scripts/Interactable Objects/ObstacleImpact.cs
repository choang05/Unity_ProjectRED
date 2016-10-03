﻿using UnityEngine;

public class ObstacleImpact : MonoBehaviour
{
    void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag(Tags.Player))
        {
            //  Cache the player's character controller
            CharacterController2D charController = other.collider.GetComponent<CharacterController2D>();

            //  Get total force. (impulse / time)
            Vector3 collisionForce = other.impulse / Time.fixedDeltaTime;

            charController.ProcessImpact(collisionForce);
        }
    }
}