using UnityEngine;
//Это скрипт для рычагов всех
//Он как скрипт души чекает коснулись ли до него крюком или активировал, метод Collect
//Он отвечает за анимации рычага, также ищет геймобжект ЛевелМанагер, и за показ UI
public class Lever : MonoBehaviour
{
    [Header("UI Settings")]
    public string triggerObjectName = "LeverUITrigger";

    [Header("Animator Reference")]
    public Animator leverAnimator;

    [Header("Settings")]
    public string triggerName = "ActivateLever";
    public bool collected = false;

    [Header("Objects Reference")]
    [SerializeField] private GameObject uiToolTip;
    [SerializeField] private GameObject uiKeybind;

    [Header("Lever Manager Search")]
    public string targetTag = "LeverManager";
    public string componentType = "LeverSearcher";

    [Header("Debug")]
    public bool showDebug = true;

    private BoxCollider boxCollider;
    private GameObject leverManager;
    private LeverSearcher leverSearcherComponent;

    void Start()
    {
        // Get reference to existing box collider
        boxCollider = GetComponent<BoxCollider>();

        // Инвок чтобы дать предметам загрузится
        Invoke("SearchForLeverManager", 0.1f);
    }

    //Ищем левел манагер объект с помощью коллайдера, в нем нам нужен в скрипт LeverCollectManager, и метод CheckCollectStatus
    void SearchForLeverManager()
    {
        if (boxCollider == null) return;

        // Use the box collider's bounds to search for objects with the target tag
        Collider[] collidersInRange = Physics.OverlapBox(
            transform.TransformPoint(boxCollider.center),
            boxCollider.size * 0.5f,
            transform.rotation
        );


        foreach (Collider collider in collidersInRange)
        {
            if (collider.CompareTag(targetTag))
            {
                leverManager = collider.gameObject;
                leverSearcherComponent = leverManager.GetComponent<LeverSearcher>();

                if (leverSearcherComponent != null)
                {
                    if (showDebug) Debug.Log("Found LeverManager with LeverSearcher via collision: " + leverManager.name);
                    break;
                }
            }
        }
    }

    // Коллект метод проверяет актвировали ли мы рычаг или нет
    public void Collect()
    {
        if (collected == true) return;

        if (leverAnimator != null)
        {
            leverAnimator.SetTrigger(triggerName);
        }
        collected = true;

        // Hide UI elements
        HideUITip();
        HideUIKeybind();

        //Данная хуйня вызывается или проверяет потом все ли рычаги активировы, если да, то открывает дверь
        leverSearcherComponent.CheckLeverStatus();
    }

    //Методы для показа UI
    public void HideUITip()
    {
        if (uiToolTip != null)
        {
            uiToolTip.SetActive(false);
        }
    }

    public void ShowUITip()
    {
        if (uiToolTip != null && !collected)
        {
            uiToolTip.SetActive(true);
        }
    }

    public void HideUIKeybind()
    {
        if (uiKeybind != null)
        {
            uiKeybind.SetActive(false);
        }
    }

    public void ShowUIKeybind()
    {
        if (uiKeybind != null && !collected)
        {
            uiKeybind.SetActive(true);
        }
    }
}