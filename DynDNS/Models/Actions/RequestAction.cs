using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynDNS.Models.Actions
{
    public class RequestAction<T>
    {
        public string action { get; set; }
        public T param { get; set; }
    }
}
