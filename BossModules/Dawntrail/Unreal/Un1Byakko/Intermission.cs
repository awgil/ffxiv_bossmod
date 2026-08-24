namespace BossMod.Dawntrail.Unreal.Un1Byakko;

class VoiceOfThunder : Components.PersistentInvertibleVoidzone
{
    public VoiceOfThunder(BossModule module) : base(module, 2, m => m.Enemies(OID.AramitamaSoul).Where(x => !x.IsDead))
    {
        InvertResolveAt = WorldState.CurrentTime;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Sources(Module).Any(x => !Shape.Check(actor.Position, x)))
            hints.Add("Touch the balls!");
    }
}

class Intermission(BossModule module) : BossComponent(module)
{
    public bool Active;

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if ((OID)actor.OID == OID.ArenaFeatures && state is 0x00040008 or 0x00100020)
            Active = state == 0x00040008;
    }
}

class IntermissionOrbAratama(BossModule module) : Components.GenericAOEs(module, AID.IntermissionOrbAratama, "GTFO from puddle!")
{
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShapeCircle _shape = new(2);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.IntermissionOrbSpawn:
                AOEs.Add(new(_shape, spell.TargetXZ, default, WorldState.FutureTime(5.1f)));
                break;
            case AID.IntermissionOrbAratama:
                ++NumCasts;
                AOEs.RemoveAll(aoe => aoe.Origin.AlmostEqual(spell.TargetXZ, 1));
                break;
        }
    }
}

class IntermissionSweepTheLeg(BossModule module) : Components.StandardAOEs(module, AID.IntermissionSweepTheLeg, new AOEShapeDonut(5, 25));
class ImperialGuard(BossModule module) : Components.StandardAOEs(module, AID.ImperialGuard, new AOEShapeRect(44.75f, 2.5f));
class FellSwoop(BossModule module) : Components.CastCounter(module, AID.FellSwoop);
