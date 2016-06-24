using UnityEngine;
using System.Collections;

public class Hangar : Building {
    protected override void Start()
    {
        base.Start();
        actions = new string[] { "BasicAircraft" };
    }

    public override void PerformAction(string actionToPerform)
    {
        base.PerformAction(actionToPerform);
        CreateUnit(actionToPerform);
    }
}
