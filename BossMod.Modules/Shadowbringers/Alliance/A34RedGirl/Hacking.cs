namespace BossMod.Shadowbringers.Alliance.A34RedGirl;

class Hacking(BossModule module) : BossComponent(module)
{
    private static ArenaBoundsCustom BuildHackerBounds()
    {
        var circle = CurveApprox.Circle(20, 0.02f);
        var clipper = new PolygonClipper();
        var room = CurveApprox.Rect(new(6, 0), new(0, 6)).Select(r => r + new WDir(0, 38));
        var tunnel = CurveApprox.Rect(new(1.5f, 0), new(0, 12)).Select(r => r + new WDir(0, 31));
        var spawn = clipper.Union(new(room), new(tunnel));
        var spawn2 = clipper.Union(new(room.Select(r => r * -1)), new(tunnel.Select(t => t * -1)));
        var poly = clipper.Union(new(clipper.Union(new(circle), new(spawn))), new(spawn2));
        return new ArenaBoundsCustom(40, poly);
    }

    // each alliance gets teleported to a different room so we just cheat
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.RedSphere)
        {
            Arena.Bounds = BuildHackerBounds();
            Arena.Center = actor.Position;
        }
    }

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        if (updateID == 0x80000005 && param1 == 0)
        {
            Arena.Bounds = new ArenaBoundsSquare(24.5f);
            Arena.Center = new(845, -851);
        }
    }
}

class HackingWalls(BossModule module) : Components.AddsMulti(module, [OID.WhiteWall, OID.BlackWall]);
class HackingPylons(BossModule module) : Components.AddsMulti(module, [OID.BlackPylon, OID.WhitePylon]);
class RedSphere(BossModule module) : Components.Adds(module, (uint)OID.RedSphere);

class HackRotation(WorldState ws) : QuestBattle.UnmanagedRotation(ws, 10)
{
    private readonly A34RedGirlConfig _config = Service.Config.Get<A34RedGirlConfig>();

    protected override void Exec(Actor? primaryTarget)
    {
        if (!_config.AutoHack)
            return;

        var whiteColor = Player.FindStatus(SID.ProgramFFFFFFF, DateTime.MaxValue);
        var blackColor = Player.FindStatus(SID.Program000000, DateTime.MaxValue);
        // prioritize pending status over current
        var color = whiteColor?.ExpireAt == DateTime.MaxValue
            ? Shade.White
            : blackColor?.ExpireAt == DateTime.MaxValue
            ? Shade.Black
            : whiteColor != null ? Shade.White : blackColor != null ? Shade.Black : default;

        if (color == default)
            return;

        // need priority sort to happen before this executes
        Hints.PotentialTargets.SortByReverse(x => x.Priority);

        var closest = Hints.PriorityTargets.MinBy(t => t.Actor.DistanceToPoint(Player.Position));
        if (closest == null)
            return;

        var targetColor = (OID)closest.Actor.OID switch
        {
            OID.WhiteWall or OID.WhitePylon => Shade.White,
            OID.BlackWall or OID.BlackPylon => Shade.Black,
            _ => default
        };

        // prioritize not matching target color
        if (targetColor == color)
            SwitchColors(color);

        // match boss color
        else if (Hints.PotentialTargets.FirstOrDefault(t => (OID)t.Actor.OID == OID.RedSphere) is { } redSphere && redSphere.Actor.CastInfo is { } castInfo)
        {
            var waveColor = (AID)castInfo.Action.ID switch
            {
                AID.WaveBlack => Shade.Black,
                AID.WaveWhite => Shade.White,
                _ => default
            };
            if (waveColor != default && waveColor != color && Player.Position.InCircle(redSphere.Actor.Position, 22))
                SwitchColors(color);
        }

        var attackAction = color == Shade.White ? Roleplay.AID.LiminalFireWhite : Roleplay.AID.LiminalFireBlack;
        UseAction(attackAction, Player, facingAngle: Player.AngleTo(closest.Actor));
    }

    private void SwitchColors(Shade currentColor)
    {
        var switchAction = currentColor == Shade.White ? Roleplay.AID.F0SwitchToBlack : Roleplay.AID.F0SwitchToWhite;
        UseAction(switchAction, Player, 10);
    }
}

class HackModule(BossModule module) : QuestBattle.RotationModule<HackRotation>(module);
