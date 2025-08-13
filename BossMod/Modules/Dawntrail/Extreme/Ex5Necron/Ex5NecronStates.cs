namespace BossMod.Dawntrail.Extreme.Ex5Necron;

class BlueShockwave(BossModule module) : Components.TankSwap(module, default(AID), AID.BlueShockwaveAOE, AID.BlueShockwaveAOE, 4.1f, new AOEShapeCone(100, 50.Degrees()), false)
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

class FearOfDeathAdds(BossModule module) : Components.StandardAOEs(module, AID.FearOfDeathPuddleAdds, 3);

class IcyHandsAdds(BossModule module) : Components.AddsMulti(module, [OID.IcyHandsAdds, OID.BeckoningHands], 1)
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

class ChokingGraspAdds(BossModule module) : Components.StandardAOEs(module, AID.ChokingGraspAdds, new AOEShapeRect(24, 3));

class MutedStruggle(BossModule module) : Components.BaitAwayCast(module, AID.MutedStruggle, new AOEShapeRect(24, 3));
class DarknessOfEternity(BossModule module) : Components.RaidwideCastDelay(module, AID.DarknessOfEternityCast, AID.DarknessOfEternityRaidwide, 6.4f);
class Invitation(BossModule module) : Components.StandardAOEs(module, AID.Invitation, new AOEShapeRect(36, 5));

class LoomingSpecter(BossModule module) : Components.GenericAOEs(module, AID.Invitation)
{
    private readonly List<(Actor, DateTime)> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted.Select(p => new AOEInstance(new AOEShapeRect(36, 5), p.Item1.Position, p.Item1.Rotation, p.Item2));

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((OID)source.OID == OID.LoomingSpecter1 && tether.ID == 102)
            _predicted.Add((source, WorldState.FutureTime(12.1f)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _predicted.Clear();
    }
}

class CircleOfLives(BossModule module) : Components.StandardAOEs(module, AID.CircleOfLives, new AOEShapeDonut(3, 50), maxCasts: 1);

class Ex5NecronStates : StateMachineBuilder
{
    private static readonly WPos[] JailArenas = [
        new(-100, -100),
        new(100, -100),
        new(-100, 100),
        new(300, -100),
        new(300, 100),
        new(300, 300),
        new(100, 300),
        new(-100, 300),
    ];

    public Ex5NecronStates(BossModule module) : base(module)
    {
        SimplePhase(0, P1, "P1")
            .Raw.Update = () => Module.Enemies(OID.IcyHandsDPSJail).Any() || Module.Enemies(OID.IcyHandsHealerJail).Any() || Module.Enemies(OID.IcyHandsTankJail).Any();
        SimplePhase(1, PJail, "Intermission")
            .OnEnter(() =>
            {
                // meh
                if (Module.Raid.Player() is { } player)
                {
                    var center = JailArenas.MinBy(j => (j - player.Position).LengthSq());
                    Module.Arena.Center = center;
                    Module.Arena.Bounds = new ArenaBoundsCircle(9);
                }
            })
            .Raw.Update = () => Module.PrimaryActor.IsTargetable;
        SimplePhase(2, P2, "P2")
            .OnEnter(() =>
            {
                Module.Arena.Center = new(100, 100);
                Module.Arena.Bounds = new ArenaBoundsRect(18, 15);
            })
            // don't check for IsDestroyed because it will trigger if the player gets jailed during P2
            .Raw.Update = () => Module.PrimaryActor.IsDead;
    }

    private void P1(uint id)
    {
        BlueShockwave(id, 14.2f);
        FearOfDeath(id + 0x100, 5);
        ColdGrip(id + 0x10000, 0);
        MementoMori(id + 0x20000, 5.6f);
        SoulReaping(id + 0x30000, 2.2f);
        GrandCross(id + 0x40000, 7.9f);
        Adds(id + 0x50000, 14.6f);
    }

    private void PJail(uint id)
    {
        Timeout(id + 0x10, 50, "Doom")
            .ActivateOnEnter<JailHands>()
            .ActivateOnEnter<JailGrasp>()
            .ActivateOnEnter<JailEnrage>()
            .ActivateOnEnter<JailSlow>()
            .ActivateOnEnter<JailInterrupt>()
            .ActivateOnEnter<JailBuster>();
    }

