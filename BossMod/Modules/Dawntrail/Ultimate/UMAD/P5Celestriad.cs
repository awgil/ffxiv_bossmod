namespace BossMod.Dawntrail.Ultimate.UMAD;

class P5Celestriad(BossModule module) : Components.GenericTowers(module)
{
    enum Element
    {
        Fire, Ice, Lightning
    }

    static SID Debuff(Element el) => el switch
    {
        Element.Fire => SID._Gen_FireResistanceDownII,
        Element.Ice => SID._Gen_IceResistanceDownII,
        Element.Lightning => SID.LightningResistanceDownII,
        _ => default
    };

    record struct TTower(WPos Position, DateTime Activation, Element Element);

    readonly List<TTower> _towers = [];

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00100020)
        {
            Element? el = (OID)actor.OID switch
            {
                OID.FireTower => Element.Fire,
                OID.IceTower => Element.Ice,
                OID.LightningTower => Element.Lightning,
                _ => null
            };

            if (el.HasValue)
            {
                _towers.Add(new(actor.Position, WorldState.FutureTime(6.1f), el.Value));
                Assign();
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Ability_FireIII or AID._Ability_BlizzardIII or AID._Ability_ThunderIII)
        {
            NumCasts++;
            _towers.RemoveAll(t => t.Position.AlmostEqual(caster.Position, 1));
            Assign();
        }
    }

    void Assign()
    {
        Towers.Clear();

        Towers.AddRange(_towers.Select(t =>
        {
            var sid = Debuff(t.Element);
            var forbidden = Raid.WithSlot().WhereActor(a => a.FindStatus(sid, WorldState.FutureTime(20)) is { } status && status.ExpireAt > t.Activation);
            return new Tower(t.Position, 3, 2, 2, forbidden.Mask(), t.Activation);
        }));
    }
}

class P5CatastrophicChoice(BossModule module) : Components.GenericAOEs(module)
{
    AOEInstance? _predicted;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_predicted);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Ability_CatastrophicChoice:
                _predicted = new(new AOEShapeCircle(10), caster.Position, default, Module.CastFinishAt(spell, 0.8f));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Ability_Quake or AID._Ability_Tornado)
        {
            NumCasts++;
            _predicted = null;
        }
    }
}
