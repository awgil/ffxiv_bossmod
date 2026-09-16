namespace BossMod.Shadowbringers.Raid.E02Voidwalker;
//TODO: Determine why AI is not looking away from either Gaze mechanic.  Doesn't stop fight from being cleared MINE.

public enum OID : uint
{
    Boss = 0x290C,
    Helper = 0x233C,
    VoidwalkerPart = 0x2912, // R0.520, x1, Part type
    Nyx = 0x290D, // R1.503, x0 untargettable adds, don't touch
    TheHandOfErebos = 0x290E, // R24.000, x0 (spawn during fight)
    BleedTower = 0x1EAC9A, // single player towers. causes bleeding while soaking
}

public enum AID : uint
{
    DoomvoidGuillotine = 15931, // Boss->self, 4.0s cast, range 50 width 10 rect
    DarkFireIIINormal = 15939, // Helper->players, no cast, range 8 circle
    UnholyDarknessNormal = 15936, // Helper->players, no cast, range 6 circle
    EntropySummonBleedTowers = 15981, // Boss->self, 6.0s cast, single-target, casts shortly before towers - does it do damage?
    EntropyRaidwide = 15982, // Helper->self, 6.5s cast, range 50 circle
    EmptyHateKnockback = 15941, // 290E->self, 5.5s cast, single-target
    EmptyHateDamage = 15942, // Helper->self, 6.0s cast, range 50 width 40 rect
    PunishingRay = 15943, // Helper->self, no cast, range 80 circle, tower check, small damage if success.
    UnholyDarknessDelayed = 15937, // Helper->player, no cast, single-target
    DarkFireIIIDelayed = 15940, // Helper->player, no cast, single-target
    ShadoweyeUnknown = 16567, // Helper->self, no cast, ???
    ShadoweyeNormal = 16566, //Boss->self
    ShadoweyeDelayed = 16568, // Helper->player, no cast, single-target
    DoomvoidSlicer = 15932, // Boss->self, 4.0s cast, range ?-30 donut
    Shadowflame = 15950, // 2912->player, 4.0s cast, single-target
    Shadowflame1 = 15949, // Boss->player, 4.0s cast, single-target
}

public enum SID : uint
{
    SpellInWaitingUnholyDarkness = 1809, // none->player, extra=0x0
    SpellInWaitingDarkFireIII = 1810, // none->player, extra=0x0
    SpellInWaitingShadoweye = 1812, // none->player, extra=0x0
}

public enum IconID : uint
{
    DarkFireIII = 76, // player->self Fire 3 spread marker, not stored/delayed.
    UnholyDarkness = 62, // player->self  stack on player, not stored/delayed
    ShadowEye = 179, // player->self, gaze
}

class EmptyHateKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, AID.EmptyHateKnockback, 25, kind: Kind.DirForward)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var s in Sources(slot, actor))
        {
            if (s.Origin.X > 100)
            {
                hints.AddForbiddenZone(new AOEShapeRect(20, 15, 20), Arena.Center - new WDir(2, 0), default, s.Activation);
            }
            else
            {
                hints.AddForbiddenZone(new AOEShapeRect(20, 15, 20), Arena.Center + new WDir(2, 0), default, s.Activation);
            }
        }
    }
}

class DarkFireIIINormal(BossModule module) : Components.IconStackSpread(module, 0, (uint)IconID.DarkFireIII, null, AID.DarkFireIIINormal, 0, 8, 0);

class UnholyDarknessNormal(BossModule module) : Components.IconStackSpread(module, (uint)IconID.UnholyDarkness, 0, AID.UnholyDarknessNormal, null, 6, 0, 0);

//Stubs for missing implementation. This mechanic happens very late into the fight, and may not have to deal with it.
//Look away from the person with the gaze icon, immediately.
//Icon appears before spell cast. Spell cast target seems not to work in default CastGaze component. Spell cast IS useful for the timing of icon decay however.
//This works on radar, and a player-controlled character automatically gazes away with the VBM setting, but AI-controlled characters seem to ignore it.
//might need to add some kind of AI hint, or it might be an autorotation setting somewhere
class ShadowEye(BossModule module) : Components.GenericGaze(module)
{
    public Actor? ShadowEyeTarget;
    public DateTime Activation;
    public override IEnumerable<Eye> ActiveEyes(int slot, Actor actor)
    {
        List<Eye> activeEyes = [];
        if (ShadowEyeTarget != null)
        {
            activeEyes.Add(new Eye(ShadowEyeTarget.Position, Activation));
        }
        return activeEyes;
    }
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.ShadowEye)
        {
            ShadowEyeTarget = actor;
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action.ID == (uint)AID.ShadoweyeNormal)
        {
            Activation = Module.CastFinishAt(spell);
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if (spell.Action.ID == (uint)AID.ShadoweyeNormal)
        {
            ShadowEyeTarget = null;
        }
    }
}

