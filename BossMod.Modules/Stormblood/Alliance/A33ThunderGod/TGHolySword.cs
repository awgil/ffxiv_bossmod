namespace BossMod.Stormblood.Alliance.A33ThunderGod;

class TGHallowedBolt(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_HallowedBolt)
{
    private readonly List<AOEInstance> _predicted = [];
    public static readonly AOEShape Shape = new AOEShapeCircle(15);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_TGHolySword:
                _predicted.Add(new(Shape, spell.LocXZ + new WDir(12.5f, 21.7f).Rotate(spell.Rotation), default, Module.CastFinishAt(spell, 1)));
                _predicted.Add(new(Shape, spell.LocXZ + new WDir(-25, 0).Rotate(spell.Rotation), default, Module.CastFinishAt(spell, 1.6f)));
                _predicted.Add(new(Shape, spell.LocXZ + new WDir(12.5f, -21.7f).Rotate(spell.Rotation), default, Module.CastFinishAt(spell, 2.1f)));
                break;
            case AID._Weaponskill_TGHolySword2:
                _predicted.Add(new(Shape, spell.LocXZ + new WDir(-12.5f, 21.7f).Rotate(spell.Rotation), default, Module.CastFinishAt(spell, 1)));
                _predicted.Add(new(Shape, spell.LocXZ + new WDir(25, 0).Rotate(spell.Rotation), default, Module.CastFinishAt(spell, 1.6f)));
                _predicted.Add(new(Shape, spell.LocXZ + new WDir(-12.5f, -21.7f).Rotate(spell.Rotation), default, Module.CastFinishAt(spell, 2.1f)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            if (_predicted.Count > 0)
                _predicted.RemoveAt(0);
            NumCasts++;
        }
    }
}

class TGJudgmentBlade(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_JudgmentBlade)
{
    private readonly List<AOEInstance> _predicted = [];
    public static readonly AOEShape Shape = new AOEShapeDonut(20, 35);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_TGHolySword1:
                _predicted.Add(new(Shape, spell.LocXZ, default, Module.CastFinishAt(spell, 0.3f)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            if (_predicted.Count > 0)
                _predicted.RemoveAt(0);
            NumCasts++;
        }
    }
}

class TGNorthswainsStrike(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_NorthswainsStrike)
{
    private readonly List<AOEInstance> _predicted = [];
    public static readonly AOEShape Shape = new AOEShapeCircle(26);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_TGHolySword3:
                _predicted.Add(new(Shape, spell.LocXZ, default, Module.CastFinishAt(spell, 0.3f)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            if (_predicted.Count > 0)
                _predicted.RemoveAt(0);
            NumCasts++;
        }
    }
}

class TGDivineRuination(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_DivineRuination)
{
    private readonly List<AOEInstance> _predicted = [];
    public static readonly AOEShape Shape = new AOEShapeCircle(16);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_TGHolySword4:
                _predicted.Add(new(Shape, spell.LocXZ + new WDir(-12.5f, 21.7f).Rotate(spell.Rotation), default, Module.CastFinishAt(spell, 0.8f)));
                _predicted.Add(new(Shape, spell.LocXZ + new WDir(12.5f, 21.7f).Rotate(spell.Rotation), default, Module.CastFinishAt(spell, 4)));
                _predicted.Add(new(Shape, spell.LocXZ + new WDir(25, 0).Rotate(spell.Rotation), default, Module.CastFinishAt(spell, 7.1f)));
                break;
            case AID._Weaponskill_TGHolySword5:
                _predicted.Add(new(Shape, spell.LocXZ + new WDir(12.5f, 21.7f).Rotate(spell.Rotation), default, Module.CastFinishAt(spell, 0.8f)));
                _predicted.Add(new(Shape, spell.LocXZ + new WDir(-12.5f, 21.7f).Rotate(spell.Rotation), default, Module.CastFinishAt(spell, 4)));
                _predicted.Add(new(Shape, spell.LocXZ + new WDir(-25, 0).Rotate(spell.Rotation), default, Module.CastFinishAt(spell, 7.1f)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            if (_predicted.Count > 0)
                _predicted.RemoveAt(0);
            NumCasts++;
        }
    }
}
