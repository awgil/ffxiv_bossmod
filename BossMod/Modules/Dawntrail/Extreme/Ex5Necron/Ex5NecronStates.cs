namespace BossMod.Dawntrail.Extreme.Ex5Necron;

class BlueShockwave(BossModule module) : Components.TankSwap(module, default(AID), AID._Weaponskill_BlueShockwave1, AID._Weaponskill_BlueShockwave1, 4.1f, new AOEShapeCone(100, 50.Degrees()), false)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == 615)
        {
            _source = actor;
            _prevTarget = targetID;
            _activation = WorldState.FutureTime(7.2f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if (NumCasts >= 2)
        {
            CurrentBaits.Clear();
            _source = null;
            _activation = default;
        }
    }
}

class FearOfDeathRaidwide(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_FearOfDeath);
class FearOfDeathPuddle(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_FearOfDeath1, 3);

class ChokingGraspInstant(BossModule module) : Components.GenericBaitAway(module, AID._Weaponskill_ChokingGrasp, damageType: AIHints.PredictedDamageType.Raidwide)
{
    private readonly List<Actor> _hands = [];

    private DateTime _activation;

    public override void Update()
    {
        CurrentBaits.Clear();

        if (_activation == default)
            return;

        foreach (var h in _hands)
        {
            var target = Raid.WithoutSlot().Closest(h.Position);
            if (target != null)
            {
                CurrentBaits.Add(new(h, target, new AOEShapeRect(24, 3), _activation));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _hands.Remove(caster);
            _activation = default;
        }

        if ((AID)spell.Action.ID == AID._Weaponskill_FearOfDeath1)
        {
            var hand = Module.Enemies(0x490C).Closest(spell.TargetXZ);
            if (hand != null)
                _hands.Add(hand);

            if (_activation == default)
                _activation = WorldState.FutureTime(2.7f);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        Arena.Actors(_hands, ArenaColor.Object, true);
    }
}

class ColdGrip(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_ColdGrip1)
{
    private Actor? _leftHand;
    private Actor? _rightHand;

    enum Side
    {
        Unknown,
        Left,
        Right
    }

    private Side _safeSide;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_leftHand is { } h)
            yield return new AOEInstance(new AOEShapeRect(100, 6), h.CastInfo!.LocXZ, h.CastInfo!.Rotation, Module.CastFinishAt(h.CastInfo), Color: _safeSide == Side.Right ? ArenaColor.Danger : ArenaColor.AOE);
        if (_rightHand is { } h2)
            yield return new AOEInstance(new AOEShapeRect(100, 6), h2.CastInfo!.LocXZ, h2.CastInfo!.Rotation, Module.CastFinishAt(h2.CastInfo), Color: _safeSide == Side.Left ? ArenaColor.Danger : ArenaColor.AOE);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            if (spell.LocXZ.X > Arena.Center.X)
                _rightHand = caster;
            else
                _leftHand = caster;
        }

        if ((AID)spell.Action.ID == AID._Weaponskill_ColdGrip)
            _safeSide = Side.Left;
        if ((AID)spell.Action.ID == AID._Weaponskill_ColdGrip2)
            _safeSide = Side.Right;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;

            if (_rightHand == caster)
                _rightHand = null;
            if (_leftHand == caster)
                _leftHand = null;
        }
    }
}

class ExistentialDread(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_ExistentialDread)
{
    enum Side
    {
        Unknown,
        Left,
        Right
    }

    private Side _safeSide;
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
        {
            var src = Module.PrimaryActor.Position;
            if (_safeSide == Side.Right)
                src.X -= 6;
            else
                src.X += 6;

            yield return new(new AOEShapeRect(100, 12), src, default, _activation);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_ColdGrip:
                _safeSide = Side.Left;
                break;
            case AID._Weaponskill_ColdGrip2:
                _safeSide = Side.Right;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_ColdGrip1 && _activation == default)
            _activation = WorldState.FutureTime(1.6f);

        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _activation = default;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_safeSide != default)
            hints.Add($"Safe side: {_safeSide}");
    }
}

