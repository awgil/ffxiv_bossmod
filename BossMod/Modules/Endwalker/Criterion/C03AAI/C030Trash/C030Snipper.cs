namespace BossMod.Endwalker.Criterion.C03AAI.C030Trash1;

class Water(BossModule module, AID aid) : Components.StackWithCastTargets(module, aid, 8, 4);
class NWater(BossModule module) : Water(module, AID.NWater);
class SWater(BossModule module) : Water(module, AID.SWater);

class BubbleShowerCrabDribble(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCone _shape1 = new(9, 45.Degrees());
    private static readonly AOEShapeCone _shape2 = new(6, 60.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NBubbleShower or AID.SBubbleShower)
        {
            _aoes.Clear();
            _aoes.Add(new(_shape1, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
            _aoes.Add(new(_shape2, caster.Position, spell.Rotation + 180.Degrees(), Module.CastFinishAt(spell, 3.6f)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NBubbleShower or AID.SBubbleShower or AID.NCrabDribble or AID.SCrabDribble && _aoes.Count > 0)
        {
            _aoes.RemoveAt(0);
        }
    }
}

class C030SnipperStates : StateMachineBuilder
{
    private readonly bool _savage;

    public C030SnipperStates(BossModule module, bool savage) : base(module)
    {
        _savage = savage;
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<NWater>(!_savage)
            .ActivateOnEnter<SWater>(_savage)
            .ActivateOnEnter<BubbleShowerCrabDribble>()
            .ActivateOnEnter<NTailScrew>(!_savage) // note: first mob is often pulled together with second one
            .ActivateOnEnter<STailScrew>(_savage)
            .ActivateOnEnter<Twister>();
    }

    private void SinglePhase(uint id)
    {
        Water(id, 7.7f);
        BubbleShowerCrabDribble(id + 0x10000, 2.1f);
        Water(id + 0x20000, 11.3f);
        BubbleShowerCrabDribble(id + 0x30000, 2.1f);
        SimpleState(id + 0xFF0000, 10, "???");
    }

    private void Water(uint id, float delay)
    {
        Cast(id, _savage ? AID.SWater : AID.NWater, delay, 5, "Stack");
    }

    private void BubbleShowerCrabDribble(uint id, float delay)
    {
        Cast(id, _savage ? AID.SBubbleShower : AID.NBubbleShower, delay, 5, "Cleave front");
        Cast(id + 0x10, _savage ? AID.SCrabDribble : AID.NCrabDribble, 2.1f, 1.5f, "Cleave back");
    }
}
class C030NSnipperStates(BossModule module) : C030SnipperStates(module, false);
class C030SSnipperStates(BossModule module) : C030SnipperStates(module, true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NSnipper, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 979, NameID = 12537, SortOrder = 2)]
public class C030NSnipper(WorldState ws, Actor primary) : C030Trash1(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.NCrab), ArenaColor.Enemy);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SSnipper, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 980, NameID = 12537, SortOrder = 2)]
public class C030SSnipper(WorldState ws, Actor primary) : C030Trash1(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.SCrab), ArenaColor.Enemy);
    }
}
