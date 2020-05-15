using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace RaptorShock
{
    public static class PaginationTools
    {
        public delegate Tuple<string, Color> LineFormatterDelegate(object lineData, int lineIndex, int pageNumber);

        #region [Nested: Settings Class]
        public class Settings
        {
            public bool IncludeHeader { get; set; }

            private string headerFormat;
            public string HeaderFormat
            {
                get { return this.headerFormat; }
                set
                {
                    if (value == null)
                        throw new ArgumentNullException();

                    this.headerFormat = value;
                }
            }

            public Color HeaderTextColor { get; set; }
            public bool IncludeFooter { get; set; }

            private string footerFormat;
            public string FooterFormat
            {
                get { return this.footerFormat; }
                set
                {
                    if (value == null)
                        throw new ArgumentNullException();

                    this.footerFormat = value;
                }
            }

            public Color FooterTextColor { get; set; }
            public string NothingToDisplayString { get; set; }
            public LineFormatterDelegate LineFormatter { get; set; }
            public Color LineTextColor { get; set; }

            private int maxLinesPerPage;

            public int MaxLinesPerPage
            {
                get { return this.maxLinesPerPage; }
                set
                {
                    if (value <= 0)
                        throw new ArgumentException("The value has to be greater than zero.");

                    this.maxLinesPerPage = value;
                }
            }

            private int pageLimit;

            public int PageLimit
            {
                get { return this.pageLimit; }
                set
                {
                    if (value < 0)
                        throw new ArgumentException("The value has to be greater than or equal to zero.");

                    this.pageLimit = value;
                }
            }


            public Settings()
            {
                this.IncludeHeader = true;
                this.headerFormat = "Page {0} of {1}";
                this.HeaderTextColor = Color.Green;
                this.IncludeFooter = true;
                this.footerFormat = "Type /<command> {0} for more.";
                this.FooterTextColor = Color.Yellow;
                this.NothingToDisplayString = null;
                this.LineFormatter = null;
                this.LineTextColor = Color.Yellow;
                this.maxLinesPerPage = 4;
                this.pageLimit = 0;
            }
        }
        #endregion

        public static void SendPage(int pageNumber, IEnumerable dataToPaginate, int dataToPaginateCount, Settings settings = null)
        {
            if (settings == null)
                settings = new Settings();

            if (dataToPaginateCount == 0)
            {
                if (settings.NothingToDisplayString != null)
                {
                    Utils.ShowMessage(settings.NothingToDisplayString, settings.HeaderTextColor);
                }
                return;
            }

            int pageCount = ((dataToPaginateCount - 1) / settings.MaxLinesPerPage) + 1;
            if (settings.PageLimit > 0 && pageCount > settings.PageLimit)
                pageCount = settings.PageLimit;
            if (pageNumber > pageCount)
                pageNumber = pageCount;

            if (settings.IncludeHeader)
            {
                Utils.ShowMessage(string.Format(settings.HeaderFormat, pageNumber, pageCount), settings.HeaderTextColor);
            }

            int listOffset = (pageNumber - 1) * settings.MaxLinesPerPage;
            int offsetCounter = 0;
            int lineCounter = 0;
            foreach (object lineData in dataToPaginate)
            {
                if (lineData == null)
                    continue;
                if (offsetCounter++ < listOffset)
                    continue;
                if (lineCounter++ == settings.MaxLinesPerPage)
                    break;

                string lineMessage;
                Color lineColor = settings.LineTextColor;
                if (lineData is Tuple<string, Color>)
                {
                    var lineFormat = (Tuple<string, Color>)lineData;
                    lineMessage = lineFormat.Item1;
                    lineColor = lineFormat.Item2;
                }
                else if (settings.LineFormatter != null)
                {
                    try
                    {
                        Tuple<string, Color> lineFormat = settings.LineFormatter(lineData, offsetCounter, pageNumber);
                        if (lineFormat == null)
                            continue;

                        lineMessage = lineFormat.Item1;
                        lineColor = lineFormat.Item2;
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(
                          "The method referenced by LineFormatter has thrown an exception. See inner exception for details.", ex);
                    }
                }
                else
                {
                    lineMessage = lineData.ToString();
                }

                if (lineMessage != null)
                {
                   Utils.ShowMessage(lineMessage, lineColor);
                }
            }

            if (lineCounter == 0)
            {
                if (settings.NothingToDisplayString != null)
                {
                    Utils.ShowMessage(settings.NothingToDisplayString, settings.HeaderTextColor);
                }
            }
            else if (settings.IncludeFooter && pageNumber + 1 <= pageCount)
            {
               Utils.ShowMessage(string.Format(settings.FooterFormat, pageNumber + 1, pageNumber, pageCount), settings.FooterTextColor);
            }
        }

        public static void SendPage(int pageNumber, IList dataToPaginate, Settings settings = null)
        {
            PaginationTools.SendPage(pageNumber, dataToPaginate, dataToPaginate.Count, settings);
        }

        public static List<string> BuildLinesFromTerms(IEnumerable terms, Func<object, string> termFormatter = null, string separator = ", ", int maxCharsPerLine = 80)
        {
            List<string> lines = new List<string>();
            StringBuilder lineBuilder = new StringBuilder();

            foreach (object term in terms)
            {
                if (term == null && termFormatter == null)
                    continue;

                string termString;
                if (termFormatter != null)
                {
                    try
                    {
                        if ((termString = termFormatter(term)) == null)
                            continue;
                    }
                    catch (Exception ex)
                    {
                        throw new ArgumentException(
                          "The method represented by termFormatter has thrown an exception. See inner exception for details.", ex);
                    }
                }
                else
                {
                    termString = term.ToString();
                }

                if (lineBuilder.Length + termString.Length + separator.Length < maxCharsPerLine)
                {
                    lineBuilder.Append(termString).Append(separator);
                }
                else
                {
                    lines.Add(lineBuilder.ToString());
                    lineBuilder.Clear().Append(termString).Append(separator);
                }
            }

            if (lineBuilder.Length > 0)
            {
                lines.Add(lineBuilder.ToString().Substring(0, lineBuilder.Length - separator.Length));
            }
            return lines;
        }

        public static bool TryParsePageNumber(List<string> commandParameters, int expectedParameterIndex, out int pageNumber)
        {
            pageNumber = 1;
            if (commandParameters.Count <= expectedParameterIndex)
                return true;

            string pageNumberRaw = commandParameters[expectedParameterIndex];
            if (!int.TryParse(pageNumberRaw, out pageNumber) || pageNumber < 1)
            {
                Utils.ShowErrorMessage($"\"{pageNumberRaw}\" is not a valid page number.");
                pageNumber = 1;
                return false;
            }

            return true;
        }
    }
}
