namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

sealed class EvilSeedBait(BossModule module) : BossComponent(module)
{
    public BitMask Baiters;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var p in Raid.WithSlot(false, false, true).IncludedInMask(Baiters).Actors())
            Arena.ZoneCircleOutline(p.Position, 5f);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.EvilSeed)
            Baiters.Set(Raid.FindSlot(actor.InstanceID));
    }
}

sealed class EvilSeedAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EvilSeedAOE, 5f);

sealed class EvilSeedVoidzone(BossModule module) : Components.Voidzone(module, 5f, module => module.Enemies((uint)OID.EvilSeed).Where(z => z.EventState != 7));

sealed class ThornyVine(BossModule module) : Components.Chains(module, (uint)TetherID.ThornyVine, default, 25f)
{
    public BitMask Targets;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.ThornyVineBait)
            Targets[Raid.FindSlot(actor.InstanceID)] = true;
    }
}
