namespace BossMod.Stormblood.Dungeon.D05CastrumAbania.D052SubjectNumberXXIV;

public enum OID : uint
{
    Boss = 0x3F3B, // R3.600, x?
    Helper = 0x233C, // R0.500, x?, Helper type
}

public enum AID : uint
{
    Attack = 871, // 3F3B/1BC3->player, no cast, single-target

    ElementalOverload1 = 33448, // 3F3B->self, 5.0s cast, range 60 circle -- Ice Loaded
    ElementalOverload2 = 33449, // 3F3B->self, 5.0s cast, range 60 circle -- Fire Loaded
    ElementalOverload3 = 33450, // 3F3B->self, 5.0s cast, range 60 circle
    ElementalOverload4 = 33451, // 3F3B->self, 5.0s cast, range 60 circle -- Ice Loaded
    ElementalOverload5 = 33452, // 3F3B->self, 5.0s cast, range 60 circle -- Fire Loaded
    ElementalOverload6 = 33453, // 3F3B->self, 5.0s cast, range 60 circle -- Thunder Loaded
    SystemError = 33459, // 3F3B->self, no cast, single-target

    DiscreteMagick1 = 33454, // 3F3B->self, 3.5+0.5s cast, single-target -- IceGrid
    DiscreteMagick2 = 33455, // 3F3B->self, 4.5+0.5s cast, single-target == Blizzard II
    DiscreteMagick3 = 33457, // 3F3B->self, 3.5+0.5s cast, single-target
    DiscreteMagick4 = 33458, // 3F3B->self, 4.5+0.5s cast, single-target -- Stack
    DiscreteMagick5 = 33748, // 3F3B->self, 5.0+0.5s cast, single-target
    DiscreteMagick6 = 33749, // 3F3B->self, 4.5+0.5s cast, single-target -- SparkingCurrent

    SerialMagicks1 = 33747, // 3F3B->self, 3.5+0.5s cast, single-target -- Fire Raidwide
    SerialMagicks2 = 33456, // 3F3B->self, 3.5+0.5s cast, single-target -- Blizzard II + Ice Grid (Checkerboard)
    SerialMagicks3 = 33750, // 3F3B->self, 5.0+0.5s cast, single-target -- ThunderII Towers

    FireII = 33462, // 233C->players, 5.0s cast, range 5 circle -- Stack
    ThunderII = 33464, // 233C->self, 5.5s cast, range 5 circle
    BlizzardII = 33461, // 233C->player, 5.0s cast, range 5 circle -- Spread?

    Triflame = 33463, // 233C->self, 4.0s cast, range 60 60-degree cone
    SparkingCurrent = 33466, // 233C->self, no cast, range 20 width 6 rect
    IceGrid = 33460, // 233C->self, 4.0s cast, range 40 width 4 rect

    A = 33467, // 233C->player, no cast, single-target
}
public enum SID : uint
{
    FireLoaded = 3626, // Boss->Boss, extra=0x0
    IceLoaded = 3627, // Boss->Boss, extra=0x0
    LightningLoaded = 3628, // Boss->Boss, extra=0x0
    SystemError = 3562, // Boss->Boss, extra=0x255
}

public enum IconID : uint
{
    Stack = 161,
    Spread = 376,
    Huh = 326,
}
class ElementalOverload1(BossModule module) : Components.RaidwideCast(module, AID.ElementalOverload1);
class ElementalOverload2(BossModule module) : Components.RaidwideCast(module, AID.ElementalOverload2);
class ElementalOverload3(BossModule module) : Components.RaidwideCast(module, AID.ElementalOverload3);
class ElementalOverload4(BossModule module) : Components.RaidwideCast(module, AID.ElementalOverload4);
class ElementalOverload5(BossModule module) : Components.RaidwideCast(module, AID.ElementalOverload5);
class ElementalOverload6(BossModule module) : Components.RaidwideCast(module, AID.ElementalOverload6);

class DiscreteMagick6(BossModule module) : Components.GenericBaitAway(module, AID.DiscreteMagick6, true)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            if (caster != null)
                foreach (var p in WorldState.Party.WithoutSlot())
                    CurrentBaits.Add(new Bait(caster, p, new AOEShapeRect(20, 3)));
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SparkingCurrent)
        {
            CurrentBaits.Clear();
        }
    }
}
class FireII(BossModule module) : Components.IconStackSpread(module, (uint)IconID.Stack, 0, AID.FireII, default, 6, 0, 5.1f);
class ThunderII(BossModule module) : Components.CastTowers(module, AID.ThunderII, 5, 1, 1)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((OID)caster.OID is OID.Helper && (AID)spell.Action.ID is AID.ThunderII)
        {
            Towers.Add(new(caster.Position, Radius));
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((OID)caster.OID is OID.Helper && (AID)spell.Action.ID is AID.ThunderII)
        {
            Towers.RemoveAll(t => t.Position.AlmostEqual(caster.Position, 1));
        }
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var t in Towers.Where(t => !t.ForbiddenSoakers[slot] && (!t.CorrectAmountInside(Module) || t.IsInside(actor))))
        {
            hints.AddForbiddenZone(ShapeContains.InvertedCircle(t.Position, Radius - 0.5f));
        }
    }
};
class BlizzardII(BossModule module) : Components.CastStackSpread(module, default, AID.BlizzardII, 0, 5);
class Triflame(BossModule module) : Components.StandardAOEs(module, AID.Triflame, new AOEShapeCone(60, 30.Degrees()));
class IceGrid(BossModule module) : Components.StandardAOEs(module, AID.IceGrid, new AOEShapeRect(40, 2f));
class D052SubjectNumberXXIVStates : StateMachineBuilder
{
    public D052SubjectNumberXXIVStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ElementalOverload1>()
            .ActivateOnEnter<ElementalOverload2>()
            .ActivateOnEnter<ElementalOverload3>()
            .ActivateOnEnter<ElementalOverload4>()
            .ActivateOnEnter<ElementalOverload5>()
            .ActivateOnEnter<ElementalOverload6>()
            .ActivateOnEnter<FireII>()
            .ActivateOnEnter<ThunderII>()
            .ActivateOnEnter<BlizzardII>()
            .ActivateOnEnter<Triflame>()
            .ActivateOnEnter<IceGrid>()
            .ActivateOnEnter<DiscreteMagick6>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 242, NameID = 12392)]
public class D052SubjectNumberXXIV(WorldState ws, Actor primary) : BossModule(ws, primary, new(10.4f, 186.5f), new ArenaBoundsCircle(19));
