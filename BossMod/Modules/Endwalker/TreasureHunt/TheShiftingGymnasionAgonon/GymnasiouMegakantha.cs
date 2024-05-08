namespace BossMod.Endwalker.TreasureHunt.ShiftingGymnasionAgonon.GymnasiouMegakantha;

public enum OID : uint
{
    Boss = 0x3D33, //R=6
    BonusAddLyssa = 0x3D4E, //R=3.75, bonus loot adds
    BossHelper = 0x233C,
    BossAdd1 = 0x3D35, //R=1.76 
    BossAdd2 = 0x3D36, //R=1.56
    BonusAddLampas = 0x3D4D, //R=2.001, bonus loot adds
}

public enum AID : uint
{
    AutoAttack = 870, // BonusAddLyssa->player, no cast, single-target
    AutoAttack2 = 872, // Boss/BossAdd2/BossAdd1->player, no cast, single-target
    OdiousAtmosphereComboStart = 32199, // Boss->self, no cast, single-target
    OdiousAtmosphere0 = 32241, // Boss->self, 4.0s cast, single-target
    OdiousAtmosphere1 = 32242, // BossHelper->self, 5.3s cast, range 40 180-degree cone
    OdiousAtmosphere2 = 33015, // BossHelper->self, 5.3s cast, range 40 180-degree cone
    OdiousAtmosphere3 = 33016, // BossHelper->self, 3.0s cast, range 40 180-degree cone
    SludgeBomb = 32239, // Boss->self, 3.0s cast, single-target
    SludgeBomb2 = 32240, // BossHelper->location, 3.0s cast, range 8 circle
    RustlingWind = 32244, // BossAdd2->self, 3.0s cast, range 15 width 4 rect
    AcidMist = 32243, // BossAdd1->self, 2.5s cast, range 6 circle
    VineWhip = 32238, // Boss->player, 5.0s cast, single-target
    OdiousAir = 32237, // Boss->self, 3.0s cast, range 12 120-degree cone

    HeavySmash = 32317, // BossAdd->location, 3.0s cast, range 6 circle
    Telega = 9630, // BonusAdds->self, no cast, single-target, bonus add disappear
}

class HeavySmash(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.HeavySmash), 6);
class SludgeBomb(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.SludgeBomb2), 8);
class RustlingWind(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RustlingWind), new AOEShapeRect(15, 2));
class AcidMist(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AcidMist), new AOEShapeCircle(6));
class OdiousAir(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.OdiousAir), new AOEShapeCone(12, 60.Degrees()));
class VineWhip(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.VineWhip));

class OdiousAtmosphere(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.OdiousAtmosphere0)
            _aoe = new(new AOEShapeCone(40, 90.Degrees()), caster.Position, spell.Rotation, spell.NPCFinishAt);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.OdiousAtmosphere0:
            case AID.OdiousAtmosphere1:
            case AID.OdiousAtmosphere2:
            case AID.OdiousAtmosphere3:
                if (++NumCasts == 6)
                {
                    _aoe = null;
                    NumCasts = 0;
                }
                break;
        }
    }
}

class MegakanthaStates : StateMachineBuilder
{
    public MegakanthaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SludgeBomb>()
            .ActivateOnEnter<RustlingWind>()
            .ActivateOnEnter<VineWhip>()
            .ActivateOnEnter<OdiousAir>()
            .ActivateOnEnter<OdiousAtmosphere>()
            .ActivateOnEnter<AcidMist>()
            .ActivateOnEnter<HeavySmash>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BossAdd1).All(e => e.IsDead) && module.Enemies(OID.BossAdd2).All(e => e.IsDead) && module.Enemies(OID.BonusAddLyssa).All(e => e.IsDead) && module.Enemies(OID.BonusAddLampas).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 909, NameID = 12009)]
public class Megakantha(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.BossAdd1))
            Arena.Actor(s, ArenaColor.Object);
        foreach (var s in Enemies(OID.BossAdd2))
            Arena.Actor(s, ArenaColor.Object);
        foreach (var s in Enemies(OID.BonusAddLyssa))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.BonusAddLampas))
            Arena.Actor(s, ArenaColor.Vulnerable);
    }

    public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.CalculateAIHints(slot, actor, assignment, hints);
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.BonusAddLampas => 4,
                OID.BonusAddLyssa => 3,
                OID.BossAdd1 or OID.BossAdd2 => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
