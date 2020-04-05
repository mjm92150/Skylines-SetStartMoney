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

        private UIButton Validate;
        private UIButton Save;

        private UITextField Instructions;
        private UITextField StartMoney;
        private UITextField NewMoney;
        private UIDropDown MyDropDown;

        private string[] vals = new string[] { "70000", "100000", "200000", "250000", "300000", "350000", "400000", "450000", "500000", "550000", "600000", "650000", "700000", "750000", "800000", "850000", "900000", "950000", "1000000", "1500000", "2000000", "2500000", "3000000", "3500000", "4000000", "4500000", "5000000", "5500000", "6000000", "6500000", "7000000", "7500000", "8000000", "8500000", "9000000", "9500000", "10000000", "10500000", "11000000", "11500000", "12000000", "12500000", "13000000", "13500000", "14000000", "14500000", "15000000", "15500000", "16000000", "16500000", "17000000", "17500000", "18000000", "18500000", "19000000", "19500000", "20000000", "20500000", "21000000" };

        public string Name { get { return "SetStartMoney"; } }
        public string Description { get { return "Définissez le montant d'argent de départ que vous souhaitez !"; } }
        
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

        private void Validate_Clicked()
        {
            int val = 0;
            int.TryParse(NewMoney.text, out val);

            bool valid = (val >= 70000 && val <= 21000000);

            if (valid)
            {
                us.StartMoney = val;
                StartMoney.text = val.ToString();
            }
            else
                NewMoney.text = us.StartMoney.ToString();
        }

        private void EventTextChanged(string c)
        {
            Validate.enabled = !(c == StartMoney.text);
        }

        private void EventTextSubmitted(string c)
        {
            //Debug.Log(c);
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            UIHelperBase group = helper.AddGroup("Définissez la valeur de départ du mod Start Money de 70 000 à 21 000 000");
            Instructions = (UITextField)group.AddTextfield("Instructions", "Cochez pour activer, sélectionnez la valeur souhaitée pour l'argent de départ.", EventTextChanged, EventTextSubmitted);
            Instructions.readOnly = true;
            Instructions.multiline = true;
            Instructions.width += 250;
            Instructions.height += 30;
            group.AddSpace(30);

            group.AddCheckbox("Activer le mod Start Money", us.Enabled, EventCheck);
            
            StartMoney = (UITextField)group.AddTextfield("Valeur actuelle :", us.StartMoney.ToString(), EventTextChanged, EventTextSubmitted);
            StartMoney.readOnly = true;

            MyDropDown = (UIDropDown)group.AddDropdown("Utilisez la liste des valeurs", vals, GetSelection(), EventSel);

            NewMoney = (UITextField)group.AddTextfield("Ou entrez une valeur. (70k - 21m)", us.StartMoney.ToString(), EventTextChanged, EventTextSubmitted);
            
            Validate = (UIButton)group.AddButton("Valider la valeur entrée manuellement", Validate_Clicked);
            Validate.enabled = false;

            MyDropDown.isVisible = us.Enabled;
            StartMoney.isVisible = us.Enabled;
            Validate.isVisible = us.Enabled;

            group.AddSpace(150);
            Save = (UIButton) group.AddButton("Sauvegarder", EventClick);
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
                    Debug.Log("Erreur lors de la définition du montant en espèces./n" + ex);
                }
            }
        }
    }

    internal class UserSettings
    {
        #region Declarations

        private string[] settings = new string[] { "Activé", "StartMoney" };

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
                Debug.Log("Erreur dans UserSettings.Save loc :" + loc + "./n" + ex);
            }
            xml.Save(us);
        }

        private void FillSettings()
        {
            _Enabled = ValidateSetting("Activé", false);
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
