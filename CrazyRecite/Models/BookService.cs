using CrazyReciteApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CrazyReciteApi.Utils;

namespace CrazyRecite.Models
{
    public class ContentType
    {
        public const string json = "application/json";
        public const string octet_stream = "application/octet-stream";
        public const string text_plain = "text/plain";
        public const string form_data="multipart/form-data";
    }

    public class BookService
    {
        private const string key = "rangeServer";
        readonly string _url = "/api/books/{0}";

        private readonly HttpClientHandler hander;

        private readonly HttpClient client;

        public BookService()
        {
            hander = new HttpClientHandler()
            {
                AllowAutoRedirect = false,
                Proxy = null,
            };

            client = new HttpClient(hander)
            {
                BaseAddress = new Uri(ConfigurationUtil.GetValue(key)),
                Timeout = new TimeSpan(0, 0, 0, 50),
            };
            client.DefaultRequestHeaders.Connection.Add("keep-alive");
        }

        private void SetAccept(params string[] accepts)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            foreach (string accept in accepts)
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(accept));
            }
        }

        private ResultData<T> GetResult<T>(string api)
        {
            string url = string.Format(_url, api);
            SetAccept(ContentType.json);
            var response = client.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();
            ResultData<T> result = null;

            if (response.IsSuccessStatusCode)
            {
                string resultStr = response.Content.ReadAsStringAsync().Result;
                result = JsonConvert.DeserializeObject<ResultData<T>>(resultStr);
            }

            return result;
        }

        public IEnumerable<Books> GetBooksList()
        {
            var result = GetResult<IEnumerable<Books>>("getBooks");
            IEnumerable<Books> books = new List<Books>();

            if (result.MsgCode == ResultTypes.Success)
            {
                books = result.Content ?? books;
            }

            return books;
        }

        public void DownloadBook(string file, string savePath)
        {
            string _action = "DownloadFile";
            string fullPath = Path.Combine(savePath, file);
            string url = string.Format(_url, _action);
            try
            {
                //如果存在相同文件名，则删除原来的文件
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
                var content = new FormUrlEncodedContent(new Dictionary<string, string>(){{ "bookName", file } });
                // 设置参数
                SetAccept(ContentType.json);
                var response = client.PostAsync(url, content).Result;
                response.EnsureSuccessStatusCode();
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                using (Stream st = response.Content.ReadAsStreamAsync().Result)
                {
                    using (Stream so = new FileStream(fullPath, System.IO.FileMode.Create))
                    {
                        byte[] by = new byte[1024];
                        int osize = st.Read(by, 0, by.Length);
                        while (osize > 0)
                        {
                            so.Write(by, 0, osize);
                            osize = st.Read(by, 0, (int)by.Length);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
                throw ex;
            }
        }

        public string UploadFile(string[] paths)
        {
            string result = string.Empty;
            string _action = "UploadFile";
            string url = string.Format(_url, _action);
            try
            {
                var content = new MultipartFormDataContent();
                foreach (var fullPath in paths)
                {
                    if (File.Exists(fullPath))
                    {
                        content.Add(new ByteArrayContent(File.ReadAllBytes(fullPath)), Path.GetFileName(fullPath));
                    }
                }
                if (content.Count()>0)
                {
                    SetAccept(ContentType.form_data);
                    var response = client.PostAsync(url, content).Result;
                    response.EnsureSuccessStatusCode();
                    result = response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    result = "no file";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }
    }
}
