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

#pragma warning disable CS0649
    // TODO: figure out a good way to communicate this visually, Color=Danger completely obscures other AOEs in p2
    public bool Highlight;
#pragma warning restore

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_leftHand is { } h)
            yield return new AOEInstance(new AOEShapeRect(100, 6), h.CastInfo!.LocXZ, h.CastInfo!.Rotation, Module.CastFinishAt(h.CastInfo), Color: _safeSide == Side.Right && Highlight ? ArenaColor.Danger : ArenaColor.AOE);
        if (_rightHand is { } h2)
            yield return new AOEInstance(new AOEShapeRect(100, 6), h2.CastInfo!.LocXZ, h2.CastInfo!.Rotation, Module.CastFinishAt(h2.CastInfo), Color: _safeSide == Side.Left && Highlight ? ArenaColor.Danger : ArenaColor.AOE);
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

class FearOfDeathAdds(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_FearOfDeath2, 3);

class IcyHandsAdds(BossModule module) : Components.AddsMulti(module, [OID._Gen_IcyHands1, OID._Gen_BeckoningHands], 1)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var a in ActiveActors)
        {
            var e = hints.FindEnemy(a);
            e?.Priority = 1;
            e?.ForbidDOTs = true;
        }
    }
}

class ChokingGraspAdds(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_ChokingGrasp2, new AOEShapeRect(24, 3));

class MutedStruggle(BossModule module) : Components.SingleTargetCast(module, AID._Weaponskill_MutedStruggle);
class DarknessOfEternity(BossModule module) : Components.RaidwideCastDelay(module, AID._Weaponskill_DarknessOfEternity, AID._Weaponskill_DarknessOfEternity1, 6.4f);
class Invitation(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Invitation1, new AOEShapeRect(36, 5));

class LoomingSpecter(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_Invitation1)
{
    private readonly List<(Actor, DateTime)> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted.Select(p => new AOEInstance(new AOEShapeRect(36, 5), p.Item1.Position, p.Item1.Rotation, p.Item2));

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((OID)source.OID == OID._Gen_LoomingSpecter && tether.ID == 102)
            _predicted.Add((source, WorldState.FutureTime(12.1f)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _predicted.Clear();
    }
}

class CrapRotation(BossModule module) : BossComponent(module)
{
    private readonly List<string> _icons = [];
    private readonly List<string> _casts = [];
    private readonly List<string> _models = [];

    // modelstate 0x15 = order 1
    // modelstate 0x93 = order 2
    // modelstate 0x41 = order 3
    // modelstate 0x16 = order 4
    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if ((OID)actor.OID == OID.Boss)
            _models.Add(modelState.ToString("X2"));
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((OID)actor.OID == OID.Boss && iconID is >= 604 and <= 607)
            _icons.Add(((IconID)iconID).ToString());
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_icons.Count > 0)
            hints.Add("Icons: " + string.Join(" -> ", _icons));
        if (_casts.Count > 0)
            hints.Add("Casts: " + string.Join(" -> ", _casts));
        if (_models.Count > 0)
            hints.Add("Models: " + string.Join(" -> ", _models));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_Aetherblight1:
                _casts.Add("Inverse rect");
                break;
            case AID._Weaponskill_Aetherblight3:
                _casts.Add("Rect");
                break;
            case AID._Weaponskill_Aetherblight5:
                _casts.Add("Circle");
                break;
            case AID._Weaponskill_Aetherblight7:
                _casts.Add("Donut");
                break;
        }
    }
}

class Ex5NecronStates : StateMachineBuilder
{
    public Ex5NecronStates(BossModule module) : base(module)
    {
        SimplePhase(0, P1, "P1")
            .Raw.Update = () => Module.Enemies(0x490D).Any() || Module.Enemies(0x490E).Any();
        SimplePhase(1, PJail, "Intermission")
            .OnEnter(() =>
            {
                // meh
                if (Module.Raid.Player() is { } player)
                {
                    Module.Arena.Center = player.Position - new WDir(0, 7.5f);
                    Module.Arena.Bounds = new ArenaBoundsCircle(9);
                }
            })
            .Raw.Update = () => Module.PrimaryActor.IsTargetable;
        DeathPhase(2, P2)
            .OnEnter(() =>
            {
                Module.Arena.Center = new(100, 100);
                Module.Arena.Bounds = new ArenaBoundsRect(18, 15);
            });
    }

    private void P1(uint id)
    {
        BlueShockwave(id, 14.2f);
        FearOfDeath(id + 0x100, 5);
        ColdGrip(id + 0x10000, 0);
        MementoMoriP1(id + 0x20000, 5.6f);
        SoulReaping(id + 0x30000, 2.2f);
        GrandCross(id + 0x40000, 7.9f);
        Adds(id + 0x50000, 14.6f);
    }

    private void PJail(uint id)
    {
        Timeout(id + 0x10, 50, "Doom")
            .ActivateOnEnter<JailHands>()
            .ActivateOnEnter<JailGrasp>();
    }

