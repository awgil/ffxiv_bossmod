namespace BossMod.Dawntrail.Savage.RM07BruteAbominator;

class BrutalImpact(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID._Weaponskill_BrutalImpact1))
{
    private DateTime Activation;
    private bool ShowHint;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_BrutalImpact)
        {
            Activation = Module.CastFinishAt(spell, 0.2f);
            ShowHint = true;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Activation >= WorldState.CurrentTime)
            hints.PredictedDamage.Add((Raid.WithSlot().Mask(), Activation));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (NumCasts >= 6)
            ShowHint = false;
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

class SporeSac(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_SporeSac), 8);
class Pollen(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_Pollen), 8);
class SinisterSeedsSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID._Spell_SinisterSeeds1), 6);
class SinisterSeedsChase(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_SinisterSeeds), 7);

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

class RootsOfEvil(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_RootsOfEvil), 12);

class TendrilsOfTerror1(BossModule module) : Components.StandardAOEs(module, ActionID.MakeSpell(AID._Spell_TendrilsOfTerror), new AOEShapeCross(60, 2));

class Impact(BossModule module) : Components.UniformStackSpread(module, 6, 0, 2, 4)
{
    public int NumCasts;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Spell_SinisterSeeds1 && Stacks.Count == 0)
            AddStacks(Raid.WithoutSlot().Where(r => r.Role == Role.Healer).Take(2), WorldState.FutureTime(10));

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

class CrossingCrosswinds(BossModule module) : Components.StandardAOEs(module, ActionID.MakeSpell(AID._Ability_CrossingCrosswinds), new AOEShapeCross(50, 5));
class WindingWildwinds(BossModule module) : Components.StandardAOEs(module, ActionID.MakeSpell(AID._Ability_WindingWildwinds), new AOEShapeDonut(5, 60))
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {

    }
}

class HurricaneForce(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID._Weaponskill_HurricaneForce), "Plant enrage!", true);

class QuarrySwamp(BossModule module) : Components.GenericLineOfSightAOE(module, ActionID.MakeSpell(AID._Weaponskill_QuarrySwamp), 60, false)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Modify(caster.Position, Module.Enemies(OID._Gen_BloomingAbomination).Where(b => b.IsDead).Select(b => (b.Position, 1f)), Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Modify(null, []);
    }
}

class Explosion(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_Explosion), 22, "GTFO from AOE!", 2)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var c in Casters)
            hints.PredictedDamage.Add((Raid.WithSlot().Mask(), Module.CastFinishAt(c.CastInfo)));
    }
}

class PulpSmash(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stack, ActionID.MakeSpell(AID._Weaponskill_PulpSmash2), 5.2f, 10);

class PulpSmashProtean(BossModule module) : Components.GenericBaitAway(module)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.Stack)
        {
            foreach (var player in Raid.WithoutSlot().Exclude(actor))
                CurrentBaits.Add(new(actor, player, new AOEShapeCone(60, 15.Degrees()), WorldState.FutureTime(7.1f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_PulpSmash2)
        {
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
}

class NeoBombarianSpecial(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_NeoBombarianSpecial));

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1024, NameID = 13756)]
public class RM07BruteAbombinator(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20));

