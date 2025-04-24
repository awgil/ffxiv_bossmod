namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS1TrinitySeeker;

class MercyFourfold(BossModule module) : Components.GenericAOEs(module, AID.MercyFourfoldAOE)
{
    public readonly List<AOEInstance> AOEs = [];
    private readonly List<AOEInstance?> _safezones = [];
    private static readonly AOEShapeCone _shapeAOE = new(50, 90.Degrees());
    private static readonly AOEShapeCone _shapeSafe = new(50, 45.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (AOEs.Count > 0)
            yield return AOEs[0];
        if (_safezones.Count > 0 && _safezones[0] != null)
            yield return _safezones[0]!.Value;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID != SID.Mercy)
            return;

        var dirOffset = status.Extra switch
        {
            0xF7 => -45.Degrees(),
            0xF8 => -135.Degrees(),
            0xF9 => 45.Degrees(),
            0xFA => 135.Degrees(),
            _ => 0.Degrees()
        };
        if (dirOffset == default)
            return;

        var dir = actor.Rotation + dirOffset;
        if (AOEs.Count > 0)
        {
            // see whether there is a safezone for two contiguous aoes
            var mid = dir.ToDirection() + AOEs[^1].Rotation.ToDirection(); // length should be either ~sqrt(2) or ~0
            if (mid.LengthSq() > 1)
                _safezones.Add(new(_shapeSafe, actor.Position, Angle.FromDirection(-mid), new(), ArenaColor.SafeFromAOE, false));
            else
                _safezones.Add(null);
        }

        var activationDelay = 15 - 1.3f * AOEs.Count;
        AOEs.Add(new(_shapeAOE, actor.Position, dir, WorldState.FutureTime(activationDelay)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action == WatchedAction)
        {
            if (AOEs.Count > 0)
                AOEs.RemoveAt(0);
            if (_safezones.Count > 0)
                _safezones.RemoveAt(0);
        }
    }
}
