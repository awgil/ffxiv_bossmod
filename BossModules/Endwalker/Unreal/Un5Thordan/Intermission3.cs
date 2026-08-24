namespace BossMod.Endwalker.Unreal.Un5Thordan;

class HiemalStormSpread(BossModule module) : Components.UniformStackSpread(module, 0, 6, alwaysShowSpreads: true)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.HiemalStorm)
            AddSpread(actor, WorldState.FutureTime(3));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.HiemalStormAOE)
            Spreads.Clear();
    }
}

class HiemalStormVoidzone(BossModule module) : Components.PersistentVoidzone(module, 6, m => m.Enemies(OID.HiemalStorm).Where(x => x.EventState != 7));
class SpiralPierce(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeRect(50, 6), (uint)TetherID.SpiralPierce, AID.SpiralPierce);
class DimensionalCollapse(BossModule module) : Components.StandardAOEs(module, AID.DimensionalCollapseAOE, 9);

class FaithUnmoving(BossModule module) : Components.Knockback(module, AID.FaithUnmoving, true)
{
    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        yield return new(Module.Center, 16);
    }
}

class CometCircle(BossModule module) : Components.Adds(module, (uint)OID.CometCircle);
class MeteorCircle(BossModule module) : Components.Adds(module, (uint)OID.MeteorCircle);

class HeavyImpact(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCone(6.5f, 135.Degrees()), new AOEShapeDonutSector(6.5f, 12.5f, 135.Degrees()), new AOEShapeDonutSector(12.5f, 18.5f, 135.Degrees()), new AOEShapeDonutSector(18.5f, 27.5f, 135.Degrees())];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HeavyImpactAOE1)
            AddSequence(caster.Position, Module.CastFinishAt(spell), spell.Rotation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var order = (AID)spell.Action.ID switch
        {
            AID.HeavyImpactAOE1 => 0,
            AID.HeavyImpactAOE2 => 1,
            AID.HeavyImpactAOE3 => 2,
            AID.HeavyImpactAOE4 => 3,
            _ => -1
        };
        if (!AdvanceSequence(order, caster.Position, WorldState.FutureTime(2), caster.Rotation))
            ReportError($"Unexpected ring {order}");
    }
}
