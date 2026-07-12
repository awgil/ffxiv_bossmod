namespace BossMod.Modules.Dawntrail.Foray.FATE.Nammu;

public enum OID : uint {
    Boss = 0x4718,
    Helper = 0x233C,
    Nammu = 0x4719, // R0.500, x0 (spawn during fight)
}

public enum AID : uint {
    AutoAttack = 41768, // Boss->player, no cast, single-target
    VoidWaterIVCast = 41785, // Boss->self, 5.0s cast, single-target
    VoidWaterIV = 41786, // 4719->location, 5.0s cast, range 40 circle

    TidelineCast = 41770, // Boss->self, 3.0s cast, single-target
    TidelineStart = 41771, // 4719->location, 5.0s cast, range 50 width 10 rect
    TidelineNext = 41772, // 4719->location, 1.0s cast, range 50 width 5 rect

    RecedingTwinTides = 41773, // Boss->self, 5.0s cast, single-target
    NearTideReceding = 41774, // Nammu->location, 5.0s cast, range 10 circle
    FarTideReceding = 41778, // Nammu->location, 7.0s cast, range ?-40 donut

    EncroachingTwinTides = 41776, // Boss->self, 5.0s cast, single-target
    FarTideEncroaching = 41777, // 4719->location, 5.0s cast, range 10-40 donut
    NearTideEncroaching = 41775, // 4719->location, 7.0s cast, range 10 circle
}

class VoidWaterIV1(BossModule module) : Components.RaidwideCast(module, AID.VoidWaterIV);

// We change this to draw to the foreground as the exaflares casts overlap with the first one, and it can look confusing where is exactly safe
class TidelineStart(BossModule module) : Components.StandardAOEs(module, AID.TidelineStart, new AOEShapeRect(50f, 5f), highlightImminent: true) {
    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        foreach (var c in ActiveAOEs(pcSlot, pc)) {
            var col = c.Color;
            if (col == 0 && c.Inverted)
                col = ArenaColor.SafeFromAOE;

            c.Shape.Draw(Arena, c.Origin, c.Rotation, col);
        }
    }
}

class TidelineExaFlare(BossModule module) : Components.Exaflare(module, new AOEShapeRect(25.0f, 2.5f, 25.0f)) {
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        foreach (var (c, t, r) in FutureAOEs()) {
            yield return new(Shape, c, r, t, FutureColor, Risky: false);
        }

        foreach (var (c, t, r) in ImminentAOEs()) {
            yield return new(Shape, c, r, t, Color: NumCasts == 0 ? FutureColor : ImminentColor, Risky: NumCasts != 0);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.TidelineCast) {
            NumCasts = 0;
            return;
        }

        var directionRight = caster.Rotation.ToDirection().OrthoR() * 5.0f;
        var directionLeft = caster.Rotation.ToDirection().OrthoL() * 5.0f;

        if ((AID)spell.Action.ID == AID.TidelineStart) {
            Lines.Add(new() {
                Next = caster.Position + directionRight + directionRight / 2,
                Advance = directionRight,
                Rotation = caster.Rotation.ToDirection().ToAngle(),
                NextExplosion = Module.CastFinishAt(spell, 2.0f),
                TimeToMove = 2.0f,
                ExplosionsLeft = 4,
                MaxShownExplosions = 2
            });

            Lines.Add(new() {
                Next = caster.Position + directionLeft + directionLeft / 2,
                Advance = directionLeft,
                Rotation = caster.Rotation.ToDirection().ToAngle(),
                NextExplosion = Module.CastFinishAt(spell, 2.0f),
                TimeToMove = 2.0f,
                ExplosionsLeft = 4,
                MaxShownExplosions = 2
            });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if ((AID)spell.Action.ID is AID.TidelineStart or AID.TidelineNext) {
            NumCasts++;
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

class TwinTides(BossModule module) : Components.GenericAOEs(module) {
    private List<AOEInstance> aoes = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.RecedingTwinTides) {
            aoes.Add(new(new AOEShapeCircle(10), caster.Position, caster.Rotation, Module.CastFinishAt(spell)));
            aoes.Add(new(new AOEShapeDonut(10.0f, 40.0f), caster.Position, caster.Rotation, Module.CastFinishAt(spell, 2.0f), Risky: false));
        }

        if (spell.Action.ID == (uint)AID.EncroachingTwinTides) {
            aoes.Add(new(new AOEShapeDonut(10.0f, 40.0f), caster.Position, caster.Rotation, Module.CastFinishAt(spell)));
            aoes.Add(new(new AOEShapeCircle(10), caster.Position, caster.Rotation, Module.CastFinishAt(spell, 2.0f), Risky: false));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.NearTideReceding || spell.Action.ID == (uint)AID.FarTideReceding ||
            spell.Action.ID == (uint)AID.NearTideEncroaching || spell.Action.ID == (uint)AID.FarTideEncroaching) {
            if (aoes.Count > 0) {
                aoes.RemoveAt(0);
            }
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        int show = 0;
        foreach (var aoe in aoes) {
            yield return aoe with { Color = show == 0 ? ArenaColor.Danger : ArenaColor.AOE, Risky = show == 0 };
            show++;
        }
    }
}

class NammuStates : StateMachineBuilder {
    public NammuStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<VoidWaterIV1>()
            .ActivateOnEnter<TidelineStart>()
            .ActivateOnEnter<TidelineExaFlare>()
            .ActivateOnEnter<TwinTides>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13701)]
public class Nammu(WorldState ws, Actor primary) : BossModule(ws, primary, new(162.3f, 681.3f), new ArenaBoundsCircle(40));
