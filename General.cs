using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Mermaid4net
{
    internal static class General
    {
        internal static List<daClass> methodCorrelations;
        internal static StringBuilder sB;
        internal static bool showDecision;
        internal static bool showDecisionTask;
        internal static bool showImplementation;
        internal static AssemblyDefinition assembly;

        internal static AnalyzedMethod AnalyzeMethodCall(Instruction instruction)
        {
            AnalyzedMethod methodCall = new AnalyzedMethod();

            var calledMethod = instruction.Operand as MethodReference;
            if (calledMethod != null)
            {
                if (!IsSystemMethod(calledMethod))
                {
                    MethodDefinition methodDef = null;
                    try
                    //if (CanResolveMethod(calledMethod)) - here throws error because needs the 3rd party dlls
                    {
                        methodDef = calledMethod.Resolve();
                    }
                    catch { }

                    if (showImplementation && methodDef != null)
                    {
                        var declaringType = methodDef.DeclaringType;
                        var ff = FindImplementingClasses(declaringType.Name, calledMethod.Name);
                        if (ff != null)
                            methodDef = ff;
                    }

                    if (methodDef != null && methodDef.HasBody && methodDef.Body.Instructions != null)
                    {
                        methodCall.methodCall = new daCall() { callName = methodDef.FullName, callOffset = instruction.Offset };

                        if (showDecisionTask)
                        {
                            methodCall.conditionalBranches = new Collection<Instruction>(methodDef.Body.Instructions.Where(y => conditionalBranches.Contains(y.OpCode)).ToList());
                            methodCall.elseBR = new Collection<Instruction>(methodDef.Body.Instructions.Where(y => y.OpCode == OpCodes.Br || y.OpCode == OpCodes.Br_S).ToList());
                        }
                    }
                }
            }

            return methodCall;

        }

        internal static bool IsSystemMethod(MethodReference methodReference)
        {
            if (methodReference.DeclaringType.Namespace.StartsWith("System"))
            {
                return true;
            }

            string m = methodReference.FullName;
            if (m.Contains("CompilerServices") || m.Contains("AsyncVoidMethodBuilder") || m.Contains("AsyncTaskMethodBuilder")
                || m == "System.Void System.Object::.ctor()" || m.Contains("get_") || m.Contains("set_") || m.Contains("Dispose()")
                || m.Contains("System.Collections.Generic.List") || m.Contains("EqualityComparer")
                || m.Contains("op_Inequality") || m.Contains("op_Equality") || m.Contains("op_Implicit")
                || m.Contains("Equals") || m.Contains("::ToString")
                || m.Contains("StringBuilder::") || m.Contains("String::")
                || m.Contains("Array::") || m.Contains("Enumerable::")
                || m.Contains("Task::") || m.Contains("Array::")
                || m.Contains("Convert::") || m.Contains("TimeSpan::")
                || m.Contains("Regex::") || m.Contains("LoggerExtensions::")
                || m.Contains("Stopwatch::") || m.Contains("ILogger::")
                || m.Contains("ConfigurationManager::") || m.Contains("::op_LessThan") || m.Contains("::op_GreaterThan") || m.Contains("::op_Division") || m.Contains("::op_Multiply") || m.Contains("::op_Subtraction") || m.Contains("::GetString") || m.Contains("Control::") || m.Contains("MessageBox::") || m.Contains("Encoding::") || m.Contains("IEnumerator::") || m.Contains("ControlCollection::") || m.Contains("Console::") || m.Contains("Delegate::") || m.Contains("ContainerControl::")
                || m.Contains("EventHandler::") || m.Contains("GetQueryableEntity") || m.Contains("CSharpArgumentInfo") || m.Contains("::GetKeyProperty") || m.Contains("MVCxGridViewColumnCollection")
                || m.Contains("::And(") || m.Contains("::Or("))
                return true;
            else
                return false;
        }

        private static MethodDefinition FindImplementingClasses(string interfaceName, string methodName)
        {
            var interfaceType = assembly.MainModule.Types.FirstOrDefault(t => t.Name == interfaceName && t.IsInterface);

            if (interfaceType == null)
                return null;

            foreach (var type in assembly.MainModule.Types)
            {
                if (type.IsClass)
                {
                    if (type.Interfaces.Any(i => i.InterfaceType.FullName == interfaceType.FullName))
                    {
                        return type.Methods.Where(o => o.Name == methodName).FirstOrDefault();
                    }
                }
            }

            return null;
        }


        //not used - throws error because needs the 3rd party dlls
        private static bool CanResolveMethod(MethodReference methodReference)
        {
            if (methodReference == null)
            {
                return false;
            }

            if (methodReference.DeclaringType == null || methodReference.DeclaringType.Resolve() == null)
            {
                return false;
            }

            if (methodReference.ReturnType == null || methodReference.ReturnType.Resolve() == null)
            {
                return false;
            }

            foreach (var parameter in methodReference.Parameters)
            {
                if (parameter.ParameterType == null || parameter.ParameterType.Resolve() == null)
                {
                    return false;
                }
            }

            return true;
        }

        internal static readonly HashSet<OpCode> conditionalBranches = new HashSet<OpCode>
        {
            OpCodes.Beq,
            OpCodes.Beq_S,
            OpCodes.Bge,
            OpCodes.Bge_S,
            OpCodes.Bge_Un,
            OpCodes.Bge_Un_S,
            OpCodes.Bgt,
            OpCodes.Bgt_S,
            OpCodes.Bgt_Un,
            OpCodes.Bgt_Un_S,
            OpCodes.Ble,
            OpCodes.Ble_S,
            OpCodes.Ble_Un,
            OpCodes.Ble_Un_S,
            OpCodes.Blt,
            OpCodes.Blt_S,
            OpCodes.Blt_Un,
            OpCodes.Blt_Un_S,
            OpCodes.Bne_Un,
            OpCodes.Bne_Un_S,
            OpCodes.Brfalse,
            OpCodes.Brfalse_S,
            OpCodes.Brtrue,
            OpCodes.Brtrue_S
        };

        internal static string WriteMermaidNodeFixedSource(string src, string dst, string dstH)
        {
            string destination = dstH + "[\"" + ExtractMethod(dst) + "\"]";
            sB.AppendLine(src + " --> " + destination);
            return destination;
        }

        internal static string GetNewMermaidNode(string src, string srcH)
        {
            return srcH + "[\"" + ExtractMethod(src) + "\"]";
        }

        private static Regex regex = new Regex(@"([A-Za-z0-9_]+::[A-Za-z0-9_]+)", RegexOptions.Compiled);

        internal static string ExtractMethod(string input)
        {
            MatchCollection matches = regex.Matches(input);

            if (matches.Count > 0)
            {
                return ReplaceHTMLtxt(matches[0].Value);
            }
            else
            {
                return ReplaceHTMLtxt(input);
            }
        }

        internal static string ReplaceHTMLtxt(string HTMLSTR)
        {
            HTMLSTR = HTMLSTR.Replace("&", "&amp;");
            HTMLSTR = HTMLSTR.Replace("\"", "&quot;");
            HTMLSTR = HTMLSTR.Replace("<", "&lt;");
            HTMLSTR = HTMLSTR.Replace(">", "&gt;");
            HTMLSTR = HTMLSTR.Replace(" ", "&nbsp;");
            HTMLSTR = HTMLSTR.Replace("'", "&#39;");

            return HTMLSTR;
        }
    }


    internal static class MethodDefinitionExtensions
    {
        public static bool IsAsync(this MethodDefinition method)
        {
            return method.CustomAttributes.Any(attr => attr.AttributeType.FullName == "System.Runtime.CompilerServices.AsyncStateMachineAttribute");
        }
    }
}
