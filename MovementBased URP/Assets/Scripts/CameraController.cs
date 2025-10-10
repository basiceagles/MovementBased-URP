using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    private const string HighscoreKey = "Sens";
    [SerializeField] private float sens;
    public Slider slider;
    [SerializeField] private float maxYAngle;

    private float rotationX;

    public void ChangeSens()
    {
        sens = slider.value;
        PlayerPrefs.SetFloat("Sens", sens);
        PlayerPrefs.Save();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        transform.parent.Rotate(Vector3.up * mouseX * sens);

        rotationX = Mathf.Clamp(rotationX - mouseY * sens, -maxYAngle, maxYAngle);
        transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
    }
}
