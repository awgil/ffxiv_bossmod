namespace BossMod.Heavensward.Trial.T04Nidhogg;

public enum OID : uint
{
    Boss = 0x16DF, // R12.000, x1
    NidhoggClone = 0x16E0, // R2.700, x11, spawns in groups of 3 or 5 for high jump
    Helper = 0x16E7, // R0.500, x18, Helper type
    TheScarletPrice = 0x16E3, // R1.500, x3, untargetable, spawns before flame blast
    ShadowBrobinyak = 0x16E4, // R3.960, x1
    ShadowFalak = 0x16E5, // R4.900, x1
    ShadowDragon = 0x16E6, // R3.400, x1
    Estinien = 0xF8062, // R0.500, x1, EventNpc type
    Exit = 0x1E850B, // R0.500, x1, EventObj type
    Actor1E8536 = 0x1E8536, // R2.000, x1, EventObj type
    HighJumpMarker = 0x1EA05C, // R0.500, x11, EventObj type, left where each clone landed, 8y bleed pool for ~16s
    Actor1EA161 = 0x1EA161, // R2.000, x1, EventObj type
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    AutoAttackBoss = 871, // Boss->player, no cast, single-target
    AutoAttackAdds = 872, // ShadowBrobinyak/ShadowFalak->player, no cast, single-target
    Ability6041 = 6041, // Boss->self, no cast, single-target
    HotWingVisual = 6044, // Boss->self, 4.0s cast, single-target
    HotTailVisual = 6046, // Boss->self, 4.0s cast, single-target
    FinalChorusVisual = 6057, // Boss->self, no cast, single-target
    Ability6074 = 6074, // TheScarletPrice->self, 5.5s cast, single-target, they despawn when it ends
    SuperJumpVisual = 6105, // Boss->self, no cast, single-target
    Ability6283 = 6283, // Helper->self, no cast, single-target
    TheScarletWhisper = 6298, // Boss->self, no cast, range 10+R 120-degree cone, aimed at the boss's target
    DeafeningBellow = 6299, // Boss->self, no cast, range 80 circle
    Cauterize = 6300, // Boss->self, 3.5s cast, range 80+R width 22 rect
    Touchdown = 6301, // Boss->self, no cast, range 80 circle
    HorridRoar = 6302, // Helper->location, 3.5s cast, range 6 circle
    HotWing = 6303, // Helper->self, 4.0s cast, range 28+R width 62 rect
    HotTail = 6304, // Helper->self, 4.0s cast, range 62+R width 16 rect
    ElectricPredation = 6305, // ShadowFalak->self, no cast, range 8+R ?-degree cone
    TheSerpentsApple = 6306, // ShadowBrobinyak->self, no cast, range 8+R ?-degree cone
    RipperClaw = 6307, // ShadowDragon->self, 3.5s cast, range 5+R 90-degree cone
    FinalChorus = 6308, // Helper->self, no cast, range 80+R circle
    HighJump = 6311, // NidhoggClone->self, no cast, range 8 circle
    Geirskogul = 6312, // Boss/NidhoggClone->self, 3.0s cast, range 62+R width 8 rect
    AlaMorn = 6313, // Boss->player, 4.0s cast, single-target
    Drachenlance = 6314, // Boss->self, no cast, range 10 90-degree cone, aimed at the boss's target
    AkhMorn = 6315, // Boss->players, 5.0s cast, range 6 circle
    AkhMornRepeat = 6316, // Boss->players, no cast, range 6 circle
    FlameBlast = 6317, // Helper->self, 10.5s cast, range 50+R width 17 rect
    SuperJump = 6318, // Helper->self, 9.0s cast, range 64 circle
    MortalChorus = 6320, // Boss->self, no cast, range 60+R circle
    Bloodrage = 6507, // Helper->self, no cast, range 100 circle
}

public enum IconID : uint
{
    HighJump = 76, // player
    AlaMorn = 198, // player
    AkhMorn = 305, // player
}

class Cauterize(BossModule module) : Components.StandardAOEs(module, AID.Cauterize, new AOEShapeRect(92, 11));
class HorridRoar(BossModule module) : Components.StandardAOEs(module, AID.HorridRoar, 6);
class HotWing(BossModule module) : Components.StandardAOEs(module, AID.HotWing, new AOEShapeRect(28.5f, 31));
class HotTail(BossModule module) : Components.StandardAOEs(module, AID.HotTail, new AOEShapeRect(62.5f, 8));
class Geirskogul(BossModule module) : Components.StandardAOEs(module, AID.Geirskogul, new AOEShapeRect(74, 4));
class AlaMorn(BossModule module) : Components.SingleTargetCast(module, AID.AlaMorn);
class FlameBlast(BossModule module) : Components.StandardAOEs(module, AID.FlameBlast, new AOEShapeRect(50.5f, 8.5f));
class SuperJump(BossModule module) : Components.ProximityAOEs(module, AID.SuperJump, 20);
class RipperClaw(BossModule module) : Components.StandardAOEs(module, AID.RipperClaw, new AOEShapeCone(8.4f, 45.Degrees()));
class ElectricPredation(BossModule module) : Components.Cleave(module, AID.ElectricPredation, new AOEShapeCone(12.9f, 60.Degrees()), (uint)OID.ShadowFalak);
class TheSerpentsApple(BossModule module) : Components.Cleave(module, AID.TheSerpentsApple, new AOEShapeCone(11.96f, 60.Degrees()), (uint)OID.ShadowBrobinyak);

