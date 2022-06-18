using UnityEngine;
using UnityEngine.Events;

namespace Feathersoft.XRI.Switches
{
    [RequireComponent(typeof(SpringJoint))]
    public class PhysicsButtonInteractable : MonoBehaviour
    {
        public Transform buttonUp;
        public Transform buttonDown;

        /// <summary>
        /// Value between 0 (fully pressed) and 1 (fully released).
        /// </summary>
        [Header("Value between 0 (fully pressed) and 1 (fully released).")]
        public UnityEvent<float> OnValueChanged;

        private Transform buttonModel;
        private float _totalDistance;
        private float _value = 1;

        private void Awake()
        {
            buttonModel = transform;
            _totalDistance = Vector3.Distance(buttonUp.position, buttonDown.position);
            OnValueChanged?.Invoke(1);
        }

        private void Update()
        {
            Vector3 localPos = buttonModel.localPosition;
            localPos.z = 0;
            localPos.x = 0;
            localPos.y = Mathf.Clamp(localPos.y, buttonDown.localPosition.y, buttonUp.localPosition.y);
            buttonModel.localPosition = localPos;

            float currentValue = Vector3.Distance(buttonModel.position, buttonDown.position) / _totalDistance;
            if (Mathf.Abs(currentValue - _value) > 0.001f)
            {
                _value = currentValue;
                OnValueChanged?.Invoke(currentValue);
            }
        }
    }
}