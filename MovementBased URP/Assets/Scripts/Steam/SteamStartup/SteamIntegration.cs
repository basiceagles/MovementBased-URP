using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(SteamIntegration))]
public class SteamIntegrationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SteamIntegration steam = (SteamIntegration)target;

        string AchievementID = steam.AchievementID;

        GUILayout.Space(10);
        GUILayout.Label("Achievement Controls", EditorStyles.boldLabel);

        if (GUILayout.Button("Check Achievement"))
        {
            steam.IsThisAchievementUnlocked(AchievementID);
        }

        if (GUILayout.Button("Unlock Achievement"))
        {
            steam.UnlockAchievment(AchievementID);
        }

        if (GUILayout.Button("Clear Achievement"))
        {
            steam.ClearAchievments(AchievementID);
        }
    }
}

public class SteamIntegration : MonoBehaviour
{
    public static SteamIntegration Instance { get; private set; }

    [Header("Achievement Debug")]
    public string AchievementID;

    private uint appID = 4187280; // playtest ID

    private void Awake()
    {
        // Singleton enforcement
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        try
        {
            Steamworks.SteamClient.Init(appID);
            PrintYourName();
            PrintFriends();
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }

    void Update()
    {
        Steamworks.SteamClient.RunCallbacks();
    }

    void OnApplicationQuit()
    {
        Steamworks.SteamClient.Shutdown();
    }

    // ---------------- ACHIEVEMENTS ----------------
    
    public static void UnlockIfNotUnlocked(string id)
    {
        if (Instance == null)
        {
            Debug.LogError("SteamIntegration instance not found in scene.");
            return;
        }

        Instance.UnlockIfNotUnlocked_Internal(id);
    }

    private void UnlockIfNotUnlocked_Internal(string id)
    {
        var ach = new Steamworks.Data.Achievement(id);

        if (ach.State)
        {
            // Already unlocked → do nothing
            Debug.Log($"Achievement {id} already unlocked.");
            return;
        }

        ach.Trigger();
        Debug.Log($"Achievement {id} unlocked.");
    }

    // Debug helpers (Inspector buttons use these)

    public void IsThisAchievementUnlocked(string id)
    {
        var ach = new Steamworks.Data.Achievement(id);
        Debug.Log($"Achievement {id} status: {ach.State}");
    }

    public void UnlockAchievment(string id)
    {
        var ach = new Steamworks.Data.Achievement(id);
        ach.Trigger();
        Debug.Log($"Achievement {id} unlocked");
    }

    public void ClearAchievments(string id)
    {
        var ach = new Steamworks.Data.Achievement(id);
        ach.Clear();
        Debug.Log($"Achievement {id} cleared");
    }

    // ---------------- INFO ----------------

    private void PrintYourName()
    {
        Debug.Log("My name is " + Steamworks.SteamClient.Name);
    }

    private void PrintFriends()
    {
        foreach (var friend in Steamworks.SteamFriends.GetFriends())
        {
            Debug.Log("My friend is " + friend.Name);
        }
    }
}