namespace BossMod.Dawntrail.Savage.RM04SWickedThunder;

class FlameSlash(BossModule module) : Components.GenericAOEs(module, AID.FlameSlashAOE)
{
    public AOEInstance? AOE;
    public bool SmallArena;

    private static readonly AOEShapeRect _shape = new(40, 10);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(AOE);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            AOE = new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            AOE = null;
            SmallArena = true;
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 17 && state == 0x00400001)
            SmallArena = false;
    }
}

class RainingSwords(BossModule module) : Components.CastTowers(module, AID.RainingSwordsAOE, 3);

class ChainLightning(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<WPos> _explosions = [];

    private static readonly AOEShapeCircle _shape = new(7);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _explosions.Skip(NumCasts).Take(6).Select(p => new AOEInstance(_shape, p)); // TODO: activation

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID is TetherID.ChainLightning1 or TetherID.ChainLightning2)
            _explosions.Add(source.Position);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ChainLightningAOEFirst or AID.ChainLightningAOERest)
            ++NumCasts;
    }
}
