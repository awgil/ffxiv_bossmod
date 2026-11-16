namespace BossMod.Stormblood.Extreme.Ex3Shinryu;

class DoomPuddle : BossComponent
{
    private const float Radius = 14;
    private static readonly WPos _back = new(2, -8);
    private readonly HashSet<ulong> _activePads = [];

    public DoomPuddle(BossModule module) : base(module)
    {
        KeepOnPhaseChange = true;
    }

    // TODO: verify the jump pad trigger radius and the edge of the back
    private bool OnBack(Actor actor) => actor.PosRot.Y > Module.PrimaryActor.PosRot.Y + 4;
    private bool WormwailActive => Module.PrimaryActor.CastInfo?.IsSpell(AID.Wormwail) == true;
    private IEnumerable<Actor> ActivePads() => Module.Enemies(OID.JumpPadWest).Concat(Module.Enemies(OID.JumpPadEast)).Where(p => _activePads.Contains(p.InstanceID));
    private Actor? ClosestPad(Actor actor) => ActivePads().MinBy(p => (p.Position - actor.Position).LengthSq());

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if ((OID)actor.OID is OID.JumpPadWest or OID.JumpPadEast)
        {
            if (state == 0x00040008)
                _activePads.Add(actor.InstanceID);
            else if (state == 0x00010002)
                _activePads.Remove(actor.InstanceID);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (OnBack(actor))
        {
            if (_activePads.Count > 0)
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(_back, 4));
            return;
        }
        if (!WormwailActive)
            hints.AddForbiddenZone(ShapeDistance.Circle(Module.PrimaryActor.Position, Radius));
        if (ClosestPad(actor) is { } pad)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(pad.Position, 2));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (OnBack(actor))
            return;
        if (actor.FindStatus(SID.Doom) != null)
            hints.Add("Doom: get out of the puddle!");
        if (ClosestPad(actor) != null)
            hints.Add("Jump onto Shinryu's back from a pad!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (!OnBack(pc) && !WormwailActive)
            Arena.ZoneCircle(Module.PrimaryActor.Position, Radius, ArenaColor.AOE);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (OnBack(pc))
            return;
        if (WormwailActive)
            Arena.AddCircle(Module.PrimaryActor.Position, Radius, ArenaColor.Danger);
        foreach (var pad in ActivePads())
            Arena.AddCircle(pad.Position, 2, ArenaColor.Safe, 2);
    }
}
