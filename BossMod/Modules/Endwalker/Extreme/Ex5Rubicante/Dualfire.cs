﻿namespace BossMod.Endwalker.Extreme.Ex5Rubicante;

class Dualfire(BossModule module) : Components.GenericBaitAway(module, AID.DualfireAOE)
{
    private static readonly AOEShapeCone _shape = new(60, 60.Degrees()); // TODO: verify angle

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Dualfire)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, _shape));
    }
}
