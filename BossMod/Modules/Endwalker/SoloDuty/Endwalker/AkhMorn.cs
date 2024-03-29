using System;
using System.Linq;

namespace BossMod.Modules.Endwalker.SoloDuty.Endwalker;
class AkhMorn : Components.GenericBaitAway
{
    private DateTime _activation;
    private bool active;

    public AkhMorn() : base(centerAtTarget: true) { }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AkhMorn)
        {
            CurrentBaits.Add(new(module.PrimaryActor, module.Raid.Player()!, new AOEShapeCircle(4)));
            active = true;
            _activation = spell.NPCFinishAt;
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AkhMorn)
            ++NumCasts;
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.AkhMornVisual)
        {
            ++NumCasts;
            if (NumCasts == 6 && module.Enemies(OID.ZenosP1).Any(x => !x.IsDead))
            {
                CurrentBaits.Clear();
                NumCasts = 0;
                active = false;
            }
            if (NumCasts == 8 && module.Enemies(OID.ZenosP1).Any(x => x.IsDead))
            {
                CurrentBaits.Clear();
                NumCasts = 0;
                active = false;
            }
        }
    }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (active && module.Enemies(OID.ZenosP1).Any(x => !x.IsDead))
            hints.Add("Tankbuster 6x");
        if (active && module.Enemies(OID.ZenosP1).Any(x => x.IsDead))
            hints.Add("Tankbuster 8x");
    }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (active)
        {
            BitMask targets = new();
            targets.Set(module.Raid.FindSlot(module.Raid.Player()!.TargetID));
            if (module.Enemies(OID.ZenosP1).Any(x => !x.IsDead))
                for (int i = 1; i < 7; ++i)
                    hints.PredictedDamage.Add((targets, _activation.AddSeconds(i * 0.7f)));
            if (module.Enemies(OID.ZenosP1).Any(x => x.IsDead))
                for (int i = 1; i < 9; ++i)
                    hints.PredictedDamage.Add((targets, _activation.AddSeconds(i * 0.7f)));
        }
    }
}