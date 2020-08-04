using System;
using System.Collections.Generic;
using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LBAMemoryModule;

namespace LBAGameChanger
{
    public class Item
    {
        public const ushort TYPE_BITFLAG = 0;
        public const ushort TYPE_VALUE = 1;
        public string friendlyName;
        public string internalName;
        public uint memoryOffset;
        public ushort maxVal;
        public ushort minVal;
        public byte size; //Number of bytes needed to store value
        public ushort type;
        public ushort lbaVersion; //1 for LBA1, or 2 for LBA2
        public override string ToString()
        {
            return internalName;
        }
    }
}
