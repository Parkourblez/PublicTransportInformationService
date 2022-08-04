using System.Collections;
using System.Collections.Generic;

namespace PublicTransportInformationService.Tools.Parser.BaseClasses
{
    public abstract class RouteInfoParserBase : IEnumerable<string>
    {
        protected readonly Dictionary<string, string> m_parsedRoutesInfo = new Dictionary<string, string>();

        public bool IsParsedSuccessful { get; protected set; }

        public string this[string key] => m_parsedRoutesInfo.
                                          ContainsKey(key) ?
                                                    m_parsedRoutesInfo[key] :
                                                    null;

        public abstract void Parse(string data);

        IEnumerator<string> IEnumerable<string>.GetEnumerator()
        {
            return m_parsedRoutesInfo?.Keys.GetEnumerator();
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable<string>)this).GetEnumerator();
        }
    }
}
