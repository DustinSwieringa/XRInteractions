using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace Feathersoft.XRI.Switches
{
    public class ProgrammedSlidingLeverInteractable : XRBaseInteractable
    {
        public float lerpSpeed = 5f;
        [SerializeField]
        private Transform _modelTransform;
        [SerializeField]
        private Transform _endTransform;

        private float _startPosition;
        private float _endPosition;
        private Transform _cachedTransform;
        private float _value = 1;
        private float _maxDistance;

        /// <summary>
        /// Value between 1 (start point) and 0 (end point).
        /// </summary>
        [Header("Value between 1 (start point) and 0 (end point).")]
        public UnityEvent<float> OnValueChanged;

        protected override void Awake()
        {
            base.Awake();
            _cachedTransform = transform;
            _startPosition = _cachedTransform.InverseTransformPoint(_modelTransform.position).z;
            _endPosition = _cachedTransform.InverseTransformPoint(_endTransform.position).z;
            _maxDistance = _endPosition - _startPosition;
            OnValueChanged?.Invoke(_value);
        }

        private void Update()
        {
            if (!isSelected)
                return;

            Transform handAttach = interactorsSelecting[0].GetAttachTransform(null);
            Vector3 leverLocal = _cachedTransform.InverseTransformPoint(handAttach.position);
            float clampedZ = Mathf.Clamp(leverLocal.z, _startPosition, _endPosition);

            Vector3 modelPosition = _cachedTransform.InverseTransformPoint(_modelTransform.position);
            modelPosition.z = Mathf.Lerp(modelPosition.z, clampedZ, lerpSpeed * Time.deltaTime);
            _modelTransform.position = _cachedTransform.TransformPoint(modelPosition);

            float newValue = (_endPosition - modelPosition.z) / _maxDistance;

            if (Mathf.Abs(newValue - _value) > 0.001)
            {
                _value = newValue;
                OnValueChanged?.Invoke(_value);
            }
        }
    }
}