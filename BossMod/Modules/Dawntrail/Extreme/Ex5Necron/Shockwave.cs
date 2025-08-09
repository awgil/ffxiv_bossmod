namespace BossMod.Dawntrail.Extreme.Ex5Necron;

class Aetherblight(BossModule module) : Components.GenericAOEs(module)
{
    enum Blight
    {
        Circle,
        Donut,
        InverseRect,
        Rect
    }

    private readonly List<Blight> _order = [];

    private DateTime _nextActivation;

    public override void AddGlobalHints(GlobalHints hints)
    {
        static string readable(Blight b) => b switch
        {
            Blight.Circle => "Circle",
            Blight.Donut => "Donut",
            Blight.InverseRect => "Center safe",
            Blight.Rect => "Walls safe",
            _ => null!
        };

        if (_order.Count > 0)
            hints.Add($"Next: {string.Join(" -> ", _order.Select(readable))}");
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_nextActivation != default && _order.Count > 0)
        {
            switch (_order[0])
            {
                case Blight.Circle:
                    yield return new(new AOEShapeCircle(20), Module.PrimaryActor.Position, default, _nextActivation);
                    break;
                case Blight.Donut:
                    yield return new(new AOEShapeDonut(20, 60), Module.PrimaryActor.Position, default, _nextActivation);
                    break;
                case Blight.Rect:
                    yield return new(new AOEShapeRect(100, 6), Module.PrimaryActor.Position, default, _nextActivation);
                    break;
                case Blight.InverseRect:
                    yield return new(new AOEShapeRect(100, 6), new WPos(88, 85), default, _nextActivation);
                    yield return new(new AOEShapeRect(100, 6), new WPos(112, 85), default, _nextActivation);
                    break;
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (actor == Module.PrimaryActor)
        {
            Blight? blight = (IconID)iconID switch
            {
                IconID.StoreCircle => Blight.Circle,
                IconID.StoreDonut => Blight.Donut,
                IconID.StoreInverseRect => Blight.InverseRect,
                IconID.StoreRect => Blight.Rect,
                _ => null
            };

            if (blight != null)
                _order.Add(blight.Value);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_TwofoldBlight or AID._Weaponskill_FourfoldBlight)
            _nextActivation = Module.CastFinishAt(spell, 1.2f);
    }

    private int _numCasts;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_Aetherblight1:
                if (++_numCasts >= 2)
                    RecordCast();
                break;
            case AID._Weaponskill_Aetherblight3:
            case AID._Weaponskill_Aetherblight5:
            case AID._Weaponskill_Aetherblight7:
                RecordCast();
                break;
        }
    }

    private void RecordCast()
    {
        if (_order.Count > 0)
            _order.RemoveAt(0);
        NumCasts++;
        _nextActivation = default;
    }
}

class Shockwave(BossModule module) : Components.CastCounter(module, default)
{
    private int _numBaits;
    private DateTime _activation;

    public static readonly AOEShape Shape2 = new AOEShapeCone(100, 22.5f.Degrees());
    public static readonly AOEShape Shape4 = new AOEShapeCone(100, 10.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_TwofoldBlight:
                _numBaits = 2;
                _activation = Module.CastFinishAt(spell, 1.4f);
                break;
            case AID._Weaponskill_FourfoldBlight:
                _numBaits = 4;
                _activation = Module.CastFinishAt(spell, 1.4f);
                break;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_numBaits == 2)
        {
            var shape = Shape2;

            if (actor.Role == Role.Healer)
            {
                var stacked = Raid.WithoutSlot().Exclude(actor).InShape(shape, Module.PrimaryActor.Position, Module.PrimaryActor.AngleTo(actor)).ToList();
                if (stacked.Any(p => p.Role == Role.Healer))
                    hints.Add("GTFO from other healer!");

                hints.Add("Stack with party!", !stacked.Any(p => p.Role != Role.Healer));
            }
            else
            {
                var healers = Raid.WithoutSlot().Where(x => x.Role == Role.Healer);
                hints.Add("Stack with healer!", !healers.Any(h => shape.Check(actor.Position, Module.PrimaryActor.Position, Module.PrimaryActor.AngleTo(h))));
            }
        }

        if (_numBaits == 4)
        {
            var stacked = Raid.WithoutSlot().Exclude(actor).InShape(Shape4, Module.PrimaryActor.Position, Module.PrimaryActor.AngleTo(actor)).Count();

            var roleName = actor.Class.IsSupport() ? "supports" : "DPS";

            if (stacked > 1)
                hints.Add($"Bait away from other {roleName}!");
            else
                hints.Add("Stack with partner!", stacked == 0);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_numBaits > 0)
            hints.AddPredictedDamage(Raid.WithSlot().Mask(), _activation);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_numBaits == 2)
        {
            var shape = Shape2;

            foreach (var pm in Raid.WithoutSlot().Where(r => r.Role == Role.Healer))
            {
                if (pm == pc)
                    shape.Outline(Arena, Module.PrimaryActor.Position, Module.PrimaryActor.AngleTo(pm), ArenaColor.Safe);
                else if (pc.Role == Role.Healer)
                    shape.Draw(Arena, Module.PrimaryActor.Position, Module.PrimaryActor.AngleTo(pm), ArenaColor.AOE);
                else
                    shape.Draw(Arena, Module.PrimaryActor.Position, Module.PrimaryActor.AngleTo(pm), ArenaColor.SafeFromAOE);
            }
        }

        if (_numBaits == 4)
            Shape4.Outline(Arena, Module.PrimaryActor.Position, Module.PrimaryActor.AngleTo(pc), ArenaColor.Danger);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_Shockwave or AID._Weaponskill_Shockwave1)
        {
            _numBaits = 0;
            NumCasts++;
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        if (_numBaits == 2 && player.Role == Role.Healer)
            return pc.Role == Role.Healer ? PlayerPriority.Danger : PlayerPriority.Interesting;

        if (_numBaits == 4 && player.Class.IsSupport() == pc.Class.IsSupport())
            return PlayerPriority.Interesting;

        return base.CalcPriority(pcSlot, pc, playerSlot, player, ref customColor);
    }
}
