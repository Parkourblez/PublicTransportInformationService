using PublicTransportInformationService.Tools.Parser.BaseClasses;
using System.Collections.Generic;

namespace PublicTransportInformationService.DataStructures.RouteInfoFactory.BaseClasses
{
    public abstract class RouteInfoFactoryBase
    {
        protected RouteInfoParserBase m_parser;

        public RouteInfoFactoryBase(RouteInfoParserBase parser) => m_parser = parser;

        public abstract List<RouteInfo> GenerateRoutesInfoBasedOn(string data);
    }
}