class MementoMoriLine(BossModule module) : Components.GroupedAOEs(module, [AID.MementoMoriDarkRight, AID.MementoMoriDarkLeft], new AOEShapeRect(100, 6))
{
    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);

        if (Casters.Count > 0)
        {
            var isTank = pc.Role == Role.Tank;

            var tanksLeft = (AID)Casters[0].CastInfo!.Action.ID == AID.MementoMoriDarkLeft;

            WPos safeOrig = (isTank == tanksLeft) ? new(88, 85) : new(112, 85);

            Arena.ZoneRect(safeOrig, safeOrig + new WDir(0, 30), 6, ArenaColor.SafeFromAOE);
        }
    }
}

class MementoMoriVoidzone(BossModule module) : Components.GenericAOEs(module)
{
    public bool Active { get; private set; }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Active)
            yield return new AOEInstance(new AOEShapeRect(30, 6), new(100, 85));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.MementoMoriDarkLeft or AID.MementoMoriDarkRight)
            Active = true;
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x18 && state == 0x00080004)
            Active = false;
    }
}

class SmiteOfGloom(BossModule module) : Components.SpreadFromCastTargets(module, AID._Weaponskill_SmiteOfGloom, 10);

class MMChokingGrasp(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_ChokingGrasp1)
{
    private readonly List<(Actor, DateTime)> _casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var (c, predicted) in _casters)
        {
            var activation = c.CastInfo == null ? predicted : Module.CastFinishAt(c.CastInfo);

            yield return new AOEInstance(new AOEShapeRect(24, 3), c.Position, c.Rotation, activation);
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID._Gen_IcyHands && id == 0x11D2)
            _casters.Add((actor, WorldState.FutureTime(5.7f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _casters.RemoveAll(c => c.Item1 == caster);
        }
    }
}

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
            _ => throw new Exception("unreachable")
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

class EndsEmbrace(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.SmallSpread4s, AID._Weaponskill_TheEndsEmbrace1, 3, 4.1f);

class EndsEmbraceBait(BossModule module) : Components.GenericBaitAway(module)
{
    private readonly List<Actor> _hands = [];

    private DateTime _activation;

    public bool Baited;

    public override void Update()
    {
        CurrentBaits.Clear();
        foreach (var h in _hands)
        {
            var closest = Raid.WithoutSlot().Closest(h.Position)!;
            CurrentBaits.Add(new(h, closest, new AOEShapeRect(24, 3), _activation));
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID._Gen_IcyHands && id == 0x11D2)
        {
            _hands.Add(actor);
            _activation = WorldState.FutureTime(2.3f);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_ChokingGrasp1)
        {
            _hands.Remove(caster);
            Baited = true;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        Arena.Actors(_hands, ArenaColor.Object, true);
    }
}

class DelayedChokingGrasp(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_ChokingGrasp1, new AOEShapeRect(24, 3));

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

class Ex5NecronStates : StateMachineBuilder
{
    public Ex5NecronStates(BossModule module) : base(module)
    {
        DeathPhase(0, P1);
    }

    private void P1(uint id)
    {
        BlueShockwave(id, 14.2f);
        FearOfDeath(id + 0x100, 5);
        ColdGrip(id + 0x10000, 0);
        MementoMoriP1(id + 0x20000, 5.6f);
        SoulReaping(id + 0x30000, 2.2f);

        Timeout(id + 0xFF0000, 9999, "???");
    }

    private void BlueShockwave(uint id, float delay)
    {
        ComponentCondition<BlueShockwave>(id, delay, b => b.NumCasts > 0, "Tankbuster 1")
            .ActivateOnEnter<BlueShockwave>();
        ComponentCondition<BlueShockwave>(id + 0x10, 4.1f, b => b.NumCasts > 1, "Tankbuster 2")
            .DeactivateOnExit<BlueShockwave>();
    }

