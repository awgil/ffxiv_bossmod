namespace BossMod.Dawntrail.Savage.RM07SBruteAbominator;

class BrutalImpact(BossModule module) : Components.CastCounter(module, AID._Weaponskill_BrutalImpact1)
{
    private DateTime Activation;
    private bool ShowHint => Activation != default;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_BrutalImpact)
            Activation = Module.CastFinishAt(spell, 0.2f);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (ShowHint)
            hints.PredictedDamage.Add((Raid.WithSlot().Mask(), Activation));
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (ShowHint)
            hints.Add("Raidwide");
    }
}

class P1Stoneringer(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? NextAOE;

    public bool Risky;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.StoneringerClub:
                NextAOE = new(new AOEShapeCircle(12), caster.Position, default, Module.CastFinishAt(spell, 9.8f));
                break;
            case AID.StoneringerSword:
                NextAOE = new(new AOEShapeDonut(9, 60), caster.Position, default, Module.CastFinishAt(spell, 9.8f));
                break;
            case AID._Weaponskill_BrutishSwing1:
            case AID._Weaponskill_BrutishSwing3:
                Risky = true;
                NextAOE = NextAOE!.Value with { Activation = Module.CastFinishAt(spell) };
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_BrutishSwing1 or AID._Weaponskill_BrutishSwing3)
        {
            NumCasts++;
            NextAOE = null;
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(NextAOE).Select(a => a with { Risky = Risky, Origin = Module.PrimaryActor.Position });
}

class P1Smash(BossModule module) : Components.GenericSharedTankbuster(module, default, 6)
{
    enum Smash
    {
        None,
        Close,
        Far
    }

    private Smash NextSmash;

    public override void Update()
    {
        switch (NextSmash)
        {
            case Smash.None:
                Source = null;
                Target = null;
                break;
            case Smash.Close:
                Source = Module.PrimaryActor;
                Target = Raid.WithoutSlot().Closest(Source.Position);
                break;
            case Smash.Far:
                Source = Module.PrimaryActor;
                Target = Raid.WithoutSlot().Farthest(Source.Position);
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_SmashHere:
                NextSmash = Smash.Close;
                break;
            case AID._Weaponskill_SmashThere:
                NextSmash = Smash.Far;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_BrutalSmash or AID._Weaponskill_BrutalSmash1)
        {
            NumCasts++;
            NextSmash = Smash.None;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (NextSmash != Smash.None)
            hints.Add(NextSmash == Smash.Close ? "Tankbuster close" : "Tankbuster far");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Target == actor && actor.Role != Role.Tank)
            hints.Add(NextSmash == Smash.Close ? "Go far from boss!" : "Go close to boss!");
        else
            base.AddHints(slot, actor, hints);
    }
}

class SporeSac(BossModule module) : Components.LocationTargetedAOEs(module, AID._Spell_SporeSac, 8);
class Pollen(BossModule module) : Components.LocationTargetedAOEs(module, AID._Spell_Pollen, 8);
class SinisterSeedsSpread(BossModule module) : Components.SpreadFromCastTargets(module, AID._Spell_SinisterSeeds1, 6);
class SinisterSeedsChase(BossModule module) : Components.LocationTargetedAOEs(module, AID._Spell_SinisterSeeds, 7);

class SinisterSeedsStored(BossModule module) : Components.GenericAOEs(module, default, "GTFO from puddle!")
{
    private readonly List<WPos> Casts = [];

    private bool Active;
    private DateTime Activation;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Spell_SinisterSeeds)
            Casts.Add(spell.TargetXZ);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Spell_RootsOfEvil)
            Active = false;
    }

    public void Activate()
    {
        Active = true;
        Activation = WorldState.FutureTime(5.4f);
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Active ? Casts.Select(s => new AOEInstance(new AOEShapeCircle(12), s, default, Activation)) : [];
}

class RootsOfEvil(BossModule module) : Components.LocationTargetedAOEs(module, AID._Spell_RootsOfEvil, 12);

class TendrilsOfTerror(BossModule module) : Components.GroupedAOEs(module, [AID._Spell_TendrilsOfTerror, AID._Spell_TendrilsOfTerror2, AID._Spell_TendrilsOfTerror4], new AOEShapeCross(60, 2))
{
    public void ResetCount()
    {
        NumCasts = 0;
    }
}

class Impact : Components.UniformStackSpread
{
    public int NumCasts;

