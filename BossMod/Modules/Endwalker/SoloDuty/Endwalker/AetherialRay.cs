using System;

namespace BossMod.Modules.Endwalker.SoloDuty.Endwalker;
class AetherialRay : Components.GenericBaitAway
{
    private DateTime _activation;
    private bool active;

    public AetherialRay() : base(centerAtTarget: true) { }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AetherialRay)
        {
            active = true;
            _activation = spell.NPCFinishAt;
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AetherialRay)
            ++NumCasts;
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.AetherialRayVisual)
        {
            ++NumCasts;
            if (NumCasts == 5)
            {
                CurrentBaits.Clear();
                NumCasts = 0;
                active = false;
            }
        }
    }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (active)
            hints.Add("Tankbuster 5x");
    }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (active)
        {
            BitMask targets = new();
            targets.Set(module.Raid.FindSlot(module.Raid.Player()!.TargetID));
            hints.PredictedDamage.Add((targets, _activation));
        }
    }
}
