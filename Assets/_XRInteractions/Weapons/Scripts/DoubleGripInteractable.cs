using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using Feathersoft.Tools;

namespace Feathersoft.XRI.Weapons
{
    public class DoubleGripInteractable : MonoBehaviour
    {
        public PositionAnchorType anchorType;
        [SerializeField]
        private AnchorPart _frontRight, _backLeft;
        public bool trackPosition = true, trackRotation = true; //todo

        public UnityEvent<ActivateEventArgs> activated;
        public UnityEvent<DeactivateEventArgs> deactivated;

        private Transform _cachedTransform;
        private Rigidbody _cachedRigidbody;

        private bool IsFrontRight(IXRInteractable interactable) =>
            interactable.transform.gameObject.GetInstanceID() == _frontRight.interactable.gameObject.GetInstanceID();

        private void Awake()
        {
            _cachedTransform = transform;
            _cachedRigidbody = GetComponent<Rigidbody>();

            SetupWeaponPart(_backLeft);
            SetupWeaponPart(_frontRight);
        }

        private void SetupWeaponPart(AnchorPart part)
        {
            XRGrabInteractable interactable = part.interactable;
            interactable.selectEntered.AddListener(SelectEntered);
            interactable.selectExited.AddListener(SelectExited);
            interactable.activated.AddListener(Activated);
            interactable.deactivated.AddListener(Deactivated);
            Transform interactTransform = interactable.GetAttachTransform(null);
            part.AttachTransform = interactTransform;
            part.OffsetToWeapon = PoseExtensions.InverseTransformPose(interactTransform.ToPose(), _cachedTransform.ToPose());
            part.OffsetToPart = PoseExtensions.InverseTransformPose(_cachedTransform.ToPose(), interactTransform.ToPose());
        }

        private void SelectEntered(SelectEnterEventArgs args)
        {
            AnchorPart enteringPart = IsFrontRight(args.interactableObject) ? _frontRight : _backLeft;
            enteringPart.IsGrabbed = true;

            if (_cachedRigidbody != null)
                _cachedRigidbody.isKinematic = true;
        }

        private void SelectExited(SelectExitEventArgs args)
        {
            AnchorPart exitingPart = IsFrontRight(args.interactableObject) ? _frontRight : _backLeft;
            exitingPart.IsGrabbed = false;
            exitingPart.Reset(_cachedTransform);

            if (!_frontRight.IsGrabbed && !_backLeft.IsGrabbed && _cachedRigidbody != null)
                _cachedRigidbody.isKinematic = false;
        }

        private void Activated(ActivateEventArgs args)
        {
            if (!IsFrontRight(args.interactableObject))
                activated?.Invoke(args);
        }

        private void Deactivated(DeactivateEventArgs args)
        {
            if (!IsFrontRight(args.interactableObject))
                deactivated?.Invoke(args);
        }

