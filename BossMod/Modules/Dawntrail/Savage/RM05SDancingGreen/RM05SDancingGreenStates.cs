namespace BossMod.Dawntrail.Savage.RM05SDancingGreen;

class DeepCut(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(60, 30.Degrees()), 471, AID.DeepCut);
class DiscoInferno(BossModule module) : Components.RaidwideCast(module, AID.DiscoInfernal);
class CelebrateGoodTimes(BossModule module) : Components.RaidwideCast(module, AID.CelebrateGoodTimes);

class LetsDance(BossModule module) : Components.GenericAOEs(module)
{
    public enum Dir { Left, Right, Up, Down }
    private readonly List<Dir> Frogs = [];

    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.LetsDance or AID.LetsDanceRemix)
        {
            var active1 = Module.CastFinishAt(spell, 1);
            _aoes.AddRange(Frogs.Select((f, i) => new AOEInstance(new AOEShapeRect(50, 50), Module.PrimaryActor.Position, f switch
            {
                Dir.Left => -90.Degrees(),
                Dir.Right => 90.Degrees(),
                Dir.Up => 180.Degrees(),
                _ => default
            }, active1.AddSeconds(i * 2.4f))));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FroggyLeft:
                Frogs.Add(Dir.Left);
                break;
            case AID.FroggyRight:
                Frogs.Add(Dir.Right);
                break;
            case AID.FroggyUp:
                Frogs.Add(Dir.Up);
                break;
            case AID.FroggyDown:
                Frogs.Add(Dir.Down);
                break;
            case AID.LetsDanceAOE:
            case AID.LetsDanceRemixAOE:
                NumCasts++;
                if (_aoes.Count > 0)
                    _aoes.RemoveAt(0);
                if (Frogs.Count > 0)
                    Frogs.RemoveAt(0);
                break;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Frogs.Count > 0)
            hints.Add($"Order: {string.Join(" -> ", Frogs.Select(s => s.ToString()))}");
    }
}

class RideTheWaves(BossModule module) : Components.GenericAOEs(module)
{
    private BitMask DisplayedTiles;
    private BitMask ActiveTiles;

    private DateTime NextActivation;

    public override void OnEventEnvControl(byte index, uint state)
    {
        switch (index, state)
        {
            case (4, 0x00800080):
                DisplayedTiles = ActiveTiles = new(0b11101101ul);
                NextActivation = WorldState.FutureTime(3.06f);
                break;
            case (4, 0x02000200):
                DisplayedTiles = ActiveTiles = new(0b10110111ul);
                NextActivation = WorldState.FutureTime(3.06f);
                break;

            case (5, 0x02000004):
                Advance(0b11101101);
                break;
            case (5, 0x04000004):
                Advance(0b11011011);
                break;
            case (5, 0x08000004):
                Advance(0b10110111);
                break;
            case (5, 0x20000004):
                Advance(0b11001001);
                break;
            case (5, 0x40000004):
                Advance(0b10010011);
                break;

            case (0x0C, _):
                if (NumCasts > 7)
                    DisplayedTiles <<= 8;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.RideTheWaves)
        {
            NumCasts++;
            ActiveTiles = DisplayedTiles;
            NextActivation = WorldState.FutureTime(2.1f);
        }
    }

    private void Advance(byte row1)
    {
        DisplayedTiles = new((DisplayedTiles.Raw | row1) << 8);
        if (NumCasts == 6)
            DisplayedTiles = new(DisplayedTiles.Raw | row1);
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (NextActivation == default)
            yield break;

        var row1Mask = new BitMask(NumCasts is > 0 and < 7 ? 0xFFu : 0);

        yield return new AOEInstance(new AOEShapeTiles(ActiveTiles | row1Mask), default, default, NextActivation);
    }
}

class QuarterBeats(BossModule module) : Components.StackWithCastTargets(module, AID.QuarterBeats, 4, minStackSize: 2, maxStackSize: 2);
class EighthBeats(BossModule module) : Components.SpreadFromCastTargets(module, AID.EighthBeats, 5);

