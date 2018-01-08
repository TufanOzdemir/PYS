using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBS.Data.Result
{
    public class Result<T>
    {
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
        public T Data { get; set; }

        public Result(bool IsSuccess, T Data, string Message)
        {
            this.Message = Message;
            this.Data = Data;
            this.IsSuccess = IsSuccess;
        }

        public Result(T Data)
        {
            this.Data = Data;
            IsSuccess = true;
            Message = "Başarılı";
        }

        public Result(string Message)
        {
            this.Message = Message;
            IsSuccess = false;
            Data = default(T);
        }
    }
}
