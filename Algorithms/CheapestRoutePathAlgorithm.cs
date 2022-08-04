using PublicTransportInformationService.Algorithms.BaseClasses;
using PublicTransportInformationService.DataStructures;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PublicTransportInformationService.Algorithms
{
    public class CheapestRoutePathAlgorithm : RoutePathAlgorithmBase
    {
        #region Private members

        private Dictionary<int, int> leadedRouteByStopPoints = new Dictionary<int, int>();

        #endregion

        #region Constructors

        public CheapestRoutePathAlgorithm(List<RouteInfo> routesInfoList) :
            this(routesInfoList,
            routesInfoList.FirstOrDefault().RoutePartsTripDuration.FirstOrDefault().Key,
            routesInfoList.FirstOrDefault().RoutePartsTripDuration.FirstOrDefault().Key,
            TimeSpan.Zero)
        { }

        public CheapestRoutePathAlgorithm(List<RouteInfo> routesInfoList, int startPoint, int finishPoint, TimeSpan tripStartTime) : base(routesInfoList, startPoint, finishPoint, tripStartTime)
        { }

        #endregion

        #region Public methods

        public override void Initialize(int startPoint, int finishPoint, TimeSpan tripStartTime)
        {
            base.Initialize(startPoint, finishPoint, tripStartTime);

            leadedRouteByStopPoints.Clear();
        }

        #endregion

        #region Protected methods
        protected override void OnTreeEdge(TaggedEdge<int, int> edge)
        {
            base.OnTreeEdge(edge);
            leadedRouteByStopPoints[edge.Target] = edge.Tag;
        }

        protected override double WeightForEdge(TaggedEdge<int, int> edge)
        {
            double cost = 0;
            int existingLeadedRoute = leadedRouteByStopPoints.FirstOrDefault(stopPoint => stopPoint.Key == edge.Source && stopPoint.Value == edge.Tag).Value;
            if (!leadedRouteByStopPoints.Any(stopPoint => stopPoint.Key == edge.Source && stopPoint.Value == edge.Tag))
            {
                cost = routesInfoList[edge.Tag].Cost;
            }

            return cost;
        }

        #endregion

    }
}
