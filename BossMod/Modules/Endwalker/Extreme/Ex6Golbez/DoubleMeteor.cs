namespace BossMod.Endwalker.Extreme.Ex6Golbez;

class DragonsDescent(BossModule module) : Components.Knockback(module, AID.DragonsDescent)
{
    private Actor? _source;
    private DateTime _activation;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_source != null && _source != actor)
            yield return new(_source.Position, 13, _activation);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.DragonsDescent)
        {
            _source = actor;
            _activation = WorldState.FutureTime(8.2f);
        }
    }
}

class DoubleMeteor(BossModule module) : Components.UniformStackSpread(module, 0, 15, alwaysShowSpreads: true) // TODO: verify falloff
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.DoubleMeteor)
            AddSpread(actor, WorldState.FutureTime(11.1f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.DoubleMeteorAOE1 or AID.DoubleMeteorAOE2)
            Spreads.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
    }
}

class Explosion(BossModule module) : BossComponent(module)
{
    public bool Done { get; private set; }
    private BitMask _forbidden;
    private Actor? _towerTH;
    private Actor? _towerDD;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var tower = _forbidden[slot] ? null : actor.Class.IsSupport() ? _towerTH : _towerDD;
        if (tower != null)
            hints.Add("Soak the tower!", !actor.Position.InCircle(tower.Position, 4));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        DrawTower(_towerTH, !_forbidden[pcSlot] && pc.Class.IsSupport());
        DrawTower(_towerDD, !_forbidden[pcSlot] && pc.Class.IsDD());
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ExplosionDouble:
                _towerTH = caster;
                break;
            case AID.ExplosionTriple:
                _towerDD = caster;
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ExplosionDouble:
                _towerTH = null;
                Done = true;
                break;
            case AID.ExplosionTriple:
                _towerDD = null;
                Done = true;
                break;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID is IconID.DoubleMeteor or IconID.DragonsDescent)
            _forbidden.Set(Raid.FindSlot(actor.InstanceID));
    }

    private void DrawTower(Actor? tower, bool safe)
    {
        if (tower != null)
            Arena.AddCircle(tower.Position, 4, safe ? ArenaColor.Safe : ArenaColor.Danger, 2);
    }
}

class Cauterize(BossModule module) : Components.GenericBaitAway(module, AID.Cauterize)
{
    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Cauterize && WorldState.Actors.Find(tether.Target) is var target && target != null)
        {
            CurrentBaits.Add(new(source, target, new AOEShapeRect(50, 6)));
        }
    }
}
