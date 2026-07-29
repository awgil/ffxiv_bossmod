namespace BossMod.Shadowbringers.Trial.T03Hades;

public enum OID : uint
{
    Hades = 0x2AA2, // R0.000, x?
    HadesBig = 0x294A, // R20.000, x? -> Big
    Hades2 = 0x294C, // R0.000, x?
    Hades3 = 0x233C, // R0.500, x?, Helper type
    HadesBoss = 0x2949, // R6.750, x?
    Actor1ea1a1 = 0x1EA1A1, // R2.000, x?, EventObj type
    Hades5 = 0x29DB, // R1.000, x?, Part type
    Hades6 = 0x29D9, // R1.000, x?, Part type
    Hades7 = 0x29DA, // R1.000, x?, Part type
    Hades8 = 0x29D8, // R1.000, x?, Part type
    BrokenFaithObject = 0x1EAD28, // R0.500, x?, EventObj type
    ShadowOfTheAncients = 0x27AE, // R1.250, x?
    ShadowOfTheAncients1 = 0x27B7, // R1.250, x?
    AetherialGaol = 0x294B, // R8.000, x?
    DoomTower = 0x1EAD29, // R0.500, x?, EventObj type
}

public enum AID : uint
{
    _AutoAttack = 872, // 2949->player, no cast, single-target
    RavenousAssault = 16728, // 2949->player, 5.0s cast, range 5 circle
    BadFaith = 16715, // 233C->self, 5.0s cast, range 20 width 20 rect
    BadFaith1 = 16716, // 233C->self, 5.0s cast, range 20 width 20 rect
    BadFaithVisual = 16713, // 2949->self, 5.0s cast, single-target
    BadFaithVisual1 = 16714, // 2949->self, 5.0s cast, single-target
    Double = 16719, // 2949->self, 3.0s cast, single-target
    DarkEruptionVisual = 16720, // 2949->self, 3.0s cast, single-target
    DarkEruption = 16723, // 233C->location, no cast, range 6 circle
    DarkEruptionAOE = 16722, // 233C->location, 3.0s cast, range 6 circle
    DarkEruption3 = 16721, // 2949->self, no cast, single-target
    BrokenFaith = 16717, // 2949->self, 3.0s cast, single-target
    BrokenFaith1 = 16718, // 233C->self, no cast, range 10 circle
    ShadowSpread = 16726, // 233C->self, 3.0s cast, range 40 30.000-degree cone
    ShadowSpread1 = 16724, // 2949->self, 3.0s cast, single-target
    ShadowSpread2 = 16725, // 2949->self, no cast, single-target
    ShadowSpread3 = 16727, // 233C->self, 3.0s cast, range 40 30.000-degree cone
    _Spell_ = 17817, // 2949->self, no cast, single-target
    _Spell_1 = 17816, // 233C->self, no cast, single-target
    AncientRuin = 17814, // 27AE->player, no cast, single-target
    AncientWaterIII = 17812, // 27B7->players, 5.0s cast, range 6 circle
    AncientDarkness = 17811, // 27B7->player, 5.0s cast, range 5 circle
    AncientAero = 17813, // 27B7->self, 5.0s cast, range 40 width 8 rect
    AncientDarkIV = 17815, // 2949->self, 5.0s cast, range 100 circle
    _AutoAttack_ = 16764, // 294A->player, no cast, single-target
    Titanomachy = 16768, // 294A->self, 4.0s cast, range 100 circle
    ShadowStream = 16732, // 294A->self, 5.0s cast, range 100 width 16 rect
    DualStrike = 16738, // 233C->player, 5.0s cast, range 5 circle
    DualStrike1 = 16737, // 294A->self, 5.0s cast, single-target
    EchoOfTheLost = 16740, // 294A->self, 7.0s cast, range 100 ?-degree cone
    PolydegmonsPurgation = 16754, // 233C->self, 5.0s cast, range 100 width 16 rect
    PolydegmonsPurgation1 = 16753, // 233C->self, 5.0s cast, range 100 width 16 rect
    PolydegmonsPurgation2 = 16752, // 294A->self, 5.0s cast, single-target
    HellbornYawp = 16750, // 294A->self, 5.0s cast, single-target
    HellbornYawp1 = 16751, // 233C->self, 4.0s cast, range 100 60.000-degree cone
    EchoOfTheLost1 = 16739, // 294A->self, 7.0s cast, range 100 ?-degree cone
    Captivity = 16744, // 294A->self, 5.0s cast, single-target
    CaptivityCast = 16745, // 233C->player, no cast, range 8 circle
    __ = 16767, // 233C->self, no cast, single-target
    _Spell_2 = 16746, // 233C->player, no cast, single-target
    _Spell_3 = 16747, // 294A->self, no cast, single-target
    _Spell_4 = 16749, // 294A->self, no cast, single-target
    NetherBlast = 16755, // 29D8/29D9/29DA/29DB->player, no cast, range 6 circle
    LifeInCaptivity = 16757, // 294A->self, 4.0s cast, range 100 circle
    _Spell_5 = 17452, // 233C->player, no cast, single-target
    BlackCauldron = 16758, // 294A->self, no cast, single-target
    BlackCauldron1 = 16730, // 233C->self, no cast, range 100 circle
    TheDarkDevours = 16759, // 294A->self, 3.0s cast, single-target
    TheDarkDevours1 = 16761, // 233C->self, no cast, range 100 circle
    TheDarkDevours2 = 16762, // 233C->self, no cast, range 100 circle
    ChorusOfTheLost = 16748, // 294A->self, 30.0s cast, range 100 circle
}

