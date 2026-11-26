using NSMSOLUTION.Models.ApprovalUtility;
using System;
using System.Globalization;
using System.Linq;

namespace NSMSOLUTION.Models.EFCore
{
    public class Utility
    {
        public static dynamic TryParse(object value, string type, object defaultValue = null)
        {
            try
            {
                if (value == null) return defaultValue;

                string str = value.ToString().Trim();
                if (string.IsNullOrEmpty(str)) return defaultValue;

                type = (type ?? "").ToLower();

                switch (type)
                {
                    case "int":
                    case "integer":
                        if (int.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out int i)) return i;
                        return defaultValue;

                    case "long":
                        if (long.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out long l)) return l;
                        return defaultValue;

                    case "float":
                        if (float.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) return f;
                        return defaultValue;

                    case "double":
                    case "number":
                        if (double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out double d)) return d;
                        return defaultValue;

                    case "decimal":
                        if (decimal.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal m)) return m;
                        return defaultValue;

                    case "date":
                    case "datetime":

                        string[] formats = new[]
                        {
                            "yyyy-MM-dd",
                            "yyyy-MM-dd HH:mm:ss",
                            "yyyy-MM-dd HH:mm:ss.fff",
                            "yyyy-MM-ddTHH:mm:ss",
                            "dd/MM/yyyy",
                            "dd/MM/yyyy HH:mm",
                            "dd/MM/yyyy hh:mm tt",
                            "dd/MM/yyyy HH:mm:ss",
                        };

                        if (DateTime.TryParseExact(
                                str,
                                formats,
                                CultureInfo.GetCultureInfo("en-GB"),
                                DateTimeStyles.None,
                                out DateTime dt))
                            return dt;

                        return defaultValue;

                    case "bool":
                    case "boolean":
                        if (bool.TryParse(str, out bool b)) return b;

                        string v = str.ToLower();
                        if (new[] { "true", "1", "yes", "y" }.Contains(v)) return true;
                        if (new[] { "false", "0", "no", "n" }.Contains(v)) return false;

                        return defaultValue;

                    case "string":
                        return str;

                    default:
                        return value;
                }
            }
            catch
            {
                return defaultValue;
            }
        }
        public static string GetEmojiByStatus(int statusId)
        {
            string emoji = statusId == (int)ApprovalAction.Approve ? "✅" :
                statusId == (int)ApprovalAction.Disapprove ? "❌" :
                statusId == (int)ApprovalAction.Check ? "✍️" :
                statusId == (int)ApprovalAction.Cancel ? "🚫" : "❌";
            return emoji;
        }
        public static string GetActionTextByStatus(int statusId)
        {
            string strAction = "";
            switch (statusId)
            {
                case (int)ApprovalAction.Check:
                    strAction = $"checked";
                    break;
                case (int)ApprovalAction.Approve:
                    strAction = $"approved";
                    break;
                case (int)ApprovalAction.Disapprove:
                    strAction = $"disapproved";
                    break;
                case (int)ApprovalAction.Cancel:
                    strAction = $"canceled";
                    break;
                case (int)ApprovalAction.Reject:
                    strAction = $"rejected";
                    break;
            }
            return strAction;
        }
        public static string FormatBytes(string bytes)
        {
            long marker = 1000;   // change to 1024 if required
            int decimalPlaces = 0;
            long bytess = TryParse(bytes, "long", 0);

            long kiloBytes = marker;
            long megaBytes = marker * marker;
            long gigaBytes = marker * marker * marker;

            return bytess < kiloBytes
                ? bytes + " Bytes"
                : bytess < megaBytes
                ? (bytess / (double)kiloBytes).ToString($"F{decimalPlaces}") + " KB"
                : bytess < gigaBytes
                ? (bytess / (double)megaBytes).ToString($"F{decimalPlaces}") + " MB"
                : (bytess / (double)gigaBytes).ToString($"F{decimalPlaces}") + " GB";
        }

    }
}