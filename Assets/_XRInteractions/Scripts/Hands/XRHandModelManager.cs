using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Feathersoft.XRI.Hands
{
    public class XRHandModelManager : MonoBehaviour
    {
        public static XRHandModelManager Instance { get; private set; }
        public static XRHandModel LeftHand;
        public static XRHandModel RightHand;

        public InteractionLayerMask LeftHandLayer;

        /// <summary>
        /// Returns if a mask falls on the left hand layer.
        /// </summary>
        public static bool IsLeftHand(InteractionLayerMask mask) => (mask.value & Instance.LeftHandLayer.value) > 0;

        private void Awake()
        {
            if (Instance != null)
                Destroy(Instance);
            Instance = this;
        }
    }
}