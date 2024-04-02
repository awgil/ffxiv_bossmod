namespace BossMod.RealmReborn.Extreme.Ex4Ifrit;

// TODO: revise & generalize to 'baited aoe' component, with nice utilities for AI
class Eruption : Components.LocationTargetedAOEs
{
    private DateTime _baitDetectDeadline;
    public BitMask Baiters;

    public static readonly float Radius = 8;

    public Eruption() : base(ActionID.MakeSpell(AID.EruptionAOE), Radius) { }

    public override void Update(BossModule module)
    {
        if (Casters.Count == 0)
            Baiters.Reset();
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(module, caster, spell);
        switch ((AID)spell.Action.ID)
        {
            case AID.Eruption:
                _baitDetectDeadline = module.WorldState.CurrentTime.AddSeconds(1);
                break;
            case AID.EruptionAOE:
                if (module.WorldState.CurrentTime < _baitDetectDeadline)
                {
                    var baiter = module.Raid.WithoutSlot().Closest(spell.LocXZ);
                    if (baiter != null)
                        Baiters.Set(module.Raid.FindSlot(baiter.InstanceID));
                }
                break;
        }
    }
}
