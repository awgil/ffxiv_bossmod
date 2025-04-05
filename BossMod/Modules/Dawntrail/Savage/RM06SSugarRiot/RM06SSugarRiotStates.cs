namespace BossMod.Dawntrail.Savage.RM06SSugarRiot;

class MousseMural(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Spell_MousseMural));

class ColorRiot(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private readonly List<Actor> Casters = [];

    private bool IceClose;
    private DateTime Activation;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_ColorRiot1:
                Casters.Add(caster);
                Activation = Module.CastFinishAt(spell, 2.2f);
                IceClose = true;
                break;
            case AID._Weaponskill_ColorRiot:
                Casters.Add(caster);
                Activation = Module.CastFinishAt(spell, 2.2f);
                IceClose = false;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Spell_CoolBomb or AID._Spell_WarmBomb)
        {
            NumCasts++;
            Casters.Clear();
        }
    }

    public override void Update()
    {
        CurrentBaits.Clear();
        if (Casters.Count == 0)
            return;

        AddBait(Raid.WithoutSlot().Closest(Casters[0].Position));
        AddBait(Raid.WithoutSlot().Farthest(Casters[0].Position));
    }

    enum Preference { None, Close, Far }

    private Preference GetPreference(Actor actor)
    {
        if (Casters.Count == 0)
            return Preference.None;

        if (actor.FindStatus(SID._Gen_CoolTint) != null)
            return IceClose ? Preference.Far : Preference.Close;
        if (actor.FindStatus(SID._Gen_WarmTint) != null)
            return IceClose ? Preference.Close : Preference.Far;

        return Preference.None;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        switch (GetPreference(actor))
        {
            case Preference.Close:
                hints.Add("Bait close!", Raid.WithoutSlot().Closest(Casters[0].Position) != actor);
                break;
            case Preference.Far:
                hints.Add("Bait far!", Raid.WithoutSlot().Farthest(Casters[0].Position) != actor);
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Casters.Count == 0)
            return;

        switch (GetPreference(actor))
        {
            case Preference.Close:
                var closest = Raid.WithoutSlot().Exclude(actor).Closest(Casters[0].Position)!;
                hints.AddForbiddenZone(ShapeDistance.Donut(Casters[0].Position, closest.DistanceToPoint(Casters[0].Position), 100), CurrentBaits[0].Activation);
                break;
            case Preference.Far:
                var farthest = Raid.WithoutSlot().Exclude(actor).Farthest(Casters[0].Position)!;
                hints.AddForbiddenZone(ShapeDistance.Circle(Casters[0].Position, farthest.DistanceToPoint(Casters[0].Position)), CurrentBaits[0].Activation);
                break;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Casters.Count > 0)
            hints.Add(IceClose ? "Ice close, fire far" : "Ice far, fire close");
    }

    private void AddBait(Actor? target)
    {
        if (target != null)
            CurrentBaits.Add(new(Casters[0], target, new AOEShapeCircle(4), Activation));
    }
}

class Wingmark(BossModule module) : Components.Knockback(module)
{
    private BitMask Players;
    private DateTime KnockbackFinishAt = DateTime.MaxValue;

    public bool StunHappened => KnockbackFinishAt != DateTime.MaxValue;
    public bool KnockbackFinished => WorldState.CurrentTime >= KnockbackFinishAt;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_Wingmark)
            Players.Set(Raid.FindSlot(actor.InstanceID));
        if ((SID)status.ID == SID._Gen_Stun)
        {
            KnockbackFinishAt = WorldState.FutureTime(3);
            Players.Clear(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (Players[slot])
            yield return new Source(actor.Position, 35, Direction: actor.Rotation, Kind: Kind.DirForward);
    }
}

