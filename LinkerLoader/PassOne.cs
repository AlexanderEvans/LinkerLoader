using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace LinkerLoader
{
    partial class Program
    {
        static int PassOne(string input)
        {
            string[] inputLines = input.Split('\n', '\r', StringSplitOptions.RemoveEmptyEntries);
            int errorLevel = 0;
            foreach(string line in inputLines)
            {
                if(line!=null && line!="")
                {
                    switch(line[0])
                    {
                        case 'H':
                            parseHeader(line.Remove(0, 1));
                            break;
                        case 'T':
                            parseTextR(line.Remove(0, 1));
                            break;
                        case 'M':
                            parseModR(line.Remove(0, 1));
                            break;
                        case 'R':
                            parseRefR(line.Remove(0, 1));
                            break;
                        case 'D':
                            parseDefR(line.Remove(0, 1));
                            break;
                        case 'E':
                            parseEndR(line.Remove(0, 1));
                            break;
                        default:
                            Chronicler.WriteLine("Error, did not understand '" + line[0] + "' record!", Chronicler.OutputOptions.ERR);
                            return 1;
                    }
                }
            }
            return errorLevel;
        }
        static int FileStartAdress = 0x2170;
        static int CurrentFileLength = 0;
        private static void parseHeader(string v)
        {
            string progSym = v.Substring(0, 6).Trim();
            string progLoc = v.Remove(0, 6).Substring(0, 6);
            string progLength = v.Remove(0, 12);
            externalSymbolTable.Add(progSym, FileStartAdress + CurrentFileLength);
            CurrentFileLength = int.Parse(progLength, System.Globalization.NumberStyles.HexNumber);
            while (memoryTable.Count < (CurrentFileLength + (FileStartAdress-loadAdress)) * 2)
            {
                memoryTable.Add('U');
            }
        }
        static int firstExecutableInstruction = -1;
        private static void parseEndR(string v)
        {
            string firstInstruction = v;
            if (firstExecutableInstruction == -1)
                firstExecutableInstruction = int.Parse(firstInstruction, System.Globalization.NumberStyles.HexNumber) + FileStartAdress;
            FileStartAdress = FileStartAdress + CurrentFileLength;
            symRefs.Clear();
            symDefs.Clear();
        }
        private static void parseTextR(string v)
        {
            string progLoc = v.Substring(0, 6);
            string progLength = v.Remove(0, 6).Substring(0, 2);
            string objCode = v.Remove(0, 8);
            int instLoc = int.Parse(progLoc, System.Globalization.NumberStyles.HexNumber) + FileStartAdress;
            int instLength = int.Parse(progLength, System.Globalization.NumberStyles.HexNumber);

            int locInArr = instLoc - loadAdress;
            while(memoryTable.Count<(locInArr+instLength)*2)
            {
                memoryTable.Add('U');
            }
            for(int i = 0; i<(instLength*2); i++)
            {
                memoryTable[i + locInArr] = objCode[i];
            }
        }
        static List<string> symDefs = new List<string>();
        private static void parseDefR(string v)
        {
            string buffer = v;
            while (buffer != "")
            {
                string symbolName = buffer.Substring(0, 6).Trim();
                buffer = buffer.Remove(0, 6).Trim();
                int symbolLoc = int.Parse(buffer.Substring(0, 6).Trim(), System.Globalization.NumberStyles.HexNumber) + (FileStartAdress-loadAdress);
                Chronicler.WriteLine(symbolLoc.ToString("X6"), Chronicler.OutputOptions.DETAIL);
                buffer = buffer.Remove(0, 6).Trim();
                externalSymbolTable.Add(symbolName, symbolLoc);
                symDefs.Add(symbolName);
            }
        }
        static List<string> symRefs = new List<string>();
        private static void parseRefR(string v)
        {
            string buffer = v;
            while (buffer != "")
            {
                string symbolName = buffer.Length <= 6 ? buffer.Trim() : buffer.Substring(0, 6).Trim();
                buffer = buffer.Length >= 6 ? buffer.Remove(0, 6).Trim() : "";
                symRefs.Add(symbolName);
            }
        }

        struct MREC
        {
            public int address;
            public int nibbleCount;
            public int arithmaticSign;
            public string symbolName;
        }

        static List<MREC> mRecords = new List<MREC>();
        private static void parseModR(string v)
        {
            string instLoc = v.Substring(0, 6);
            string instLength = v.Remove(0, 6).Substring(0, 2);
            char sign = v[8];
            string symbol = v.Remove(0, 9).Trim();
            if(symRefs.Contains(symbol) || symDefs.Contains(symbol))
            {
                int writeAddress = int.Parse(instLoc, System.Globalization.NumberStyles.HexNumber);
                int mFactor = sign == '+' ? 1 : -1;
                if (enableDebugging)
                    Chronicler.WriteLine("(" + sign + " : " + mFactor + ")", Chronicler.OutputOptions.DETAIL);
                int length = int.Parse(instLength, System.Globalization.NumberStyles.HexNumber);
                MREC tmp;
                tmp.address = writeAddress + FileStartAdress;
                tmp.nibbleCount = length;
                tmp.arithmaticSign = mFactor;
                tmp.symbolName = symbol;
                mRecords.Add(tmp);
            }
            else
            {
                Chronicler.WriteLine("Error: MRecord not defined or exposed by reference!", Chronicler.OutputOptions.ERR);
            }
        }
        static Dictionary<string, int> externalSymbolTable = new Dictionary<string, int>();
    }
}
