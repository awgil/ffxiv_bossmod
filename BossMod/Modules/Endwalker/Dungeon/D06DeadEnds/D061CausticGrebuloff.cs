namespace BossMod.Endwalker.Dungeon.D06DeadEnds.D061CausticGrebuloff;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x12 (spawn during fight), Helper type
    Boss = 0x34C4, // R6.650, x1
    WeepingMiasma = 0x34C5, // R1.000, x0 (spawn during fight)
    Grebuloff = 0x3679, // R0.690, x0 (spawn during fight)
}

public enum AID : uint
{
    Miasmata = 25916, // Boss->self, 5.0s cast, range 40 circle
    NecroticFluid = 25919, // 34C5->self, 6.5s cast, range 6 circle
    NecroticMist = 28348, // Helper->location, 1.3s cast, range 6 circle
    CoughUp = 25917, // Boss->self, 4.0s cast, single-target
    CoughUp1 = 25918, // Helper->location, 4.0s cast, range 6 circle
    CertainSolitude = 28349, // Boss->self, no cast, range 40 circle
    WaveOfNausea = 28347, // Boss->self, 5.5s cast, range 6-40 donut
    PoxFlail = 25920, // Boss->player, 5.0s cast, single-target
    BlightedWater = 25921, // Boss->self, 5.0s cast, single-target
    BlightedWater1 = 25922, // Helper->players, 5.2s cast, range 6 circle
    Befoulment = 25923, // Boss->self, 5.0s cast, single-target
    Befoulment1 = 25924, // Helper->players, 5.2s cast, range 6 circle
}

public enum SID : uint
{
    CravenCompanionship = 2966, // Boss->player, extra=0x0
    Necrosis = 2965, // Boss/WeepingMiasma->player, extra=0x0
}

class PoxFlail(BossModule module) : Components.SingleTargetCast(module, AID.PoxFlail);
class BlightedWater(BossModule module) : Components.StackWithCastTargets(module, AID.BlightedWater1, 6);
class Befoulment(BossModule module) : Components.SpreadFromCastTargets(module, AID.Befoulment1, 6);
class Miasmata(BossModule module) : Components.RaidwideCast(module, AID.Miasmata);

class WaveOfNausea(BossModule module) : Components.StandardAOEs(module, AID.WaveOfNausea, new AOEShapeDonut(6, 40));

class CravenCompanionship(BossModule module) : Components.StackTogether(module, 55, 4.9f);

class NecroticFluid(BossModule module) : Components.Exaflare(module, new AOEShapeCircle(6))
{
    enum WindDirection
    {
        None,
        North,
        South
    }

    private WindDirection Wind;

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x2B && state == 0x01000010)
            Wind = WindDirection.North;

        if (index == 0x2B && state == 0x02000001)
            Wind = WindDirection.South;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.NecroticFluid)
        {
            Lines.Add(new Line()
            {
                Next = caster.Position,
                Advance = Wind switch
                {
                    WindDirection.North => new(0, -6),
                    WindDirection.South => new(0, 6),
                    _ => new(0, 0)
                },
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 2.07f,
                ExplosionsLeft = 7,
                MaxShownExplosions = 2
            });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        WPos target = (AID)spell.Action.ID switch
        {
            AID.NecroticMist => spell.TargetXZ,
            AID.NecroticFluid => caster.Position,
            _ => default
        };
        if (target == default)
            return;

        int index = Lines.FindIndex(item => item.Next.AlmostEqual(target, 1));
        if (index == -1)
        {
            ReportError($"No entry for {target}");
            return;
        }

        AdvanceLine(Lines[index], target);
        if (Lines[index].ExplosionsLeft == 0 || !Module.Bounds.Contains(Lines[index].Next - Module.Arena.Center))
            Lines.RemoveAt(index);
    }
}

class CoughUp(BossModule module) : Components.StandardAOEs(module, AID.CoughUp1, 6);

class CausticGrebuloffStates : StateMachineBuilder
{
    public CausticGrebuloffStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<NecroticFluid>()
            .ActivateOnEnter<CoughUp>()
            .ActivateOnEnter<CravenCompanionship>()
            .ActivateOnEnter<WaveOfNausea>()
            .ActivateOnEnter<Miasmata>()
            .ActivateOnEnter<Befoulment>()
            .ActivateOnEnter<BlightedWater>()
            .ActivateOnEnter<PoxFlail>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 792, NameID = 10313)]
public class CausticGrebuloff(WorldState ws, Actor primary) : BossModule(ws, primary, new(266.5f, -178f), new ArenaBoundsCircle(20));
