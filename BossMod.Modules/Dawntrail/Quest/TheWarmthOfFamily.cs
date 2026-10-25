namespace BossMod.Dawntrail.Quest.TheWarmthOfFamily;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x21, Helper type
    Boss = 0x4638, // R5.950, x1
    ScaleArmoredLeg = 0x4648, // R5.950, x0 (spawn during fight), Part type
    BallOfFire = 0x4639, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->4631, no cast, single-target
    Ensnare = 40566, // Boss->location, 5.0s cast, range 6 circle
    Regenerate = 40583, // Boss->self, 4.0s cast, single-target
    SearingSwellAOE = 40579, // Helper->self, 4.0s cast, range 40 45-degree cone
    SearingSwellCast = 40577, // Boss->self, 4.0s cast, single-target
    SearingSwellInstant = 40578, // Boss->self, no cast, single-target
    FlameBlast = 40580, // 4639->self, 2.0s cast, range 40 width 4 rect
    TriceraSnareCast = 40573, // Boss->self, 5.0s cast, single-target
    TriceraSnare = 40574, // Boss->player/4630/4631, no cast, range 6 circle
    Unk1 = 40664, // Helper->Boss, 1.2s cast, single-target
    RazingRoar1 = 40586, // Boss->self, no cast, single-target
    RazingRoar2 = 41245, // Helper->player, no cast, single-target
    RazingRoar3 = 41247, // Helper->4630, 0.5s cast, single-target
    RazingRoar4 = 41246, // Helper->4631, 0.5s cast, single-target
    Unk2 = 40570, // Boss->self, no cast, single-target
    FirestormCast = 40575, // Boss->self, 5.0s cast, single-target
    Firestorm = 40576, // Helper->self, 5.0s cast, range 10 circle
    CandescentRay = 40572, // Boss->self/players, 8.0s cast, range 50 width 8 rect
    PrimordialRoar1 = 41271, // Boss->self, 5.0s cast, range 40 circle
    PrimordialRoar2 = 40584, // Boss->self, 5.0s cast, range 40 circle
    Aethercall = 40581, // Boss->self, 5.0s cast, single-target
    OrbAbsorb1 = 40582, // 463A->4630/4631, no cast, single-target
    OrbAbsorb2 = 41117, // 463A->Boss, no cast, single-target
}

public enum SID : uint
{
    Unk2056 = 2056, // none->4630/Boss, extra=0x32C/0x33C
    AdamantScales = 4173, // none->4648, extra=0x0
    InEvent = 1268, // none->player, extra=0x0
    Rehabilitation = 279, // Boss->Boss, extra=0x0
    BloodOfTheBraax = 4170, // 4631->4631, extra=0x64
    LuwatenaPulse = 3785, // 4631->4631, extra=0x0
}

public enum IconID : uint
{
    Spread = 140, // 4630/4631/player->self
    Laser = 523, // Boss->player
}

public enum TetherID : uint
{
    Holmgang = 294, // 4631->Boss
    Analysis = 295, // 4630->Boss
    Orb = 2, // 463A->Boss
}

class AdamantScales(BossModule module) : Components.InvincibleStatus(module, (uint)SID.AdamantScales)
{
    protected override IEnumerable<Actor> ForbiddenTargets(int slot, Actor actor) => base.ForbiddenTargets(slot, actor).Where(t => (OID)t.OID != OID.ScaleArmoredLeg);
}
class ScaleArmoredLeg(BossModule module) : Components.Adds(module, (uint)OID.ScaleArmoredLeg, 0, true);

class Ensnare(BossModule module) : Components.StandardAOEs(module, AID.Ensnare, 6);
class SearingSwell(BossModule module) : Components.StandardAOEs(module, AID.SearingSwellAOE, new AOEShapeCone(40, 22.5f.Degrees()));
class FlameBlast(BossModule module) : Components.StandardAOEs(module, AID.FlameBlast, new AOEShapeRect(40, 2));
class TriceraSnare(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spread, AID.TriceraSnare, 6, 5.2f);
class Firestorm(BossModule module) : Components.StandardAOEs(module, AID.Firestorm, 10, 5);
class CandescentRay(BossModule module) : Components.IconLineStack(module, 4, 50, (uint)IconID.Laser, AID.CandescentRay, 8.1f);
class PrimordialRoar1(BossModule module) : Components.RaidwideCast(module, AID.PrimordialRoar1);
class PrimordialRoar2(BossModule module) : Components.RaidwideCast(module, AID.PrimordialRoar2);

class Orb(BossModule module) : BossComponent(module)
{
    readonly List<Actor> _orbs = [];

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var orb in _orbs)
            hints.GoalZones.Add(AIHints.GoalSingleTarget(orb.Position + orb.Rotation.ToDirection() * 2, 2, 0.5f));
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var orb in _orbs)
            Arena.ZoneCircle(orb.Position, 2, ArenaColor.SafeFromAOE);
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.Orb && source.OID == 0x463A)
            _orbs.Add(source);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.OrbAbsorb1 or AID.OrbAbsorb2)
            _orbs.Remove(caster);
    }
}

class TturuhhetsoStates : StateMachineBuilder
{
    public TturuhhetsoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AdamantScales>()
            .ActivateOnEnter<ScaleArmoredLeg>()
            .ActivateOnEnter<Ensnare>()
            .ActivateOnEnter<SearingSwell>()
            .ActivateOnEnter<FlameBlast>()
            .ActivateOnEnter<TriceraSnare>()
            .ActivateOnEnter<Firestorm>()
            .ActivateOnEnter<CandescentRay>()
            .ActivateOnEnter<PrimordialRoar1>()
            .ActivateOnEnter<PrimordialRoar2>()
            .ActivateOnEnter<Orb>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70785, NameID = 13594)]
public class Tturuhhetso(ModuleInit init) : BossModule(init, new(395.737f, -45.356f), new ArenaBoundsCircle(20));

