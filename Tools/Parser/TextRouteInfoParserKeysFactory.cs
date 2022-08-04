namespace PublicTransportInformationService.Tools.Parser
{
    public static class TextRouteInfoParserKeysFactory
    {
        #region Private members

        private readonly static string routesStartTimeKey = "routesStartTime";
        private readonly static string routesCostKey = "routesCost";
        private readonly static string routeStopPointsKey = "routeStops";
        private readonly static string routesStopPointsKey = "routeStopPoints";
        private readonly static string routePartsDurationKey = "routePartsMovementTime";

        #endregion

        #region Public methods
        public static string GetRoutesStartTimeKeyFor(int routeIndex)
        {
            return GetKeyWithIndex(routesStartTimeKey, routeIndex);
        }

        public static string GetRoutesCostKeyFor(int routeIndex)
        {
            return GetKeyWithIndex(routesCostKey, routeIndex);
        }

        public static string GetRouteStopsKeyFor(int routeIndex)
        {
            return GetKeyWithIndex(routeStopPointsKey, routeIndex);
        }
        public static string GetRoutesStopPointsKeyFor(int routeIndex, int routePartIndex)
        {
            return GetKeyWithIndex(routesStopPointsKey, routeIndex, routePartIndex);
        }
        
        public static string GetRoutePartsTripDurationKeyFor(int routeIndex, int routePartIndex)
        {
            return GetKeyWithIndex(routePartsDurationKey, routeIndex, routePartIndex);
        }

        #endregion

        #region Private methods
        private static string GetKeyWithIndex(string key, int routeIndex, int routePartIndex = -1)
        {
            return key + routeIndex + (routePartIndex != -1 ? "/" + routePartIndex : string.Empty);
        }

        #endregion
    }
}
