using UnityEngine;

public class PerkSelector : MonoBehaviour
{
    [Header("Object Settings")]
    public GameObject[] objectsArray; // Assign your GameObjects here in inspector
    public int amountToActivate = 2; // Adjustable amount of objects to activate

    public void OpenPerkMenu()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        ActivateRandomObjects();
    }
    
    public void ClosePerkMenu()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    // Call this method to activate random objects
    public void ActivateRandomObjects()
    {
        // First, deactivate all objects in the array
        foreach (GameObject obj in objectsArray)
        {
            if (obj != null)
                obj.SetActive(false);
        }
        
        // Check if we have enough objects in the array
        if (objectsArray.Length == 0)
        {
            return;
        }
        
        if (amountToActivate > objectsArray.Length)
        {
            amountToActivate = objectsArray.Length;
        }
        
        // Activate random objects
        for (int i = 0; i < amountToActivate; i++)
        {
            GameObject randomObject = GetRandomInactiveObject();
            if (randomObject != null)
            {
                randomObject.SetActive(true);
            }
        }
    }
    
    private GameObject GetRandomInactiveObject()
    {
        // Create a list of inactive objects
        System.Collections.Generic.List<GameObject> inactiveObjects = new System.Collections.Generic.List<GameObject>();
        
        foreach (GameObject obj in objectsArray)
        {
            if (obj != null && !obj.activeInHierarchy)
            {
                inactiveObjects.Add(obj);
            }
        }
        
        // If no inactive objects found, return null
        if (inactiveObjects.Count == 0)
            return null;
        
        // Return a random inactive object
        int randomIndex = Random.Range(0, inactiveObjects.Count);
        return inactiveObjects[randomIndex];
    }
}