namespace BossMod.Dawntrail.Ultimate.FRU;

class P2LuminousHammer(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(6), (uint)IconID.LuminousHammer, ActionID.MakeSpell(AID.LuminousHammer), 7.1f, true)
{
    private readonly int[] _baitsPerPlayer = new int[PartyState.MaxPartySize];

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            var slot = Raid.FindSlot(spell.MainTargetID);
            if (slot >= 0 && ++_baitsPerPlayer[slot] >= 5)
                CurrentBaits.RemoveAll(b => b.Target == Raid[slot]);
        }
    }
}

// TODO: tower assignments (based on angle sorting?)
class P2BrightHunger1(BossModule module) : Components.GenericTowers(module, ActionID.MakeSpell(AID.BrightHunger))
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.LuminousHammer)
        {
            if (Towers.Count == 0)
                for (int i = 0; i < 6; ++i)
                    Towers.Add(new(Module.Center + 16 * (i * 60.Degrees()).ToDirection(), 4, 1, 1, default, WorldState.FutureTime(10.3f)));
            foreach (ref var t in Towers.AsSpan())
                t.ForbiddenSoakers.Set(Raid.FindSlot(actor.InstanceID));
        }
    }
}

// TODO: we can start showing aoes ~3s earlier if we check spawns
class P2HolyLightBurst(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HolyLightBurst), new AOEShapeCircle(11), 3);

class P2PowerfulLight(BossModule module) : Components.UniformStackSpread(module, 5, 0, 4, 4)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.WeightOfLight)
            AddStack(actor, status.ExpireAt);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.PowerfulLight)
            Stacks.Clear();
    }
}

class P2BrightHunger2(BossModule module) : Components.GenericTowers(module, ActionID.MakeSpell(AID.BrightHunger))
{
    private BitMask _forbidden;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Lightsteeped && status.Extra >= 3)
            _forbidden.Set(Raid.FindSlot(actor.InstanceID));
    }

    // TODO: better criteria for activating a tower...
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Towers.Count == 0 && (AID)spell.Action.ID == AID.HolyLightBurst)
            Towers.Add(new(Module.Center, 4, 1, 8, _forbidden, WorldState.FutureTime(6.5f)));
    }
}

class P2HouseOfLightBoss(BossModule module) : Components.GenericBaitAway(module, ActionID.MakeSpell(AID.HouseOfLightBossAOE), false)
{
    private static readonly AOEShapeCone _shape = new(60, 30.Degrees()); // TODO: verify angle

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HouseOfLightBoss)
            foreach (var p in Raid.WithoutSlot(true))
                CurrentBaits.Add(new(caster, p, _shape, Module.CastFinishAt(spell, 0.9f)));
    }
}