abstract class TimedFrontal(BossModule module, Enum aid, AOEShape shape) : Components.GenericAOEs(module, aid)
{
    protected DateTime Next;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Next != default && Next <= WorldState.FutureTime(5) && Next > WorldState.FutureTime(-1) && Module.PrimaryActor.IsTargetable && actor.InstanceID != Module.PrimaryActor.TargetID)
            yield return new(shape, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation, Next);
    }
}

class Drachenlance(BossModule module) : TimedFrontal(module, AID.Drachenlance, new AOEShapeCone(10, 45.Degrees()))
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (caster != Module.PrimaryActor)
            return;
        switch ((AID)spell.Action.ID)
        {
            case AID.Geirskogul when NumCasts == 0:
                Next = WorldState.FutureTime(7.2f);
                break;
            case AID.Drachenlance:
                ++NumCasts;
                Next = NumCasts == 1 ? WorldState.FutureTime(4.2f) : default;
                break;
            case AID.SuperJumpVisual when NumCasts == 2:
                Next = WorldState.FutureTime(9.2f);
                break;
            case AID.AlaMorn when NumCasts == 3:
                Next = WorldState.FutureTime(5.2f);
                break;
        }
    }
}

class TheScarletWhisper : TimedFrontal
{
    private AID _prev;

    public TheScarletWhisper(BossModule module) : base(module, AID.TheScarletWhisper, new AOEShapeCone(22, 60.Degrees()))
    {
        Next = WorldState.FutureTime(6.1f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (caster != Module.PrimaryActor)
            return;
        var aid = (AID)spell.Action.ID;
        switch (aid)
        {
            case AID.TheScarletWhisper:
                ++NumCasts;
                Next = default;
                break;
            case AID.HotWingVisual when NumCasts == 1:
                Next = WorldState.FutureTime(8.2f);
                break;
            case AID.DeafeningBellow when _prev == AID.TheScarletWhisper && NumCasts == 2:
                Next = WorldState.FutureTime(4.1f);
                break;
            case AID.DeafeningBellow when _prev == AID.Touchdown && NumCasts == 3:
                Next = WorldState.FutureTime(8.3f);
                break;
            case AID.DeafeningBellow when _prev == AID.HotTailVisual:
                Next = WorldState.FutureTime(9.2f);
                break;
        }
        if (aid is not (AID.AutoAttack or AID.AutoAttackBoss or AID.Ability6041))
            _prev = aid;
    }
}

class HighJump(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.HighJump, AID.HighJump, 8, 5.1f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.HighJump && Spreads.Count > 0)
        {
            Spreads.Clear();
            ++NumFinishedSpreads;
        }
    }
}

class AkhMorn(BossModule module) : Components.StackWithIcon(module, (uint)IconID.AkhMorn, AID.AkhMorn, 6, 4.9f, 4, 8, 4)
{
    private int _hits;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is not (AID.AkhMorn or AID.AkhMornRepeat) || Stacks.Count == 0)
            return;
        if (++_hits >= 4)
        {
            Stacks.Clear();
            _hits = 0;
            ++NumFinishedStacks;
            return;
        }
        var stack = Stacks[0];
        stack.Activation = WorldState.FutureTime(_hits == 1 ? 2.1f : 1.1f);
        stack.Hits = 4 - _hits;
        Stacks[0] = stack;
    }
}

class HighJumpPool(BossModule module) : Components.Voidzone(module, 8, OID.HighJumpMarker);

class Adds(BossModule module) : Components.AddsMulti(module, [OID.ShadowBrobinyak, OID.ShadowFalak, OID.ShadowDragon], 1);

class T04NidhoggStates : StateMachineBuilder
{
    public T04NidhoggStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Cauterize>()
            .ActivateOnEnter<HorridRoar>()
            .ActivateOnEnter<HotWing>()
            .ActivateOnEnter<HotTail>()
            .ActivateOnEnter<Geirskogul>()
            .ActivateOnEnter<AlaMorn>()
            .ActivateOnEnter<FlameBlast>()
            .ActivateOnEnter<SuperJump>()
            .ActivateOnEnter<RipperClaw>()
            .ActivateOnEnter<ElectricPredation>()
            .ActivateOnEnter<TheSerpentsApple>()
            .ActivateOnEnter<Drachenlance>()
            .ActivateOnEnter<TheScarletWhisper>()
            .ActivateOnEnter<HighJump>()
            .ActivateOnEnter<HighJumpPool>()
            .ActivateOnEnter<AkhMorn>()
            .ActivateOnEnter<Adds>();
    }
}

[ModuleInfo(Contributors = "Nonu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 169, NameID = 3458)]
public class T04Nidhogg(ModuleInit init) : BossModule(init, new(0, 0), new ArenaBoundsSquare(22));
