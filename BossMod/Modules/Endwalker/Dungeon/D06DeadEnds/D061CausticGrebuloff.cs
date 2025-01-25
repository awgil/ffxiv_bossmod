namespace BossMod.Endwalker.Dungeon.D06DeadEnds.D061CausticGrebuloff;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x12 (spawn during fight), Helper type
    Boss = 0x34C4, // R6.650, x1
    _Gen_WeepingMiasma = 0x34C5, // R1.000, x0 (spawn during fight)
    _Gen_Grebuloff = 0x3679, // R0.690, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_Attack = 872, // Boss->player, no cast, single-target
    _Weaponskill_Miasmata = 25916, // Boss->self, 5.0s cast, range 40 circle
    _Weaponskill_NecroticFluid = 25919, // 34C5->self, 6.5s cast, range 6 circle
    _Ability_NecroticMist = 28348, // Helper->location, 1.3s cast, range 6 circle
    _Ability_CoughUp = 25917, // Boss->self, 4.0s cast, single-target
    _Ability_CoughUp1 = 25918, // Helper->location, 4.0s cast, range 6 circle
    _Weaponskill_CertainSolitude = 28349, // Boss->self, no cast, range 40 circle
    _Weaponskill_WaveOfNausea = 28347, // Boss->self, 5.5s cast, range 6-40 donut
    _Weaponskill_PoxFlail = 25920, // Boss->player, 5.0s cast, single-target
    _Ability_BlightedWater = 25921, // Boss->self, 5.0s cast, single-target
    _Ability_BlightedWater1 = 25922, // Helper->players, 5.2s cast, range 6 circle
    _Ability_Befoulment = 25923, // Boss->self, 5.0s cast, single-target
    _Ability_Befoulment1 = 25924, // Helper->players, 5.2s cast, range 6 circle
}

public enum SID : uint
{
    _Gen_CravenCompanionship = 2966, // Boss->player, extra=0x0
    _Gen_Hysteria = 296, // Boss->player, extra=0x0
    _Gen_Transcendent = 2648, // none->player, extra=0x0
    _Gen_VulnerabilityUp = 1789, // Boss/Helper->player, extra=0x1/0x2/0x3
    _Gen_Necrosis = 2965, // Boss/_Gen_WeepingMiasma->player, extra=0x0
}

class PoxFlail(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID._Weaponskill_PoxFlail));
class BlightedWater(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID._Ability_BlightedWater1), 6);
class Befoulment(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID._Ability_Befoulment1), 6);
class Miasmata(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_Miasmata));

class WaveOfNausea(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_WaveOfNausea), new AOEShapeDonut(6, 40));

class CravenCompanionship(BossModule module) : BossComponent(module)
{
    private readonly DateTime[] Expires = Utils.MakeArray<DateTime>(4, default);

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_CravenCompanionship)
            Expires[Raid.FindSlot(actor.InstanceID)] = status.ExpireAt;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_CravenCompanionship)
            Expires[Raid.FindSlot(actor.InstanceID)] = default;
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Expires[pcSlot] != default)
            foreach (var actor in Raid.WithoutSlot(excludeNPCs: true))
                Arena.AddCircle(actor.Position, 3, ArenaColor.Safe);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Expires[slot] != default)
        {
            var nearestAlly = Raid.WithoutSlot(excludeNPCs: true).Exclude(actor).MinBy(p => p.DistanceToHitbox(actor));
            if (nearestAlly != null)
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(nearestAlly.Position, 3), Expires[slot]);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Expires[slot] != default)
            hints.Add("Stack with allies!", !Raid.WithoutSlot().Exclude(actor).Any(r => r.Position.InCircle(actor.Position, 3)));
    }
}

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
        if ((AID)spell.Action.ID == AID._Weaponskill_NecroticFluid)
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
            AID._Ability_NecroticMist => spell.TargetXZ,
            AID._Weaponskill_NecroticFluid => caster.Position,
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

class CoughUp(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_CoughUp1), 6);

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

