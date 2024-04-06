namespace BossMod.Shadowbringers.Foray.Duel.Duel5Menenius;

class GunberdShot : BossComponent
{
    private Actor? _gunberdCaster;

    public bool darkShotLoaded { get; private set; }
    public bool windslicerLoaded { get; private set; }

    public bool Gunberding { get; private set; }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (Gunberding)
        {
            if (darkShotLoaded)
                hints.Add("Maintain Distance");
            if (windslicerLoaded)
                hints.Add("Knockback");
        }
        else
        {
            if (darkShotLoaded)
                hints.Add("Dark Loaded");
            if (windslicerLoaded)
                hints.Add("Windslicer Loaded");
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.DarkShot:
                darkShotLoaded = true;
                break;
            case AID.WindslicerShot:
                windslicerLoaded = true;
                break;
            case AID.GunberdDark:
            case AID.GunberdWindslicer:
                Gunberding = true;
                _gunberdCaster = caster;
                break;
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.GunberdDark:
                darkShotLoaded = false;
                Gunberding = false;
                break;
            case AID.GunberdWindslicer:
                windslicerLoaded = false;
                Gunberding = false;
                break;
        }
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (Gunberding && windslicerLoaded)
        {
            var adjPos = Components.Knockback.AwayFromSource(pc.Position, _gunberdCaster, 10);
            Components.Knockback.DrawKnockback(pc, adjPos, arena);
        }
    }
}
