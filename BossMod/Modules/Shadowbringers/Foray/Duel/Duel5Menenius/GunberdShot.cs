namespace BossMod.Shadowbringers.Foray.Duel.Duel5Menenius;

class GunberdShot(BossModule module) : BossComponent(module)
{
    private Actor? _gunberdCaster;

    public bool DarkShotLoaded { get; private set; }
    public bool WindslicerLoaded { get; private set; }

    public bool Gunberding { get; private set; }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Gunberding)
        {
            if (DarkShotLoaded)
                hints.Add("Maintain Distance");
            if (WindslicerLoaded)
                hints.Add("Knockback");
        }
        else
        {
            if (DarkShotLoaded)
                hints.Add("Dark Loaded");
            if (WindslicerLoaded)
                hints.Add("Windslicer Loaded");
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.DarkShot:
                DarkShotLoaded = true;
                break;
            case AID.WindslicerShot:
                WindslicerLoaded = true;
                break;
            case AID.GunberdDark:
            case AID.GunberdWindslicer:
                Gunberding = true;
                _gunberdCaster = caster;
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.GunberdDark:
                DarkShotLoaded = false;
                Gunberding = false;
                break;
            case AID.GunberdWindslicer:
                WindslicerLoaded = false;
                Gunberding = false;
                break;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Gunberding && WindslicerLoaded)
        {
            var adjPos = Components.Knockback.AwayFromSource(pc.Position, _gunberdCaster, 10);
            Components.Knockback.DrawKnockback(pc, adjPos, Arena);
        }
    }
}
