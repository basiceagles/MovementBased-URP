using UnityEngine;

public class CallCreditsScroller : MonoBehaviour
{
    public GameObject textCredits;
    private CreditsScroller creditsScroller;
    public bool callStart;
    public bool callStop;
    void Start()
    {
        creditsScroller = textCredits.GetComponent<CreditsScroller>();
    }

    public void CallStart()
    {
        if (callStart)
        {
            creditsScroller.StartScroll();
        }
    }
    
    public void CallStop()
    {
        if (callStop)
        {
            creditsScroller.StopScroll();
        }
    }
}
