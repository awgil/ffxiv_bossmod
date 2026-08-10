namespace BossMod.Dawntrail.Foray.FATE.WavedAway;

public enum OID : uint {
    ArchKelpie = 0x4B1F,
    Helper = 0x233C,
    ArchKelpieHelper = 0x4B5B, // R0.500, x0 (spawn during fight)
}

public enum AID : uint {
    AutoAttack = 47381, // ArchKelpie->player, no cast, single-target
    Teleport = 47382, // ArchKelpie->player, no cast, single-target
    WaveWhistle = 47383, // ArchKelpie->self, 5.0s cast, range 25 width 50 rect
    WaterIV = 47386, // ArchKelpie->self, 5.5s cast, range 60 circle

    BloodyPuddleCast = 47384, // ArchKelpie->self, 3.0+1.0s cast, single-target
    BloodyPuddle = 47385, // 4B5B->location, 3.0s cast, range 8 circle

    StormWaveStart = 47387, // 4B5B->location, 5.0s cast, range 50 width 10 rect
    StormWaveNext = 47388, // 4B5B->location, no cast, range 50 width 5 rect
}

sealed class WaveWhistle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WaveWhistle, new AOEShapeRect(25.0f, 25.0f));
sealed class WaterIV(BossModule module) : Components.RaidwideCast(module, (uint)AID.WaterIV);

sealed class BloodyPuddle : Components.SimpleAOEs {
    public BloodyPuddle(BossModule module) : base(module, (uint)AID.BloodyPuddle, 8f) {
        Color = Colors.Danger;
    }
}

class StormWaveStart : Components.SimpleAOEs {
    public StormWaveStart(BossModule module) : base(module, (uint)AID.StormWaveStart, new AOEShapeRect(50.0f, 5.0f)) {
        Color = Colors.Danger;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        base.AddAIHints(slot, actor, assignment, hints);
        var casters = CollectionsMarshal.AsSpan(Casters);
        var count = casters.Length;
        for (int i = 0; i < count; i++) {
            var aoe = casters[i];
            var right = aoe.Origin + aoe.Rotation.ToDirection().OrthoR() * 1.0f;
            var left = aoe.Origin + aoe.Rotation.ToDirection().OrthoL() * 1.0f;
            hints.GoalZones.Add(p => aoe.Shape.Check(p, right, aoe.Rotation) || aoe.Shape.Check(p, left, aoe.Rotation) ? 100.0f : 0.0f);
        }
    }
}

class StormWave(BossModule module) : Components.Exaflare(module, new AOEShapeRect(25.0f, 2.5f, 25.0f)) {
    private readonly List<WaveSet> waves = [];

    private class WaveSet(Line right, Line left, ulong casterID) {
        public readonly Line RightLine = right;
        public readonly Line LeftLine = left;
        public bool waveStart = false;
        public ulong casterInstanceID = casterID;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if ((AID)spell.Action.ID == AID.StormWaveStart) {
            var directionRight = caster.Rotation.ToDirection().OrthoR() * 5.0f;
            var directionLeft = caster.Rotation.ToDirection().OrthoL() * 5.0f;

            var rightLine = new Line(caster.Position + directionRight + directionRight / 2, directionRight, Module.CastFinishAt(spell), 2.0f, 4, 2,
                caster.Rotation.ToDirection().ToAngle());
            Lines.Add(rightLine);

            var leftLine = new Line(caster.Position + directionLeft + directionLeft / 2, directionLeft, Module.CastFinishAt(spell), 2.0f, 4, 2,
                caster.Rotation.ToDirection().ToAngle());
            Lines.Add(leftLine);
            waves.Add(new WaveSet(rightLine, leftLine, caster.InstanceID));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.StormWaveStart) {
            var set = waves.Find(w => w.casterInstanceID == caster.InstanceID);
            if (set != null) {
                set.waveStart = true;
                currentVersion++;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if ((AID)spell.Action.ID is AID.StormWaveStart or AID.StormWaveNext) {
            var ix = Lines.FindIndex(l => l.Next.AlmostEqual(caster.Position + l.Advance / 2, 1.0f));
            if (ix >= 0) {
                var line = Lines[ix];
                AdvanceLine(Lines[ix], caster.Position + Lines[ix].Advance / 2);
                if (Lines[ix].ExplosionsLeft <= 0) {
                    Lines.RemoveAt(ix);
                    waves.RemoveAll(w => (w.RightLine == line || w.LeftLine == line) && !Lines.Contains(w.RightLine) && !Lines.Contains(w.LeftLine));
                }
            }
        }
    }

    public override void Update() {
        var linesCount = Lines.Count;
        if (lastCount != linesCount || currentVersion != lastVersion) {
            var futureAOEs = CollectionsMarshal.AsSpan(FutureAOEs(linesCount));
            var imminentAOEs = ImminentAOEs(linesCount);
            var futureLen = futureAOEs.Length;
            var imminentLen = imminentAOEs.Length;

            _aoes = new AOEInstance[futureLen + imminentLen];
            for (var i = 0; i < futureLen; ++i) {
                ref var aoe = ref futureAOEs[i];
                var origin = aoe.Item1;
                var rotation = aoe.Item3;
                _aoes[i] = new(Shape, origin, rotation, aoe.Item2, FutureColor, false, shapeDistance: Shape.Distance(origin, rotation));
            }

            for (var i = 0; i < imminentLen; ++i) {
                ref var aoe = ref imminentAOEs[i];
                var origin = aoe.Item1;
                var rotation = aoe.Item3;
                var line = Lines[i];
                var waveStarted = waves.Find(w => w.RightLine == line || w.LeftLine == line)?.waveStart ?? true;
                var color = waveStarted ? ImminentColor : FutureColor;
                _aoes[futureLen + i] = new(Shape, origin, rotation, aoe.Item2, color, waveStarted, shapeDistance: Shape.Distance(origin, rotation));
            }
            lastCount = linesCount;
            lastVersion = currentVersion;
        }
    }
}

[SkipLocalsInit]
sealed class WavedAwayStates : StateMachineBuilder {
    public WavedAwayStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<WaveWhistle>()
            .ActivateOnEnter<WaterIV>()
            .ActivateOnEnter<BloodyPuddle>()
            .ActivateOnEnter<StormWaveStart>()
            .ActivateOnEnter<StormWave>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(WavedAwayStates),
    ConfigType = null, // replace null with typeof(ArchKelpieConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
    StatusIDType = null, // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.ArchKelpie,
    Contributors = "Equilius",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.ForayFATE,
    GroupID = 1093u,
    NameID = 2077u,
    SortOrder = 6,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class WavedAway(WorldState ws, Actor primary) : OpenWorldFate(ws, primary);