class WingmarkAdds(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> Adds = [];
    private DateTime Activation;
    public bool Risky;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID is TetherID._Gen_Tether_319 or TetherID._Gen_Tether_320 && !source.IsAlly)
        {
            Adds.Add(source);
            if (Activation == default)
                Activation = WorldState.FutureTime(13.2f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_Burst:
            case AID._Weaponskill_Burst1:
            case AID._Weaponskill_BadBreath:
            case AID._Spell_DarkMist:
                NumCasts++;
                Adds.Remove(caster);
                break;
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var add in Adds)
        {
            switch ((OID)add.OID)
            {
                case OID._Gen_PaintBomb:
                    yield return new AOEInstance(new AOEShapeCircle(15), add.Position, Activation: Activation, Risky: Risky);
                    break;
                case OID._Gen_HeavenBomb:
                    yield return new AOEInstance(new AOEShapeCircle(15), add.Position + add.Rotation.ToDirection() * 16, Activation: Activation, Risky: Risky);
                    break;
                case OID._Gen_CandiedSuccubus:
                    yield return new AOEInstance(new AOEShapeCircle(30), add.Position, Activation: Activation, Risky: Risky);
                    break;
                case OID._Gen_MouthwateringMorbol:
                    yield return new AOEInstance(new AOEShapeCone(50, 50.Degrees()), add.Position, add.Rotation, Activation: Activation, Risky: Risky);
                    break;
            }
        }
    }
}

class ColorClash(BossModule module) : Components.GenericStackSpread(module)
{
    private readonly List<Stack> SavedStacks = [];
    public int NumCasts;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_ColorClash:
                foreach (var h in Raid.WithoutSlot().Where(r => r.Class.GetRole() == Role.Healer).Take(2))
                    SavedStacks.Add(new(h, 6, 4, activation: WorldState.FutureTime(24.5f)));
                break;
            case AID._Weaponskill_ColorClash1:
                foreach (var h in Raid.WithoutSlot().Where(r => r.Class.GetRole3() == Role3.Support).Take(4))
                    SavedStacks.Add(new(h, 6, 2, 2, activation: WorldState.FutureTime(24.5f)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Spell_ColorClash or AID._Spell_ColorClash1)
        {
            NumCasts++;
            Stacks.Clear();
        }
    }

    public void Activate()
    {
        Stacks.AddRange(SavedStacks);
        SavedStacks.Clear();
    }
}

class StickyMousse(BossModule module) : Components.GenericBaitAway(module, ActionID.MakeSpell(AID._Spell_StickyMousse), centerAtTarget: true)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_StickyMousse)
        {
            foreach (var tar in Raid.WithoutSlot())
                CurrentBaits.Add(new(caster, tar, new AOEShapeCircle(4), Module.CastFinishAt(spell, 0.9f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action == WatchedAction)
            CurrentBaits.Clear();
    }
}

class StickyBurst(BossModule module) : Components.UniformStackSpread(module, 4, 0, 4)
{
    public int NumCasts;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_MousseMine)
            AddStack(actor, status.ExpireAt);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Spell_Burst)
        {
            NumCasts++;
            Stacks.Clear();
        }
    }
}

class SprayPain(BossModule module) : Components.StandardAOEs(module, ActionID.MakeSpell(AID._Weaponskill_SprayPain), new AOEShapeCircle(10), maxCasts: 5);

class HeatingUp(BossModule module) : Components.UniformStackSpread(module, 0, 15, alwaysShowSpreads: true)
{
    private readonly List<Spread> SavedSpreads = [];
    public int NumCasts;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_HeatingUp)
        {
            SavedSpreads.Add(new(actor, 15, Activation: status.ExpireAt));
        }
    }

    public void Activate()
    {
        SavedSpreads.SortBy(s => s.Activation);
        if (SavedSpreads.Count > 0)
        {
            Spreads.AddRange(SavedSpreads[0..2]);
            SavedSpreads.RemoveRange(0, 2);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Spell_Brulee)
        {
            NumCasts++;
            if (Spreads.Count > 0)
                Spreads.RemoveAt(0);
        }
    }
}

