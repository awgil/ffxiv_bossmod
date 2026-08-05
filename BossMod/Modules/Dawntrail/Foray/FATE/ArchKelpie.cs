namespace BossMod.Dawntrail.Foray.FATE.ArchKelpie;

public enum OID : uint {
    Boss = 0x4B1F,
    Helper = 0x233C,
    ArchKelpie = 0x4B5B, // R0.500, x0 (spawn during fight)
}

public enum AID : uint {
    AutoAttack = 47381, // Boss->player, no cast, single-target
    Teleport = 47382, // Boss->player, no cast, single-target
    WaveWhistle = 47383, // Boss->self, 5.0s cast, range 25 width 50 rect
    WaterIV = 47386, // Boss->self, 5.5s cast, range 60 circle

    BloodyPuddleCast = 47384, // Boss->self, 3.0+1.0s cast, single-target
    BloodyPuddle = 47385, // 4B5B->location, 3.0s cast, range 8 circle

    StormWaveStart = 47387, // 4B5B->location, 5.0s cast, range 50 width 10 rect
    StormWaveNext = 47388, // 4B5B->location, no cast, range 50 width 5 rect
}

class WaveWhistle(BossModule module) : Components.StandardAOEs(module, AID.WaveWhistle, new AOEShapeRect(25.0f, 25.0f));
class WaterIV(BossModule module) : Components.RaidwideCast(module, AID.WaterIV);

class BloodyPuddle : Components.StandardAOEs {
    public BloodyPuddle(BossModule module) : base(module, AID.BloodyPuddle, 8.0f) {
        Color = ArenaColor.Danger;
    }
}

class StormWaveStart : Components.StandardAOEs {
    public StormWaveStart(BossModule module) : base(module, AID.StormWaveStart, new AOEShapeRect(50.0f, 5.0f)) {
        Color = ArenaColor.Danger;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        base.AddAIHints(slot, actor, assignment, hints);
        var shape = Shape;

        foreach (var caster in ActiveCasters) {
            var spellInstance = caster.CastInfo;
            if (spellInstance == null) {
                continue;
            }

            var rotation = spellInstance.Rotation;
            var right = spellInstance.LocXZ + rotation.ToDirection().OrthoR() * 1.5f;
            var left = spellInstance.LocXZ + rotation.ToDirection().OrthoL() * 1.5f;
            hints.GoalZones.Add(p => shape.Check(p, right, rotation) || shape.Check(p, left, rotation) ? 100.0f : 0.0f);
        }
    }
}

class StormWave(BossModule module) : Components.Exaflare(module, new AOEShapeRect(25.0f, 2.5f, 25.0f)) {
    private readonly Dictionary<ulong, (WaveLine right, WaveLine left)> pendingWaves = [];

    private class WaveLine : Line {
        public bool started;
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        foreach (var (c, t, r) in FutureAOEs()) {
            yield return new(Shape, c, r, t, FutureColor, Risky: false);
        }

        foreach (var l in Lines.Where(l => l.ExplosionsLeft > 0)) {
            var started = l is WaveLine { started: true };
            yield return new(Shape, l.Next, l.Rotation, l.NextExplosion, started ? ImminentColor : FutureColor, started);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if ((AID)spell.Action.ID == AID.StormWaveStart) {
            var directionRight = caster.Rotation.ToDirection().OrthoR() * 5.0f;
            var directionLeft = caster.Rotation.ToDirection().OrthoL() * 5.0f;

            var rightLine = new WaveLine {
                Next = caster.Position + directionRight + directionRight / 2,
                Advance = directionRight,
                Rotation = caster.Rotation,
                NextExplosion = Module.CastFinishAt(spell, 2.0f),
                TimeToMove = 2.0f,
                ExplosionsLeft = 4,
                MaxShownExplosions = 2
            };

            var leftLine = new WaveLine {
                Next = caster.Position + directionLeft + directionLeft / 2,
                Advance = directionLeft,
                Rotation = caster.Rotation,
                NextExplosion = Module.CastFinishAt(spell, 2.0f),
                TimeToMove = 2.0f,
                ExplosionsLeft = 4,
                MaxShownExplosions = 2
            };

            Lines.Add(rightLine);
            Lines.Add(leftLine);
            pendingWaves[caster.InstanceID] = (rightLine, leftLine);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.StormWaveStart && pendingWaves.Remove(caster.InstanceID, out var wave)) {
            wave.right.started = true;
            wave.left.started = true;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if ((AID)spell.Action.ID is AID.StormWaveStart or AID.StormWaveNext) {
            var ix = Lines.FindIndex(l => l.Next.AlmostEqual(caster.Position + l.Advance / 2, 1.0f));
            if (ix >= 0) {
                AdvanceLine(Lines[ix], caster.Position + Lines[ix].Advance / 2);
                if (Lines[ix].ExplosionsLeft <= 0) {
                    Lines.RemoveAt(ix);
                }
            }
        }
    }
}

class ArchKelpieStates : StateMachineBuilder {
    public ArchKelpieStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<WaveWhistle>()
            .ActivateOnEnter<WaterIV>()
            .ActivateOnEnter<BloodyPuddle>()
            .ActivateOnEnter<StormWaveStart>()
            .ActivateOnEnter<StormWave>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14728)]
public class ArchKelpie(WorldState ws, Actor primary) : BossModule(ws, primary, new(330.000f, -250.000f), new ArenaBoundsCircle(30));
