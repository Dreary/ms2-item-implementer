using System;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace item_implementer
{
    public partial class formWeapon : Form
    {
        public formWeapon()
        {
            InitializeComponent();
            PopulateCbStaticDefault();
            PopulateJobComboBox();
            PopulateTransferComboBox();
            PopulateRarityComboBox();
            PopulateCbRandom();

            // Subscribe to the SelectedIndexChanged event for cbClass
            cbClass.SelectedIndexChanged += cbClass_SelectedIndexChanged;

            // Subscribe to the TextChanged event for txtItemId
            txtItemId.TextChanged += txtItemId_TextChanged;
        }

        private Image loadedIcon;
        private Image weaponImage = null;
        private bool isXmlLoaded = false;  // Add a flag to track if an XML is loaded

        private void exitBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void minimiseBtn_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        Point mousePoint;
        private void formWeapon_MouseDown(object sender, MouseEventArgs e)
        {
            mousePoint = new Point(e.X, e.Y);
        }

        private void formWeapon_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) != MouseButtons.Left) return;

            this.Location = new Point(this.Left - (mousePoint.X - e.X),
                this.Top - (mousePoint.Y - e.Y));
        }

        private void btnItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string xmlFilePath = openFileDialog.FileName;

                // Load the XML file
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFilePath);

                // Now, mark that an XML is loaded (as a template, but allow modifications)
                isXmlLoaded = true;

                // Populate other fields from the XML...
                LoadModelPath(xmlDoc);
                LoadEffectOptions(xmlDoc);
                LoadAdditionalEffectPath(xmlDoc);
                LoadGearScore(xmlDoc);
                LoadLevelLimit(xmlDoc);
                LoadJobClass(xmlDoc);
                LoadSlotIcon(xmlDoc);
                LoadOptionID(xmlDoc);
                LoadTradeCount(xmlDoc);
                LoadItemPreset(xmlDoc);

                // Extract the item ID from the filename (e.g., 15060293 from 15060293.xml)
                string itemId = Path.GetFileNameWithoutExtension(xmlFilePath);
                LoadNameAndDescription(itemId);
                LoadTransferType(xmlDoc);
                LoadItemRarity(itemId);
                LoadAndSetRarityImage();
            }
        }

        private void LoadModelPath(XmlDocument xmlDoc)
        {
            XmlNode modelNode = xmlDoc.SelectSingleNode("//asset[@name]");
            if (modelNode != null && !string.IsNullOrEmpty(modelNode.Attributes["name"]?.Value))
            {
                txtModel.Text = modelNode.Attributes["name"].Value;
            }
            else
            {
                txtModel.Clear();
            }
        }

        private void LoadGearScore(XmlDocument xmlDoc)
        {
            XmlNode gsNode = xmlDoc.SelectSingleNode("//property[@gearScore]");
            if (gsNode != null && !string.IsNullOrEmpty(gsNode.Attributes["gearScore"]?.Value))
            {
                txtScore.Text = gsNode.Attributes["gearScore"].Value;
            }
            else
            {
                txtScore.Clear();
            }
        }

        private void LoadAdditionalEffectPath(XmlDocument xmlDoc)
        {
            // Find the AdditionalEffect node
            XmlNodeList effectNodes = xmlDoc.SelectNodes("//AdditionalEffect[@id]");

            if (effectNodes != null && effectNodes.Count > 0)
            {
                // Collect all "id" values into a comma-separated string
                List<string> effectIds = new List<string>();
                foreach (XmlNode effectNode in effectNodes)
                {
                    string idValue = effectNode.Attributes["id"]?.Value;
                    if (!string.IsNullOrEmpty(idValue))
                    {
                        effectIds.Add(idValue);
                    }
                }

                // If we found any ids, display them in txtEffect, otherwise clear the field
                if (effectIds.Count > 0)
                {
                    txtEffect.Text = string.Join(", ", effectIds);
                }
                else
                {
                    txtEffect.Clear();
                }
            }
            else
            {
                txtEffect.Clear();
            }
        }

        private void LoadEffectOptions(XmlDocument xmlDoc)
        {
            XmlNode optionNode = xmlDoc.SelectSingleNode("//option[@title]");
            if (optionNode != null)
            {
                // Assign individual attributes to respective text boxes
                txtTitle.Text = optionNode.Attributes["title"]?.Value ?? string.Empty;
                txtStatic.Text = optionNode.Attributes["static"]?.Value ?? string.Empty;
                txtRandom.Text = optionNode.Attributes["random"]?.Value ?? string.Empty;
                txtConstant.Text = optionNode.Attributes["constant"]?.Value ?? string.Empty;
            }
            else
            {
                // Clear text boxes if no option node is found
                txtTitle.Clear();
                txtStatic.Clear();
                txtRandom.Clear();
                txtConstant.Clear();
            }
        }

        private void LoadLevelLimit(XmlDocument xmlDoc)
        {
            XmlNode levelNode = xmlDoc.SelectSingleNode("//limit[@levelLimit]");
            if (levelNode != null && !string.IsNullOrEmpty(levelNode.Attributes["levelLimit"]?.Value))
            {
                txtLevel.Text = levelNode.Attributes["levelLimit"].Value;
            }
            else
            {
                txtLevel.Clear();
            }
        }

        private void LoadItemPreset(XmlDocument xmlDoc)
        {
            // Assuming the XML node for itemPreset is in the tool node
            XmlNode loadItemNode = xmlDoc.SelectSingleNode("//tool[@itemPreset]");
            if (loadItemNode != null && !string.IsNullOrEmpty(loadItemNode.Attributes["itemPreset"]?.Value))
            {
                txtPreset.Text = loadItemNode.Attributes["itemPreset"].Value;
            }
            else
            {
                txtPreset.Clear();
            }
        }

        private void LoadTradeCount(XmlDocument xmlDoc)
        {
            XmlNode loadTradeNode = xmlDoc.SelectSingleNode("//property[@tradableCount]");
            if (loadTradeNode != null && !string.IsNullOrEmpty(loadTradeNode.Attributes["tradableCount"]?.Value))
            {
                txtTrade.Text = loadTradeNode.Attributes["tradableCount"].Value;
            }
            else
            {
                txtTrade.Clear();
            }
        }

        private void LoadJobClass(XmlDocument xmlDoc)
        {
            XmlNode limitNode = xmlDoc.SelectSingleNode("//limit[@recommendJobs]");
            if (limitNode != null && !string.IsNullOrEmpty(limitNode.Attributes["recommendJobs"]?.Value))
            {
                int jobCode = int.Parse(limitNode.Attributes["recommendJobs"].Value);
                SetComboBoxClassByJobCode(jobCode);
            }
            else
            {
                if (cbClass.Items.Count > 0) cbClass.SelectedIndex = -1; // Deselect if no job class found
            }
        }

        private void LoadSlotIcon(XmlDocument xmlDoc)
        {
            XmlNode propertyNode = xmlDoc.SelectSingleNode("//property[@slotIcon]");
            if (propertyNode != null && !string.IsNullOrEmpty(propertyNode.Attributes["slotIcon"]?.Value))
            {
                string slotIconPath = propertyNode.Attributes["slotIcon"].Value;

                // Remove the "./Data/Resource/Image/" part of the path to make it relative to the Image folder
                string relativeIconPath = slotIconPath.Replace("./Data/Resource/Image/", string.Empty);

                // Set the base directory for the images, assuming you have an "Image" folder
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string imageDirectory = Path.Combine(baseDirectory, "!Extracted", "Image");

                // Combine the image folder path with the relative icon path (make sure it only appends the relative part after "Image/")
                string fullIconPath = Path.Combine(imageDirectory, relativeIconPath);

                // Check if the image file exists
                if (File.Exists(fullIconPath))
                {
                    pictureBox1.Image = Image.FromFile(fullIconPath);
                    pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                }
                else
                {
                    pictureBox1.Image = null;
                }
            }
            else
            {
                pictureBox1.Image = null;
            }
        }

        private void LoadOptionID(XmlDocument xmlDoc)
        {
            XmlNode optionNode = xmlDoc.SelectSingleNode("//option[@optionID]");
            if (optionNode != null && !string.IsNullOrEmpty(optionNode.Attributes["optionID"]?.Value))
            {
                string optionID = optionNode.Attributes["optionID"].Value;

                bool isValidOption = PopulateCbStaticByOptionID(optionID);

                // If the optionID is not valid (not found in either physical or magic weapon list)
                if (!isValidOption)
                {
                    cbStatic.Items.Clear();
                }
            }
            else
            {
                cbStatic.Items.Clear();  // Clear comboBox if no optionID found
            }
        }

        private bool PopulateCbStaticByOptionID(string optionID)
        {
            // Clear the existing items in the combobox at the start to avoid duplication
            cbStatic.Items.Clear();

            // Physical weapon IDs and their corresponding options
            Dictionary<string, int> physicalWeaponIDs = new Dictionary<string, int>
            {
                { "134010101", 0 }, { "134010201", 1 }, { "150010101", 0 }, { "150010201", 1 },
                { "151010101", 0 }, { "151010201", 1 }, { "153010101", 0 }, { "153010201", 1 },
                { "155010101", 0 }, { "155010201", 1 }
            };

            // Magic weapon IDs and their corresponding options
            Dictionary<string, int> magicWeaponIDs = new Dictionary<string, int>
            {
                { "133010101", 0 }, { "133010201", 1 }, { "152010101", 0 }, { "152010201", 1 },
                { "154010101", 0 }, { "154010201", 1 }, { "156010101", 0 }, { "156010201", 1 }
            };

            // If no XML has been loaded, display all four options
            if (!isXmlLoaded)
            {
                // Add both physical and magic options to the combobox
                cbStatic.Items.Add("None");
                cbStatic.Items.Add("maxatk, crate, physatk");
                cbStatic.Items.Add("maxatk, health, physatk");
                cbStatic.Items.Add("maxatk, crate, magatk");
                cbStatic.Items.Add("maxatk, health, magatk");

                cbStatic.SelectedIndex = 0;  // Set a default selected index
                return true;  // Return true since options are successfully added
            }
            else
            {
                // Check if the optionID belongs to a physical weapon
                if (physicalWeaponIDs.ContainsKey(optionID))
                {
                    // Show only physical options
                    cbStatic.Items.Add("maxatk, crate, physatk");
                    cbStatic.Items.Add("maxatk, health, physatk");
                    cbStatic.Items.Add("None");
                    cbStatic.SelectedIndex = physicalWeaponIDs[optionID]; // Select appropriate option
                    return true;  // Valid option found for physical weapon
                }
                // Check if the optionID belongs to a magic weapon
                else if (magicWeaponIDs.ContainsKey(optionID))
                {
                    // Show only magic options
                    cbStatic.Items.Add("maxatk, crate, magatk");
                    cbStatic.Items.Add("maxatk, health, magatk");
                    cbStatic.Items.Add("None");
                    cbStatic.SelectedIndex = magicWeaponIDs[optionID]; // Select appropriate option
                    return true;  // Valid option found for magic weapon
                }
                else
                {
                    // If no valid optionID is found, set cbStatic to "None"
                    cbStatic.Items.Add("None");
                    cbStatic.SelectedIndex = 0;
                    return false;  // No valid option found, return false
                }
            }
        }

        private void PopulateJobComboBox()
        {
            cbClass.Items.Clear();
            cbClass.Items.Add("Global");
            cbClass.Items.Add("Knight");
            cbClass.Items.Add("Berserker");
            cbClass.Items.Add("Wizard");
            cbClass.Items.Add("Priest");
            cbClass.Items.Add("Archer");
            cbClass.Items.Add("Heavy Gunner");
            cbClass.Items.Add("Thief");
            cbClass.Items.Add("Assassin");
            cbClass.Items.Add("Runeblade");
            cbClass.Items.Add("Striker");
            cbClass.Items.Add("Soulbinder");

            // Optionally, set the default selected index (e.g., first option selected by default)
            cbClass.SelectedIndex = 0;
        }

        private void SetComboBoxClassByJobCode(int jobCode)
        {
            string jobName;

            switch (jobCode)
            {
                case 0:
                    jobName = "Global";
                    break;
                case 10:
                    jobName = "Knight";
                    break;
                case 20:
                    jobName = "Berserker";
                    break;
                case 30:
                    jobName = "Wizard";
                    break;
                case 40:
                    jobName = "Priest";
                    break;
                case 50:
                    jobName = "Archer";
                    break;
                case 60:
                    jobName = "Heavy Gunner";
                    break;
                case 70:
                    jobName = "Thief";
                    break;
                case 80:
                    jobName = "Assassin";
                    break;
                case 90:
                    jobName = "Runeblade";
                    break;
                case 100:
                    jobName = "Striker";
                    break;
                case 110:
                    jobName = "Soulbinder";
                    break;
                default:
                    jobName = "Global";  // Default to Global if the job code is not found
                    break;
            }

            cbClass.SelectedItem = jobName;
        }

        private void PopulateCbRandom()
        {
            // Clear any existing items in cbRandom to avoid duplicates
            cbRandom.Items.Clear();

            // Add the options for cbRandom (e.g., One-handed and Two-handed values)
            cbRandom.Items.Add("None");
            cbRandom.Items.Add("One-handed values");
            cbRandom.Items.Add("Two-handed values");

            // Optionally, set the default selected index (e.g., first option selected by default)
            cbRandom.SelectedIndex = 0;
        }

        private void LoadNameAndDescription(string itemId)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string xmlDirectory = Path.Combine(baseDirectory, "!Extracted", "Xml", "string", "en");

            string nameFilePath = Path.Combine(xmlDirectory, "itemname.xml");
            string descriptionFilePath = Path.Combine(xmlDirectory, "koritemdescription.xml");

            // Load the item name and the extracted itemId (it will update the itemId if present)
            (string itemName, string extractedItemId) = LoadItemName(nameFilePath, itemId);

            // Load the tooltip and guide descriptions
            (string tooltipDescription, string guideDescription) = LoadItemDescriptions(descriptionFilePath, itemId);

            // Set the name in the textbox
            if (!string.IsNullOrEmpty(itemName))
            {
                txtName.Text = itemName;
            }
            else
            {
                txtName.Clear();
            }

            // Set the tooltip description
            if (!string.IsNullOrEmpty(tooltipDescription))
            {
                txtTipDescription.Text = tooltipDescription;
            }
            else
            {
                txtTipDescription.Clear();
            }

            // Set the guide description
            if (!string.IsNullOrEmpty(guideDescription))
            {
                txtGuideDescription.Text = guideDescription;
            }
            else
            {
                txtGuideDescription.Clear();
            }

            // Set the extracted item ID in the txtItemId textbox
            if (!string.IsNullOrEmpty(extractedItemId))
            {
                txtItemId.Text = extractedItemId;
            }
            else
            {
                txtItemId.Clear();
            }
        }

        private (string itemName, string extractedItemId) LoadItemName(string nameFilePath, string itemId)
        {
            string extractedItemId = string.Empty;

            if (File.Exists(nameFilePath))
            {
                XmlDocument nameDoc = new XmlDocument();
                nameDoc.Load(nameFilePath);

                // Select the node with the item ID
                XmlNode keyNode = nameDoc.SelectSingleNode($"//key[@id='{itemId}']");

                if (keyNode != null)
                {
                    // Extract the name attribute
                    string itemName = keyNode.Attributes["name"]?.Value ?? string.Empty;

                    // Extract the id attribute
                    extractedItemId = keyNode.Attributes["id"]?.Value ?? string.Empty;

                    return (itemName, extractedItemId);
                }
            }
            return (string.Empty, extractedItemId);
        }

        private (string tooltipDescription, string guideDescription) LoadItemDescriptions(string descriptionFilePath, string itemId)
        {
            if (File.Exists(descriptionFilePath))
            {
                XmlDocument descriptionDoc = new XmlDocument();
                descriptionDoc.Load(descriptionFilePath);

                XmlNode keyNode = descriptionDoc.SelectSingleNode($"//key[@id='{itemId}']");
                if (keyNode != null)
                {
                    string tooltip = keyNode.Attributes["tooltipDescription"]?.Value ?? string.Empty;
                    string guide = keyNode.Attributes["guideDescription"]?.Value ?? string.Empty;

                    return (tooltip, guide);
                }
            }
            return (string.Empty, string.Empty);
        }

        private void btnExpand1_Click_1(object sender, EventArgs e)
        {
            using (formExpand expandForm = new formExpand(txtTipDescription.Text))
            {
                if (expandForm.ShowDialog() == DialogResult.OK)
                {
                    txtTipDescription.Text = expandForm.DescriptionText;
                }
            }
        }

        private void btnExpand2_Click_1(object sender, EventArgs e)
        {
            using (formExpand expandForm = new formExpand(txtGuideDescription.Text))
            {
                if (expandForm.ShowDialog() == DialogResult.OK)
                {
                    txtGuideDescription.Text = expandForm.DescriptionText;
                }
            }
        }

        private void LoadTransferType(XmlDocument xmlDoc)
        {
            XmlNode limitNode = xmlDoc.SelectSingleNode("//limit");
            if (limitNode != null)
            {
                string transferTypeValue = limitNode.Attributes["transferType"]?.Value;
                if (!string.IsNullOrEmpty(transferTypeValue))
                {
                    int transferTypeCode;
                    if (int.TryParse(transferTypeValue, out transferTypeCode))
                    {
                        SetComboBoxTransferByType(transferTypeCode);
                    }
                }
                else
                {
                    if (cbTransfer.Items.Count > 0) cbTransfer.SelectedIndex = -1;
                }
            }
            else
            {
                if (cbTransfer.Items.Count > 0) cbTransfer.SelectedIndex = -1;
            }
        }

        private void SetComboBoxTransferByType(int transferTypeCode)
        {
            switch (transferTypeCode)
            {
                case 0:
                    cbTransfer.SelectedIndex = 0;  // "Tradeable"
                    break;
                case 1:
                    cbTransfer.SelectedIndex = 1;  // "Untradeable"
                    break;
                case 2:
                    cbTransfer.SelectedIndex = 2;  // "BindOnLoot"
                    break;
                case 3:
                    cbTransfer.SelectedIndex = 3;  // "BindOnEquip"
                    break;
                case 4:
                    cbTransfer.SelectedIndex = 4;  // "BindOnUse"
                    break;
                case 5:
                    cbTransfer.SelectedIndex = 5;  // "BindOnTrade"
                    break;
                case 6:
                    cbTransfer.SelectedIndex = 6;  // "BlackMarketOnly"
                    break;
                case 7:
                    cbTransfer.SelectedIndex = 7;  // "BindPet"
                    break;
                default:
                    cbTransfer.SelectedIndex = -1;  // Clear selection if invalid
                    MessageBox.Show("Invalid transferType code.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }

        private int GetTransferTypeCodeFromString(string transferType)
        {
            switch (transferType)
            {
                case "Tradeable":
                    return 0;
                case "Untradeable":
                    return 1;
                case "BindOnLoot":
                    return 2;
                case "BindOnEquip":
                    return 3;
                case "BindOnUse":
                    return 4;
                case "BindOnTrade":
                    return 5;
                case "BlackMarketOnly":
                    return 6;
                case "BindPet":
                    return 7;
                default:
                    return -1;  // Return -1 if the transferType is invalid
            }
        }

        private void PopulateTransferComboBox()
        {
            cbTransfer.Items.Clear();
            cbTransfer.Items.Add("Tradeable");
            cbTransfer.Items.Add("Untradeable");
            cbTransfer.Items.Add("BindOnLoot");
            cbTransfer.Items.Add("BindOnEquip");
            cbTransfer.Items.Add("BindOnUse");
            cbTransfer.Items.Add("BindOnTrade");
            cbTransfer.Items.Add("BlackMarketOnly");
            cbTransfer.Items.Add("BindPet");

            cbTransfer.SelectedIndex = 0;
        }

        private void PopulateRarityComboBox()
        {
            if (cbRarity.Items.Count == 0)  // Only populate if the items are not already populated
            {
                cbRarity.Items.Clear();
                cbRarity.Items.Add("Normal");
                cbRarity.Items.Add("Rare");
                cbRarity.Items.Add("Exceptional");
                cbRarity.Items.Add("Epic");
                cbRarity.Items.Add("Legendary");
                cbRarity.Items.Add("Ascendant");

                cbRarity.SelectedIndex = 0;
            }
        }

        private void LoadItemRarity(string itemId)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string itemWebFinderPath = Path.Combine(baseDirectory, "!Extracted", "Xml", "table", "na", "itemwebfinder.xml");

            if (File.Exists(itemWebFinderPath))
            {
                XmlDocument itemWebFinderDoc = new XmlDocument();
                itemWebFinderDoc.Load(itemWebFinderPath);

                XmlNode keyNode = itemWebFinderDoc.SelectSingleNode($"//key[@id='{itemId}']");
                if (keyNode != null && !string.IsNullOrEmpty(keyNode.Attributes["grade"]?.Value))
                {
                    int grade;
                    if (int.TryParse(keyNode.Attributes["grade"].Value, out grade))
                    {
                        SetComboBoxRarityByGrade(grade);
                    }
                }
                else
                {
                    if (cbRarity.Items.Count > 0) cbRarity.SelectedIndex = -1;
                }
            }
            else
            {
                if (cbRarity.Items.Count > 0) cbRarity.SelectedIndex = -1;
            }
        }

        private void SetComboBoxRarityByGrade(int grade)
        {
            PopulateRarityComboBox();

            switch (grade)
            {
                case 1:
                    cbRarity.SelectedIndex = 0;  // "Normal"
                    break;
                case 2:
                    cbRarity.SelectedIndex = 1;  // "Rare"
                    break;
                case 3:
                    cbRarity.SelectedIndex = 2;  // "Exceptional"
                    break;
                case 4:
                    cbRarity.SelectedIndex = 3;  // "Epic"
                    break;
                case 5:
                    cbRarity.SelectedIndex = 4;  // "Legendary"
                    break;
                case 6:
                    cbRarity.SelectedIndex = 5;  // "Ascendant"
                    break;
                default:
                    cbRarity.SelectedIndex = -1;  // Clear selection if invalid
                    MessageBox.Show("Invalid grade value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }

        private void SetRarityAndWeaponImage(string rarity, Image weaponImg)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string resourcesPath = Path.Combine(baseDirectory, "Resources");

            // Check if rarity is null or empty, and treat it as "normal" if that's the case
            if (string.IsNullOrEmpty(rarity))
            {
                rarity = "normal";
            }

            string rarityImagePath = null;
            switch (rarity.ToLower())
            {
                case "normal":
                    pictureBox1.BackgroundImage = null;
                    break;
                case "rare":
                    rarityImagePath = Path.Combine(resourcesPath, "rare.png");
                    break;
                case "exceptional":
                    rarityImagePath = Path.Combine(resourcesPath, "exceptional.png");
                    break;
                case "epic":
                    rarityImagePath = Path.Combine(resourcesPath, "epic.png");
                    break;
                case "legendary":
                    rarityImagePath = Path.Combine(resourcesPath, "legendary.png");
                    break;
                case "ascendant":
                    rarityImagePath = Path.Combine(resourcesPath, "ascendant.png");
                    break;
                case "limit-break":
                    rarityImagePath = Path.Combine(resourcesPath, "limit-break.png");
                    break;
                default:
                    pictureBox1.BackgroundImage = null;  // Clear background for invalid or unknown rarities
                    break;
            }

            if (!string.IsNullOrEmpty(rarityImagePath) && File.Exists(rarityImagePath))
            {
                pictureBox1.BackgroundImage = Image.FromFile(rarityImagePath);
                pictureBox1.BackgroundImageLayout = ImageLayout.Stretch;  // Stretch the background image
            }
            else if (rarity.ToLower() != "normal")
            {
                pictureBox1.BackgroundImage = null;
            }

            // Store the weapon image to be drawn later
            weaponImage = weaponImg;

            // Invalidate the PictureBox to force it to repaint and show the new weapon image
            pictureBox1.Invalidate();
        }

        private void pictureBox1_Paint_1(object sender, PaintEventArgs e)
        {
            if (weaponImage != null)
            {
                // Draw the weapon image over the background
                e.Graphics.DrawImage(weaponImage, new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height));
            }
        }

        private void LoadAndSetRarityImage()
        {
            string selectedRarity = cbRarity.SelectedItem?.ToString();

            Image weaponImg = weaponImage;
            SetRarityAndWeaponImage(selectedRarity, weaponImg);
        }

        private void btnSaveMain_Click(object sender, EventArgs e)
        {
            try
            {
                // Get the base directory where the .exe is located
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

                // Define the path for the XML template
                string templateDirectory = Path.Combine(baseDirectory, "Resources");
                string templateFilePath = Path.Combine(templateDirectory, "ItemBaseXml.xml");

                // Ensure the template file exists
                if (!File.Exists(templateFilePath))
                {
                    MessageBox.Show("ItemBaseXml.xml not found in Resources folder.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Load the template XML file
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(templateFilePath);

                // Replace the XML nodes with the data from the text boxes and combo boxes
                ReplaceXmlContent(xmlDoc);  // This includes the transferType replacement now

                // Get the item ID from the txtItemId TextBox
                string itemId = txtItemId.Text;

                if (string.IsNullOrEmpty(itemId))
                {
                    MessageBox.Show("Item ID cannot be empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Saves image from PictureBox to the Image folder
                SaveImageFromPictureBox();

                // Set the OptionID based on the class and cbStatic value
                SetOptionID(xmlDoc);

                // Define the path to save the modified XML
                string saveDirectory = Path.Combine(baseDirectory, "!Extracted", "Xml", "item", itemId.Substring(0, 1), itemId.Substring(1, 2));
                if (!Directory.Exists(saveDirectory))
                {
                    Directory.CreateDirectory(saveDirectory);
                }
                string saveFilePath = Path.Combine(saveDirectory, $"{itemId}.xml");

                // Save the modified XML with the item ID as the filename
                xmlDoc.Save(saveFilePath);

                ModifyItemNameXml();  // <-- Call to modify itemname.xml
                ModifyItemNamePluralXml();  // <-- Call to modify itemname_plural.xml
                ModifyKorItemDescriptionXml(); // <-- Call to modify koritemdescription.xml
                ModifyItemOptionRandomXml(); // <-- Call to modify itemoptionrandom.xml
                UpdateItemWebFinderXml(); // <-- Call to update itemwebfinder.xml

                MessageBox.Show($"XML file saved successfully as {itemId}.xml", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ReplaceXmlContent(XmlDocument xmlDoc)
        {
            // Replace levelLimit in the "limit" Xml Node
            XmlNode levelNode = xmlDoc.SelectSingleNode("//limit[@levelLimit]");
            if (levelNode != null)
            {
                levelNode.Attributes["levelLimit"].Value = txtLevel.Text;
            }

            // Replace gearScore in the "property" Xml Node
            XmlNode gearScoreNode = xmlDoc.SelectSingleNode("//property[@gearScore]");
            if (gearScoreNode != null)
            {
                gearScoreNode.Attributes["gearScore"].Value = txtScore.Text;
            }

            // Replace name in the "asset" Xml Node
            XmlNode modelNode = xmlDoc.SelectSingleNode("//asset[@name]");
            if (modelNode != null)
            {
                modelNode.Attributes["name"].Value = txtModel.Text;
            }

            // Replace title, static, random, constant, and optionLevelFactor in the "option" Xml Node
            XmlNode optionNode = xmlDoc.SelectSingleNode("//option[@title]");
            if (optionNode != null)
            {
                if (optionNode.Attributes["title"] != null)
                {
                    optionNode.Attributes["title"].Value = txtTitle.Text;
                }
                if (optionNode.Attributes["static"] != null)
                {
                    optionNode.Attributes["static"].Value = txtStatic.Text;
                }
                if (optionNode.Attributes["random"] != null)
                {
                    optionNode.Attributes["random"].Value = txtRandom.Text;
                }
                if (optionNode.Attributes["constant"] != null)
                {
                    optionNode.Attributes["constant"].Value = txtConstant.Text;
                }

                if (optionNode.Attributes["optionLevelFactor"] != null)
                {
                    optionNode.Attributes["optionLevelFactor"].Value = txtScore.Text;
                }
            }

            // Update itemPreset and itemPresetPath
            ReplaceItemPreset(xmlDoc);

            // Update the AdditionalEffect id and level based on txtEffect
            ReplaceAdditionalEffect(xmlDoc);

            // Update transferType
            ReplaceTransferType(xmlDoc);

            // Update the slotIcon path
            UpdateSlotIcon(xmlDoc);

            // Update presetPath based on item ID
            UpdatePresetPath(xmlDoc);  // <-- Add this method call here
        }

        private void ReplaceItemPreset(XmlDocument xmlDoc)
        {
            // Find the "tool" XML node where itemPreset and itemPresetPath exist
            XmlNode toolNode = xmlDoc.SelectSingleNode("//tool");

            if (toolNode != null)
            {
                // Update or add the "itemPreset" attribute
                XmlAttribute itemPresetAttr = toolNode.Attributes["itemPreset"];
                if (itemPresetAttr == null)
                {
                    // If the attribute doesn't exist, create it
                    itemPresetAttr = xmlDoc.CreateAttribute("itemPreset");
                    toolNode.Attributes.Append(itemPresetAttr);
                }
                itemPresetAttr.Value = txtPreset.Text;  // Set the value of itemPreset from txtPreset

                // Extract the first 3 digits from txtPreset to create itemPresetPath
                if (txtPreset.Text.Length >= 3)
                {
                    string firstDigit = txtPreset.Text.Substring(0, 1);        // Get the first digit
                    string secondAndThirdDigits = txtPreset.Text.Substring(1, 2);  // Get the second and third digits
                    string itemPresetPath = $"{firstDigit}/{secondAndThirdDigits}/";  // Format the path

                    // Update or add the "itemPresetPath" attribute
                    XmlAttribute itemPresetPathAttr = toolNode.Attributes["itemPresetPath"];
                    if (itemPresetPathAttr == null)
                    {
                        // If the attribute doesn't exist, create it
                        itemPresetPathAttr = xmlDoc.CreateAttribute("itemPresetPath");
                        toolNode.Attributes.Append(itemPresetPathAttr);
                    }
                    itemPresetPathAttr.Value = itemPresetPath;  // Set the formatted path
                }
            }
        }

        private void ReplaceAdditionalEffect(XmlDocument xmlDoc)
        {
            // Extract IDs from txtEffect, assuming they're comma-separated
            string[] effectIds = txtEffect.Text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(id => id.Trim()).ToArray();

            if (effectIds.Length == 0) return; // If no effects are present, do nothing

            // Find the first AdditionalEffect node (or create one if it doesn't exist)
            XmlNode effectNode = xmlDoc.SelectSingleNode("//AdditionalEffect");
            if (effectNode == null)
            {
                // If no <AdditionalEffect> node exists, create one
                effectNode = xmlDoc.CreateElement("AdditionalEffect");
                xmlDoc.DocumentElement.AppendChild(effectNode);
            }

            // Combine the effect IDs into a comma-separated string and set it as the id attribute
            effectNode.Attributes["id"].Value = string.Join(",", effectIds);

            // Generate the corresponding "level" values (i.e., "1" for each effect ID)
            string levelValue = string.Join(",", Enumerable.Repeat("1", effectIds.Length));

            // Set the level attribute in the AdditionalEffect node
            if (effectNode.Attributes["level"] == null)
            {
                // If the "level" attribute doesn't exist, create it
                XmlAttribute levelAttribute = xmlDoc.CreateAttribute("level");
                levelAttribute.Value = levelValue;
                effectNode.Attributes.Append(levelAttribute);
            }
            else
            {
                // If the "level" attribute exists, update it
                effectNode.Attributes["level"].Value = levelValue;
            }
        }

        private void ReplaceTransferType(XmlDocument xmlDoc)
        {
            // Find the "limit" XML node where transferType exists
            XmlNode limitNode = xmlDoc.SelectSingleNode("//limit[@transferType]");

            if (limitNode != null)
            {
                // Get the selected value from cbTransfer
                string selectedTransferType = cbTransfer.SelectedItem?.ToString();

                // Convert the selected value to the corresponding numeric ID
                int transferTypeCode = GetTransferTypeCodeFromString(selectedTransferType);

                // Update or add the "transferType" attribute
                if (limitNode.Attributes["transferType"] != null)
                {
                    limitNode.Attributes["transferType"].Value = transferTypeCode.ToString();
                }
            }
        }

        private void UpdateSlotIcon(XmlDocument xmlDoc)
        {
            // Find the "property" XML node where the slotIcon attribute exists
            XmlNode propertyNode = xmlDoc.SelectSingleNode("//property[@slotIcon]");

            if (propertyNode != null)
            {
                // Construct the slotIcon path using the item ID from txtItemId
                string slotIconPath = $"./Data/Resource/Image/item/icon/{txtItemId.Text}.png";

                // Update or add the "slotIcon" attribute
                XmlAttribute slotIconAttr = propertyNode.Attributes["slotIcon"];
                if (slotIconAttr == null)
                {
                    // If the attribute doesn't exist, create it
                    slotIconAttr = xmlDoc.CreateAttribute("slotIcon");
                    propertyNode.Attributes.Append(slotIconAttr);
                }

                // Set the value to the constructed path
                slotIconAttr.Value = slotIconPath;
            }
        }

        private void UpdatePresetPath(XmlDocument xmlDoc)
        {
            // Find the "property" XML node where the presetPath attribute exists
            XmlNode propertyNode = xmlDoc.SelectSingleNode("//property[@presetPath]");

            if (propertyNode != null)
            {
                // Extract the first digit and the second and third digits from the item ID
                if (txtItemId.Text.Length >= 3)
                {
                    string firstDigit = txtItemId.Text.Substring(0, 1);  // Get the first digit
                    string secondAndThirdDigits = txtItemId.Text.Substring(1, 2);  // Get the second and third digits

                    // Construct the new presetPath value using the first three digits of the item ID
                    string presetPath = $"{firstDigit}/{secondAndThirdDigits}/";

                    // Update or add the "presetPath" attribute
                    XmlAttribute presetPathAttr = propertyNode.Attributes["presetPath"];
                    if (presetPathAttr == null)
                    {
                        // If the attribute doesn't exist, create it
                        presetPathAttr = xmlDoc.CreateAttribute("presetPath");
                        propertyNode.Attributes.Append(presetPathAttr);
                    }

                    // Set the value to the constructed path
                    presetPathAttr.Value = presetPath;
                }
            }
        }

        private void SetOptionID(XmlDocument xmlDoc)
        {
            // Dictionaries for physical and magic weapons, keyed by class prefix + selected index
            Dictionary<string, string> physicalWeaponIDs = new Dictionary<string, string>
            {
                { "134_0", "134010101" }, { "134_1", "134010201" },
                { "150_0", "150010101" }, { "150_1", "150010201" },
                { "151_0", "151010101" }, { "151_1", "151010201" },
                { "153_0", "153010101" }, { "153_1", "153010201" },
                { "155_0", "155010101" }, { "155_1", "155010201" }
            };

            Dictionary<string, string> magicWeaponIDs = new Dictionary<string, string>
            {
                { "133_2", "133010101" }, { "133_3", "133010201" },
                { "152_2", "152010101" }, { "152_3", "152010201" },
                { "154_2", "154010101" }, { "154_3", "154010201" },
                { "156_2", "156010101" }, { "156_3", "156010201" }
            };

            // Detect class by the first three digits and selected class in cbClass
            string classWeaponPrefix = GetWeaponPrefixByClass();

            if (string.IsNullOrEmpty(classWeaponPrefix))
            {
                MessageBox.Show("Invalid class selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Determine whether the weapon is physical or magical based on cbStatic value
            bool isPhysicalWeapon = cbStatic.SelectedIndex == 0 || cbStatic.SelectedIndex == 1; // First two indices are physical

            string optionID = "";

            // Create the key by combining the class prefix with the selected index
            string key = $"{classWeaponPrefix}_{cbStatic.SelectedIndex}";

            // Find the appropriate optionID from the dictionaries based on the class prefix and selected index
            if (isPhysicalWeapon)
            {
                optionID = GetOptionIDByPrefix(physicalWeaponIDs, key);
            }
            else
            {
                optionID = GetOptionIDByPrefix(magicWeaponIDs, key);
            }

            if (!string.IsNullOrEmpty(optionID))
            {
                // Set the optionID in the XML
                XmlNode optionNode = xmlDoc.SelectSingleNode("//option[@optionID]");
                if (optionNode != null)
                {
                    optionNode.Attributes["optionID"].Value = optionID;
                }
            }
            else
            {
                MessageBox.Show("Could not find a valid OptionID for the selected class and weapon type.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetOptionIDByPrefix(Dictionary<string, string> weaponDictionary, string key)
        {
            // Return the matching weapon ID if the key exists in the dictionary
            if (weaponDictionary.ContainsKey(key))
            {
                return weaponDictionary[key];
            }
            return string.Empty;  // Return empty string if no match is found
        }

        private string GetWeaponPrefixByClass()
        {
            string className = cbClass.SelectedItem?.ToString();

            switch (className)
            {
                case "Thief":
                    return "131";
                case "Knight":
                    return "132";
                case "Priest":
                    return "133";
                case "Assassin":
                    return "134";
                case "Berserker":
                    return "150";
                case "Archer":
                    return "151";
                case "Wizard":
                    return "152";
                case "Heavy Gunner":
                    return "153";
                case "Runeblade":
                    return "154";
                case "Striker":
                    return "155";
                case "Soulbinder":
                    return "156";
                default:
                    return "000";  // Default or unknown class
            }
        }

        private void PopulateCbStaticDefault()
        {
            // Clear any existing items in cbStatic
            cbStatic.Items.Clear();

            // Add the default options for both physical and magic weapons
            cbStatic.Items.Add("None");
            cbStatic.Items.Add("maxatk, crate, physatk");
            cbStatic.Items.Add("maxatk, health, physatk");
            cbStatic.Items.Add("maxatk, crate, magatk");
            cbStatic.Items.Add("maxatk, health, magatk");

            // Optionally set the default selected index
            cbStatic.SelectedIndex = 0;
        }

        private void cbClass_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Trigger the same logic when the class is changed
            string selectedClass = cbClass.SelectedItem?.ToString();

            if (IsPhysicalClass(selectedClass))
            {
                PopulateCbStaticForPhysical();
            }
            else if (IsMagicClass(selectedClass))
            {
                PopulateCbStaticForMagic();
            }
        }

        private bool IsPhysicalClass(string className)
        {
            var physicalClasses = new List<string>
            {
                 "Thief", "Knight", "Berserker", "Archer", "Heavy Gunner", "Assassin", "Striker"
            };

            return physicalClasses.Contains(className);
        }

        private bool IsMagicClass(string className)
        {
            var magicClasses = new List<string>
            {
                "Priest", "Wizard", "Soulbinder", "Runeblade"
            };

            return magicClasses.Contains(className);
        }

        private void PopulateCbStaticForPhysical()
        {
            cbStatic.Items.Clear();
            cbStatic.Items.Add("maxatk, crate, physatk");
            cbStatic.Items.Add("maxatk, health, physatk");
            cbStatic.SelectedIndex = 0;
        }

        private void PopulateCbStaticForMagic()
        {
            cbStatic.Items.Clear();
            cbStatic.Items.Add("maxatk, crate, magatk");
            cbStatic.Items.Add("maxatk, health, magatk");
            cbStatic.SelectedIndex = 0;
        }

        private void txtItemId_TextChanged(object sender, EventArgs e)
        {
            // Get the current Item ID from the text box
            string itemId = txtItemId.Text;

            if (itemId.Length >= 3)
            {
                // Extract the first three digits of the Item ID
                string itemPrefix = itemId.Substring(0, 3);

                // Detect the class based on the prefix and set the appropriate class in cbClass
                SetComboBoxClassByWeaponPrefix(itemPrefix);

                // Get the selected class from cbClass
                string selectedClass = cbClass.SelectedItem?.ToString();

                // Determine if the class is physical or magic and update cbStatic
                if (IsPhysicalClass(selectedClass))
                {
                    PopulateCbStaticForPhysical();
                }
                else if (IsMagicClass(selectedClass))
                {
                    PopulateCbStaticForMagic();
                }
            }
        }

        private void SetComboBoxClassByWeaponPrefix(string itemPrefix)
        {
            // Use the prefix to determine the class and set cbClass to the appropriate value
            switch (itemPrefix)
            {
                case "131":
                    cbClass.SelectedItem = "Thief";
                    break;
                case "132":
                    cbClass.SelectedItem = "Knight";
                    break;
                case "133":
                    cbClass.SelectedItem = "Priest";
                    break;
                case "134":
                    cbClass.SelectedItem = "Assassin";
                    break;
                case "150":
                    cbClass.SelectedItem = "Berserker";
                    break;
                case "151":
                    cbClass.SelectedItem = "Archer";
                    break;
                case "152":
                    cbClass.SelectedItem = "Wizard";
                    break;
                case "153":
                    cbClass.SelectedItem = "Heavy Gunner";
                    break;
                case "154":
                    cbClass.SelectedItem = "Runeblade";
                    break;
                case "155":
                    cbClass.SelectedItem = "Striker";
                    break;
                case "156":
                    cbClass.SelectedItem = "Soulbinder";
                    break;
                default:
                    // Clear the class if no valid match is found
                    cbClass.SelectedIndex = -1;
                    break;
            }
        }

        private void ModifyItemNameXml()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string itemNameFilePath = Path.Combine(baseDirectory, "!Extracted", "Xml", "string", "en", "itemname.xml");

            // Ensure the itemname.xml file exists
            if (!File.Exists(itemNameFilePath))
            {
                MessageBox.Show("itemname.xml not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            XmlDocument itemNameDoc = new XmlDocument();
            itemNameDoc.Load(itemNameFilePath);

            // Get the root node <ms2> where items are listed
            XmlNode root = itemNameDoc.SelectSingleNode("/ms2");

            if (root == null)
            {
                MessageBox.Show("Invalid itemname.xml format.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string newItemId = txtItemId.Text;
            XmlNode existingItem = root.SelectSingleNode($"//key[@id='{newItemId}']");

            // Check if the item with the same ID already exists
            if (existingItem != null)
            {
                // Ask the user if they want to overwrite the existing item
                DialogResult result = MessageBox.Show($"An item with ID {newItemId} already exists. Do you want to overwrite it?", "Item Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.No)
                {
                    return;  // If the user chooses not to overwrite, stop here
                }

                // If the user chooses to overwrite, update the existing item's attributes
                UpdateExistingItem(existingItem);
            }
            else
            {
                // Ensure the <!-- Custom items --> and <!-- Vanilla items --> markers are in place
                XmlComment customItemsComment = null;
                XmlComment vanillaItemsComment = null;

                foreach (XmlNode node in root.ChildNodes)
                {
                    if (node.NodeType == XmlNodeType.Comment && node.Value == "Custom items")
                    {
                        customItemsComment = (XmlComment)node;
                    }
                    else if (node.NodeType == XmlNodeType.Comment && node.Value == "Vanilla items")
                    {
                        vanillaItemsComment = (XmlComment)node;
                    }
                }

                // If the custom items comment is not found, create it and add it to the document
                if (customItemsComment == null)
                {
                    customItemsComment = itemNameDoc.CreateComment("Custom items");
                    root.InsertBefore(customItemsComment, root.FirstChild);
                }

                // If the vanilla items comment is not found, create it and add it before the first vanilla item
                if (vanillaItemsComment == null)
                {
                    foreach (XmlNode node in root.ChildNodes)
                    {
                        if (node.Name == "key" && node.Attributes["id"]?.Value == "1")
                        {
                            vanillaItemsComment = itemNameDoc.CreateComment("Vanilla items");
                            root.InsertBefore(vanillaItemsComment, node);
                            break;
                        }
                    }
                }

                // Create a new custom item entry under the "Custom items" section
                XmlElement newItem = itemNameDoc.CreateElement("key");

                // Set the id attribute
                XmlAttribute idAttr = itemNameDoc.CreateAttribute("id");
                idAttr.Value = txtItemId.Text;
                newItem.Attributes.Append(idAttr);

                // Convert the class from cbClass to the appropriate value
                string className = ConvertClassToWeaponType(cbClass.SelectedItem?.ToString());
                if (!string.IsNullOrEmpty(className))
                {
                    XmlAttribute classAttr = itemNameDoc.CreateAttribute("class");
                    classAttr.Value = className;
                    newItem.Attributes.Append(classAttr);
                }

                // Set the name attribute
                XmlAttribute nameAttr = itemNameDoc.CreateAttribute("name");
                nameAttr.Value = txtName.Text;
                newItem.Attributes.Append(nameAttr);

                // Insert the new item right after the "Custom items" comment
                root.InsertAfter(newItem, customItemsComment);
            }

            // Save the updated itemname.xml
            itemNameDoc.Save(itemNameFilePath);
            MessageBox.Show($"Item with ID {newItemId} has been successfully added/updated.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void UpdateExistingItem(XmlNode existingItem)
        {
            // Update the name attribute with the new value from txtName
            XmlAttribute nameAttr = existingItem.Attributes["name"];
            if (nameAttr != null)
            {
                nameAttr.Value = txtName.Text;
            }
            else
            {
                nameAttr = existingItem.OwnerDocument.CreateAttribute("name");
                nameAttr.Value = txtName.Text;
                existingItem.Attributes.Append(nameAttr);
            }

            // Update the class attribute based on the selected class in cbClass
            string className = ConvertClassToWeaponType(cbClass.SelectedItem?.ToString());
            XmlAttribute classAttr = existingItem.Attributes["class"];
            if (!string.IsNullOrEmpty(className))
            {
                if (classAttr != null)
                {
                    classAttr.Value = className;
                }
                else
                {
                    classAttr = existingItem.OwnerDocument.CreateAttribute("class");
                    classAttr.Value = className;
                    existingItem.Attributes.Append(classAttr);
                }
            }
        }

        private string ConvertClassToWeaponType(string className)
        {
            // Convert the selected class to the appropriate weapon type
            switch (className)
            {
                case "Thief":
                    return "dagger";
                case "Knight":
                    return "longsword";
                case "Priest":
                    return "scepter";
                case "Assassin":
                    return "javelin";
                case "Berserker":
                    return "large sword";
                case "Archer":
                    return "bow";
                case "Wizard":
                    return "staff";
                case "Heavy Gunner":
                    return "cannon";
                case "Runeblade":
                    return "blade";
                case "Striker":
                    return "knuckle";
                case "Soulbinder":
                    return "orb";
                default:
                    return string.Empty;  // Return empty if no matching class is found
            }
        }

        private void ModifyItemNamePluralXml()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string itemNamePluralFilePath = Path.Combine(baseDirectory, "!Extracted", "Xml", "string", "en", "itemnameplural.xml");

            // Ensure the itemnameplural.xml file exists
            if (!File.Exists(itemNamePluralFilePath))
            {
                MessageBox.Show("itemnameplural.xml not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            XmlDocument itemNamePluralDoc = new XmlDocument();
            itemNamePluralDoc.Load(itemNamePluralFilePath);

            // Get the root node <ms2> where items are listed
            XmlNode root = itemNamePluralDoc.SelectSingleNode("/ms2");

            if (root == null)
            {
                MessageBox.Show("Invalid itemnameplural.xml format.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string newItemId = txtItemId.Text;
            XmlNode existingItem = root.SelectSingleNode($"//key[@id='{newItemId}']");

            // Check if the item with the same ID already exists
            if (existingItem != null)
            {
                // Ask the user if they want to overwrite the existing item
                DialogResult result = MessageBox.Show($"An item with ID {newItemId} already exists in itemnameplural.xml. Do you want to overwrite it?", "Item Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.No)
                {
                    return;  // If the user chooses not to overwrite, stop here
                }

                // If the user chooses to overwrite, update the existing item's attributes
                UpdateExistingItemPlural(existingItem);
            }

            else
            {
                // Ensure the <!-- Custom items --> and <!-- Vanilla items --> markers are in place
                XmlComment customItemsComment = null;
                XmlComment vanillaItemsComment = null;

                foreach (XmlNode node in root.ChildNodes)
                {
                    if (node.NodeType == XmlNodeType.Comment && node.Value == "Custom items")
                    {
                        customItemsComment = (XmlComment)node;
                    }
                    else if (node.NodeType == XmlNodeType.Comment && node.Value == "Vanilla items")
                    {
                        vanillaItemsComment = (XmlComment)node;
                    }
                }

                // If the custom items comment is not found, create it and add it to the document
                if (customItemsComment == null)
                {
                    customItemsComment = itemNamePluralDoc.CreateComment("Custom items");
                    root.InsertBefore(customItemsComment, root.FirstChild);
                }

                // If the vanilla items comment is not found, create it and add it before the first vanilla item
                if (vanillaItemsComment == null)
                {
                    foreach (XmlNode node in root.ChildNodes)
                    {
                        if (node.Name == "key" && node.Attributes["id"]?.Value == "1")
                        {
                            vanillaItemsComment = itemNamePluralDoc.CreateComment("Vanilla items");
                            root.InsertBefore(vanillaItemsComment, node);
                            break;
                        }
                    }
                }

                // Create a new custom item entry under the "Custom items" section
                XmlElement newItem = itemNamePluralDoc.CreateElement("key");

                // Set the id attribute
                XmlAttribute idAttr = itemNamePluralDoc.CreateAttribute("id");
                idAttr.Value = txtItemId.Text;
                newItem.Attributes.Append(idAttr);

                // Convert the class from cbClass to the appropriate value
                string className = ConvertClassToWeaponType(cbClass.SelectedItem?.ToString());
                if (!string.IsNullOrEmpty(className))
                {
                    XmlAttribute classAttr = itemNamePluralDoc.CreateAttribute("class");
                    classAttr.Value = className;
                    newItem.Attributes.Append(classAttr);
                }

                // Set the name attribute with "s" added to the end
                XmlAttribute nameAttr = itemNamePluralDoc.CreateAttribute("name");
                nameAttr.Value = txtName.Text + "s";  // Add "s" at the end of the name
                newItem.Attributes.Append(nameAttr);

                // Insert the new item right after the "Custom items" comment
                root.InsertAfter(newItem, customItemsComment);
            }

            // Save the updated itemnameplural.xml
            itemNamePluralDoc.Save(itemNamePluralFilePath);
            MessageBox.Show($"Item with ID {newItemId} has been successfully added/updated in itemnameplural.xml.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void UpdateExistingItemPlural(XmlNode existingItem)
        {
            // Update the name attribute with the new value from txtName, adding "s" at the end
            XmlAttribute nameAttr = existingItem.Attributes["name"];
            if (nameAttr != null)
            {
                nameAttr.Value = txtName.Text + "s";
            }
            else
            {
                nameAttr = existingItem.OwnerDocument.CreateAttribute("name");
                nameAttr.Value = txtName.Text + "s";
                existingItem.Attributes.Append(nameAttr);
            }

            // Update the class attribute based on the selected class in cbClass
            string className = ConvertClassToWeaponType(cbClass.SelectedItem?.ToString());
            XmlAttribute classAttr = existingItem.Attributes["class"];
            if (!string.IsNullOrEmpty(className))
            {
                if (classAttr != null)
                {
                    classAttr.Value = className;
                }
                else
                {
                    classAttr = existingItem.OwnerDocument.CreateAttribute("class");
                    classAttr.Value = className;
                    existingItem.Attributes.Append(classAttr);
                }
            }
        }

        private void ModifyKorItemDescriptionXml()
        {
            try
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string korItemDescriptionFilePath = Path.Combine(baseDirectory, "!Extracted", "Xml", "string", "en", "koritemdescription.xml");

                // Ensure the koritemdescription.xml file exists
                if (!File.Exists(korItemDescriptionFilePath))
                {
                    MessageBox.Show("koritemdescription.xml not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                XmlDocument korItemDescriptionDoc = new XmlDocument();
                korItemDescriptionDoc.Load(korItemDescriptionFilePath);

                // Get the root node <ms2> where items are listed
                XmlNode root = korItemDescriptionDoc.SelectSingleNode("/ms2");

                if (root == null)
                {
                    MessageBox.Show("Invalid koritemdescription.xml format.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string newItemId = txtItemId.Text;
                XmlNode existingItem = root.SelectSingleNode($"//key[@id='{newItemId}']");

                // Check if the item with the same ID already exists
                if (existingItem != null)
                {
                    // Ask the user if they want to overwrite the existing item
                    DialogResult result = MessageBox.Show($"An item with ID {newItemId} already exists in koritemdescription.xml. Do you want to overwrite it?", "Item Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.No)
                    {
                        return;  // If the user chooses not to overwrite, stop here
                    }

                    // If the user chooses to overwrite, update the existing item's attributes
                    UpdateExistingKorItemDescription(existingItem);
                }
                else
                {
                    // Ensure the <!-- Custom items --> and <!-- Vanilla items --> markers are in place
                    XmlComment customItemsComment = null;
                    XmlComment vanillaItemsComment = null;

                    // Iterate through all child nodes of the root node
                    foreach (XmlNode node in root.ChildNodes)
                    {
                        if (node.NodeType == XmlNodeType.Comment && node.Value == "Custom items")
                        {
                            customItemsComment = (XmlComment)node;
                        }
                        else if (node.NodeType == XmlNodeType.Comment && node.Value == "Vanilla items")
                        {
                            vanillaItemsComment = (XmlComment)node;
                        }
                    }

                    // If the custom items comment is not found, create it and add it to the document
                    if (customItemsComment == null)
                    {
                        customItemsComment = korItemDescriptionDoc.CreateComment("Custom items");
                        root.InsertBefore(customItemsComment, root.FirstChild);  // Insert at the top
                    }

                    // If the vanilla items comment is not found, create it and add it before the first vanilla item
                    if (vanillaItemsComment == null)
                    {
                        foreach (XmlNode node in root.ChildNodes)
                        {
                            if (node.Name == "key" && node.Attributes["id"] != null && node.Attributes["id"].Value == "11020005") // Example: detect vanilla items
                            {
                                vanillaItemsComment = korItemDescriptionDoc.CreateComment("Vanilla items");
                                root.InsertBefore(vanillaItemsComment, node);  // Insert before the first vanilla item
                                break;
                            }
                        }
                    }

                    // Create a new custom item entry under the "Custom items" section
                    XmlElement newItem = korItemDescriptionDoc.CreateElement("key");

                    // Set the id attribute
                    XmlAttribute idAttr = korItemDescriptionDoc.CreateAttribute("id");
                    idAttr.Value = txtItemId.Text;
                    newItem.Attributes.Append(idAttr);

                    // Set the tooltipDescription attribute
                    XmlAttribute tooltipAttr = korItemDescriptionDoc.CreateAttribute("tooltipDescription");
                    tooltipAttr.Value = txtTipDescription.Text;
                    newItem.Attributes.Append(tooltipAttr);

                    // Set the guideDescription attribute
                    XmlAttribute guideAttr = korItemDescriptionDoc.CreateAttribute("guideDescription");
                    guideAttr.Value = txtGuideDescription.Text;
                    newItem.Attributes.Append(guideAttr);

                    // Insert the new item right after the "Custom items" comment
                    root.InsertAfter(newItem, customItemsComment);
                }

                // Save the updated koritemdescription.xml
                korItemDescriptionDoc.Save(korItemDescriptionFilePath);
                MessageBox.Show($"Item with ID {newItemId} has been successfully added/updated in koritemdescription.xml.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateExistingKorItemDescription(XmlNode existingItem)
        {
            // Update the tooltipDescription attribute with the new value from txtTipDescription
            XmlAttribute tooltipAttr = existingItem.Attributes["tooltipDescription"];
            if (tooltipAttr != null)
            {
                tooltipAttr.Value = txtTipDescription.Text;
            }
            else
            {
                tooltipAttr = existingItem.OwnerDocument.CreateAttribute("tooltipDescription");
                tooltipAttr.Value = txtTipDescription.Text;
                existingItem.Attributes.Append(tooltipAttr);
            }

            // Update the guideDescription attribute with the new value from txtGuideDescription
            XmlAttribute guideAttr = existingItem.Attributes["guideDescription"];
            if (guideAttr != null)
            {
                guideAttr.Value = txtGuideDescription.Text;
            }
            else
            {
                guideAttr = existingItem.OwnerDocument.CreateAttribute("guideDescription");
                guideAttr.Value = txtGuideDescription.Text;
                existingItem.Attributes.Append(guideAttr);
            }
        }

        private void ModifyItemOptionRandomXml()
        {
            try
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string xmlDirectory = Path.Combine(baseDirectory, "!Extracted", "Xml");  // Ensure we are looking inside the Xml folder
                string itemId = txtItemId.Text;

                // Get the second and third digits of the Item ID for opening the correct file
                if (itemId.Length < 3)
                {
                    MessageBox.Show("Invalid Item ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string secondAndThirdDigits = itemId.Substring(1, 2);  // Extract 2nd and 3rd digits

                // Construct the path to the itemoptionrandom_n.xml file
                string optionRandomFilePath = Path.Combine(xmlDirectory, "itemoption", "option", "random", $"itemoptionrandom_{secondAndThirdDigits}.xml");

                // Ensure the file exists
                if (!File.Exists(optionRandomFilePath))
                {
                    MessageBox.Show($"itemoptionrandom_{secondAndThirdDigits}.xml not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Load the XML document
                XmlDocument optionRandomDoc = new XmlDocument();
                optionRandomDoc.Load(optionRandomFilePath);

                // Get the root node <ms2> where the options are listed
                XmlNode root = optionRandomDoc.SelectSingleNode("/ms2");

                if (root == null)
                {
                    MessageBox.Show("Invalid itemoptionrandom XML format.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string randomId = txtRandom.Text;
                XmlNode existingItem = root.SelectSingleNode($"//option[@code='{randomId}']");

                // Check if the item with the same random ID already exists
                if (existingItem != null)
                {
                    // Ask the user if they want to overwrite the existing item
                    DialogResult result = MessageBox.Show($"An item with ID {randomId} already exists in itemoptionrandom_{secondAndThirdDigits}.xml. Do you want to overwrite it?", "Item Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.No)
                    {
                        return;  // If the user chooses not to overwrite, stop here
                    }

                    // If the user chooses to overwrite, update the existing item's attributes
                    UpdateExistingItemOptionRandom(existingItem);
                }
                else
                {
                    // Ensure the <!-- Custom items --> and <!-- Vanilla items --> markers are in place
                    XmlComment customItemsComment = null;
                    XmlComment vanillaItemsComment = null;

                    foreach (XmlNode node in root.ChildNodes)
                    {
                        if (node.NodeType == XmlNodeType.Comment && node.Value == "Custom items")
                        {
                            customItemsComment = (XmlComment)node;
                        }
                        else if (node.NodeType == XmlNodeType.Comment && node.Value == "Vanilla items")
                        {
                            vanillaItemsComment = (XmlComment)node;
                        }
                    }

                    // If the custom items comment is not found, create it and add it to the document
                    if (customItemsComment == null)
                    {
                        customItemsComment = optionRandomDoc.CreateComment("Custom items");
                        root.InsertBefore(customItemsComment, root.FirstChild);
                    }

                    // If the vanilla items comment is not found, create it and add it before the first vanilla item
                    if (vanillaItemsComment == null)
                    {
                        foreach (XmlNode node in root.ChildNodes)
                        {
                            if (node.Name == "option" && node.Attributes["code"]?.Value == "15000001")  // Adjust to detect vanilla items
                            {
                                vanillaItemsComment = optionRandomDoc.CreateComment("Vanilla items");
                                root.InsertBefore(vanillaItemsComment, node);
                                break;
                            }
                        }
                    }

                    // Create a new custom item entry under the "Custom items" section
                    XmlElement newItem = optionRandomDoc.CreateElement("option");

                    // Set the code (id) attribute
                    XmlAttribute codeAttr = optionRandomDoc.CreateAttribute("code");
                    codeAttr.Value = txtRandom.Text;
                    newItem.Attributes.Append(codeAttr);

                    // Set the grade attribute from cbRarity (convert it to original value)
                    XmlAttribute gradeAttr = optionRandomDoc.CreateAttribute("grade");
                    gradeAttr.Value = GetGradeFromRarity(cbRarity.SelectedItem?.ToString());
                    newItem.Attributes.Append(gradeAttr);

                    // Set the optionNumPick attribute based on grade
                    XmlAttribute optionNumPickAttr = optionRandomDoc.CreateAttribute("optionNumPick");
                    optionNumPickAttr.Value = GetOptionNumPickFromGrade(gradeAttr.Value);
                    newItem.Attributes.Append(optionNumPickAttr);

                    // Add the static values for the item (based on weapon type or file)
                    AppendStaticItemValues(newItem, secondAndThirdDigits);

                    // Insert the new item right after the "Custom items" comment
                    root.InsertAfter(newItem, customItemsComment);
                }

                // Save the updated itemoptionrandom_n.xml
                optionRandomDoc.Save(optionRandomFilePath);
                MessageBox.Show($"Item with Random ID {randomId} has been successfully added/updated in itemoptionrandom_{secondAndThirdDigits}.xml.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Method to get the optionNumPick value from grade
        private string GetOptionNumPickFromGrade(string grade)
        {
            switch (grade)
            {
                case "1":
                    return "0,0";
                case "2":
                    return "1,1";
                case "3":
                    return "2,2";
                case "4":
                    return "2,2";
                case "5":
                    return "2,2";
                case "6":
                    return "3,3";
                default:
                    return "0,0";  // Default value
            }
        }

        // Method to append static values for the item based on the second and third digits of Item ID
        // This is temporary for now, in the future need to make it dynamic with user selection of stats
        private void AppendStaticItemValues(XmlElement newItem, string secondAndThirdDigits)
        {
            switch (secondAndThirdDigits)
            {
                case "31":
                    AppendAttributes(newItem, "str_value_base=\"8\" dex_value_base=\"8\" int_value_base=\"8\" luk_value_base=\"8\" cap_value_base=\"9\" cad_value_base=\"34\" pap_value_base=\"5\" pen_rate_base=\"0.014\" sgi_rate_base=\"0.014\" sgi_target=\"4\" poisondamage_rate_base=\"0.027\" finaladditionaldamage_rate_base=\"0.013\" parpen_rate_base=\"0.048\"");
                    break;
                case "32":
                    AppendAttributes(newItem, "str_value_base=\"8\" dex_value_base=\"8\" int_value_base=\"8\" luk_value_base=\"8\" cap_value_base=\"9\" cad_value_base=\"34\" pap_value_base=\"5\" pen_rate_base=\"0.014\" sgi_rate_base=\"0.014\" sgi_target=\"4\" lightdamage_rate_base=\"0.027\" finaladditionaldamage_rate_base=\"0.013\" parpen_rate_base=\"0.048\"");
                    break;
                case "33":
                    AppendAttributes(newItem, "str_value_base=\"8\" dex_value_base=\"8\" int_value_base=\"8\" luk_value_base=\"8\" cap_value_base=\"9\" cad_value_base=\"34\" map_value_base=\"5\" pen_rate_base=\"0.014\" sgi_rate_base=\"0.014\" sgi_target=\"4\" heal_rate_base=\"0.027\" lightdamage_rate_base=\"0.027\" finaladditionaldamage_rate_base=\"0.013\" marpen_rate_base=\"0.048\"");
                    break;
                case "34":
                    AppendAttributes(newItem, "str_value_base=\"8\" dex_value_base=\"8\" int_value_base=\"8\" luk_value_base=\"8\" cap_value_base=\"9\" cad_value_base=\"34\" pap_value_base=\"5\" pen_rate_base=\"0.014\" sgi_rate_base=\"0.014\" sgi_target=\"4\" darkdamage_rate_base=\"0.027\" finaladditionaldamage_rate_base=\"0.013\" parpen_rate_base=\"0.048\"");
                    break;
                case "50":
                    AppendAttributes(newItem, "multiply_factor=\"2\" str_value_base=\"16\" dex_value_base=\"16\" int_value_base=\"16\" luk_value_base=\"16\" cap_value_base=\"18\" cad_value_base=\"69\" pap_value_base=\"10\" pen_rate_base=\"0.029\" sgi_rate_base=\"0.029\" sgi_target=\"4\" darkdamage_rate_base=\"0.055\" finaladditionaldamage_rate_base=\"0.026\" parpen_rate_base=\"0.096\"");
                    break;
                case "51":
                    AppendAttributes(newItem, "multiply_factor=\"2\" str_value_base=\"16\" dex_value_base=\"16\" int_value_base=\"16\" luk_value_base=\"16\" cap_value_base=\"18\" cad_value_base=\"69\" pap_value_base=\"10\" pen_rate_base=\"0.029\" sgi_rate_base=\"0.029\" sgi_target=\"4\" icedamage_rate_base=\"0.055\" firedamage_rate_base=\"0.055\" finaladditionaldamage_rate_base=\"0.026\" parpen_rate_base=\"0.096\"");
                    break;
                case "52":
                    AppendAttributes(newItem, "multiply_factor=\"2\" str_value_base=\"16\" dex_value_base=\"16\" int_value_base=\"16\" luk_value_base=\"16\" cap_value_base=\"18\" cad_value_base=\"69\" map_value_base=\"10\" pen_rate_base=\"0.029\" sgi_rate_base=\"0.029\" sgi_target=\"4\" icedamage_rate_base=\"0.055\" firedamage_rate_base=\"0.055\" thunderdamage_rate_base=\"0.055\" finaladditionaldamage_rate_base=\"0.026\" marpen_rate_base=\"0.096\"");
                    break;
                case "53":
                    AppendAttributes(newItem, "multiply_factor=\"2\" str_value_base=\"16\" dex_value_base=\"16\" int_value_base=\"16\" luk_value_base=\"16\" cap_value_base=\"18\" cad_value_base=\"69\" pap_value_base=\"10\" pen_rate_base=\"0.029\" sgi_rate_base=\"0.029\" sgi_target=\"4\" firedamage_rate_base=\"0.055\" thunderdamage_rate_base=\"0.055\" finaladditionaldamage_rate_base=\"0.026\" parpen_rate_base=\"0.096\"");
                    break;
                case "54":
                    AppendAttributes(newItem, "multiply_factor=\"2\" str_value_base=\"16\" dex_value_base=\"16\" int_value_base=\"16\" luk_value_base=\"16\" cap_value_base=\"18\" cad_value_base=\"69\" map_value_base=\"10\" pen_rate_base=\"0.029\" sgi_rate_base=\"0.029\" sgi_target=\"4\" icedamage_rate_base=\"0.055\" firedamage_rate_base=\"0.055\" thunderdamage_rate_base=\"0.055\" finaladditionaldamage_rate_base=\"0.026\" parpen_rate_base=\"0.096\"");
                    break;
                case "55":
                    AppendAttributes(newItem, "multiply_factor=\"2\" str_value_base=\"16\" dex_value_base=\"16\" int_value_base=\"16\" luk_value_base=\"16\" cap_value_base=\"18\" cad_value_base=\"69\" pap_value_base=\"10\" pen_rate_base=\"0.029\" sgi_rate_base=\"0.029\" sgi_target=\"4\" firedamage_rate_base=\"0.055\" finaladditionaldamage_rate_base=\"0.026\" parpen_rate_base=\"0.096\"");
                    break;
                case "56":
                    AppendAttributes(newItem, "multiply_factor=\"2\" str_value_base=\"16\" dex_value_base=\"16\" int_value_base=\"16\" luk_value_base=\"16\" cap_value_base=\"18\" cad_value_base=\"69\" map_value_base=\"10\" pen_rate_base=\"0.029\" sgi_rate_base=\"0.029\" sgi_target=\"4\" finaladditionaldamage_rate_base=\"0.026\" marpen_rate_base=\"0.096\"");
                    break;
                default:
                    MessageBox.Show("Invalid Item ID digits.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }

        // Helper method to append attributes to the newItem node
        private void AppendAttributes(XmlElement newItem, string attributes)
        {
            // Split the attributes string and create XmlAttributes for each key-value pair
            string[] pairs = attributes.Split(' ');
            foreach (string pair in pairs)
            {
                string[] keyValue = pair.Split('=');
                if (keyValue.Length == 2)
                {
                    XmlAttribute newAttr = newItem.OwnerDocument.CreateAttribute(keyValue[0]);
                    newAttr.Value = keyValue[1].Trim('"');
                    newItem.Attributes.Append(newAttr);
                }
            }
        }

        private void UpdateExistingItemOptionRandom(XmlNode existingItem)
        {
            // Update the grade attribute based on cbRarity (convert it back to the original value)
            XmlAttribute gradeAttr = existingItem.Attributes["grade"];
            if (gradeAttr != null)
            {
                gradeAttr.Value = GetGradeFromRarity(cbRarity.SelectedItem?.ToString());
            }
            else
            {
                gradeAttr = existingItem.OwnerDocument.CreateAttribute("grade");
                gradeAttr.Value = GetGradeFromRarity(cbRarity.SelectedItem?.ToString());
                existingItem.Attributes.Append(gradeAttr);
            }
        }

        private void UpdateItemWebFinderXml()
        {
            try
            {
                // Define the path to the itemwebfinder.xml file
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string webFinderFilePath = Path.Combine(baseDirectory, "!Extracted", "Xml", "table", "na", "itemwebfinder.xml");

                // Check if the file exists
                if (!File.Exists(webFinderFilePath))
                {
                    MessageBox.Show("itemwebfinder.xml not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Load the XML file
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(webFinderFilePath);

                // Get the root node <ms2>
                XmlNode rootNode = xmlDoc.SelectSingleNode("/ms2");

                if (rootNode == null)
                {
                    MessageBox.Show("Invalid itemwebfinder.xml format. Root <ms2> tag not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string itemId = txtItemId.Text;
                XmlNode existingItemNode = rootNode.SelectSingleNode($"//key[@id='{itemId}']");

                // Check if the item already exists by its ID
                if (existingItemNode != null)
                {
                    // Prompt the user to confirm if they want to overwrite the existing item
                    DialogResult result = MessageBox.Show($"Item with ID {itemId} already exists. Do you want to overwrite it?", "Item Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.No)
                    {
                        return;
                    }

                    // If the user chooses to overwrite, remove the existing node
                    rootNode.RemoveChild(existingItemNode);
                }

                // Ensure the <!-- Custom --> and <!-- Vanilla --> comments are in place
                XmlComment customItemsComment = null;
                XmlComment vanillaItemsComment = null;

                foreach (XmlNode node in rootNode.ChildNodes)
                {
                    if (node.NodeType == XmlNodeType.Comment && node.Value.Trim() == "Custom")
                    {
                        customItemsComment = (XmlComment)node;
                    }
                    else if (node.NodeType == XmlNodeType.Comment && node.Value.Trim() == "Vanilla")
                    {
                        vanillaItemsComment = (XmlComment)node;
                    }
                }

                // If the custom items comment is not found, create it and add it to the document
                if (customItemsComment == null)
                {
                    customItemsComment = xmlDoc.CreateComment(" Custom ");
                    rootNode.InsertBefore(customItemsComment, rootNode.FirstChild); // Insert at the top
                }

                // If the vanilla items comment is not found, create it and add it before the first vanilla item
                if (vanillaItemsComment == null)
                {
                    foreach (XmlNode node in rootNode.ChildNodes)
                    {
                        if (node.Name == "key")
                        {
                            vanillaItemsComment = xmlDoc.CreateComment(" Vanilla ");
                            rootNode.InsertBefore(vanillaItemsComment, node); // Insert before the first vanilla item
                            break;
                        }
                    }
                }

                // Create a new <key> element for the custom item
                XmlElement newKey = xmlDoc.CreateElement("key");

                // Set the id attribute
                newKey.SetAttribute("id", itemId);

                // Get the grade value based on the current rarity selected
                string selectedRarity = cbRarity.SelectedItem?.ToString();
                int grade = int.Parse(GetGradeFromRarity(selectedRarity));

                // Set the grade attribute
                newKey.SetAttribute("grade", grade.ToString());

                // Set the value attribute (hardcoded to 1 as per the request)
                newKey.SetAttribute("value", "1");

                // Insert the new custom item just after the custom marker and before the vanilla marker (if exists)
                if (vanillaItemsComment != null)
                {
                    // Insert the new key just before the vanilla comment
                    rootNode.InsertBefore(newKey, vanillaItemsComment);
                }
                else
                {
                    // Insert after custom items comment if vanilla comment doesn't exist
                    rootNode.InsertAfter(newKey, customItemsComment);
                }

                // Now, to ensure proper indentation and formatting when saving the file
                XmlWriterSettings settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "\t",  // Use tabs for indentation
                    NewLineOnAttributes = false  // Keep attributes on the same line
                };

                using (XmlWriter writer = XmlWriter.Create(webFinderFilePath, settings))
                {
                    xmlDoc.Save(writer);
                }

                MessageBox.Show("itemwebfinder.xml updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while updating itemwebfinder.xml: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetGradeFromRarity(string rarity)
        {
            // Convert the selected cbRarity value to the corresponding original value
            switch (rarity)
            {
                case "Normal":
                    return "1";
                case "Rare":
                    return "2";
                case "Exceptional":
                    return "3";
                case "Epic":
                    return "4";
                case "Legendary":
                    return "5";
                case "Ascendant":
                    return "6";
                default:
                    return "0";  // Default to 0 if the rarity is invalid or null
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            // Create an OpenFileDialog to select the image file
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp|All Files|*.*";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Load the selected image into a Bitmap object
                    Image originalImage = Image.FromFile(openFileDialog.FileName);

                    // Resize the image to fit inside pictureBox1 while maintaining aspect ratio
                    Image resizedImage = ResizeImageToFitPictureBox(originalImage, pictureBox1.Width, pictureBox1.Height);

                    // Display the resized image in pictureBox1
                    pictureBox1.Image = resizedImage;

                    // Store the loaded image in the loadedIcon variable for future saving
                    loadedIcon = resizedImage;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private Image ResizeImageToFitPictureBox(Image image, int targetWidth, int targetHeight)
        {
            // Calculate the aspect ratio of the image
            float aspectRatio = (float)image.Width / image.Height;

            int newWidth, newHeight;

            // Determine the new width and height while maintaining the aspect ratio
            if (targetWidth / (float)targetHeight > aspectRatio)
            {
                // If the PictureBox is wider than the image's aspect ratio, fit by height
                newHeight = targetHeight;
                newWidth = (int)(targetHeight * aspectRatio);
            }
            else
            {
                // Otherwise, fit by width
                newWidth = targetWidth;
                newHeight = (int)(targetWidth / aspectRatio);
            }

            // Create a new Bitmap with the calculated dimensions
            Bitmap resizedImage = new Bitmap(newWidth, newHeight);

            // Use Graphics to draw the resized image
            using (Graphics graphics = Graphics.FromImage(resizedImage))
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);
            }

            return resizedImage;
        }

        private void SaveImageFromPictureBox()
        {
            if (pictureBox1.Image != null)
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string saveDirectory = Path.Combine(baseDirectory, "!Extracted", "Image", "item", "icon");

                if (!Directory.Exists(saveDirectory))
                {
                    Directory.CreateDirectory(saveDirectory);
                }

                string saveFilePath = Path.Combine(saveDirectory, $"{txtItemId.Text}.png");

                try
                {
                    // Resize the image to 60x60 pixels before saving, since it's size of the icons in /Image/
                    Image resizedImage = ResizeImage(pictureBox1.Image, 60, 60);

                    // Save the resized image as a PNG file, same format as icons in /Image/
                    resizedImage.Save(saveFilePath, System.Drawing.Imaging.ImageFormat.Png);

                    MessageBox.Show($"Image saved successfully as {txtItemId.Text}.png", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("No image to save.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Image ResizeImage(Image imgToResize, int width, int height)
        {
            Bitmap resizedImage = new Bitmap(width, height);
            using (Graphics graphics = Graphics.FromImage(resizedImage))
            {
                // Set high-quality resizing settings
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // Draw the resized image
                graphics.DrawImage(imgToResize, 0, 0, width, height);
            }
            return resizedImage;
        }

        private void btnClear_Click_1(object sender, EventArgs e)
        {
            // Clear all textboxes and reset the PictureBox
            txtItemId.Clear();
            txtName.Clear();
            txtTipDescription.Clear();
            txtGuideDescription.Clear();
            txtScore.Clear();
            txtRandom.Clear();
            txtPreset.Clear();
            txtConstant.Clear();
            txtStatic.Clear();
            txtLevel.Clear();
            txtTitle.Clear();
            txtEffect.Clear();
            txtModel.Clear();
            cbClass.SelectedIndex = -1;
            cbRarity.SelectedIndex = -1;
            pictureBox1.Image = null;
            loadedIcon = null;
        }
    }
}