using UnityEngine.XR.Interaction.Toolkit;

namespace Feathersoft.Weapons.Ammo
{
    public class AmmoSocket : XRSocketInteractor
    {
        public AmmoType allowedAmmoTypes;

        public override bool CanHover(IXRHoverInteractable interactable) =>
            base.CanHover(interactable) && CanEquipAmmo(interactable);

        public override bool CanSelect(IXRSelectInteractable interactable) =>
            base.CanSelect(interactable) && CanEquipAmmo(interactable);

        private bool CanEquipAmmo(IXRInteractable interactable)
        {
            Ammo ammo = interactable.transform.GetComponent<Ammo>();
            return ammo != null && allowedAmmoTypes.HasFlag(ammo.type);
        }
    }
}