using System.Collections;
using System.Collections.Generic;

namespace PublicTransportInformationService.Tools.Parser.BaseClasses
{
    public abstract class RouteInfoParserBase : IEnumerable<string>
    {
        protected readonly Dictionary<string, string> parsedRoutesInfo = new Dictionary<string, string>();

        public bool IsParsedSuccessful { get; protected set; }

        public string this[string key] => parsedRoutesInfo.
                                          ContainsKey(key) ?
                                                    parsedRoutesInfo[key] :
                                                    null;

        public abstract void Parse(string data);

        IEnumerator<string> IEnumerable<string>.GetEnumerator()
        {
            return parsedRoutesInfo?.Keys.GetEnumerator();
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable<string>)this).GetEnumerator();
        }
    }
}
