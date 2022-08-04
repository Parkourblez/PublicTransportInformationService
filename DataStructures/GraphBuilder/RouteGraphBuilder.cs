using System.Collections.Generic;
using QuickGraph;
using System.Linq;

namespace PublicTransportInformationService.DataStructures.GraphBuilder
{
    public static class RouteGraphBuilder
    {
        public static BidirectionalGraph<int, TaggedEdge<int, int>> BuildRouteGraph(List<RouteInfo> routesInfoList)
        {
            BidirectionalGraph<int, TaggedEdge<int, int>> graph = new BidirectionalGraph<int, TaggedEdge<int, int>>(true);

            List<TaggedEdge<int, int>> edgeList = new List<TaggedEdge<int, int>>();
            foreach (var routeInfo in routesInfoList)
            {
                List<int> verteces = routeInfo.RoutePartsTripDuration.Keys.ToList();

                for (int i = 0; i < verteces.Count; i++)
                {
                    int routeIndex = routesInfoList.IndexOf(routeInfo);
                    int vi = verteces[i];
                    int vj = verteces[(i + 1) % verteces.Count];

                    edgeList.Add(new TaggedEdge<int, int>(vi, vj, routeIndex));
                }
            }
            graph.AddVerticesAndEdgeRange(edgeList);

            return graph;
        }
    }
}
