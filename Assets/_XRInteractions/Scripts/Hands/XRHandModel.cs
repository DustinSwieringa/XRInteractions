using UnityEngine;

namespace Feathersoft.XRI.Hands
{
    public class XRHandModel : MonoBehaviour
    {
        [SerializeField]
        private XRHandType _handType;
        [SerializeField]
        private Transform _modelTransform;

        private Transform _targetTransform;
        private bool _isUnlinked;
        private bool _isFlipped;

        private void Awake()
        {
            if (_handType == XRHandType.Left)
                XRHandModelManager.LeftHand = this;
            else XRHandModelManager.RightHand = this;
        }

        public void LockHand(Transform target)
        {
            _isUnlinked = true;
            _targetTransform = target;
            transform.localPosition = Vector3.zero;
        }

        public void ResetHand()
        {
            _isUnlinked = false;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            if (_isFlipped)
                Flip();

        }

        public void Flip()
        {
            _isFlipped = !_isFlipped;
            _modelTransform.Rotate(0, 0, 180);
        }

        private void Update()
        {
            if (!_isUnlinked)
                return;

            transform.position = _targetTransform.position;
            transform.rotation = _targetTransform.rotation;
            //transform.rotation = Quaternion.LookRotation(_targetTransform.forward, _trackedHandTransform.forward);
        }
    }
}