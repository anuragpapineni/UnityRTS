using UnityEngine;
using System.Collections;

public class Airfield : Building
{
    protected override void Start()
    {
        base.Start();
        actions = new string[] { "AdvancedAircraft" };
    }

    public override void PerformAction(string actionToPerform)
    {
        base.PerformAction(actionToPerform);
        CreateUnit(actionToPerform);
    }
}