public enum SID : uint
{
    Doom = 210, // none->player, extra=0x0
    VulnerabilityUp = 202, // Hades3/HadesBig->player, extra=0x1/0x2/0x3/0x4
    _Gen_ = 2056, // none->ShadowOfTheAncients/ShadowOfTheAncients1, extra=0x7D
    Double = 661, // HadesBoss->HadesBoss, extra=0x0
    Stun = 149, // Hades3->player, extra=0x0
    Fetters = 770, // none->player, extra=0x0
    Fetters1 = 1614, // HadesBig->player, extra=0x181E
    LightBeyondDarkness = 1929, // none->player, extra=0x0
    LightInTheDark = 1904, // none->player, extra=0x0
    Bleeding = 320, // none->player, extra=0x0
}

public enum IconID : uint
{
    Tankbuster = 343, //Player -> self : vfx tank_lockonae_5m_5s_01k1
    SpreadIcon = 139, //Player -> self : vfx target_ae_s5f
    _Gen3 = 62, //Player -> self : vfx com_share0c
    _Gen4 = 96, //Player -> self : vfx loc05sp_05af
    _Gen5 = 40, // Player -> self : vfx m0117_earth_shake_01s
    CaptivityBait = 120, //Player - Self : vfx loc08sp_05at
}

public enum TetherID : uint
{
    DottedLineTether = 17 // Player -> 29D8/29D9/29DA/29DB (Hades Parts) : vfx chn_tergetfix1f
}

// Phase 1
// Ravenous Assault Tankbuster
sealed class RavenousAssault(BossModule module) : Components.BaitAwayCast(module, (uint)AID.RavenousAssault, new AOEShapeCircle(5f), centerAtTarget: true, endsOnCastEvent: true, tankbuster: true);

sealed class BadFaith(BossModule module)
    : Components.SimpleAOEGroups(module, [(uint)AID.BadFaith, (uint)AID.BadFaith1], new AOEShapeRect(20f, 10f));

// Dark Eruption : 5 players spread from mark : can be doubled. cast delay is an estimate
sealed class DarkEruptionSpread(BossModule module) : Components.BaitAwayIcon(module, 6f, (uint)IconID.SpreadIcon,
    (uint)AID.DarkEruption, 3f, source: module.PrimaryActor)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        // Account for netherblast because the same SpreadIcon is used for both.
        // this prevents the bait circle from staying on radar all fight long.
        if (CurrentBaits.Count != 0 && spell.Action.ID is (uint)AID.DarkEruption or (uint)AID.NetherBlast)
        {
            CurrentBaits.RemoveAt(0);
        }
    }
}

sealed class DarkEruptionCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DarkEruptionAOE, new AOEShapeCircle(6f));

