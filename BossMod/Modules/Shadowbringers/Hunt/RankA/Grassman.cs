namespace BossMod.Shadowbringers.Hunt.RankA.Grassman;

public enum OID : uint
{
    Boss = 0x283A, // R=4.0
}

public enum AID : uint
{
    AutoAttack = 872, // 283A->player, no cast, single-target
    ChestThump = 17859, // 283A->self, 4.0s cast, range 30 circle, one cast on 1st time, 5 hits on subsequent times, dmg buff on boss for each cast
    ChestThump2 = 17863, // 283A->self, no cast, range 30 circle
    StoolPelt = 17861, // 283A->location, 3.0s cast, range 5 circle
    Browbeat = 17860, // 283A->player, 4.0s cast, single-target
    Streak = 17862, // 283A->location, 3.0s cast, width 6 rect charge, knockback 10, away from source
}

class ChestThump(BossModule module) : BossComponent(module)
{
    private int NumCasts;
    private int NumCasts2;
    private bool casting;
    private DateTime _activation;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ChestThump)
        {
            casting = true;
            _activation = spell.NPCFinishAt;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ChestThump)
        {
            ++NumCasts;
            if (NumCasts == 1)
                casting = false;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ChestThump2)
        {
            ++NumCasts2;
            if (NumCasts2 == 4)
            {
                casting = false;
                NumCasts2 = 0;
            }
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (casting && NumCasts == 0)
            hints.Add($"Raidwide");
        if (casting && NumCasts > 0)
            hints.Add($"Raidwide x5");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.PredictedDamage.Add((Raid.WithSlot().Mask(), _activation));
    }
}

class StoolPelt(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.StoolPelt), 5);
class Browbeat(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Browbeat));
class Streak(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.Streak), 3);

class GrassmanStates : StateMachineBuilder
{
    public GrassmanStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Streak>()
            .ActivateOnEnter<Browbeat>()
            .ActivateOnEnter<StoolPelt>()
            .ActivateOnEnter<ChestThump>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 8892)]
public class Grassman(WorldState ws, Actor primary) : SimpleBossModule(ws, primary) { }
