using PublicTransportInformationService.Tools.Parser.BaseClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PublicTransportInformationService.Tools.Parser
{
    public class TextRouteInfoParser : RouteInfoParserBase
    {
        #region Private members

        private const string s_integerValuePattern = "(?:(?:\\d+)\\s?)";
        private const string s_routesOrderedSequenceGroupName = "routesOrderedSequence";
        private const string s_routesDurationGroupName = "routePartsDuration";

        private int i_routesCount;

        private readonly Regex m_routeStopsCountRegex = new Regex(@"^\d+");

        private Queue<string> m_infoLinesQueue;
        private Queue<Action<string>> m_parsingPipeline = new Queue<Action<string>>();

        #endregion

        #region Events

        public event Action ParsedSuccessfulEvent;

        #endregion
        #region Constructors
        public TextRouteInfoParser() => ParsedSuccessfulEvent += OnParsedSuccessful;

        #endregion

        #region Public methods
        public override void Parse(string dataString)
        {
            try
            {
                InitParsingQueue(dataString);
                InitParser(dataString);

                Action<string> currentLineParseHandler = null;

                while (m_infoLinesQueue.Count > 0)
                {
                    if (m_parsingPipeline.Count > 0)
                    {
                        currentLineParseHandler = m_parsingPipeline.Dequeue();
                    }

                    currentLineParseHandler?.Invoke(m_infoLinesQueue.Dequeue());
                }

                ParsedSuccessfulEvent?.Invoke();
            }
            catch(Exception e)
            {
                IsParsedSuccessful = false;
                throw e;
            }
        }

        #endregion

        #region Private methods
        private void InitParser(string dataString)
        {
            IsParsedSuccessful = false;
            InitParsingPipeline();
        }

        private void InitParsingQueue(string dataString)
        {
            var dataRows = dataString.
                    Split('\n').
                    Select(l => l.Trim('\r'));

            m_infoLinesQueue = new Queue<string>(
                dataRows.Where(r => r != dataRows.ElementAt(1))                    //Filter stop points count for a while
                );
        }

        private void InitParsingPipeline()
        {
            m_parsingPipeline.Enqueue(ParseRoutesCountLine);
            m_parsingPipeline.Enqueue(ParseRoutesStartTime);
            m_parsingPipeline.Enqueue(ParseRouteCost);
            m_parsingPipeline.Enqueue(ParseRouteInfoLines);
        }

        private void ParseRoutesCountLine(string routesCountRow)
        {
            i_routesCount = int.Parse(routesCountRow);
        }

        private void ParseRouteCost(string routeCostInfoLine)
        {
            PutAsParsed(TextRouteInfoParserKeysFactory.GetRoutesCostKeyFor, routeCostInfoLine.Split());
        }

        private void ParseRoutesStartTime(string routesStartTimeInfoRow)
        {
            PutAsParsed(TextRouteInfoParserKeysFactory.GetRoutesStartTimeKeyFor, routesStartTimeInfoRow.Split());
        }

        private void ParseRouteInfoLines(string routeInfoLine)
        {
            int routeStopsCount = int.Parse(m_routeStopsCountRegex.Match(routeInfoLine).Value);

            string routeInfoLinePattern = $"{s_integerValuePattern}" +
                                          $"(?<{s_routesOrderedSequenceGroupName}>{s_integerValuePattern}{{{routeStopsCount}}})" +
                                          $"(?<{s_routesDurationGroupName}>{s_integerValuePattern}{{{routeStopsCount}}})";

            Regex routeInfoLineRegex = new Regex(routeInfoLinePattern);

            var routeStopPointsOrderedSequence = routeInfoLineRegex.Match(routeInfoLine).Groups[s_routesOrderedSequenceGroupName].Value.
                TrimEnd().
                Split();

            var routePartsDuration = routeInfoLineRegex.Match(routeInfoLine).Groups[s_routesDurationGroupName].Value.
                TrimEnd().
                Split();

            int currentRouteIndex = i_routesCount - (m_infoLinesQueue.Count + 1);

            PutAsParsed(new List<Tuple<Func<int, string>, string[]>>()
            {
                new Tuple<Func<int, string>, string[]>
                (
                    (partIndex) => 
                    {
                        return TextRouteInfoParserKeysFactory.GetRoutesStopPointsKeyFor(currentRouteIndex, partIndex); 
                    },
                    routeStopPointsOrderedSequence
                ),
                new Tuple<Func<int, string>, string[]>
                (
                    (partIndex) =>
                    {
                        return TextRouteInfoParserKeysFactory.GetRoutePartsTripDurationKeyFor(currentRouteIndex, partIndex);
                    },
                    routePartsDuration
                )
            });
        }

        // TODO List<Tupple> can be used
        private void PutAsParsed(List<Tuple<Func<int, string>, string[]>> keyBuildersAndData)
        {
            foreach(var keyBuilderAndData in keyBuildersAndData)
            {
                PutAsParsed(keyBuilderAndData.Item1, keyBuilderAndData.Item2);
            }
        }

        private void PutAsParsed(Func<int, string> keyBuilder, string[] parsedRoutesData)
        {
            string infoKey = string.Empty;

            for (int keyIndex = 0; keyIndex < parsedRoutesData.Length; keyIndex++)
            {
                infoKey = keyBuilder(keyIndex);

                m_parsedRoutesInfo.Add(infoKey, parsedRoutesData[keyIndex]);

            }
        }

        private void OnParsedSuccessful()
        {
            IsParsedSuccessful = true;
            ParsedSuccessfulEvent -= OnParsedSuccessful;
        }

        #endregion

    }
}
