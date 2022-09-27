using System;
using System.Collections.Generic;

namespace BossMod
{
    // information relevant for AI decision making process for a specific player
    public class AIHints
    {
        public static ArenaBounds DefaultBounds = new ArenaBoundsSquare(new(), 1000);

        public ArenaBounds Bounds = DefaultBounds;

        // positioning: two lists below define areas that player is allowed to be standing in, now or in near future
        // a resulting 'safe zone' is calculated as: arena-bounds INTERSECT union-of(restricted-zones) MINUS union-of(forbidden-zones)
        // AI will try to move in such a way to avoid standing in any forbidden zone after its activation or outside of some restricted zone after its activation, even at the cost of uptime
        public List<(AOEShape shape, WPos origin, Angle rot, DateTime activation)> ForbiddenZones = new();
        public List<(AOEShape shape, WPos origin, Angle rot, DateTime activation)> RestrictedZones = new();

        // imminent forced movements (knockbacks, attracts, etc.)
        public List<(WDir move, DateTime activation)> ForcedMovements = new();

        // positioning: position hint - if set, player will move closer to this position, assuming it is safe and in target's range, without losing uptime
        //public WPos? RecommendedPosition = null;

        // orientation restrictions (e.g. for gaze attacks): a list of forbidden orientation ranges, now or in near future
        // AI will rotate to face allowed orientation at last possible moment, potentially losing uptime
        public List<(Angle center, Angle halfWidth, DateTime activation)> ForbiddenDirections = new();

        // predicted incoming damage (raidwides, tankbusters, etc.)
        // AI will attempt to shield & mitigate
        public List<(BitMask players, DateTime activation)> PredictedDamage = new();

        // clear all stored data
        public void Clear()
        {
            Bounds = DefaultBounds;
            ForbiddenZones.Clear();
            RestrictedZones.Clear();
            ForcedMovements.Clear();
            ForbiddenDirections.Clear();
            PredictedDamage.Clear();
        }

        // normalize all entries after gathering data: sort by activation timestamp
        public void Normalize()
        {
            ForbiddenZones.SortBy(e => e.activation);
            RestrictedZones.SortBy(e => e.activation);
            ForcedMovements.SortBy(e => e.activation);
            ForbiddenDirections.SortBy(e => e.activation);
            PredictedDamage.SortBy(e => e.activation);
        }
    }
}
