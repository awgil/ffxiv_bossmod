using Dalamud.Utility;

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

class WingmarkKB(BossModule module) : Components.Knockback(module)
{
    private BitMask Players;
    private DateTime KnockbackFinishAt = DateTime.MaxValue;
    private WingmarkAdds? _adds;

    public bool Risky;

    public override void Update()
    {
        _adds ??= Module.FindComponent<WingmarkAdds>();
    }

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

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => Risky && base.DestinationUnsafe(slot, actor, pos);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_adds?.SafeCorner() is { } p && actor.FindStatus(SID._Gen_Wingmark) is { } st)
        {
            hints.AddForbiddenZone(ShapeDistance.Circle(p, 35), st.ExpireAt);
            var angleToCorner = Angle.FromDirection(p - actor.Position);
            hints.ForbiddenDirections.Add((angleToCorner + 180.Degrees(), 178.Degrees(), st.ExpireAt));
        }
    }
}

class WingmarkAdds(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<Actor> Adds = [];
    private DateTime Activation;
    public bool Risky;

    private BitMask DangerCorners;
    private static readonly WPos[] Corners = [new(80, 80), new(120, 80), new(120, 120), new(80, 120)];

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID is TetherID._Gen_Tether_319 or TetherID._Gen_Tether_320 && !source.IsAlly)
        {
            Adds.Add(source);
            if (Activation == default)
                Activation = WorldState.FutureTime(13.2f);

            switch ((OID)source.OID)
            {
                case OID._Gen_PaintBomb:
                    MarkDanger(p => p.InCircle(source.Position, 15));
                    break;
                case OID._Gen_HeavenBomb:
                    MarkDanger(p => p.InCircle(source.Position + source.Rotation.ToDirection() * 16, 15));
                    break;
                case OID._Gen_MouthwateringMorbol:
                    MarkDanger(p => p.InCone(source.Position, source.Rotation, 50.Degrees()));
                    break;
                case OID._Gen_CandiedSuccubus:
                    MarkDanger(p => p.InCircle(source.Position, 30));
                    break;
            }
        }
    }

    private void MarkDanger(Func<WPos, bool> f)
    {
        for (var i = 0; i < Corners.Length; i++)
            if (f(Corners[i]))
                DangerCorners.Set(i);
    }

    public WPos? SafeCorner()
    {
        if (DangerCorners.NumSetBits() == 3)
            for (var i = 0; i < Corners.Length; i++)
                if (!DangerCorners[i])
                    return Corners[i];

        return null;
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
                if (Adds.Count == 0)
                    DangerCorners.Reset();
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

    private string _hint = "";

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_ColorClash:
                _hint = "Next: party stacks";
                foreach (var h in Raid.WithoutSlot().Where(r => r.Class.GetRole() == Role.Healer).Take(2))
                    SavedStacks.Add(new(h, 6, 4, activation: WorldState.FutureTime(24.5f)));
                break;
            case AID._Weaponskill_ColorClash1:
                _hint = "Next: pairs";
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

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (!_hint.IsNullOrEmpty())
            hints.Add(_hint);
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
            foreach (var tar in Raid.WithoutSlot().Where(x => x.Class.GetRole() != Role.Tank))
                CurrentBaits.Add(new(caster, tar, new AOEShapeCircle(4), Module.CastFinishAt(spell, 0.9f)));
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

class SprayPain(BossModule module) : Components.StandardAOEs(module, ActionID.MakeSpell(AID._Weaponskill_SprayPain), new AOEShapeCircle(10), maxCasts: 10)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var i = 0;
        foreach (var aoe in base.ActiveAOEs(slot, actor))
        {
            if (++i > 5)
                yield return aoe with { Color = ArenaColor.AOE, Risky = false };
            else
                yield return aoe with { Color = ArenaColor.Danger };
        }
    }
}

class HeatingUpHints(BossModule module) : BossComponent(module)
{
    private readonly DateTime[] SpreadAt = new DateTime[PartyState.MaxPartySize];
    private readonly DateTime[] StackAt = new DateTime[PartyState.MaxPartySize];

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID._Gen_HeatingUp:
                SpreadAt[Raid.FindSlot(actor.InstanceID)] = status.ExpireAt;
                break;
            case SID._Gen_BurningUp:
                StackAt[Raid.FindSlot(actor.InstanceID)] = status.ExpireAt;
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID._Gen_HeatingUp:
                SpreadAt[Raid.FindSlot(actor.InstanceID)] = default;
                break;
            case SID._Gen_BurningUp:
                StackAt[Raid.FindSlot(actor.InstanceID)] = default;
                break;
        }
    }

    public void Prune()
    {
        for (var i = 0; i < SpreadAt.Length; i++)
        {
            if (SpreadAt[i] > WorldState.CurrentTime && SpreadAt[i] < WorldState.FutureTime(20))
                SpreadAt[i] = default;
            if (StackAt[i] > WorldState.CurrentTime && StackAt[i] < WorldState.FutureTime(20))
                StackAt[i] = default;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (SpreadAt[slot] != default)
            hints.Add($"Defamation in {(SpreadAt[slot] - WorldState.CurrentTime).TotalSeconds:f1}s", false);

        if (StackAt[slot] != default)
            hints.Add($"Party stack in {(StackAt[slot] - WorldState.CurrentTime).TotalSeconds:f1}s", false);
    }
}

