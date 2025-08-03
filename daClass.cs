using Mono.Cecil.Cil;
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
        public IEnumerable<Instruction> conditionalBranches { get; set; }
        public IEnumerable<Instruction> elseBR { get; set; }
        public string methodName {get;set;}
        public List<daCall> calls { get; set; }

        public daMethod()
        {
            calls = new List<daCall>();
        }
    }

    public class daCall
    {
        public string callName { get; set; }
        public int callOffset { get; set; }
    }
}
