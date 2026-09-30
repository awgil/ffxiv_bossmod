namespace BossMod.RealmReborn.Dungeon.D04Halatali.D041Firemane;

public enum OID : uint
{
    Boss = 0x4643,
    Helper = 0x233C
}

public enum AID : uint
{
    Fire = 40055, // Boss->player, no cast, single-target
    FireII = 40592, // Boss->location, 4.0s cast, range 5 circle

    FireflowVisual = 40587, // Boss->self, 6.0s cast, single-target
    Fireflow1 = 40588, // Helper->self, 6.0s cast, range 60 45-degree cone
    Fireflow2 = 40589, // Helper->self, 9.0s cast, range 60 45-degree cone

    BurningBoltVisual = 40590, // Boss->self, 5.0s cast, single-target
    BurningBolt = 40591 // Helper->player, 5.0s cast, single-target
}

class FireII(BossModule module) : Components.StandardAOEs(module, AID.FireII, 5);
// two waves of 4 cones from the arena center, second wave rotated 45 degrees and resolving 3s later;
// showing both at once covers the whole arena, so only show whichever wave resolves next
class Fireflow(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone _shape = new(60, 22.5f.Degrees());
    private readonly List<(ulong caster, AOEInstance aoe)> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count == 0)
            yield break;
        var deadline = _aoes.Min(a => a.aoe.Activation).AddSeconds(1);
        foreach (var a in _aoes.Where(a => a.aoe.Activation <= deadline))
            yield return a.aoe;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Fireflow1 or AID.Fireflow2)
            _aoes.Add((caster.InstanceID, new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell))));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Fireflow1 or AID.Fireflow2)
            _aoes.RemoveAll(a => a.caster == caster.InstanceID);
    }
}
class BurningBolt(BossModule module) : Components.SingleTargetCast(module, AID.BurningBolt);

class D041FiremaneStates : StateMachineBuilder
{
    public D041FiremaneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FireII>()
            .ActivateOnEnter<Fireflow>()
            .ActivateOnEnter<BurningBolt>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 7, NameID = 1194)]
public class D041Firemane(ModuleInit init) : BossModule(init, init.Primary.Position, new ArenaBoundsCircle(20));