    public Impact(BossModule module) : base(module, 6, 0, 2, 4)
    {
        AddStacks(Raid.WithoutSlot().Where(r => r.Role == Role.Healer).Take(2), WorldState.FutureTime(4.5f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_Impact)
        {
            NumCasts++;
            if (Stacks.Count > 0)
                Stacks.RemoveAt(0);
        }
    }
}

class BloomingAbomination(BossModule module) : Components.Adds(module, (uint)OID._Gen_BloomingAbomination)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var mob in ActiveActors)
        {
            if (hints.FindEnemy(mob) is { } e)
            {
                e.ForbidDOTs = true;
                e.Priority = 1;
                e.ShouldBeInterrupted = mob.CastInfo?.Action.ID == (uint)AID._Ability_WindingWildwinds;
            }
        }
    }
}

class P3BloomingAbomination(BossModule module) : Components.AddsPointless(module, (uint)OID._Gen_BloomingAbomination);

class CrossingCrosswinds(BossModule module) : Components.StandardAOEs(module, AID._Ability_CrossingCrosswinds, new AOEShapeCross(50, 5));
class WindingWildwinds : Components.StandardAOEs
{
    public WindingWildwinds(BossModule module) : base(module, AID._Ability_WindingWildwinds, new AOEShapeDonut(5, 60))
    {
        Risky = false;
    }
}

class HurricaneForce(BossModule module) : Components.CastHint(module, AID._Weaponskill_HurricaneForce, "Plant enrage!", true);

class QuarrySwamp(BossModule module) : Components.GenericLineOfSightAOE(module, AID._Weaponskill_QuarrySwamp, 60, false)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Modify(caster.Position, Module.Enemies(OID._Gen_BloomingAbomination).Select(b => (b.Position, b.HitboxRadius)), Module.CastFinishAt(spell));
    }

    public override void Update()
    {
        if (Origin != null)
            Modify(Origin, Module.Enemies(OID._Gen_BloomingAbomination).Select(b => (b.Position, b.HitboxRadius)), NextExplosion);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Modify(null, []);
    }
}

class Explosion(BossModule module) : Components.LocationTargetedAOEs(module, AID._Spell_Explosion, 22, "GTFO from aoe!", 2)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        foreach (var c in Casters)
            hints.PredictedDamage.Add((Raid.WithSlot().Mask(), Module.CastFinishAt(c.CastInfo)));
    }
}

class PulpSmash(BossModule module) : Components.StackWithIcon(module, (uint)IconID.PulpSmash, AID._Weaponskill_PulpSmash2, 6, 5.1f);

class PulpSmashProtean(BossModule module) : Components.GenericBaitAway(module)
{
    private bool Risky;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.PulpSmash)
        {
            foreach (var player in Raid.WithoutSlot().Exclude(actor))
                CurrentBaits.Add(new(actor, player, new AOEShapeCone(60, 15.Degrees()), WorldState.FutureTime(7.1f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_PulpSmash2)
        {
            Risky = true;
            for (var i = 0; i < CurrentBaits.Count; i++)
                CurrentBaits.Ref(i).Source = Module.PrimaryActor;
            CurrentBaits.Add(new(Module.PrimaryActor, WorldState.Actors.Find(spell.MainTargetID)!, new AOEShapeCone(60, 15.Degrees()), CurrentBaits[0].Activation));
        }

        if ((AID)spell.Action.ID == AID._Spell_TheUnpotted)
        {
            NumCasts++;
            CurrentBaits.Clear();
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Risky)
            base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Risky)
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class ItCameFromTheDirt(BossModule module) : Components.LocationTargetedAOEs(module, AID._Spell_ItCameFromTheDirt, 6);

class NeoBombarianSpecial(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_NeoBombarianSpecial);

class P2BrutishSwing(BossModule module) : Components.GenericAOEs(module)
{
    public bool Risky;

    private readonly List<(Actor Caster, bool In)> Casters = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_BrutishSwing6:
                Casters.Add((caster, true));
                break;
            case AID._Weaponskill_BrutishSwing9:
                Casters.Add((caster, false));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_BrutishSwing6 or AID._Weaponskill_BrutishSwing9)
        {
            Casters.RemoveAll(c => c.Caster == caster);
            NumCasts++;
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Select(c => new AOEInstance(c.In ? new AOEShapeDonut(21.712f, 88) : new AOEShapeCircle(25), c.Caster.CastInfo!.LocXZ, c.Caster.CastInfo!.Rotation, Module.CastFinishAt(c.Caster.CastInfo), Risky: Risky));
}

class P2GlowerPower(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_GlowerPower1, new AOEShapeRect(65, 7));

class P2ElectrogeneticForce(BossModule module) : Components.UniformStackSpread(module, 0, 6, alwaysShowSpreads: true)
{
    public int NumCasts;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_GlowerPower)
            AddSpreads(Raid.WithoutSlot(), WorldState.FutureTime(3.9f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Spell_ElectrogeneticForce)
        {
            NumCasts++;
            Spreads.Clear();
        }
    }
}

class RevengeOfTheVines(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_RevengeOfTheVines);

class ThornsOfDeath(BossModule module) : BossComponent(module)
{
    public readonly List<(Actor, Actor)> Tethers = [];

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID is TetherID.ThornsOfDeathTank or TetherID.ThornsOfDeath or TetherID.ThornsOfDeathPre)
            Tethers.Add((source, WorldState.Actors.Find(tether.Target)!));
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID is TetherID.ThornsOfDeathTank or TetherID.ThornsOfDeath or TetherID.ThornsOfDeathPre)
            Tethers.RemoveAll(t => t.Item1 == source && t.Item2.InstanceID == tether.Target);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var (src, tar) in Tethers)
            Arena.AddLine(src.Position, tar.Position, ArenaColor.Danger);
    }
}

