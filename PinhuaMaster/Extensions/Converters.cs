using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PinhuaMaster.Extensions.Converters
{
    public class MyDateTimeConverter : Newtonsoft.Json.Converters.IsoDateTimeConverter
    {
        public MyDateTimeConverter() : base()
        {
            base.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        }
    }

    public class MyDateConverter : Newtonsoft.Json.Converters.IsoDateTimeConverter
    {
        public MyDateConverter() : base()
        {
            base.DateTimeFormat = "yyyy-MM-dd";
        }
    }
}
