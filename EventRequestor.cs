using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;

namespace TimtableFH
{
    class EventRequestor
    {
        private const string url = "http://stundenplan.fh-joanneum.at/index.php", postData = "user=msd&pass=msd&login=Login",
            csvRequestUrl = "http://stundenplan.fh-joanneum.at/index.php" +
            "?new_stg=MSD&new_jg=2018&new_date=1551081600&new_viewmode=matrix_vertical&write_file=1";

        public static async Task<IEnumerable<Event>> RegquestEvents(DateTime after)
        {
            try
            {
                string html = await GetBodyOfPost(url, postData);

                return GetEvents(html);
            }
            catch
            {
                return Enumerable.Empty<Event>();
            }
        }

        private static async Task<string> GetBodyOfPost(string url, string postText)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            byte[] data = Encoding.ASCII.GetBytes(postText);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            using (Stream stream = await request.GetRequestStreamAsync())
            {
                stream.Write(data, 0, data.Length);
            }

            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            {
                if (response.StatusCode != HttpStatusCode.OK) throw new Exception();

                Stream receiveStream = response.GetResponseStream();

                using (StreamReader readStream = new StreamReader(receiveStream))
                {
                    return readStream.ReadToEnd();
                }
            }
        }

        public static IEnumerable<Event> GetEvents(string html)
        {
            DateTime monday;
            int index = html.IndexOf("<body ");
            string body = GetContent(html, ref index);

            do
            {
                string dateContent = GetContent(html, ref index);
            } while (!TryGetDate(body, out monday));

            return new Event[0];
        }

        private static string GetContent(string html, ref int index)
        {
            int level = 1;
            string elementType = string.Empty, content = string.Empty;

            if (html[index] != '<') throw new Exception();

            while (index < html.Length && html[index] != ' ')
            {
                elementType += html[index++];
            }

            while (index < html.Length && html[index] != '>') index++;
            index++;

            while (index < html.Length)
            {
                if (ContinuesWith(html, index, "<" + elementType))
                {
                    level++;
                }
                else if (ContinuesWith(html, index, "</" + elementType))
                {
                    level--;
                    if (level == 0) break;
                }

                content += html[index++];
            }

            while (index < html.Length && html[index] != '>') index++;
            index++;

            return content;
        }

        private static bool ContinuesWith(string text, int index, string with)
        {
            if (text.Length < index + with.Length) return false;

            for (int i = 0; i < with.Length; i++)
            {
                if (text[index + i] != with[i]) return false;
            }

            return true;
        }

        private static bool TryGetDate(string content, out DateTime date)
        {
            date = DateTime.MinValue;

            if (content.Length != 22 || !content.StartsWith("Monday<br>")) return false;

            if (content[12] != '.' || content[13] != ' ' || content[16] != '.' || content[17] != ' ') return false;

            int day, month, year;
            if (int.TryParse(content[10].ToString() + content[11], out day)) return false;
            if (int.TryParse(content[14].ToString() + content[15], out month)) return false;
            if (int.TryParse(content[18].ToString() + content[19] + content[20] + content[21], out year)) return false;

            date = new DateTime(year, month, day);

            return true;
        }

        public static async Task<StorageFile> DownloadCsv()
        {
            string html = await GetBodyOfPost(csvRequestUrl, postData);
            string downloadUrl = GetDownloadUrl(html);

            return await DownloadFile(downloadUrl);
        }

        public static string GetDownloadUrl(string html)
        {
            //8c5a48c66e6dc7d42d71267a759bbab0.csv\";
            const string searchKey = "url = \"stg_files/exports/";
            int index = html.IndexOf(searchKey);

            if (!html.Skip(index + searchKey.Length).Take(32).All(IsNumberOrLowerChar)) throw new Exception();
            if (!ContinuesWith(html, index + searchKey.Length + 32, ".csv\"")) throw new Exception();

            return " http://stundenplan.fh-joanneum.at/" + html.Substring(index + 7, 54);
        }

        private static bool IsNumberOrLowerChar(char c)
        {
            return char.IsNumber(c) || char.IsLower(c);
        }

        private async static Task<StorageFile> DownloadFile(string downloadUrl)
        {
            Uri source = new Uri(downloadUrl);
            StorageFile destinationFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                "Timetable.csv", CreationCollisionOption.GenerateUniqueName);

            BackgroundDownloader downloader = new BackgroundDownloader();
            DownloadOperation download = downloader.CreateDownload(source, destinationFile);

            await download.StartAsync();

            return destinationFile;
        }
    }
}
