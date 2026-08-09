namespace BossMod.Endwalker.DeepDungeon.PilgrimsTraverse.DD99EminentGrief;

public enum OID : uint
{
    EminentGrief = 0x48EA, // R28.5
    EminentGriefHelper = 0x486D, // R1.0
    DevouredEater = 0x48EB, // R15.0
    VodorigaMinion = 0x48EC, // R1.2
    Crystal = 0x1EBE70, // R0.5
    BallOfFire = 0x48ED, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttackVisual1 = 44820, // EminentGriefHelper->player, 0.5s cast, single-target
    AutoAttackVisual2 = 44802, // EminentGriefHelper->player, 0.5s cast, single-target
    AutoAttackVisual3 = 44095, // DevouredEater->self, no cast, single-target
    AutoAttackVisual4 = 44094, // EminentGrief->self, no cast, single-target
    AutoAttack1 = 44813, // Helper->player, 0.8s cast, single-target
    AutoAttack2 = 44096, // Helper->player, 0.8s cast, single-target
    AutoAttackAdd = 45196, // VodorigaMinion->player, no cast, single-target

    ChainsOfCondemnationVisual1 = 44063, // EminentGrief->location, 5.3+0,7s cast, single-target
    ChainsOfCondemnationVisual2 = 44069, // EminentGrief->location, 8.3+0,7s cast, single-target
    ChainsOfCondemnation1 = 44064, // Helper->location, 6.0s cast, range 30 circle, applies chains of condemnation status
    ChainsOfCondemnation2 = 44070, // Helper->location, 9.0s cast, range 30 circle

    BladeOfFirstLightVisual1 = 44072, // DevouredEater->self, 8.2+0,8s cast, single-target
    BladeOfFirstLightVisual2 = 44071, // DevouredEater->self, 8.2+0,8s cast, single-target
    BladeOfFirstLightVisual3 = 44065, // DevouredEater->self, 5.2+0,8s cast, single-target
    BladeOfFirstLightVisual4 = 44066, // DevouredEater->self, 5.2+0,8s cast, single-target
    BladeOfFirstLight1 = 44073, // Helper->self, 9.0s cast, range 30 width 15 rect
    BladeOfFirstLight2 = 44067, // Helper->self, 6.0s cast, range 30 width 15 rect

    BoundsOfSinVisual = 44081, // DevouredEater->self, 3.3+0,7s cast, single-target
    BoundsOfSinPull = 44082, // Helper->self, 4.0s cast, range 40 circle, pull into middle
    BoundsOfSin = 44083, // Helper->self, 3.0s cast, range 3 circle
    BoundsOfSinEnd = 44084, // Helper->self, no cast, range 8 circle, aoe in middle

    SpinelashVisual1 = 44085, // EminentGrief->self, 2.0s cast, single-target
    SpinelashVisual2 = 44086, // EminentGrief->self, 1.0+0,8s cast, single-target
    Spinelash = 45118, // Helper->self, 1.8s cast, range 60 width 4 rect
    SpinelashEndVisual = 44087, // EminentGrief->self, no cast, single-target

    TerrorEye = 45115, // VodorigaMinion->location, 3.0s cast, range 6 circle
    BloodyClaw = 45114, // VodorigaMinion->player, no cast, single-target

    DrainAetherVisual1 = 44092, // DevouredEater->self, 11.0+1,0s cast, single-target
    DrainAetherVisual2 = 44090, // DevouredEater->self, 6.0+1,0s cast, single-target
    DrainAether1 = 44088, // EminentGrief->self, 7.0s cast, range 50 width 50 rect, need light
    DrainAether2 = 44093, // EminentGriefHelper->self, 12.0s cast, range 50 width 50 rect, need darkness
    DrainAether3 = 44091, // EminentGriefHelper->self, 7.0s cast, range 50 width 50 rect, need darkness
    DrainAether4 = 44089, // EminentGrief->self, 12.0s cast, range 50 width 50 rect, need light
    DrainAetherFail1 = 44270, // Helper->EminentGrief, no cast, single-target, grief gets healed up
    DrainAetherFail2 = 44314, // Helper->DevouredEater, no cast, single-target, eater gets healed up

