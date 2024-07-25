using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LineSorterApp.DataStructures;

namespace LineSorterApp.Helpers
{
    public static class RowHelpers
    {
        public static Row ToRow(this string input)
        {
            var separator = input.IndexOf('.') + 2;
            return new Row
            {
                Content = input[separator..],
                Id = input[0..(separator - 2)]
            };
        }

        public static void WriteAllRows(this List<Row> inputRows, string outputPath)
        {
            var writeStream = new StreamWriter(outputPath, false, Encoding.ASCII);
            inputRows.ForEach(row => writeStream.WriteLine(row.ToFormattedString()));
            writeStream.Close();
        }

        public static string ToFormattedString(this Row row)
        {
            return $"{row.Id}. {row.Content}";
        }
    }
}
