namespace BossMod.Dawntrail.Foray.CriticalEngagement.NymianPetalodus;

public enum OID : uint
{
    Boss = 0x4704, // R2.470, x1
    Helper = 0x233C, // R0.500, x40 (spawn during fight), Helper type
    PetalodusProgeny1 = 0x47BC, // R1.950, x9 (spawn during fight)
    PetalodusProgeny2 = 0x4705, // R1.950, x3
    Marker1 = 0x4706, // R1.000, x0 (spawn during fight)
    Marker2 = 0x4707, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 872, // Boss/PetalodusProgeny2->player, no cast, single-target
    MarineMayhemCast = 41689, // Boss->self, 5.0s cast, single-target
    MarineMayhem = 41691, // Helper->self, no cast, ???
    PelagicCleaverJump = 41685, // PetalodusProgeny1->location, 8.3s cast, single-target
    PelagicCleaver = 41970, // Helper->self, 9.0s cast, range 50 60-degree cone
    Unk1 = 41693, // PetalodusProgeny1->location, no cast, single-target
    Dive = 41676, // Boss->location, 3.0s cast, single-target
    Unk2 = 41683, // PetalodusProgeny2->location, no cast, single-target
    MarkerAppear = 41682, // Marker1->location, no cast, single-target
    TidalGuillotineJumpSlow = 41677, // Boss->location, 7.0s cast, single-target
    TidalGuillotineSlow = 41723, // Helper->location, 7.9s cast, range 20 circle
    TidalGuillotineJumpFast = 43148, // Boss/PetalodusProgeny1->location, 0.5s cast, single-target
    TidalGuillotineFast = 41722, // Helper->location, 1.2s cast, range 20 circle
    TidalGuillotineJumpFast2 = 41679, // Boss->location, 0.5s cast, single-target
    OpenWaterCast = 41686, // Boss->self, 5.0s cast, single-target
    OpenWaterFirst = 41687, // Helper->self, 5.0s cast, range 4 circle
    OpenWaterRepeat1 = 43151, // Helper->self, no cast, range 4 circle
    OpenWaterRepeat2 = 41688, // Helper->self, no cast, range 4 circle
    Hydrocleave = 43149, // Boss->self, 5.0s cast, range 50 60-degree cone
    TidalGuillotineJumpAddsSlow = 41678, // PetalodusProgeny1->location, 7.0s cast, single-target
    TidalGuillotineJumpAddsFast = 41680, // PetalodusProgeny1->location, 0.5s cast, single-target
}

class MarineMayhem(BossModule module) : Components.RaidwideCast(module, AID.MarineMayhemCast);
class PelagicCleaver(BossModule module) : Components.StandardAOEs(module, AID.PelagicCleaver, new AOEShapeCone(50, 30.Degrees()));
class PetalodusProgeny(BossModule module) : Components.Adds(module, (uint)OID.PetalodusProgeny2, 1);

class OpenWater(BossModule module) : Components.GenericAOEs(module)
{
    class Line
    {
        public float Distance;
        public Angle Next;
        public DateTime NextExplosion;
        public Angle Increment;
        public float Speed;
        public int NumLeft;
        public bool Outside;

        public void Advance(DateTime timestamp)
        {
            NumLeft--;
            Next += Increment;
            NextExplosion = timestamp.AddSeconds(Speed);
        }
    }

    private Line? _lineInner;
    private Line? _lineOuter;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => ShowLine(_lineInner).Concat(ShowLine(_lineOuter));

    private IEnumerable<AOEInstance> ShowLine(Line? l)
    {
        if (l == null)
            yield break;

        var showNum = l.Outside ? 7 : 4;

        for (var i = 0; i < Math.Min(showNum, l.NumLeft); i++)
        {
            var next = (l.Next + l.Increment * i).ToDirection() * l.Distance;
            yield return new AOEInstance(new AOEShapeCircle(4), Arena.Center + next, Activation: l.NextExplosion.AddSeconds(l.Speed * i), Color: i == 0 ? ArenaColor.Danger : ArenaColor.AOE);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.OpenWaterFirst)
        {
            var dir = caster.Position - Arena.Center;
            var dist = dir.Length();
            var outside = dist > 15;
            float cw = dir.OrthoR().Dot(caster.Rotation.ToDirection()) > 0 ? -1 : 1;
            var line = new Line()
            {
                Distance = dist,
                Next = Angle.FromDirection(dir),
                Increment = (cw * 360f / (outside ? 28 : 16)).Degrees(),
                Speed = outside ? 0.7f : 1.1f,
                NumLeft = outside ? 59 : 35,
                NextExplosion = Module.CastFinishAt(spell),
                Outside = outside
            };
            if (outside)
                _lineOuter = line;
            else
                _lineInner = line;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.OpenWaterRepeat1)
        {
            var dist = (caster.Position - Arena.Center).Length();
            if (dist > 15)
            {
                if (_lineOuter == null)
                    ReportError($"OpenWater cast @ {caster.Position} but none were predicted");
                _lineOuter?.Advance(WorldState.CurrentTime);
                if (_lineOuter?.NumLeft == 0)
                    _lineOuter = null;
            }
            else
            {
                if (_lineInner == null)
                    ReportError($"OpenWater cast @ {caster.Position} but none were predicted");
                _lineInner?.Advance(WorldState.CurrentTime);
                if (_lineInner?.NumLeft == 0)
                    _lineInner = null;
            }
        }
    }
}

class Hydrocleave(BossModule module) : Components.StandardAOEs(module, AID.Hydrocleave, new AOEShapeCone(50, 30.Degrees()));

class TidalGuillotine(BossModule module) : Components.StandardAOEs(module, AID.TidalGuillotineSlow, new AOEShapeCircle(20));

class TidalGuillotineMarker(BossModule module) : Components.GenericAOEs(module, AID.TidalGuillotineFast)
{
    private readonly List<(WPos, DateTime)> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted.Select(p => new AOEInstance(new AOEShapeCircle(20), p.Item1, Activation: p.Item2));

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.MarkerAppear)
            _predicted.Add((spell.TargetXZ, WorldState.FutureTime(_predicted.Count == 0 ? 8.7f : 9.9f)));

        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            if (_predicted.Count > 0)
                _predicted.RemoveAt(0);
        }
    }
}

class NymianPetalodusStates : StateMachineBuilder
{
    public NymianPetalodusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MarineMayhem>()
            .ActivateOnEnter<PelagicCleaver>()
            .ActivateOnEnter<TidalGuillotine>()
            .ActivateOnEnter<TidalGuillotineMarker>()
            .ActivateOnEnter<PetalodusProgeny>()
            .ActivateOnEnter<OpenWater>()
            .ActivateOnEnter<Hydrocleave>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13717)]
public class NymianPetalodus(WorldState ws, Actor primary) : BossModule(ws, primary, new(-117, -850), new ArenaBoundsCircle(20))
{
    public override bool DrawAllPlayers => true;
}
