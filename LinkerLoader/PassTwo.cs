using System;
using System.Collections.Generic;
using System.Text;

namespace LinkerLoader
{
    partial class Program
    {
        static bool enableDebugging = false;
        static void PassTwo()
        {
            string ops = "";
            foreach(MREC mREC in mRecords)
            {
                //calculate bounds
                int offsetAdress = mREC.address - loadAdress;
                int indexedAddress = mREC.nibbleCount%2==0 ? (offsetAdress*2): (offsetAdress * 2)+1;

                //read substring
                string buffer = "";
                for (int i = indexedAddress; i < indexedAddress+mREC.nibbleCount; i++)
                {
                    buffer += memoryTable[i];
                }

                //create cached value
                int value = int.Parse(buffer, System.Globalization.NumberStyles.HexNumber);
                ops += "Starting("+ offsetAdress.ToString("X6")+" : "+value.ToString($"X{mREC.nibbleCount}") +")\t";
                int mRecVal;

                //perform lookup
                if (externalSymbolTable.TryGetValue(mREC.symbolName, out mRecVal) == false)
                    Chronicler.WriteLine("Error: "+ mREC.symbolName+" is not in the external symbol table!\nPlease check the input data and files being passed!\n", Chronicler.OutputOptions.ERR);


                ops += "Found(" + mREC.symbolName + " : "+ mRecVal.ToString($"X{mREC.nibbleCount}") + ")\t";
                //apply modification
                mRecVal *= mREC.arithmaticSign;
                value += mRecVal;

                ops += "Result(" + value.ToString($"X{mREC.nibbleCount}") + ")\t";
                //format string for writeback
                string replacementStr = value.ToString($"X{mREC.nibbleCount}");

                ops += "Replacement(" + replacementStr + ")\n";
                //perform write back
                for (int i = indexedAddress; i < indexedAddress + mREC.nibbleCount; i++)
                {
                    memoryTable[i] = replacementStr[i-indexedAddress];
                }
            }
            Chronicler.WriteLine(ops, Chronicler.OutputOptions.DETAIL);
        }
    }
}
