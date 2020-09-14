using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using TimetableFH.Models;

namespace TimetableFH.Helpers
{
    static class CsvParser
    {
        public static async Task<IEnumerable<Event>> GetEvents(StorageFile file)
        {
            byte[] fileData = await GetBytes(file);
            string data = Encoding.UTF8.GetString(fileData, 0, fileData.Length);
            string[] lines = data.Split('\n').Select(l => l.TrimEnd('\r')).ToArray();

            if (lines.Length == 0) return Enumerable.Empty<Event>();

            Event[] events = new Event[lines.Length - 1];

            Parallel.ForEach(GetCsvLines(lines), (d, s, i) => events[i] = new Event(d));

            return events;
        }

        private static async Task<byte[]> GetBytes(IStorageFile file)
        {
            using (Stream stream = await file.OpenStreamForReadAsync())
            {
                List<byte> buffer = new List<byte>();

                while (true)
                {
                    byte[] bytes = new byte[1024];
                    int count = await stream.ReadAsync(bytes, 0, bytes.Length);

                    if (count == 0) return buffer.ToArray();

                    buffer.AddRange(bytes.Take(count));
                }
            }
        }

        private static IEnumerable<Dictionary<string, string>> GetCsvLines(string[] lines)
        {
            string[] headers = Split(lines[0]).ToArray();

            for (int i = 1; i < lines.Length; i++)
            {
                int headerIndex = 0;
                Dictionary<string, string> dict = new Dictionary<string, string>();

                foreach (string value in Split(lines[i]))
                {
                    dict.Add(headers[headerIndex++], value);
                }

                yield return dict;
            }
        }

        private static IEnumerable<string> Split(string line)
        {
            int index = 0;

            while (index < line.Length)
            {
                bool withQuotation;
                string value = string.Empty;

                if (line[index] == '\"')
                {
                    withQuotation = true;
                    index++;
                }
                else withQuotation = false;

                while (index < line.Length)
                {
                    if (withQuotation && line[index] == '"' && (index + 2 > line.Length || line[index + 1] == ',' && line[index + 2] == '"'))
                    {
                        index++;
                        break;
                    }
                    else if (!withQuotation && line[index] == ',') break;

                    value += line[index++];
                }

                if (index < line.Length)
                {
                    if (line[index] != ',') throw new Exception();
                    else index++;
                }

                yield return value;
            }
        }
    }
}
