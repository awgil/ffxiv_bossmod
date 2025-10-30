namespace BossMod.Endwalker.Savage.P12S2PallasAthena;

class Gaiaochos(BossModule module) : Components.StandardAOEs(module, AID.GaiaochosTransition, new AOEShapeDonut(7, 30));

// TODO: we could show it earlier, casters do PATE 11D2 ~4s before starting cast
class UltimaRay(BossModule module) : Components.StandardAOEs(module, AID.UltimaRay, new AOEShapeRect(20, 3));

class MissingLink(BossModule module) : Components.Chains(module, (uint)TetherID.MissingLink, AID.MissingLink);

class DemiParhelion(BossModule module) : Components.StandardAOEs(module, AID.DemiParhelionAOE, new AOEShapeCircle(2));

class Geocentrism(BossModule module) : Components.GenericAOEs(module)
{
    public int NumConcurrentAOEs { get; private set; }
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shapeLine = new(20, 2);
    private static readonly AOEShapeCircle _shapeCircle = new(2);
    private static readonly AOEShapeDonut _shapeDonut = new(3, 7);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.GeocentrismV:
                _aoes.Add(new(_shapeLine, new(95, 83), default, Module.CastFinishAt(spell, 0.6f)));
                _aoes.Add(new(_shapeLine, new(100, 83), default, Module.CastFinishAt(spell, 0.6f)));
                _aoes.Add(new(_shapeLine, new(105, 83), default, Module.CastFinishAt(spell, 0.6f)));
                NumConcurrentAOEs = 3;
                break;
            case AID.GeocentrismC:
                _aoes.Add(new(_shapeCircle, new(100, 90), default, Module.CastFinishAt(spell, 0.6f)));
                _aoes.Add(new(_shapeDonut, new(100, 90), default, Module.CastFinishAt(spell, 0.6f)));
                NumConcurrentAOEs = 2;
                break;
            case AID.GeocentrismH:
                _aoes.Add(new(_shapeLine, new(93, 85), 90.Degrees(), Module.CastFinishAt(spell, 0.6f)));
                _aoes.Add(new(_shapeLine, new(93, 90), 90.Degrees(), Module.CastFinishAt(spell, 0.6f)));
                _aoes.Add(new(_shapeLine, new(93, 95), 90.Degrees(), Module.CastFinishAt(spell, 0.6f)));
                NumConcurrentAOEs = 3;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.DemiParhelionGeoLine or AID.DemiParhelionGeoDonut or AID.DemiParhelionGeoCircle)
            ++NumCasts;
    }
}

class DivineExcoriation(BossModule module) : Components.UniformStackSpread(module, 0, 1)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.DivineExcoriation)
            AddSpread(actor, WorldState.FutureTime(3.1f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.DivineExcoriation)
            Spreads.Clear();
    }
}

class GaiaochosEnd(BossModule module) : BossComponent(module)
{
    public bool Finished { get; private set; }

    public override void OnMapEffect(byte index, uint state)
    {
        // note: there are 3 env controls happening at the same time, not sure which is the actual trigger: .9=02000001, .11=00800001, .12=00080004
        if (index == 9 && state == 0x02000001)
            Finished = true;
    }
}

// TODO: assign pairs, draw wrong pairs as aoes
class UltimaBlow(BossModule module) : Components.CastCounter(module, AID.UltimaBlow)
{
    private readonly List<(Actor source, Actor target)> _tethers = [];
    private BitMask _vulnerable;

    private static readonly AOEShapeRect _shape = new(20, 3);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_vulnerable[slot])
        {
            var source = _tethers.Find(t => t.target == actor).source;
            var numHit = source != null ? Raid.WithoutSlot().Exclude(actor).InShape(_shape, source.Position, Angle.FromDirection(actor.Position - source.Position)).Count() : 0;
            if (numHit == 0)
                hints.Add("Hide behind partner!");
            else if (numHit > 1)
                hints.Add("Bait away from raid!");
        }
        else if (_tethers.Count > 0)
        {
            var numHit = _tethers.Count(t => _shape.Check(actor.Position, t.source.Position, Angle.FromDirection(t.target.Position - t.source.Position)));
            if (numHit == 0)
                hints.Add("Intercept the charge!");
            else if (numHit > 1)
                hints.Add("GTFO from other charges!");
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return _tethers.Any(t => t.target == player) ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_vulnerable[pcSlot]) // TODO: reconsider
            foreach (var t in _tethers.Where(t => t.target != pc))
                _shape.Draw(Arena, t.source.Position, Angle.FromDirection(t.target.Position - t.source.Position));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var t in _tethers)
        {
            Arena.Actor(t.source, ArenaColor.Object, true);
            Arena.AddLine(t.source.Position, t.target.Position, ArenaColor.Danger);
            if (t.target == pc || !_vulnerable[pcSlot])
                _shape.Outline(Arena, t.source.Position, Angle.FromDirection(t.target.Position - t.source.Position), t.target == pc ? ArenaColor.Safe : ArenaColor.Danger); // TODO: reconsider...
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.ClassicalConceptsShapes && WorldState.Actors.Find(tether.Target) is var target && target != null)
        {
            _tethers.Add((source, target));
            _vulnerable.Set(Raid.FindSlot(tether.Target));
        }
    }
}
