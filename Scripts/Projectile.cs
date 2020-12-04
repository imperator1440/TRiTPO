﻿using UnityEngine;

namespace Assets.Scripts
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float m_speed = 10f;  // Wrong naming ('m_'), private field must start with "_"
        [SerializeField] private int m_damage = 10;  // Wrong naming ('m_'), private field must start with "_"
 
        [SerializeField] private Rigidbody2D m_rigidBody;  // Wrong naming ('m_'), private field must start with "_"

        public enum Parent
        {
            Player,
            Enemy,
            Default
        };

        [SerializeField] private Parent m_parent = Parent.Default;  // Wrong naming ('m_'), private field must start with "_"

        private void Start()
        {
            Vector3 newPos = new Vector3(transform.position.x, transform.position.y, 0f);
            gameObject.transform.position = newPos;
            m_rigidBody.velocity = transform.right * m_speed;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (m_parent == Parent.Enemy)
            {
                PlayerControl player = collision.gameObject.GetComponent<PlayerControl>();
                if (player != null) player.TakeDamage(m_damage);
                if (collision.gameObject.tag == "Player") Destroy(gameObject);
            }
            if (collision.gameObject.layer == 8)  // '8' what this number is?
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            Debug.Log("Player hit " + collision.gameObject.transform.position);
            if (m_parent == Parent.Player)
            {
                Skeleton skeleton_archer = collision.gameObject.GetComponent<Skeleton>();
                Melee_enemy melee_Enemy = collision.gameObject.GetComponent<Melee_enemy>();
                if (skeleton_archer != null) skeleton_archer.TakeDamage(m_damage); //  if (skeleton_archer)
                if (melee_Enemy != null) melee_Enemy.TakeDamage(m_damage); //  if (melee_Enemy)
                if (collision.gameObject.tag == "Enemy") Destroy(gameObject);
            }
            if (collision.gameObject.layer == 8) // '8' what this number is?
            {
                Destroy(gameObject);
            }
        }

        public void SetParent(Parent parent)
        {
            m_parent = parent;
        }
    }
}