    BallOfFireVisual1 = 44068, // EminentGrief->self, 9.0s cast, single-target
    BallOfFireVisual2 = 44061, // EminentGrief->self, 6.0s cast, single-target
    BallOfFire = 44062, // Helper->location, 2.1s cast, range 6 circle

    AbyssalBlazeVisualWE1 = 44074, // EminentGrief->self, 3.0s cast, single-target
    AbyssalBlazeVisualNS1 = 44075, // EminentGrief->self, 3.0s cast, single-target
    AbyssalBlazeVisualNS2 = 44077, // EminentGrief->self, no cast, single-target
    AbyssalBlazeVisualWE2 = 44076, // EminentGrief->self, no cast, single-target
    SpawnCrystal = 44078, // Helper->location, no cast, single-target
    AbyssalBlazeFirst = 44079, // Helper->location, 7.0s cast, range 5 circle
    AbyssalBlazeRest = 44080 // Helper->location, no cast, range 5 circle
}

public enum SID : uint
{
    ChainsOfCondemnation = 4562, // Helper->player, extra=0x0
    LightVengeance = 4560, // none->player, extra=0x0
    DarkVengeance = 4559 // none->player, extra=0x0
}

public enum IconID : uint
{
    Spinelash = 234 // player->self
}

[SkipLocalsInit]
sealed class LightAndDark(BossModule module) : LightAndDarkBase(module)
{
    private readonly DD99EminentGrief bossmod = (DD99EminentGrief)module;
    private int numPartyMembersHalf;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.DrainAether1:
                UpdateWantedStatus(0, true);
                break;
            case (uint)AID.DrainAether2:
                UpdateWantedStatus(1, false);
                break;
            case (uint)AID.DrainAether3:
                UpdateWantedStatus(0, false);
                break;
            case (uint)AID.DrainAether4:
                UpdateWantedStatus(1, true);
                break;
        }

        void UpdateWantedStatus(int slot, bool wantL)
        {
            wantLight[slot] = wantL;
            aetherdrainActive = true;
            activations[slot] = Module.CastFinishAt(spell, -1d);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.DrainAether1 or (uint)AID.DrainAether2 or (uint)AID.DrainAether3 or (uint)AID.DrainAether4)
        {
            if ((++NumCasts & 1) == 0)
            {
                aetherdrainActive = false;
            }
            wantLight[0] = wantLight[1];
            activations[0] = activations[1];
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.LightVengeance:
                UpdateStatus(ref lightBuff, actor, status.ExpireAt);
                break;
            case (uint)SID.DarkVengeance:
                UpdateStatus(ref darkBuff, actor, status.ExpireAt);
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.LightVengeance:
                UpdateStatus(ref lightBuff, actor, default);
                break;
            case (uint)SID.DarkVengeance:
                UpdateStatus(ref darkBuff, actor, default);
                break;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Module.StateMachine.ActivePhaseIndex == -1)
        {
            var countL = lightBuff.NumSetBits();
            var countD = darkBuff.NumSetBits();
            if (!combined[slot])
            {
                if (combined.NumSetBits() == 0 || countL == 1 && countD == 1)
                {
                    hints.Add(either);
                }
                else if (countL > countD)
                {
                    hints.Add(dark);
                }
                else
                {
                    hints.Add(light);
                }
            }
            else
            {
                if (numPartyMembersHalf == 0)
                {
                    numPartyMembersHalf = (int)MathF.Ceiling(Module.Raid.WithoutSlot(true, true, true).Length * 0.5f);
                }
                if (darkBuff[slot] && countD > numPartyMembersHalf || lightBuff[slot] && countL > numPartyMembersHalf)
                {
                    hints.Add(switchColor);
                }
            }
        }
        else if (aetherdrainActive)
        {
            if (wantLight[0])
            {
                hints.Add(light, !lightBuff[slot]);
            }
            else
            {
                hints.Add(dark, !darkBuff[slot]);
            }
        }
        else if (!aetherdrainActive)
        {
            if (eaterHP <= 1u && !lightBuff[slot])
            {
                hints.Add(light);
            }
            else if (griefHP <= 1u && !darkBuff[slot])
            {
                hints.Add(dark);
            }
            else if (Math.Abs(hpDifference) > 25f || !combined[slot])
            {
                if (hpDifference > 0f)
                {
                    hints.Add(dark);
                }
                else
                {
                    hints.Add(light);
                }
            }
            else
            {
                hints.Add($"Target {(darkBuff[slot] ? bossmod.BossEater?.Name : Module.PrimaryActor.Name)}!", false);
            }
        }
    }

    public override void Update()
    {
        if (bossmod.BossEater is Actor eater)
        {
            ref var eaterHPref = ref eater.HPMP;
            ref var primaryHPref = ref Module.PrimaryActor.HPMP;
            var eaterHPs = eaterHP = eaterHPref.CurHP;
            var griefHPs = griefHP = primaryHPref.CurHP;
            hpDifference = (int)(eaterHPs - griefHPs) * 100f / primaryHPref.MaxHP;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            var oid = e.Actor.OID;
            ref var hp = ref e.Actor.HPMP;
            e.Priority = hp.CurHP <= 1u ? AIHints.Enemy.PriorityInvincible : darkBuff[slot] && oid == (uint)OID.DevouredEater ? 0
                : lightBuff[slot] && oid == (uint)OID.EminentGrief ? 0 : oid == (uint)OID.VodorigaMinion ? 1 : AIHints.Enemy.PriorityInvincible;
        }

        base.AddAIHints(slot, actor, assignment, hints);
    }
}

