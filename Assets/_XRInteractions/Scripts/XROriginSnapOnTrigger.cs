using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XROriginSnapOnTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        XROrigin _origin = other.GetComponent<XROrigin>();
        if (_origin != null)
        {
            Vector3 position = transform.position;
            position.y += _origin.CameraInOriginSpaceHeight;
            _origin.MoveCameraToWorldLocation(position);
            _origin.MatchOriginUpCameraForward(_origin.transform.up, transform.forward);
        }
    }
}