class BurningUp(BossModule module) : Components.UniformStackSpread(module, 6, 0)
{
    public int NumCasts;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID._Gen_BurningUp:
                Stacks.Add(new(actor, 6, activation: status.ExpireAt));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Spell_CrowdBrulee:
                NumCasts++;
                Stacks.Clear();
                break;
        }
    }
}

class Quicksand(BossModule module) : Components.GenericAOEs(module, warningText: "GTFO from quicksand!")
{
    public WPos? Center;
    public int Activations;
    public int Deactivations;

    private DateTime activatedAt;

    private BitMask StandInQuicksand;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID._Gen_Tether_320)
            StandInQuicksand.Set(Raid.FindSlot(source.InstanceID));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Spell_PuddingGraf1)
            StandInQuicksand.Reset();
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001)
        {
            switch (index)
            {
                case 0x1F:
                    Activate(Arena.Center);
                    break;
                case 0x20:
                    Activate(new(100, 80));
                    break;
                case 0x21:
                    Activate(new(100, 120));
                    break;
                case 0x22:
                    Activate(new(120, 100));
                    break;
                case 0x23:
                    Activate(new(80, 100));
                    break;
            }
        }
        else if (state == 0x00080004 && index is >= 0x1F and <= 0x23)
        {
            Deactivations++;
            activatedAt = default;
            Center = null;
        }
    }

    private void Activate(WPos p)
    {
        Center = p;
        Activations++;
        activatedAt = WorldState.CurrentTime;
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(Center).Select(c => new AOEInstance(new AOEShapeCircle(23), c, default, activatedAt.AddSeconds(9), Color: StandInQuicksand[slot] ? ArenaColor.SafeFromAOE : 0, Risky: !StandInQuicksand[slot]));
}

class SprayPain2(BossModule module) : Components.StandardAOEs(module, ActionID.MakeSpell(AID._Weaponskill_SprayPain1), new AOEShapeCircle(10));

class PuddingGraf(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID._Spell_PuddingGraf1), 6);
class PuddingGrafAim(BossModule module) : BossComponent(module)
{
    private BitMask Aimers;
    private Quicksand? _qs;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID._Gen_Tether_319)
            Aimers.Set(Raid.FindSlot(source.InstanceID));
    }

    public override void Update()
    {
        _qs ??= Module.FindComponent<Quicksand>();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Spell_PuddingGraf1)
            Aimers.Reset();
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (GetBombLocation(pcSlot, pc) is { } bomb)
        {
            Arena.AddLine(pc.Position, bomb, ArenaColor.Danger);
            Arena.ActorProjected(pc.Position, bomb, pc.Rotation, ArenaColor.Danger);
        }
    }

    private WPos? GetBombLocation(int slot, Actor actor) => Aimers[slot] ? actor.Position + actor.Rotation.ToDirection() * 16 : null;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (GetBombLocation(slot, actor) is { } bomb && _qs?.Center is { } qsCenter)
            hints.Add("Aim at quicksand!", !bomb.InCircle(qsCenter, 23));
    }
}

class Adds(BossModule module) : BossComponent(module)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Module.Enemies(OID._Gen_Yan), ArenaColor.Enemy);
        Arena.Actors(Module.Enemies(OID._Gen_Mu), ArenaColor.Enemy);
        Arena.Actors(Module.Enemies(OID._Gen_GimmeCat), ArenaColor.Enemy);
        Arena.Actors(Module.Enemies(OID._Gen_Jabberwock), ArenaColor.Enemy);
        Arena.Actors(Module.Enemies(OID._Gen_FeatherRay), ArenaColor.Enemy);

        foreach (var ram in Module.Enemies(OID._Gen_Yan).Where(x => x.IsTargetable && !x.IsDead && x.TargetID == pc.InstanceID))
            Arena.AddCircle(ram.Position, 13, ArenaColor.Danger); // estimate of Rallying Cheer radius
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
        {
            switch ((OID)h.Actor.OID)
            {
                case OID._Gen_GimmeCat:
                case OID._Gen_Mu:
                    h.ForbidDOTs = true;
                    break;

                case OID._Gen_FeatherRay:
                    // prevent melees/tanks from getting aggro on ray
                    // if ray times out and tethers a random person, it will be added to the set and this conditional will be skipped
                    if (!TetheredRays.Contains(h.Actor.InstanceID) && actor.Role is not (Role.Ranged or Role.Healer))
                        h.Priority = AIHints.Enemy.PriorityForbidden;
                    break;

                case OID._Gen_Jabberwock:
                    h.Priority = 5;
                    h.ShouldBeStunned = true;
                    break;
            }
        }
    }

    private readonly HashSet<ulong> TetheredRays = [];

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID._Gen_Tether_17)
            TetheredRays.Add(source.InstanceID);
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID._Gen_Tether_17)
            TetheredRays.Remove(source.InstanceID);
    }
}