class HeatingUp(BossModule module) : Components.UniformStackSpread(module, 6, 15, alwaysShowSpreads: true)
{
    public int NumCasts;
    public bool EnableAIHints;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID._Gen_HeatingUp:
                if (status.ExpireAt < WorldState.FutureTime(20))
                    AddSpread(actor, status.ExpireAt);
                break;
            case SID._Gen_BurningUp:
                AddStack(actor, status.ExpireAt);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Spell_Brulee:
                NumCasts++;
                if (Spreads.Count > 0)
                    Spreads.RemoveAt(0);
                break;
            case AID._Spell_CrowdBrulee:
                NumCasts++;
                Stacks.Clear();
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (EnableAIHints)
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class Quicksand(BossModule module) : Components.GenericAOEs(module, warningText: "GTFO from quicksand!")
{
    public WPos? Center;
    public int AppearCount;
    public int ActivationCount;
    public int DisappearCount;
    public DateTime Activation;

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
            DisappearCount++;
            Center = null;
        }
        else if (state == 0x00200010 && index is >= 0x1F and <= 0x23)
            ActivationCount++;
    }

    private void Activate(WPos p)
    {
        Center = p;
        Activation = WorldState.FutureTime(9);
        AppearCount++;
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(Center).Select(c => new AOEInstance(new AOEShapeCircle(23), c, default, Activation, Color: StandInQuicksand[slot] ? ArenaColor.SafeFromAOE : 0, Risky: !StandInQuicksand[slot]));
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
    public int YanCounter;
    public int SquirrelCounter;
    public int CatCounter;
    public int JabberwockCounter;
    public int RayCounter;

    private int HuffyCat;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var squirrels = Module.Enemies(OID._Gen_Mu);
        Arena.Actors(Module.Enemies(OID._Gen_Yan), ArenaColor.Enemy);
        Arena.Actors(squirrels, ArenaColor.Enemy);
        Arena.Actors(Module.Enemies(OID._Gen_GimmeCat), ArenaColor.Enemy);
        Arena.Actors(Module.Enemies(OID._Gen_Jabberwock), ArenaColor.Enemy);
        Arena.Actors(Module.Enemies(OID._Gen_FeatherRay), ArenaColor.Enemy);

        if (squirrels.Any(x => !x.IsDeadOrDestroyed))
            foreach (var ram in Module.Enemies(OID._Gen_Yan).Where(x => x.IsTargetable && !x.IsDead && x.TargetID == pc.InstanceID))
                Arena.AddCircle(ram.Position, 13, ArenaColor.Danger); // estimate of Rallying Cheer radius
    }

    public override void OnTargetable(Actor actor)
    {
        switch ((OID)actor.OID)
        {
            case OID._Gen_Yan:
                YanCounter++;
                break;
            case OID._Gen_Mu:
                SquirrelCounter++;
                break;
            case OID._Gen_GimmeCat:
                CatCounter++;
                break;
            case OID._Gen_Jabberwock:
                JabberwockCounter++;
                break;
            case OID._Gen_FeatherRay:
                RayCounter++;
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var playerIsRanged = actor.Role is Role.Ranged or Role.Healer;

        foreach (var h in hints.PotentialTargets)
        {
            switch ((OID)h.Actor.OID)
            {
                case OID._Gen_GimmeCat:
                    h.ForbidDOTs = true;
                    h.Priority = 1;
                    break;

                case OID._Gen_Mu:
                case OID._Gen_Yan:
                    h.Priority = 1;
                    h.ForbidDOTs = true;
                    break;

                case OID._Gen_FeatherRay:
                    // rays usually don't live long enough - TODO should this be job-specific?
                    h.ForbidDOTs = true;

                    if (TetheredRays.Contains(h.Actor.InstanceID))
                    {
                        // prioritize rays over using aoe on squirrel
                        if (actor.DistanceToHitbox(h.Actor) <= 3)
                            h.Priority = 3;
                    }
                    else if (!playerIsRanged)
                        // prevent melees/tanks from getting aggro on ray
                        // if ray times out and tethers a random person, it will be added to the set and this conditional will be skipped
                        h.Priority = AIHints.Enemy.PriorityForbidden;
                    break;

                case OID._Gen_Jabberwock:
                    h.ForbidDOTs = true;
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

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_HuffyCat)
            HuffyCat++;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_HuffyCat)
            HuffyCat = 0;
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (HuffyCat > 0)
            hints.Add($"Cat stacks: {HuffyCat}");
    }
}

