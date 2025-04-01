namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

class P1Explosion(BossModule module) : Components.CastTowers(module, ActionID.MakeSpell(AID._Spell_Explosion), 3);

class SpecterOfTheLost(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCone(48, 30.Degrees()), (uint)TetherID._Gen_Tether_89, ActionID.MakeSpell(AID._Ability_SpecterOfTheLost));
class SpecterOfTheLostAOE(BossModule module) : Components.StandardAOEs(module, ActionID.MakeSpell(AID._Ability_SpecterOfTheLost), new AOEShapeCone(48, 30.Degrees()));

class P2Explosion(BossModule module) : Components.CastTowers(module, ActionID.MakeSpell(AID._Spell_Explosion1), 3, minSoakers: 3, maxSoakers: 4)
{
    private BitMask TetheredPlayers;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.AddsTether && Raid.FindSlot(tether.Target) is var slot && slot >= 0)
            TetheredPlayers.Set(slot);
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.AddsTether && Raid.FindSlot(tether.Target) is var slot && slot >= 0)
            TetheredPlayers.Clear(slot);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action == WatchedAction)
            for (var i = 0; i < Towers.Count; i++)
                Towers.Ref(i).ForbiddenSoakers |= TetheredPlayers;
    }
}

class StockBreak(BossModule module) : Components.UniformStackSpread(module, 6, 0)
{
    public int NumCasts;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.StockBreak)
            AddStack(actor, WorldState.FutureTime(8.3f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Ability_StockBreak1:
            case AID._Ability_StockBreak2:
                NumCasts++;
                break;
            case AID._Ability_StockBreak3:
                NumCasts++;
                Stacks.Clear();
                break;
        }
    }
}

class CumMeter(BossModule module) : BossComponent(module)
{
    public uint Progress { get; private set; }
    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        if (updateID == 0x8000000C && param1 == 0x00000056)
            Progress = param2;
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Progress > 0)
            hints.Add($"Cum power: {Progress / 100f:f2}%");
    }
}

class RosebloodDrop(BossModule module) : Components.Adds(module, (uint)OID.RosebloodDrop1)
{
    public bool Spawned { get; private set; }

    public override void Update()
    {
        Spawned |= ActiveActors.Any();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.PrioritizeTargetsByOID(OID.RosebloodDrop1, 1);

        if (actor.Role is Role.Healer or Role.Ranged && ActiveActors.MaxBy(a => a.HPMP.CurHP) is { } target)
            hints.SetPriority(target, 2);
    }
}

class PerfumedQuietus(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_PerfumedQuietus1));

class AlexandrianThunderII(BossModule module) : Components.GenericRotatingAOE(module)
{
    private Angle Rotation;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch ((IconID)iconID)
        {
            case IconID.ThunderCCW:
                Rotation = 10.Degrees();
                break;
            case IconID.ThunderCW:
                Rotation = -10.Degrees();
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Ability_AlexandrianThunderII && Rotation != default)
            Sequences.Add(new(new AOEShapeCone(24, 22.5f.Degrees()), caster.Position, caster.Rotation, Rotation, Module.CastFinishAt(spell), 1, 15));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Ability_AlexandrianThunderII or AID._Ability_AlexandrianThunderII1)
        {
            NumCasts++;
            AdvanceSequence(caster.Position, caster.Rotation, WorldState.CurrentTime);
        }
    }
}

class AlexandrianThunderIII(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.AlexandrianThunderIII, ActionID.MakeSpell(AID._Spell_AlexandrianThunderIII1), 4, 5)
{
    private TileTracker? Tiles;

    public override void Update()
    {
        Tiles ??= Module.FindComponent<TileTracker>();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Tiles != null && Spreads.Count > 0)
            hints.AddForbiddenZone(Tiles.ActiveTiles, Spreads[0].Activation);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Spreads.Count > 0 && Tiles != null && Tiles.InActiveTile(actor))
            hints.Add($"GTFO from rose tile!");
    }
}

class ZeleniaStates : StateMachineBuilder
{
    public ZeleniaStates(BossModule module) : base(module)
    {
        SimplePhase(0, Phase1, "P1")
            .ActivateOnEnter<TileTracker>()
            .ActivateOnEnter<ThornedCatharsis>()
            .Raw.Update = () => !Module.PrimaryActor.IsTargetable;

        SimplePhase(1, AddsPhase, "Adds")
            .SetHint(StateMachine.PhaseHint.StartWithDowntime)
            .Raw.Update = () =>
            {
                var adds = Module.FindComponent<RosebloodDrop>()!;
                if (adds.Spawned && !adds.ActiveActors.Any())
                {
                    // she likes to stick around until the adds are done
                    return Module.FindComponent<SpearpointBait>()?.ActiveBaits.Count() == 0 && Module.FindComponent<SpearpointAOE>()?.Casters.Count == 0;
                }
                return false;
            };

        DeathPhase(2, Phase2)
            .SetHint(StateMachine.PhaseHint.StartWithDowntime);
    }

