using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(RawImage))]
public class VideoFlickerFixer : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 0.5f;

    private RawImage rawImage;
    private float originalAlpha;

    private void Awake()
    {
        rawImage = GetComponent<RawImage>();
        originalAlpha = 255f;
    }

    private void Start()
    {
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        // Set to white with zero alpha (255f == 1f in Unity Color)
        rawImage.color = new Color(255f, 255f, 255f, 0f);

        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            yield return null;
        }

        rawImage.color = new Color(255f, 255f, 255f, originalAlpha);
    }
}