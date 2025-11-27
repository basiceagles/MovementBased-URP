using System;
using UnityEngine;

public class PerkShopUi : MonoBehaviour
{
    [SerializeField] private GameObject keybindHint;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private Animator moneyTextAnimator;
    [SerializeField] private Animator perkSelectorAnimator;
    [SerializeField] private AnimationClip openTextAnimation;
    [SerializeField] private AnimationClip closeTextAnimation;
    [SerializeField] private AnimationClip openPerkSelectorAnimation;
    [SerializeField] private AnimationClip closePerkSelectorAnimation;

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
        moneyTextAnimator.Play(openTextAnimation.name);
        perkSelectorAnimator.Play(openPerkSelectorAnimation.name);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        cameraController.enabled = false;
    }

    public void ClosePerkShop()
    {
        moneyTextAnimator.Play(closeTextAnimation.name);
        perkSelectorAnimator.Play(closePerkSelectorAnimation.name);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        cameraController.enabled = true;
    }

}
