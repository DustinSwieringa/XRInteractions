using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace Feathersoft.XRI.Switches
{
    public class ProgrammedDialInteractable : XRBaseInteractable
    {
        public Transform modelTransform;

        /// <summary>
        /// Value between 1 (start point) and 0 (end point).
        /// </summary>
        [Header("Value between 1 (start point) and 0 (end point).")]
        public UnityEvent<float> OnValueChanged;

        [SerializeField]
        private float _startAngle;
        [SerializeField]
        private float _endAngle;

        private Vector3 _previousForward;
        private float _maxAngle;
        private bool _rotateRight;
        private float _currentAngle;
        private float _value = 1;

        public float CurrentAngle
        {

            get => _currentAngle;
            private set
            {
                if (_rotateRight)
                    _currentAngle = Mathf.Clamp(value, _startAngle, _endAngle);
                else
                    _currentAngle = Mathf.Clamp(value, _endAngle, _startAngle);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            modelTransform = transform;
            _rotateRight = _startAngle < _endAngle;
            _maxAngle = Mathf.Abs(_endAngle - _startAngle);
            _currentAngle = _startAngle;
            UpdateRotation();
        }

        private void Update()
        {
            if (!isSelected)
                return;

            Transform handTransform = firstInteractorSelecting.GetAttachTransform(null);
            Vector3 currentForward = handTransform.forward;

            if (handTransform.lossyScale.x < 0) // account for right hand
                CurrentAngle -= Vector3.SignedAngle(_previousForward, currentForward, -handTransform.right);
            else
                CurrentAngle -= Vector3.SignedAngle(_previousForward, currentForward, handTransform.right);

            UpdateRotation();
            _previousForward = currentForward;
        }

        private void UpdateRotation()
        {
            modelTransform.localRotation = Quaternion.Euler(0, CurrentAngle, 0);

            float currentValue = Mathf.Abs((_endAngle - CurrentAngle) / _maxAngle);
            if (Mathf.Abs(currentValue - _value) > 0.001f)
            {
                _value = currentValue;
                OnValueChanged?.Invoke(currentValue);
            }
            OnValueChanged?.Invoke(Mathf.Abs((_endAngle - CurrentAngle) / _maxAngle));
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            _previousForward = interactorsSelecting[0].transform.forward;
        }
    }
}