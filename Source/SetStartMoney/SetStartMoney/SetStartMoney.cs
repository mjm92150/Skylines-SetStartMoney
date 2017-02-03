using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using ICities;
using UnityEngine;
using ColossalFramework.UI;

namespace SetStartMoney
{
    public class SetStartMoney : IUserMod
    {
        private UserSettings us = new UserSettings();

        private UITextField Instructions;
        private UITextField StartMoney;
        private UIDropDown MyDropDown;

        private string[] vals = new string[] { "50000", "100000", "200000", "250000", "300000", "350000", "400000", "450000", "500000", "550000", "600000", "650000", "700000", "750000", "800000", "850000", "900000", "950000", "1000000", "1500000", "2000000", "2500000", "3000000", "3500000", "4000000", "4500000", "5000000", "5500000", "6000000", "6500000", "7000000", "7500000", "8000000", "8500000", "9000000", "9500000", "10000000", "10500000", "11000000", "11500000", "12000000", "12500000", "13000000", "13500000", "14000000", "14500000", "15000000", "15500000", "16000000", "16500000", "17000000", "17500000", "18000000", "18500000", "19000000", "19500000", "20000000", "20500000", "21000000", "21500000", "22000000", "22500000", "23000000", "23500000", "24000000", "24500000", "25000000", "25500000", "26000000", "26500000", "27000000", "27500000", "28000000", "28500000", "29000000", "29500000", "30000000", "30500000", "31000000", "31500000", "32000000", "32500000", "33000000", "33500000", "34000000", "34500000", "35000000", "35500000", "36000000", "36500000", "37000000", "37500000", "38000000", "38500000", "39000000", "39500000", "40000000", "40500000", "41000000", "41500000", "42000000", "42500000", "43000000", "43500000", "44000000", "44500000", "45000000", "45500000", "46000000", "46500000", "47000000", "47500000", "48000000", "48500000", "49000000", "49500000", "50000000" };

        public string Name { get { return "SetStartMoney"; } }
        public string Description { get { return "Set the amount of Start Money you want!"; } }

        private void EventCheck(bool c)
        {
            us.Enabled = c;
            MyDropDown.isVisible = c;
            StartMoney.isVisible = c;
        }

        private void EventClick()
        {
            us.Save();
        }

        private void EventSel(int sel)
        {
            int val = 0;
            int.TryParse(vals[sel], out val);
            us.StartMoney = val;
            StartMoney.text = vals[sel];
        }

        private void EventTextChanged(string c)
        {
            //Debug.Log(c);
        }

        private void EventTextSubmitted(string c)
        {
            //Debug.Log(c);
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            UIHelperBase group = helper.AddGroup("Set Start Money");
            Instructions = (UITextField)group.AddTextfield("Instructions", "Check to enable, select the value you want for start money.", EventTextChanged, EventTextSubmitted);
            Instructions.readOnly = true;
            Instructions.multiline = true;
            Instructions.width += 250;
            Instructions.height += 30;
            group.AddSpace(30);

            group.AddCheckbox("Enable Start Money", us.Enabled, EventCheck);
            StartMoney = (UITextField)group.AddTextfield("Current Start Money Value", us.StartMoney.ToString(), EventTextChanged, EventTextSubmitted); Instructions.readOnly = true;
            StartMoney.readOnly = true;
            MyDropDown = (UIDropDown) group.AddDropdown("Select an amount of start money", vals, GetSelection(), EventSel);

            group.AddSpace(250);
            group.AddButton("Save", EventClick);
        }

        private int GetSelection()
        {
            int index = Array.FindIndex(vals, row => row.Contains(us.StartMoney.ToString()));
            return index;
        }
    }

    public class LoadingExtension : LoadingExtensionBase
    {
        private UserSettings us = new UserSettings();

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);