    private void Phase1(uint id)
    {
        Raidwide(id, 6.1f);

        CastStart(id + 2, AID._Weaponskill_Shock, 7.6f)
            .ActivateOnEnter<P1Explosion>()
            .ActivateOnEnter<ShockDonutBait>()
            .ActivateOnEnter<ShockCircleBait>()
            .ActivateOnEnter<ShockAOEs>()
            .ActivateOnEnter<SpecterOfTheLost>()
            .ActivateOnEnter<SpecterOfTheLostAOE>();

        ComponentCondition<ShockAOEs>(id + 0x10, 11.8f, e => e.NumCasts > 0, "AOEs 1");
        ComponentCondition<P1Explosion>(id + 0x12, 1.3f, e => e.NumCasts > 0, "Towers");
        ComponentCondition<ShockAOEs>(id + 0x14, 4.2f, e => e.NumCasts >= 96, "AOEs 11")
            .DeactivateOnExit<P1Explosion>()
            .DeactivateOnExit<ShockDonutBait>()
            .DeactivateOnExit<ShockCircleBait>()
            .DeactivateOnExit<ShockAOEs>();

        ComponentCondition<SpecterOfTheLostAOE>(id + 0x20, 8.6f, e => e.NumCasts >= 2, "Tankbusters")
            .DeactivateOnExit<SpecterOfTheLostAOE>();

        EscelonsFall(id + 0x10000, 6.7f, "EF1");
        StockBreak(id + 0x20000, 2.2f);

        CastStart(id + 0x30000, AID._Weaponskill_BlessedBarricade, 3);

        Targetable(id + 0x30002, false, 3.8f, "Boss disappears");
    }

    private void AddsPhase(uint id)
    {
        ComponentCondition<RosebloodDrop>(id, 0.29f, d => d.ActiveActors.Any(), "Adds spawn")
            .ActivateOnEnter<RosebloodDrop>()
            .ActivateOnEnter<P2Explosion>()
            .ActivateOnEnter<SpearpointAOE>()
            .ActivateOnEnter<SpearpointBait>()
            .ActivateOnEnter<CumMeter>();

        ComponentCondition<P2Explosion>(id + 0x10, 10.8f, e => e.NumCasts >= 2, "Towers 1");
        ComponentCondition<P2Explosion>(id + 0x12, 12, e => e.NumCasts >= 4, "Towers 2");
        ComponentCondition<P2Explosion>(id + 0x14, 12, e => e.NumCasts >= 6, "Towers 3");
        ComponentCondition<SpearpointAOE>(id + 0x16, 12.9f, e => e.NumCasts >= 8, "Baits 4");

        Timeout(id + 0x100, 9999, "Enrage");
    }

    private void Phase2(uint id)
    {
        Targetable(id, true, 5, "Boss appears")
            .ActivateOnEnter<PerfumedQuietus>();
        CastStart(id + 0x10, AID._Weaponskill_PerfumedQuietus, 0.1f);
        ComponentCondition<PerfumedQuietus>(id + 0x12, 9.2f, p => p.NumCasts > 0, "Raidwide");

        Cast(id + 0x20, AID._Weaponskill_RosebloodBloom, 12.3f, 2.6f, "Bloom 1 start")
            .ActivateOnEnter<AlexandrianThunderII>()
            .ActivateOnEnter<AlexandrianThunderIII>();

        CastStart(id + 0x30, AID._Weaponskill_AlexandrianThunderII, 3.5f);
        ComponentCondition<AlexandrianThunderII>(id + 0x32, 5.7f, e => e.NumCasts > 0, "Rotating AOEs start");
        ComponentCondition<AlexandrianThunderII>(id + 0x34, 14.2f, e => e.NumCasts >= 90, "Rotating AOEs end");
        ComponentCondition<AlexandrianThunderIII>(id + 0x36, 5.7f, e => e.NumFinishedSpreads > 0, "Spreads");

        Raidwide(id + 0x100, 2.4f);

        id += 0x10000;

        Cast(id, AID._Weaponskill_Roseblood2NdBloom, 7.4f, 2.6f, "Bloom 2 start");

        Timeout(id + 0x100000, 9999, "Enrage");
    }

    private void Raidwide(uint id, float delay)
    {
        Cast(id, AID._Weaponskill_ThornedCatharsis, delay, 5, "Raidwide");
    }

    private void EscelonsFall(uint id, float delay, string name)
    {
        CastStart(id, AID._Weaponskill_EscelonsFall, delay, $"{name} start")
            .ActivateOnEnter<EscelonsFall>();

        ComponentCondition<EscelonsFall>(id + 2, 13.9f, e => e.NumCasts >= 4, "Baits 1");
        ComponentCondition<EscelonsFall>(id + 4, 3.1f, e => e.NumCasts >= 8, "Baits 2");
        ComponentCondition<EscelonsFall>(id + 6, 3.1f, e => e.NumCasts >= 12, "Baits 3");
        ComponentCondition<EscelonsFall>(id + 8, 3.1f, e => e.NumCasts >= 16, "Baits 4")
            .DeactivateOnExit<EscelonsFall>();
    }

    private void StockBreak(uint id, float delay)
    {
        CastStart(id, AID._Weaponskill_StockBreak, delay)
            .ActivateOnEnter<StockBreak>();

        ComponentCondition<StockBreak>(id + 0x10, 9.2f, s => s.NumCasts > 0, "Stack start");
        ComponentCondition<StockBreak>(id + 0x20, 2.2f, s => s.NumCasts >= 3, "Stack end")
            .DeactivateOnExit<StockBreak>();
    }
}