sealed class ShadowSpreadCones(BossModule module)
    : Components.SimpleAOEGroups(module, [(uint)AID.ShadowSpread, (uint)AID.ShadowSpread3], new AOEShapeCone(40f, 15f.Degrees()));

// Broken Faith - causes a random pattern of aetheric (glowing blue) circles to slowly transcend. Explode on landing
sealed class BrokenFaith(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BrokenFaith1, new AOEShapeCircle(10f))
{
    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID is (uint)OID.BrokenFaithObject && state == 0x00010002u && actor.OID != (uint)OID.Actor1ea1a1)
            Casters.Add(new(Shape, actor.Position, actor.Rotation, WorldState.FutureTime(8.5f), shapeDistance: Shape.Distance(actor.Position, actor.Rotation)));
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.BrokenFaithObject)
        {
            Casters.RemoveAll(cast => cast.Origin == actor.Position);
        }
    }
}

// Phase 2 : adds
//AncientWaterIII = 17812, // 27B7->players, 5.0s cast, range 6 circle stack.
sealed class AncientWaterIII(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.AncientWaterIII, 6f, 2, 8);

sealed class AncientDarkness(BossModule module) : Components.BaitAwayCast(module, (uint)AID.AncientDarkness, new AOEShapeCircle(5f), centerAtTarget: true);

sealed class AncientAero(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AncientAero, new AOEShapeRect(40f, 4f));

sealed class AncientDarkIV(BossModule module) : Components.RaidwideCast(module, (uint)AID.AncientDarkIV);

//Phase 3 Giant Hades
sealed class Titanomachy(BossModule module) : Components.RaidwideCast(module, (uint)AID.Titanomachy);

//ShadowStream shoots ball down center of arena. Polydegmon is inverse of that.
sealed class AOERectangles(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ShadowStream, (uint)AID.PolydegmonsPurgation, (uint)AID.PolydegmonsPurgation1], new AOEShapeRect(100f, 8f));

sealed class DualStrike(BossModule module) : Components.BaitAwayCast(module, (uint)AID.DualStrike, new AOEShapeCircle(5f), centerAtTarget: true, endsOnCastEvent: true, damageType: AIHints.PredictedDamageType.Tankbuster);

//EchoOfTheLost range 100 ?-degree cone. Big Hades raises arm and swipes across arena. Angle is an estimate
sealed class EchoOfTheLost(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EchoOfTheLost, new AOEShapeCone(100f, 45f.Degrees()));

sealed class EchoOfTheLostCone(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EchoOfTheLost1, new AOEShapeCone(100f, 45f.Degrees()));

// targets both tanks (two highest enmity targets) with earthshaker markers, indicating that a cone-AoE will be
// aimed from Hades in the direction of each marker. Marked players can bait the attack away from group
sealed class HellbornYawp(BossModule module) : Components.BaitAwayCast(module, (uint)AID.HellbornYawp1, new AOEShapeCone(100f, 30f.Degrees()), centerAtTarget: true, tankbuster: true);

// Party member gets the Captivity icon and baits away. The cast happens and they become stunned. Then they are drawn in to the Gaol after.
sealed class CaptivityBait(BossModule module) : Components.BaitAwayIcon(module, 8f, (uint)IconID.CaptivityBait, (uint)AID.CaptivityCast, 4.7d);

//TODO this might need some logic for if you are the one in gaol.
sealed class AetherialGaol(BossModule module) : BossComponent(module)
{
    private Actor? _gaoler;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Module.Enemies((uint)OID.AetherialGaol), Colors.Enemy);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        _gaoler = Module.Enemies((uint)OID.AetherialGaol).FirstOrDefault();
        base.AddAIHints(slot, actor, assignment, hints);
        var attackRadius = 8;

        if (_gaoler != null && !_gaoler.IsDeadOrDestroyed)
        {
            for (var i = 0; i < hints.PotentialTargets.Count; ++i)
            {
                var e = hints.PotentialTargets[i];
                e.Priority = e.Actor.OID switch
                {
                    (uint)OID.AetherialGaol => 1,
                    (uint)OID.HadesBig => AIHints.Enemy.PriorityInvincible,
                    _ => 0
                };
            }

            var pos = _gaoler.Position;
            WPos optimalAttackPosition = new(pos.X, pos.Z + 1);
            // move towards gaol. Should probably account for being in the gaol and unable to move.
            hints.GoalZones.Add(AIHints.GoalSingleTarget(optimalAttackPosition, attackRadius - 2, 10));
        }
    }
}

