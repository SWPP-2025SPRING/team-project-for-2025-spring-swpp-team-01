using System.Collections.Generic;
using UnityEngine;

public class BugRideTracker : MonoBehaviour
{
    public static BugRideTracker Instance;

    private HashSet<string> riddenBugs = new HashSet<string>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void AddBug(string bugName)
    {
        if (!riddenBugs.Contains(bugName))
            riddenBugs.Add(bugName);
    }

    public List<string> GetRiddenBugNames()
    {
        return new List<string>(riddenBugs);
    }

    public void ResetTracker()
    {
        riddenBugs.Clear();
    }

    public void Register(string bugName)
    {
        Debug.Log("🐛 Registering bug: " + bugName);
        AddBug(bugName);
        Debug.Log("🐛 Total bugs registered: " + riddenBugs.Count);
    }

}
