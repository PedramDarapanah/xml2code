//The MIT License (MIT)

//Copyright (c) 2015 Pedram Darapanah

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;
using System.Xml;

namespace xml2code
{
    class Program
    {
        struct String3
        {
            public string ClassName, ID, Name;
        }

        //xml nodes names,id,classes
        static List<String3> items = new List<String3>();

        static private void TraverseNodes(XmlNode node)
        {
            string name = node.LocalName;
            XmlAttribute atribID = node.Attributes["android:id"];
            XmlAttribute atribName = node.Attributes["android:name"];

            if (atribID != null)
            {
                String3 item = new String3();
                item.ClassName = name;
                item.ID = atribID.Value.Replace("@+id/", "");
                if (atribName != null)
                {
                    item.Name = atribName.Value;
                }
                items.Add(item);
            }

            if (node.HasChildNodes)
            {
                foreach (XmlNode n in node.ChildNodes)
                {
                    TraverseNodes(n);
                }
            }
        }

        [STAThread]
        static void Main(string[] args)
        {
            //message
            Console.WriteLine("Xml2Code Developed By Pedram Darapanah");
            Console.WriteLine("\nEmail: Pedram.Darapanah@gmail.com");

            //get dropped file
            string droppedFile = (args.Length == 0) ? "" : args[0];

            //open registery
            RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32);
            RegistryKey regKey = baseKey.OpenSubKey(@"SOFTWARE\Pedram.Darapanah", RegistryKeyPermissionCheck.ReadSubTree);

            //choose openning method
            if (droppedFile.Length == 0 && regKey == null)
            {
                MessageBox.Show("Please drag a layout on application icon", "Xabt", MessageBoxButtons.OK);
                Environment.Exit(0);
            }
            else
            {
                //check if it's file drop
                if (droppedFile.Length == 0)
                {
                    droppedFile = (string)regKey.GetValue("android-layout-helper");

                    //check if saved file path is okay
                    if (!File.Exists(droppedFile))
                        Environment.Exit(0);
                }
                else
                {
                    //check file type
                    if (Path.GetExtension(droppedFile) != ".xml")
                    {
                        MessageBox.Show("Please drag a layout file. you dropped " + Path.GetFileName(droppedFile), "Xabt", MessageBoxButtons.OK);
                        Environment.Exit(0);
                    }

                    //save new layout path in reg
                    regKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Pedram.Darapanah");
                    regKey.SetValue("android-layout-helper", droppedFile);
                    regKey.Close();
                }
            }

            //open file
            StringBuilder sb = new StringBuilder();
            FileStream fs = new FileStream(droppedFile, FileMode.Open);
            StreamReader sReader = new StreamReader(fs);
            sb.Append(sReader.ReadLine());
            sReader.Close();
            fs.Close();

            string layoutContent = sb.ToString();

            //load xml
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(droppedFile);

            foreach (XmlNode node in xmlDoc.ChildNodes)
                TraverseNodes(node);

            //generate code
            StringBuilder sbCode = new StringBuilder();
            sbCode.AppendLine("\t//Get Views ---------------------------------------------------");
            foreach (String3 item in items)
            {
                if (item.Name == null)
                {
                    sbCode.Append("").Append(item.ClassName).Append(" ").Append(item.ID).Append(" = ");
                    sbCode.Append("(").Append(item.ClassName).Append(")findViewById(R.id.").Append(item.ID).AppendLine(");");
                }
                else
                {
                    sbCode.Append(item.Name).Append(" ").Append(item.ID).Append(" = ");
                    sbCode.Append("(").Append(item.Name).Append(")findViewById(R.id.").Append(item.ID).AppendLine(");");
                }
            }

            //copy to clipboard
            Clipboard.Clear();
            Clipboard.SetText(sbCode.ToString(), TextDataFormat.Text);

            //message
            MessageBox.Show("Copied to clipboard");

            Environment.Exit(0);
        }
    }
}
