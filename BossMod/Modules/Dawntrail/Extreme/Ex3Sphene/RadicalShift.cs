namespace BossMod.Dawntrail.Extreme.Ex3Sphene;

class RadicalShift(BossModule module) : BossComponent(module)
{
    public enum Rotation { None, Left, Right }

    private ArenaBoundsCustom? _left;
    private ArenaBoundsCustom? _right;
    private Rotation _nextRotation;
    private List<RelTriangle>? _triangulation;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var platform = NextPlatform;
        if (platform != null && !platform.Contains(actor.Position - Module.Center))
            hints.Add("Go to safe platform!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_triangulation != null)
            Arena.Zone(_triangulation, ArenaColor.AOE);
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 12)
        {
            var rot = state switch
            {
                0x01000080 => Rotation.Left,
                0x08000400 => Rotation.Right,
                _ => Rotation.None
            };
            if (rot != Rotation.None)
            {
                _nextRotation = rot;
                UpdateTriangulation(NextPlatform);
            }
        }
        else if (state is 0x00020001 or 0x00200010)
        {
            ArenaBoundsCustom? platform = index switch
            {
                9 => Ex3Sphene.WindBounds,
                10 => Ex3Sphene.EarthBounds,
                11 => Ex3Sphene.IceBounds,
                _ => null
            };
            if (platform != null)
            {
                (state == 0x00020001 ? ref _right : ref _left) = platform;
                UpdateTriangulation(NextPlatform);
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.RadicalShift)
        {
            var platform = NextPlatform;
            if (platform != null)
                Arena.Bounds = platform;
            _left = _right = null;
            _nextRotation = Rotation.None;
            _triangulation = null;
        }
    }

    private ArenaBoundsCustom? NextPlatform => _nextRotation switch
    {
        Rotation.Left => _left,
        Rotation.Right => _right,
        _ => null
    };

    private void UpdateTriangulation(ArenaBoundsCustom? platform)
        => _triangulation = platform != null ? Arena.Bounds.ClipAndTriangulate(platform.Clipper.Difference(new(CurveApprox.Rect(new(0, 1), platform.Radius, platform.Radius)), new(platform.Poly))) : null;
}

class RadicalShiftAOE(BossModule module) : Components.SpreadFromCastTargets(module, AID.RadicalShiftAOE, 5);
