using static FFXIVClientStructs.FFXIV.Client.Game.InstanceContent.InstanceContentDeepDungeon;

namespace BossMod.Global.DeepDungeon;

abstract partial class AutoClear : ZoneModule
{
    private readonly List<Vector3> _trapsCurrentZone = [];
    private readonly List<WPos> ProblematicTrapLocations = [];
    private bool _trapsHidden = true;

    private readonly List<Vector3> _trapsCurrentFloor = [];

    private void FilterTraps()
    {
        _trapsCurrentFloor.Clear();
        if (_trapsCurrentZone.Count == 0)
        {
            _trapsCurrentZone.AddRange(PalacePalInterop.GetTrapLocationsForZone(World.CurrentZone));
            _trapsCurrentZone.RemoveAll(t => ProblematicTrapLocations.Any(l => l.AlmostEqual(t.ToWPos(), 0.1f)));
        }

        for (var i = 0; i < Palace.Rooms.Length; i++)
            if (Palace.Rooms[i] > 0 && !Palace.Rooms[i].HasFlag(RoomFlags.Home))
                _trapsCurrentFloor.AddRange(_trapsCurrentZone.Where(t => _floorRects[Palace.Progress.Tileset][i].Contains(t)));

        foreach (var a in World.Actors)
            HandleBeacon(a);
    }

    private void DrawTraps()
    {
        if (_config.DrawTraps)
            foreach (var p in _trapsCurrentFloor)
                Camera.Instance?.DrawWorldCircle(p, 2, 0xFF0000FF, 2);
    }

    private void HandleTrap(Actor actor, ActorCastEvent ev)
    {
        switch (ev.Action.ID)
        {
            // standard traps
            case 6275:
            case 6276:
            case 6277:
            case 6278:
            case 6279:

            case 11287: // otter trap
            case 32375: // owl trap
            case 44038: // fae trap
                var box = FindClosestRoom(actor.Position).Room;
                _trapsCurrentFloor.RemoveAll(box.Contains);
                break;
        }
    }

    private void HandleBeacon(Actor c)
    {
        if ((OID)c.OID is OID.BeaconHoH or OID.BandedCofferIndicator)
            _trapsCurrentFloor.RemoveAll(t => t.ToWPos().InCircle(c.Position, 2));
    }
}
