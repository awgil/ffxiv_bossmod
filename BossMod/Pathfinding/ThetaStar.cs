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
        }

        private Map _map;
        private (int x, int y)[] _goals;
        private Node[] _nodes;
        private List<int> _openList = new();
        private float _resSq;
        private float _deltaGSide;
        private float _deltaGDiag;

        public ref Node NodeByIndex(int index) => ref _nodes[index];

        public ThetaStar(Map map, IEnumerable<(int x, int y)> goals, int startX, int startY)
        {
            _map = map;
            _goals = goals.ToArray();
            _nodes = new Node[map.Width * map.Height];
            _resSq = map.Resolution * map.Resolution;
            _deltaGSide = map.Resolution;
            _deltaGDiag = _deltaGSide * 1.414214f;

            int startIndex = startY * _map.Width + startX;
            _nodes[startIndex].GScore = 0;
            _nodes[startIndex].HScore = HeuristicDistance(startX, startY);
            _nodes[startIndex].ParentX = startX; // start's parent is self
            _nodes[startIndex].ParentY = startY;
            AddToOpen(startIndex);
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
            if (_map.Pixels[nodeIndex].MaxG < nodeG)
                return; // node is blocked along this path

            if (_nodes[nodeIndex].OpenHeapIndex == 0)
            {
                // first time we're visiting this node, calculate heuristic
                _nodes[nodeIndex].GScore = float.MaxValue;
                _nodes[nodeIndex].HScore = HeuristicDistance(nodeX, nodeY);

                // check LoS from grandparent
                int grandParentX = _nodes[parentIndex].ParentX;
                int grandParentY = _nodes[parentIndex].ParentY;
                if (LineOfSight(grandParentX, grandParentY, nodeX, nodeY, nodeG))
                {
                    parentX = grandParentX;
                    parentY = grandParentY;
                    parentIndex = parentY * _map.Width + parentX;
                    var dx = nodeX - parentX;
                    var dy = nodeY - parentY;
                    nodeG = _nodes[parentIndex].GScore + _deltaGSide * MathF.Sqrt(dx * dx + dy * dy);
                }

                if (nodeG + 0.00001f < _nodes[nodeIndex].GScore)
                {
                    _nodes[nodeIndex].GScore = nodeG;
                    _nodes[nodeIndex].ParentX = parentX;
                    _nodes[nodeIndex].ParentY = parentY;
                    AddToOpen(nodeIndex);
                }
            }
        }

        private bool LineOfSight(int x1, int y1, int x2, int y2, float maxG)
        {
            int dx = x2 - x1;
            int dy = y2 - y1;
            int sx = dx > 0 ? 1 : -1;
            int sy = dy > 0 ? 1 : -1;
            dx = Math.Abs(dx);
            dy = Math.Abs(dy);
            if (dx >= dy)
            {
                int err = 2 * dy - dx;
                do
                {
                    x1 += sx;
                    if (_map[x1, y1].MaxG < maxG)
                        return false;

                    if (err > 0)
                    {
                        y1 += sy;
                        if (_map[x1, y1].MaxG < maxG)
                            return false;
                        err -= 2 * dx;
                    }
                    err += 2 * dy;
                }
                while (x1 != x2);
            }
            else
            {
                int err = 2 * dx - dy;
                do
                {
                    y1 += sy;
                    if (_map[x1, y1].MaxG < maxG)
                        return false;

                    if (err > 0)
                    {
                        x1 += sx;
                        if (_map[x1, y1].MaxG < maxG)
                            return false;
                        err -= 2 * dy;
                    }
                    err += 2 * dx;
                }
                while (y1 != y2);
            }
            return true;
        }

        private float HeuristicDistance(int x, int y)
        {
            float best = float.MaxValue;
            foreach (var g in _goals)
            {
                var cur = Distance(x, y, g.x, g.y);
                if (cur < best)
                    best = cur;
            }
            return best;
        }

        private float Distance(int x1, int y1, int x2, int y2)
        {
            var dx = x1 - x2;
            var dy = y1 - y2;
            return (dx * dx + dy * dy) * _resSq;
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
