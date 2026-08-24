namespace BossMod.Dawntrail.Extreme.Ex1Valigarmanda;

class Stance(BossModule module) : Components.GenericAOEs(module)
{
    private (Actor, AOEShape)? _caster;

    private static readonly AOEShapeCone _shapeCone = new(50, 40.Degrees()); // TODO: verify angle & origin
    private static readonly AOEShapeCone _shapeOut = new(24, 90.Degrees());
    private static readonly AOEShapeDonut _shapeIn = new(8, 30);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_caster is { } c)
            yield return new(c.Item2, c.Item1.CastInfo!.LocXZ, c.Item1.CastInfo!.Rotation, Module.CastFinishAt(c.Item1.CastInfo));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        // TODO: origin should be spell.LocXZ (once it's fully fixed)
        AOEShape? shape = (AID)spell.Action.ID switch
        {
            AID.SusurrantBreathAOE => _shapeCone,
            AID.SlitheringStrikeAOE => _shapeOut,
            AID.StranglingCoilAOE => _shapeIn,
            _ => null
        };
        if (shape != null)
            _caster = (caster, shape);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SusurrantBreathAOE or AID.SlitheringStrikeAOE or AID.StranglingCoilAOE)
        {
            _caster = null;
            ++NumCasts;
        }
    }
}

class CharringCataclysm(BossModule module) : Components.UniformStackSpread(module, 4, 0, 2, 2)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.SusurrantBreathAOE or AID.SlitheringStrikeAOE or AID.StranglingCoilAOE)
        {
            // note: dd vs supports is random, select supports arbitrarily
            AddStacks(Module.Raid.WithoutSlot(true).Where(p => p.Class.IsSupport()), Module.CastFinishAt(spell, 0.7f));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.CharringCataclysm)
        {
            Stacks.Clear();
        }
    }
}

class ChillingCataclysm(BossModule module) : Components.GenericAOEs(module, AID.ChillingCataclysmAOE)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCross _shape = new(40, 2.5f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.ChillingCataclysmArcaneSphere)
        {
            _aoes.Add(new(_shape, actor.Position, 0.Degrees(), WorldState.FutureTime(5.6f)));
            _aoes.Add(new(_shape, actor.Position, 45.Degrees(), WorldState.FutureTime(5.6f)));
        }
    }
}

class CracklingCataclysm(BossModule module) : Components.StandardAOEs(module, AID.CracklingCataclysm, 6);
