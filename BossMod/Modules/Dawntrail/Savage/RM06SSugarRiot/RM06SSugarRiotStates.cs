namespace BossMod.Dawntrail.Savage.RM06SSugarRiot;

class MousseMural(BossModule module) : Components.RaidwideCast(module, AID.MousseMural);

class SweetShot(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<(Actor Caster, DateTime Activation)> Casters = [];

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((OID)source.OID == OID.SweetShot)
        {
            // end of adds phase
            if ((TetherID)tether.ID == TetherID.PinkTether)
                Casters.Add((source, WorldState.FutureTime(6.3f)));

            // start of river phase
            if ((TetherID)tether.ID == TetherID.BlueTether)
                Casters.Add((source, WorldState.FutureTime(7.2f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Rush)
        {
            NumCasts++;
            Casters.RemoveAll(c => c.Caster == caster);
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Select(c => new AOEInstance(new AOEShapeRect(100, 3.5f), c.Caster.Position, c.Caster.Rotation, c.Activation));
}

class PuddingParty(BossModule module) : Components.StackWithIcon(module, (uint)IconID.PuddingParty, AID.PuddingParty, 6, 5.1f)
{
    public int NumCasts;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == StackAction && ++NumCasts >= 5)
            Stacks.Clear();
    }
}

class RushEnrage(BossModule module) : Components.CastCounter(module, AID.RushEnrage);

class RM06SSugarRiotStates : StateMachineBuilder
{
    public RM06SSugarRiotStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<MousseMural>();
    }

    private void SinglePhase(uint id)
    {
        Cast(id, AID.MousseMural, 6.2f, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);

        ColorRiot(id + 0x10, 6.2f);
        Wingmark1(id + 0x20, 6.6f);
        StickyMousse(id + 0x100, 2.3f);
        ColorRiot(id + 0x200, 0.4f);

        id += 0x10000;
        Cast(id, AID.SugarscapeDesert, 6.45f, 1, "Desert start")
            .ActivateOnEnter<SprayPain>()
            .ActivateOnEnter<HeatingUpHints>();
        Cast(id + 0x10, AID.LayerDesert1, 15.3f, 1);

        Cactus1(id + 0x20, 7.2f);
        StickyMousse(id + 0x100, 2.7f);

        id += 0x10000;
        Cactus2(id, 0.5f);

        id += 0x10000;
        PuddingGraf(id, 3.55f);
        ColorRiot(id + 0x10, 2.8f);

        id += 0x10000;
        AddsPhase(id, 5.2f);

        id += 0x10000;
        RiverPhase(id, 8.5f);

        id += 0x10000;
        StormPhase(id, 8.15f);

        id += 0x10000;
        Cast(id, AID.LayerLava, 8.1f, 1);
        LavaTowers1(id + 0x10, 11.2f);

        id += 0x10000;
        LavaTowers2(id, 7.3f);

        id += 0x10000;
        StickyMousse(id, 5.15f);
        ColorRiot(id + 0x10, 0.4f);
        Wingmark1(id + 0x20, 6.6f);

        id += 0x10000;
        Cast(id, AID.ArtisticAnarchy, 9, 8);
        ComponentCondition<RushEnrage>(id + 2, 6.1f, r => r.NumCasts > 0, "Enrage")
            .ActivateOnEnter<RushEnrage>();
    }

    private State ColorRiot(uint id, float delay)
    {
        CastStartMulti(id, [AID.ColorRiotFireClose, AID.ColorRiotIceClose], delay)
            .ActivateOnEnter<ColorRiot>();
        return ComponentCondition<ColorRiot>(id + 1, 7, tb => tb.NumCasts > 0, "Tankbuster")
            .DeactivateOnExit<ColorRiot>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void StickyMousse(uint id, float delay)
    {
        CastStart(id, AID.StickyMousseVisual, delay)
            .ActivateOnEnter<StickyMousse>()
            .ActivateOnEnter<StickyBurst>();
        CastEnd(id + 1, 5);
        ComponentCondition<StickyMousse>(id + 2, 0.8f, m => m.NumCasts > 0, "Spreads");
        ComponentCondition<StickyBurst>(id + 3, 6, b => b.NumCasts > 0, "Stacks")
            .DeactivateOnExit<StickyMousse>()
            .DeactivateOnExit<StickyBurst>();
    }

    private void Wingmark1(uint id, float delay)
    {
        Cast(id, AID.WingmarkBoss, delay, 4)
            .ActivateOnEnter<WingmarkKB>()
            .ActivateOnEnter<WingmarkAdds>()
            .ActivateOnEnter<ColorClash>();

        CastMulti(id + 2, [AID.ColorClashParty, AID.ColorClashPairs], 3.1f, 3);

        CastStartMulti(id + 4, [AID.DoubleStyleAdds1, AID.DoubleStyleAdds2, AID.DoubleStyleAdds3, AID.DoubleStyleAdds4, AID.DoubleStyleAdds5, AID.DoubleStyleAdds6, AID.DoubleStyleAdds7, AID.DoubleStyleAdds8], 4.4f);

        ComponentCondition<WingmarkAdds>(id + 6, 3, w => w.Adds.Count > 0, "Summons appear")
            .ExecOnExit<WingmarkKB>(w => w.Risky = true);

        ComponentCondition<WingmarkKB>(id + 0x10, 8.44f, w => w.StunHappened, "Stun")
            .SetHint(StateMachine.StateHint.DowntimeStart)
            .ExecOnExit<WingmarkAdds>(w => w.Risky = true)
            .ExecOnExit<ColorClash>(c => c.Activate());

        ComponentCondition<WingmarkKB>(id + 0x11, 3, w => w.KnockbackFinished)
            .SetHint(StateMachine.StateHint.DowntimeEnd);

        ComponentCondition<WingmarkAdds>(id + 0x12, 1.6f, w => w.NumCasts > 0, "AOEs");
        ComponentCondition<ColorClash>(id + 0x13, 0.5f, w => w.NumCasts > 0, "Stacks")
            .DeactivateOnExit<WingmarkKB>()
            .DeactivateOnExit<WingmarkAdds>()
            .DeactivateOnExit<ColorClash>();
    }

    private void Cactus1(uint id, float delay)
    {
        ComponentCondition<SprayPain>(id, delay, s => s.Casters.Count > 0, "Cacti appear");

        ComponentCondition<SprayPain>(id + 2, 6.9f, s => s.NumCasts > 0, "Cactus AOEs 1");
        ComponentCondition<SprayPain>(id + 3, 3.1f, s => s.NumCasts > 5);
        ComponentCondition<SprayPain>(id + 4, 3, s => s.NumCasts > 10);
        ComponentCondition<SprayPain>(id + 5, 3, s => s.NumCasts > 15);
        ComponentCondition<SprayPain>(id + 6, 3, s => s.NumCasts > 20)
            .ActivateOnEnter<HeatingUp>()
            .ExecOnEnter<HeatingUpHints>(h => h.Prune());
        ComponentCondition<SprayPain>(id + 0x10, 3.1f, s => s.NumCasts > 25, "Cactus AOEs 6")
            .ExecOnExit<HeatingUp>(h => h.EnableAIHints = true);

        ComponentCondition<HeatingUp>(id + 0x20, 4.5f, b => b.NumCasts > 0, "Defams + stack")
            .DeactivateOnExit<HeatingUp>()
            .DeactivateOnExit<SprayPain>();
    }

    private void Cactus2(uint id, float delay)
    {
        Cast(id, AID.LayerDesert2, delay, 1)
            .ActivateOnEnter<Quicksand>()
            .ActivateOnEnter<SprayPain2>()
            .ActivateOnEnter<PuddingGraf>()
            .ActivateOnEnter<PuddingGrafAim>();

        ComponentCondition<Quicksand>(id + 2, 7.1f, q => q.AppearCount > 0, "Quicksand appear")
            .DeactivateOnExit<HeatingUpHints>();

        ComponentCondition<HeatingUp>(id + 0x10, 8, b => b.NumCasts > 0, "Defams 2")
            .ActivateOnEnter<HeatingUp>()
            .DeactivateOnExit<HeatingUp>();
        ComponentCondition<SprayPain2>(id + 0x11, 0.6f, s => s.NumCasts > 0, "Safe corner");
        ComponentCondition<Quicksand>(id + 0x12, 0.46f, q => q.ActivationCount > 0, "Quicksand activate")
            .DeactivateOnExit<SprayPain2>();

        Cast(id + 0x20, AID.PuddingGrafVisual, 0.12f, 3);

        ComponentCondition<Quicksand>(id + 0x30, 0.9f, q => q.DisappearCount > 0, "Quicksand disappear");
    }

    private void PuddingGraf(uint id, float delay)
    {
        CastStart(id, AID.DoubleStyleBombs, delay);

        ComponentCondition<Quicksand>(id + 2, 1.5f, q => q.AppearCount > 1, "Quicksand appear");

        ComponentCondition<PuddingGraf>(id + 3, 7.4f, p => p.NumFinishedSpreads > 0, "Place bombs");

        ComponentCondition<Quicksand>(id + 4, 2.5f, q => q.ActivationCount > 1, "Quicksand activate");

        Cast(id + 8, AID.MousseMural, 1.6f, 5, "Raidwide");
        ComponentCondition<Quicksand>(id + 0xA, 0.3f, q => q.DisappearCount > 1, "Quicksand disappear")
            .DeactivateOnExit<PuddingGraf>()
            .DeactivateOnExit<PuddingGrafAim>()
            .DeactivateOnExit<Quicksand>();
    }

    private void AddsPhase(uint id, float delay)
    {
        Cast(id, AID.SoulSugar, delay, 3);
        Cast(id + 0x10, AID.LivePainting1, 3.25f, 4)
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<ReadyOreNot>()
            .ActivateOnEnter<ICraveViolence>()
            .ActivateOnEnter<HangryHiss>()
            .ActivateOnEnter<OreRigato>()
            .ActivateOnEnter<WaterIII>()
            .ActivateOnEnter<WaterIIITether>();
        ComponentCondition<Adds>(id + 0x12, 3.1f, c => c.YanCounter > 0, "Adds 1");

        Cast(id + 0x20, AID.LivePainting2, 22.1f, 4);
        ComponentCondition<Adds>(id + 0x22, 3.1f, c => c.RayCounter > 0, "Adds 2");

        Cast(id + 0x30, AID.LivePainting3, 15, 4);
        ComponentCondition<Adds>(id + 0x32, 3.1f, c => c.YanCounter > 1, "Adds 3");

        ComponentCondition<Adds>(id + 0x34, 8.1f, c => c.JabberwockCounter > 0, "Jabberwock 1");

        Cast(id + 0x40, AID.ReadyOreNot, 11.9f, 7, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        Cast(id + 0x50, AID.LivePainting4, 5.2f, 4);
        ComponentCondition<Adds>(id + 0x52, 3.1f, c => c.RayCounter > 2, "Adds 4");
        ComponentCondition<Adds>(id + 0x54, 8.1f, c => c.JabberwockCounter > 1, "Jabberwock 2")
            .ActivateOnEnter<SweetShot>();

        Cast(id + 0x60, AID.ReadyOreNot, 56, 7, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);

        CastStart(id + 0x70, AID.SingleStyle, 3.3f);
        ComponentCondition<SweetShot>(id + 0x72, 10.1f, s => s.NumCasts > 0, "Arrows")
            .DeactivateOnExit<SweetShot>();

        ColorRiot(id + 0x80, 4.1f)
            .DeactivateOnExit<WaterIII>()
            .DeactivateOnExit<ReadyOreNot>()
            .DeactivateOnExit<Adds>()
            .DeactivateOnExit<ICraveViolence>()
            .DeactivateOnExit<HangryHiss>()
            .DeactivateOnExit<OreRigato>()
            .DeactivateOnExit<WaterIIITether>();

        Cast(id + 0x90, AID.MousseMural, 1.1f, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void RiverPhase(uint id, float delay)
    {
        Cast(id, AID.SugarscapeRiver, delay, 1, "River start")
            .ActivateOnEnter<SweetShot>();
        CastStartMulti(id + 0x10, [AID.DoubleStyleFire, AID.DoubleStyleLightning], 14.1f)
            .ActivateOnEnter<DoubleStyleFireLightning>()
            .ActivateOnEnter<RiverPhaseArena>();
        ComponentCondition<SweetShot>(id + 0x12, 10.1f, s => s.NumCasts > 0, "Arrows");
        ComponentCondition<DoubleStyleFireLightning>(id + 0x14, 0.1f, d => d.NumCasts > 0, "Spread/stack")
            .DeactivateOnExit<DoubleStyleFireLightning>()
            .DeactivateOnExit<RiverPhaseArena>()
            .DeactivateOnExit<SweetShot>();
    }

    private void StormPhase(uint id, float delay)
    {
        ComponentCondition<StormPhaseArena>(id, delay, s => s.CurArena == StormPhaseArena.ArenaType.Storm, "Thunderstorm start")
            .ActivateOnEnter<StormPhaseArena>()
            .ActivateOnEnter<LightningFlash>()
            .ActivateOnEnter<TasteOfLightningBait>()
            .ActivateOnEnter<TasteOfLightningDelayed>()
            .ActivateOnEnter<TempestPiece>()
            .ActivateOnEnter<Highlightning>()
            .ActivateOnEnter<LightningBolt>()
            .ActivateOnEnter<LightningStorm>()
            .ActivateOnEnter<PuddingParty>()
            .ActivateOnEnter<TasteOfThunder>()
            .ActivateOnEnter<MousseDrip>()
            .ActivateOnEnter<MousseDripStack>();

        ComponentCondition<LightningFlash>(id + 0x10, 3.2f, f => f.NumCasts > 0, "Bait AOEs")
            .DeactivateOnExit<LightningFlash>();

        ComponentCondition<Highlightning>(id + 0x20, 5, h => h.NumCasts > 0, "Thundercloud 1")
            .DeactivateOnExit<TasteOfLightningBait>();
        ComponentCondition<Highlightning>(id + 0x21, 10.5f, h => h.NumCasts > 1, "Thundercloud 2");
        ComponentCondition<Highlightning>(id + 0x22, 10.6f, h => h.NumCasts > 2, "Thundercloud 3");
        ComponentCondition<Highlightning>(id + 0x23, 10.6f, h => h.NumCasts > 3, "Thundercloud 4");
        ComponentCondition<Highlightning>(id + 0x24, 10.6f, h => h.NumCasts > 4, "Thundercloud 5")
            .DeactivateOnExit<Highlightning>();

        ComponentCondition<PuddingParty>(id + 0x30, 5.8f, p => p.NumCasts > 0, "Stack 1");
        ComponentCondition<PuddingParty>(id + 0x32, 4.25f, p => p.NumCasts >= 5, "Stack 5");
    }

    private void LavaTowers1(uint id, float delay)
    {
        CastStart(id + 0x10, AID.MousseDripVisual, delay)
            .ActivateOnEnter<Moussacre>();
        ComponentCondition<MousseDripStack>(id + 0x12, 10.1f, m => m.NumCasts > 0, "Stacks 1");

        Cast(id + 0x20, AID.MoussacreVisual, 1, 4);
        ComponentCondition<Moussacre>(id + 0x22, 0.9f, m => m.NumCasts > 0, "Proteans");

        ComponentCondition<MousseDripStack>(id + 0x30, 0.2f, m => !m.ActiveStacks.Any(), "Stacks 4");

        ComponentCondition<LightningFlash>(id + 0x31, 3, f => f.NumCasts > 0, "Bait AOEs")
            .ActivateOnEnter<LightningFlash>()
            .ActivateOnEnter<TasteOfLightningBait>()
            .DeactivateOnExit<LightningFlash>()
            .ExecOnExit<TasteOfThunder>(t => t.Risky = true);

        ComponentCondition<TasteOfThunder>(id + 0x32, 4.9f, t => t.Towers.Count == 0, "Towers")
            .DeactivateOnExit<TasteOfThunder>()
            .DeactivateOnExit<TasteOfLightningBait>();
    }

    private void LavaTowers2(uint id, float delay)
    {
        ComponentCondition<TasteOfThunder>(id, delay, t => t.Towers.Count > 0, "Towers 2 appear")
            .ActivateOnEnter<TasteOfThunder>()
            .ActivateOnEnter<WingmarkKB>();

        ComponentCondition<WingmarkKB>(id + 2, 7.9f, w => w.StunHappened, "Stun")
            .SetHint(StateMachine.StateHint.DowntimeStart);

        ComponentCondition<WingmarkKB>(id + 3, 3, w => w.KnockbackFinished)
            .ActivateOnEnter<TasteOfLightningBait>()
            .DeactivateOnExit<WingmarkKB>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);

        ComponentCondition<LightningFlash>(id + 4, 3.2f, f => f.NumCasts > 0, "Bait AOEs")
            .ActivateOnEnter<LightningFlash>()
            .DeactivateOnExit<LightningFlash>()
            .ExecOnExit<TasteOfThunder>(t => t.Risky = true);

        ComponentCondition<TasteOfThunder>(id + 0x10, 5, t => t.Towers.Count == 0, "Towers 2")
            .DeactivateOnExit<TasteOfThunder>()
            .DeactivateOnExit<TasteOfLightningBait>();

        Cast(id + 0x20, AID.MousseMural, 0, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }
}
