namespace BossMod.Shadowbringers.Foray.DelubrumReginae.Normal.DRN2Dahu;

class FallingRock(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.FallingRock), 4);
class HotCharge(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.HotCharge), 4);
class Firebreathe(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Firebreathe), new AOEShapeCone(60, 45.Degrees()));
class HeadDown(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.HeadDown), 2);
class HuntersClaw(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HuntersClaw), new AOEShapeCircle(8));

class FeralHowl(BossModule module) : Components.Knockback(module)
{
    private Actor? _source;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_source != null)
            yield return new(_source.Position, 30, _source.CastInfo!.NPCFinishAt);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.FeralHowl)
            _source = caster;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.FeralHowl)
        {
            _source = null;
            ++NumCasts;
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 760, NameID = 9751)]
public class DRN2Dahu(WorldState ws, Actor primary) : BossModule(ws, primary, new(82, 138), new ArenaBoundsCircle(30))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actors(Enemies(OID.Marchosias), ArenaColor.Enemy);
    }
}
