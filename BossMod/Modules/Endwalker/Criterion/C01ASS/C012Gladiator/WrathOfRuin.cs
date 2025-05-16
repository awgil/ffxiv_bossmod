namespace BossMod.Endwalker.Criterion.C01ASS.C012Gladiator;

class GoldenSilverFlame(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _goldenFlames = [];
    private readonly List<Actor> _silverFlames = [];
    private readonly int[] _debuffs = new int[PartyState.MaxPartySize]; // silver << 16 | gold

    public bool Active => _goldenFlames.Count + _silverFlames.Count > 0;

    private static readonly AOEShapeRect _shape = new(60, 5);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (DebuffsAtPosition(actor.Position) != _debuffs[slot])
            hints.Add("Go to correct cell!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // TODO: implement
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Active)
            foreach (var c in SafeCenters(_debuffs[pcSlot]))
                Arena.ZoneRect(c, new WDir(1, 0), _shape.HalfWidth, _shape.HalfWidth, _shape.HalfWidth, ArenaColor.SafeFromAOE);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        int debuff = (SID)status.ID switch
        {
            SID.GildedFate => status.Extra,
            SID.SilveredFate => status.Extra << 16,
            _ => 0
        };

        if (debuff == 0)
            return;
        if (Raid.TryFindSlot(actor, out var slot))
            _debuffs[slot] |= debuff;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        CasterList(spell)?.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
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

    private IEnumerable<WPos> SafeCenters(int debuff)
    {
        var limit = Module.Center + new WDir(Module.Bounds.Radius, Module.Bounds.Radius);
        var first = Module.Center - new WDir(Module.Bounds.Radius - _shape.HalfWidth, Module.Bounds.Radius - _shape.HalfWidth);
        var advance = 2 * _shape.HalfWidth;
        for (float x = first.X; x < limit.X; x += advance)
            for (float z = first.Z; z < limit.Z; z += advance)
                if (DebuffsAtPosition(new WPos(x, z)) == debuff)
                    yield return new(x, z);
    }
}

// note: actual spell targets location, but it seems to be incorrect...
// note: we can predict cast start during Regret actor spawn...
class RackAndRuin(BossModule module, AID aid) : Components.StandardAOEs(module, aid, new AOEShapeRect(40, 2.5f), 8);
class NRackAndRuin(BossModule module) : RackAndRuin(module, AID.NRackAndRuin);
class SRackAndRuin(BossModule module) : RackAndRuin(module, AID.SRackAndRuin);

class NothingBesideRemains(BossModule module, AID aid) : Components.SpreadFromCastTargets(module, aid, 8);
class NNothingBesideRemains(BossModule module) : NothingBesideRemains(module, AID.NNothingBesideRemainsAOE);
class SNothingBesideRemains(BossModule module) : NothingBesideRemains(module, AID.SNothingBesideRemainsAOE);
