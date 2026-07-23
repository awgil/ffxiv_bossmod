namespace BossMod.Endwalker.Criterion.C02AMR.C023Moko;

class AzureAuspice(BossModule module, AID aid) : Components.StandardAOEs(module, aid, new AOEShapeDonut(6, 40)); // TODO: verify inner radius
class NAzureAuspice(BossModule module) : AzureAuspice(module, AID.NAzureAuspice);
class SAzureAuspice(BossModule module) : AzureAuspice(module, AID.SAzureAuspice);

class BoundlessAzure(BossModule module, AID aid) : Components.StandardAOEs(module, aid, new AOEShapeRect(60, 5));
class NBoundlessAzure(BossModule module) : BoundlessAzure(module, AID.NBoundlessAzureAOE);
class SBoundlessAzure(BossModule module) : BoundlessAzure(module, AID.SBoundlessAzureAOE);

// note: each initial line sends out two 'exaflares' to the left & right
// each subsequent exaflare moves by distance 5, and happen approximately 2s apart
// each wave is 5 subsequent lines, except for two horizontal ones that go towards edges - they only have 1 line - meaning there's a total 32 'rest' casts
class Upwell(BossModule module) : Components.Exaflare(module, _shapeNarrow)
{
    public bool BlocksFollowingMechanics { get => field && !(_initial.Count == 0 && (Lines.Count == 0 || Lines.All(l => l.ExplosionsLeft <= 2))); private set; }
    private static readonly AOEShapeRect _shapeWide = new(30, 5, 30);
    private static readonly AOEShapeRect _shapeNarrow = new(30, 2.5f, 30);

    private readonly List<AOEInstance> _initial = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        for (var i = 0; i < _initial.Count; ++i)
        {
            var aoe = _initial[i];

            yield return i == 0 ? aoe with { Color = ArenaColor.Danger } : aoe;
        }

        foreach (var aoe in base.ActiveAOEs(slot, actor))
            yield return aoe;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is not (AID.NUpwellFirst or AID.SUpwellFirst))
            return;

        BlocksFollowingMechanics = true;

        _initial.Add(new(_shapeWide, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is not (AID.NUpwellFirst or AID.SUpwellFirst))
            return;

        ++NumCasts;

        var index = _initial.FindIndex(aoe =>
            aoe.Origin.AlmostEqual(caster.Position, 1) &&
            aoe.Rotation.AlmostEqual(spell.Rotation, 0.1f));

        if (index < 0)
            return;

        _initial.RemoveAt(index);

        var advance = spell.Rotation.ToDirection().OrthoR() * 5;
        var nextExplosion = WorldState.FutureTime(2);

        Lines.Add(new()
        {
            Next = caster.Position + advance,
            Rotation = spell.Rotation,
            Advance = advance,
            NextExplosion = nextExplosion,
            TimeToMove = 2,
            ExplosionsLeft = NumExplosions(caster.Position + advance, advance),

            MaxShownExplosions = 1
        });

        Lines.Add(new()
        {
            Next = caster.Position - advance,
            Rotation = (spell.Rotation + 180.Degrees()).Normalized(),
            Advance = -advance,
            NextExplosion = nextExplosion,
            TimeToMove = 2,
            ExplosionsLeft = NumExplosions(caster.Position - advance, -advance),
            MaxShownExplosions = 1
        });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is not (AID.NUpwellRest or AID.SUpwellRest))
            return;

        ++NumCasts;

        var index = Lines.FindIndex(line =>
            line.Next.AlmostEqual(caster.Position, 1) &&
            line.Rotation.AlmostEqual(caster.Rotation, 0.1f));

        if (index < 0)
            return;

        AdvanceLine(Lines[index], caster.Position);

        if (Lines[index].ExplosionsLeft == 0)
            Lines.RemoveAt(index);
    }

    private int NumExplosions(WPos origin, WDir advance)
    {
        int count = 0;

        while (true)
        {
            var offset = (origin - Module.Center).Abs();
            if (offset.X >= 19 || offset.Z >= 19)
                break;

            ++count;
            origin += advance;
        }

        return count;
    }
}
