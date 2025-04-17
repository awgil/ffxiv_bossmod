namespace BossMod.Shadowbringers.Dungeon.D01Holminser.D012TesleentheForgiven;

public enum OID : uint
{
    Boss = 0x278B, // R1.800, x1
    HolyWaterVoidzone = 0x1EABF9, // R0.500, EventObj type, spawn during fight
    Helper = 0x233C, // x3
    Helper2 = 0x2A4B,
    Helper3 = 0x2A4C, // R1.320, x5
    Helper4 = 0x2A4D, // R2.000, x2
}

public enum AID : uint
{
    AutoAttack = 870, // 278B->player, no cast, single-target
    TheTickler = 15823, // 278B->player, 4.0s cast, single-target, tankbuster
    ScoldsBridle = 15824, // 278B->self, 4.0s cast, range 40 circle, raidwide
    FeveredFlagellation1 = 15829, // 278B->self, 8.0s cast, single-target
    FeveredFlagellation2 = 15830, // 278B->players, no cast, width 4 rect charge, limit cut mechanic
    ExorciseA = 15826, // 278B->none, 5.0s cast, single-target
    ExorciseB = 15827, // 278B->location, no cast, range 6 circle
    HolyWaterVoidzones = 15825, // 278B->self, no cast, single-target
    HolyWater = 15828, // Helper->location, 7.0s cast, range 6 circle
}

public enum IconID : uint
{
    Tankbuster = 198, // player
    Icon1 = 79, // player
    Icon2 = 80, // player
    Icon3 = 81, // player
    Icon4 = 82, // player
    Stackmarker = 62, // player
}

class TheTickler(BossModule module) : Components.SingleTargetDelayableCast(module, AID.TheTickler);
class ScoldsBridle(BossModule module) : Components.RaidwideCast(module, AID.ScoldsBridle);

class FeveredFlagellation(BossModule module) : Components.GenericBaitAway(module)
{
    public override void Update()
    {
        foreach (ref var b in CurrentBaits.AsSpan())
        {
            if (b.Shape is AOEShapeRect shape)
            {
                var len = (b.Target.Position - b.Source.Position).Length();
                if (shape.LengthFront != len)
                    b.Shape = shape with { LengthFront = len };
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.FeveredFlagellation2)
            CurrentBaits.RemoveAt(0);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var icon = (IconID)iconID;
        if (icon is >= IconID.Icon1 and <= IconID.Icon4)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, new AOEShapeRect(0, 2)));
    }
}

class Exorcise(BossModule module) : Components.StackWithCastTargets(module, AID.ExorciseA, 6);
class HolyWater(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 6, AID.HolyWater, m => m.Enemies(OID.HolyWaterVoidzone).Where(z => z.EventState != 7), 0.8f);

class D012TesleentheForgivenStates : StateMachineBuilder
{
    public D012TesleentheForgivenStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TheTickler>()
            .ActivateOnEnter<ScoldsBridle>()
            .ActivateOnEnter<FeveredFlagellation>()
            .ActivateOnEnter<Exorcise>()
            .ActivateOnEnter<HolyWater>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "legendoficeman, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 676, NameID = 8300)]
public class D012TesleentheForgiven(WorldState ws, Actor primary) : BossModule(ws, primary, new(78, -82), new ArenaBoundsCircle(19.5f));
