namespace BossMod.Endwalker.Criterion.C02AMR.C023Moko;

class AzureAuspice : Components.SelfTargetedAOEs
{
    public AzureAuspice(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeDonut(6, 40)) { } // TODO: verify inner radius
}
class NAzureAuspice : AzureAuspice { public NAzureAuspice() : base(AID.NAzureAuspice) { } }
class SAzureAuspice : AzureAuspice { public SAzureAuspice() : base(AID.SAzureAuspice) { } }

class BoundlessAzure : Components.SelfTargetedAOEs
{
    public BoundlessAzure(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeRect(30, 5, 30)) { }
}
class NBoundlessAzure : BoundlessAzure { public NBoundlessAzure() : base(AID.NBoundlessAzureAOE) { } }
class SBoundlessAzure : BoundlessAzure { public SBoundlessAzure() : base(AID.SBoundlessAzureAOE) { } }

// note: each initial line sends out two 'exaflares' to the left & right
// each subsequent exaflare moves by distance 5, and happen approximately 2s apart
// each wave is 5 subsequent lines, except for two horizontal ones that go towards edges - they only have 1 line - meaning there's a total 32 'rest' casts
class Upwell : Components.GenericAOEs
{
    private class LineSequence
    {
        public WPos NextOrigin;
        public WDir Advance;
        public Angle Rotation;
        public DateTime NextActivation;
        public AOEShapeRect? NextShape; // wide for first line, null for first line mirror, narrow for remaining lines
    }

    private List<LineSequence> _lines = new();

    private static readonly AOEShapeRect _shapeWide = new(30, 5, 30);
    private static readonly AOEShapeRect _shapeNarrow = new(30, 2.5f, 30);

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        // TODO: think about imminent/future color/risk, esp for overlapping lines
        var imminentDeadline = module.WorldState.CurrentTime.AddSeconds(5);
        foreach (var l in _lines)
            if (l.NextShape != null && l.NextActivation <= imminentDeadline)
                yield return new(l.NextShape, l.NextOrigin, l.Rotation, l.NextActivation);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NUpwellFirst or AID.SUpwellFirst)
        {
            var advance = spell.Rotation.ToDirection().OrthoR() * 5;
            _lines.Add(new() { NextOrigin = caster.Position, Advance = advance, Rotation = spell.Rotation, NextActivation = spell.NPCFinishAt, NextShape = _shapeWide });
            _lines.Add(new() { NextOrigin = caster.Position, Advance = -advance, Rotation = (spell.Rotation + 180.Degrees()).Normalized(), NextActivation = spell.NPCFinishAt });
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NUpwellFirst or AID.SUpwellFirst)
        {
            ++NumCasts;
            var index = _lines.FindIndex(l => l.NextOrigin.AlmostEqual(caster.Position, 1) && l.NextShape == _shapeWide && l.Rotation.AlmostEqual(spell.Rotation, 0.1f));
            if (index < 0 || index + 1 >= _lines.Count)
            {
                module.ReportError(this, $"Unexpected exaline end");
            }
            else
            {
                Advance(module, _lines[index]);
                Advance(module, _lines[index + 1]);
            }
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NUpwellRest or AID.SUpwellRest)
        {
            ++NumCasts;
            var index = _lines.FindIndex(l => l.NextOrigin.AlmostEqual(caster.Position, 1) && l.NextShape == _shapeNarrow && l.Rotation.AlmostEqual(caster.Rotation, 0.1f));
            if (index < 0)
            {
                module.ReportError(this, $"Unexpected exaline @ {caster.Position} / {caster.Rotation}");
            }
            else
            {
                Advance(module, _lines[index]);
            }
        }
    }

    private void Advance(BossModule module, LineSequence line)
    {
        line.NextOrigin += line.Advance;
        line.NextActivation = module.WorldState.CurrentTime.AddSeconds(2);
        var offset = (line.NextOrigin - module.Bounds.Center).Abs();
        line.NextShape = offset.X < 19 && offset.Z < 19 ? _shapeNarrow : null;
    }
}
