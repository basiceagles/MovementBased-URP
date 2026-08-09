using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PerkShopUi : MonoBehaviour
{
    [SerializeField] private GameObject keybindHint;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private ItemPickup itemPickup;
    [SerializeField] private Animator moneyTextAnimator;
    [SerializeField] private Animator perkSelectorAnimator;
    [SerializeField] private AnimationClip openTextAnimation;
    [SerializeField] private AnimationClip closeTextAnimation;
    [SerializeField] private AnimationClip openPerkSelectorAnimation;
    [SerializeField] private AnimationClip closePerkSelectorAnimation;
    private bool ShopIsOpen = false;

    void Update()
    {
        if (InputSystem.actions.FindAction("OpenSettings").WasPressedThisFrame() && ShopIsOpen)
        {
            ClosePerkShop();
        }
    }

    public void ShowKeybindHint()
    {
        keybindHint.SetActive(true);
    }

    public void HideKeybindHint()
    {
        keybindHint.SetActive(false);
    }

    public void OpenPerkShop()
    {
        ShopIsOpen = true;

        Settings.SetSettingsOpenEnabled(false);

        moneyTextAnimator.Play(openTextAnimation.name);
        perkSelectorAnimator.Play(openPerkSelectorAnimation.name);

        if (playerController != null)
        {
            playerController.SetCameraMovementEnabled(false);
            playerController.SetMovementEnabled(false);
        }
        if (itemPickup != null) itemPickup.SetInteractionEnabled(false);
    }

    public void ClosePerkShop()
    {
        ShopIsOpen = false;
        
        Settings.SetSettingsOpenEnabled(true);
        
        moneyTextAnimator.Play(closeTextAnimation.name);
        perkSelectorAnimator.Play(closePerkSelectorAnimation.name);

        if (playerController != null)
        {
            playerController.SetCameraMovementEnabled(true);
            playerController.SetMovementEnabled(true);
        }
        if (itemPickup != null) itemPickup.SetInteractionEnabled(true);
    }
}