class ICraveViolence(BossModule module) : Components.StandardAOEs(module, ActionID.MakeSpell(AID._Weaponskill_ICraveViolence), new AOEShapeCircle(6));
class WaterIII(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 8, ActionID.MakeSpell(AID._Spell_WaterIII1), m => m.Enemies(0x1EBD91).Where(o => o.EventState != 7), 1.5f, 5);
class WaterIIITether(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCircle(8), (uint)TetherID._Gen_Tether_17, centerAtTarget: true)
{
    public override void Update()
    {
        CurrentBaits.RemoveAll(b => b.Source.IsDeadOrDestroyed);
    }
}
class ReadyOreNot(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_ReadyOreNot));
class OreRigato(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_OreRigato), "Squirrel enrage!");
class HangryHiss(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_HangryHiss), "Cat enrage!");

class SweetShot(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<(Actor Caster, DateTime Activation)> Casters = [];

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((OID)source.OID == OID._Gen_SweetShot && (TetherID)tether.ID is TetherID._Gen_Tether_319 or TetherID._Gen_Tether_320)
            Casters.Add((source, WorldState.FutureTime(6.3f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_Rush)
        {
            NumCasts++;
            Casters.RemoveAll(c => c.Caster == caster);
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Select(c => new AOEInstance(new AOEShapeRect(100, 3.5f), c.Caster.Position, c.Caster.Rotation, c.Activation));
}

class DoubleStyle2(BossModule module) : Components.GenericStackSpread(module)
{
    public int NumCasts;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_DoubleStyle9)
        {
            foreach (var p in Raid.WithoutSlot().Where(r => r.Role == Role.Healer))
                Stacks.Add(new(p, 6, 4, 4, WorldState.FutureTime(10.2f)));
        }

        if ((AID)spell.Action.ID == AID._Weaponskill_DoubleStyle10)
        {
            foreach (var p in Raid.WithoutSlot())
                Spreads.Add(new(p, 6, WorldState.FutureTime(10.2f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Spell_TasteOfFire && Stacks.Count > 0)
        {
            NumCasts++;
            Stacks.RemoveAt(0);
        }
        if ((AID)spell.Action.ID == AID._Spell_TasteOfThunder1 && Spreads.Count > 0)
        {
            NumCasts++;
            Spreads.RemoveAt(0);
        }
    }
}

class DoubleStyle2Arena(BossModule module) : BossComponent(module)
{
    private bool RiverSafe;
    private DateTime Activation;

    private RelSimplifiedComplexPolygon RiverPoly => ((SugarRiot)Module).RiverPoly;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_DoubleStyle9)
        {
            RiverSafe = true;
            Activation = WorldState.FutureTime(10.2f);
        }

        if ((AID)spell.Action.ID == AID._Weaponskill_DoubleStyle10)
        {
            RiverSafe = false;
            Activation = WorldState.FutureTime(10.2f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Spell_TasteOfThunder1 or AID._Spell_TasteOfFire)
            Activation = default;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Activation == default)
            return;

        if (RiverSafe)
            hints.Add("Stand in water!", !RiverPoly.Contains(actor.Position - Arena.Center));
        else
            hints.Add("Avoid water!", RiverPoly.Contains(actor.Position - Arena.Center));
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Activation == default)
            return;

        Arena.ZoneComplex(Arena.Center, default, RiverPoly, RiverSafe ? ArenaColor.SafeFromAOE : ArenaColor.AOE);
    }
}

// cats enrage 48.4-48.6s after targetable
// squirrels 1 enrage 84.4s after targetable
// squirrels 2 enrage 55.2s after targetable (simultaneous with squirrels 1)
// squirrels 3 who the fuck knows

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
            .ActivateOnEnter<WingmarkKB>()
            .ActivateOnEnter<WingmarkAdds>()
            .ActivateOnEnter<ColorClash>();

        CastMulti(id + 0x22, [AID._Weaponskill_ColorClash, AID._Weaponskill_ColorClash1], 3.1f, 3);

        CastStartMulti(id + 0x24, [AID._Weaponskill_DoubleStyle4, AID._Weaponskill_DoubleStyle3, AID._Weaponskill_DoubleStyle, AID._Weaponskill_DoubleStyle1, AID._Weaponskill_DoubleStyle6, AID._Weaponskill_DoubleStyle5, AID._Weaponskill_DoubleStyle7, AID._Weaponskill_DoubleStyle8], 4.4f);

        ComponentCondition<WingmarkAdds>(id + 0x26, 3, w => w.Adds.Count > 0, "Summons appear")
            .ExecOnExit<WingmarkKB>(w => w.Risky = true);

        ComponentCondition<WingmarkKB>(id + 0x34, 8.44f, w => w.StunHappened, "Stun")
            .SetHint(StateMachine.StateHint.DowntimeStart)
            .ExecOnExit<WingmarkAdds>(w => w.Risky = true)
            .ExecOnExit<ColorClash>(c => c.Activate());

        ComponentCondition<WingmarkKB>(id + 0x35, 3, w => w.KnockbackFinished)
            .SetHint(StateMachine.StateHint.DowntimeEnd);

        ComponentCondition<WingmarkAdds>(id + 0x36, 1.6f, w => w.NumCasts > 0, "AOEs");
        ComponentCondition<ColorClash>(id + 0x37, 0.5f, w => w.NumCasts > 0, "Stacks")
            .DeactivateOnExit<WingmarkKB>()
            .DeactivateOnExit<WingmarkAdds>()
            .DeactivateOnExit<ColorClash>();

        StickyMousse(id + 0x100, 2.3f);

        ColorRiot(id + 0x200, 0.4f);

        id += 0x10000;
        Cast(id, AID._Weaponskill_Sugarscape, 6.45f, 1)
            .ActivateOnEnter<SprayPain>()
            .ActivateOnEnter<HeatingUpHints>();
        Cast(id + 0x10, AID._Weaponskill_Layer, 15.3f, 1);

        ComponentCondition<SprayPain>(id + 0x20, 7.2f, s => s.Casters.Count > 0, "Cacti appear");

        ComponentCondition<SprayPain>(id + 0x22, 6.9f, s => s.NumCasts > 0, "Cactus AOEs 1");
        ComponentCondition<SprayPain>(id + 0x23, 3.1f, s => s.NumCasts > 5);
        ComponentCondition<SprayPain>(id + 0x24, 3, s => s.NumCasts > 10);
        ComponentCondition<SprayPain>(id + 0x25, 3, s => s.NumCasts > 15);
        ComponentCondition<SprayPain>(id + 0x26, 3, s => s.NumCasts > 20)
            .ActivateOnEnter<HeatingUp>()
            .ExecOnEnter<HeatingUpHints>(h => h.Prune());
        ComponentCondition<SprayPain>(id + 0x30, 3.1f, s => s.NumCasts > 25, "Cactus AOEs 6")
            .ExecOnExit<HeatingUp>(h => h.EnableAIHints = true);

        ComponentCondition<HeatingUp>(id + 0x40, 4.5f, b => b.NumCasts > 0, "Defams + stack")
            .DeactivateOnExit<HeatingUp>()
            .DeactivateOnExit<SprayPain>();

        StickyMousse(id + 0x100, 2.7f);

        id += 0x10000;
        Cast(id + 0x110, AID._Weaponskill_Layer1, 0.5f, 1)
            .ActivateOnEnter<Quicksand>()
            .ActivateOnEnter<SprayPain2>()
            .ActivateOnEnter<PuddingGraf>()
            .ActivateOnEnter<PuddingGrafAim>();

        ComponentCondition<Quicksand>(id + 0x112, 7.1f, q => q.AppearCount > 0, "Quicksand appear")
            .DeactivateOnExit<HeatingUpHints>();

        ComponentCondition<HeatingUp>(id + 0x120, 8, b => b.NumCasts > 0, "Defams 2")
            .ActivateOnEnter<HeatingUp>()
            .DeactivateOnExit<HeatingUp>();
        ComponentCondition<SprayPain2>(id + 0x121, 0.6f, s => s.NumCasts > 0, "Safe corner");
        ComponentCondition<Quicksand>(id + 0x122, 0.46f, q => q.ActivationCount > 0, "Quicksand activate");

        Cast(id + 0x130, AID._Spell_PuddingGraf, 0.12f, 3);

        ComponentCondition<Quicksand>(id + 0x140, 0.9f, q => q.DisappearCount > 0, "Quicksand disappear");

        id += 0x10000;

        CastStart(id, AID._Weaponskill_DoubleStyle2, 3.55f);

        ComponentCondition<Quicksand>(id + 2, 1.5f, q => q.AppearCount > 1, "Quicksand appear");

        ComponentCondition<PuddingGraf>(id + 3, 7.4f, p => p.NumFinishedSpreads > 0, "Place bombs");

        ComponentCondition<Quicksand>(id + 4, 2.5f, q => q.ActivationCount > 1, "Quicksand activate");

        Cast(id + 8, AID._Spell_MousseMural, 1.6f, 5, "Raidwide");
        ComponentCondition<Quicksand>(id + 0xA, 0.3f, q => q.DisappearCount > 1, "Quicksand disappear");

        ColorRiot(id + 0x10, 2.8f);

        id += 0x10000;
        Cast(id, AID._Spell_SoulSugar, 5.2f, 3);
        Cast(id + 0x10, AID._Weaponskill_LivePainting, 3.25f, 4)
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<ReadyOreNot>()
            .ActivateOnEnter<ICraveViolence>()
            .ActivateOnEnter<HangryHiss>()
            .ActivateOnEnter<OreRigato>()
            .ActivateOnEnter<WaterIII>()
            .ActivateOnEnter<WaterIIITether>();
        ComponentCondition<Adds>(id + 0x12, 3.1f, c => c.YanCounter > 0, "Adds 1");

        Cast(id + 0x20, AID._Weaponskill_LivePainting1, 22.1f, 4);
        ComponentCondition<Adds>(id + 0x22, 3.1f, c => c.RayCounter > 0, "Adds 2");

        Cast(id + 0x30, AID._Weaponskill_LivePainting2, 15, 4);
        ComponentCondition<Adds>(id + 0x32, 3.1f, c => c.YanCounter > 1, "Adds 3");

        ComponentCondition<Adds>(id + 0x34, 8.1f, c => c.JabberwockCounter > 0, "Jabberwock 1");

        Cast(id + 0x40, AID._Weaponskill_ReadyOreNot, 11.9f, 7, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        Cast(id + 0x50, AID._Weaponskill_LivePainting3, 5.2f, 4);
        ComponentCondition<Adds>(id + 0x52, 3.1f, c => c.RayCounter > 2, "Adds 4");
        ComponentCondition<Adds>(id + 0x54, 8.1f, c => c.JabberwockCounter > 1, "Jabberwock 2")
            .ActivateOnEnter<SweetShot>();

        Cast(id + 0x60, AID._Weaponskill_ReadyOreNot, 56, 7, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);

        CastStart(id + 0x70, AID._Weaponskill_SingleStyle, 3.3f);
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

        Cast(id + 0x90, AID._Spell_MousseMural, 1.1f, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);

        id += 0x10000;
        Cast(id, AID._Weaponskill_Sugarscape1, 8.5f, 1, "Sugarscape")
            .ActivateOnEnter<WingmarkKB>()
            .ActivateOnEnter<SweetShot>();
        CastStartMulti(id + 0x10, [AID._Weaponskill_DoubleStyle9, AID._Weaponskill_DoubleStyle10], 14.1f)
            .ActivateOnEnter<DoubleStyle2>()
            .ActivateOnEnter<DoubleStyle2Arena>();
        ComponentCondition<SweetShot>(id + 0x12, 10.1f, s => s.NumCasts > 0, "Arrows");
        ComponentCondition<DoubleStyle2>(id + 0x14, 0.1f, d => d.NumCasts > 0, "Spread/stack");

        SimpleState(id + 0xFF0000, 9999, "???");
    }

    private State ColorRiot(uint id, float delay)
    {
        CastStartMulti(id, [AID._Weaponskill_ColorRiot, AID._Weaponskill_ColorRiot1], delay)
            .ActivateOnEnter<ColorRiot>();
        return ComponentCondition<ColorRiot>(id + 1, 7, tb => tb.NumCasts > 0, "Tankbuster")
            .DeactivateOnExit<ColorRiot>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void StickyMousse(uint id, float delay)
    {
        CastStart(id, AID._Weaponskill_StickyMousse, delay)
            .ActivateOnEnter<StickyMousse>()
            .ActivateOnEnter<StickyBurst>();
        CastEnd(id + 1, 5);
        ComponentCondition<StickyMousse>(id + 2, 0.8f, m => m.NumCasts > 0, "Spreads");
        ComponentCondition<StickyBurst>(id + 3, 6, b => b.NumCasts > 0, "Stacks")
            .DeactivateOnExit<StickyMousse>()
            .DeactivateOnExit<StickyBurst>();
    }

    //private void XXX(uint id, float delay)
}
