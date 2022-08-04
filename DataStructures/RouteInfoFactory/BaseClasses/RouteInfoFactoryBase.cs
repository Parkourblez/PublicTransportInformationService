using PublicTransportInformationService.Tools.Parser.BaseClasses;
using System.Collections.Generic;

namespace PublicTransportInformationService.DataStructures.RouteInfoFactory.BaseClasses
{
    public abstract class RouteInfoFactoryBase
    {
        protected RouteInfoParserBase parser;

        public RouteInfoFactoryBase(RouteInfoParserBase parser) => this.parser = parser;

        public abstract List<RouteInfo> GenerateRoutesInfoBasedOn(string data);
    }
}
