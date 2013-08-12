using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace THOK.WCS.Bll.Models
{
    public class RestReturn
    {
        public bool IsSuccess { get; set; }
        public object Data { get; set; }
        public string Message { get; set; }
    }
}
