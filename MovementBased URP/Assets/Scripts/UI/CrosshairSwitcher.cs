using UnityEngine;

public class CrosshairSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject normalCrosshair;
    [SerializeField] private GameObject selectedCrosshair;
    [SerializeField] private bool isCrosshairSwitched = true;

    public void SwitchCrosshair()
    {
        
        if (!isCrosshairSwitched)
        {
            isCrosshairSwitched = !isCrosshairSwitched;
            normalCrosshair.SetActive(true);
            selectedCrosshair.SetActive(false);
        }
        else
        {
            isCrosshairSwitched = !isCrosshairSwitched;
            normalCrosshair.SetActive(false);
            selectedCrosshair.SetActive(true);
        }
    }
}
