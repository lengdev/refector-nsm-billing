
using System;
using System.Text;

namespace NSMSOLUTION.Services
{
    public class ChangeBuilder
    {
        private readonly StringBuilder _sb = new StringBuilder();

        private static string N(object value)
        {
            if (value == null) return "";

            if (value is DateTime date)
                return date.ToString("dd-MMM-yyyy h:mm tt");

            var s = Convert.ToString(value)?.Trim() ?? "";

            return s == "0" ? "" : s;
        }

        public void Field(
            string label,
            object oldV,
            object newV,
            bool secret = false)
        {
            var oldValue = N(oldV);
            var newValue = N(newV);

            if (oldValue == newValue)
                return;

            if (secret)
            {
                _sb.Append(label + ": changed;");
                return;
            }

            _sb.Append(
                label + ": " +
                (oldValue == "" ? "(empty)" : oldValue) +
                " → " +
                (newValue == "" ? "(empty)" : newValue) +
                ";"
            );
        }

        public void Add(string label, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            _sb.Append(label + ": " + value + ";");
        }

        public bool Any => _sb.Length > 0;

        public override string ToString()
        {
            return _sb.ToString();
        }
    }
}
