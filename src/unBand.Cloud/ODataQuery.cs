using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace unBand.Cloud
{
    internal enum ODataOperator 
    {
        eq,
        gt,
        ge,
        lt,
        le,
        ne,
    }

    internal struct ODataFilter
    {
        internal string Property { get; set; }
        internal ODataOperator Operator { get; set; }
        internal object Value { get; set; }
    }

    internal class ODataQuery
    {
        public int TopItemCount { get; set; }

        private List<ODataFilter> _filters = new List<ODataFilter>();

        /// <summary>
        /// For now this only supports "and" query portions
        /// 
        /// For filter string options / construction see: http://msdn.microsoft.com/en-us/library/azure/ff683669.aspx
        /// </summary>
        /// <param name="property"></param>
        /// <param name="op"></param>
        /// <param name="value"></param>
        public void AddFilter(string property, ODataOperator op, object value)
        {
            _filters.Add(new ODataFilter() { Property = property, Operator = op, Value = value });
        }

        public string GenerateQuery()
        {
            var sb = new StringBuilder();

            if (TopItemCount > 0)
            {
                sb.Append("$top=");
                sb.Append(TopItemCount);
                sb.Append("&");
            }

            // indicator that this the first filter, used for appending an and.
            // the alternative is a for loop :)
            var first = true;

            foreach (var filter in _filters)
            {
                if (first)
                {
                    sb.Append("$filter=");
                } 
                else
                {
                    sb.Append("+and+");
                }

                // TODO? we could get cute and shorthand bool operations by only 
                //       putting the property name if it's a bool (and "not <property>")
                //       but there's no point right now.
                sb.Append(filter.Property).Append('+').Append(filter.Operator.ToString()).Append('+');

                if (filter.Value is DateTime)
                {
                    // The format of the datetime offset is UTC
                    // A sample value along with code to create it is at http://msdn.microsoft.com/library/azure/dd894027.aspx
                    // The actual serive accepts dates that are a little different to the one in the article, for example:
                    // 2014-12-08T00:00:00.0000000-08:00
                    
                    // ...though it turns out that the service also accepts plain dates with no time which is really what we want.
                    sb.Append("datetimeoffset'").Append(((DateTime)filter.Value).ToUniversalTime().ToString("yyyy-MM-dd")).Append('\'');
                }
                else if (IsNumber(filter.Value))
                {
                    sb.Append(filter.Value);
                }
                else
                {
                    sb.Append('\'').Append(WebUtility.UrlEncode(filter.Value.ToString())).Append('\'');
                }

                first = false;
            }

            // note: we don't call encode here, since it encodes the wrong items (for example, & etc), 
            // so we do manual encoding above
            return sb.ToString();
        }

        /// <summary>
        /// Helper function to check if a type is a number
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool IsNumber(object value)
        {
            return
                value is int ||
                value is Int16 ||
                value is Int32 ||
                value is Int64 ||
                value is uint ||
                value is UInt16 ||
                value is UInt32 ||
                value is UInt64 ||
                value is double ||
                value is byte ||
                value is short ||
                value is ushort ||
                value is long ||
                value is ulong ||
                value is double ||
                value is decimal;
        }

    }

}
