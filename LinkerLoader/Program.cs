using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace LinkerLoader
{
    partial class Program
    {
        static List<char> memoryTable = new List<char>();
        static int loadAdress = 0x02170;
        static int Main(string[] args)
        {
            List<string> fileNames = new List<string>();
            string outputFileName = "";
            foreach(string arg in args)
            {
                if (outputFileName == "")
                    outputFileName = arg;
                else
                    fileNames.Add(arg);
            }

            int rtnVal = LoadAndLink(fileNames, outputFileName);
            dumpMemory();
            return rtnVal;
        }

        static void dumpMemory()
        {
            int curLineAddr = loadAdress;

            Chronicler.WriteLine("ADDRES  0  1  2  3  4  5  6  7  8  9  A  B  C  D  E  F");

            for(int i = 0; i<memoryTable.Count;i++)
            {
                if (i % 32 == 0)
                    Chronicler.Write(curLineAddr.ToString("X6"));
                if (i % 2 == 0)
                {
                    curLineAddr++;
                    Chronicler.Write(" " + memoryTable[i]);
                }
                else
                    Chronicler.Write(memoryTable[i].ToString());
                if (i % 32 == 31)
                    Chronicler.Write("\n");
            }
        }

        static int LoadAndLink(List<string> files, string executableName)
        {
            if (files.Count < 1 || executableName=="")
            {
                Chronicler.WriteLine("Error, user must pass an output file name and at least one file to load!", Chronicler.OutputOptions.ERR);
                return 1;
            }

            //remove quotes from fileNames
            for(int i = 0; i<files.Count; i++)
            {
                files[i] = files[i].Trim().Trim('"').Trim();
            }

            foreach(string fileName in files)
            {
                string path = "../../../"+fileName;
                string text = File.ReadAllText(path);

                PassOne(text);
            }

            PassTwo();

            return 0;
        }
    }
}
