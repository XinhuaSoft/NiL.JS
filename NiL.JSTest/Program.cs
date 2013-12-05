﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NiL.JS;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;
using System.IO;

namespace NiL.JSTest
{
    class Program
    {
        private static void benchmark()
        {
            const int iterations = 100000000;
            Console.WriteLine("iterations count: " + iterations);

            long init = DateTime.Now.Ticks;
            Script s = new Script(@"
var a = 1; for(var i = 0; i < " + iterations + @";i++){ a = a * 3 * i; }
");
            init = DateTime.Now.Ticks - init;
            long start = DateTime.Now.Ticks;
            s.Invoke();
            long l = (DateTime.Now.Ticks - start);
            Console.WriteLine("script: " + (l / 10000).ToString());
            Console.WriteLine("initialization: " + (init / 10000).ToString());
            var a = 1;
            long nativeStart = DateTime.Now.Ticks;
            for (var i = 0; i < iterations; i++)
                a = a * 3 * i;
            long nativeL = (DateTime.Now.Ticks - nativeStart);
            Console.WriteLine(a);
            Console.WriteLine("native: " + (nativeL / 10000).ToString());
            Console.WriteLine("rate: " + ((double)l / (double)nativeL).ToString());
        }

        private static void featureSupportTest()
        {
            Script s = new Script(@"

");
            s.Invoke(); 
        }

        private static void runFile(string filename)
        {
            Console.WriteLine("Processing file: " + filename);
            Console.WriteLine("-------------------------------------");
            var f = new FileStream(filename, FileMode.Open, FileAccess.Read);
            var sr = new StreamReader(f);
            var s = new Script(sr.ReadToEnd());
            s.Context.GetField("$ERROR").Assign(new CallableField((t, x) =>
            {
                Console.WriteLine("ERROR: " + x[0].Invoke());
                return null;
            }));
            s.Invoke();
            sr.Dispose();
            f.Dispose();
            Console.WriteLine("-------------------------------------");
            Console.WriteLine("Complite.");
        }

        private static void sputnicTests(string folderPath = "tests\\")
        {
            Action<string> _ = Console.WriteLine;

            try
            {
                _("Sputnik testing begin...");
                _("Directory: \"" + Directory.GetParent(folderPath) + "\"");

                _("Scaning directory...");
                var fls = Directory.EnumerateFiles(folderPath, "*.js", SearchOption.AllDirectories).ToArray();
                _("Founded " + fls.Length + " js-files");

                int passed = 0;
                int failed = 0;

                for (int i = 0; i < fls.Length; i++)
                {
                    bool pass = true;
                    Console.Write("Processing file \"" + fls[i] + "\" ");
                    var f = new FileStream(fls[i], FileMode.Open, FileAccess.Read);
                    var sr = new StreamReader(f);
                    string code = sr.ReadToEnd();
                    if (code.IndexOf('"') == -1)
                        code = "eval(\"" + code + "\");";
                    else if (code.IndexOf('\'') == -1)
                        code = "eval('" + code.Replace("\\", "\\\\") + "');";
                    var s = new Script(code);
                    s.Context.GetField("$ERROR").Assign(new CallableField((t, x) =>
                    {
                        Console.WriteLine("ERROR: " + x[0].Invoke());
                        pass = false;
                        return null;
                    }));
                    s.Invoke();
                    sr.Dispose();
                    f.Dispose();
                    if (pass)
                    {
                        _("Passed");
                        passed++;
                    }
                    else
                    {
                        _("Failed");
                        failed++;
                    }
                }

                _("Sputnik testing complite");
            }
            catch (Exception e)
            {
                _("Sputnik testing fail (" + e.Message + ", " + e.StackTrace + ")");
            }
        }

        static void Main(string[] args)
        {
            NiL.JS.Core.Context.GlobalContext.GetField("platform").Assign("NiL.JS");
            //runFile(@"tests.js");
            runFile(@"ftest.js");
            //runFile(@"tests\ch07\7.4\S7.4_A6.js");
            //benchmark();
            //featureSupportTest();
            //sputnicTests();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            Console.WriteLine("GC.CollectionCount: " + GC.CollectionCount(0));
            Console.WriteLine("GC.CollectionCount: " + GC.CollectionCount(1));
            Console.WriteLine("GC.CollectionCount: " + GC.CollectionCount(2));
            Console.WriteLine("GC.MaxGeneration: " + GC.MaxGeneration);
            Console.WriteLine("GC.GetTotalMemory: " + GC.GetTotalMemory(false));
        }
    }
}