class AbominableBlink(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(23), (uint)IconID.Flare, AID._Ability_AbominableBlink, activationDelay: 6.5f, centerAtTarget: true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (CurrentBaits.Count > 0)
            hints.PredictedDamage.Add((Raid.WithSlot().Mask(), CurrentBaits[0].Activation));
    }
}

class StrangeSeeds(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.SinisterSeed, AID._Spell_StrangeSeeds, 6, 5)
{
    public bool Risky = true;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Risky)
            base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Risky)
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class StrangeSeedsCounter(BossModule module) : BossComponent(module)
{
    public int NumCasts;

    // no cast event if target dies, but CastFinished is always fired
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Spell_StrangeSeeds)
            NumCasts++;
    }
}
class KillerSeeds(BossModule module) : Components.StackWithCastTargets(module, AID._Spell_KillerSeeds, 6, 2, 2);

class Powerslam(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_Powerslam);
class Sporesplosion(BossModule module) : Components.LocationTargetedAOEs(module, AID._Spell_Sporesplosion, 8, maxCasts: 12);

class P3BrutishSwing(BossModule module) : Components.GenericAOEs(module)
{
    public bool Risky;

    private readonly List<(Actor Caster, bool In)> Casters = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_BrutishSwing14:
                Casters.Add((caster, true));
                break;
            case AID._Weaponskill_BrutishSwing17:
                Casters.Add((caster, false));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_BrutishSwing14 or AID._Weaponskill_BrutishSwing17)
        {
            Casters.RemoveAll(c => c.Caster == caster);
            NumCasts++;
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Select(c => new AOEInstance(c.In ? new AOEShapeDonut(21.712f, 88) : new AOEShapeCircle(25), c.Caster.CastInfo!.LocXZ, c.Caster.CastInfo!.Rotation, Module.CastFinishAt(c.Caster.CastInfo), Risky: Risky));
}

class Lariat(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_LashingLariat1, AID._Weaponskill_LashingLariat3], new AOEShapeRect(70, 16));
class P3Glower(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_GlowerPower3, new AOEShapeRect(65, 7));
class Slaminator(BossModule module) : Components.CastTowers(module, AID._Weaponskill_Slaminator1, 8, maxSoakers: 8);

class P3ElectrogeneticForce : Components.UniformStackSpread
{
    public int NumCasts;
    public bool Risky;

    public P3ElectrogeneticForce(BossModule module) : base(module, 0, 6, alwaysShowSpreads: true)
    {
        AddSpreads(Raid.WithoutSlot(), WorldState.FutureTime(11.3f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Spell_ElectrogeneticForce)
        {
            NumCasts++;
            Spreads.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Risky)
            base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Risky)
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

#if DEBUG
[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1024, NameID = 13756, PlanLevel = 100)]
public class RM07SBruteAbombinator(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        if (Bounds.Contains(PrimaryActor.Position - Center))
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        else
        {
            var (hx, hz) = Bounds switch
            {
                ArenaBoundsRect rect => (rect.HalfWidth, rect.HalfHeight),
                ArenaBoundsSquare s => (s.HalfWidth, s.HalfWidth),
                _ => throw new Exception("unreachable")
            };

            // the boss looks off-center if we use regular raycast clamp to bounds
            Arena.ActorOutsideBounds(new(Math.Clamp(PrimaryActor.Position.X, Center.X - hx, Center.X + hx), Math.Clamp(PrimaryActor.Position.Z, Center.Z - hz, Center.Z + hz)), PrimaryActor.Rotation, ArenaColor.Enemy);
        }
    }
}
#endif
