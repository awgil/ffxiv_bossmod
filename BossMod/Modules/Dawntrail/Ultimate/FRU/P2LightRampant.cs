namespace BossMod.Dawntrail.Ultimate.FRU;

class P2LightRampant(BossModule module) : BossComponent(module)
{
    private readonly Actor?[] _tetherTargets = new Actor?[PartyState.MaxPartySize];

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        for (int i = 0; i < _tetherTargets.Length; ++i)
        {
            var source = Raid[i];
            var target = _tetherTargets[i];
            if (source != null && target != null)
                Arena.AddLine(source.Position, target.Position, (source.Position - target.Position).LengthSq() < 625 ? ArenaColor.Danger : ArenaColor.Safe);
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID is TetherID.LightRampantChains or TetherID.LightRampantCurse && Raid.FindSlot(source.InstanceID) is var slot && slot >= 0)
            _tetherTargets[slot] = WorldState.Actors.Find(tether.Target);
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID is TetherID.LightRampantChains or TetherID.LightRampantCurse && Raid.FindSlot(source.InstanceID) is var slot && slot >= 0)
            _tetherTargets[slot] = null;
    }
}

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

class P2BrightHunger1(BossModule module) : Components.GenericTowers(module, ActionID.MakeSpell(AID.BrightHunger))
{
    private readonly FRUConfig _config = Service.Config.Get<FRUConfig>();
    private BitMask _forbidden;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.LuminousHammer)
        {
            _forbidden.Set(Raid.FindSlot(actor.InstanceID));
            RebuildTowers();
        }
    }

    private void RebuildTowers()
    {
        List<(int slot, int prio)> conga = [];
        foreach (var (slot, group) in _config.P2LightRampantAssignment.Resolve(Raid))
            if (!_forbidden[slot])
                conga.Add((slot, group));
        conga.SortBy(kv => kv.prio);
        if (conga.Count == 6)
        {
            var firstSouth = conga.FindIndex(kv => kv.prio >= 4);
            if (firstSouth == 2)
            {
                // rotate SW->NW
                (conga[2], conga[1]) = (conga[1], conga[2]);
                (conga[1], conga[0]) = (conga[0], conga[1]);
            }
            else if (firstSouth == 4)
            {
                // rotate NE->SE
                (conga[3], conga[4]) = (conga[4], conga[3]);
                (conga[4], conga[5]) = (conga[5], conga[4]);
            }
            // swap SE & SW to make order CW from NW
            (conga[3], conga[5]) = (conga[5], conga[3]);
            // finally, swap N & S and NW & NE to convert prepositions to tower positions
            (conga[0], conga[2]) = (conga[2], conga[0]);
            (conga[1], conga[4]) = (conga[4], conga[1]);
        }
        else
        {
            // bad assignments, assume there are none set
            conga.Clear();
        }

        Towers.Clear();
        for (int i = 0; i < 6; ++i)
        {
            var dir = (240 - i * 60).Degrees();
            var forbidden = conga.Count == 6 ? BitMask.Build(conga[i].slot) ^ new BitMask(0xFF) : _forbidden;
            Towers.Add(new(Module.Center + 16 * dir.ToDirection(), 4, 1, 1, forbidden, WorldState.FutureTime(10.3f)));
        }
    }
}

// note: we can start showing aoes ~3s earlier if we check spawns, but it's not really needed
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
