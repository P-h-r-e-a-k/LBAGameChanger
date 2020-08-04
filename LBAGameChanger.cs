using LBAMemoryModule;
using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;

namespace LBAGameChanger
{
    /** 
     *  !set health 10
     *  !set health max
     *  !set health min
     *  !give gawleys_horn
     *  !remove gawleys_horn
    */
    public class LBAGameChanger
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
        Mem m = new Mem();
        Items items;
        enum LBA1Offsets : uint { FlagByteOne = 0xD54C, FlagByteTwo = 0xD54D, Armour = 0xD500 }
        public LBAGameChanger(string filesPath, ushort LBAVer)
        {
            items = new Items(filesPath, LBAVer);
        }

        private string processSkinCommand(string command)
        {
            string val;
            if ((val = replaceIfContains(command, "tunic_sword", "2")) != null) return val;
            if ((val = replaceIfContains(command, "tunic_med", "0")) != null) return val;
            if ((val = replaceIfContains(command, "tunic_horn", "5")) != null) return val;
            if ((val = replaceIfContains(command, "tunic", "1")) != null) return val;
            if ((val = replaceIfContains(command, "prison", "3")) != null) return val;
            if ((val = replaceIfContains(command, "nurse", "4")) != null) return val;
            return null;
        }
        private string processChangeWeaponCommand(string command)
        {
            string val;
            if ((val = replaceIfContains(command, "magic_ball", "0")) != null) return val;
            if ((val = replaceIfContains(command, "funfrocks_saber", "1")) != null) return val;
            return null;
        }

        private string replaceIfContains(string source, string val, string newVal)
        {
            if (source.Contains(val))
            {
                return source.Replace(val, newVal);
            }
            return null;
        }
        private void toggleBit(uint offset, byte bitNumber)
        {
            byte data = (byte)m.readVal(offset, 1);
            data ^= (byte)Math.Pow(2, bitNumber);
            m.WriteVal(offset, data, 1);
        }
        private bool isByteOneBitSet(byte bitNumber)
        {
            byte val = (byte)m.readVal((uint)LBA1Offsets.FlagByteOne, 1);

            if (0 == bitNumber) return 1 == (1 & val);
            if (1 == bitNumber) return 2 == (2 & val);
            if (2 == bitNumber) return 4 == (4 & val);
            if (3 == bitNumber) return 8 == (8 & val);
            if (4 == bitNumber) return 16 == (16 & val);
            if (5 == bitNumber) return 32 == (32 & val);
            if (6 == bitNumber) return !(64 == (64 & val));
            return false;
        }
        private bool isByteTwoBitSet(byte bitNumber)
        {
            byte val = (byte)m.readVal((uint)LBA1Offsets.FlagByteTwo, 1);
            if (1 == bitNumber) return (2 == (2 & val));
            if (2 == bitNumber) return 4 == (4 & val);
            if (3 == bitNumber) return 8 == (8 & val);
            if (4 == bitNumber) return !(16 == (16 & val));
            if (5 == bitNumber) return 32 == (32 & val);
            if (6 == bitNumber) return 64 == (64 & val);
            if (7 == bitNumber) return 128 == (128 & val);
            return false;
        }

        private string processFlagByteOne(string command)
        {
            byte bitNum = 0;
            command = removeCommandPortion(command, "set");

            //Object Collision
            if (command.Contains("obj_col"))
            {
                command = removeCommandPortion(command, "obj_col");
                bitNum = 0;
            }
            //Brick Collision
            if (command.Contains("brick_col"))
            {
                command = removeCommandPortion(command, "brick_col");
                bitNum = 1;
            }
            //!set walk_on_water on/off
            if (command.Contains("walk_on_water"))
            {
                command = removeCommandPortion(command, "walk_on_water");
                bitNum = 6;
            }

            if (command.Contains("on"))
            {
                if (isByteOneBitSet(bitNum)) return null;
                toggleBit((uint)LBA1Offsets.FlagByteOne, bitNum);
            }
            if (command.Contains("off"))
            {
                if (!isByteOneBitSet(bitNum)) return null;
                toggleBit((uint)LBA1Offsets.FlagByteOne, bitNum);
            }
            //!set visible on/off
            return null;
        }
        private string processFlagByteTwo(string command)
        {
            byte bitNum = 0;
            command = removeCommandPortion(command, "set");

            //Object Collision
            if (command.Contains("invisible"))
            {
                command = removeCommandPortion(command, "invisible");
                bitNum = 1;
            }
            //Brick Collision
            if (command.Contains("gravity"))
            {
                command = removeCommandPortion(command, "gravity");
                bitNum = 3;
            }
            //!set walk_on_water on/off
            if (command.Contains("shadow"))
            {
                command = removeCommandPortion(command, "shadow");
                bitNum = 4;
            }

            if (command.Contains("on"))
            {
                if (isByteTwoBitSet(bitNum)) return null;
                toggleBit((uint)LBA1Offsets.FlagByteTwo, bitNum);
            }
            if (command.Contains("off"))
            {
                if (!isByteTwoBitSet(bitNum)) return null;
                toggleBit((uint)LBA1Offsets.FlagByteTwo, bitNum);
            }
            //!set visible on/off
            return null;
        }
        private byte getChapter()
        {
            const uint LBA1_CHAPTER = 0xE28;
            return (byte) m.readVal(LBA1_CHAPTER, 1);
        }

