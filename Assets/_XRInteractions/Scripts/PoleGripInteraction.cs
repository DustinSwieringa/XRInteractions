using Feathersoft.XRI.Hands;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace Feathersoft.XRI
{
    namespace Feathersoft.XRI
    {
        public class PoleGripInteraction : MonoBehaviour
        {
            [SerializeField]
            private Transform _bottom, _top;

            private XRBaseInteractable _interactable;
            private bool _isFlipped;

            private bool _isActive;
            private List<TrackedLink> _activeLinks = new List<TrackedLink>();

            private void Awake()
            {
                _interactable = GetComponent<XRBaseInteractable>();
                _interactable.selectEntered.AddListener(OnSelectEntered);
                _interactable.selectExited.AddListener(OnSelectExited);
                _interactable.activated.AddListener(OnActivated);
                _interactable.deactivated.AddListener(OnDeactivated);
            }

            private bool ProcessHand(AttachedTransformLink link)
            {
                Transform hand = link.Hand;
                float localHandY = transform.InverseTransformPoint(hand.position).y;
                link.Attached.localPosition = new Vector3(0, Mathf.Clamp(localHandY, _bottom.localPosition.y, _top.localPosition.y), 0);

                bool flip = Vector3.Dot(hand.up, transform.up) < 0;
                if (flip) // flip
                    FlipAttached(hand);

                return flip;
            }

            private void OnSelectEntered(SelectEnterEventArgs args)
            {
                AttachedTransformLink link = Getlink(args);
                bool isLeft = XRHandModelManager.IsLeftHand(args.interactorObject.interactionLayers);
                bool flip = ProcessHand(link);

                if (isLeft)
                {
                    XRHandModelManager.LeftHand.LockHand(link.Attached);
                    if (flip)
                        XRHandModelManager.LeftHand.Flip();
                }
                else
                {
                    XRHandModelManager.RightHand.LockHand(link.Attached);

                    if (flip)
                        XRHandModelManager.RightHand.Flip();
                }
            }

            private void OnActivated(ActivateEventArgs args)
            {
                _activeLinks.Add(new TrackedLink(Getlink(args)));
                _isActive = true;
            }

            private AttachedTransformLink Getlink(BaseInteractionEventArgs args)
            {
                return new AttachedTransformLink(
                    args.interactableObject.GetAttachTransform(args.interactorObject),
                    args.interactorObject.GetAttachTransform(args.interactableObject));
            }

            private void OnDeactivated(DeactivateEventArgs args)
            {
                _activeLinks.Remove(new TrackedLink(Getlink(args)));
                _isActive = false;
            }

            private void Update()
            {
                if (!_isActive)
                    return;

                foreach (TrackedLink link in _activeLinks)
                {
                    Vector3 worldshaftY = link.Link.Attached.position;
                    Vector3 diff = worldshaftY - link.PreviousYPos;
                    Vector3 convertedLocal = transform.rotation * diff;
                    Vector3 currentPos = link.Link.Attached.localPosition;
                    currentPos.y = Mathf.Clamp(currentPos.y + convertedLocal.y, _bottom.localPosition.y, _top.localPosition.y);
                    link.Link.Attached.localPosition = currentPos;
                    link.PreviousYPos = worldshaftY;
                }
            }

            private void FlipAttached(Transform attachedHand)
            {
                attachedHand.Rotate(0, 0, 180, Space.Self);
                _isFlipped = !_isFlipped;
            }

            private void OnSelectExited(SelectExitEventArgs args)
            {
                if (_isFlipped)
                    FlipAttached(args.interactorObject.GetAttachTransform(args.interactableObject));

                if (XRHandModelManager.IsLeftHand(args.interactorObject.interactionLayers))
                    XRHandModelManager.LeftHand.ResetHand();
                else
                    XRHandModelManager.RightHand.ResetHand();
            }

            private class TrackedLink : IEquatable<TrackedLink>
            {
                public TrackedLink(AttachedTransformLink link)
                {
                    Link = link;
                    PreviousYPos = link.Attached.position;
                }

                public AttachedTransformLink Link { get; set; }
                public Vector3 PreviousYPos { get; set; }

                public bool Equals(TrackedLink other)
                {
                    return Link.Equals(other.Link);
                }
            }

            private struct AttachedTransformLink
            {
                public AttachedTransformLink(Transform attached, Transform hand)
                {
                    Attached = attached;
                    Hand = hand;
                }

                public Transform Attached { get; }
                public Transform Hand { get; }
            }
        }
    }
}