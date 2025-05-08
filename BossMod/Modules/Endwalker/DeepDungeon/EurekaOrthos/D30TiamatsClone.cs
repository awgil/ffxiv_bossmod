namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos.D30TiamatsClone;

public enum OID : uint
{
    Boss = 0x3D9A, // R19.000
    DarkWanderer = 0x3D9B, // R2.000
    Helper = 0x233C, // R0.500
}

public enum AID : uint
{
    HeadAttack = 31842, // 233C->player, no cast, single-target
    AutoAttack = 32702, // 3D9A->player, no cast, single-target
    CreatureOfDarkness = 31841, // 3D9A->self, 3.0s cast, single-target // Summon Heads E<->W heading S
    DarkMegaflare1 = 31849, // 3D9A->self, 3.0s cast, single-target
    DarkMegaflare2 = 31850, // 233C->location, 3.0s cast, range 6 circle
    DarkWyrmtail1 = 31843, // 3D9A->self, 5.0s cast, single-target
    DarkWyrmtail2 = 31844, // 233C->self, 6.0s cast, range 40 width 16 rect // Summon Heads Heading E/W from Middle Lane
    DarkWyrmwing1 = 31845, // 3D9A->self, 5.0s cast, single-target
    DarkWyrmwing2 = 31846, // 233C->self, 6.0s cast, range 40 width 16 rect  // Summon Heads Heading E/W from E/W Walls
    WheiMornFirst = 31847, // 3D9A->location, 5.0s cast, range 6 circle
    WheiMornRest = 31848, // 3D9A->location, no cast, range 6 circle
}

public enum IconID : uint
{
    ChasingAOE = 197, // player
}

class WheiMorn(BossModule module) : Components.StandardChasingAOEs(module, new AOEShapeCircle(6), AID.WheiMornFirst, AID.WheiMornRest, 6, 2, 5)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if (spell.Action == ActionRest && Chasers.Count == 0)
            ExcludedTargets.Reset();
    }
}
class DarkMegaflare(BossModule module) : Components.StandardAOEs(module, AID.DarkMegaflare2, 6);
class DarkWyrmwing(BossModule module) : Components.StandardAOEs(module, AID.DarkWyrmwing2, new AOEShapeRect(40, 8));
class DarkWyrmtail(BossModule module) : Components.StandardAOEs(module, AID.DarkWyrmtail2, new AOEShapeRect(40, 8));

class CreatureOfDarkness(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _heads = [];
    private static readonly AOEShapeRect rect = new(2, 2, 2);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in _heads)
        {
            yield return new(rect, c.Position, c.Rotation, Color: ArenaColor.Danger);
            yield return new(new AOEShapeRect(6, 2, 2), c.Position, c.Rotation, WorldState.FutureTime(1.5f));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        // encourage AI mode casters to preposition out of harm's way
        foreach (var c in _heads)
            hints.AddForbiddenZone(new AOEShapeRect(50, 2, 2), c.Position, c.Rotation, WorldState.FutureTime(10));
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if ((OID)actor.OID == OID.DarkWanderer)
        {
            if (animState1 == 1)
                _heads.Add(actor);
            if (animState1 == 0)
                _heads.Remove(actor);
        }
    }
}

class D30TiamatsCloneStates : StateMachineBuilder
{
    public D30TiamatsCloneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DarkWyrmwing>()
            .ActivateOnEnter<DarkWyrmtail>()
            .ActivateOnEnter<DarkMegaflare>()
            .ActivateOnEnter<WheiMorn>()
            .ActivateOnEnter<CreatureOfDarkness>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 899, NameID = 12242)]
public class D30TiamatsClone(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -300), new ArenaBoundsSquare(19.5f));
