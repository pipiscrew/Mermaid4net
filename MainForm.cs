using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Mermaid4net
{
    public partial class MainForm : Form
    {
        private static INodeHandleGenerator NewNodeHandle = null;

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

            General.assembly = AssemblyDefinition.ReadAssembly(txtFile.Text);

            General.methodCorrelations = new List<daClass>();
            General.showDecision = chkDecision.Checked; //make it accessible by static methods
            General.showDecisionTask = chkDecisionTask.Checked;
            General.showImplementation = chkInterfaceImplementation.Checked;

            foreach (var type in General.assembly.MainModule.Types)
            {
                if (type.IsInterface)
                    continue;

                if (type.Name.Contains("`1") || type.Name.StartsWith("<PrivateImplementation") || type.Name.Contains("__AnonymousType") || type.Name.Equals("Resources") || type.Name.Equals("Settings"))
                    continue;

                daClass newType = new daClass();
                newType.className = type.Name;

                foreach (var method in type.Methods)
                {
                    if (General.IsSystemMethod(method) || method.FullName == "System.Void Program::<Main>(System.String[])") //.net8 console async main
                        continue;

                    daMethod newMethod = new daMethod();
                    newMethod.methodName = method.FullName;

                    if (method.HasBody)
                    {
                        AnalyzeMethod(method, newMethod, type);
                    }

                    newType.methods.Add(newMethod);
                }

                General.methodCorrelations.Add(newType);
                //Console.WriteLine("------------------------" + type.Name + "------------------------");
            }

            
            if (optLetter.Checked)
                NewNodeHandle = new LetterGenerator();
            else
                NewNodeHandle = new NumGenerator();


            General.sB = new StringBuilder();
            string lastMethod = string.Empty;

            ///////////////// EXPORT [start]
            if (exportAllinOne.Checked)
            {
                if (chkExportAllinOneWithHeaders.Checked)
                    ExportAllinOneWithHeaders();
                else
                    ExportAllinOne();
            }
            else if (exportPer.Checked)
            {
                ExportPerEntity(Path.GetDirectoryName(txtFile.Text) + "\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + "\\");
            }

            ///////////////// EXPORT [end]



            //General.sB.AppendLine("<pre class=\"mermaid\">\r\ngraph LR"); //TD

            //foreach (daClass item in General.methodCorrelations)
            //{
            //    if (item.methods.Count == 0)
            //        continue;

            //    //General.sB.AppendLine("<h2>" + item.className + "</h2>\r\n");
            //    //General.sB.AppendLine("<pre class=\"mermaid\">\r\ngraph LR"); //TD
            //    foreach (daMethod m in item.methods) //.Where(z=>z.calls.Count>0))
            //    {
            //        lastMethod = NewNodeHandle.GetNextNodeHandle() + "[\"" + General.ExtractMethod(m.methodName) + "\"]";
            //        DigCall(m, item, lastMethod, NewNodeHandle, General.sB, 0, 10);
            //    }

            //    //General.sB.AppendLine("</pre>");
            //}

            //General.sB.AppendLine("</pre>");

            General.assembly.Dispose();

            ////////////////////////////////// only when all-in-one file
            if (exportAllinOne.Checked)
            {
                string output = Path.GetDirectoryName(txtFile.Text) + "\\"
                                + Path.GetFileNameWithoutExtension(txtFile.Text)
                                + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".html";

                if (File.Exists(output))
                {
                    MessageBox.Show("Could not save the file, same file exists.\n\n" + output);
                    return;
                }

                string template = Mermaid4net.Properties.Resources.templateAllInOneZoom;
                
                if (chkExportAllinOneWithHeaders.Checked)
                    template =  Mermaid4net.Properties.Resources.template;
                
                    File.WriteAllText(output, template
                                                .Replace("{placeholder}", General.sB.ToString())
                                                .Replace("{filename}", Path.GetFileName(txtFile.Text)));
 

                System.Diagnostics.Process.Start(output);
            }

            GC.Collect();
        }

        static string elseNodeMethod = string.Empty;
        static string elseNodeLetter = string.Empty;
        private static void DigCall(daMethod m, daClass item, string parentMethod, INodeHandleGenerator NewLetter, StringBuilder sB, int currentDepth, int maxDepth)
        {
            if (currentDepth > maxDepth) //recursive limit - avoid input file recursive functions 
            {
                sB.AppendLine("Max recursion depth reached");
                return;
            }

            string destination = string.Empty;

            foreach (var c in m.calls)
            {
                if (General.showDecision)
                {
                    var occuringIfCondiiton = m.conditionalBranches.Where(i => i.Offset <= c.callOffset && (i.Operand as Instruction).Offset >= c.callOffset).ToList();
                    var occuringElseCondition = m.elseBR.Where(i => i.Offset <= c.callOffset && (i.Operand as Instruction).Offset >= c.callOffset).ToList();

                    if (occuringIfCondiiton.Count > 0)
                    {
                        string lastLetter = NewLetter.GetNextNodeHandle();
                        sB.AppendLine(parentMethod + " --> " + lastLetter + "{Decision?}");
                        destination = General.WriteMermaidNodeFixedSource(lastLetter, General.ExtractMethod(c.callName), NewLetter.GetNextNodeHandle());

                        elseNodeLetter = lastLetter;
                        elseNodeMethod = m.methodName;
                    }
                    else if (occuringElseCondition.Count > 0)
                    {
                        if (elseNodeMethod == m.methodName)
                        {
                            destination = General.WriteMermaidNodeFixedSource(elseNodeLetter, General.ExtractMethod(c.callName), NewLetter.GetNextNodeHandle());
                        }
                        elseNodeLetter = string.Empty;
                    }
                }

                if (!c.callName.Contains("__"))
                {
                    destination = NewLetter.GetNextNodeHandle() + "[\"" + General.ExtractMethod(c.callName) + "\"]";
                    sB.AppendLine(parentMethod + " --> " + destination);
                }
                else
                    destination = parentMethod;

                var f = General.methodCorrelations.SelectMany(type => type.methods).Where(dd => dd.methodName.Equals(c.callName)).FirstOrDefault();

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
        private static void AnalyzeMethod(MethodDefinition method, daMethod methodCorrelations, TypeDefinition parentType)
        {
            methodCorrelations.conditionalBranches = method.Body.Instructions.Where(y => General.conditionalBranches.Contains(y.OpCode));
            methodCorrelations.elseBR = method.Body.Instructions.Where(y => y.OpCode == OpCodes.Br || y.OpCode == OpCodes.Br_S);

            if (method.IsAsync())
            {
                foreach (var instruction in method.Body.Instructions)
                {
                    if (instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt)
                    {
                        AnalyzedMethod x = General.AnalyzeMethodCall(instruction);

                        if (x.methodCall != null)
                        {
                            methodCorrelations.calls.Add(x.methodCall);

                            if (General.showDecisionTask)
                            {
                                methodCorrelations.conditionalBranches = x.conditionalBranches;
                                methodCorrelations.elseBR = x.elseBR;
                            }
                        }
                    }
                    else if (instruction.OpCode == OpCodes.Ldftn)
                    {
                        // handle delegate invocation (as Task.Run)
                        AnalyzedMethod x = General.AnalyzeMethodCall(instruction);

                        if (x.methodCall != null)
                        {
                            methodCorrelations.calls.Add(x.methodCall);

                            if (General.showDecisionTask)
                            {
                                methodCorrelations.conditionalBranches = x.conditionalBranches;
                                methodCorrelations.elseBR = x.elseBR;
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
                                    AnalyzedMethod x = General.AnalyzeMethodCall(instruction);

                                    if (x.methodCall != null)
                                    {
                                        methodCorrelations.calls.Add(x.methodCall);

                                        if (General.showDecisionTask)
                                        {
                                            methodCorrelations.conditionalBranches = x.conditionalBranches;
                                            methodCorrelations.elseBR = x.elseBR;
                                        }
                                    }
                                }
                                else if (instruction.OpCode == OpCodes.Ldftn)
                                {
                                    // handle delegate invocation in the state machine
                                    AnalyzedMethod x = General.AnalyzeMethodCall(instruction);

                                    if (x.methodCall != null)
                                    {
                                        methodCorrelations.calls.Add(x.methodCall);

                                        if (General.showDecisionTask)
                                        {
                                            methodCorrelations.conditionalBranches = x.conditionalBranches;
                                            methodCorrelations.elseBR = x.elseBR;
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
                        AnalyzedMethod x = General.AnalyzeMethodCall(instruction);

                        if (x.methodCall != null)
                        {
                            methodCorrelations.calls.Add(x.methodCall);
                        }
                    }
                }
            }
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

        private void chkDecision_CheckStateChanged(object sender, EventArgs e)
        {
            chkDecisionTask.Checked = chkDecisionTask.Visible = chkDecision.Checked;
        }

        //private void exportPer_CheckedChanged(object sender, EventArgs e)
        //{
        //    chkExportPerZoom.Visible = exportPer.Checked;
        //}


        //private void exportAllinOne_CheckedChanged(object sender, EventArgs e)
        //{
        //    chkExportAllinOneWithHeaders.Visible = exportAllinOne.Checked;
        //}

        private static void ExportAllinOneWithHeaders()
        {
            string lastMethod = string.Empty;

            foreach (daClass item in General.methodCorrelations)
            {
                if (item.methods.Count == 0)
                    continue;

                General.sB.AppendLine("<h2>" + item.className + "</h2>\r\n");
                General.sB.AppendLine("<pre class=\"mermaid\">\r\ngraph LR"); //TD

                foreach (daMethod m in item.methods)
                {
                    lastMethod = NewNodeHandle.GetNextNodeHandle() + "[\"" + General.ExtractMethod(m.methodName) + "\"]";
                    DigCall(m, item, lastMethod, NewNodeHandle, General.sB, 0, 10);
                }

                General.sB.AppendLine("</pre>");
            }
        }

        private static void ExportAllinOne()
        {
            string lastMethod = string.Empty;
            General.sB.AppendLine("<pre class=\"mermaid\">\r\ngraph LR"); //TD

            foreach (daClass item in General.methodCorrelations)
            {
                if (item.methods.Count == 0)
                    continue;

                foreach (daMethod m in item.methods)
                {
                    lastMethod = NewNodeHandle.GetNextNodeHandle() + "[\"" + General.ExtractMethod(m.methodName) + "\"]";
                    DigCall(m, item, lastMethod, NewNodeHandle, General.sB, 0, 10);
                }
            }

            General.sB.AppendLine("</pre>");
        }

        private static void ExportPerEntity(string folder)
        {
            string lastClass = string.Empty;
            string lastMethod = string.Empty;
            string entity = string.Empty;

            Directory.CreateDirectory(folder);

            foreach (daClass item in General.methodCorrelations)
            {
                if (item.methods.Count == 0)
                    continue;

                //export
                if (lastClass != item.className)
                {
                    if (lastClass != string.Empty)
                    {
                        File.WriteAllText(folder + lastClass + ".html", Mermaid4net.Properties.Resources.templateAllInOneZoom
                                          .Replace("{placeholder}", General.sB.ToString())
                                          .Replace("{filename}", lastClass));
                    }
                    General.sB = new StringBuilder();
                }

                //start new entity
                General.sB.AppendLine("<pre class=\"mermaid\">\r\ngraph LR"); //TD

                foreach (daMethod m in item.methods)
                {
                    lastMethod = NewNodeHandle.GetNextNodeHandle() + "[\"" + General.ExtractMethod(m.methodName) + "\"]";
                    DigCall(m, item, lastMethod, NewNodeHandle, General.sB, 0, 10);
                }

                General.sB.AppendLine("</pre>");

                lastClass = item.className;
            }

            //the last one
            if (lastClass != string.Empty)
            {
                File.WriteAllText(folder + lastClass + ".html", Mermaid4net.Properties.Resources.templateAllInOneZoom
                                      .Replace("{placeholder}", General.sB.ToString())
                                      .Replace("{filename}", lastClass));
            }

            Process.Start(folder);
        }
        


        private void exportAllinOne_CheckedChanged(object sender, EventArgs e)
        {
            chkExportAllinOneWithHeaders.Visible = exportAllinOne.Checked;
        }
    }
}
