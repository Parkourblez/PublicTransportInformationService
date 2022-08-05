using PublicTransportInformationService.DataStructures.RouteInfoFactory.BaseClasses;
using PublicTransportInformationService.Tools.Parser;
using PublicTransportInformationService.Tools.Parser.BaseClasses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PublicTransportInformationService.DataStructures.RouteInfoFactory
{
    public class TextRouteInfoFactory : RouteInfoFactoryBase
    {
        #region Constructors

        public TextRouteInfoFactory(RouteInfoParserBase parser) : base(parser) { }

        #endregion

        #region Public mthods
        
        public override List<RouteInfo> GenerateRoutesInfoBasedOn(string stringData)
        {
            parser.Parse(stringData);

            if (!parser.IsParsedSuccessful)
            {
                return null;
            }

            List<RouteInfo> routesInfoList = new List<RouteInfo>();

            int routeIndex = 0;
            int routePartIndex = 0;
            int routeStopPointNumber;
            TimeSpan routePartTripDuration;
            int routeCost;
            int[] hhmm;
            TimeSpan routeStartTime;
            Dictionary<int, TimeSpan> routePartsTripDuration = null;

            RouteInfo routeInfo;

            string key;
            while (parser[key = TextRouteInfoParserKeysFactory.GetRoutesStartTimeKeyFor(routeIndex)] != null)
            {
                routeStartTime = TimeSpan.Parse(parser[key]);

                key = TextRouteInfoParserKeysFactory.GetRoutesCostKeyFor(routeIndex);
                routeCost = int.Parse(parser[key]);

                routePartsTripDuration = new Dictionary<int, TimeSpan>();
                while (parser[key = TextRouteInfoParserKeysFactory.GetRoutesStopPointsKeyFor(routeIndex, routePartIndex)] != null)
                {
                    routeStopPointNumber = int.Parse(parser[key]);

                    key = TextRouteInfoParserKeysFactory.GetRoutePartsTripDurationKeyFor(routeIndex, routePartIndex);
                    routePartTripDuration = TimeSpan.FromMinutes(int.Parse(parser[key]));

                    routePartsTripDuration.Add(routeStopPointNumber, routePartTripDuration);

                    routePartIndex++;
                }

                routeInfo = new RouteInfo(routeIndex, routeCost, routeStartTime)
                {
                    RoutePartsTripDuration = routePartsTripDuration
                };

                routesInfoList.Add(routeInfo);

                routePartIndex = 0;
                routeIndex++;
            }

            return routesInfoList;
        }

        #endregion
    }
}
