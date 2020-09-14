using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;

namespace TimetableFH.Helpers
{
    static class EventRequester
    {
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

        private static bool ContinuesWith(string text, int index, string with)
        {
            if (text.Length < index + with.Length) return false;

            for (int i = 0; i < with.Length; i++)
            {
                if (text[index + i] != with[i]) return false;
            }

            return true;
        }

        public static async Task<StorageFile> DownloadCsv(string baseUrl, string requestUrlAddition, string postData)
        {
            string csvRequestUrl = baseUrl + requestUrlAddition;
            string html = await GetBodyOfPost(csvRequestUrl, postData);
            string downloadUrl = baseUrl + GetDownloadFileName(html);

            return await DownloadFile(downloadUrl);
        }

        private static string GetDownloadFileName(string html)
        {
            //8c5a48c66e6dc7d42d71267a759bbab0.csv\";
            const string searchKey = "url = \"stg_files/exports/";
            const int indexOffset = 7;
            int index = html.IndexOf(searchKey, StringComparison.Ordinal) + indexOffset;
            int length = searchKey.Length - indexOffset;

            while (true)
            {
                if (html.Length <= index + length) throw new Exception();
                if (html[index + length] == '.') break;
                if (!IsNumberOrLowerChar(html[index + length])) throw new Exception();

                length++;
            }

            if (html[index + ++length] != 'c' || html[index + ++length] != 's' ||
                html[index + ++length] != 'v') throw new Exception();

            return html.Substring(index, length + 1);
        }

        private static bool IsNumberOrLowerChar(char c)
        {
            return char.IsNumber(c) || char.IsLower(c);
        }

        private static async Task<StorageFile> DownloadFile(string downloadUrl)
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
