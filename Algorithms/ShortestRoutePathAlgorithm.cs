using PublicTransportInformationService.Algorithms.BaseClasses;
using PublicTransportInformationService.DataStructures;
using PublicTransportInformationService.DataStructures.GraphBuilder;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PublicTransportInformationService.Algorithms
{
    public class ShortestRoutePathAlgorithm : RoutePathAlgorithmBase
    {
        #region Private members

        private readonly List<VertexTimeInfo> verticesTimeInfoList = new List<VertexTimeInfo>();

        #endregion

        #region Constructors

        public ShortestRoutePathAlgorithm(List<RouteInfo> routesInfoList) :
            this(routesInfoList,
            routesInfoList.FirstOrDefault().RoutePartsTripDuration.FirstOrDefault().Key,
            routesInfoList.FirstOrDefault().RoutePartsTripDuration.FirstOrDefault().Key,
            TimeSpan.Zero)
        { }

        public ShortestRoutePathAlgorithm(List<RouteInfo> routesInfoList, int startPoint, int finishPoint, TimeSpan tripStartTime) : base(routesInfoList, startPoint, finishPoint, tripStartTime)
        { }

        #endregion

        #region Public methods
        public override void Initialize(int startPoint, int finishPoint, TimeSpan tripStartTime)
        {
            base.Initialize(startPoint, finishPoint, tripStartTime);

            verticesTimeInfoList.Clear();
            verticesTimeInfoList.Add(new VertexTimeInfo(startPoint, tripStartTime));
        }

        #endregion

        #region Protected methods
        protected override double WeightForEdge(TaggedEdge<int, int> edge)
        {
            var sourceVertex = verticesTimeInfoList.FirstOrDefault(vertexInfo => vertexInfo.VertexId == edge.Source);
            TimeSpan waitTime = TimeSpan.Zero;
            if (sourceVertex != null)
            {
                var busArrivalTime = routesInfoList[edge.Tag].GetClosestArrivalTimeForStopPoint(sourceVertex.ArrivalTime, sourceVertex.VertexId);
                waitTime = busArrivalTime - sourceVertex.ArrivalTime;
            }

            var tripTime = (waitTime + routesInfoList[edge.Tag].RoutePartsTripDuration[edge.Source]).TotalMinutes;

            return tripTime;
        }

        protected override void OnTreeEdge(TaggedEdge<int, int> edge)
        {
            base.OnTreeEdge(edge);

            var distance = dijkstraShortestPathAlgorithm.Distances[edge.Target];
            var arrivalTimeToTarget = tripStartTime.Add(TimeSpan.FromMinutes(distance));

            VertexTimeInfo targetVertexInfo = verticesTimeInfoList.FirstOrDefault(vertexInfo => vertexInfo.VertexId == edge.Target);
            if (targetVertexInfo != null)
            {
                targetVertexInfo.ArrivalTime = arrivalTimeToTarget;
            }
            else
            {
                verticesTimeInfoList.Add(new VertexTimeInfo(edge.Target, arrivalTimeToTarget));
            }
        }

        #endregion

        #region Nested types

        private class VertexTimeInfo
        {
            public readonly int VertexId;
            public TimeSpan ArrivalTime;

            public VertexTimeInfo(int vertexId, TimeSpan arrivalTime)
            {
                VertexId = vertexId;
                ArrivalTime = arrivalTime;
            }
        }

        #endregion

    }
}
