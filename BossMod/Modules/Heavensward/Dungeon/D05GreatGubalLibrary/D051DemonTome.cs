namespace BossMod.Heavensward.Dungeon.D05GreatGubalLibrary.D051DemonTome;

public enum OID : uint
{
    Boss = 0xE82, // R7.840, x?
    DemonTome = 0xED6, // R0.500, x?
    Helper = 0x233C, // x?
}
public enum AID : uint
{
    LiquefyInner = 3520, // ED6->self, 3.0s cast, range 50+R width 8 rect
    LiquefyOuter = 3521, // ED6->self, 3.0s cast, range 50+R width 7 rect
    Repel = 3519, // E82->self, 3.0s cast, range 40+R ?-degree cone
    Disclosure = 4818, // ED6->self, 8.0s cast, range 20+R width 22 rect
    DisclosureVisual = 3518, // E82->self, 8.0s cast, single-target
    A = 3989, // ED6->self, no cast, range 12+R circle - I think this is the Spinaround Knockback Mech
    WordsOfWinter = 3961, // ED6->self, 4.5s cast, range 40+R width 22 rect
    WordsOfWinterVisual = 3517, // E82->self, 4.0s cast, single-target
}

class LiquefyInner(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LiquefyInner), new AOEShapeRect(50f + 7.84f, 4));
class LiquefyOuter(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LiquefyOuter), new AOEShapeRect(50f + 7.84f, 3.5f));
class Repel(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Repel), 20, true, kind: Kind.DirForward, stopAtWall: true);
class Disclosure(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _casters = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Disclosure)
            _casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Disclosure)
            _casters.Remove(caster);
    }
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_casters.Count > 0)
            hints.Add("Get Behind Boss!");
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var c in _casters)
        {
            hints.AddForbiddenZone(new AOEShapeRect(20f + 7.84f, 11f, 7.84f), c.Position, c.Rotation);
            hints.AddForbiddenZone(new AOEShapeRect(20f + 7.84f, 5.5f, 3.42f), c.Position, c.Rotation + 180.Degrees());
        }
    }
    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var c in _casters)
        {
            Arena.ZoneRect(c.Position, c.Rotation.ToDirection().Rotate(0.Degrees()), 20f + 7.84f, 7.84f, 11f, ArenaColor.Danger);
            Arena.ZoneRect(c.Position, c.Rotation.ToDirection().Rotate(180.Degrees()), 8f + 7.84f, 3.42f, 5.5f, ArenaColor.Danger);
        }
    }
};
class D051DemonTomeStates : StateMachineBuilder
{
    public D051DemonTomeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LiquefyInner>()
            .ActivateOnEnter<LiquefyOuter>()
            .ActivateOnEnter<Repel>()
            .ActivateOnEnter<Disclosure>();
    }
}
[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 31, NameID = 3923)]
public class D051DemonTome(WorldState ws, Actor primary) : BossModule(ws, primary, new(0.5f, 0), new ArenaBoundsRect(19.5f, 10f));
