namespace BossMod.Endwalker.Savage.P8S1Hephaistos;

class SunforgeCenterHint(BossModule module) : Components.CastHint(module, AID.SunforgeCenter, "Avoid center")
{
    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Active)
        {
            Arena.ZoneRect(Module.Center, new WDir(1, 0), 21, -7, 21, ArenaColor.SafeFromAOE);
            Arena.ZoneRect(Module.Center, new WDir(-1, 0), 21, -7, 21, ArenaColor.SafeFromAOE);
        }
    }
}

class SunforgeSidesHint(BossModule module) : Components.CastHint(module, AID.SunforgeSides, "Avoid sides")
{
    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Active)
        {
            Arena.ZoneRect(Module.Center, new WDir(0, 1), 21, 21, 7, ArenaColor.SafeFromAOE);
        }
    }
}

class SunforgeCenter(BossModule module) : Components.StandardAOEs(module, AID.ScorchingFang, new AOEShapeRect(42, 7));
class SunforgeSides(BossModule module) : Components.StandardAOEs(module, AID.SunsPinion, new AOEShapeRect(14, 21));
class SunforgeCenterIntermission(BossModule module) : Components.StandardAOEs(module, AID.ScorchingFangIntermission, new AOEShapeRect(42, 7));
class SunforgeSidesIntermission(BossModule module) : Components.StandardAOEs(module, AID.ScorchedPinion, new AOEShapeRect(14, 42));
