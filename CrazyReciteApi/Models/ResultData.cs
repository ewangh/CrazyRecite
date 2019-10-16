using CrazyReciteApi.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Policy;
using System.Web;

namespace CrazyReciteApi.Models
{
    //[Serializable]
    public class ResultData<T> : ISerializable
    {
        public Guid ResultId { get; set; }
        public ResultTypes MsgCode { get; set; }
        public string Message { get; set; }
        public DateTime CurrenTime { get; set; }

        public T Content { get; set; }

        public ResultData()
        {
            this.ResultId = Guid.NewGuid();
            this.CurrenTime=DateTime.Now;
        }

        public ResultData<T> SetContent(T content)
        {
            try
            {
                return SuccessResult(content);
            }
            catch (Exception e)
            {
                return ErrorResult(e);
            }
        }

        public ResultData<T> SetMessage(ResultTypes code)
        {
            this.MsgCode = code;
            this.Message = MessageEmpty.GetResultMsg(code);
            return this;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // 运用info对象来添加你所需要序列化的项
            info.AddValue("resultId", ResultId);
            info.AddValue("msgCode", MsgCode);
            info.AddValue("currenTime", CurrenTime);
            info.AddValue("message", Message);

            if (Content != null)
            {
                info.AddValue("content", Convert.ChangeType(Content, Content.GetType()));
            }
            else
            {
                info.AddValue("content", null);
            }
        }

        protected ResultData<T> SuccessResult(T content)
        {
            this.Content = content;
            this.MsgCode = ResultTypes.Success;
            return this;
        }

        protected ResultData<T> ErrorResult(Exception e)
        {
            this.MsgCode = ResultTypes.Error;
            this.Message = e.Message;
            return this;
        }
    }
}