//When the Spell-in-waiting: Shadoweye debuff expires, everyone must look away from that person.
class SpellInWaitingShadowEye(BossModule module) : Components.GenericGaze(module)
{
    public Actor? ShadowEyeTarget;
    public DateTime Activation;
    public override IEnumerable<Eye> ActiveEyes(int slot, Actor actor)
    {
        List<Eye> activeEyes = [];
        if (ShadowEyeTarget != null)
        {
            activeEyes.Add(new Eye(ShadowEyeTarget.Position, Activation));
        }
        return activeEyes;
    }
    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if (status.ID == (uint)SID.SpellInWaitingShadoweye)
        {
            ShadowEyeTarget = actor;
            Activation = status.ExpireAt;
        }
    }
    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if (status.ID == (uint)SID.SpellInWaitingShadoweye)
        {
            ShadowEyeTarget = null;
        }
    }
}

class SpellInWaitingStackSpread(BossModule module) : Components.GenericStackSpread(module)
{
    public float StackRadius = 6;
    public float SpreadRadius = 8;
    public int MinStackSize = 4;
    public int MaxStackSize = 8;

    public IEnumerable<Actor> ActiveStackTargets => ActiveStacks.Select(s => s.Target);
    public IEnumerable<Actor> ActiveSpreadTargets => ActiveSpreads.Select(s => s.Target);

    public void AddStack(Actor target, DateTime activation = default, BitMask forbiddenPlayers = default) => Stacks.Add(new(target, StackRadius, MinStackSize, MaxStackSize, activation, forbiddenPlayers));
    public void AddSpread(Actor target, DateTime activation = default) => Spreads.Add(new(target, SpreadRadius, activation));

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.SpellInWaitingDarkFireIII:
                AddSpread(actor, status.ExpireAt);
                break;
            case SID.SpellInWaitingUnholyDarkness:
                AddStack(actor, status.ExpireAt);
                break;
        }
    }
    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.SpellInWaitingDarkFireIII:
                Spreads.Clear();
                break;
            case SID.SpellInWaitingUnholyDarkness:
                Stacks.Clear();
                break;

        }
    }
}

//Source of confusion: There are actually two waves of bleed towers that happen at once, so 5 bleed towers (almost the same as invertible void zones) can amount to 10 actors with different spawn times.
//6 seconds after the first set spawns, another set spawns in the same place, presumably doubling the damage players take? although maybe not.
//approximately 6 seconds after that, the towers must be resolved by the time Punishing Ray is instantly cast.
//we were tracking the tower objects as the target to stand in, but instead, must track the distinct positions of the towers.
//Additional complication: OnActorCreated building a towers list ends up with different orders on different clients.  M1 might get assigned to top left on one client, and middle left on another, despite being M1 on both.
//To be consistent, we need to sort them either by position or instanceID.
//Since we are tracking positions instead of actors, it is better to sort by X,Z, maybe?
class BleedTower(BossModule module) : Components.GenericTowers(module)
{
    public List<Tower> futureTowers = [];
    public DateTime activationTime;

    public float waitForTimeRemaining = 9;

