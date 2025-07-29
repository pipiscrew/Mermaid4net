using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mermaid4net
{
    public class daClass
    {
        public string className { get; set; }
        public List<daMethod> methods { get; set; }

        public daClass()
        {
            methods = new List<daMethod>();
        }
    }

    public class daMethod
    {
        public string methodName {get;set;}
        public List<string> calls { get; set; }

        public daMethod()
        {
            calls = new List<string>();
        }
    }
}