        private void Update()
        {
            if (!_frontRight.IsGrabbed && !_backLeft.IsGrabbed) // No grip
                return;

            if (!_frontRight.IsGrabbed || !_backLeft.IsGrabbed) // Single Grip
            {
                AnchorPart part = _frontRight.IsGrabbed ? _frontRight : _backLeft;
                IXRSelectInteractable interactable = part.interactable;
                IXRSelectInteractor lastSelectingInteractor = interactable.interactorsSelecting[interactable.interactorsSelecting.Count - 1];
                bool isRightHand = lastSelectingInteractor.GetAttachTransform(null).lossyScale.x < 0;
                Transform source = part.AttachTransform;

                Quaternion rotation;
                if (trackRotation)
                {
                    if (isRightHand)
                    {
                        switch (part.reverseGrip)
                        {
                            case ReverseGripType.X:
                                rotation = Quaternion.LookRotation(-source.forward, -source.up);
                                break;
                            case ReverseGripType.Y:
                                rotation = Quaternion.LookRotation(source.forward, -source.up);
                                break;
                            case ReverseGripType.Z:
                                rotation = Quaternion.LookRotation(-source.forward, source.up);
                                break;
                            case ReverseGripType.None:
                            default:
                                rotation = source.rotation;
                                break;
                        }
                    }
                    else
                        rotation = source.rotation;

                    rotation *= part.OffsetToWeapon.rotation;
                }
                else rotation = _cachedTransform.rotation;

                if (trackPosition)
                    _cachedTransform.position = source.position + rotation * part.OffsetToWeapon.position;
                if (trackRotation)
                    _cachedTransform.rotation = rotation;
            }
            else // Double Grip
            {
                // Get required Transforms.
                Pose transformPose = _cachedTransform.ToPose();
                Pose partHandle = PoseExtensions.TransformPose(transformPose, _backLeft.OffsetToPart);
                Transform handHandle = _backLeft.AttachTransform;
                Pose partGrip = PoseExtensions.TransformPose(transformPose, _frontRight.OffsetToPart);
                Transform handGrip = _frontRight.AttachTransform;

                // Get directions
                Vector3 handDirectionHandleToGrip = handGrip.position - handHandle.position;
                Vector3 partDirectionHandleToGrip = partGrip.position - partHandle.position;

                // Get local Pose
                Quaternion localDirectionFromPartsToGun = Quaternion.FromToRotation(partDirectionHandleToGrip, _cachedTransform.forward);

                // Apply local Pose to hand.
                Quaternion targetRotation = localDirectionFromPartsToGun * Quaternion.LookRotation(handDirectionHandleToGrip, handHandle.up);

                Vector3 targetPosition = Vector3.zero;
                switch (anchorType)
                {
                    case PositionAnchorType.Back:
                        Vector3 directionFromPartBackToMain = _cachedTransform.position - partHandle.position;
                        Vector3 localPositionFromBackToMain = _cachedTransform.InverseTransformDirection(directionFromPartBackToMain);
                        targetPosition = handHandle.position + targetRotation * localPositionFromBackToMain;
                        break;
                    case PositionAnchorType.Mid:
                        Vector3 positionBetweenHands = Vector3.Lerp(handHandle.position, handGrip.position, 0.5f);
                        Vector3 positionBetweenParts = Vector3.Lerp(partHandle.position, partGrip.position, 0.5f);
                        Vector3 directionFromPartMidToMain = _cachedTransform.position - positionBetweenParts;
                        Vector3 localPositionFromMidToMain = _cachedTransform.InverseTransformDirection(directionFromPartMidToMain);
                        targetPosition = positionBetweenHands + targetRotation * localPositionFromMidToMain;
                        break;
                    case PositionAnchorType.Front:
                        Vector3 directionFromPartFrontToMain = _cachedTransform.position - partGrip.position;
                        Vector3 localPositionFromFrontToMain = _cachedTransform.InverseTransformDirection(directionFromPartFrontToMain);
                        targetPosition = handGrip.position + targetRotation * localPositionFromFrontToMain;
                        break;
                }

                if (trackPosition)
                    _cachedTransform.position = targetPosition;
                if (trackRotation)
                    _cachedTransform.rotation = targetRotation;
            }
        }

        [Serializable]
        public class AnchorPart
        {
            public XRGrabInteractable interactable;
            public ReverseGripType reverseGrip;

            public bool IsGrabbed { get; set; }
            public Pose OffsetToPart { get; set; }
            public Pose OffsetToWeapon { get; set; }
            public Transform AttachTransform { get; set; }

            public void Reset(Transform parent)
            {
                Pose worldPose = PoseExtensions.TransformPose(parent.ToPose(), OffsetToPart);
                interactable.transform.SetPositionAndRotation(worldPose.position, worldPose.rotation);
            }
        }

        public enum PositionAnchorType
        {
            Back,
            Mid,
            Front,
        }

        public enum ReverseGripType
        {
            None,
            X, Y, Z,
        }
    }
}