class ICraveViolence(BossModule module) : Components.StandardAOEs(module, ActionID.MakeSpell(AID._Weaponskill_ICraveViolence), new AOEShapeCircle(6));
class WaterIII(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 8, ActionID.MakeSpell(AID._Spell_WaterIII1), m => m.Enemies(0x1EBD91).Where(o => o.EventState != 7), 1.5f);
class ReadyOreNot(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_ReadyOreNot));

class SugarRiotStates : StateMachineBuilder
{
    public SugarRiotStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<MousseMural>();
    }

    private void SinglePhase(uint id)
    {
        Cast(id, AID._Spell_MousseMural, 6.2f, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);

        ColorRiot(id + 0x10, 6.2f);

        Cast(id + 0x20, AID._Weaponskill_Wingmark, 6.6f, 4)
            .ActivateOnEnter<Wingmark>()
            .ActivateOnEnter<WingmarkAdds>()
            .ActivateOnEnter<ColorClash>();

        CastMulti(id + 0x22, [AID._Weaponskill_ColorClash, AID._Weaponskill_ColorClash1], 3.1f, 3);

        CastStartMulti(id + 0x24, [AID._Weaponskill_DoubleStyle4, AID._Weaponskill_DoubleStyle3, AID._Weaponskill_DoubleStyle, AID._Weaponskill_DoubleStyle1, AID._Weaponskill_DoubleStyle6, AID._Weaponskill_DoubleStyle5, AID._Weaponskill_DoubleStyle7, AID._Weaponskill_DoubleStyle8], 4.4f);

        ComponentCondition<Wingmark>(id + 0x34, 11.5f, w => w.StunHappened, "Stun")
            .SetHint(StateMachine.StateHint.DowntimeStart);

        ComponentCondition<Wingmark>(id + 0x35, 3, w => w.KnockbackFinished)
            .SetHint(StateMachine.StateHint.DowntimeEnd)
            .ExecOnExit<WingmarkAdds>(w => w.Risky = true)
            .ExecOnExit<ColorClash>(c => c.Activate());

        ComponentCondition<WingmarkAdds>(id + 0x36, 1.6f, w => w.NumCasts > 0, "AOEs");
        ComponentCondition<ColorClash>(id + 0x37, 0.7f, w => w.NumCasts > 0, "Stacks")
            .DeactivateOnExit<Wingmark>()
            .DeactivateOnExit<WingmarkAdds>()
            .DeactivateOnExit<ColorClash>();

        StickyMousse(id + 0x100, 2.3f);

        ColorRiot(id + 0x200, 0.4f);

        id += 0x10000;
        Cast(id, AID._Weaponskill_Sugarscape, 6.45f, 1).ActivateOnEnter<SprayPain>();
        Cast(id + 0x10, AID._Weaponskill_Layer, 15.3f, 1);

        ComponentCondition<SprayPain>(id + 0x12, 14.2f, s => s.NumCasts > 0, "Cactus 1");
        ComponentCondition<SprayPain>(id + 0x13, 3.1f, s => s.NumCasts > 5);
        ComponentCondition<SprayPain>(id + 0x14, 3, s => s.NumCasts > 10);
        ComponentCondition<SprayPain>(id + 0x15, 3, s => s.NumCasts > 15);
        ComponentCondition<SprayPain>(id + 0x16, 3, s => s.NumCasts > 20)
            .ActivateOnEnter<HeatingUp>()
            .ExecOnEnter<HeatingUp>(h => h.Activate());
        ComponentCondition<SprayPain>(id + 0x20, 3.1f, s => s.NumCasts > 25, "Cactus 6")
            .ActivateOnEnter<BurningUp>();

        ComponentCondition<BurningUp>(id + 0x30, 4.5f, b => b.NumCasts > 0, "Defams + stack");

        StickyMousse(id + 0x100, 2.7f);

        id += 0x10000;
        Cast(id + 0x110, AID._Weaponskill_Layer1, 0.5f, 1)
            .ActivateOnEnter<Quicksand>()
            .ActivateOnEnter<SprayPain2>()
            .ActivateOnEnter<PuddingGraf>()
            .ActivateOnEnter<PuddingGrafAim>();

        ComponentCondition<Quicksand>(id + 0x112, 7.1f, q => q.Activations > 0, "Quicksand appear")
            .ExecOnExit<HeatingUp>(h => h.Activate());

        ComponentCondition<HeatingUp>(id + 0x120, 7.9f, b => b.NumCasts > 2, "Defams 2");
        ComponentCondition<SprayPain2>(id + 0x121, 0.8f, s => s.NumCasts > 0, "Safe corner");

        Cast(id + 0x130, AID._Spell_PuddingGraf, 0.5f, 3);

        ComponentCondition<Quicksand>(id + 0x140, 0.9f, q => q.Deactivations > 0, "Quicksand disappear");

        id += 0x10000;
        Cast(id, AID._Weaponskill_DoubleStyle2, 3.55f, 8);
        ComponentCondition<PuddingGraf>(id + 2, 1, p => p.NumFinishedSpreads > 0, "Place bombs");

        Cast(id + 4, AID._Spell_MousseMural, 4.1f, 5, "Raidwide");

        ColorRiot(id + 0x10, 3.2f);

        id += 0x10000;
        Cast(id, AID._Spell_SoulSugar, 5.2f, 3);
        Cast(id + 0x10, AID._Weaponskill_LivePainting, 3.25f, 4, "Adds 1")
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<ICraveViolence>()
            .ActivateOnEnter<WaterIII>();
        Cast(id + 0x20, AID._Weaponskill_LivePainting1, 25.3f, 4, "Adds 2");
        Cast(id + 0x30, AID._Weaponskill_LivePainting2, 18.2f, 4, "Adds 3")
            .ActivateOnEnter<ReadyOreNot>();
        Cast(id + 0x40, AID._Weaponskill_ReadyOreNot, 23.2f, 7, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        Cast(id + 0x50, AID._Weaponskill_LivePainting3, 5.2f, 4, "Adds 4");

        SimpleState(id + 0xFF0000, 9999, "???");
    }

    private void ColorRiot(uint id, float delay)
    {
        CastStartMulti(id, [AID._Weaponskill_ColorRiot, AID._Weaponskill_ColorRiot1], delay)
            .ActivateOnEnter<ColorRiot>();
        ComponentCondition<ColorRiot>(id + 1, 7, c => c.NumCasts > 0, "Tankbuster")
            .DeactivateOnExit<ColorRiot>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void StickyMousse(uint id, float delay)
    {
        CastStart(id, AID._Weaponskill_StickyMousse, delay)
            .ActivateOnEnter<StickyMousse>()
            .ActivateOnEnter<StickyBurst>();
        CastEnd(id + 1, 5);
        ComponentCondition<StickyMousse>(id + 2, 0.8f, b => b.NumCasts > 0, "Spreads");
        ComponentCondition<StickyBurst>(id + 3, 6, b => b.NumCasts > 0, "Stacks")
            .DeactivateOnExit<StickyMousse>()
            .DeactivateOnExit<StickyBurst>();
    }

    //private void XXX(uint id, float delay)
}
