namespace BossMod.Dawntrail.Ultimate.UMAD;

class P5Celestriad(BossModule module) : Components.GenericTowers(module)
{
    readonly UMADConfig _config = Service.Config.Get<UMADConfig>();

    enum Element
    {
        Fire, Ice, Lightning
    }

    static SID Debuff(Element el) => el switch
    {
        Element.Fire => SID.FireResistanceDownII,
        Element.Ice => SID.IceResistanceDownII,
        Element.Lightning => SID.LightningResistanceDownII,
        _ => default
    };

    class TTower
    {
        public required Actor Actor;
        public DateTime Activation;
        public Element Element;
        public BitMask Allowed;
        public (int Group, int Order) Pos;
    }

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
                _towers.Add(new() { Actor = actor, Activation = WorldState.FutureTime(6.1f), Element = el.Value, Pos = GetRelativeTowerPos(actor.Position) });
                Assign();
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.TriadFireIII or AID.TriadBlizzardIII or AID.TriadThunderIII)
        {
            NumCasts++;
            _towers.RemoveAll(t => t.Actor.Position.AlmostEqual(caster.Position, 1));
            Assign();
        }
    }

    void Assign()
    {
        Towers.Clear();

        if (_towers.Count != 4)
            return;

        if (_config.P5CelestriadStrategy == UMADConfig.P5CelestriadAssignment.None)
        {
            AssignSimple();
            return;
        }

        // players with the highest number of debuffs get first dibs
        // not checking expiration date here, since every player only has 2 vulns by the time set 3 resolves
        var soakers = Raid.WithSlot().GroupBy(a => new Element[] { Element.Fire, Element.Ice, Element.Lightning }.Count(e => a.Item2.FindStatus(Debuff(e), DateTime.MaxValue) != null)).OrderByDescending(s => s.Key).GetEnumerator();

        try
        {
            soakers.MoveNext();
        }
        catch (InvalidOperationException ex)
        {
            ReportError($"crash while assigning towers: {ex}");
            return;
        }

        foreach (var (slot, debuff) in soakers.Current)
        {
            var ft = _towers.Where(t => IsForbidden(debuff, t.Element, t.Activation));
            var forbidden = _towers.Where(t => IsForbidden(debuff, t.Element, t.Activation)).MaxBy(t => t.Pos.Group, new ClockComparer());

            if (forbidden == null)
            {
                ReportError($"Unable to find a forbidden tower for #{slot} {debuff}");
                continue;
            }

            var allowedGroup = (forbidden.Pos.Group + 1) % 3;

            var allowedTower = _towers.Where(t => t.Pos.Group == allowedGroup).MinBy(t => t.Pos.Order);

            if (allowedTower == null)
            {
                ReportError($"Unable to find a permitted tower for #{slot} {debuff}");
                continue;
            }

            allowedTower.Allowed.Set(slot);
        }

        try
        {
            soakers.MoveNext();
        }
        catch (InvalidOperationException ex)
        {
            ReportError($"crash while assigning towers: {ex}");
            return;
        }

        var others = soakers.Current.Mask();

        foreach (var t in _towers)
            Towers.Add(new(t.Actor.Position, 3, 2, 2, t.Allowed.Any() ? ~t.Allowed : ~others, t.Activation));
    }

    void AssignSimple()
    {
        foreach (var t in _towers)
            Towers.Add(new(t.Actor.Position, 3, 2, 2, Raid.WithSlot().WhereActor(a => IsForbidden(a, t.Element, t.Activation)).Mask(), t.Activation));
    }

    (int Group, int Order) GetRelativeTowerPos(WPos t)
    {
        // 0 if NW
        // 1 if NE
        // 2 if S
        var (group, angle) = t.Z > 102 ? (2, default) : t.X < 100 ? (0, -120.Degrees()) : (1, 120.Degrees());

        var tAngle = (t - Arena.Center).ToAngle() - angle;

        var res = (group, MathF.Abs(tAngle.Rad) < 0.1f ? 1 : tAngle.Rad < 0 ? 2 : 0);

        if (_config.P5CelestriadStrategy == UMADConfig.P5CelestriadAssignment.CCW)
            return (2 - res.group, 2 - res.Item2);

        return res;
    }

    bool IsForbidden(Actor actor, Element e, DateTime a)
    {
        return actor.FindStatus(Debuff(e), WorldState.FutureTime(20)) is { } status && status.ExpireAt > a;
    }
}

class ClockComparer : IComparer<int>
{
    public int Compare(int x, int y)
    {
        if (x == y)
            return 0;

        if (x == 0 && y == 2)
            return 1;

        if (x == 2 && y == 0)
            return -1;

        return x - y;
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
            case AID.CatastrophicChoiceEarth:
                _predicted = new(new AOEShapeCircle(10), caster.Position, default, Module.CastFinishAt(spell, 0.8f));
                break;
            case AID.CatastrophicChoiceWind:
                _predicted = new(new AOEShapeDonut(10, 40), caster.Position, default, Module.CastFinishAt(spell, 0.8f));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Quake or AID.Tornado)
        {
            NumCasts++;
            _predicted = null;
        }
    }
}
