namespace BossMod.Shadowbringers.Dungeon.D13Paglthan.D133LunarBahamut;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x24, Helper type
    Boss = 0x316A, // R8.400, x1
    LunarNail = 0x316B, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    TwistedScream = 23367, // Boss->self, 3.0s cast, range 40 circle
    Upburst = 24667, // LunarNail->self, 3.0s cast, range 2 circle
    BigBurst = 23368, // LunarNail->self, 4.0s cast, range 9 circle
    PerigeanBreath = 23385, // Boss->self, 5.0s cast, range 30 90-degree cone
    AkhMorn = 23381, // Boss->players, 5.0s cast, range 4 circle
    AkhMornRepeat = 23382, // Boss->players, no cast, range 4 circle
    Megaflare1 = 23373, // Helper->player, 5.0s cast, range 5 circle
    Megaflare2 = 23374, // Helper->location, 3.0s cast, range 6 circle
    MegaflareDive = 23378, // Boss->self, 4.0s cast, range 41 width 12 rect
    KanRhai = 23375, // Boss->self, 4.0s cast, single-target
    KanRhai1 = 23376, // Helper->self, no cast, range 30 width 6 rect
    LunarFlare1 = 23370, // Helper->location, 10.0s cast, range 11 circle
    LunarFlare2 = 23371, // Helper->location, 10.0s cast, range 6 circle
    Gigaflare = 23383, // Boss->self, 7.0s cast, range 40 circle
    Flatten = 23384, // Boss->player, 5.0s cast, single-target
}

class Flatten(BossModule module) : Components.SingleTargetCast(module, AID.Flatten);
class KanRhaiBait(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    public static readonly AOEShape Cross = new AOEShapeCross(15, 3);
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == 260)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, Cross, IgnoreRotation: true));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.KanRhai)
            CurrentBaits.Clear();
    }
}
class KanRhaiRepeat(BossModule module) : Components.GenericAOEs(module)
{
    private WPos? Source;
    private int Counter;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(Source).Select(p => new AOEInstance(KanRhaiBait.Cross, p));

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.KanRhai1)
        {
            Counter++;
            if (Source == null)
                Source = caster.Position;
            else if (Counter >= 20)
            {
                Source = null;
                Counter = 0;
            }
        }
    }
}
class Gigaflare(BossModule module) : Components.RaidwideCast(module, AID.Gigaflare);
class LunarFlare1(BossModule module) : Components.StandardAOEs(module, AID.LunarFlare1, 11);
class LunarFlare2(BossModule module) : Components.StandardAOEs(module, AID.LunarFlare2, 6);
class MegaflareDive(BossModule module) : Components.StandardAOEs(module, AID.MegaflareDive, new AOEShapeRect(41, 6));
class Megaflare1(BossModule module) : Components.SpreadFromCastTargets(module, AID.Megaflare1, 5);
class Megaflare2(BossModule module) : Components.StandardAOEs(module, AID.Megaflare2, 6);
class PerigeanBreath(BossModule module) : Components.StandardAOEs(module, AID.PerigeanBreath, new AOEShapeCone(30, 45.Degrees()));
class TwistedScream(BossModule module) : Components.RaidwideCast(module, AID.TwistedScream);
class Upburst(BossModule module) : Components.StandardAOEs(module, AID.Upburst, new AOEShapeCircle(2));
class BigBurst(BossModule module) : Components.StandardAOEs(module, AID.BigBurst, new AOEShapeCircle(9));
class AkhMorn(BossModule module) : Components.UniformStackSpread(module, 4, 0, 4)
{
    private int Counter;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AkhMorn)
            AddStack(WorldState.Actors.Find(spell.TargetID)!, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AkhMorn or AID.AkhMornRepeat)
        {
            if (++Counter >= 4)
            {
                Stacks.Clear();
                Counter = 0;
            }
        }
    }
}

class LunarBahamutStates : StateMachineBuilder
{
    public LunarBahamutStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TwistedScream>()
            .ActivateOnEnter<Upburst>()
            .ActivateOnEnter<BigBurst>()
            .ActivateOnEnter<PerigeanBreath>()
            .ActivateOnEnter<AkhMorn>()
            .ActivateOnEnter<Megaflare1>()
            .ActivateOnEnter<Megaflare2>()
            .ActivateOnEnter<MegaflareDive>()
            .ActivateOnEnter<Gigaflare>()
            .ActivateOnEnter<LunarFlare1>()
            .ActivateOnEnter<LunarFlare2>()
            .ActivateOnEnter<KanRhaiBait>()
            .ActivateOnEnter<KanRhaiRepeat>()
            .ActivateOnEnter<Flatten>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 777, NameID = 10077)]
public class LunarBahamut(WorldState ws, Actor primary) : BossModule(ws, primary, new(796.5f, -97.5f), new ArenaBoundsCircle(20));
