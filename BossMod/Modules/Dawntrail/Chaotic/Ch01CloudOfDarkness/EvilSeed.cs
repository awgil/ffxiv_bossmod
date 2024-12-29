namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

class EvilSeedBait(BossModule module) : BossComponent(module)
{
    public BitMask Baiters;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var p in Raid.WithSlot().IncludedInMask(Baiters).Actors())
            Arena.AddCircle(p.Position, 5, ArenaColor.Danger);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.EvilSeed)
            Baiters.Set(Raid.FindSlot(actor.InstanceID));
    }
}

class EvilSeedAOE(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.EvilSeedAOE), 5);

// todo: should be chains...
class ThornyVine(BossModule module) : BossComponent(module)
{
    public BitMask Targets;
    public bool HaveTethers;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.ThornyVineBait)
            Targets.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.ThornyVine)
            HaveTethers = true;
    }
}
