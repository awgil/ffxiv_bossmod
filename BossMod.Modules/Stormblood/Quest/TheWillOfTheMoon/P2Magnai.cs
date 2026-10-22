using BossMod.QuestBattle;

namespace BossMod.Stormblood.Quest.TheWillOfTheMoon.P2Magnai;

public enum OID : uint
{
    Boss = 0x24A1,
    Helper = 0x233C,
    Hien = 0x24A3,
    Daidukul = 0x24A2, // R0.500, x1
    TheScaleOfTheFather = 0x2532, // R1.000, x0 (spawn during fight)
    TheScaleOfTheFatherInvincible = 0x249F, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoMagnai = 870, // Boss->player, no cast, single-target
    AutoDaidukul = 872, // Daidukul->Hien, no cast, single-target
    FlatlandFury = 13244, // 2532->self, 17.0s cast, range 10 circle
    FlatlandFuryEnrage = 13329, // 249F->self, 25.0s cast, range 10 circle
    ViolentEarth = 13236, // 233C->location, 3.0s cast, range 6 circle
    WindChisel = 13518, // 233C->self, 2.0s cast, range 34+R 20-degree cone
    TranquilAnnihilation = 13233, // _Gen_DaidukulTheMirthful->24A3, 15.0s cast, single-target
    DispassionateKick = 13232, // Daidukul->Hien, no cast, single-target
    AzimZephyr = 13230, // Boss->player, no cast, single-target
    PlacidPunch = 13231, // Daidukul->Hien, no cast, single-target
    TheScaleOfTheFather = 13347, // Boss->self, 3.0s cast, single-target
    FlatlandFuryFastCast = 13838, // Boss->self, 14.0s cast, single-target
    ViolentEarthBossCast = 13235, // Boss->self, 1.5s cast, single-target
    ViolentEarthBossInstant = 13237, // Boss->self, no cast, single-target
    Tomahawk = 13238, // Boss->Hien, 3.0s cast, single-target
    WindChiselCast = 13517, // Boss->self, 2.0s cast, single-target
    BrokenRidge = 13239, // Boss->self, 5.0s cast, range 50 circle
    FlatlandFurySlowCast = 13833, // Boss->self, 23.0s cast, single-target
    TheOneRuler = 13240, // Boss->self, no cast, single-target
}

class Daidukul(BossModule module) : Components.AddsPointless(module, (uint)OID.Daidukul);
class FlatlandFury(BossModule module) : Components.StandardAOEs(module, AID.FlatlandFury, new AOEShapeCircle(10))
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // if all axes are alive, kill the closest
        if (Casters.Count == 9)
            hints.FindEnemy(Casters.MinBy(c => actor.DistanceToHitbox(c)))?.Priority = 2;
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class ViolentEarth(BossModule module) : Components.StandardAOEs(module, AID.ViolentEarth, 6);
class WindChisel(BossModule module) : Components.StandardAOEs(module, AID.WindChisel, new AOEShapeCone(34, 10.Degrees()));

class Scales(BossModule module) : Components.AddsMulti(module, [OID.TheScaleOfTheFather, OID.TheScaleOfTheFatherInvincible]);

class FollowHien(BossModule module) : Components.GenericAOEs(module)
{
    DateTime _deadline;
    WPos? _destination;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_destination).Select(p => new AOEInstance(new AOEShapeCircle(2), p, default, _deadline, Inverted: true));

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == 0x1EA1A1 && state == 0x00010002)
        {
            _destination = actor.Position;
            _deadline = Module.CastFinishAt(WorldState.Actors.FirstOrDefault(a => a.CastInfo?.IsSpell(AID.FlatlandFuryEnrage) == true)?.CastInfo);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.FlatlandFuryEnrage)
            _destination = null;
    }
}

class AutoYshtola(WorldState ws) : UnmanagedRotation(ws, 25)
{
    private Actor Magnai => World.Actors.First(x => (OID)x.OID == OID.Boss);
    private Actor Hien => World.Actors.First(x => (OID)x.OID == OID.Hien);
    private Actor? Daidukul => World.Actors.FirstOrDefault(x => (OID)x.OID == OID.Daidukul);

    protected override void Exec(Actor? primaryTarget)
    {
        var hienMinHP = Daidukul?.CastInfo?.Action.ID == (uint)AID.TranquilAnnihilation
            ? 28000
            : 10000;

        if (Hien.PendingHPRaw < hienMinHP)
        {
            Hints.GoalZones.Add(AIHints.GoalSingleTarget(Hien.Position, 25, 0.5f));
            UseAction(Roleplay.AID.CureIISeventhDawn, Hien);
        }

        var aero = StatusDetails(Magnai, WHM.SID.AeroII, Player.InstanceID);
        if (aero.Left < 4.6f)
            UseAction(Roleplay.AID.AeroIISeventhDawn, Magnai);

        UseAction(Roleplay.AID.StoneIVSeventhDawn, primaryTarget);

        if (Player.HPMP.CurMP < 5000)
            UseAction(Roleplay.AID.Aetherwell, Player);
    }
}

class YshtolaAI(BossModule module) : RotationModule<AutoYshtola>(module);

class MagnaiTheOlderStates : StateMachineBuilder
{
    public MagnaiTheOlderStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<YshtolaAI>()
            .ActivateOnEnter<Daidukul>()
            .ActivateOnEnter<Scales>()
            .ActivateOnEnter<FollowHien>()
            .ActivateOnEnter<FlatlandFury>()
            .ActivateOnEnter<ViolentEarth>()
            .ActivateOnEnter<WindChisel>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68683, NameID = 6153)]
public class MagnaiTheOlder(ModuleInit init) : BossModule(init, new(-186.572f, 550.737f), new ArenaBoundsCircle(19.5f))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.PrioritizeTargetsByOID(OID.Boss, 1);
    }
}