class Moonwalk(BossModule module) : Components.GenericAOEs(module)
{
    public BitMask ActiveTiles;
    public DateTime NextActivation;

    public int Activations;

    private BitMask FillColumns(byte mask)
    {
        var bm = new BitMask();
        for (var i = 0; i < 8; i++)
        {
            bm <<= 8;
            bm |= new BitMask(mask);
        }
        return bm;
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        switch ((index, state))
        {
            case (0x1D, 0x00020001):
            case (0x1F, 0x00200010):
                Set(FillColumns(0b00000111));
                break;
            case (0x1D, 0x00200010):
            case (0x1F, 0x00020001):
                Set(FillColumns(0b00111000));
                break;
            case (0x1E, 0x00020001):
            case (0x20, 0x00200010):
                Set(FillColumns(0b00011100));
                break;
            case (0x1E, 0x00200010):
            case (0x20, 0x00020001):
                Set(FillColumns(0b11100000));
                break;
            case (0x21, 0x00020001):
            case (0x23, 0x00200010):
                Set(new(0x0000FFFFFF000000ul));
                break;
            case (0x21, 0x00200010):
            case (0x23, 0x00020001):
                Set(new(0x0000000000FFFFFFul));
                break;
            case (0x22, 0x00020001):
            case (0x24, 0x00200010):
                Set(new(0xFFFFFF0000000000ul));
                break;
            case (0x22, 0x00200010):
            case (0x24, 0x00020001):
                Set(new(0x000000FFFFFF0000ul));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Moonburn1 or AID.Moonburn2)
        {
            NumCasts++;
            ActiveTiles.Reset();
            NextActivation = default;
        }
    }

    private void Set(BitMask mask)
    {
        ActiveTiles |= mask;
        Activations++;
        if (NextActivation == default)
            NextActivation = WorldState.FutureTime(10.5f);
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!ActiveTiles.Any())
            yield break;

        yield return new(new AOEShapeTiles(ActiveTiles), default, default, NextActivation);
    }
}

class BackupDance(BossModule module) : Components.GenericBaitAway(module)
{
    public readonly List<Actor> Casters = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BackUpDanceVisual)
            Casters.Add(caster);
    }

    public override void Update()
    {
        CurrentBaits.Clear();
        foreach (var c in Casters)
        {
            if (Raid.WithoutSlot().Closest(c.Position) is { } baiter)
                CurrentBaits.Add(new(c, baiter, new AOEShapeCone(60, 22.5f.Degrees()), Module.CastFinishAt(c.CastInfo, 0.5f)));
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.FrogtourageFan && Raid.TryFindSlot(actor.InstanceID, out var slot))
            ForbiddenPlayers.Set(slot);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        Arena.Actors(Casters, ArenaColor.Object, true);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.BackUpDance)
        {
            NumCasts++;
            if (Casters.Count > 0)
                Casters.RemoveAt(0);
        }
    }
}

class DoTheHustle(BossModule module) : Components.GroupedAOEs(module, [AID.DoTheHustleFrogs1, AID.DoTheHustleFrogs2], new AOEShapeCone(50, 90.Degrees()), maxCasts: 2);
class DoTheHustleBoss(BossModule module) : Components.GroupedAOEs(module, [AID.DoTheHustleBoss1, AID.DoTheHustleBoss2], new AOEShapeCone(50, 90.Degrees()));

