using FishNet;
using UnityEngine;
using KinematicCharacterController;
using FishNet.Object;
using System.Collections.Generic;

public class KinematicManager : NetworkBehaviour
{
    public KinematicCharacterSystem _kcs;
    public List<KinematicCharacterMotor> _motors;
    public List<PhysicsMover> _movers;
    // Start is called before the first frame update
    void Start()
    {
        InstanceFinder.TimeManager.OnTick += TimeManager_OnTick; // Could also be in Awake
        _kcs = KinematicCharacterSystem.GetInstance();
    }

    private void OnDestroy()
    {
        if (InstanceFinder.TimeManager != null)
            InstanceFinder.TimeManager.OnTick -= TimeManager_OnTick;
    }

    private void TimeManager_OnTick()
    {
        _motors = new List<KinematicCharacterMotor>(GetComponents<KinematicCharacterMotor>());
        print(new List<EntityController>(GetComponents<EntityController>()).Count);

        _movers = new List<PhysicsMover>(GetComponents<PhysicsMover>());

        KinematicCharacterSystem.Simulate((float)TimeManager.TickDelta, _motors, _movers);
    }
}
