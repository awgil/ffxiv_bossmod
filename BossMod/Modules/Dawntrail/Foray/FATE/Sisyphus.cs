namespace BossMod.Modules.Dawntrail.Foray.FATE.Sisyphus;

public enum OID : uint {
    Boss = 0x4735,
    Helper = 0x233C,
    Sisyphus = 0x4736, // R0.500, x0 (spawn during fight)
}

public enum AID : uint {
    AutoAttack = 41994, // Boss->player, no cast, single-target
    Teleport = 41971, // Boss->location, no cast, single-target

    ThunderousMemoryCircleCast = 41973, // Boss->self, 3.3+0.7s cast, single-target
    ThunderousMemoryCircle = 41974, // 4736->location, 4.0s cast, range 10 circle
    ResoundingMemoryCircle = 41980, // 4736->location, 5.0s cast, range 10 circle

    ThunderousMemoryConeCast = 41977, // Boss->self, 3.3+0.7s cast, single-target
    ThunderousMemoryCone = 41978, // 4736->location, 4.0s cast, range 70 45-degree cone
    ResoundingMemoryCone = 41982, // 4736->location, 5.0s cast, range 70 ?-degree cone

    ThunderousMemoryDonutCast = 41975, // Boss->self, 3.3+0.7s cast, single-target
    ThunderousMemoryDonut = 41976, // Sisyphus->location, 4.0s cast, range ?-30 donut
    ResoundingMemoryDonut = 41981, // Sisyphus->location, 5.0s cast, range ?-30 donut

    ResoundingMemoryCast = 41979, // Boss->self, 4.3+0.7s cast, single-target

    ThriceComeThunderCast = 41983, // Boss->self, 4.3+0.7s cast, single-target
    ThriceComeThunderInner = 41984, // 4736->location, 5.0s cast, range 10 circle
    ThriceComeThunderMiddle = 41985, // 4736->location, 7.0s cast, range 10-20 donut
    ThriceComeThunderOuter = 41986, // 4736->location, 9.0s cast, range 20-30 donut

    ThunderIICast = 41987, // Boss->self, 2.3+0.7s cast, single-target
    ThunderII = 41988, // 4736->location, 3.0s cast, range 6 circle
    ThunderIVCast = 41991, // Boss->self, 4.3+0.7s cast, single-target
    ThunderIV = 41992, // 4736->location, 5.0s cast, range 40 circle

    Trounce = 41972, // Boss->self, 5.0s cast, range 40 60-degree cone
}

public enum SID : uint {
    ThunderousMemory = 4382, // Boss->Boss, extra=0x1/0x2
}

class ThunderousMemoryCircle(BossModule module) : Components.GroupedAOEs(module, [AID.ThunderousMemoryCircle, AID.ResoundingMemoryCircle], new AOEShapeCircle(10.0f));
class ThunderousMemoryCone(BossModule module) : Components.GroupedAOEs(module, [AID.ThunderousMemoryCone, AID.ResoundingMemoryCone], new AOEShapeCone(70.0f, 22.5f.Degrees()));
class ThunderousMemoryDonut(BossModule module) : Components.GroupedAOEs(module, [AID.ThunderousMemoryDonut, AID.ResoundingMemoryDonut], new AOEShapeDonut(10.0f, 30.0f));
class ThunderII(BossModule module) : Components.StandardAOEs(module, AID.ThunderII, new AOEShapeCircle(6.0f));
class ThunderIV(BossModule module) : Components.RaidwideCast(module, AID.ThunderIV);
class Trounce(BossModule module) : Components.StandardAOEs(module, AID.Trounce, new AOEShapeCone(40f, 30.Degrees()));

class ThriceComeThunder(BossModule module) : Components.GenericAOEs(module) {
    private List<AOEInstance> aoes = [];
    private int waves = 0;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.ThriceComeThunderInner) {
            aoes.Add(new(new AOEShapeCircle(10), caster.Position, caster.Rotation, Module.CastFinishAt(spell)));
            waves++;
        }

        if (spell.Action.ID == (uint)AID.ThriceComeThunderMiddle) {
            aoes.Add(new(new AOEShapeDonut(10, 20), caster.Position, caster.Rotation, Module.CastFinishAt(spell)));
        }

        if (spell.Action.ID == (uint)AID.ThriceComeThunderOuter) {
            aoes.Add(new(new AOEShapeDonut(20, 30), caster.Position, caster.Rotation, Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID is (uint)AID.ThriceComeThunderInner or (uint)AID.ThriceComeThunderMiddle or (uint)AID.ThriceComeThunderOuter) {
            aoes.SortBy(a => a.Activation);
            if (aoes.Count > 0) {
                aoes.RemoveAt(0);

                if (spell.Action.ID == (uint)AID.ThriceComeThunderOuter) {
                    waves--;
                }
            }
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        int show = 0;
        foreach (var aoe in aoes.OrderBy(a => a.Activation).Take(2 * waves)) {
            yield return aoe with { Color = show < waves ? ArenaColor.Danger : ArenaColor.AOE, Risky = show < waves };
            show++;
        }
    }
}

class SisyphusStates : StateMachineBuilder {
    public SisyphusStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<ThunderousMemoryCircle>()
            .ActivateOnEnter<ThunderousMemoryCone>()
            .ActivateOnEnter<ThunderousMemoryDonut>()
            .ActivateOnEnter<ThunderII>()
            .ActivateOnEnter<ThunderIV>()
            .ActivateOnEnter<ThriceComeThunder>()
            .ActivateOnEnter<Trounce>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13703)]
public class Sisyphus(WorldState ws, Actor primary) : BossModule(ws, primary, new(-227.0f, 37.0f), new ArenaBoundsCircle(40));
