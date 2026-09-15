namespace BossMod.Dawntrail.Extreme.Ex5Necron;

class Aetherblight(BossModule module) : Components.GenericAOEs(module)
{
    public enum Blight
    {
        Circle,
        Donut,
        InverseRect,
        Rect
    }

    protected readonly List<Blight> _order = [];

    protected DateTime _nextActivation;

    public override void AddGlobalHints(GlobalHints hints)
    {
        static string readable(Blight b) => b switch
        {
            Blight.Circle => "Out",
            Blight.Donut => "In",
            Blight.InverseRect => "Center",
            Blight.Rect => "Walls",
            _ => null!
        };

        if (_order.Count > 0)
            hints.Add($"Next: {string.Join(" -> ", _order.Select(readable))}");
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _nextActivation == default ? [] : RenderAOEs().Take(1).SelectMany(x => x);

    protected IEnumerable<AOEInstance[]> RenderAOEs()
    {
        var next = _nextActivation;
        foreach (var aoe in _order)
        {
            switch (aoe)
            {
                case Blight.Circle:
                    yield return [new(new AOEShapeCircle(20), Module.PrimaryActor.Position, default, _nextActivation)];
                    break;
                case Blight.Donut:
                    yield return [new(new AOEShapeDonut(16, 60), Module.PrimaryActor.Position, default, _nextActivation)];
                    break;
                case Blight.Rect:
                    yield return [new(new AOEShapeRect(100, 6), Module.PrimaryActor.Position, default, _nextActivation)];
                    break;
                case Blight.InverseRect:
                    yield return [new(new AOEShapeRect(100, 6), new WPos(88, 85), default, _nextActivation),
                     new(new AOEShapeRect(100, 6), new WPos(112, 85), default, _nextActivation)];
                    break;
            }
            next = next.AddSeconds(2.8f);
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
        if ((AID)spell.Action.ID is AID.TwofoldBlight or AID.FourfoldBlight)
            _nextActivation = Module.CastFinishAt(spell, 1.2f);

        if ((AID)spell.Action.ID is AID.TheSecondSeason or AID.TheFourthSeason)
            _nextActivation = Module.CastFinishAt(spell, 1.2f);
    }

    private int _numCasts;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.AetherblightSides:
                if (++_numCasts >= 2)
                    RecordCast();
                break;
            case AID.AetherblightMiddle:
            case AID.AetherblightCircle:
            case AID.AetherblightDonut:
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

class CropCircle(BossModule module) : Aetherblight(module)
{
    public bool Active;
    private bool _failed;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Active && !_failed)
        {
            var danger = true;
            foreach (var aoes in RenderAOEs().Take(2))
            {
                foreach (var aoe in aoes)
                    yield return aoe with { Color = danger ? ArenaColor.Danger : ArenaColor.AOE, Risky = danger };
                danger = false;
            }
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_failed)
            hints.Add("Unable to predict order. Hope your party members were watching!");
        else if (Active)
            base.AddGlobalHints(hints);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.TheSecondSeason or AID.TheFourthSeason)
            Active = true;
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (Active)
            return;

        if ((OID)actor.OID == OID.Boss)
        {
            switch (modelState)
            {
                case 0x15:
                    // no rotation
                    break;
                case 0x93:
                    RotateSafe(1);
                    break;
                case 0x41:
                    RotateSafe(2);
                    break;
                case 0x16:
                    RotateSafe(3);
                    break;
            }
        }
    }

    private void RotateSafe(int count)
    {
        if (_order.Count <= count)
        {
            ReportError($"CropCircle triggered but we didn't see the order, unable to predict");
            _failed = true;
            return;
        }
        for (var i = 0; i < count; i++)
        {
            var first = _order[0];
            _order.RemoveAt(0);
            _order.Add(first);
        }
    }
}

class Shockwave(BossModule module) : Components.CastCounter(module, default)
{
    private int _numBaits;
    private DateTime _activation;

    public static readonly AOEShape Shape2 = new AOEShapeCone(100, 10.Degrees());
    public static readonly AOEShape Shape4 = new AOEShapeCone(100, 10.Degrees());

    public bool Enabled = true;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.TwofoldBlight:
                _numBaits = 2;
                _activation = Module.CastFinishAt(spell, 1.4f);
                break;
            case AID.FourfoldBlight:
                _numBaits = 4;
                _activation = Module.CastFinishAt(spell, 1.4f);
                break;
            case AID.TheSecondSeason:
                _numBaits = 2;
                _activation = Module.CastFinishAt(spell, 9.8f);
                break;
            case AID.TheFourthSeason:
                _numBaits = 4;
                _activation = Module.CastFinishAt(spell, 9.8f);
                break;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!Enabled)
            return;

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
        if (!Enabled)
            return;

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
        if ((AID)spell.Action.ID is AID.ShockwaveParties or AID.ShockwavePairs)
        {
            _numBaits = 0;
            NumCasts++;
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        if (Enabled)
        {
            if (_numBaits == 2 && player.Role == Role.Healer)
                return pc.Role == Role.Healer ? PlayerPriority.Danger : PlayerPriority.Interesting;

            if (_numBaits == 4 && player.Class.IsSupport() == pc.Class.IsSupport())
                return PlayerPriority.Interesting;
        }

        return base.CalcPriority(pcSlot, pc, playerSlot, player, ref customColor);
    }
}
