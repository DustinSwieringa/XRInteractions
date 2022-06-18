using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Feathersoft.XRI.Weapons
{
    public class XRDoubleGrabInteractrable : XRGrabInteractable
    {
        public Transform gripAttachTransform;

        // Overriding how the weapon's transform is processed depending on if we have one or two selections.
        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            if (interactorsSelecting.Count == 1)
                base.ProcessInteractable(updatePhase);
            else if (interactorsSelecting.Count == 2 &&
                updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
            {
                ProcessDoubleGrip();
            }
        }

        // Because XRGRabInteractable forces selectMode to be Single, we'll set it at runtime:
        protected override void Awake()
        {
            base.Awake();
            selectMode = InteractableSelectMode.Multiple;
        }

        /// <summary>
        /// What do we need?
        /// 1. What is the handle of the weapon?
        /// 2. What is the grip of the weapon?
        /// 3. Which hand tracks the handle?
        /// 4. Which hand tracks the grip?
        /// 
        /// How do we do it?
        /// 5. How does the weapon track between the hands? Anchor from handle? lerp between them?
        /// 6. Direciton between hands
        /// 7. Direction between parts
        /// 8. Getting local offset from weapon to parts (position and rotation)
        /// 9. Applying the offset oriented from the handle hand.
        /// 
        /// 10. Fix the grab/drop interactions.
        /// </summary>
        private void ProcessDoubleGrip()
        {
            // Get required Transforms.
            Transform partHandle = GetAttachTransform(null);
            Transform firstHand = interactorsSelecting[0].transform;
            Transform partGrip = gripAttachTransform;
            Transform handGrip = interactorsSelecting[1].transform;

            // Get directions
            Vector3 handDirectionHandleToGrip = handGrip.position - firstHand.position;
            Vector3 partDirectionHandleToGrip = partGrip.position - partHandle.position;
            Vector3 directionFromPartHandleToGun = transform.position - partHandle.position;

            // Get local Pose
            Vector3 localPositionFromHandleToGun = transform.InverseTransformDirection(directionFromPartHandleToGun);
            Quaternion localDirectionFromPartsToGun = Quaternion.FromToRotation(partDirectionHandleToGrip, transform.forward);

            // Apply local Pose to hand.
            Quaternion targetRotation = localDirectionFromPartsToGun * Quaternion.LookRotation(handDirectionHandleToGrip, firstHand.up);
            Vector3 targetPosition = firstHand.position + targetRotation * localPositionFromHandleToGun;

            transform.SetPositionAndRotation(targetPosition, targetRotation);
        }

        // Make sure we only do the weapon grab setup for the first grab.
        protected override void Grab()
        {
            if (interactorsSelecting.Count == 1)
                base.Grab();
        }

        // Make sure we only do the weapon drop when there's no interactors selecting.
        protected override void Drop()
        {
            if (interactorsSelecting.Count == 0)
                base.Drop();
        }

        // Only activate/deactivate when the "handle" hand in activating. (Optional)
        protected override void OnActivated(ActivateEventArgs args)
        {
            if (interactorsSelecting[0] == args.interactorObject)
                base.OnActivated(args);
        }
        protected override void OnDeactivated(DeactivateEventArgs args)
        {
            if (interactorsSelecting[0] == args.interactorObject)
                base.OnDeactivated(args);
        }
    }
}