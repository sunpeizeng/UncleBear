// Shatter Toolkit
// Copyright 2015 Gustav Olsson
using UnityEngine;

namespace ShatterToolkit.Helpers
{
    public class ShatterOnCollision : MonoBehaviour
    {
        public float requiredVelocity = 1.0f;
        public float cooldownTime = 0.5f;
        
        protected float timeSinceInstantiated;
        
        public void Update()
        {
            timeSinceInstantiated += Time.deltaTime;
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (timeSinceInstantiated >= cooldownTime && collision.relativeVelocity.magnitude >= requiredVelocity)
            {
                ContactPoint[] contacts = collision.contacts;
                for (int i = 0; i < contacts.Length; i++)
                {
                    // Make sure that we don't shatter if another object in the hierarchy was hit
                    if (contacts[i].otherCollider == collision.collider)
                    {
                        contacts[i].thisCollider.SendMessage("Shatter", contacts[i].point, SendMessageOptions.DontRequireReceiver);
                        break;
                    }
                }
            }
        }
    }
}