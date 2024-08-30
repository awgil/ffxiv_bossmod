namespace BossMod.RealmReborn.Dungeon.D29TheKeeperoftheLake.D293Midgardsormr;

public enum OID : uint
{
    Boss = 0x392A,
    Helper = 0x233C,
    MirageDragonA = 0x392C, // R5.000, x1
    MirageDragonB = 0x392B, // R5.000, x1
    MirageDragonC = 0x392D, // R5.000, x1
    MirageDragonD = 0x392E, // R5.000, x1
}

public enum IconID : uint
{
    AhkMornStack = 305, // player
    TankBuster = 218, // player // Tank Buster
}

public enum AID : uint
{
    Admonishment = 29889, // Boss->self, 4.0s cast, range 40 width 12 rect // done
    AkhMornCast = 29283, // Boss->players, 5.0s cast, range 6 circle 
    AkhMornFollowup = 29284, // Boss->players, no cast, range 6 circle 
    Animadversion = 29281, // Boss->self, 7.0s cast, range 50 circle // done
    AntipathyBoss = 29285, // Boss->self, 4.0s cast, single-target
    AntipathyCast1 = 29286, // Helper->self, 4.0s cast, range 6 circle // done
    AntipathyCast2 = 29287, // Helper->self, 4.0s cast, range -12 donut // done
    AntipathyCast3 = 29288, // Helper->self, 4.0s cast, range -20 donut // done
    AutoAttack = 870, // 392D/392E->player, no cast, single-target
    Condescension = 29602, // Boss->player, 5.0s cast, single-target // done
    Disgust = 29891, // Boss->self, 4.0s cast, single-target // done
    DisgustAttack = 29892, // Helper->self, no cast, range 20 circle //done
    InnerTurmoil = 29888, // Boss->self, 4.0s cast, range 22 circle // done
    MirageAdmonishment = 29290, // MirageDragonB/MirageDragonA->self, 2.0s cast, range 40 width 12 rect // done
    PhantomAdmonishment = 29280, // Boss->self, 7.0s cast, range 40 width 12 rect // done
    PhantomInnerTurmoil = 29278, // Boss->self, 7.0s cast, range 22 circle // done
    PhantomKin = 29277, // Boss->self, 4.0s cast, single-target // summons the clones on the side for phantom attacks
    PhantomOuterTurmoil = 29279, // Boss->self, 7.0s cast, range 39 ?-degree cone
    UnknownAbility = 30226, // 392D/392E->self, no cast, single-target
}

class Adonishment(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Admonishment), new AOEShapeRect(40, 6));
class MidgarAdds(BossModule module) : Components.AddsMulti(module, [(uint)OID.MirageDragonC, (uint)OID.MirageDragonD]);
class Animadversion(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Animadversion), "Raidwide");
class Condescension(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Condescension)); // tank buster
class Disgust(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Disgust), "Raidwide");
class InnerTurmoil(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.InnerTurmoil), new AOEShapeCircle(22));
class PhandomAdomishment(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PhantomAdmonishment), new AOEShapeRect(40, 6));
class PhantomInner(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PhantomInnerTurmoil), new AOEShapeCircle(22));
class PhantomOuter(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PhantomOuterTurmoil), new AOEShapeDonutSector(22, 40, 90.Degrees()));

class MirageAdmonishment(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(40, 6);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x11D3 && (OID)actor.OID is OID.MirageDragonA or OID.MirageDragonB)
            _aoes.Add(new(rect, actor.Position, actor.Rotation, WorldState.FutureTime(12)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.MirageAdmonishment)
            _aoes.Clear();
    }
}

class Anitpathy(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(6), new AOEShapeDonut(6, 12), new AOEShapeDonut(12, 20)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AntipathyCast1)
            AddSequence(Module.Center, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var order = (AID)spell.Action.ID switch
        {
            AID.AntipathyCast1 => 0,
            AID.AntipathyCast2 => 1,
            AID.AntipathyCast3 => 2,
            _ => -1
        };
        if (!AdvanceSequence(order, caster.Position, WorldState.FutureTime(2f)))
            ReportError($"unexpected order {order}");
    }
}

class AhkMornInitial(BossModule module) : Components.StackWithIcon(module, (uint)IconID.AhkMornStack, ActionID.MakeSpell(AID.AkhMornCast), 6, 0, 2, 4);
class AhkMornFollow(BossModule module) : Components.UniformStackSpread(module, 6, 0, 4)
{
    public int NumCasts { get; private set; }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AkhMornFollowup)
            ++NumCasts;

        if ((AID)spell.Action.ID is AID.AkhMornFollowup && NumCasts >= 3)
        {
            // reset for next usage of mechanic, thanks xan for the tip on how tf to fix this
            NumCasts = 0;
            Stacks.Clear();
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.AhkMornStack)
            AddStack(actor, WorldState.FutureTime(4.4f));
    }
}

class D293MidgardsormrStates : StateMachineBuilder
{
    public D293MidgardsormrStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Adonishment>()
            .ActivateOnEnter<MidgarAdds>()
            .ActivateOnEnter<Animadversion>()
            .ActivateOnEnter<Condescension>()
            .ActivateOnEnter<Disgust>()
            .ActivateOnEnter<InnerTurmoil>()
            .ActivateOnEnter<MirageAdmonishment>()
            .ActivateOnEnter<PhandomAdomishment>()
            .ActivateOnEnter<PhantomInner>()
            .ActivateOnEnter<PhantomOuter>()
            .ActivateOnEnter<Anitpathy>()
            .ActivateOnEnter<AhkMornInitial>()
            .ActivateOnEnter<AhkMornFollow>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman, Xan, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 32, NameID = 3374)]
public class D293Midgardsormr(WorldState ws, Actor primary) : BossModule(ws, primary, new(-41, -78), new ArenaBoundsCircle(19.5f));
