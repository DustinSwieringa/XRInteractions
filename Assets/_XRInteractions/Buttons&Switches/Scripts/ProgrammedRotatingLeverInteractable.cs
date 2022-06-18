using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace Feathersoft.XRI.Switches
{
    public class ProgrammedRotatingLeverInteractable : XRBaseInteractable
    {
        public float lerpSpeed = 5f;
        public Transform anchorTransform;

        [SerializeField]
        private float _maxAngle = 45;

        /// <summary>
        /// Value between 0 (start point) and 1 (end point).
        /// </summary>
        [Header("Value between 0 (start point) and 1 (end point).")]
        public UnityEvent<float> OnValueChanged;

        private Quaternion _targetRotation;
        private float
            _startAngle,
            _endAngle,
            _value = 0;

        protected override void Awake()
        {
            _startAngle = -_maxAngle;
            _endAngle = _maxAngle;
            anchorTransform.rotation = transform.rotation * Quaternion.Euler(_maxAngle, 0, 0);
            _maxAngle = Mathf.Abs(_startAngle - _endAngle); // Convert max angle into a magnitude
            OnValueChanged?.Invoke(_value);
        }

        private void Update()
        {
            if (!isSelected)
                return;

            Transform handTransform = firstInteractorSelecting.GetAttachTransform(null);
            Vector3 localHandPos = transform.InverseTransformPoint(handTransform.position);
            localHandPos.x = 0;
            Vector3 fixedHandPosition = transform.TransformPoint(localHandPos);
            Vector3 directionToHand = fixedHandPosition - transform.position;

            // Angle between my hand and the local
            float angle = Vector3.SignedAngle(anchorTransform.parent.up, directionToHand, anchorTransform.parent.right);
            angle = Mathf.Clamp(angle, _startAngle, _endAngle);
            _targetRotation = transform.rotation * Quaternion.Euler(angle, 0, 0); // convert local to world space

            anchorTransform.rotation = Quaternion.Lerp(anchorTransform.rotation, _targetRotation, lerpSpeed * Time.deltaTime);

            float newValue = (_endAngle - angle) / _maxAngle;
            if (Mathf.Abs(newValue - _value) > 0.001)
            {
                _value = newValue;
                OnValueChanged?.Invoke(_value);
            }
        }
    }
}