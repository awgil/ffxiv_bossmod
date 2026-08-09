namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

sealed class RadicalShift(BossModule module) : Components.GenericAOEs(module)
{
    public enum Rotation { None, Left, Right }
    public enum Platform { None, Wind, Earth, Ice }

    private Platform _left;
    private Platform _right;
    private Rotation _nextRotation;
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x0C)
        {
            var rot = state switch
            {
                0x01000080u => Rotation.Left,
                0x08000400u => Rotation.Right,
                _ => Rotation.None
            };
            if (rot != Rotation.None)
            {
                _nextRotation = rot;
                UpdateAOE(NextPlatform());
            }
        }
        else if (state is 0x00020001u or 0x00200010u)
        {
            var platform = index switch
            {
                0x09 => Platform.Wind,
                0x0A => Platform.Earth,
                0x0B => Platform.Ice,
                _ => Platform.None
            };
            if (platform != Platform.None)
            {
                (state == 0x00020001u ? ref _right : ref _left) = platform;
                UpdateAOE(NextPlatform());
            }
        }

        Platform NextPlatform() => _nextRotation switch
        {
            Rotation.Left => _left,
            Rotation.Right => _right,
            _ => default
        };
    }

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        if (_aoe.Length != 0 && updateID == 0x8000000D && param1 is 0x02u or 0x04u or 0x08u)
        {
            _aoe = [];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RadicalShift)
        {
            _left = _right = Platform.None;
            _nextRotation = Rotation.None;
        }
    }

    private void UpdateAOE(Platform platform)
    {
        AOEShapeCustom? aoe = null;
        var center = Arena.Center;
        Square[] defaultSquare = [new(new(100f, 100f), 20f)];
        if (platform == Platform.Wind)
        {
            aoe = new(center, defaultSquare, Trial.T03QueenEternal.T03QueenEternal.GetXArenaRects());
        }
        else if (platform == Platform.Earth)
        {
            aoe = new(center, defaultSquare, Trial.T03QueenEternal.T03QueenEternal.GetSplitArenaRects());
        }
        else if (platform == Platform.Ice)
        {
            aoe = new(center, defaultSquare, Ex3QueenEternal.GetAllIceRects());
        }
        if (aoe != null)
        {
            _aoe = [new(aoe, center, default, WorldState.FutureTime(6d), shapeDistance: aoe.Distance(center, default))];
        }
    }
}

sealed class RadicalShiftAOE(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.RadicalShiftAOE, 5f);
