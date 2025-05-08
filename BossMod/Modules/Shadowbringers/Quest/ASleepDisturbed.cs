namespace BossMod.Shadowbringers.Quest.ASleepDistubed;

public enum OID : uint
{
    Uimet = 0x2D3F, // R0.500, x?
    Cymet = 0x2D40, // R0.500, x?
    Almet = 0x2D3E, // R0.500, x?
    Yshtola = 0x2D3D, // R0.500, x?
    BossHelper = 0x233C, // R0.500, x?, 523 type
    Boss = 0x2D3C, // R7.500, x?
    ThornHuaca = 0x2D42, // R1.600, x?, Voidgate helper?
    Voidgate = 0x2D44, // R1.000, x?
    CubusHuaca = 0x2D43, // R2.250, x?, adds
}

public enum AID : uint
{
    TheTouchOfShadowCast = 19616, // Boss->self, 4.0s cast, single-target
    TheTouchOfShadow = 19617, // BossHelper->self, 4.0s cast, range 70 circle
    TheMarrowOfFlameCast = 19612, // Boss->self, 8.0s cast, ???
    TheMarrowOfFlame = 19613, // BossHelper->player/Cymet/Uimet/Almet/Yshtola, 8.0s cast, range 8 circle
    TheGraceOfCalamityCast = 19614, // Boss->self, 5.0s cast, single-target
    TheGraceOfCalamity = 19615, // BossHelper->Yshtola, 5.0s cast, range 6 circle
    Summon = 19622, // Boss->self, 3.0s cast, single-target
    TimeAfar = 19619, // Boss->self, 3.0s cast, single-target
    AetherialPull = 19620, // Voidgate->player/Cymet, no cast, single-target
    LimbsOfLead = 19621, // Voidgate->player/Cymet, no cast, single-target
    BurningBeam = 19623, // ThornHuaca->Cymet/Yshtola/Almet/Uimet, no cast, range 40 width 4 rect
    BurningBeam2 = 19624, // ThornHuaca->player/Yshtola/Almet/Uimet, no cast, range 40 width 4 rect
    TheSoundOfHeat = 19618, // Boss->self, 4.0s cast, range 60 60-degree cone
    TheDeceitOfPainCast = 19628, // Boss->self, 5.0s cast, ???
    TheDeceitOfPain = 19629, // BossHelper->location, 5.0s cast, range 14 circle
    Animate = 19630, // Boss->self, 5.0s cast, single-target
    TheBalmOfDisgraceCast = 19626, // Boss->self, 4.0s cast, ???
    TheBalmOfDisgrace = 19627, // BossHelper->self, 4.0s cast, range 12 circle
}

public enum TetherID : uint
{
    NPCBaitAway = 1, // ThornHuaca->player/Cymet/Yshtola/Almet/Uimet
    BaitAway = 84, // ThornHuaca->Cymet/player
}

class TouchOfShadow(BossModule module) : Components.RaidwideCast(module, AID.TheTouchOfShadow);
class MarrowOfFlame(BossModule module) : Components.SpreadFromCastTargets(module, AID.TheMarrowOfFlame, 8);
class GraceOfCalamity(BossModule module) : Components.StackWithCastTargets(module, AID.TheGraceOfCalamity, 6);
class BurningBeamNPC(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeRect(40, 2), (uint)TetherID.NPCBaitAway);
class BurningBeamPlayer(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeRect(40, 2), (uint)TetherID.BaitAway);
class SoundOfHeat(BossModule module) : Components.StandardAOEs(module, AID.TheSoundOfHeat, new AOEShapeCone(60, 30.Degrees()));
class DeceitOfPain(BossModule module) : Components.StandardAOEs(module, AID.TheDeceitOfPain, 14);
class BalmOfDisgrace(BossModule module) : Components.StandardAOEs(module, AID.TheBalmOfDisgrace, new AOEShapeCircle(12));

class ASleepDisturbedStates : StateMachineBuilder
{
    public ASleepDisturbedStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TouchOfShadow>()
            .ActivateOnEnter<MarrowOfFlame>()
            .ActivateOnEnter<GraceOfCalamity>()
            .ActivateOnEnter<BurningBeamNPC>()
            .ActivateOnEnter<BurningBeamPlayer>()
            .ActivateOnEnter<SoundOfHeat>()
            .ActivateOnEnter<DeceitOfPain>()
            .ActivateOnEnter<BalmOfDisgrace>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "croizat", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69301, NameID = 9296)]
public class ASleepDisturbed(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20))
{
    protected override bool CheckPull() => PrimaryActor.IsTargetable;

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.PrioritizeTargetsByOID(OID.Boss, 0);
    }
}
