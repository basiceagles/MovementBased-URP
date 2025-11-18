using UnityEngine;

public class CrosshairSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject normalCrosshair;
    [SerializeField] private GameObject selectedCrosshair;
    [SerializeField] private bool isCrosshairSwitched = true;

    public void ShowSwitchCrosshair()
    {
        isCrosshairSwitched = !isCrosshairSwitched;
        normalCrosshair.SetActive(false);
        selectedCrosshair.SetActive(true);
    }
    public void HideSwitchCrosshair()
    {
        isCrosshairSwitched = !isCrosshairSwitched;
        normalCrosshair.SetActive(true);
        selectedCrosshair.SetActive(false);
    }
}
