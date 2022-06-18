using UnityEngine.XR.Interaction.Toolkit;

namespace Feathersoft.XRI.KeyLock
{
    public class LockSocketInteractor : XRSocketInteractor
    {
        public LockType lockType;

        public override bool CanHover(IXRHoverInteractable interactable)
        {
            if (base.CanHover(interactable))
            {
                return CanInteract(interactable);
            }

            return false;
        }

        public override bool CanSelect(IXRSelectInteractable interactable)
        {
            if (base.CanSelect(interactable))
            {
                return CanInteract(interactable);
            }

            return false;
        }

        private bool CanInteract(IXRInteractable interactable)
        {
            //return gameObject.CompareTag(interactable.transform.gameObject.tag);
            Key key = interactable.transform.GetComponent<Key>();
            if (key != null)
            {
                return (lockType & key.lockType) != 0;
            }

            return false;
        }
    }
}