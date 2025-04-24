namespace BossMod.Dawntrail.Savage.RM04SWickedThunder;

class WideningNarrowingWitchHunt(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShapeCircle _shapeOut = new(10);
    private static readonly AOEShapeDonut _shapeIn = new(10, 60);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs.Skip(NumCasts).Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var (first, second) = (AID)spell.Action.ID switch
        {
            AID.WideningWitchHunt => (_shapeOut, _shapeIn),
            AID.NarrowingWitchHunt => (_shapeIn, _shapeOut),
            _ => ((AOEShape?)null, (AOEShape?)null)
        };
        if (first != null && second != null)
        {
            AOEs.Add(new(first, caster.Position, spell.Rotation, Module.CastFinishAt(spell, 1.1f)));
            AOEs.Add(new(second, caster.Position, spell.Rotation, Module.CastFinishAt(spell, 4.6f)));
            AOEs.Add(new(first, caster.Position, spell.Rotation, Module.CastFinishAt(spell, 8.1f)));
            AOEs.Add(new(second, caster.Position, spell.Rotation, Module.CastFinishAt(spell, 11.6f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.LightningVortexAOE or AID.ThunderingAOE)
            ++NumCasts;
    }
}

class WideningNarrowingWitchHuntBait(BossModule module) : Components.GenericBaitAway(module, AID.WitchHuntAOE, centerAtTarget: true)
{
    public enum Mechanic { None, Near, Far }

    public Mechanic CurMechanic;
    private DateTime _activation;

    private static readonly AOEShapeCircle _shape = new(6);

    public override void Update()
    {
        CurrentBaits.Clear();
        if (CurMechanic != Mechanic.None)
        {
            var targets = Raid.WithoutSlot().SortedByRange(Module.Center);
            targets = CurMechanic == Mechanic.Near ? targets.Take(2) : targets.TakeLast(2);
            foreach (var t in targets)
                CurrentBaits.Add(new(Module.PrimaryActor, t, _shape, _activation));
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurMechanic != Mechanic.None)
            hints.Add($"Next bait: {CurMechanic}");
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Marker && CurMechanic == Mechanic.None)
        {
            _activation = status.ExpireAt.AddSeconds(12.2f);
            CurMechanic = status.Extra switch
            {
                0x2F6 => Mechanic.Near,
                0x2F7 => Mechanic.Far,
                _ => Mechanic.None
            };
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            ForbiddenPlayers.Set(Raid.FindSlot(spell.MainTargetID));
            _activation = WorldState.FutureTime(3.5f);
            if ((NumCasts & 1) == 0)
                CurMechanic = CurMechanic == Mechanic.Near ? Mechanic.Far : Mechanic.Near;
        }
    }
}
