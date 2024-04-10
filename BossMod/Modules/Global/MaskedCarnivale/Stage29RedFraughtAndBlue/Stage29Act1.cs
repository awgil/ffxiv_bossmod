namespace BossMod.Global.MaskedCarnivale.Stage29.Act1;

public enum OID : uint
{
    Boss = 0x2C5B, //R=3.0
    FireTornado = 0x2C5C, // R=4.0
    Helper = 0x233C,
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    Unknown = 18872, // Boss->self, no cast, single-target
    FluidSwing = 18689, // Boss->self/player, 4,0s cast, range 11 30-degree cone, interruptible, knockback 50, source forward
    SeaOfFlamesVisual = 18693, // Boss->self, 3,0s cast, single-target
    SeaOfFlames = 18694, // Helper->location, 3,0s cast, range 6 circle
    Pyretic = 18691, // Boss->self, 4,0s cast, range 80 circle, applies pyretic
    FireII = 18692, // Boss->location, 4,0s cast, range 5 circle
    PillarOfFlameVisual = 18695, // Boss->self, 3,0s cast, single-target
    PillarOfFlame = 18696, // Helper->location, 3,0s cast, range 8 circle
    PillarOfFlameVisual2 = 18894, // Boss->self, 6,0s cast, single-target
    PillarOfFlame2 = 18895, // Helper->location, 6,0s cast, range 8 circle
    Rush = 18690, // Boss->player, 5,0s cast, width 4 rect charge, does distance based damage, seems to scale all the way until the other side of the arena
    FlareStarVisual = 18697, // Boss->self, 5,0s cast, single-target
    FlareStar = 18698, // Helper->location, 5,0s cast, range 40 circle, distance based AOE, radius 10 seems to be a good compromise
    FireBlast = 18699, // FireTornado->self, 3,0s cast, range 70+R width 4 rect
}

public enum SID : uint
{
    Pyretic = 960, // Boss->player, extra=0x0

}

class FluidSwing() : Components.CastInterruptHint(ActionID.MakeSpell(AID.FluidSwing));
class FluidSwingKnockback() : Components.KnockbackFromCastTarget(ActionID.MakeSpell(AID.FluidSwing), 50, kind: Kind.DirForward);
class SeaOfFlames() : Components.LocationTargetedAOEs(ActionID.MakeSpell(AID.SeaOfFlames), 6);
class FireII() : Components.LocationTargetedAOEs(ActionID.MakeSpell(AID.FireII), 5);
class PillarOfFlame() : Components.LocationTargetedAOEs(ActionID.MakeSpell(AID.PillarOfFlame), 8);
class PillarOfFlame2() : Components.LocationTargetedAOEs(ActionID.MakeSpell(AID.PillarOfFlame2), 8);
class Rush() : Components.CastHint(ActionID.MakeSpell(AID.Rush), "GTFO from boss! (Distance based charge)");
class FlareStar() : Components.LocationTargetedAOEs(ActionID.MakeSpell(AID.FlareStar), 10);
class FireBlast() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.FireBlast), new AOEShapeRect(74, 2));
class PyreticHint() : Components.CastHint(ActionID.MakeSpell(AID.Pyretic), "Pyretic, stop everything! Dodge the AOE after it runs out.");

class Pyretic : Components.StayMove
{
    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.Pyretic)
        {
            if (module.Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0 && slot < Requirements.Length)
                Requirements[slot] = Requirement.Stay;
        }
    }

    public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.Pyretic)
        {
            if (module.Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0 && slot < Requirements.Length)
                Requirements[slot] = Requirement.None;
        }
    }
}

class Hints : BossComponent
{
    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        hints.Add($"{module.PrimaryActor.Name} will spawn upto 4 waves of adds which are weak to fire.\nA way to quickly heal yourself to full is mandatory and\na ranged fire ability such as Mustard Bomb are highly recommended.");
    }
}

class Stage28States : StateMachineBuilder
{
    public Stage28States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FluidSwing>()
            .ActivateOnEnter<FluidSwingKnockback>()
            .ActivateOnEnter<SeaOfFlames>()
            .ActivateOnEnter<FireII>()
            .ActivateOnEnter<PillarOfFlame>()
            .ActivateOnEnter<PillarOfFlame2>()
            .ActivateOnEnter<Rush>()
            .ActivateOnEnter<FlareStar>()
            .ActivateOnEnter<FireBlast>()
            .ActivateOnEnter<Pyretic>()
            .ActivateOnEnter<PyreticHint>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 698, NameID = 9239)]
public class Stage28 : BossModule
{
    public Stage28(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 16))
    {
        ActivateComponent<Hints>();
    }
}