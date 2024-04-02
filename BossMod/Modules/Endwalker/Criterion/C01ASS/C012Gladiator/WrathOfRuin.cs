namespace BossMod.Endwalker.Criterion.C01ASS.C012Gladiator;

class GoldenSilverFlame : BossComponent
{
    private List<Actor> _goldenFlames = new();
    private List<Actor> _silverFlames = new();
    private int[] _debuffs = new int[PartyState.MaxPartySize]; // silver << 16 | gold

    public bool Active => _goldenFlames.Count + _silverFlames.Count > 0;

    private static readonly AOEShapeRect _shape = new(60, 5);

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (DebuffsAtPosition(actor.Position) != _debuffs[slot])
            hints.Add("Go to correct cell!");
    }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // TODO: implement
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (Active)
            foreach (var c in SafeCenters(module, _debuffs[pcSlot]))
                arena.ZoneRect(c, new WDir(1, 0), _shape.HalfWidth, _shape.HalfWidth, _shape.HalfWidth, ArenaColor.SafeFromAOE);
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        int debuff = (SID)status.ID switch
        {
            SID.GildedFate => status.Extra,
            SID.SilveredFate => status.Extra << 16,
            _ => 0
        };

        if (debuff == 0)
            return;
        var slot = module.Raid.FindSlot(actor.InstanceID);
        if (slot >= 0)
            _debuffs[slot] |= debuff;
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        CasterList(spell)?.Add(caster);
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        CasterList(spell)?.Remove(caster);
    }

    private List<Actor>? CasterList(ActorCastInfo spell) => (AID)spell.Action.ID switch
    {
        AID.NGoldenFlame or AID.SGoldenFlame => _goldenFlames,
        AID.NSilverFlame or AID.SSilverFlame => _silverFlames,
        _ => null
    };

    private int CastersHittingPosition(List<Actor> casters, WPos pos) => casters.Count(a => _shape.Check(pos, a.Position, a.CastInfo!.Rotation));
    private int DebuffsAtPosition(WPos pos) => CastersHittingPosition(_silverFlames, pos) | (CastersHittingPosition(_goldenFlames, pos) << 16);

    private IEnumerable<WPos> SafeCenters(BossModule module, int debuff)
    {
        var limit = module.Bounds.Center + new WDir(module.Bounds.HalfSize, module.Bounds.HalfSize);
        var first = module.Bounds.Center - new WDir(module.Bounds.HalfSize - _shape.HalfWidth, module.Bounds.HalfSize - _shape.HalfWidth);
        var advance = 2 * _shape.HalfWidth;
        for (float x = first.X; x < limit.X; x += advance)
            for (float z = first.Z; z < limit.Z; z += advance)
                if (DebuffsAtPosition(new WPos(x, z)) == debuff)
                    yield return new(x, z);
    }
}

// note: actual spell targets location, but it seems to be incorrect...
// note: we can predict cast start during Regret actor spawn...
class RackAndRuin : Components.SelfTargetedAOEs
{
    public RackAndRuin(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeRect(40, 2.5f), 8) { }
}
class NRackAndRuin : RackAndRuin { public NRackAndRuin() : base(AID.NRackAndRuin) { } }
class SRackAndRuin : RackAndRuin { public SRackAndRuin() : base(AID.SRackAndRuin) { } }

class NothingBesideRemains : Components.SpreadFromCastTargets
{
    public NothingBesideRemains(AID aid) : base(ActionID.MakeSpell(aid), 8) { }
}
class NNothingBesideRemains : NothingBesideRemains { public NNothingBesideRemains() : base(AID.NNothingBesideRemainsAOE) { } }
class SNothingBesideRemains : NothingBesideRemains { public SNothingBesideRemains() : base(AID.SNothingBesideRemainsAOE) { } }