class RM05SDancingGreenStates : StateMachineBuilder
{
    public RM05SDancingGreenStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<ABSide>()
            .ActivateOnEnter<CelebrateGoodTimes>()
            .ActivateOnEnter<DiscoInferno>();
    }

    private void SinglePhase(uint id)
    {
        DeepCut(id, 9.2f);

        id += 0x10000;
        SideSelect(id, 7.7f);
        TwistNDrop(id + 0x10, 2.25f);

        id += 0x10000;
        SideSelect(id, 2.9f);
        TwistNDrop(id + 0x10, 2.2f);
        Cast(id + 0x20, AID.CelebrateGoodTimes, 0.9f, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);

        id += 0x10000;
        DiscoPhase(id, 8.4f);

        id += 0x10000;
        ArcadyNightFever(id, 8);

        id += 0x10000;
        RideTheWaves(id, 14.2f);

        id += 0x10000;
        Cast(id, AID.Frogtourage, 8.3f, 3)
            .ActivateOnEnter<Moonwalk>()
            .ActivateOnEnter<QuarterBeats>()
            .ActivateOnEnter<EighthBeats>();

        // sanity check that we didn't miss any patterns
        ComponentCondition<Moonwalk>(id + 2, 4.2f, m => m.Activations == 4);

        CastMulti(id + 0x10, [AID.QuarterBeatsBoss, AID.EighthBeatsBoss], 5.1f, 5, "Stack/spread")
            .DeactivateOnExit<QuarterBeats>()
            .DeactivateOnExit<EighthBeats>();
        ComponentCondition<Moonwalk>(id + 0x12, 0.5f, m => m.NumCasts > 0, "Tiles")
            .DeactivateOnExit<Moonwalk>();

        Cast(id + 0x20, AID.DiscoInfernal, 3.7f, 4, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide)
            .ActivateOnEnter<BackupDance>();

        id += 0x10000;
        ComponentCondition<BackupDance>(id, 2.3f, b => b.Casters.Count > 0)
            .ActivateOnEnter<BurnBabyBurn2>();
        ComponentCondition<BackupDance>(id + 2, 9.5f, b => b.NumCasts > 0, "Baits 1");

        SideSelect(id + 0x10, 5.7f);
        ComponentCondition<BackupDance>(id + 0x12, 0.6f, b => b.NumCasts > 4, "Baits 2");

        TwistNDrop(id + 0x100, 1.6f);
        Cast(id + 0x200, AID.CelebrateGoodTimes, 1, 5, "Raidwide")
            .DeactivateOnExit<BurnBabyBurn2>()
            .DeactivateOnExit<BackupDance>()
            .SetHint(StateMachine.StateHint.Raidwide);

        id += 0x10000;
        Cast(id, AID.EnsembleAssemble, 8.4f, 3)
            .ActivateOnEnter<LetsDance>()
            .ActivateOnEnter<GetDownAOE>()
            .ActivateOnEnter<GetDownProtean>()
            .ActivateOnEnter<GetDownRepeat>();
        Cast(id + 0x10, AID.ArcadyNightEncore, 3.2f, 4.8f);
        ComponentCondition<GetDownAOE>(id + 0x12, 0.4f, g => g.NumCasts > 0, "Proteans start");
        ComponentCondition<GetDownRepeat>(id + 0x14, 20.1f, g => g.NumCasts >= 8, "Proteans end")
            .DeactivateOnExit<GetDownAOE>()
            .DeactivateOnExit<GetDownProtean>()
            .DeactivateOnExit<GetDownRepeat>();

        CastStart(id + 0x100, AID.LetsDanceRemix, 1.5f);
        CastEnd(id + 0x101, 5.8f);
        ComponentCondition<LetsDance>(id + 0x102, 1, f => f.NumCasts > 0, "Cleave 1");
        ComponentCondition<LetsDance>(id + 0x110, 10.4f, f => f.NumCasts >= 8, "Cleave 8");

        Cast(id + 0x200, AID.LetsPose2, 2.3f, 5, "Resolve")
            .DeactivateOnExit<LetsDance>();

        id += 0x10000;
        Cast(id, AID.Frogtourage, 16.5f, 3)
            .ActivateOnEnter<DoTheHustle>()
            .ActivateOnEnter<DoTheHustleBoss>()
            .ActivateOnEnter<Moonwalk>()
            .ActivateOnEnter<BackupDance>();

        ComponentCondition<DoTheHustle>(id + 2, 7.2f, d => d.NumCasts >= 2, "Cleaves 1");
        ComponentCondition<DoTheHustle>(id + 4, 4.1f, d => d.NumCasts >= 4, "Cleaves 2");
        ComponentCondition<DoTheHustleBoss>(id + 6, 4, d => d.NumCasts > 0, "Cleaves 3");

        ComponentCondition<Moonwalk>(id + 0x10, 6.2f, m => m.Activations >= 2);

        ComponentCondition<Moonwalk>(id + 0x20, 10.5f, m => m.NumCasts >= 2, "Floor");
        ComponentCondition<BackupDance>(id + 0x21, 0.2f, b => b.NumCasts >= 4, "Baits 1");

        ComponentCondition<Moonwalk>(id + 0x30, 15.8f, m => m.NumCasts >= 4, "Floor");
        ComponentCondition<BackupDance>(id + 0x31, 0.1f, b => b.NumCasts >= 8, "Baits 2");

        CastMulti(id + 0x40, [AID.DoTheHustleBoss1, AID.DoTheHustleBoss2], 4.4f, 5);
        ComponentCondition<DoTheHustle>(id + 0x42, 0.1f, d => d.Casters.Count == 0, "Safe dorito")
            .DeactivateOnExit<DoTheHustle>()
            .DeactivateOnExit<DoTheHustleBoss>()
            .DeactivateOnExit<Moonwalk>()
            .DeactivateOnExit<BackupDance>();

        DeepCut(id + 0x50, 2);

        id += 0x10000;
        CastStart(id, AID.FunkyFloor, 7.8f)
            .ActivateOnEnter<FunkyFloor>()
            .ActivateOnEnter<QuarterBeats>()
            .ActivateOnEnter<EighthBeats>()
            .ExecOnEnter<FunkyFloor>(f => f.MaxCasts = 8);
        CastEnd(id + 1, 2.5f);

        CastMulti(id + 0x10, [AID.QuarterBeatsBoss, AID.EighthBeatsBoss], 3.2f, 5, "Stack/spread");

        InsideOutside(id + 0x20, 3.1f);

        CastMulti(id + 0x30, [AID.QuarterBeatsBoss, AID.EighthBeatsBoss], 4.6f, 5, "Stack/spread");

        Cast(id + 0x40, AID.CelebrateGoodTimes, 2.2f, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);

        Cast(id + 0x50, AID.CelebrateGoodTimes, 5.2f, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);

        Cast(id + 0x60, AID.FrogtourageFinale, 17.3f, 3);

        Cast(id + 0x70, AID.HiNRGFever, 3.3f, 12, "Enrage");
    }

    private void DeepCut(uint id, float delay)
    {
        CastStart(id, AID.DeepCutVisual, delay)
            .ActivateOnEnter<DeepCut>();
        CastEnd(id + 2, 5);

        ComponentCondition<DeepCut>(id + 4, 0.7f, d => d.NumCasts >= 2, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .DeactivateOnExit<DeepCut>();
    }

    private State SideSelect(uint id, float delay)
    {
        return CastMulti(id, [AID.FlipToASide, AID.FlipToBSide], delay, 4);
    }

    private State TwistNDrop(uint id, float delay)
    {
        CastStartMulti(id, Savage.RM05SDancingGreen.TwistNDrop.BossCasts, delay)
            .ActivateOnEnter<TwistNDrop>()
            .ActivateOnEnter<PlayASide>()
            .ActivateOnEnter<PlayBSide>()
            .ActivateOnEnter<PlaySideCounter>();

        CastEnd(id + 0x02, 5);

        ComponentCondition<TwistNDrop>(id + 0x20, 3.5f, t => t.Side2, "Left/right");
        return ComponentCondition<PlaySideCounter>(id + 0x22, 1.8f, p => p.NumCasts >= 2, "Stack/spread")
            .DeactivateOnExit<PlaySideCounter>()
            .DeactivateOnExit<PlayASide>()
            .DeactivateOnExit<PlayBSide>()
            .DeactivateOnExit<TwistNDrop>();
    }

    private void DiscoPhase(uint id, float delay)
    {
        Cast(id, AID.DiscoInfernal, delay, 4, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide)
            .ActivateOnEnter<FloorCounter>()
            .ActivateOnEnter<FunkyFloor>()
            .ActivateOnEnter<BurnBabyBurn>();

        id += 0x100;

        CastStart(id, AID.FunkyFloor, 2.2f);
        ComponentCondition<FloorCounter>(id + 0x10, 3.1f, c => c.NumCasts > 0, "Floor start");

        InsideOutside(id + 0x20, 3.1f);

        ComponentCondition<BurnBabyBurn>(id + 0x30, 8.3f, b => b.NumShort == 0, "Debuffs 1");
        ComponentCondition<BurnBabyBurn>(id + 0x40, 7.9f, b => b.NumLong == 0, "Debuffs 2");

        TwistNDrop(id + 0x100, 2.8f);
        Cast(id + 0x200, AID.CelebrateGoodTimes, 1, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<BurnBabyBurn>()
            .DeactivateOnExit<FunkyFloor>()
            .DeactivateOnExit<FloorCounter>();

        DeepCut(id + 0x300, 2.1f);
    }

    private void InsideOutside(uint id, float delay)
    {
        CastStartMulti(id, [AID.InsideOutVisual, AID.OutsideInVisual], delay)
            .ActivateOnEnter<InsideOutside>();
        CastEnd(id + 1, 5, "In/out");
        ComponentCondition<InsideOutside>(id + 2, 2.5f, i => i.NumCasts >= 2, "Out/in")
            .DeactivateOnExit<InsideOutside>();
    }

    private void ArcadyNightFever(uint id, float delay)
    {
        Cast(id, AID.EnsembleAssemble, delay, 3)
            .ActivateOnEnter<GetDownAOE>()
            .ActivateOnEnter<GetDownProtean>()
            .ActivateOnEnter<GetDownRepeat>()
            .ActivateOnEnter<LetsDance>();
        Cast(id + 0x10, AID.ArcadyNightFever, 3.1f, 4.8f);
        ComponentCondition<GetDownAOE>(id + 0x12, 0.4f, g => g.NumCasts > 0, "Proteans start");
        ComponentCondition<GetDownRepeat>(id + 0x14, 20.1f, g => g.NumCasts >= 8, "Proteans end")
            .DeactivateOnExit<GetDownAOE>()
            .DeactivateOnExit<GetDownProtean>()
            .DeactivateOnExit<GetDownRepeat>();

        CastStart(id + 0x100, AID.LetsDance, 1.5f)
            .ActivateOnEnter<Wavelength>();
        CastEnd(id + 0x101, 5.8f);
        ComponentCondition<LetsDance>(id + 0x102, 1, f => f.NumCasts > 0, "Cleave 1");
        ComponentCondition<LetsDance>(id + 0x110, 17.15f, f => f.NumCasts >= 8, "Cleave 8");

        Cast(id + 0x200, AID.LetsPose1, 3.2f, 5, "Resolve")
            .DeactivateOnExit<LetsDance>()
            .DeactivateOnExit<Wavelength>();
    }

    private void RideTheWaves(uint id, float delay)
    {
        SideSelect(id, delay);
        Cast(id + 0x10, AID.RideTheWavesVisual, 2.1f, 3.5f)
            .ActivateOnEnter<RideTheWaves>()
            .ActivateOnEnter<QuarterBeats>()
            .ActivateOnEnter<EighthBeats>();

        ComponentCondition<RideTheWaves>(id + 0x12, 4, r => r.NumCasts > 0, "Tiles start");

        CastMulti(id + 0x20, [AID.QuarterBeatsBoss, AID.EighthBeatsBoss], 1.2f, 5, "Stack/spread 1");
        CastMulti(id + 0x30, [AID.QuarterBeatsBoss, AID.EighthBeatsBoss], 3.2f, 5, "Stack/spread 2");

        InsideOutside(id + 0x40, 5.45f);
        TwistNDrop(id + 0x50, 2.5f)
            .DeactivateOnExit<RideTheWaves>()
            .DeactivateOnExit<QuarterBeats>()
            .DeactivateOnExit<EighthBeats>();

        DeepCut(id + 0x60, 2);
        Cast(id + 0x180, AID.CelebrateGoodTimes, 1.5f, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    //private void XXX(uint id, float delay)
}
