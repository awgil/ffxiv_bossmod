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

class EvilSeedAOE(BossModule module) : Components.StandardAOEs(module, AID.EvilSeedAOE, 5);

class EvilSeedVoidzone(BossModule module) : Components.PersistentVoidzone(module, 5, module => module.Enemies(OID.EvilSeed).Where(z => z.EventState != 7));

class ThornyVine(BossModule module) : Components.Chains(module, (uint)TetherID.ThornyVine, default, 25)
{
    public BitMask Targets;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.ThornyVineBait)
            Targets.Set(Raid.FindSlot(actor.InstanceID));
    }
}
