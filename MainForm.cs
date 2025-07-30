using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Mermaid4net
{
    public partial class MainForm : Form
    {
        internal static List<daClass> methodCorrelations;

        public MainForm()
        {
            InitializeComponent();
            this.BackColor = Color.FromArgb(26, 15, 30);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            txtFile.Text = txtFile.Text.Trim();

            if (!File.Exists(txtFile.Text))
            {
                MessageBox.Show("Could not find the input file");
                return;
            }

            var assembly = AssemblyDefinition.ReadAssembly(txtFile.Text);

            methodCorrelations = new List<daClass>();

            foreach (var type in assembly.MainModule.Types)
            {
                if (type.IsInterface)
                    continue;

                if (type.Name.Contains("`1") || type.Name.StartsWith("<PrivateImplementation") || type.Name.Contains("__AnonymousType") || type.Name.Equals("Resources") || type.Name.Equals("Settings"))
                    continue;

                daClass newType = new daClass();
                newType.className = type.Name;

                foreach (var method in type.Methods)
                {
                    if (IsSystemMethod(method) || method.FullName=="System.Void Program::<Main>(System.String[])") //.net8 console
                        continue;

                    //Console.WriteLine("*1*" + method.FullName);

                    daMethod newMethod = new daMethod();
                    newMethod.methodName = method.FullName;

                    if (method.HasBody)
                    {
                        AnalyzeMethod(method, newMethod.calls, type);
                    }

                    newType.methods.Add(newMethod);
                }

                methodCorrelations.Add(newType);
                //Console.WriteLine("------------------------" + type.Name + "------------------------");
            }

            INodeHandleGenerator NewNodeHandle;
            if (optLetter.Checked)
                NewNodeHandle = new LetterGenerator();
            else
                NewNodeHandle = new NumGenerator();


            StringBuilder sB = new StringBuilder();
            string lastMethod = string.Empty;
            foreach (daClass item in methodCorrelations)
            {
                if (item.methods.Count == 0)
                    continue;


                sB.AppendLine("<h2>" + item.className + "</h2>\r\n");
                sB.AppendLine("<pre class=\"mermaid\">\r\ngraph LR"); //TD
                foreach (daMethod m in item.methods) //.Where(z=>z.calls.Count>0))
                {

                    lastMethod = NewNodeHandle.GetNextNodeHandle() + "[\"" + ExtractMethod(m.methodName) + "\"]";
                    DigCall(m, item, lastMethod, NewNodeHandle, sB, 0, 10);

                }
                sB.AppendLine("</pre>");

            }

            assembly.Dispose();

            string output = Path.GetDirectoryName(txtFile.Text) + "\\"
                            + Path.GetFileNameWithoutExtension(txtFile.Text) 
                            + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".html";
            
            if (File.Exists(output))
            {
                MessageBox.Show("Could not save the file, same file exists.\n\n" + output);
                return;
            }

            File.WriteAllText(output, Mermaid4net.Properties.Resources.template
                                        .Replace("{placeholder}", sB.ToString())
                                        .Replace("{filename}", Path.GetFileName(txtFile.Text)));

            System.Diagnostics.Process.Start(output);

            GC.Collect();
        }

        private static void DigCall(daMethod m, daClass item, string parentMethod, INodeHandleGenerator NewLetter, StringBuilder sB, int currentDepth, int maxDepth)
        {
            if (currentDepth > maxDepth) //recursive limit - avoid input file recursive functions 
            {
                sB.AppendLine("Max recursion depth reached");
                return;
            }

            // string lastMethod =NewLetter.GetNextLetter() + "[\"" + ExtractMethod(parentMethod) + "\"]";
            string destination = string.Empty;
            foreach (var c in m.calls)
            {
                if (!c.Contains("__"))
                {
                    destination = NewLetter.GetNextNodeHandle() + "[\"" + ExtractMethod(c) + "\"]";
                    sB.AppendLine(parentMethod + " --> " + destination);
                }
                else
                    destination = parentMethod;

                //var f = item.methods.Where(hh => hh.methodName.Equals(c)).FirstOrDefault();
                var f = methodCorrelations.SelectMany(type => type.methods).Where(dd => dd.methodName.Equals(c)).FirstOrDefault();

                if (f != null)
                {
                    if (f.calls.Count > 0)
                        DigCall(f, item, destination, NewLetter, sB, currentDepth + 1, maxDepth);
                }

            }
        }

        /*
         *  with lambda & async support
        */
        private static void AnalyzeMethod(MethodDefinition method, List<string> methodCorrelations, TypeDefinition parentType)
        {
            if (method.IsAsync())
            {
                foreach (var instruction in method.Body.Instructions)
                {
                    if (instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt)
                    {

                        var calledMethod = instruction.Operand as MethodReference;
                        if (calledMethod != null)
                        {

                            if (!IsSystemMethod(calledMethod))
                            {
                                methodCorrelations.Add(calledMethod.FullName);
                                //Console.WriteLine("*2" + calledMethod.FullName);
                            }
                        }
                    }
                    else if (instruction.OpCode == OpCodes.Ldftn)
                    {
                        // handle delegate invocation (as Task.Run)
                        var delegateMethod = instruction.Operand as MethodReference;
                        if (delegateMethod != null)
                        {

                            if (!IsSystemMethod(delegateMethod))
                            {
                                methodCorrelations.Add(delegateMethod.FullName);
                                //Console.WriteLine("*3" + delegateMethod.FullName);
                            }
                        }
                    }
                }

                // find the state machine class within the parent type
                string stateMachineClassNamePrefix = "<" + method.Name + ">d__";
                TypeDefinition stateMachineClass = null;

                foreach (var type in parentType.NestedTypes)
                {
                    if (type.Name.StartsWith(stateMachineClassNamePrefix))
                    {
                        stateMachineClass = type;
                        break;
                    }
                }

                if (stateMachineClass != null)
                {
                    foreach (var stateMachineMethod in stateMachineClass.Methods)
                    {
                        if (stateMachineMethod.HasBody)
                        {
                            foreach (var instruction in stateMachineMethod.Body.Instructions)
                            {
                                if (instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt)
                                {
                                    var calledMethod = instruction.Operand as MethodReference;
                                    if (calledMethod != null)
                                    {

                                        if (!IsSystemMethod(calledMethod))
                                        {
                                            methodCorrelations.Add(calledMethod.FullName);
                                            //Console.WriteLine("*4" + calledMethod.FullName);
                                        }
                                    }
                                }
                                else if (instruction.OpCode == OpCodes.Ldftn)
                                {
                                    // handle delegate invocation in the state machine
                                    var delegateMethod = instruction.Operand as MethodReference;
                                    if (delegateMethod != null)
                                    {

                                        if (!IsSystemMethod(delegateMethod))
                                        {
                                            methodCorrelations.Add(delegateMethod.FullName);
                                            //Console.WriteLine("*5" + delegateMethod.FullName);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // regular methods
                foreach (var instruction in method.Body.Instructions)
                {
                    if (instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt)
                    {
                        var calledMethod = instruction.Operand as MethodReference;
                        if (calledMethod != null)
                        {
                            if (!IsSystemMethod(calledMethod))
                            {
                                methodCorrelations.Add(calledMethod.FullName);
                                //Console.WriteLine("*6" + calledMethod.FullName);
                            }
                        }
                    }
                }
            }
        }


        private static Regex regex = new Regex(@"([A-Za-z0-9_]+::[A-Za-z0-9_]+)", RegexOptions.Compiled);

        private static string ExtractMethod(string input)
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

        private static string ReplaceHTMLtxt(string HTMLSTR)
        {
            HTMLSTR = HTMLSTR.Replace("&", "&amp;");
            HTMLSTR = HTMLSTR.Replace("\"", "&quot;");
            HTMLSTR = HTMLSTR.Replace("<", "&lt;");
            HTMLSTR = HTMLSTR.Replace(">", "&gt;");
            HTMLSTR = HTMLSTR.Replace(" ", "&nbsp;");
            HTMLSTR = HTMLSTR.Replace("'", "&#39;");

            return HTMLSTR;
        }

        private static bool IsSystemMethod(MethodReference methodReference)
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

        #region TextBox DragDrop

        private void textBox1_DragDrop(object sender, DragEventArgs e)
        {
            string[] FileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            if (FileList[0].ToLower().EndsWith(".dll") || FileList[0].ToLower().EndsWith(".exe"))
                txtFile.Text = FileList[0];
        }

        private void textBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        #endregion
    }


    public static class MethodDefinitionExtensions
    {
        public static bool IsAsync(this MethodDefinition method)
        {
            return method.CustomAttributes.Any(attr => attr.AttributeType.FullName == "System.Runtime.CompilerServices.AsyncStateMachineAttribute");
        }
    }
}
