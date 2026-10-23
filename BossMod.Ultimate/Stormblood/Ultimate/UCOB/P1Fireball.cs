namespace BossMod.Stormblood.Ultimate.UCOB;

class P1Fireball(BossModule module) : Components.StrictStackWithIcon(module, (uint)IconID.Fireball, AID.Fireball, 4, 5.3f, 4)
{
    int _neurolinkCount;
    readonly PartyRolesConfig _prc = Service.Config.Get<PartyRolesConfig>();

    public WPos Destination { get; private set; }

    public override (WPos, float)? GetStackDestination(int order, Actor target) => (Destination, 0);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == StackIcon)
        {
            var slot = Raid.FindSlot(actor.InstanceID);

            if (_neurolinkCount == 0)
            {
                BitMask forbidden = new();

                if (Service.Config.Get<UCOBConfig>().P1Fireball1LBHints)
                {
                    var assignments = _prc.AssignmentsPerSlot(Raid);
                    if (assignments.Length > 0)
                    {
                        var hAvoid = PartyRolesConfig.Assignment.H1;
                        if (assignments[slot] == hAvoid)
                            hAvoid = PartyRolesConfig.Assignment.H2;

                        for (var i = 0; i < assignments.Length; i++)
                        {
                            if (assignments[i] is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT || assignments[i] == hAvoid)
                                forbidden.Set(i);
                        }
                    }
                }

                AddStack(actor, WorldState.FutureTime(5.3f), forbidden);

                Destination = Module.PrimaryActor.Position + Module.PrimaryActor.DirectionTo(Arena.Center) * (Module.PrimaryActor.HitboxRadius + 3);
            }
            else
                // stack activation is quite delayed, seen 7-7.5 seconds
                AddStack(actor, WorldState.FutureTime(7));
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Neurolink)
            _neurolinkCount++;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == StackAction)
        {
            Stacks.Clear();
            Destination = default;
        }

        if ((AID)spell.Action.ID == AID.LiquidHell && Destination == default && _neurolinkCount > 0)
        {
            var bossToPuddle = spell.TargetXZ - Module.PrimaryActor.Position;
            var safeDist = Math.Max(Module.PrimaryActor.HitboxRadius, 7 - bossToPuddle.Length());
            // stack on opposite side of boss from first fireball spawn
            Destination = Module.PrimaryActor.Position + bossToPuddle.Normalized() * -safeDist;
        }
    }
}