[SkipLocalsInit]
sealed class TerrorEyeBallOfFire(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.TerrorEye, (uint)AID.BallOfFire], 6f);

[SkipLocalsInit]
sealed class ChainsOfCondemnation(BossModule module) : Components.StayMove(module, 2d)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ChainsOfCondemnation1 or (uint)AID.ChainsOfCondemnation2)
        {
            var act = Module.CastFinishAt(spell, 0.1d);
            Array.Fill(PlayerStates, new(Requirement.Stay2, act));
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.ChainsOfCondemnation && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            PlayerStates[slot] = default;

            // ensure players who never got the debuff are properly reset
            var party = Raid.WithSlot(true, false, false);
            var len = party.Length;
            for (var i = 0; i < len; ++i)
            {
                ref var p = ref party[i];
                if (p.Item2.IsDead)
                {
                    PlayerStates[p.Item1] = default;
                }
            }
        }
    }
}

[SkipLocalsInit]
sealed class BladeOfFirstLight(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BladeOfFirstLight1, (uint)AID.BladeOfFirstLight2], new AOEShapeRect(30f, 7.5f));

[SkipLocalsInit]
sealed class BoundsOfSinSmallAOE : Components.SimpleAOEs
{
    public BoundsOfSinSmallAOE(BossModule module) : base(module, (uint)AID.BoundsOfSin, 3f)
    {
        MaxDangerColor = 3;
    }
}

[SkipLocalsInit]
sealed class BoundsOfSinEnd(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private readonly AOEShapeCircle circle = new(8f);
    private readonly List<Polygon> pillars = [with(12)];
    private const float radius = 2.57745f; // adjusted for hitbox radius

    private readonly Polygon[] pillarPolygons =
    [
        new(new(-600f, -306.99991f), radius, 64), // ENVC 0x00
        new(new(-596.50012f, -306.06201f), radius, 64, -30f.Degrees()), // ENVC 0x01
        new(new(-593.93793f, -303.49991f), radius, 64, -60f.Degrees()), // ENVC 0x02
        new(new(-592.99988f, -299.99991f), radius, 64, -89.98f.Degrees()), // ENVC 0x03
        new(new(-593.93781f, -296.49991f), radius, 64, -120f.Degrees()), // ENVC 0x04
        new(new(-596.50012f, -293.93771f), radius, 64, -150f.Degrees()), // ENVC 0x05
        new(new(-600f, -293f), radius, 64), // ENVC 0x06
        new(new(-603.5f, -293.93781f), radius, 64, 150f.Degrees()), // ENVC 0x07
        new(new(-606.06219f, -296.5f), radius, 64, 120f.Degrees()), //  ENVC 0x08
        new(new(-607.00012f, -300f), radius, 64, 89.98f.Degrees()), //  ENVC 0x09
        new(new(-606.06219f, -303.5f), radius, 64, 60f.Degrees()), // ENVC 0x0A
        new(new(-603.5f, -306.06219f), radius, 64, 30f.Degrees()) // ENVC 0x0B
    ];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BoundsOfSinPull)
        {
            var loc = Arena.Center.Quantized();
            _aoe = [new(circle, loc, default, WorldState.FutureTime(6d), shapeDistance: circle.Distance(loc, default))]; // true activation is 11.7, but slightly lower values seem to improve the pathfinding here
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.BoundsOfSinEnd)
        {
            _aoe = [];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is >= 0x00 and <= 0x0B)
        {
            if (state == 0x00020001u)
            {
                pillars.Add(pillarPolygons[index]);
                Arena.Bounds = new ArenaBoundsCustom([new Rectangle(new(-600f, -300f), 20f, 15f)], [.. pillars]);
            }
            else if (index == 0x00 && state == 0x00080004u)
            {
                Arena.Bounds = DD99EminentGrief.BuildArena().arena;
                pillars.Clear();
            }
        }
    }
}

