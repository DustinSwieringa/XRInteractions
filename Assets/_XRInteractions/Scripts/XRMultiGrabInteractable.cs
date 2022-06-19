using Feathersoft.XRI.Hands;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Feathersoft.XRI
{
    public class XRMultiGrabInteractable : XRBaseInteractable
    {
        [SerializeField]
        private Transform m_AttachLeftTarget, m_AttachRightTarget;

        [SerializeField]
        private float m_AttachEaseInTime = 0.15f;

        [SerializeField]
        private MovementType m_MovementType = MovementType.Instantaneous;

        [SerializeField]
        [Range(0f, 1f)]
        private float m_VelocityDamping = 1f;

        [SerializeField]
        private float m_VelocityScale = 1f;

        [SerializeField]
        [Range(0f, 1f)]
        private float m_AngularVelocityDamping = 1f;

        [SerializeField]
        private float m_AngularVelocityScale = 1f;

        [SerializeField]
        private bool m_TrackPosition = true;

        [SerializeField]
        private bool m_SmoothPosition;

        [SerializeField]
        [Range(0f, 20f)]
        private float m_SmoothPositionAmount = 5f;

        [SerializeField]
        [Range(0f, 1f)]
        private float m_TightenPosition = 0.5f;

        [SerializeField]
        private bool m_TrackRotation = true;

        [SerializeField]
        private bool m_SmoothRotation;

        [SerializeField]
        [Range(0f, 20f)]
        private float m_SmoothRotationAmount = 5f;

        [SerializeField]
        [Range(0f, 1f)]
        private float m_TightenRotation = 0.5f;

        [SerializeField]
        private bool m_ThrowOnDetach = true;

        [SerializeField]
        private float m_ThrowSmoothingDuration = 0.25f;

        [SerializeField]
        private AnimationCurve m_ThrowSmoothingCurve = AnimationCurve.Linear(1f, 1f, 1f, 0f);

        [SerializeField]
        private float m_ThrowVelocityScale = 1.5f;

        [SerializeField]
        private float m_ThrowAngularVelocityScale = 1f;

        [SerializeField]
        private bool m_ForceGravityOnDetach;

        [SerializeField]
        private bool m_RetainTransformParent = true;

        private Vector3 m_InteractorLocalPosition;

        private Quaternion m_InteractorLocalRotation;

        private Vector3 m_TargetWorldPosition;

        private Quaternion m_TargetWorldRotation;

        private float m_CurrentAttachEaseTime;

        private MovementType m_CurrentMovementType;

        private bool m_DetachInLateUpdate;

        private Vector3 m_DetachVelocity;

        private Vector3 m_DetachAngularVelocity;

        private int m_ThrowSmoothingCurrentFrame;

        private readonly float[] m_ThrowSmoothingFrameTimes = new float[20];

        private readonly Vector3[] m_ThrowSmoothingVelocityFrames = new Vector3[20];

        private readonly Vector3[] m_ThrowSmoothingAngularVelocityFrames = new Vector3[20];

        private Rigidbody m_Rigidbody;

        private Vector3 m_LastPosition;

        private Quaternion m_LastRotation;

        private bool m_WasKinematic;

        private bool m_UsedGravity;

        private float m_OldDrag;

        private float m_OldAngularDrag;

        private Transform m_OriginalSceneParent;

        private TeleportationProvider m_TeleportationProvider;

        private Pose m_PoseBeforeTeleport;

        public bool IsDoubleGripping { get; private set; }

        public float attachEaseInTime
        {
            get
            {
                return m_AttachEaseInTime;
            }
            set
            {
                m_AttachEaseInTime = value;
            }
        }

        public MovementType movementType
        {
            get
            {
                return m_MovementType;
            }
            set
            {
                m_MovementType = value;
                if (base.isSelected)
                {
                    SetupRigidbodyDrop(m_Rigidbody);
                    UpdateCurrentMovementType();
                    SetupRigidbodyGrab(m_Rigidbody);
                }
            }
        }

        public float velocityDamping
        {
            get
            {
                return m_VelocityDamping;
            }
            set
            {
                m_VelocityDamping = value;
            }
        }

        public float velocityScale
        {
            get
            {
                return m_VelocityScale;
            }
            set
            {
                m_VelocityScale = value;
            }
        }

        public float angularVelocityDamping
        {
            get
            {
                return m_AngularVelocityDamping;
            }
            set
            {
                m_AngularVelocityDamping = value;
            }
        }

        public float angularVelocityScale
        {
            get
            {
                return m_AngularVelocityScale;
            }
            set
            {
                m_AngularVelocityScale = value;
            }
        }

        public bool trackPosition
        {
            get
            {
                return m_TrackPosition;
            }
            set
            {
                m_TrackPosition = value;
            }
        }

        public bool smoothPosition
        {
            get
            {
                return m_SmoothPosition;
            }
            set
            {
                m_SmoothPosition = value;
            }
        }

        public float smoothPositionAmount
        {
            get
            {
                return m_SmoothPositionAmount;
            }
            set
            {
                m_SmoothPositionAmount = value;
            }
        }

        public float tightenPosition
        {
            get
            {
                return m_TightenPosition;
            }
            set
            {
                m_TightenPosition = value;
            }
        }

        public bool trackRotation
        {
            get
            {
                return m_TrackRotation;
            }
            set
            {
                m_TrackRotation = value;
            }
        }

        public bool smoothRotation
        {
            get
            {
                return m_SmoothRotation;
            }
            set
            {
                m_SmoothRotation = value;
            }
        }

        public float smoothRotationAmount
        {
            get
            {
                return m_SmoothRotationAmount;
            }
            set
            {
                m_SmoothRotationAmount = value;
            }
        }

        public float tightenRotation
        {
            get
            {
                return m_TightenRotation;
            }
            set
            {
                m_TightenRotation = value;
            }
        }

        public bool throwOnDetach
        {
            get
            {
                return m_ThrowOnDetach;
            }
            set
            {
                m_ThrowOnDetach = value;
            }
        }

        public float throwSmoothingDuration
        {
            get
            {
                return m_ThrowSmoothingDuration;
            }
            set
            {
                m_ThrowSmoothingDuration = value;
            }
        }

        public AnimationCurve throwSmoothingCurve
        {
            get
            {
                return m_ThrowSmoothingCurve;
            }
            set
            {
                m_ThrowSmoothingCurve = value;
            }
        }

        public float throwVelocityScale
        {
            get
            {
                return m_ThrowVelocityScale;
            }
            set
            {
                m_ThrowVelocityScale = value;
            }
        }

        public float throwAngularVelocityScale
        {
            get
            {
                return m_ThrowAngularVelocityScale;
            }
            set
            {
                m_ThrowAngularVelocityScale = value;
            }
        }

        public bool forceGravityOnDetach
        {
            get
            {
                return m_ForceGravityOnDetach;
            }
            set
            {
                m_ForceGravityOnDetach = value;
            }
        }

        public bool retainTransformParent
        {
            get
            {
                return m_RetainTransformParent;
            }
            set
            {
                m_RetainTransformParent = value;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            m_CurrentMovementType = m_MovementType;
            m_Rigidbody = GetComponent<Rigidbody>();
            if (m_Rigidbody == null)
            {
                Debug.LogError("Grab Interactable does not have a required Rigidbody.", this);
            }
        }

        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractable(updatePhase);
            switch (updatePhase)
            {
                case XRInteractionUpdateOrder.UpdatePhase.Fixed:
                    if (base.isSelected)
                    {
                        if (m_CurrentMovementType == MovementType.Kinematic)
                        {
                            PerformKinematicUpdate(updatePhase);
                        }
                        else if (m_CurrentMovementType == MovementType.VelocityTracking)
                        {
                            PerformVelocityTrackingUpdate(Time.deltaTime, updatePhase);
                        }
                    }

                    break;
                case XRInteractionUpdateOrder.UpdatePhase.Dynamic:
                    if (base.isSelected)
                    {
                        if (IsDoubleGripping)
                        {
                            UpdateTargets(Time.deltaTime);
                        }
                        else
                        {
                            IXRSelectInteractor interactor2 = interactorsSelecting[0];
                            UpdateInteractorLocalPose(interactor2);
                            UpdateTarget(interactor2, Time.deltaTime);
                            SmoothVelocityUpdate(interactor2);
                        }

                        if (m_CurrentMovementType == MovementType.Instantaneous)
                        {
                            PerformInstantaneousUpdate(updatePhase);
                        }
                    }

                    break;
                case XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender:
                    if (base.isSelected)
                    {
                        if (IsDoubleGripping)
                        {
                            UpdateTargets(Time.deltaTime);
                        }
                        else
                        {
                            IXRSelectInteractor interactor = interactorsSelecting[0];
                            UpdateInteractorLocalPose(interactor);
                            UpdateTarget(interactor, Time.deltaTime);
                        }

                        if (m_CurrentMovementType == MovementType.Instantaneous)
                        {
                            PerformInstantaneousUpdate(updatePhase);
                        }
                    }

                    break;
                case XRInteractionUpdateOrder.UpdatePhase.Late:
                    if (m_DetachInLateUpdate)
                    {
                        if (!base.isSelected)
                        {
                            Detach();
                        }

                        m_DetachInLateUpdate = false;
                    }

                    break;
            }
        }

        public override Transform GetAttachTransform(IXRInteractor interactor) =>
            XRHandModelManager.IsLeftHand(interactor.interactionLayers) ? m_AttachLeftTarget : m_AttachRightTarget;

        private Vector3 GetWorldAttachPosition(IXRInteractor interactor)
        {
            Transform attachTransform = interactor.GetAttachTransform(this);

            if (!m_TrackRotation)
            {
                Transform attachTransform2 = GetAttachTransform(interactor);
                return attachTransform.position +
                    attachTransform2.TransformDirection(m_InteractorLocalPosition);
            }

            return attachTransform.position + attachTransform.rotation * m_InteractorLocalPosition;
        }

        private Quaternion GetWorldAttachRotation(IXRInteractor interactor)
        {
            if (!m_TrackRotation)
            {
                return m_TargetWorldRotation;
            }

            Transform attachTransform = interactor.GetAttachTransform(this);
            return attachTransform.rotation * m_InteractorLocalRotation;
        }

        private void UpdateTarget(IXRInteractor interactor, float timeDelta)
        {
            Vector3 worldAttachPosition = GetWorldAttachPosition(interactor);
            Quaternion worldAttachRotation = GetWorldAttachRotation(interactor);
            UpdateTarget(timeDelta, worldAttachPosition, worldAttachRotation);
        }

        private void UpdateTargets(float timeDelta)
        {
            //IXRSelectInteractor interactor1 = interactorsSelecting[0];
            //IXRSelectInteractor interactor2 = interactorsSelecting[1];

            //Transform attachTransform = GetAttachTransform(interactor);
            //Vector3 direction = base.transform.position - attachTransform.position;
            //m_InteractorLocalPosition = attachTransform.InverseTransformDirection(direction);

            //Vector3 worldAttachPosition1 = GetWorldAttachPosition(interactor1);
            //Vector3 worldAttachPosition2 = GetWorldAttachPosition(interactor2);

            //Transform attachTransform1 = interactor1.GetAttachTransform(this);
            //Transform attachTransform2 = interactor2.GetAttachTransform(this);

            IXRSelectInteractor localAttach1 = interactorsSelecting[0];
            IXRSelectInteractor localAttach2 = interactorsSelecting[1];
            Transform localAttach1Transform = GetAttachTransform(localAttach1);
            Transform localAttach2Transform = GetAttachTransform(localAttach2);

            // Interactable
            test1.position = Vector3.Lerp(localAttach1Transform.position, localAttach2Transform.position, 0.5f);
            Vector3 offsetToInteractable = base.transform.position - test1.position;
            m_InteractorLocalPosition = offsetToInteractable;

            Vector3 localDirection = (localAttach2Transform.position - localAttach1Transform.position).normalized;
            Quaternion localRotation = Quaternion.Lerp(localAttach1Transform.rotation, localAttach2Transform.rotation, 0.5f);
            test1.rotation = Quaternion.LookRotation(localRotation * Vector3.forward, localDirection);
            m_InteractorLocalRotation = Quaternion.Inverse(Quaternion.Inverse(base.transform.rotation) * test1.rotation);

            // Hand
            Transform Attach1Transform = localAttach1.transform;
            Transform Attach2Transform = localAttach2.transform;

            test2.position = Vector3.Lerp(Attach1Transform.position, Attach2Transform.position, 0.5f);
            Vector3 directionOfHands = (Attach2Transform.position - Attach1Transform.position).normalized;
            Vector3 averageUp = Vector3.Lerp(Attach1Transform.up, Attach2Transform.up, 0.5f);
            test2.rotation = Quaternion.LookRotation(directionOfHands, averageUp) * Quaternion.Euler(0, 90, 90);

            Vector3 worldAttachPosition = test2.position + test2.rotation * m_InteractorLocalPosition;
            Quaternion worldAttachRotation = test2.rotation;

            UpdateTarget(timeDelta, worldAttachPosition, worldAttachRotation);
        }

        public Transform test1, test2;

        private void UpdateTarget(float timeDelta, Vector3 worldAttachPosition, Quaternion worldAttachRotation)
        {
            if (m_AttachEaseInTime > 0f && m_CurrentAttachEaseTime <= m_AttachEaseInTime)
            {
                float t = m_CurrentAttachEaseTime / m_AttachEaseInTime;
                m_TargetWorldPosition = Vector3.Lerp(m_TargetWorldPosition, worldAttachPosition, t);
                m_TargetWorldRotation = Quaternion.Slerp(m_TargetWorldRotation, worldAttachRotation, t);
                m_CurrentAttachEaseTime += timeDelta;
                return;
            }

            if (m_SmoothPosition)
            {
                m_TargetWorldPosition = Vector3.Lerp(m_TargetWorldPosition, worldAttachPosition, m_SmoothPositionAmount * timeDelta);
                m_TargetWorldPosition = Vector3.Lerp(m_TargetWorldPosition, worldAttachPosition, m_TightenPosition);
            }
            else
            {
                m_TargetWorldPosition = worldAttachPosition;
            }

            if (m_SmoothRotation)
            {
                m_TargetWorldRotation = Quaternion.Slerp(m_TargetWorldRotation, worldAttachRotation, m_SmoothRotationAmount * timeDelta);
                m_TargetWorldRotation = Quaternion.Slerp(m_TargetWorldRotation, worldAttachRotation, m_TightenRotation);
            }
            else
            {
                m_TargetWorldRotation = worldAttachRotation;
            }
        }

        private void PerformInstantaneousUpdate(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic || updatePhase == XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender)
            {
                if (m_TrackPosition)
                {
                    base.transform.position = m_TargetWorldPosition;
                }

                if (m_TrackRotation)
                {
                    base.transform.rotation = m_TargetWorldRotation;
                }
            }
        }

        private void PerformKinematicUpdate(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Fixed)
            {
                if (m_TrackPosition)
                {
                    Vector3 position = m_TargetWorldPosition;
                    m_Rigidbody.velocity = Vector3.zero;
                    m_Rigidbody.MovePosition(position);
                }

                if (m_TrackRotation)
                {
                    m_Rigidbody.angularVelocity = Vector3.zero;
                    m_Rigidbody.MoveRotation(m_TargetWorldRotation);
                }
            }
        }

        private void PerformVelocityTrackingUpdate(float timeDelta, XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            if (updatePhase != 0)
            {
                return;
            }

            if (m_TrackPosition)
            {
                m_Rigidbody.velocity *= 1f - m_VelocityDamping;
                Vector3 a = m_TargetWorldPosition - base.transform.position;
                Vector3 a2 = a / timeDelta;
                if (!float.IsNaN(a2.x))
                {
                    m_Rigidbody.velocity += a2 * m_VelocityScale;
                }
            }

            if (!m_TrackRotation)
            {
                return;
            }

            m_Rigidbody.angularVelocity *= 1f - m_AngularVelocityDamping;
            (m_TargetWorldRotation * Quaternion.Inverse(base.transform.rotation)).ToAngleAxis(out float angle, out Vector3 axis);
            if (angle > 180f)
            {
                angle -= 360f;
            }

            if (Mathf.Abs(angle) > Mathf.Epsilon)
            {
                Vector3 a3 = axis * (angle * ((float)Math.PI / 180f)) / timeDelta;
                if (!float.IsNaN(a3.x))
                {
                    m_Rigidbody.angularVelocity += a3 * m_AngularVelocityScale;
                }
            }
        }

        private void UpdateInteractorLocalPose(IXRInteractor interactor)
        {
            Transform attachTransform = GetAttachTransform(interactor);
            Vector3 direction = base.transform.position - attachTransform.position;
            m_InteractorLocalPosition = attachTransform.InverseTransformDirection(direction);
            m_InteractorLocalRotation = Quaternion.Inverse(Quaternion.Inverse(base.transform.rotation) * attachTransform.rotation);
        }

        private void UpdateCurrentMovementType()
        {
            IXRSelectInteractor iXRSelectInteractor = base.interactorsSelecting[0];
            XRBaseInteractor xRBaseInteractor = iXRSelectInteractor as XRBaseInteractor;
            m_CurrentMovementType = (((xRBaseInteractor != null) ? xRBaseInteractor.selectedInteractableMovementTypeOverride : null) ?? m_MovementType);
        }

        protected override void OnSelectEntering(SelectEnterEventArgs args)
        {
            if (interactorsSelecting.Count == 1)
                IsDoubleGripping = true;

            base.OnSelectEntering(args);
            Grab(args);
        }

        protected override void OnSelectExiting(SelectExitEventArgs args)
        {
            if (interactorsSelecting.Count == 2)
                IsDoubleGripping = false;

            base.OnSelectExiting(args);
            Drop();
        }

        protected virtual void Grab(SelectEnterEventArgs args)
        {
            if (interactorsSelecting.Count == 1) // First time grab setup
            {
                Transform transform = base.transform;
                m_OriginalSceneParent = transform.parent;
                transform.SetParent(null);
                UpdateCurrentMovementType();
                SetupRigidbodyGrab(m_Rigidbody);
                m_DetachVelocity = Vector3.zero;
                m_DetachAngularVelocity = Vector3.zero;
                m_TargetWorldPosition = transform.position;
                m_TargetWorldRotation = transform.rotation;
                m_CurrentAttachEaseTime = 0f;
            }

            IXRSelectInteractor interactor = args.interactorObject;
            UpdateInteractorLocalPose(interactor);
            SmoothVelocityStart(interactor);
        }

        protected virtual void Drop()
        {
            if (interactorsSelecting.Count != 0)
                return;

            if (m_RetainTransformParent && m_OriginalSceneParent != null && !m_OriginalSceneParent.gameObject.activeInHierarchy)
            {
                if (!EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    Debug.LogWarning("Retain Transform Parent is set to true, and has a non-null Original Scene Parent. However, the old parent is deactivated so we are choosing not to re-parent upon dropping.", this);
                }
            }
            else if (m_RetainTransformParent && base.gameObject.activeInHierarchy)
            {
                base.transform.SetParent(m_OriginalSceneParent);
            }

            SetupRigidbodyDrop(m_Rigidbody);
            m_CurrentMovementType = m_MovementType;
            m_DetachInLateUpdate = true;
            SmoothVelocityEnd();
        }

        protected virtual void Detach()
        {
            if (m_ThrowOnDetach)
            {
                m_Rigidbody.velocity = m_DetachVelocity;
                m_Rigidbody.angularVelocity = m_DetachAngularVelocity;
            }
        }

        protected virtual void SetupRigidbodyGrab(Rigidbody rigidbody)
        {
            m_WasKinematic = rigidbody.isKinematic;
            m_UsedGravity = rigidbody.useGravity;
            m_OldDrag = rigidbody.drag;
            m_OldAngularDrag = rigidbody.angularDrag;
            rigidbody.isKinematic = (m_CurrentMovementType == MovementType.Kinematic || m_CurrentMovementType == MovementType.Instantaneous);
            rigidbody.useGravity = false;
            rigidbody.drag = 0f;
            rigidbody.angularDrag = 0f;
        }

        protected virtual void SetupRigidbodyDrop(Rigidbody rigidbody)
        {
            rigidbody.isKinematic = m_WasKinematic;
            rigidbody.useGravity = m_UsedGravity;
            rigidbody.drag = m_OldDrag;
            rigidbody.angularDrag = m_OldAngularDrag;
            if (!base.isSelected)
            {
                m_Rigidbody.useGravity |= m_ForceGravityOnDetach;
            }
        }

        private void SmoothVelocityStart(IXRInteractor interactor)
        {
            SetTeleportationProvider(interactor);
            Transform attachTransform = interactor.GetAttachTransform(this);
            m_LastPosition = attachTransform.position;
            m_LastRotation = attachTransform.rotation;
            Array.Clear(m_ThrowSmoothingFrameTimes, 0, m_ThrowSmoothingFrameTimes.Length);
            Array.Clear(m_ThrowSmoothingVelocityFrames, 0, m_ThrowSmoothingVelocityFrames.Length);
            Array.Clear(m_ThrowSmoothingAngularVelocityFrames, 0, m_ThrowSmoothingAngularVelocityFrames.Length);
            m_ThrowSmoothingCurrentFrame = 0;
        }

        private void SmoothVelocityEnd()
        {
            if (m_ThrowOnDetach)
            {
                Vector3 smoothedVelocityValue = GetSmoothedVelocityValue(m_ThrowSmoothingVelocityFrames);
                Vector3 smoothedVelocityValue2 = GetSmoothedVelocityValue(m_ThrowSmoothingAngularVelocityFrames);
                m_DetachVelocity = smoothedVelocityValue * m_ThrowVelocityScale;
                m_DetachAngularVelocity = smoothedVelocityValue2 * m_ThrowAngularVelocityScale;
            }

            ClearTeleportationProvider();
        }

        private void SmoothVelocityUpdate(IXRInteractor interactor)
        {
            Transform attachTransform = interactor.GetAttachTransform(this);
            Vector3 position = attachTransform.position;
            Quaternion rotation = attachTransform.rotation;
            m_ThrowSmoothingFrameTimes[m_ThrowSmoothingCurrentFrame] = Time.time;
            m_ThrowSmoothingVelocityFrames[m_ThrowSmoothingCurrentFrame] = (position - m_LastPosition) / Time.deltaTime;
            Quaternion quaternion = rotation * Quaternion.Inverse(m_LastRotation);
            m_ThrowSmoothingAngularVelocityFrames[m_ThrowSmoothingCurrentFrame] = new Vector3(Mathf.DeltaAngle(0f, quaternion.eulerAngles.x), Mathf.DeltaAngle(0f, quaternion.eulerAngles.y), Mathf.DeltaAngle(0f, quaternion.eulerAngles.z)) / Time.deltaTime * ((float)Math.PI / 180f);
            m_ThrowSmoothingCurrentFrame = (m_ThrowSmoothingCurrentFrame + 1) % 20;
            m_LastPosition = position;
            m_LastRotation = rotation;
        }

        private Vector3 GetSmoothedVelocityValue(Vector3[] velocityFrames)
        {
            Vector3 a = default(Vector3);
            float num = 0f;
            for (int i = 0; i < 20; i++)
            {
                int num2 = ((m_ThrowSmoothingCurrentFrame - i - 1) % 20 + 20) % 20;
                if (m_ThrowSmoothingFrameTimes[num2] == 0f)
                {
                    break;
                }

                float num3 = (Time.time - m_ThrowSmoothingFrameTimes[num2]) / m_ThrowSmoothingDuration;
                float num4 = m_ThrowSmoothingCurve.Evaluate(Mathf.Clamp(1f - num3, 0f, 1f));
                a += velocityFrames[num2] * num4;
                num += num4;
                if (Time.time - m_ThrowSmoothingFrameTimes[num2] > m_ThrowSmoothingDuration)
                {
                    break;
                }
            }

            if (num > 0f)
            {
                return a / num;
            }

            return Vector3.zero;
        }

        private void OnBeginTeleportation(LocomotionSystem locomotionSystem)
        {
            Transform transform = locomotionSystem.xrOrigin.Origin.transform;
            m_PoseBeforeTeleport = new Pose(transform.position, transform.rotation);
        }

        private void OnEndTeleportation(LocomotionSystem locomotionSystem)
        {
            Transform transform = locomotionSystem.xrOrigin.Origin.transform;
            Vector3 vector = transform.position - m_PoseBeforeTeleport.position;
            Quaternion quaternion = transform.rotation * Quaternion.Inverse(m_PoseBeforeTeleport.rotation);
            for (int i = 0; i < 20 && m_ThrowSmoothingFrameTimes[i] != 0f; i++)
            {
                m_ThrowSmoothingVelocityFrames[i] = quaternion * m_ThrowSmoothingVelocityFrames[i];
            }

            m_LastPosition += vector;
            m_LastRotation = quaternion * m_LastRotation;
        }

        private void SetTeleportationProvider(IXRInteractor interactor)
        {
            ClearTeleportationProvider();
            Transform transform = interactor?.transform;
            if (!(transform == null))
            {
                m_TeleportationProvider = transform.GetComponentInParent<TeleportationProvider>();
                if (!(m_TeleportationProvider == null))
                {
                    m_TeleportationProvider.beginLocomotion += OnBeginTeleportation;
                    m_TeleportationProvider.endLocomotion += OnEndTeleportation;
                }
            }
        }

        private void ClearTeleportationProvider()
        {
            if (!(m_TeleportationProvider == null))
            {
                m_TeleportationProvider.beginLocomotion -= OnBeginTeleportation;
                m_TeleportationProvider.endLocomotion -= OnEndTeleportation;
                m_TeleportationProvider = null;
            }
        }
    }
}