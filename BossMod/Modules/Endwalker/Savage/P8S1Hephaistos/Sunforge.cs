namespace BossMod.Endwalker.Savage.P8S1Hephaistos;

class SunforgeCenterHint : Components.CastHint
{
    public SunforgeCenterHint() : base(ActionID.MakeSpell(AID.SunforgeCenter), "Avoid center") { }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (Active)
        {
            arena.ZoneRect(module.Bounds.Center, new WDir(1, 0), 21, -7, 21, ArenaColor.SafeFromAOE);
            arena.ZoneRect(module.Bounds.Center, new WDir(-1, 0), 21, -7, 21, ArenaColor.SafeFromAOE);
        }
    }
}

class SunforgeSidesHint : Components.CastHint
{
    public SunforgeSidesHint() : base(ActionID.MakeSpell(AID.SunforgeSides), "Avoid sides") { }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (Active)
        {
            arena.ZoneRect(module.Bounds.Center, new WDir(0, 1), 21, 21, 7, ArenaColor.SafeFromAOE);
        }
    }
}

class SunforgeCenter : Components.SelfTargetedAOEs
{
    public SunforgeCenter() : base(ActionID.MakeSpell(AID.ScorchingFang), new AOEShapeRect(21, 7, 21)) { }
}

class SunforgeSides : Components.SelfTargetedAOEs
{
    public SunforgeSides() : base(ActionID.MakeSpell(AID.SunsPinion), new AOEShapeRect(21, 21, -7)) { }
}

class SunforgeCenterIntermission : Components.SelfTargetedAOEs
{
    public SunforgeCenterIntermission() : base(ActionID.MakeSpell(AID.ScorchingFangIntermission), new AOEShapeRect(42, 7)) { }
}

class SunforgeSidesIntermission : Components.SelfTargetedAOEs
{
    public SunforgeSidesIntermission() : base(ActionID.MakeSpell(AID.ScorchedPinion), new AOEShapeRect(21, 42, -7)) { }
}
