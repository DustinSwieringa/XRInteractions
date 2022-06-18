using UnityEngine;
using UnityEngine.InputSystem;

namespace Feathersoft.XRI.Hands
{
    public class HandAnimationController : MonoBehaviour
    {
        public InputActionProperty gripAction;

        private readonly int _BoolId = Animator.StringToHash("IsGripping");

        [SerializeField]
        private Animator _animator;

        private void Awake()
        {
            gripAction.action.started += x => Grip(true);
            gripAction.action.canceled += x => Grip(false);
        }

        public void Grip(bool isGripping) => _animator.SetBool(_BoolId, isGripping);
    }
}