        private bool isLBA1FocusWindow()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();
            uint processID;
            GetWindowThreadProcessId(handle, out processID);
            if (Process.GetProcessById((int)processID).MainWindowTitle.Contains("RELENT"))
                return true;
            return false;
        }

        private void Zoom()
        {
            if (isLBA1FocusWindow())
                SendKeys.SendWait("{F5}");
            SendKeys.Flush();
        }
        private string preprocessCommand(string command)
        {
            string normalisedCommand = command.ToLower();
            if (command.ToLower().Contains("zoom")) Zoom();
            if ("give tunic" == command) if (2 > getChapter()) return null;
            if (normalisedCommand.Contains("skin"))
                return processSkinCommand(normalisedCommand);
            if (normalisedCommand.Contains("selected_weapon"))
                return processChangeWeaponCommand(normalisedCommand);
            if (normalisedCommand.Contains("obj_col"))
                return processFlagByteOne(normalisedCommand);
            if (normalisedCommand.Contains("brick_col"))
                return processFlagByteOne(normalisedCommand);
            if (normalisedCommand.Contains("walk_on_water"))
                return processFlagByteOne(normalisedCommand);
            if (normalisedCommand.Contains("invisible"))
                return processFlagByteTwo(normalisedCommand);
            if (normalisedCommand.Contains("gravity"))
                return processFlagByteTwo(normalisedCommand);
            if (normalisedCommand.Contains("shadow"))
                return processFlagByteTwo(normalisedCommand);
            return command;
        }
        public void ParseCommand(string command)
        {
            command = preprocessCommand(command);
            if (null == command) return;
            InternalNameValue intNamVal = new InternalNameValue();
            Item x;
            x = Get_Item(getInternalNameGiveRemoveCommand(command));
            if (null == x) x = new Item();
            if (command.StartsWith("give"))
            {
                command = removeCommandPortion(command, "give");
                command = "set " + command + " " + x.maxVal.ToString();
            }
            if (command.StartsWith("remove"))
            {
                command = removeCommandPortion(command, "remove");
                command = "set " + command + " " + x.minVal.ToString();
            }
            if (command.StartsWith("set"))
            {
                intNamVal = processSetCommand(command);
                if (null == intNamVal) return;
                Set_Item(intNamVal.internalName, intNamVal.value);
                return;
            }
        }

        private string getInternalNameGiveRemoveCommand(string command)
        {
            if (command.Contains("give") || command.Contains("remove"))
                return command.Substring(command.IndexOf(' ')).Trim();
            return command;
        }
        //Maybe have armour etc. as a set command and special case here somehow
        private InternalNameValue processSetCommand(string command)
        {
            InternalNameValue intNamVal = new InternalNameValue();
            Item x = new Item();

            command = removeCommandPortion(command, "set");
            if (!command.Contains(" ")) return null;
            intNamVal.internalName = command.Substring(0, command.IndexOf(' '));
            x = Get_Item(intNamVal.internalName);
            if (null == x) return null;
            command = removeCommandPortion(command, intNamVal.internalName);
            if ("max" == command)
            {
                intNamVal.value = x.maxVal;
                return intNamVal;
            }

            if ("min" == command)
            {
                intNamVal.value = x.minVal;
                return intNamVal;
            }

            int val = getInt(command);
            if (-1 == val) return null;
            intNamVal.value = (ushort)val;
            return intNamVal;
        }
        private int getInt(string value)
        {
            int val;
            if (!int.TryParse(value, out val)) return -1;
            return val;
        }
        private string removeCommandPortion(string commandString, string portionToRemove)
        {
            return commandString.Remove(0, portionToRemove.Length + 1).Trim();
        }
        private bool setInventoryUsedFlag(string internalName)
        {
            if ("ferry_ticket" == internalName) return true;
            if ("book_of_bu" == internalName) return true;
            return false;
        }
        public void Set_Item(string internalName, ushort val)
        {
            Item item = items.getTwinsenItem(internalName);
            if (null != item)
            {
                setItem(item, val);
                return;
            }

            if (null == item)
            {
                item = items.getInventoryItem(internalName);
                if (null != item)
                {
                    setItem(item, val);
                    if(setInventoryUsedFlag(internalName))
                        item = items.getInventoryUsedItem(internalName + "_u");
                    if (null != item)
                        setItem(item, val);
                    return;
                }
            }
        }

        public Item Get_Item(string internalName)
        {
            Item x = items.getTwinsenItem(internalName);
            if (null != x) return x;
            return items.getInventoryItem(internalName);
        }

        private void setItem(Item x, ushort val)
        {
            if (null == x) return;
            if (val < x.minVal) val = x.minVal;
            if (val > x.maxVal) val = x.maxVal;
            m = new Mem();
            m.WriteVal(x.memoryOffset, val, x.size);
        }

    }

    public class InternalNameValue
    {
        public string internalName;
        public ushort value;
    }
}
