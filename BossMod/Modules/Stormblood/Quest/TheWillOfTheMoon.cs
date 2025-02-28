using BossMod.QuestBattle;
using RPID = BossMod.Roleplay.AID;

namespace BossMod.Stormblood.Quest.TheWillOfTheMoon;

public enum OID : uint
{
    Boss = 0x24A0,
    Magnai = 0x24A1,
    Helper = 0x233C,
    KhunShavar = 0x252F, // R1.820, x0 (spawn during fight)
    Hien = 0x24A3,
    Daidukul = 0x24A2, // R0.500, x1
    TheScaleOfTheFather = 0x2532, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    DispellingWind = 13223, // Boss->self, 3.0s cast, range 40+R width 8 rect
    Epigraph = 13225, // 252D->self, 3.0s cast, range 45+R width 8 rect
    WhisperOfLivesPast = 13226, // 252E->self, 3.5s cast, range -12 donut
    AncientBlizzard = 13227, // 252F->self, 3.0s cast, range 40+R 45-degree cone
    Tornado = 13228, // 252F->location, 5.0s cast, range 6 circle
    Epigraph2 = 13222, // 2530->self, 3.0s cast, range 45+R width 8 rect
    FlatlandFury = 13244, // 2532->self, 17.0s cast, range 10 circle
    FlatlandFuryEnrage = 13329, // 249F->self, 25.0s cast, range 10 circle
    ViolentEarth = 13236, // 233C->location, 3.0s cast, range 6 circle
    WindChisel = 13518, // 233C->self, 2.0s cast, range 34+R 20-degree cone
    TranquilAnnihilation = 13233, // _Gen_DaidukulTheMirthful->24A3, 15.0s cast, single-target
}

public enum SID : uint
{
    Invincibility = 775, // none->Boss, extra=0x0
}

class DispellingWind(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DispellingWind), new AOEShapeRect(40, 4));
class Epigraph(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Epigraph), new AOEShapeRect(45, 4));
class Whisper(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WhisperOfLivesPast), new AOEShapeDonut(6, 12));
class Blizzard(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AncientBlizzard), new AOEShapeCone(40, 22.5f.Degrees()));
class Tornado(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Tornado), 6);
class Epigraph1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Epigraph2), new AOEShapeRect(45, 4));

public class FlatlandFury(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FlatlandFury), new AOEShapeCircle(10))
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // if all 9 adds are alive, instead of drawing forbidden zones (which would fill the whole arena), force AI to target nearest one to kill it
        if (ActiveCasters.Count() == 9)
            hints.ForcedTarget = ActiveCasters.MinBy(actor.DistanceToHitbox);
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

public class FlatlandFuryEnrage(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FlatlandFuryEnrage), new AOEShapeCircle(10))
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (ActiveCasters.Count() < 9)
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

public class ViolentEarth(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.ViolentEarth), 6);
public class WindChisel(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WindChisel), new AOEShapeCone(34, 10.Degrees()));

public class Scales(BossModule module) : Components.Adds(module, (uint)OID.TheScaleOfTheFather);

class AutoYshtola(WorldState ws) : UnmanagedRotation(ws, 25)
{
    private Actor Magnai => World.Actors.First(x => (OID)x.OID == OID.Magnai);
    private Actor Hien => World.Actors.First(x => (OID)x.OID == OID.Hien);
    private Actor Daidukul => World.Actors.First(x => (OID)x.OID == OID.Daidukul);

    protected override void Exec(Actor? primaryTarget)
    {
        var hienMinHP = Daidukul.CastInfo?.Action.ID == (uint)AID.TranquilAnnihilation
            ? 28000
            : 10000;

        if (Hien.PredictedHPRaw < hienMinHP)
        {
            if (Player.DistanceToHitbox(Hien) > 25)
                Hints.ForcedMovement = Player.DirectionTo(Hien).ToVec3();

            UseAction(RPID.CureIISeventhDawn, Hien);
        }

        if (Hien.CastInfo?.Action.ID == 13234)
            Hints.GoalZones.Add(Hints.GoalSingleTarget(Hien.Position, 2, 5));

        var aero = StatusDetails(Magnai, WHM.SID.AeroII, Player.InstanceID);
        if (aero.Left < 4.6f)
            UseAction(RPID.AeroIISeventhDawn, Magnai);

        UseAction(RPID.StoneIVSeventhDawn, primaryTarget);

        if (Player.HPMP.CurMP < 5000)
            UseAction(RPID.Aetherwell, Player);
    }
}

class YshtolaAI(BossModule module) : RotationModule<AutoYshtola>(module);

class P1Hints(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            if (e.Actor.FindStatus(SID.Invincibility) != null)
                e.Priority = AIHints.Enemy.PriorityInvincible;

            // they do very little damage and sadu will raise them after a short delay, no point in attacking
            if ((OID)e.Actor.OID == OID.KhunShavar)
                e.Priority = AIHints.Enemy.PriorityPointless;
        }
    }
}

class P2Hints(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = e.Actor.OID == (uint)OID.Magnai ? 1 : 0;
        }
    }
}

class SaduHeavensflameStates : StateMachineBuilder
{
    public SaduHeavensflameStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<P1Hints>()
            .ActivateOnEnter<DispellingWind>()
            .ActivateOnEnter<Epigraph>()
            .ActivateOnEnter<Whisper>()
            .ActivateOnEnter<Blizzard>()
            .ActivateOnEnter<Tornado>()
            .ActivateOnEnter<Epigraph1>()
            .Raw.Update = () => Module.Enemies(OID.Magnai).Any();
        TrivialPhase(1)
            .ActivateOnEnter<P2Hints>()
            .ActivateOnEnter<YshtolaAI>()
            .ActivateOnEnter<Scales>()
            .ActivateOnEnter<FlatlandFury>()
            .ActivateOnEnter<FlatlandFuryEnrage>()
            .ActivateOnEnter<ViolentEarth>()
            .ActivateOnEnter<WindChisel>()
            .OnEnter(() =>
            {
                Module.Arena.Center = new(-186.5f, 550.5f);
            })
            .Raw.Update = () => Module.Raid.Player()?.IsDeadOrDestroyed ?? true;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68683, NameID = 6152)]
public class SaduHeavensflame(WorldState ws, Actor primary) : BossModule(ws, primary, new(-223, 519), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
    }
}
