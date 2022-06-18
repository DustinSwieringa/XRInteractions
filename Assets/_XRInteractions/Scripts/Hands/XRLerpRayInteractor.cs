using UnityEngine;
using Feathersoft.Tools;
using UnityEngine.XR.Interaction.Toolkit;

public class XRLerpRayInteractor : XRBaseInteractor, ILineRenderable
{
    public LayerMask layerMask;
    public float offsetDistance = 0.1f;
    public float rayDistance = 10;

    private Transform _cachedTransform;
    private IXRInteractable _currentTarget;
    private Pose _interactableOffset;
    private RaycastHit? _currentHitInfo;

    protected override void Awake()
    {
        base.Awake();
        _cachedTransform = transform;
    }

    /// <inheritdoc />
    //public override void PreprocessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    //{
    //    if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender)
    //    {
    //        if (CastRay(Vector3.zero, out IXRSelectInteractable interactable) ||
    //            CastRay(new Vector3(offsetDistance, 0, 0), out interactable) ||
    //            CastRay(new Vector3(-offsetDistance, 0, 0), out interactable) ||
    //            CastRay(new Vector3(0, offsetDistance, 0), out interactable) ||
    //            CastRay(new Vector3(0, -offsetDistance, 0), out interactable))
    //            _currentTarget = interactable;
    //        else
    //            _currentTarget = null;
    //    }
    //}

    private void Update()
    {
        if (CastRay(Vector3.zero, out IXRSelectInteractable interactable) ||
            CastRay(new Vector3(offsetDistance, 0, 0), out interactable) ||
            CastRay(new Vector3(-offsetDistance, 0, 0), out interactable) ||
            CastRay(new Vector3(0, offsetDistance, 0), out interactable) ||
            CastRay(new Vector3(0, -offsetDistance, 0), out interactable))
            _currentTarget = interactable;
        else
            _currentTarget = null;
    }

    public override bool CanHover(IXRHoverInteractable interactable)
    {
        return interactable == _currentTarget;
    }

    public override bool CanSelect(IXRSelectInteractable interactable)
    {
        return interactable == _currentTarget;
    }

    private bool CastRay(Vector3 offset, out IXRSelectInteractable interactable)
    {
        Ray ray = new Ray(_cachedTransform.position + _cachedTransform.TransformPoint(offset), _cachedTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, layerMask))
        {
            interactable = hit.collider.GetComponentInParent<IXRSelectInteractable>();
            _currentHitInfo = hit;
        }
        else
        {
            interactable = null;
            _currentHitInfo = null;
        }

        return interactable != null &&
            (interactionLayers & interactable.interactionLayers) != 0 &&
            !interactable.isSelected;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        var handAttach = GetAttachTransform(null);
        handAttach.SetParent(null);
        var interactableAttach = args.interactableObject.GetAttachTransform(this).ToPose();

        _interactableOffset = _cachedTransform.ToPose().InverseTransformPose(interactableAttach);
        handAttach.SetPositionAndRotation(interactableAttach.position, interactableAttach.rotation);
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        var handAttach = GetAttachTransform(null);
        handAttach.SetParent(transform);

        base.OnSelectExited(args);
    }

    public float lerpSpeed = 5;

    /// <inheritdoc />
    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        base.OnSelectEntering(args);

        var targetPose = _cachedTransform.ToPose().TransformPose(_interactableOffset);
        var interactableAttach = args.interactableObject.GetAttachTransform(this);
        var speed = Time.deltaTime * lerpSpeed;
        var position = Vector3.Lerp(interactableAttach.position, targetPose.position, speed);
        var rotation = Quaternion.Lerp(interactableAttach.rotation, targetPose.rotation, speed);
        interactableAttach.SetPositionAndRotation(position, rotation);
    }

    public bool GetLinePoints(ref Vector3[] linePoints, out int numPoints)
    {
        if (_currentHitInfo == null)
        {
            numPoints = 0;
            return false;
        }

        linePoints = new Vector3[2];
        linePoints[0] = transform.position;
        linePoints[1] = _currentHitInfo.Value.point;
        numPoints = 2;
        return true;
    }

    public bool TryGetHitInfo(out Vector3 position, out Vector3 normal, out int positionInLine, out bool isValidTarget)
    {
        if (_currentHitInfo == null)
        {
            position = Vector3.zero;
            positionInLine = 0;
            normal= Vector3.zero;
            isValidTarget = false;
            return false;
        }

        var hitInfo = _currentHitInfo.Value;
        position = hitInfo.point;
        normal = hitInfo.normal;
        positionInLine = 1;
        isValidTarget = true;
        return true;
    }
}