    public float towerLifespanSeconds = 12;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.BleedTower)
        {
            var towerAlreadyInPosition = false;
            foreach (var tower in futureTowers)
            {
                if (tower.Position.X == actor.Position.X && tower.Position.Z == actor.Position.Z)
                {
                    towerAlreadyInPosition = true;
                }
            }
            if (!towerAlreadyInPosition)
            {
                futureTowers.Add(new(actor.Position, 2, 1, 1, default, WorldState.FutureTime(towerLifespanSeconds)));
            }
            activationTime = futureTowers.MinBy(t => t.Activation).Activation;
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.BleedTower)
        {
            Towers.RemoveAll(t => t.Position.AlmostEqual(actor.Position, 1));
            if (Towers.Count == 0)
            {
                futureTowers.Clear();
            }
        }
    }

    public override void Update()
    {
        base.Update();
        for (var i = 0; i < futureTowers.Count; i++)
        {
            if (Towers.Count <= i)
            {
                if (activationTime < WorldState.FutureTime(waitForTimeRemaining))
                {
                    if (Towers.Count <= i)
                    {
                        Towers.Add(futureTowers[i]);
                    }
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        futureTowers = [.. futureTowers.OrderBy(t => t.Position.X).ThenBy(t => t.Position.Z)];
        if (Towers.Count == 0 || !EnableHints)
            return;

        var assignmentTower = 5; //towers 0-4 are assigned for role responsibility, the other 3 players can try to fill in any gaps.
        switch (assignment)
        {
            case PartyRolesConfig.Assignment.MT: assignmentTower = 0; break;
            case PartyRolesConfig.Assignment.OT: assignmentTower = 1; break;
            case PartyRolesConfig.Assignment.M1: assignmentTower = 2; break;
            case PartyRolesConfig.Assignment.M2: assignmentTower = 3; break;
            case PartyRolesConfig.Assignment.R1: assignmentTower = 4; break;
        }

        // we consider some list of towers part of the same "group" if their activations are within 500ms, as there can be varying delays between helper actors in an encounter casting the "same" spell
        // generally, successive towers that are meant to be soaked by one player (i.e. in quest battles) activate no more frequently than about 2 seconds apart, so this is pretty conservative
        var firstActivation = Towers.MinBy(t => t.Activation).Activation;
        var deadline = firstActivation.AddSeconds(0.5f);

        var soakingPlayers = new BitMask();

        // first see if we have one or more towers we need to soak - if so, add hints to take one of them
        // if there are no towers to soak, add hints to avoid forbidden ones
        // note that if we're currently inside a tower that has min number of soakers, we can't leave it
        List<Func<WPos, float>> zones = [];
        List<Func<WPos, float>> forbiddenZones = [];
        bool haveTowersToSoak = false;
        foreach (var t in Towers.Where(t => t.Activation <= deadline))
        {
            if (Towers.IndexOf(t) == assignmentTower || assignmentTower == 5) // should be 5 towers, 3 players should remain to flex soak
            {
                soakingPlayers |= Raid.WithSlot().InRadius(t.Position, t.Radius).Mask();

                var effNumSoakers = t.ForbiddenSoakers[slot] ? int.MaxValue : t.NumInside(Module);
                if (effNumSoakers < t.MinSoakers || effNumSoakers == t.MinSoakers && t.IsInside(actor))
                {
                    // this tower needs to be soaked; if this is the first one, clear out any previously found towers to avoid
                    if (!haveTowersToSoak)
                    {
                        zones.Clear();
                        haveTowersToSoak = true;
                    }
                    zones.Add(ShapeDistance.Circle(t.Position, t.Radius));
                }
                else if (effNumSoakers > t.MaxSoakers && !haveTowersToSoak)
                {
                    // this tower needs to be avoided; if we already have towers to soak, do nothing - presumably soaking other tower will automatically avoid this one
                    zones.Add(ShapeDistance.Circle(t.Position, t.Radius));
                }
                else if (t.ForbiddenSoakers[slot])
                    forbiddenZones.Add(ShapeDistance.Circle(t.Position, t.Radius));
            }
        }
        if (zones.Count > 0)
        {
            var zoneUnion = ShapeDistance.Union(zones);
            hints.AddForbiddenZone(haveTowersToSoak ? p => -zoneUnion(p) : zoneUnion, firstActivation);
        }
        if (forbiddenZones.Count > 0)
        {
            var fzu = ShapeDistance.Union(forbiddenZones);
            hints.AddForbiddenZone(fzu, firstActivation);
        }
        if (soakingPlayers.Any() && DamageType != AIHints.PredictedDamageType.None)
            hints.AddPredictedDamage(soakingPlayers, firstActivation, DamageType);
    }
}
class BleedTowerDanger(BossModule module) : Components.Voidzone(module, 2f, OID.BleedTower)
{
    public struct TowerPosition(WPos position, DateTime activation = default)
    {
        public WPos Position = position;
        public DateTime Activation = activation;
    }

    public DateTime Activation;

    public List<TowerPosition> TowerPositions = [];

    public override void OnActorCreated(Actor actor)
    {
        base.OnActorCreated(actor);
        if (actor.OID == (uint)OID.BleedTower)
        {
            var alreadyKnownTowerPosition = false;
            foreach (var towerPosition in TowerPositions)
            {
                if (actor.Position.X == towerPosition.Position.X && actor.Position.Z == towerPosition.Position.Z)
                {
                    alreadyKnownTowerPosition = true;
                }
            }
            if (!alreadyKnownTowerPosition)
            {
                TowerPositions.Add(new TowerPosition(actor.Position, WorldState.FutureTime(9f)));
                Activation = TowerPositions.MinBy(t => t.Activation).Activation;
            }
        }
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.BleedTower)
        {
            TowerPositions.Clear();
        }
        base.OnActorDestroyed(actor);
    }
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var towerPosition in TowerPositions)
        {
            if (WorldState.CurrentTime < towerPosition.Activation)
            {
                yield return new AOEInstance(Shape, towerPosition.Position);
            }
        }
        yield break;
    }

    //need to clear any forbidden zones associated with no-longer ActiveAOES.
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Activation > WorldState.CurrentTime)
        {
            var preActivation = Activation.AddSeconds(-3f);
            if (WorldState.CurrentTime < preActivation)
            {
                foreach (var t in TowerPositions)
                {
                    hints.AddForbiddenZone(ShapeDistance.Circle(t.Position, 2f), WorldState.CurrentTime); // void zone, active until it's not, don't try to pass through.
                    hints.AddForbiddenZone(ShapeDistance.Circle(t.Position, 2f), preActivation); // perhaps this will signal to the AI that it WILL be "safe" to enter? We need the AI to make their way to their tower, even though it's a void zone.
                }
            }
        }
    }
}

