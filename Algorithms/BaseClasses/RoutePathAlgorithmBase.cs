using PublicTransportInformationService.DataStructures;
using PublicTransportInformationService.DataStructures.GraphBuilder;
using QuickGraph;
using QuickGraph.Algorithms.ShortestPath;
using System;
using System.Collections.Generic;
using System.Threading;

namespace PublicTransportInformationService.Algorithms.BaseClasses
{
    public abstract class RoutePathAlgorithmBase
    {
        #region Protected members

        protected DijkstraShortestPathAlgorithm<int, TaggedEdge<int, int>> dijkstraShortestPathAlgorithm;
        protected List<RouteInfo> routesInfoList;
        protected int finishPoint;
        protected TimeSpan tripStartTime;

        protected Dictionary<int, TaggedEdge<int, int>> pathByStopNumber = new Dictionary<int, TaggedEdge<int, int>>();

        protected CancellationToken ct;

        #endregion

        #region Properties

        public TimeSpan TripStartTime => tripStartTime;

        #endregion

        #region Constructors

        public RoutePathAlgorithmBase(List<RouteInfo> routesInfoList, int startPoint, int finishPoint, TimeSpan startTime)
        {
            this.routesInfoList = routesInfoList;

            var graph = RouteGraphBuilder.BuildRouteGraph(routesInfoList);
            dijkstraShortestPathAlgorithm = new DijkstraShortestPathAlgorithm<int, TaggedEdge<int, int>>(graph, WeightForEdge);

            Initialize(startPoint, finishPoint, startTime);
        }

        #endregion

        #region Public methods

        public virtual void Initialize(int startPoint, int finishPoint, TimeSpan tripStartTime)
        {
            this.finishPoint = finishPoint;

            if (CheckIfRecalculationIsNeeded(startPoint, tripStartTime))
            {
                this.tripStartTime = tripStartTime;

                dijkstraShortestPathAlgorithm.SetRootVertex(startPoint);

                Resubscribe();

                pathByStopNumber.Clear();
            }
        }

        public virtual void Compute(CancellationToken token)
        {
            ct = token;
            dijkstraShortestPathAlgorithm.Compute();
        }

        public virtual bool TryGetDistanceToFinish(out double distance)
        {
            distance = int.MaxValue;
            return dijkstraShortestPathAlgorithm.State == QuickGraph.Algorithms.ComputationState.Finished &&
                dijkstraShortestPathAlgorithm.TryGetDistance(finishPoint, out distance) && distance != int.MaxValue;
        }

        public virtual bool TryGetPathToFinish(out List<Tuple<int, int>> path)
        {
            bool result = TryGetDistanceToFinish(out double distance);
            if (!result)
            {
                path = null;
            }
            else if (distance == 0)
            {
                path = new List<Tuple<int, int>>() { new Tuple<int, int>(finishPoint, 0) };
            }
            else
            {
                path = new List<Tuple<int, int>>();

                var currentTripPart = pathByStopNumber[finishPoint];
                path.Add(new Tuple<int, int>(currentTripPart.Target, currentTripPart.Tag));

                dijkstraShortestPathAlgorithm.TryGetRootVertex(out int startPoint);
                while (currentTripPart.Source != startPoint)
                {
                    path.Add(new Tuple<int, int>(currentTripPart.Source, currentTripPart.Tag));
                    currentTripPart = pathByStopNumber[currentTripPart.Source];
                }

                path.Add(new Tuple<int, int>(startPoint, currentTripPart.Tag));
                path.Reverse();
            }

            return result;
        }

        public int GetCurrentStartPoint()
        {
            dijkstraShortestPathAlgorithm.TryGetRootVertex(out int rootVertex);

            return rootVertex;
        }

        public bool CheckIfRecalculationIsNeeded(int newStartPoint, TimeSpan newStartTime)
        {
            return dijkstraShortestPathAlgorithm.TryGetRootVertex(out int rootVertex) ||
                rootVertex != newStartPoint || tripStartTime != newStartTime;
        }

        #endregion

        #region Protected methods

        protected abstract double WeightForEdge(TaggedEdge<int, int> edge);

        protected virtual void OnTreeEdge(TaggedEdge<int, int> edge)
        {
            pathByStopNumber[edge.Target] = edge;
        }

        protected void OnFinished(object sender, EventArgs args)
        {
            Unsubscribe();
        }

        protected void Resubscribe()
        {
            Unsubscribe();
            Subscribe();
        }

        protected void Subscribe()
        {
            dijkstraShortestPathAlgorithm.TreeEdge += OnTreeEdge;
            dijkstraShortestPathAlgorithm.ExamineEdge += CheckCancel;
            dijkstraShortestPathAlgorithm.Finished += OnFinished;
        }

        private void CheckCancel(TaggedEdge<int, int> e)
        {
            ct.ThrowIfCancellationRequested();
        }

        protected void Unsubscribe()
        {
            dijkstraShortestPathAlgorithm.TreeEdge -= OnTreeEdge;
            dijkstraShortestPathAlgorithm.ExamineEdge -= CheckCancel;
            dijkstraShortestPathAlgorithm.Finished -= OnFinished;
        }

        #endregion

    }
}
