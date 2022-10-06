using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Pathfinding
{
    public class ThetaStar
    {
        public struct Node
        {
            public float GScore;
            public float HScore;
            public int ParentX;
            public int ParentY;
            public int OpenHeapIndex; // -1 if in closed list, 0 if not in any lists, otherwise (index+1)
            public float PathLeeway;
        }

        private Map _map;
        private (int x, int y)[] _goals;
        private Node[] _nodes;
        private List<int> _openList = new();
        private float _deltaGSide;
        private float _deltaGDiag;

        public ref Node NodeByIndex(int index) => ref _nodes[index];
        public int CellIndex(int x, int y) => y * _map.Width + x;
        public WPos CellCenter(int index) => _map.GridToWorld(index % _map.Width, index / _map.Width, 0.5f, 0.5f);

        // gMultiplier is typically inverse speed, which turns g-values into time
        public ThetaStar(Map map, IEnumerable<(int x, int y)> goals, (int x, int y) start, float gMultiplier)
        {
            _map = map;
            _goals = goals.ToArray();
            _nodes = new Node[map.Width * map.Height];
            _deltaGSide = map.Resolution * gMultiplier;
            _deltaGDiag = _deltaGSide * 1.414214f;

            start = map.ClampToGrid(start);
            int startIndex = CellIndex(start.x, start.y);
            _nodes[startIndex].GScore = 0;
            _nodes[startIndex].HScore = HeuristicDistance(start.x, start.y);
            _nodes[startIndex].ParentX = start.x; // start's parent is self
            _nodes[startIndex].ParentY = start.y;
            _nodes[startIndex].PathLeeway = float.MaxValue; // min diff along path between node's g-value and cell's g-value
            AddToOpen(startIndex);
        }

        public ThetaStar(Map map, int goalPriority, WPos startPos, float gMultiplier)
            : this(map, map.Goals().Where(g => g.priority >= goalPriority).Select(g => (g.x, g.y)), map.WorldToGrid(startPos), gMultiplier)
        {
        }

        // returns whether search is to be terminated; on success, first node of the open list would contain found goal
        public bool ExecuteStep()
        {
            if (_goals.Length == 0 || _openList.Count == 0 || _nodes[_openList[0]].HScore <= 0)
                return false;

            int nextNodeIndex = PopMinOpen();
            var nextNodeX = nextNodeIndex % _map.Width;
            var nextNodeY = nextNodeIndex / _map.Width;
            bool haveN = nextNodeIndex >= _map.Width;
            bool haveS = nextNodeIndex < (_map.Width - 1) * _map.Height;
            bool haveE = nextNodeX > 0;
            bool haveW = nextNodeX < _map.Width - 1;
            if (haveN)
            {
                VisitNeighbour(nextNodeX, nextNodeY, nextNodeIndex, nextNodeX, nextNodeY - 1, nextNodeIndex - _map.Width, _deltaGSide);
                if (haveE)
                    VisitNeighbour(nextNodeX, nextNodeY, nextNodeIndex, nextNodeX - 1, nextNodeY - 1, nextNodeIndex - _map.Width - 1, _deltaGDiag);
                if (haveW)
                    VisitNeighbour(nextNodeX, nextNodeY, nextNodeIndex, nextNodeX + 1, nextNodeY - 1, nextNodeIndex - _map.Width + 1, _deltaGDiag);
            }
            if (haveE)
                VisitNeighbour(nextNodeX, nextNodeY, nextNodeIndex, nextNodeX - 1, nextNodeY, nextNodeIndex - 1, _deltaGSide);
            if (haveW)
                VisitNeighbour(nextNodeX, nextNodeY, nextNodeIndex, nextNodeX + 1, nextNodeY, nextNodeIndex + 1, _deltaGSide);
            if (haveS)
            {
                VisitNeighbour(nextNodeX, nextNodeY, nextNodeIndex, nextNodeX, nextNodeY + 1, nextNodeIndex + _map.Width, _deltaGSide);
                if (haveE)
                    VisitNeighbour(nextNodeX, nextNodeY, nextNodeIndex, nextNodeX - 1, nextNodeY + 1, nextNodeIndex + _map.Width - 1, _deltaGDiag);
                if (haveW)
                    VisitNeighbour(nextNodeX, nextNodeY, nextNodeIndex, nextNodeX + 1, nextNodeY + 1, nextNodeIndex + _map.Width + 1, _deltaGDiag);
            }
            return true;
        }

        public int CurrentResult() => _openList.Count > 0 && _nodes[_openList[0]].HScore <= 0 ? _openList[0] : -1;

        public int Execute()
        {
            while (ExecuteStep())
                ;
            return CurrentResult();
        }

        private void VisitNeighbour(int parentX, int parentY, int parentIndex, int nodeX, int nodeY, int nodeIndex, float deltaG)
        {
            if (_nodes[nodeIndex].OpenHeapIndex < 0)
                return; // in closed list already
            var nodeG = _nodes[parentIndex].GScore + deltaG;
            var nodeLeeway = _map.Pixels[nodeIndex].MaxG - nodeG;
            if (nodeLeeway < 0)
                return; // node is blocked along this path

            if (_nodes[nodeIndex].OpenHeapIndex == 0)
            {
                // first time we're visiting this node, calculate heuristic
                _nodes[nodeIndex].GScore = float.MaxValue;
                _nodes[nodeIndex].HScore = HeuristicDistance(nodeX, nodeY);

                // check LoS from grandparent
                int grandParentX = _nodes[parentIndex].ParentX;
                int grandParentY = _nodes[parentIndex].ParentY;
                var losLeeway = LineOfSight(grandParentX, grandParentY, nodeX, nodeY, nodeG);
                if (losLeeway >= 0)
                {
                    parentX = grandParentX;
                    parentY = grandParentY;
                    parentIndex = CellIndex(parentX, parentY);
                    nodeG = _nodes[parentIndex].GScore + _deltaGSide * MathF.Sqrt(DistanceSq(nodeX, nodeY, parentX, parentY));
                    nodeLeeway = losLeeway;
                }

                if (nodeG + 0.00001f < _nodes[nodeIndex].GScore)
                {
                    _nodes[nodeIndex].GScore = nodeG;
                    _nodes[nodeIndex].ParentX = parentX;
                    _nodes[nodeIndex].ParentY = parentY;
                    _nodes[nodeIndex].PathLeeway = MathF.Min(_nodes[parentIndex].PathLeeway, nodeLeeway);
                    AddToOpen(nodeIndex);
                }
            }
        }

        private float LineOfSight(int x1, int y1, int x2, int y2, float maxG)
        {
            float minLeeway = float.MaxValue;
            foreach (var (x, y) in _map.EnumeratePixelsInLine(x1, y1, x2, y2))
            {
                minLeeway = Math.Min(minLeeway, _map[x, y].MaxG - maxG);
                if (minLeeway < 0)
                    return minLeeway;
            }
            return minLeeway;
        }

        private float HeuristicDistance(int x, int y)
        {
            float bestSq = float.MaxValue;
            foreach (var g in _goals)
            {
                var curSq = DistanceSq(x, y, g.x, g.y);
                if (curSq < bestSq)
                    bestSq = curSq;
            }
            return _deltaGSide * MathF.Sqrt(bestSq);
        }

        private float DistanceSq(int x1, int y1, int x2, int y2)
        {
            var dx = x1 - x2;
            var dy = y1 - y2;
            return dx * dx + dy * dy;
        }

        private void AddToOpen(int nodeIndex)
        {
            if (_nodes[nodeIndex].OpenHeapIndex <= 0)
            {
                _openList.Add(nodeIndex);
                _nodes[nodeIndex].OpenHeapIndex = _openList.Count;
            }
            // update location
            PercolateUp(_nodes[nodeIndex].OpenHeapIndex - 1);
        }

        // remove first (minimal) node from open heap and mark as closed
        private int PopMinOpen()
        {
            int nodeIndex = _openList[0];
            _openList[0] = _openList[_openList.Count - 1];
            _nodes[nodeIndex].OpenHeapIndex = -1;
            _openList.RemoveAt(_openList.Count - 1);
            if (_openList.Count > 0)
            {
                _nodes[_openList[0]].OpenHeapIndex = 1;
                PercolateDown(0);
            }
            return nodeIndex;
        }

        private void PercolateUp(int heapIndex)
        {
            int nodeIndex = _openList[heapIndex];
            int parent = (heapIndex - 1) >> 1;
            while (heapIndex > 0 && HeapLess(nodeIndex, _openList[parent]))
            {
                _openList[heapIndex] = _openList[parent];
                _nodes[_openList[heapIndex]].OpenHeapIndex = heapIndex + 1;
                heapIndex = parent;
                parent = (heapIndex - 1) >> 1;
            }
            _openList[heapIndex] = nodeIndex;
            _nodes[nodeIndex].OpenHeapIndex = heapIndex + 1;
        }

        private void PercolateDown(int heapIndex)
        {
            int nodeIndex = _openList[heapIndex];
            int maxSize = _openList.Count;
            while (true)
            {
                int child1 = (heapIndex << 1) + 1;
                if (child1 >= maxSize)
                    break;
                int child2 = child1 + 1;
                if (child2 == maxSize || HeapLess(_openList[child1], _openList[child2]))
                {
                    if (HeapLess(_openList[child1], nodeIndex))
                    {
                        _openList[heapIndex] = _openList[child1];
                        _nodes[_openList[heapIndex]].OpenHeapIndex = heapIndex + 1;
                        heapIndex = child1;
                    }
                    else
                    {
                        break;
                    }
                }
                else if (HeapLess(_openList[child2], nodeIndex))
                {
                    _openList[heapIndex] = _openList[child2];
                    _nodes[_openList[heapIndex]].OpenHeapIndex = heapIndex + 1;
                    heapIndex = child2;
                }
                else
                {
                    break;
                }
            }
            _openList[heapIndex] = nodeIndex;
            _nodes[nodeIndex].OpenHeapIndex = heapIndex + 1;
        }

        private bool HeapLess(int nodeIndexLeft, int nodeIndexRight)
        {
            ref var nodeL = ref _nodes[nodeIndexLeft];
            ref var nodeR = ref _nodes[nodeIndexRight];
            var fl = nodeL.GScore + nodeL.HScore;
            var fr = nodeR.GScore + nodeR.HScore;
            if (fl + 0.00001f < fr)
                return true;
            else if (fr + 0.00001f < fl)
                return false;
            else
                return nodeL.GScore > nodeR.GScore; // tie-break towards larger g-values
        }
    }
}
