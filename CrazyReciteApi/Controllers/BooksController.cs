using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using CrazyReciteApi.Models;
using Newtonsoft.Json.Linq;

namespace CrazyReciteApi.Controllers
{
    public class BooksController : ApiController
    {
        private CrDBContext db = new CrDBContext();
        private const string _dic = "Files";

        // GET: api/Books/GetBooks
        [HttpGet]
        public ResultData<IEnumerable<Books>> GetBooks()
        {
            var result = new ResultData<IEnumerable<Books>>().SetContent(db.BooksList);
            return result;
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        [HttpPost]
        public async Task<HttpResponseMessage> DownloadFile([FromBody]JObject data)
        {
            try
            {
                string fileName = data["bookName"].ToObject<string>();
                string path = Path.Combine(HttpContext.Current.Server.MapPath(String.Format("~/{0}", _dic)), fileName ?? "");
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                {
                    string filename = Path.GetFileName(path);
                    var stream = new FileStream(path, FileMode.Open);
                    HttpResponseMessage resp = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StreamContent(stream)
                    };
                    resp.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = HttpUtility.UrlEncode(fileName),
                    };
                    resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    resp.Content.Headers.ContentLength = stream.Length;

                    return await Task.FromResult(resp);
                }
            }
            catch (Exception ex)
            {
            }
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        [HttpGet]
        public async Task<HttpResponseMessage> DownloadFile(string data)
        {
            try
            {
                string path = Path.Combine(HttpContext.Current.Server.MapPath(String.Format("~/{0}", _dic)), data ?? "");
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                {
                    string filename = Path.GetFileName(path);
                    var stream = new FileStream(path, FileMode.Open);
                    HttpResponseMessage resp = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StreamContent(stream)
                    };
                    resp.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = HttpUtility.UrlEncode(data),
                    };
                    resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    resp.Content.Headers.ContentLength = stream.Length;

                    return await Task.FromResult(resp);
                }
            }
            catch (Exception ex)
            {
            }
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        [HttpGet]
        public IHttpActionResult GetFile(string file = null)
        {
            string path = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath($"/{_dic}"), file ?? "");

            if (File.Exists(path))
            {
                var url = String.Format($"{HttpContext.Current.Request.Url.Scheme}://{HttpContext.Current.Request.Url.Authority}/{_dic}/{file}"); 
                return this.Redirect(url);
            }

            return StatusCode(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        [HttpPost]
        public async Task<HttpResponseMessage> UploadFile()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            HttpRequestMessage request = this.Request;
            HttpResponseMessage ret = new HttpResponseMessage();

            string root = HttpContext.Current.Server.MapPath(String.Format("~/{0}", _dic));
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }
            var provider = new MultipartFormDataStreamProvider(root);

            try
            {
                // Read the form data.
                await Request.Content.ReadAsMultipartAsync(provider);

                // This illustrates how to get the file names.
                foreach (var file in provider.Contents)
                {
                    var fileName = file.Headers.ContentDisposition.FileName ?? file.Headers.ContentDisposition.Name;
                    if (String.IsNullOrWhiteSpace(fileName))
                    {
                        continue;
                    }

                    string destFile = Path.Combine(root, fileName);

                    if (File.Exists(destFile))
                    {
                        File.Delete(destFile);
                    }

                    var ms = file.ReadAsStreamAsync().Result;
                    using (var br = new BinaryReader(ms))
                    {
                        var data = br.ReadBytes((int)ms.Length);
                        File.WriteAllBytes(destFile, data);
                    }
                    //var newbook=new Books()
                    //{
                    //    Id = Guid.NewGuid(),
                    //    Name = Path.GetFileNameWithoutExtension(destFile),
                    //    Path = fileName,
                    //    type = BookTypes.Privates,
                    //    IsEnable = true,
                    //};
                    //db.BooksList.Add(newbook);
                    //db.SaveChanges();
                }
                ret.StatusCode = HttpStatusCode.OK;
                ret.Content = new StringContent("File uploaded.");
            }
            catch (Exception ex)
            {
                ret.StatusCode = HttpStatusCode.UnsupportedMediaType;
                ret.Content = new StringContent("File upload failed.");
            }

            return ret;
        }

        //GET: api/Books/5
        //[ResponseType(typeof(Books))]
        //public IHttpActionResult GetBooks(Guid id)
        //{
        //    Books books = db.BooksList.Find(id);
        //    if (books == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(books);
        //}

        // PUT: api/Books/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutBooks(Guid id, Books books)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != books.Id)
            {
                return BadRequest();
            }

            db.Entry(books).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BooksExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Books
        [ResponseType(typeof(Books))]
        public IHttpActionResult PostBooks(Books books)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.BooksList.Add(books);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (BooksExists(books.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = books.Id }, books);
        }

        // DELETE: api/Books/5
        [ResponseType(typeof(Books))]
        public IHttpActionResult DeleteBooks(Guid id)
        {
            Books books = db.BooksList.Find(id);
            if (books == null)
            {
                return NotFound();
            }

            db.BooksList.Remove(books);
            db.SaveChanges();

            return Ok(books);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool BooksExists(Guid id)
        {
            return db.BooksList.Count(e => e.Id == id) > 0;
        }
    }
}