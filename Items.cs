using System;
using System.Collections.Generic;
using System.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using LBAMemoryModule;

namespace LBAGameChanger
{
    class Items
    {
        private Item[] Inventory;
        private Item[] InventoryUsed;
        private Item[] Twinsen;
        public Item[] Quest;
        Mem m = new Mem();

        public Items(string LBAFilesPath, ushort LBAVer)
        {
            string fullFilePath = LBAFilesPath + "lba" + LBAVer.ToString() + ".xml";

            Twinsen = loadItems(fullFilePath, "//Twinsen/item");
            Inventory = loadItems(fullFilePath, "//inventory/item");
            InventoryUsed = loadItems(fullFilePath, "//inventoryUsed/item");
            Quest = loadItems(LBAFilesPath + "Quests.xml", "//quests/item");
        }
        private string getFilesDirectroyPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory + "files\\";
        }

        //Assumes all files are in .\files\ folder
        private Item[] loadItems(string fullFilePath, string XMLQueryString)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(fullFilePath);
            XmlNodeList nodes = doc.DocumentElement.SelectNodes(XMLQueryString);
            Item[] items = new Item[nodes.Count];
            for (int i = 0; i < items.Length; i++)
                items[i] = getItem(nodes[i]);
            return items;
        }

        private Item getItem(XmlNode xn)
        {
            Item item = new Item();

            item.friendlyName = xn.SelectSingleNode("friendlyName").InnerText.Trim();
            item.internalName = xn.SelectSingleNode("internalName").InnerText.Trim();
            string s = xn.SelectSingleNode("memoryOffset").InnerText.Trim();
            item.memoryOffset = uint.Parse(s, System.Globalization.NumberStyles.HexNumber);
            string maxVal = xn.SelectSingleNode("maxVal").InnerText.Trim();
            item.maxVal = ushort.Parse(xn.SelectSingleNode("maxVal").InnerText.Trim());
            item.minVal = ushort.Parse(xn.SelectSingleNode("minVal").InnerText.Trim());
            item.size = byte.Parse(xn.SelectSingleNode("size").InnerText.Trim());
            item.type = ushort.Parse(xn.SelectSingleNode("type").InnerText.Trim());
            item.lbaVersion = byte.Parse(xn.SelectSingleNode("lbaVersion").InnerText.Trim());
            //If we adjust item to contain an arraylist of items, then for each item in item we can call getitem
            return item;
        }

        private Item getItemFromName(string name, Item[] items)
        {
            for (int i = 0; i < items.Length; i++)
                if (items[i].internalName.ToLower() == name.ToLower()) return items[i];
            return null;
        }

        public Item getTwinsenItem(string name)
        {
            return getItemFromName(name, Twinsen);
        }

        public Item getInventoryItem(string name)
        {
            return getItemFromName(name, Inventory);
        }
        public Item getInventoryUsedItem(string name)
        {
            return getItemFromName(name, InventoryUsed);
        }

        public Item getQuestItem(string name)
        {
            return getItemFromName(name, Quest);
        }
    }
}