[SkipLocalsInit]
sealed class SpinelashBait(BossModule module) : Components.GenericBaitAway(module)
{
    private readonly AOEShapeRect rect = new(60f, 2f);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Spinelash)
        {
            CurrentBaits.Add(new(Module.PrimaryActor, actor, rect, WorldState.FutureTime(6.3d), customRotation: Angle.AnglesCardinals[1]));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Spinelash)
        {
            CurrentBaits.Clear();
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) { } // handled by hint component

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { }  // not really needed until cast starts
}

[SkipLocalsInit]
sealed class SpinelashBaitHint(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private Actor? target;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Spinelash)
        {
            target = actor;
            var pos = Arena.Center;
            AOEShapeCustom shape = new(pos, [new Rectangle(new(-593f, -300f), 1f, 16f), new Rectangle(new(-607f, -300f), 1f, 16f)]);
            _aoe = [new(shape, pos, default, WorldState.FutureTime(6.3d), Colors.SafeFromAOE, shapeDistance: shape.InvertedDistance(pos, default))];
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Spinelash)
        {
            target = null;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (target == actor)
        {
            ref var aoe = ref _aoe[0];
            hints.Add("Avoid breaking windows!", aoe.Check(actor.Position));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (target == actor)
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (target == pc)
        {
            base.DrawArenaBackground(pcSlot, pc);
        }
    }
}

[SkipLocalsInit]
sealed class Spinelash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Spinelash, new AOEShapeRect(60f, 2f));

[SkipLocalsInit]
sealed class BallOfFire(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private readonly AOEShapeCircle circle = new(6f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.BallOfFireVisual1:
            case (uint)AID.BallOfFireVisual2:
                var party = Raid.WithoutSlot(false, true, true);
                var len = party.Length;
                var act = Module.CastFinishAt(spell, 0.1d);
                for (var i = 0; i < len; ++i)
                {
                    CurrentBaits.Add(new(Module.PrimaryActor, party[i], circle, act));
                }
                break;
            case (uint)AID.BallOfFire:
                CurrentBaits.Clear();
                break;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) { } // handled by hint component

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { }  // not really needed until cast starts
}

