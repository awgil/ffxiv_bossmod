namespace BossMod.Shadowbringers.Alliance.A35FalseIdol;

sealed class LighterNoteBait(BossModule module) : BossComponent(module)
{
    private readonly List<(Actor caster, bool isNorthSouth)> casters = [with(3)];
    private DateTime activation;
    private BitMask targets;
    private bool readyToRemove;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.LighterNote)
        {
            targets.Set(Raid.FindSlot(targetID));
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is var oid && oid is (uint)OID.LighterNoteWS or (uint)OID.LighterNoteNS)
        {
            casters.Add((actor, oid == (uint)OID.LighterNoteNS));
            activation = WorldState.FutureTime(4.9d);
        }
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (animState1 == 1 && actor.OID is var oid && oid is (uint)OID.LighterNoteWS or (uint)OID.LighterNoteNS)
        {
            readyToRemove = true;
            targets = default;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var count = casters.Count;
        for (var i = 0; i < count; ++i)
        {
            var c = casters[i];
            var cc = c.caster;
            var center = Arena.Center;
            {
                WDir[] dirs = c.isNorthSouth ? [new(default, 1f), new(default, -1f)] : [new(1f, default), new(-1f, default)];
                for (var j = 0; j < 2; ++j)
                {
                    var intersect = (int)Intersect.RayAABB(cc.Position - center, dirs[j], 27f, 27f); // exaflare is allowed to go upto 27y away from center
                    var maxexplosions = intersect / 6;
                    if (j == 0)
                    {
                        maxexplosions += 1;
                    }
                    for (var k = 0; k < maxexplosions; ++k)
                    {
                        Arena.ZoneCircleOutline(cc.Position + k * (j == 0 && k == 0 ? default : 6f) * dirs[j], 6f);
                    }
                }
            }
        }
    }

    public override void Update()
    {
        if (!readyToRemove)
        {
            return;
        }
        var count = casters.Count - 1;
        for (var i = count; i >= 0; --i)
        {
            var c = casters[i];
            var cc = c.caster;
            if (cc.LastFrameMovementVec4 == default) // remove hints only when the marker stopped moving
            {
                casters.RemoveAt(i);
                if (casters.Count == 0)
                {
                    readyToRemove = false;
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = casters.Count;
        for (var i = 0; i < count; ++i)
        {
            hints.AddForbiddenZone(new SDCircle(casters[i].caster.Position, 6f), activation);
        }
        if (targets[slot])
        {
            hints.AddForbiddenZone(new SDCircle(Arena.Center, 24.5f), activation);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (targets[slot])
        {
            hints.Add("Bait exaflare away from raid!");
        }
    }
}

sealed class LighterNoteExaflare(BossModule module) : Components.Exaflare(module, 6f)
{
    private readonly List<(Actor caster, bool isNorthSouth)> casters = [with(3)];
    private DateTime activation;

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (animState1 == 1 && actor.OID is var oid && oid is (uint)OID.LighterNoteWS or (uint)OID.LighterNoteNS)
        {
            casters.Add((actor, oid == (uint)OID.LighterNoteNS));
            activation = WorldState.FutureTime(3d);
        }
    }

    public override void Update()
    {
        var count = casters.Count - 1;
        for (var i = count; i >= 0; --i)
        {
            var c = casters[i];
            var cc = c.caster;
            if (cc.LastFrameMovementVec4 != default) // add exaflare only when all markers stopped moving
            {
                return;
            }
        }
        for (var i = count; i >= 0; --i)
        {
            var c = casters[i];
            var cc = c.caster;
            var center = Arena.Center;
            WDir[] dirs = c.isNorthSouth ? [new(default, 1f), new(default, -1f)] : [new(1f, default), new(-1f, default)];
            for (var j = 0; j < 2; ++j)
            {
                var intersect = (int)Intersect.RayAABB(cc.Position - center, dirs[j], 27f, 27f); // exaflare is allowed to go upto 27y away from center
                var maxexplosions = intersect / 6;
                if (j == 0)
                {
                    maxexplosions += 1;
                }
                Lines.Add(new(cc.Position + (j == 0 ? default : 6f) * dirs[j], 6f * dirs[j], j != 0 ? activation : activation.AddSeconds(1.1d), 1.1d, maxexplosions, 9));
            }
            casters.RemoveAt(i);
        }
        base.Update();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.LighterNoteFirst or (uint)AID.LighterNoteRest)
        {
            var count = Lines.Count;
            var pos = spell.TargetXZ;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 0.05f))
                {
                    AdvanceLine(line, pos);
                    if (line.ExplosionsLeft == 0)
                    {
                        Lines.RemoveAt(i);
                    }
                    break;
                }
            }
        }
    }
}
