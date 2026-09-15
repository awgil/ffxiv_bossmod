namespace BossMod.Dawntrail.Foray.FATE.ArchKelpie;

public enum OID : uint
{
    Boss = 0x4B1F,
    Helper = 0x233C,
    ArchKelpie = 0x4B5B, // R0.500, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 47381, // Boss->player, no cast, single-target
    Teleport = 47382, // Boss->player, no cast, single-target
    WaveWhistle = 47383, // Boss->self, 5.0s cast, range 25 width 50 rect
    WaterIV = 47386, // Boss->self, 5.5s cast, range 60 circle

    BloodyPuddleCast = 47384, // Boss->self, 3.0+1.0s cast, single-target
    BloodyPuddle = 47385, // 4B5B->location, 3.0s cast, range 8 circle

    StormWaveStart = 47387, // 4B5B->location, 5.0s cast, range 50 width 10 rect
    StormWaveNext = 47388, // 4B5B->location, no cast, range 50 width 5 rect
}

class WaveWhistle(BossModule module) : Components.StandardAOEs(module, AID.WaveWhistle, new AOEShapeRect(25, 25));
class WaterIV(BossModule module) : Components.RaidwideCast(module, AID.WaterIV);

class BloodyPuddle(BossModule module) : Components.StandardAOEs(module, AID.BloodyPuddle, 8);

class StormWave : Components.Exaflare
{
    public StormWave(BossModule module) : base(module, new AOEShapeRect(50, 2.5f))
    {
        FutureRisky = false;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.StormWaveStart)
        {
            var origin = spell.LocXZ;
            var rotation = spell.Rotation;

            // both helpers cast from the same position but face opposite directions...
            Lines.Add(new()
            {
                Next = origin + rotation.ToDirection().OrthoR() * 2.5f,
                Advance = rotation.ToDirection().OrthoR() * 5,
                Rotation = rotation,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 2.1f,
                ExplosionsLeft = 5,
                MaxShownExplosions = 2
            });
            Lines.Add(new()
            {
                Next = origin + rotation.ToDirection() * 50 + rotation.ToDirection().OrthoL() * 2.5f,
                Advance = rotation.ToDirection().OrthoL() * 5,
                Rotation = rotation + 180.Degrees(),
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 2.1f,
                ExplosionsLeft = 5,
                MaxShownExplosions = 2
            });
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        foreach (var l in Lines.Where(l => l.ExplosionsLeft == 5))
            hints.GoalZones.Add(p => p.InRect(l.Next, l.Rotation, 50, 0, 4) ? 0.5f : 0);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.StormWaveStart)
        {
            foreach (var l in Lines)
                if (l.Rotation.AlmostEqual(spell.Rotation, 0.1f) || l.Rotation.AlmostEqual(spell.Rotation + 180.Degrees(), 0.1f))
                    AdvanceLine(l, l.Next);
        }

        if ((AID)spell.Action.ID == AID.StormWaveNext)
            foreach (var l in Lines)
                if (l.Rotation.AlmostEqual(spell.Rotation, 0.1f))
                    AdvanceLine(l, l.Next);

        Lines.RemoveAll(l => l.ExplosionsLeft <= 0);
    }
}

class ArchKelpieStates : StateMachineBuilder
{
    public ArchKelpieStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WaveWhistle>()
            .ActivateOnEnter<WaterIV>()
            .ActivateOnEnter<BloodyPuddle>()
            .ActivateOnEnter<StormWave>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14728)]
public class ArchKelpie(WorldState ws, Actor primary) : BossModule(ws, primary, new(330, -250), new ArenaBoundsCircle(30));