[SkipLocalsInit]
sealed class AbyssalBlaze(BossModule module) : Components.Exaflare(module, 5f)
{
    private readonly List<(WDir, WPos)> crystals = [with(8)];
    private AOEShapeCustom? shape;
    private WDir next;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AbyssalBlazeFirst)
        {
            var count = crystals.Count;
            var pos = caster.Position;
            var crys = CollectionsMarshal.AsSpan(crystals);

            for (var i = 0; i < count; ++i)
            {
                ref var c = ref crys[i];
                var loc = c.Item2;
                if (loc.AlmostEqual(pos, 1f))
                {
                    var dir = c.Item1;
                    var offset = loc - Arena.Center;
                    var intersect = (int)Intersect.RayAABB(offset, dir, 18.1f, 14.1f);
                    var maxexplosions = intersect / 4 + 1;
                    var act = Module.CastFinishAt(spell);
                    Lines.Add(new(loc, 4f * dir, act, 1d, maxexplosions, maxexplosions, rotation: dir.ToAngle()));
                    intersect = (int)Intersect.RayAABB(offset, -dir, 18.1f, 14.1f);
                    maxexplosions = intersect / 4 + 1;
                    Lines.Add(new(loc, -4f * dir, act, 1d, maxexplosions, maxexplosions, rotation: (-dir).ToAngle()));
                    if (Lines.Count == 16)
                    {
                        crystals.Clear();
                        shape = null;
                    }
                    return;
                }
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Crystal)
        {
            crystals.Add((next, actor.Position));
            if (crystals.Count == 8)
            {
                var rects = new Rectangle[8];
                for (var i = 0; i < 8; ++i)
                {
                    var c = crystals[i];
                    rects[i] = new(c.Item2, 5f, 40f, c.Item1.ToAngle());
                }
                var center = Arena.Center;
                shape = new(center, [new Rectangle(center, 20f, 15f)], rects);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (shape == null)
        {
            return;
        }

        shape.Outline(Arena, Arena.Center, default, Colors.Safe, 2f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.AbyssalBlazeVisualWE1:
            case (uint)AID.AbyssalBlazeVisualWE2:
                next = new(1f, default);
                break;
            case (uint)AID.AbyssalBlazeVisualNS1:
            case (uint)AID.AbyssalBlazeVisualNS2:
                next = new(default, 1f);
                break;
            case (uint)AID.AbyssalBlazeFirst:
                var pos = caster.Position;

                var count = Lines.Count - 1;
                for (var i = count; i >= 0; --i)
                {
                    var line = Lines[i];
                    if (line.Next.AlmostEqual(pos, 0.1f) && (WorldState.CurrentTime - line.NextExplosion).TotalSeconds < 0.5d)
                    {
                        AdvanceLine(line, pos);
                        if (line.ExplosionsLeft == 0)
                        {
                            Lines.RemoveAt(i);
                        }
                    }
                }
                break;
            case (uint)AID.AbyssalBlazeRest:
                var pos2 = spell.TargetXZ;

                var count2 = Lines.Count;
                for (var i = 0; i < count2; ++i)
                {
                    var line = Lines[i];
                    if (line.Next.AlmostEqual(pos2, 0.1f) && caster.Rotation.AlmostEqual(line.Rotation, Angle.DegToRad))
                    {
                        AdvanceLine(line, pos2);
                        if (line.ExplosionsLeft == 0)
                        {
                            Lines.RemoveAt(i);
                        }
                        return;
                    }
                }
                break;
        }
    }
}

[SkipLocalsInit]
sealed class DD99EminentGriefStates : StateMachineBuilder
{
    public DD99EminentGriefStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SpinelashBaitHint>()
            .ActivateOnEnter<ChainsOfCondemnation>()
            .ActivateOnEnter<BladeOfFirstLight>()
            .ActivateOnEnter<BoundsOfSinSmallAOE>()
            .ActivateOnEnter<BoundsOfSinEnd>()
            .ActivateOnEnter<Spinelash>()
            .ActivateOnEnter<SpinelashBait>()
            .ActivateOnEnter<TerrorEyeBallOfFire>()
            .ActivateOnEnter<BallOfFire>()
            .ActivateOnEnter<AbyssalBlaze>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport,
StatesType = typeof(DD99EminentGriefStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = typeof(SID),
TetherIDType = null,
IconIDType = typeof(IconID),
PrimaryActorOID = (uint)OID.EminentGrief,
Contributors = "The Combat Reborn Team (Malediktus)",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.DeepDungeon,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1041u,
NameID = 14037u,
SortOrder = 1,
PlanLevel = 0)]
[SkipLocalsInit]
public sealed class DD99EminentGrief : BossModule // module also works in Final Verse normal, everything but the zone ID seem to be identical
{
    public DD99EminentGrief(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private DD99EminentGrief(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena)
    {
        ActivateComponent<LightAndDark>();
        FindComponent<LightAndDark>()!.AddAOE();
        vodorigas = Enemies((uint)OID.VodorigaMinion);
    }

    public static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Rectangle(new(-600f, -300f), 20f, 15f)], AdjustForHitboxOutwards: true);
        return (arena.Center, arena);
    }

    public Actor? BossEater;
    private readonly List<Actor> vodorigas;

    protected override void UpdateModule()
    {
        BossEater ??= GetActor((uint)OID.DevouredEater);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(vodorigas);
    }
}