    private void P2(uint id)
    {
        EndsEmbraceP2(id, 5.7f);
        CropCircle(id + 0x10000, 6.6f);

        Timeout(id + 0xFF0000, 9999, "P3!");
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

        ComponentCondition<EndsEmbraceBait>(id + 0x220, 2.2f, e => e.Baited, "Baits")
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

    private void GrandCross(uint id, float delay)
    {
        Cast(id, AID._Weaponskill_GrandCross, delay, 7, "Raidwide")
            .ActivateOnEnter<GrandCrossArena>()
            .ActivateOnEnter<GrandCrossRaidwide>()
            .ActivateOnEnter<GrandCrossPuddle>()
            .ActivateOnEnter<GrandCrossSpread>()
            .ActivateOnEnter<GrandCrossLine>()
            .ActivateOnEnter<GrandCrossLineCast>()
            .ActivateOnEnter<Shock>();

        ComponentCondition<GrandCrossArena>(id + 0x10, 1.1f, t => t.NumChanges > 0, "Arena change");

        ComponentCondition<Shock>(id + 0x20, 13, s => s.NumCasts >= 4, "Towers 1");
        ComponentCondition<Shock>(id + 0x30, 11, s => s.NumCasts >= 8, "Towers 2")
            .DeactivateOnExit<GrandCrossLine>()
            .DeactivateOnExit<GrandCrossLineCast>()
            .DeactivateOnExit<GrandCrossSpread>()
            .DeactivateOnExit<Shock>();

        ComponentCondition<GrandCrossProximity>(id + 0x100, 8.7f, g => g.NumCasts > 0, "Proximity")
            .ActivateOnEnter<GrandCrossProximity>()
            .DeactivateOnExit<GrandCrossPuddle>()
            .DeactivateOnExit<GrandCrossProximity>();

        Cast(id + 0x200, AID._Weaponskill_NeutronRing, 3.1f, 7)
            .ActivateOnEnter<NeutronRing>();

        ComponentCondition<NeutronRing>(id + 0x210, 2.6f, n => n.NumCasts > 0, "Raidwide + restore arena");
    }

    private void Adds(uint id, float delay)
    {
        Targetable(id, false, delay, "Boss disappears (adds)")
            .ActivateOnEnter<FearOfDeathAdds>()
            .ActivateOnEnter<IcyHandsAdds>()
            .ActivateOnEnter<ChokingGraspAdds>()
            .ActivateOnEnter<MutedStruggle>()
            .ClearHint(StateMachine.StateHint.DowntimeStart);

        Targetable(id + 0x100, true, 60, "Boss reappears")
            .ActivateOnEnter<DarknessOfEternity>()
            .DeactivateOnExit<FearOfDeathAdds>()
            .DeactivateOnExit<IcyHandsAdds>()
            .DeactivateOnExit<ChokingGraspAdds>()
            .DeactivateOnExit<MutedStruggle>()
            .ClearHint(StateMachine.StateHint.DowntimeEnd);

        Targetable(id + 0x200, false, 16.4f, "Boss disappears (intermission)");
    }

    private void EndsEmbraceP2(uint id, float delay)
    {
        Cast(id, AID._Weaponskill_SpecterOfDeath, delay, 5)
            .ActivateOnEnter<Invitation>()
            .ActivateOnEnter<LoomingSpecter>()
            .ActivateOnEnter<EndsEmbrace>()
            .ActivateOnEnter<EndsEmbraceBait>();

        ComponentCondition<EndsEmbrace>(id + 0x10, 8.2f, e => e.NumFinishedSpreads > 0, "Stacks")
            .ActivateOnEnter<DelayedChokingGrasp>()
            .DeactivateOnExit<EndsEmbrace>()
            .DeactivateOnExit<Invitation>()
            .DeactivateOnExit<LoomingSpecter>();

        CastStartMulti(id + 0x20, [AID._Weaponskill_ColdGrip, AID._Weaponskill_ColdGrip2], 2.1f)
            .ActivateOnEnter<ColdGrip>()
            .ActivateOnEnter<ExistentialDread>();

        ComponentCondition<EndsEmbraceBait>(id + 0x30, 1, e => e.Baited, "Baits")
            .DeactivateOnExit<EndsEmbraceBait>();

        ComponentCondition<ColdGrip>(id + 0x40, 4.9f, c => c.NumCasts > 0, "Line AOEs")
            .DeactivateOnExit<ColdGrip>();

        ComponentCondition<ExistentialDread>(id + 0x50, 1.6f, e => e.NumCasts > 0, "Safe side")
            .DeactivateOnExit<DelayedChokingGrasp>()
            .DeactivateOnExit<ExistentialDread>();
    }

    private void CropCircle(uint id, float delay)
    {
        CastStart(id, AID._Weaponskill_RelentlessReaping, delay)
            .ActivateOnEnter<CropCircle>()
            .ActivateOnEnter<Shockwave>()
            .ExecOnEnter<Shockwave>(s => s.Enabled = false);

        CastEnd(id + 1, 15);

        Cast(id + 0x10, AID._Weaponskill_CropRotation, 3.1f, 3);

        CastStartMulti(id + 0x20, [AID._Weaponskill_TheFourthSeason, AID._Weaponskill_TheSecondSeason], 3.2f);

        ComponentCondition<CropCircle>(id + 0x30, 9.3f, c => c.NumCasts > 0, "AOE 1");
        ComponentCondition<CropCircle>(id + 0x31, 2.8f, c => c.NumCasts > 1, "AOE 2");
        ComponentCondition<CropCircle>(id + 0x32, 2.8f, c => c.NumCasts > 2, "AOE 3")
            .ExecOnEnter<Shockwave>(s => s.Enabled = true);
        ComponentCondition<Shockwave>(id + 0x40, 2.9f, s => s.NumCasts > 0, "AOE 4 + stacks")
            .DeactivateOnExit<CropCircle>()
            .DeactivateOnExit<Shockwave>();
    }
}
