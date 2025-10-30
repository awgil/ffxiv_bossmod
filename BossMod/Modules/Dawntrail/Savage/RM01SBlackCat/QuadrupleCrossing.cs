namespace BossMod.Dawntrail.Savage.RM01SBlackCat;

// same component covers normal, leaping and leaping clone versions
class QuadrupleCrossingProtean(BossModule module) : Components.GenericBaitAway(module)
{
    public Actor? Origin;
    private DateTime _activation;
    private Actor? _clone;
    private Angle _jumpDirection;

    private static readonly AOEShapeCone _shape = new(100, 22.5f.Degrees());

    public override void Update()
    {
        CurrentBaits.Clear();
        if (Origin != null && _activation != default)
            foreach (var p in Module.Raid.WithoutSlot().SortedByRange(Origin.Position).Take(4))
                CurrentBaits.Add(new(Origin, p, _shape, _activation));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (_clone != null && CurrentBaits.Count == 0)
            Arena.Actor(_clone.Position + 10 * (_clone.Rotation + _jumpDirection).ToDirection(), _clone.Rotation, ArenaColor.Object);
    }

    public override void OnActorCreated(Actor actor)
    {
        // note: tether target is created after boss is tethered...
        if ((OID)actor.OID == OID.LeapTarget && Module.PrimaryActor.Tether.Target == actor.InstanceID)
        {
            Origin = actor;
            _jumpDirection = Angle.FromDirection(actor.Position - Module.PrimaryActor.Position) - (Module.PrimaryActor.CastInfo?.Rotation ?? Module.PrimaryActor.Rotation);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.QuadrupleCrossingFirst:
                Origin = caster;
                _activation = Module.CastFinishAt(spell, 0.8f);
                break;
            case AID.LeapingQuadrupleCrossingBossL:
            case AID.LeapingQuadrupleCrossingBossR:
                // origin will be set to leap target when it's created
                _activation = Module.CastFinishAt(spell, 1.8f);
                break;
            case AID.NailchipperAOE:
                if (NumCasts == 8)
                    ForbiddenPlayers.Set(Module.Raid.FindSlot(spell.TargetID));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.QuadrupleCrossingProtean or AID.LeapingQuadrupleCrossingBossProtean or AID.LeapingQuadrupleCrossingShadeProtean)
        {
            if (NumCasts == 8)
            {
                ForbiddenPlayers.Reset(); // third set => clear nailchippers
            }

            _activation = Module.WorldState.FutureTime(3);
            foreach (var t in spell.Targets)
                ForbiddenPlayers.Set(Module.Raid.FindSlot(t.ID));

            if (++NumCasts is 8 or 16)
            {
                Origin = null;
                ForbiddenPlayers.Reset();
            }
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (NumCasts < 8 || tether.ID != (uint)TetherID.Soulshade)
            return; // not relevant tether

        if (_clone == null)
        {
            _clone = source;
        }
        else if (_clone == source)
        {
            var origin = source.Position + 10 * (source.Rotation + _jumpDirection).ToDirection();
            Origin = new(0, 0, -1, 0, "", 0, ActorType.None, Class.None, 0, new(origin.X, source.PosRot.Y, origin.Z, source.PosRot.W));
            _activation = Module.WorldState.FutureTime(17);
        }
    }
}

class QuadrupleCrossingAOE(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCone _shape = new(100, 22.5f.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count >= 8 ? _aoes.Skip(NumCasts).Take(4) : [];

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.QuadrupleCrossingProtean:
            case AID.LeapingQuadrupleCrossingBossProtean:
            case AID.LeapingQuadrupleCrossingShadeProtean:
                _aoes.Add(new(_shape, caster.Position, caster.Rotation, Module.WorldState.FutureTime(5.9f)));
                break;
            case AID.QuadrupleCrossingAOE:
            case AID.LeapingQuadrupleCrossingBossAOE:
            case AID.LeapingQuadrupleCrossingShadeAOE:
                ++NumCasts;
                break;
        }
    }
}
