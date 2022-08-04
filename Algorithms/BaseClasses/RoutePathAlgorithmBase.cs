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
            this.tripStartTime = tripStartTime;
            this.finishPoint = finishPoint;

            dijkstraShortestPathAlgorithm.SetRootVertex(startPoint);

            Resubscribe(OnTreeEdge);

            pathByStopNumber.Clear();
        }

        public virtual void Compute(CancellationToken token)
        {
            ct = token;
            dijkstraShortestPathAlgorithm.Compute();
        }

        public virtual bool TryGetDistanceToFinish(out double distance)
        {
            return dijkstraShortestPathAlgorithm.TryGetDistance(finishPoint, out distance);
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

        #endregion

        #region Protected methods

        protected abstract double WeightForEdge(TaggedEdge<int, int> edge);

        protected virtual void OnTreeEdge(TaggedEdge<int, int> edge)
        {
            pathByStopNumber[edge.Target] = edge;
        }

        protected void OnFinished(object sender, EventArgs args)
        {
            Unsubscribe(OnTreeEdge);
        }

        protected void Resubscribe(EdgeAction<int, TaggedEdge<int, int>> action)
        {
            Unsubscribe(action);
            Subscribe(action);
        }

        protected void Subscribe(EdgeAction<int, TaggedEdge<int, int>> action)
        {
            dijkstraShortestPathAlgorithm.ExamineEdge += CheckCancel;
            dijkstraShortestPathAlgorithm.TreeEdge += action;
            dijkstraShortestPathAlgorithm.Finished += OnFinished;
        }

        private void CheckCancel(TaggedEdge<int, int> e)
        {
            ct.ThrowIfCancellationRequested();
        }

        protected void Unsubscribe(EdgeAction<int, TaggedEdge<int, int>> action)
        {
            dijkstraShortestPathAlgorithm.TreeEdge -= action;
            dijkstraShortestPathAlgorithm.Finished -= OnFinished;
            dijkstraShortestPathAlgorithm.ExamineEdge -= CheckCancel;
        }

        #endregion

    }
}