            if (mode == LoadMode.NewGame && us.Enabled == true)
            {
                try
                {
                    var type = typeof(EconomyManager);
                    var cashAmountField = type.GetField("m_cashAmount", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                    cashAmountField.SetValue(EconomyManager.instance, us.StartMoney * 100);
                }
                catch (Exception ex)
                {
                    Debug.Log("Error setting Cash Amount./n" + ex);
                }
            }
        }
    }

    internal class UserSettings
    {
        #region Declarations

        private string[] settings = new string[] { "Enabled", "StartMoney" };

        private bool _Enabled = false;
        public bool Enabled { get { return _Enabled; } set { _Enabled = value; } }

        private int _StartMoney;
        public int StartMoney { get { return _StartMoney; } set { _StartMoney = value; } }

        private const string fileName = "SetStartMoney.xml";
        private string us = fileName;
        private XmlDocument xml = new XmlDocument();

        #endregion

        public UserSettings()
        {
            bool create = true;

            //Do we have a file?
            if (File.Exists(us))
            {
                xml.Load(us);
                //was it any good?
                create = (xml.SelectSingleNode("UserSettings") == null);
            }
            //if we need to create a new file
            if (create)
            {
                CreateSettings();
            }
            FillSettings();
        }

        public void Save()
        {
            int loc = 0;
            try
            {
                xml.SelectSingleNode("UserSettings/Enabled").InnerText = Enabled.ToString();
                xml.SelectSingleNode("UserSettings/StartMoney").InnerText = StartMoney.ToString();
            }
            catch (Exception ex)
            {
                Debug.Log("Error in UserSettings.Save loc: " + loc + "./n" + ex);
            }
            xml.Save(us);
        }

        private void FillSettings()
        {
            _Enabled = ValidateSetting("Enabled", false);
            _StartMoney = ValidateSetting("StartMoney", 50000);
        }

        private bool ValidateSetting(string node, bool setting = false)
        {
            //create a new node
            if (xml.SelectSingleNode("UserSettings/" + node) == null)
            {
                XmlNode tb = xml.SelectSingleNode("UserSettings");
                XmlNode nd = xml.CreateNode(XmlNodeType.Element, node, "");
                nd.InnerText = setting.ToString();
                //RoadUpdateTool.WriteLog("creating a node: " + node);
                tb.AppendChild(nd);
            }
            //we have a node get the value
            setting = (xml.SelectSingleNode("UserSettings/" + node).InnerText == "True");
            return setting;
        }

        private double ValidateSetting(string node, double type)
        {
            //we already validated the file exists and has our node "UserSettings"
            double setting = 0.0;
            //create a new node
            if (xml.SelectSingleNode("UserSettings/" + node) == null)
            {
                XmlNode tb = xml.SelectSingleNode("UserSettings");
                XmlNode nd = xml.CreateNode(XmlNodeType.Element, node, "");
                nd.InnerText = false.ToString();
                //RoadUpdateTool.WriteLog("Creating a node: " + node);
                tb.AppendChild(nd);
            }
            //we have a node get the value
            string temp = xml.SelectSingleNode("UserSettings/" + node).InnerText;
            if (double.TryParse(temp, out setting) == false)
                setting = 0.0;
            return setting;
        }

        private int ValidateSetting(string node, int type)
        {
            //we already validated the file exists and has our node "UserSettings"
            int setting = 1;
            //create a new node
            if (xml.SelectSingleNode("UserSettings/" + node) == null)
            {
                XmlNode tb = xml.SelectSingleNode("UserSettings");
                XmlNode nd = xml.CreateNode(XmlNodeType.Element, node, "");
                nd.InnerText = false.ToString();
                //RoadUpdateTool.WriteLog("Creating a node: " + node);
                tb.AppendChild(nd);
            }
            //we have a node get the value
            string temp = xml.SelectSingleNode("UserSettings/" + node).InnerText;
            if (int.TryParse(temp, out setting) == false)
                setting = 1;
            return setting;
        }

        public void CreateSettings()
        {
            //RoadUpdateTool.WriteLog("Entring CreateSettings");
            xml = new XmlDocument();
            XmlDeclaration xmlDeclaration = xml.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = xml.DocumentElement;
            xml.InsertBefore(xmlDeclaration, root);
            //XmlElement temp = xml.CreateElement("UserSettings", "");
            XmlNode table = xml.CreateNode(XmlNodeType.Element, "UserSettings", "");
            xml.AppendChild(table);
            foreach (string s in settings)
            {
                XmlNode node = xml.CreateNode(XmlNodeType.Element, s, "");
                node.InnerText = false.ToString();
                table.AppendChild(node);
            }
            xml.Save(us);
        }
    }
}