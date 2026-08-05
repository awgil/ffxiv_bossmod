namespace BossMod.Dawntrail.Foray.CriticalEngagement.ElmGigas;

public enum OID : uint
{
    Boss = 0x4BD9, // R3.500, x1
    Helper = 0x233C, // R0.500, x26, Helper type
    UnbowedSpirit = 0x4BDA, // R4.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 50851, // Boss->player, no cast, single-target
    AncientAeroIIICast = 47544, // Boss->self, 3.5+1.5s cast, single-target
    AncientAeroIII = 48041, // Helper->self, 5.0s cast, ???
    UnbowedSpiritCast = 47530, // Boss->self, 3.0+1.0s cast, single-target
    UnbowedSpiritVoidzone = 47531, // Helper->self, no cast, range 4 circle
    SpinningSweep = 47541, // Boss->self, 6.0s cast, range 40 120-degree cone
    InspiritedCrosswindsCast = 47533, // Boss->self, 6.0+0.8s cast, single-target
    InspiritedCrosswinds = 47535, // 4BDA->self, 6.0s cast, range 60 width 8 cross
    InspiritedImpactCast = 47542, // Boss->self, 3.0s cast, single-target
    InspiritedImpact = 47543, // Helper->self, 9.6s cast, range 25 circle
    InspiritedHurricaneCast = 47536, // Boss->self, 4.3+0.7s cast, single-target
    InspiritedHurricaneCircle = 47537, // Helper->self, 5.0s cast, range 12 circle
    InspiritedHurricaneCross = 47538, // Helper->self, 5.0s cast, range 60 width 10 cross
    AncientAero = 47540, // Helper->self, 3.0s cast, range 70 width 6 rect
    InspiritedCycloneCast = 47532, // Boss->self, 5.0+1.0s cast, single-target
    InspiritedCyclone = 47534, // 4BDA->self, 6.0s cast, range 12 circle
}

public enum SID : uint
{
    Unk = 2234, // none->4BDA, extra=0xFFE4/0x1E/0xFFAB
}

class AncientAeroIII(BossModule module) : Components.RaidwideCastDelay(module, AID.AncientAeroIIICast, AID.AncientAeroIII, 1.5f);

class SpinningSweep(BossModule module) : Components.StandardAOEs(module, AID.SpinningSweep, new AOEShapeCone(40, 60.Degrees()));

class UnbowedSpirit(BossModule module) : Components.PersistentVoidzone(module, 4, m => m.Enemies(OID.UnbowedSpirit))
{
    enum Mode
    {
        Line,
        Circle,
        None
    }

    Mode _currentMode;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.InspiritedCrosswinds or AID.InspiritedCyclone)
            _currentMode = Mode.None;

        if ((AID)spell.Action.ID is AID.UnbowedSpiritCast)
            _currentMode = Mode.Line;

        if ((AID)spell.Action.ID == AID.InspiritedHurricaneCast)
            _currentMode = Mode.Circle;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var wind in Sources(Module))
        {
            hints.AddForbiddenZone(ShapeDistance.Circle(wind.Position, 4));

            switch (_currentMode)
            {
                case Mode.Line:
                    hints.AddForbiddenZone(ShapeDistance.Capsule(wind.Position, wind.Rotation, 4, 4), WorldState.FutureTime(2));
                    break;
                case Mode.Circle:
                    var off = wind.Position - Arena.Center;
                    var start = wind.Position;
                    var end = Arena.Center + off.Rotate(-60.Degrees());
                    hints.AddForbiddenZone(ShapeDistance.DonutSector(Arena.Center, off.Length() - 4, off.Length() + 4, off.ToAngle() - 30.Degrees(), 30.Degrees()), WorldState.FutureTime(4));
                    hints.AddForbiddenZone(ShapeDistance.Circle(end, 4), WorldState.FutureTime(4));
                    break;
            }
        }
    }
}

class InspiritedCrosswinds(BossModule module) : Components.StandardAOEs(module, AID.InspiritedCrosswinds, new AOEShapeCross(60, 4));
class InspiritedImpact(BossModule module) : Components.StandardAOEs(module, AID.InspiritedImpact, 25, 3);
class InspiritedHurricane1(BossModule module) : Components.StandardAOEs(module, AID.InspiritedHurricaneCircle, 12);
class InspiritedHurricane2(BossModule module) : Components.StandardAOEs(module, AID.InspiritedHurricaneCross, new AOEShapeCross(60, 5));
class InspiritedCyclone(BossModule module) : Components.StandardAOEs(module, AID.InspiritedCyclone, 12);
class AncientAero(BossModule module) : Components.StandardAOEs(module, AID.AncientAero, new AOEShapeRect(70, 3));

class ElmGigasStates : StateMachineBuilder
{
    public ElmGigasStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AncientAeroIII>()
            .ActivateOnEnter<SpinningSweep>()
            .ActivateOnEnter<UnbowedSpirit>()
            .ActivateOnEnter<InspiritedCrosswinds>()
            .ActivateOnEnter<InspiritedImpact>()
            .ActivateOnEnter<InspiritedHurricane1>()
            .ActivateOnEnter<InspiritedHurricane2>()
            .ActivateOnEnter<AncientAero>()
            .ActivateOnEnter<InspiritedCyclone>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14508)]
public class ElmGigas(WorldState ws, Actor primary) : CEModule(ws, primary, new(-390, 700), new ArenaBoundsCircle(29.5f));
