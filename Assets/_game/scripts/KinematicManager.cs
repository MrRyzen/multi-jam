using FishNet;
using UnityEngine;

public class KinematicManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        InstanceFinder.TimeManager.OnTick += TimeManager_OnTick; // Could also be in Awake
    }

    private void TimeManager_OnTick()
    {
    }
}
