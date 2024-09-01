namespace BossMod.Heavensward.Dungeon.D02SohmAl.D023Tioman;
public enum OID : uint
{
    Boss = 0xE96, // R6.840, x?
    LeftWingOfTragedy = 0x10B4, // Spawn During the Fight
    RightWingOfInjury = 0x10B5, // Spawn During the Fight
    Helper = 0x233C, // Helper

}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    AbyssicBuster = 3811, // E96->self, no cast, range 25+R ?-degree cone
    ChaosBlast = 3813, // E96->location, 2.0s cast, range 2 circle
    ChaosBlast2 = 3819, // 1B2->self, 2.0s cast, range 50+R width 4 rect
    Comet = 3816, // 1B2->location, 3.0s cast, range 4 circle
    Comet2 = 3814, // E96->self, 4.0s cast, single-target
    MeteorBait = 4999, // 1B2->self, 3.5s cast, range 30+R circle
    MeteorImpact = 4997, // 13AD->self, no cast, range 30 circle
    HeavensfallVisual = 3815, // E96->self, no cast, single-target
    Heavensfall2 = 3817, // 1B2->player, no cast, range 5 circle
    Heavensfall = 3818, // 1B2->location, 3.0s cast, range 5 circle
    HypothermalCombustion = 3156, // F2D->self, 4.0s cast, range 8+R circle
    DarkStar = 3812, // E96->self, 5.0s cast, range 50+R circle
}
public enum GID : uint
{
    Invincibility = 325,
}
public enum IconID : uint
{
    Comet = 10, // player
    Meteor = 7, // player
}

class AbyssicBuster(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.AbyssicBuster), new AOEShapeCone(25, 45.Degrees()))
{
    private readonly List<Actor> _boss = [];
    private IEnumerable<(Actor origin, Actor target, Angle angle)> OriginsAndTargets()
    {
        foreach (var b in _boss)
        {
            if (b.IsDead || !ActiveForUntargetable && !b.IsTargetable || !ActiveWhileCasting && b.CastInfo != null || b.FindStatus(GID.Invincibility) != null)
                continue;

            var target = WorldState.Actors.Find(b.TargetID);
            if (target != null)
            {
                yield return (OriginAtTarget ? target : b, target, Angle.FromDirection(target.Position - b.Position));
            }
        }
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var (origin, target, angle) in OriginsAndTargets())
        {
            if (actor != target)
            {
                hints.AddForbiddenZone(Shape, origin.Position, angle, NextExpected);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var e in OriginsAndTargets())
        {
            Shape.Outline(Arena, e.origin.Position, e.angle);
        }
    }
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.Boss)
        {
            _boss.Add(actor);
        }
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID is OID.Boss)
        {
            _boss.Remove(actor);
        }
    }
};
class ChaosBlast(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.ChaosBlast), 2);
class ChaosBlast2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ChaosBlast2), new AOEShapeRect(50, 2));
class Comet(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Comet), 4);
class MeteorBait(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(30), (uint)IconID.Meteor, ActionID.MakeSpell(AID.MeteorBait), 9.1f, true)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.MeteorBait)
        {
            CurrentBaits.Clear();
        }
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (ActiveBaits.Any(x => x.Target == actor))
            hints.AddForbiddenZone(new AOEShapeCircle(23), Module.Center);
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell) { }
}

class MeteorImpact(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _meteors = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _meteors;
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.MeteorBait)
        {
            _meteors.Add(new(new AOEShapeCircle(15), caster.Position, Activation: Module.CastFinishAt(spell, 1f)));
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.MeteorImpact)
        {
            _meteors.Clear();
        }
    }
}
class Heavensfall(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Heavensfall), 5);
class DarkStar(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DarkStar));
class MultiAddModule(BossModule module) : Components.AddsMulti(module, [(uint)OID.LeftWingOfTragedy, (uint)OID.RightWingOfInjury])
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.RightWingOfInjury or OID.LeftWingOfTragedy => 2,
                OID.Boss => 1,
                _ => 0
            };
    }
};
class D023TiomanStates : StateMachineBuilder
{
    public D023TiomanStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AbyssicBuster>()
            .ActivateOnEnter<ChaosBlast>()
            .ActivateOnEnter<ChaosBlast2>()
            .ActivateOnEnter<Comet>()
            .ActivateOnEnter<MeteorBait>()
            .ActivateOnEnter<MeteorImpact>()
            .ActivateOnEnter<Heavensfall>()
            .ActivateOnEnter<DarkStar>()
            .ActivateOnEnter<MultiAddModule>();

    }
}
[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 37, NameID = 3798)]
public class D023Tioman(WorldState ws, Actor primary) : BossModule(ws, primary, new(-103, -395), new ArenaBoundsCircle(27f));