    private void P2(uint id)
    {
        EndsEmbraceP2(id, 5.7f);
        CropCircle(id + 0x10000, 6.6f);
        Circles1(id + 0x20000, 6.8f);
        MacabreMark(id + 0x30000, 7.7f);
        ColdGrip(id + 0x40000, 0.9f);
        CropCircle(id + 0x50000, 5.6f);
        FearOfDeath(id + 0x60000, 1.7f);
        Circles2(id + 0x70000, 1.3f);
        MementoMori(id + 0x80000, 2.3f);
        ColdGrip(id + 0x90000, 0, 4.2f);
        GrandCross(id + 0xA0000, 4.6f);

        Cast(id + 0xB0000, AID.DarknessOfEternityEnrage, 9.7f, 10, "Enrage");
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
        CastStart(id, AID.FearOfDeathRaidwide, delay)
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

    private void ColdGrip(uint id, float delay, float delay2 = 5.3f)
    {
        CastStartMulti(id, [AID.ColdGripLeftSafe, AID.ColdGripRightSafe], delay)
            .ActivateOnEnter<ColdGrip>()
            .ActivateOnEnter<ExistentialDread>();

        ComponentCondition<ColdGrip>(id + 0x10, delay2, c => c.NumCasts > 0, "Line AOEs")
            .DeactivateOnExit<ColdGrip>();

        ComponentCondition<ExistentialDread>(id + 0x20, 1.6f, e => e.NumCasts > 0, "Safe side").DeactivateOnExit<ExistentialDread>();
    }

    private void MementoMori(uint id, float delay)
    {
        CastStartMulti(id, [AID.MementoMoriDarkRight, AID.MementoMoriDarkLeft], delay)
            .ActivateOnEnter<MementoMoriLine>()
            .ActivateOnEnter<MementoMoriVoidzone>()
            .ActivateOnEnter<SmiteOfGloom>()
            .ActivateOnEnter<CenterChokingGrasp>();

        ComponentCondition<MementoMoriLine>(id + 0x10, 5, m => m.NumCasts > 0, "Line AOE")
            .DeactivateOnExit<MementoMoriLine>();

        ComponentCondition<CenterChokingGrasp>(id + 0x20, 6.2f, m => m.NumCasts > 0, "Hands")
            .DeactivateOnExit<CenterChokingGrasp>();

        ComponentCondition<SmiteOfGloom>(id + 0x30, 1, s => s.NumFinishedSpreads > 0, "Spreads")
            .DeactivateOnExit<SmiteOfGloom>();

        ComponentCondition<MementoMoriVoidzone>(id + 0x40, 4, m => !m.Active, "Voidzone disappear")
            .DeactivateOnExit<MementoMoriVoidzone>();
    }

    private void SoulReaping(uint id, float delay)
    {
        Cast(id, AID.SoulReaping, delay, 4, "Store AOE")
            .ActivateOnEnter<Aetherblight>()
            .ActivateOnEnter<Shockwave>();

        ComponentCondition<Aetherblight>(id + 0x10, 9.4f, a => a.NumCasts > 0);
        ComponentCondition<Shockwave>(id + 0x12, 0.1f, s => s.NumCasts > 0, "Stored AOE + stacks")
            .DeactivateOnExit<Shockwave>();

        Cast(id + 0x20, AID.SoulReaping, 1.9f, 4, "Store AOE");

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
        Cast(id, AID.GrandCrossRaidwide, delay, 7, "Raidwide")
            .ActivateOnEnter<GrandCrossArena>()
            .ActivateOnEnter<GrandCrossRaidwide>()
            .ActivateOnEnter<GrandCrossPuddle>()
            .ActivateOnEnter<GrandCrossSpread>()
            .ActivateOnEnter<GrandCrossLine>()
            .ActivateOnEnter<GrandCrossLineCast>()
            .ActivateOnEnter<Shock>();

        ComponentCondition<GrandCrossArena>(id + 0x10, 1.1f, t => t.NumChanges > 0, "Arena change");

        ComponentCondition<Shock>(id + 0x20, 13, s => s.NumCasts >= 4, "Towers 1")
            .ExecOnExit<Shock>(s => s.Reset());
        ComponentCondition<Shock>(id + 0x30, 11, s => s.NumCasts >= 8, "Towers 2")
            .DeactivateOnExit<GrandCrossLine>()
            .DeactivateOnExit<GrandCrossLineCast>()
            .DeactivateOnExit<GrandCrossSpread>()
            .DeactivateOnExit<Shock>();

        ComponentCondition<GrandCrossProximity>(id + 0x100, 8.7f, g => g.NumCasts > 0, "Proximity")
            .ActivateOnEnter<GrandCrossProximity>()
            .DeactivateOnExit<GrandCrossPuddle>()
            .DeactivateOnExit<GrandCrossProximity>();

        Cast(id + 0x200, AID.NeutronRingCast, 3.1f, 7)
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
        Cast(id, AID.SpecterOfDeath, delay, 5)
            .ActivateOnEnter<Invitation>()
            .ActivateOnEnter<LoomingSpecter>()
            .ActivateOnEnter<EndsEmbrace>()
            .ActivateOnEnter<EndsEmbraceBait>();

        ComponentCondition<Invitation>(id + 0x2, 7.2f, i => i.NumCasts > 0, "Safe rect")
            .DeactivateOnExit<Invitation>();

        ComponentCondition<EndsEmbrace>(id + 0x10, 1, e => e.NumFinishedSpreads > 0, "Spreads")
            .ActivateOnEnter<DelayedChokingGrasp>()
            .DeactivateOnExit<EndsEmbrace>()
            .DeactivateOnExit<LoomingSpecter>();

        CastStartMulti(id + 0x20, [AID.ColdGripLeftSafe, AID.ColdGripRightSafe], 2.1f)
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
        CastStart(id, AID.RelentlessReaping, delay)
            .ActivateOnEnter<CropCircle>()
            .ActivateOnEnter<Shockwave>()
            .ExecOnEnter<Shockwave>(s => s.Enabled = false);

        CastEnd(id + 1, 15);

        Cast(id + 0x10, AID.CropRotation, 3.1f, 3);

        CastStartMulti(id + 0x20, [AID.TheFourthSeason, AID.TheSecondSeason], 3.2f);

        ComponentCondition<CropCircle>(id + 0x30, 9.3f, c => c.NumCasts > 0, "AOE 1");
        ComponentCondition<CropCircle>(id + 0x31, 2.8f, c => c.NumCasts > 1, "AOE 2");
        ComponentCondition<CropCircle>(id + 0x32, 2.8f, c => c.NumCasts > 2, "AOE 3")
            .ExecOnEnter<Shockwave>(s => s.Enabled = true);
        ComponentCondition<Shockwave>(id + 0x40, 2.9f, s => s.NumCasts > 0, "AOE 4 + stacks")
            .DeactivateOnExit<CropCircle>()
            .DeactivateOnExit<Shockwave>();
    }

    private void Circles1(uint id, float delay)
    {
        CastStart(id, AID.CircleOfLivesCast, delay)
            .ActivateOnEnter<CircleOfLives>()
            .ActivateOnEnter<Invitation>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<Aetherblight>();

        ComponentCondition<CircleOfLives>(id + 0x10, 14, c => c.NumCasts > 0, "Circle 1");
        ComponentCondition<CircleOfLives>(id + 0x11, 5, c => c.NumCasts > 1, "Circle 2");
        ComponentCondition<CircleOfLives>(id + 0x12, 5, c => c.NumCasts > 2, "Circle 3");
        ComponentCondition<CircleOfLives>(id + 0x13, 5, c => c.NumCasts > 3, "Circle 4");
        ComponentCondition<CircleOfLives>(id + 0x14, 5, c => c.NumCasts > 4, "Circle 5")
            .DeactivateOnExit<CircleOfLives>()
            .DeactivateOnExit<Invitation>();

        ComponentCondition<Shockwave>(id + 0x20, 6.7f, s => s.NumCasts > 0, "AOE + stacks")
            .DeactivateOnExit<Shockwave>()
            .DeactivateOnExit<Aetherblight>();
    }

    private void Circles2(uint id, float delay)
    {
        CastStart(id, AID.CircleOfLivesCast, delay)
            .ActivateOnEnter<CircleOfLives>()
            .ActivateOnEnter<Invitation>();

        ComponentCondition<CircleOfLives>(id + 0x10, 14.1f, c => c.NumCasts > 0, "Circle 1");
        ComponentCondition<CircleOfLives>(id + 0x11, 5, c => c.NumCasts > 1, "Circle 2 + hands");
        ComponentCondition<CircleOfLives>(id + 0x12, 5, c => c.NumCasts > 2, "Circle 3");
        ComponentCondition<CircleOfLives>(id + 0x13, 5, c => c.NumCasts > 3, "Circle 4 + hands")
            .DeactivateOnExit<CircleOfLives>()
            .DeactivateOnExit<Invitation>();
    }

    private void MacabreMark(uint id, float delay)
    {
        CastStart(id, AID.MassMacabre, delay)
            .ActivateOnEnter<MacabreMark>()
            .ActivateOnEnter<MementoMoriLine>()
            .ActivateOnEnter<MementoMoriVoidzone>()
            .ActivateOnEnter<CenterChokingGrasp>()
            .ExecOnEnter<MementoMoriLine>(m => m.HighlightSafe = false);

        ComponentCondition<MementoMoriLine>(id + 0x10, 11.2f, m => m.NumCasts > 0, "Voidzone spawn")
            .DeactivateOnExit<MementoMoriLine>();

        CastStart(id + 0x20, AID.BlueShockwaveCast, 20)
            .ActivateOnEnter<BlueShockwave>();

        ComponentCondition<CenterChokingGrasp>(id + 0x30, 6.2f, m => m.NumCasts > 0, "Hand AOEs")
            .DeactivateOnExit<CenterChokingGrasp>();

        ComponentCondition<MementoMoriVoidzone>(id + 0x32, 4.9f, m => !m.Active, "Voidzone disappear")
            .DeactivateOnExit<MementoMoriVoidzone>();

        ComponentCondition<BlueShockwave>(id + 0x40, 0.6f, b => b.NumCasts > 0, "Tankbuster 1");
        ComponentCondition<BlueShockwave>(id + 0x41, 4.1f, b => b.NumCasts > 1, "Tankbuster 2")
            .DeactivateOnExit<BlueShockwave>();
    }
}