    private void FearOfDeath(uint id, float delay)
    {
        CastStart(id, AID._Weaponskill_FearOfDeath, delay)
            .ActivateOnEnter<FearOfDeathRaidwide>()
            .ActivateOnEnter<FearOfDeathPuddle>()
            .ActivateOnEnter<ChokingGraspInstant>();

        ComponentCondition<FearOfDeathRaidwide>(id + 0x10, 5, f => f.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<FearOfDeathRaidwide>();

        ComponentCondition<FearOfDeathPuddle>(id + 0x20, 3, f => f.NumCasts > 0, "Hands appear")
            .DeactivateOnExit<FearOfDeathPuddle>();

        ComponentCondition<ChokingGraspInstant>(id + 0x30, 2.8f, c => c.NumCasts > 0, "Baits")
            .DeactivateOnExit<ChokingGraspInstant>();
    }

    private void ColdGrip(uint id, float delay)
    {
        CastStartMulti(id, [AID._Weaponskill_ColdGrip, AID._Weaponskill_ColdGrip2], delay)
            .ActivateOnEnter<ColdGrip>()
            .ActivateOnEnter<ExistentialDread>();

        ComponentCondition<ColdGrip>(id + 0x10, 5.3f, c => c.NumCasts > 0, "Line AOEs")
            .DeactivateOnExit<ColdGrip>();

        ComponentCondition<ExistentialDread>(id + 0x20, 1.6f, e => e.NumCasts > 0, "Safe side").DeactivateOnExit<ExistentialDread>();
    }

    private void MementoMoriP1(uint id, float delay)
    {
        CastStartMulti(id, [AID.MementoMoriDarkRight, AID.MementoMoriDarkLeft], delay)
            .ActivateOnEnter<MementoMoriLine>()
            .ActivateOnEnter<MementoMoriVoidzone>()
            .ActivateOnEnter<SmiteOfGloom>()
            .ActivateOnEnter<MMChokingGrasp>();

        ComponentCondition<MementoMoriLine>(id + 0x10, 5, m => m.NumCasts > 0, "Line AOE")
            .DeactivateOnExit<MementoMoriLine>();

        ComponentCondition<MMChokingGrasp>(id + 0x20, 6.2f, m => m.NumCasts > 0, "Hands")
            .DeactivateOnExit<MMChokingGrasp>();

        ComponentCondition<SmiteOfGloom>(id + 0x30, 1, s => s.NumFinishedSpreads > 0, "Spreads")
            .DeactivateOnExit<SmiteOfGloom>();

        ComponentCondition<MementoMoriVoidzone>(id + 0x40, 4, m => !m.Active, "Voidzone disappear")
            .DeactivateOnExit<MementoMoriVoidzone>();
    }

    private void SoulReaping(uint id, float delay)
    {
        Cast(id, AID._Weaponskill_SoulReaping, delay, 4, "Store AOE")
            .ActivateOnEnter<Aetherblight>()
            .ActivateOnEnter<Shockwave>();

        ComponentCondition<Aetherblight>(id + 0x10, 9.4f, a => a.NumCasts > 0);
        ComponentCondition<Shockwave>(id + 0x12, 0.1f, s => s.NumCasts > 0, "Stored AOE + stacks")
            .DeactivateOnExit<Shockwave>();

        Cast(id + 0x20, AID._Weaponskill_SoulReaping, 1.9f, 4, "Store AOE");

        FearOfDeath(id + 0x100, 3.2f);

        ComponentCondition<EndsEmbrace>(id + 0x200, 4.3f, e => e.NumFinishedSpreads > 0, "Spreads")
            .ActivateOnEnter<EndsEmbrace>()
            .ActivateOnEnter<EndsEmbraceBait>()
            .ActivateOnEnter<DelayedChokingGrasp>();

        ComponentCondition<EndsEmbraceBait>(id + 0x210, 1, e => e.CurrentBaits.Count > 0, "Hands 2 appear");

        ComponentCondition<EndsEmbraceBait>(id + 0x220, 2.2f, e => e.Baited)
            .ActivateOnEnter<BlueShockwave>()
            .DeactivateOnExit<EndsEmbrace>()
            .DeactivateOnExit<EndsEmbraceBait>();

        ComponentCondition<DelayedChokingGrasp>(id + 0x230, 3, e => e.NumCasts > 0, "Baits resolve")
            .DeactivateOnExit<DelayedChokingGrasp>();

        ComponentCondition<BlueShockwave>(id + 0x300, 5.1f, b => b.NumCasts > 0, "Tankbuster 1");
        ComponentCondition<BlueShockwave>(id + 0x310, 4.1f, b => b.NumCasts > 1, "Tankbuster 2")
            .DeactivateOnExit<BlueShockwave>();

        ComponentCondition<Aetherblight>(id + 0x400, 7.1f, a => a.NumCasts > 1, "Stored AOE + stacks")
            .ActivateOnEnter<Shockwave>()
            .DeactivateOnExit<Aetherblight>();
    }
}
