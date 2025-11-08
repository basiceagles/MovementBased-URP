using UnityEngine;
using System.Collections.Generic;
using SmallHedge.SoundManager;

public class LightsController : MonoBehaviour
{
    [Header("Light Objects")]
    [Tooltip("Drag and drop GameObjects with animators here")]
    public GameObject[] lightObjects;
    public GameObject[] lampObjects;

    [Header("Shutdown Settings")]
    [Tooltip("Chance of lights shutting down (0-100%)")]
    [Range(0, 100)]
    public float shutdownChance = 30f;

    private List<Animator> lightAnimators = new List<Animator>();
    private List<Animator> lampAnimators = new List<Animator>();


    void Start()
    {
        // Get all animator components from the gameobjects
        GetAnimatorComponents();
    }

    void GetAnimatorComponents()
    {
        lightAnimators.Clear();
        lampAnimators.Clear();

        foreach (GameObject lightObject in lightObjects)
        {
            if (lightObject != null)
            {
                Animator animator = lightObject.GetComponent<Animator>();
                if (animator != null)
                {
                    lightAnimators.Add(animator);
                }
            }
        }
        
        foreach (GameObject lampObject in lampObjects)
        {
            if (lampObject != null)
            {
                Animator animator = lampObject.GetComponent<Animator>();
                if (animator != null)
                {
                    lampAnimators.Add(animator);
                }
            }
        }
    }

    public void LightsShutdownMethod()
    {
        // Calculate if we hit the chance
        float randomValue = Random.Range(0f, 100f);
        
        if (randomValue <= shutdownChance)
        {
            // Chance hit - trigger the animation on all objects
            TriggerShutdownAnimation();
        }
    }

    void TriggerShutdownAnimation()
    {
        SoundManager.PlaySound(SoundType.LIGHTBREAK);
        
        foreach (Animator animator in lightAnimators)
        {
            if (animator != null)
            {
                animator.Play("LightShutdown");
            }
        }

        foreach (Animator animator in lampAnimators)
        {
            if (animator != null)
            {
                animator.Play("LampShutdown");
            }
        }
    }
}