using UnityEngine;
using UnityEngine.Events;

namespace Feathersoft.Weapons
{
    public class PumpAction : MonoBehaviour
    {
        public Transform pumpStart, pumpEnd, pumpModel, trackedPumpAttach;

        [Range(0, 1f)]
        public float pumpThresholdPercent = 0.2f;

        public UnityEvent OnPumpAction;

        private Transform _cachedTransform;
        private float _cachedPumpThreshold;
        private bool _isPumped;

        private void Awake()
        {
            _cachedTransform = transform;
            _cachedPumpThreshold = Mathf.Abs(pumpEnd.localPosition.z - pumpStart.localPosition.z) * pumpThresholdPercent;
        }

        private void Update()
        {
            float handDistanceFromGrip = _cachedTransform.InverseTransformPoint(trackedPumpAttach.position).z;
            handDistanceFromGrip = Mathf.Clamp(handDistanceFromGrip, pumpEnd.localPosition.z, pumpStart.localPosition.z);
            Vector3 position = pumpModel.transform.localPosition;
            position.z = handDistanceFromGrip;
            pumpModel.transform.localPosition = position;

            bool pumpDown = Mathf.Abs(pumpEnd.localPosition.z - handDistanceFromGrip) <= _cachedPumpThreshold;
            bool pumpUp = Mathf.Abs(pumpStart.localPosition.z - handDistanceFromGrip) <= _cachedPumpThreshold;

            if (pumpDown && !_isPumped)
            {
                OnPumpAction?.Invoke();
                _isPumped = true;
            }
            else if (pumpUp && _isPumped)
            {
                _isPumped = false;
            }
        }
    }
}