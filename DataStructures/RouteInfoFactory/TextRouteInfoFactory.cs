using PublicTransportInformationService.DataStructures.RouteInfoFactory.BaseClasses;
using PublicTransportInformationService.Tools.Parser;
using PublicTransportInformationService.Tools.Parser.BaseClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PublicTransportInformationService.DataStructures.RouteInfoFactory
{
    public class TextRouteInfoFactory : RouteInfoFactoryBase
    {
        #region Private members

        private Regex indexesRegex = new Regex(@"(\d+)/?(\d*)");

        #endregion

        #region Constructors
        public TextRouteInfoFactory(RouteInfoParserBase parser) : base(parser) { }

        #endregion

        #region Public mthods
        
        public override List<RouteInfo> GenerateRoutesInfoBasedOn(string stringData)
        {
            m_parser.Parse(stringData);

            if (!m_parser.IsParsedSuccessful)
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
            while (m_parser[key = TextRouteInfoParserKeysFactory.GetRoutesStartTimeKeyFor(routeIndex)] != null)
            {
                hhmm = m_parser[key].Split(':').Select(val => int.Parse(val)).ToArray();
                routeStartTime = new TimeSpan(hhmm[0], hhmm[1], 0);

                key = TextRouteInfoParserKeysFactory.GetRoutesCostKeyFor(routeIndex);
                routeCost = int.Parse(m_parser[key]);

                routePartsTripDuration = new Dictionary<int, TimeSpan>();
                while (m_parser[key = TextRouteInfoParserKeysFactory.GetRoutesStopPointsKeyFor(routeIndex, routePartIndex)] != null)
                {
                    routeStopPointNumber = int.Parse(m_parser[key]);

                    key = TextRouteInfoParserKeysFactory.GetRoutePartsTripDurationKeyFor(routeIndex, routePartIndex);
                    routePartTripDuration = TimeSpan.FromMinutes(int.Parse(m_parser[key]));

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
