namespace BossMod.Stormblood.Alliance.A32Agrias;

class Jail : BossComponent
{
    BitMask Prisoners;
    static readonly WPos JailPos = new(300, -50);
    static readonly ArenaBoundsCircle JailBounds = new(29.5f);

    public Jail(BossModule module) : base(module)
    {
        KeepOnPhaseChange = true;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Jail)
        {
            Prisoners = BitMask.Build([.. spell.Targets.Select(t => Raid.FindSlot(t.ID))]);
            Arena.Bounds = new ArenaBoundsCircle(19.5f);
            Arena.Center = new(300, -50);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Prisoners[pcSlot])
        {
            if (Arena.Center != JailPos)
            {
                Arena.Center = JailPos;
                Arena.Bounds = JailBounds;
            }
        }
        else if (Arena.Center == JailPos)
        {
            Arena.Center = new(600, -54);
            Arena.Bounds = new ArenaBoundsCircle(29.5f);
        }
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.Doom && Raid.TryFindSlot(actor, out var slot))
            Prisoners.Clear(slot);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Prisoners[slot] && actor.FindStatus(SID.Swordbearer) == null)
            hints.GoalZones.Add(AIHints.GoalSingleTarget(new(300, -65), 5, 5));
    }
}

class TaintedSoul(BossModule module) : Components.Adds(module, (uint)OID.TaintedSoul, forbidDots: true);
class TaintlessSoul(BossModule module) : Components.AddsPointless(module, (uint)OID.TaintlessSoul);
