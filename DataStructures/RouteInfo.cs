using System;
using System.Collections.Generic;
using System.Linq;

namespace PublicTransportInformationService.DataStructures
{
    public class RouteInfo
    {
        #region Public members

        public readonly int RouteId;
        public readonly int Cost;
        public readonly TimeSpan RouteStartTime;
        public Dictionary<int, TimeSpan> RoutePartsTripDuration;

        #endregion

        #region Constructors

        private RouteInfo() { }

        public RouteInfo(int routeId, int cost, TimeSpan routeStartTime)
        {
            RouteId = routeId;
            Cost = cost;
            RouteStartTime = routeStartTime;
            RoutePartsTripDuration = null;
        }

        #endregion

        #region Public methods
        public TimeSpan GetClosestArrivalTimeForStopPoint(TimeSpan fromTime, int stopPoint)
        {
            TimeSpan arrivalTime = RouteStartTime;
            var routeStopPointsKeyList = RoutePartsTripDuration.Keys.ToList();

            var oneDay = TimeSpan.FromDays(1);
            for (int routeStopPointKeyIndex = 0;
                routeStopPointsKeyList[routeStopPointKeyIndex] != stopPoint || fromTime - arrivalTime > TimeSpan.Zero;
                routeStopPointKeyIndex = (routeStopPointKeyIndex + 1) % routeStopPointsKeyList.Count)
            {
                arrivalTime += RoutePartsTripDuration[routeStopPointsKeyList[routeStopPointKeyIndex]];
                if(arrivalTime > oneDay)
                {
                    arrivalTime = RouteStartTime;
                }
            }

            return arrivalTime;
        }

        #endregion

    }
}