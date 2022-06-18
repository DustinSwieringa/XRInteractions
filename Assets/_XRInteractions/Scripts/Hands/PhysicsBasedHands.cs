using UnityEngine;

namespace Feathersoft.XRI.Hands
{
    public class PhysicsBasedHands : MonoBehaviour
    {
        public Transform handTarget;
        public Rigidbody handModelRigidbody;
        public float velocityMultiplier = 10;

        private Transform _cachedTransform;

        private void Awake()
        {
            _cachedTransform = handModelRigidbody.transform;
        }

        private void FixedUpdate()
        {
            Vector3 directionToTarget = handTarget.position - _cachedTransform.position;
            handModelRigidbody.velocity = directionToTarget * velocityMultiplier;
            _cachedTransform.rotation = handTarget.rotation;
            //Quaternion rot = handTarget.rotation * Quaternion.Inverse(_cachedTransform.rotation);
            //rot.ToAngleAxis(out float angle, out Vector3 axis);
            //handModelRigidbody.angularVelocity = axis * velocityMultiplier;
        }
    }
}