/*Doom - causes five bright circles to appear within the arena. Simultaneously, the platform itself will visibly
 turn black, and all members of the raid will acquire an unavoidable Doom debuff. To cleanse the debuff,all circles
 will need to be occupied by at least one player. Once done, the raid will be cleansed of all Doom debuffs.
*/
sealed class DoomTowers(BossModule module) : Components.GenericTowers(module)
{
    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.DoomTower)
        {
            Towers.Add(new(actor.Position, 5f, 1, 2));
        }
    }

    // A fallback tower removal in case all the towers were soaked
    public override void OnActorDestroyed(Actor actor)
    {
        base.OnActorDestroyed(actor);
        if (actor.OID == (uint)OID.DoomTower)
        {
            Towers.Clear();
        }
    }
}

/* TODO : Wall of the lost was on the wiki page, but I have not seen it in my fights.
 * knockback ability apparently.
 */

// Hades parts put tethers on party members and shoot a 6f circle aoe at them.
sealed class NetherBlast(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCircle(6f),
    (uint)TetherID.DottedLineTether, (uint)AID.NetherBlast, activationDelay: 4.7d, centerAtTarget: true)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == WatchedAction && CurrentBaits.Count == 0)
        {
            activation = default;
        }
        if (spell.Action.ID == WatchedAction && CurrentBaits.Count != 0)
        {
            // remove bait if player dies during tether
            CurrentBaits.RemoveAll(tether => tether.Target.IsDead);
        }
    }
}

sealed class LastPhaseRaidwides(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.LifeInCaptivity, (uint)AID.BlackCauldron, (uint)AID.TheDarkDevours, (uint)AID.ChorusOfTheLost]);

[SkipLocalsInit]
sealed class HadesStates : StateMachineBuilder
{
    public HadesStates(BossModule module) : base(module)
    {
        TrivialPhase()
            // Phase 1
            .ActivateOnEnter<RavenousAssault>()
            .ActivateOnEnter<BadFaith>()
            .ActivateOnEnter<DarkEruptionSpread>()
            .ActivateOnEnter<DarkEruptionCircle>()
            .ActivateOnEnter<ShadowSpreadCones>()
            .ActivateOnEnter<BrokenFaith>()
            // Phase 2 adds
            .ActivateOnEnter<AncientWaterIII>()
            .ActivateOnEnter<AncientDarkness>()
            .ActivateOnEnter<AncientAero>()
            .ActivateOnEnter<AncientDarkIV>()
            // Phase 3 "I am stifled by this vessel of flesh"
            // Switches to giant hades
            .ActivateOnEnter<Titanomachy>()
            .ActivateOnEnter<AOERectangles>()
            .ActivateOnEnter<DualStrike>()
            .ActivateOnEnter<EchoOfTheLost>()
            .ActivateOnEnter<HellbornYawp>()
            .ActivateOnEnter<EchoOfTheLostCone>()
            .ActivateOnEnter<LastPhaseRaidwides>()
            .ActivateOnEnter<CaptivityBait>()
            .ActivateOnEnter<AetherialGaol>()
            .ActivateOnEnter<DoomTowers>()
            .ActivateOnEnter<NetherBlast>()
            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed && module.Enemies((uint)OID.HadesBig).All(k => k.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(HadesStates),
    ConfigType = null, // replace null with typeof(HadesConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = typeof(SID),
    TetherIDType = typeof(TetherID),
    IconIDType = typeof(IconID),
    PrimaryActorOID = (uint)OID.HadesBoss,
    Contributors = "wen",
    Expansion = BossModuleInfo.Expansion.Shadowbringers,
    Category = BossModuleInfo.Category.Trial,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 687u,
    NameID = 8352u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class Hades(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsCircle(20f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actors(WorldState.Actors.Where(a => !a.IsAlly), Colors.Enemy);
    }
}
