using BossMod.QuestBattle;

namespace BossMod.Shadowbringers.Quest.TheLostAndTheFound.Yxtlilton;

public enum OID : uint
{
    Boss = 0x29B0,
    Helper = 0x233C,
}

public enum AID : uint
{
    _AutoAttack_Attack = 872, // Boss->29AF, no cast, single-target
    _Spell_TheCodexOfDarknessII = 17010, // Boss->self, 3.0s cast, range 100 circle
    _Spell_TheCodexOfThunderIII = 17012, // Boss->self, 6.0s cast, single-target
    _Spell_TheCodexOfThunderIII1 = 17013, // Helper->29AE, no cast, range 3 circle
    _Spell_TheCodexOfGravity = 17014, // Boss->player, 4.5s cast, range 6 circle
    _Spell_TheCodexOfDarkness = 17011, // Boss->player/29AD, 10.0s cast, single-target
    _Ability_AwakenGuardian = 17015, // Boss->self, 3.0s cast, single-target
    _Spell_CurseOfTheRonka = 17016, // 29B1->29AD, no cast, single-target
}

public enum TetherID : uint
{
    _Gen_Tether_17 = 17, // Boss->29AD/player
}

class CodexOfDarknessII(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Spell_TheCodexOfDarknessII));
class CodexOfGravity(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID._Spell_TheCodexOfGravity), 6)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Stacks.Count > 0)
            hints.AddForbiddenZone(new AOEShapeDonut(1.5f, 100), Arena.Center, default, Stacks[0].Activation);
    }
}

class LamittAI(WorldState ws) : UnmanagedRotation(ws, 25)
{
    protected override void Exec(Actor? primaryTarget)
    {
        if (primaryTarget == null)
            return;

        var party = World.Party.WithoutSlot().ToList();

        Hints.GoalZones.Add(p => party.Count(act => act.Position.InCircle(p, 15 + Player.HitboxRadius + act.HitboxRadius)));
        Hints.GoalZones.Add(Hints.GoalSingleTarget(primaryTarget, 25));

        var lowest = party.MinBy(p => p.PredictedHPRatio)!;
        var esunable = party.FirstOrDefault(x => x.FindStatus(482) != null);
        var doomed = party.FirstOrDefault(x => x.FindStatus(1769) != null);
        var partyHealth = party.Average(p => p.PredictedHPRatio);

        // pre heal during doom cast since it does insane damage for some reason
        if (primaryTarget.CastInfo is { Action.ID: 17011 } ci && ci.TargetID == Player.InstanceID)
        {
            if (Player.PredictedHPRatio <= 0.8f)
                UseAction(Roleplay.AID.RonkanCureII, Player);
        }

        if (partyHealth < 0.6f)
            UseAction(Roleplay.AID.RonkanMedica, Player);

        if (lowest.HPMP.CurHP * 3 <= lowest.HPMP.MaxHP)
            UseAction(Roleplay.AID.RonkanCureII, lowest);

        if (esunable != null)
            UseAction(Roleplay.AID.RonkanEsuna, esunable);

        if (doomed != null)
        {
            UseAction(Roleplay.AID.RonkanRenew, doomed);
            UseAction(Roleplay.AID.RonkanCureII, doomed);
        }

        UseAction(Roleplay.AID.RonkanStoneII, primaryTarget);
    }
}

class AutoLamitt(BossModule module) : Components.RotationModule<LamittAI>(module);

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            h.Priority = 0;
    }
}

class YxtliltonStates : StateMachineBuilder
{
    public YxtliltonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Hints>()
            .ActivateOnEnter<AutoLamitt>()
            .ActivateOnEnter<CodexOfDarknessII>()
            .ActivateOnEnter<CodexOfGravity>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68806, NameID = 8393)]
public class Yxtlilton(WorldState ws, Actor primary) : BossModule(ws, primary, new(-120, -770), new ArenaBoundsCircle(20))
{
    protected override bool CheckPull() => true;

    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
}
