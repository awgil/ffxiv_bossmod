using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Unreal.Un5Thordan;

class HiemalStormSpread : Components.UniformStackSpread
{
    public HiemalStormSpread() : base(0, 6, alwaysShowSpreads: true) { }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.HiemalStorm)
            AddSpread(actor, module.WorldState.CurrentTime.AddSeconds(3));
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.HiemalStormAOE)
            Spreads.Clear();
    }
}

class HiemalStormVoidzone : Components.PersistentVoidzoneAtCastTarget
{
    public HiemalStormVoidzone() : base(6, ActionID.MakeSpell(AID.HiemalStormAOE), m => m.Enemies(OID.HiemalStorm).Where(x => x.EventState != 7), 1.5f) { }
}

class SpiralPierce : Components.BaitAwayTethers
{
    public SpiralPierce() : base(new AOEShapeRect(50, 6), (uint)TetherID.SpiralPierce, ActionID.MakeSpell(AID.SpiralPierce)) { }
}

class DimensionalCollapse : Components.LocationTargetedAOEs
{
    public DimensionalCollapse() : base(ActionID.MakeSpell(AID.DimensionalCollapseAOE), 9) { }
}

class FaithUnmoving : Components.Knockback
{
    public FaithUnmoving() : base(ActionID.MakeSpell(AID.FaithUnmoving), true) { }

    public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
    {
        yield return new(module.Bounds.Center, 16);
    }
}

class CometCircle : Components.Adds
{
    public CometCircle() : base((uint)OID.CometCircle) { }
}

class MeteorCircle : Components.Adds
{
    public MeteorCircle() : base((uint)OID.MeteorCircle) { }
}

class HeavyImpact : Components.GenericAOEs
{
    struct Sequence
    {
        public WPos Origin;
        public Angle Rotation;
        public DateTime NextActivation;
        public int NumCastsDone;
    }

    private List<Sequence> _sequences = new();

    private static AOEShape[] _shapes = { new AOEShapeCone(6.5f, 135.Degrees()), new AOEShapeDonutSector(6.5f, 12.5f, 135.Degrees()), new AOEShapeDonutSector(12.5f, 18.5f, 135.Degrees()), new AOEShapeDonutSector(18.5f, 27.5f, 135.Degrees()) };

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _sequences.Where(s => s.NumCastsDone < _shapes.Length).Select(s => new AOEInstance(_shapes[s.NumCastsDone], s.Origin, s.Rotation, s.NextActivation));

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HeavyImpactAOE1)
            _sequences.Add(new() { Origin = caster.Position, Rotation = spell.Rotation, NextActivation = spell.FinishAt });
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        var order = (AID)spell.Action.ID switch
        {
            AID.HeavyImpactAOE1 => 0,
            AID.HeavyImpactAOE2 => 1,
            AID.HeavyImpactAOE3 => 2,
            AID.HeavyImpactAOE4 => 3,
            _ => -1
        };
        if (order >= 0)
        {
            ++NumCasts;

            var index = _sequences.FindIndex(s => s.NumCastsDone == order && s.Origin.AlmostEqual(caster.Position, 1) && s.Rotation.AlmostEqual(caster.Rotation, 0.05f));
            if (index >= 0)
            {
                ref var s = ref _sequences.AsSpan()[index];
                ++s.NumCastsDone;
                s.NextActivation = module.WorldState.CurrentTime.AddSeconds(2);
            }
            else
            {
                module.ReportError(this, $"Unexpected ring {order}");
            }
        }
    }
}