class DoomvoidGuillotine(BossModule module) : Components.StandardAOEs(module, AID.DoomvoidGuillotine, new AOEShapeRect(50f, 5f));
class EntropyRaidwide(BossModule module) : Components.RaidwideCast(module, AID.EntropyRaidwide);
class EmptyHateDamage(BossModule module) : Components.RaidwideCast(module, AID.EmptyHateDamage);
class DoomvoidSlicer(BossModule module) : Components.StandardAOEs(module, AID.DoomvoidSlicer, new AOEShapeDonut(5f, 30));
class Shadowflame(BossModule module) : Components.SingleTargetCast(module, AID.Shadowflame);
class Shadowflame1(BossModule module) : Components.SingleTargetCast(module, AID.Shadowflame1);

//Nyx radius seems more like 2, but janky
class Nyx(BossModule module) : Components.Voidzone(module, 4f, OID.Nyx)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var Nyxes = Module.Enemies(OID.Nyx);
        foreach (var n in Nyxes)
        {
            hints.AddForbiddenZone(new AOEShapeRect(7, 2, 4), n.Position, n.Rotation);
        }
        base.AddAIHints(slot, actor, assignment, hints);
    }
}

class E02VoidwalkerStates : StateMachineBuilder
{
    public E02VoidwalkerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<EmptyHateKnockback>()
            .ActivateOnEnter<DarkFireIIINormal>()
            .ActivateOnEnter<UnholyDarknessNormal>()
            .ActivateOnEnter<DoomvoidGuillotine>()
            .ActivateOnEnter<EntropyRaidwide>()
            .ActivateOnEnter<EmptyHateDamage>()
            .ActivateOnEnter<DoomvoidSlicer>()
            .ActivateOnEnter<Shadowflame>()
            .ActivateOnEnter<Shadowflame1>()
            .ActivateOnEnter<Nyx>()
            .ActivateOnEnter<SpellInWaitingStackSpread>()
            .ActivateOnEnter<BleedTower>()
            .ActivateOnEnter<BleedTowerDanger>()
            .ActivateOnEnter<ShadowEye>()
            .ActivateOnEnter<SpellInWaitingShadowEye>();
    }
}

[ModuleInfo(Incomplete = false, Contributors = "AndMyAxe", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 684, NameID = 8382)]
public class E02Voidwalker(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsRect(15f, 20f));
