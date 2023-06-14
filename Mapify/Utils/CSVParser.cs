using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Mapify.Utils
{
    public static class CSV
    {
        /// <summary>
        ///     Parses a CSV string into a dictionary of columns, each of which is a dictionary of rows, keyed by the first column.
        /// </summary>
        public static ReadOnlyDictionary<string, Dictionary<string, string>> Parse(string data)
        {
            string[] lines = data.Split('\n');

            // Dictionary<string, Dictionary<string, string>>
            OrderedDictionary columns = new OrderedDictionary(lines.Length - 1);

            List<string> keys = ParseLine(lines[0]);
            foreach (string key in keys)
                columns.Add(key, new Dictionary<string, string>());

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                List<string> values = ParseLine(line);
                if (values.Count == 0)
                    continue;
                string key = values[0];
                for (int j = 0; j < values.Count; j++)
                    ((Dictionary<string, string>)columns[j]).Add(key, values[j]);
            }

            return new ReadOnlyDictionary<string, Dictionary<string, string>>(columns.Cast<DictionaryEntry>()
                .ToDictionary(entry => (string)entry.Key, entry => (Dictionary<string, string>)entry.Value));
        }

        private static List<string> ParseLine(string line)
        {
            bool inQuotes = false;
            List<string> values = new List<string>();
            StringBuilder builder = new StringBuilder();

            void FinishLine()
            {
                values.Add(builder.ToString());
                builder.Clear();
            }

            foreach (char c in line)
            {
                if (c == '\n' || (!inQuotes && c == ','))
                {
                    FinishLine();
                    continue;
                }

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }

                builder.Append(c);
            }

            if (builder.Length > 0)
                FinishLine();

            return values;
        }
    }
}
