using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NHSE.Core;

namespace NHSE.WinForms
{
    public sealed class DreamEditor : Form
    {
        private readonly HorizonSave CurrentSave;
        private readonly CheckBox[] CBs;
        private TextBox TB_TargetPath = null!;
        private Button B_BrowseTarget = null!;
        private Label L_DreamCopyLabel = null!;
        private Button B_DreamCopy = null!;

        private const string DisplayName = "梦境复刻";

        public DreamEditor(HorizonSave current)
        {
            CurrentSave = current;
            var items = new (string Label, string Tag, bool Default)[]
            {
                ("地图一层物品", "MapLayer0", true),
                ("地图二层物品", "MapLayer1", true),
                ("建筑", "Buildings", true),
                ("广场坐标", "Plaza", true),
                ("地皮", "Acre", true),
                ("耕地", "Terrain", true),
                ("设计", "Designs", true),
                ("专业设计", "PRODesigns", true),
                ("所有玩家房屋", "PlayerHouses", false),
                ("所有玩家属性", "PlayerProperties", false),
                ("裁缝设计", "TailorDesigns", false),
                ("地图属性", "MapProperties", false),
                ("半球", "Hemisphere", false),
                ("机场颜色", "AirportColor", false),
                ("气象种子", "WeatherSeed", false),
                ("岛屿旗帜", "IslandFlag", false),
                ("所有小动物", "AllVillagers", false),
                ("博物馆", "Museum", false),
                ("水果", "FruitFlower", false),
                ("移除信箱", "RemoveMailbox", false),
            };

            CBs = new CheckBox[items.Length];
            InitializeComponent(items);
        }

        private void InitializeComponent((string Label, string Tag, bool Default)[] items)
        {
            int y = 70;
            int col = 0;
            int colWidth = 230;
            int col2X = 250;
            for (int i = 0; i < items.Length; i++)
            {
                int x = col == 0 ? 10 : col2X;
                col++;
                if (col >= 2)
                {
                    col = 0;
                    y += 25;
                }
            }
            y += 25;
            int formHeight = y + 145;

            ClientSize = new Size(500, formHeight);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            StartPosition = FormStartPosition.CenterParent;
            Text = DisplayName;
            MaximizeBox = false;
            MinimizeBox = false;

            var lbl = new Label { Location = new Point(10, 10), Size = new Size(480, 20), Text = "目标存档文件夹或main.dat文件:" };
            TB_TargetPath = new TextBox { Location = new Point(10, 35), Size = new Size(410, 20) };
            B_BrowseTarget = new Button { Location = new Point(425, 34), Size = new Size(60, 22), Text = "浏览" };
            B_BrowseTarget.Click += B_BrowseTarget_Click;

            y = 70;
            col = 0;
            for (int i = 0; i < items.Length; i++)
            {
                int x = col == 0 ? 10 : col2X;
                var cb = new CheckBox
                {
                    Location = new Point(x, y),
                    Size = new Size(colWidth, 20),
                    Text = items[i].Label,
                    Tag = items[i].Tag,
                    Checked = items[i].Default
                };
                CBs[i] = cb;
                Controls.Add(cb);
                col++;
                if (col >= 2)
                {
                    col = 0;
                    y += 25;
                }
            }
            y += 25;

            L_DreamCopyLabel = new Label { Location = new Point(10, y + 15), Size = new Size(480, 40), Text = "该软件免费发布，并不保证软件的可用性，可能会有bug，不包更新，有需要请联系侃总购买成熟的软件，侃总联系方式:1455682411—by念若安止", AutoSize = false };
            B_DreamCopy = new Button { Location = new Point(10, y + 70), Size = new Size(480, 30), Text = "复刻" };
            B_DreamCopy.Click += B_DreamCopy_Click;

            Controls.AddRange(new Control[] { lbl, TB_TargetPath, B_BrowseTarget, L_DreamCopyLabel, B_DreamCopy });
        }

        private void B_BrowseTarget_Click(object? sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "选择目标存档";
            dialog.Filter = "所有支持格式|main.dat;*|文件夹|*|main.dat文件|main.dat";
            dialog.FileName = "main.dat";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var path = dialog.FileName;
                if (File.Exists(path))
                {
                    var dir = Path.GetDirectoryName(path);
                    if (dir != null)
                        TB_TargetPath.Text = dir;
                }
                else if (Directory.Exists(path))
                {
                    TB_TargetPath.Text = path;
                }
            }
        }

        private void B_DreamCopy_Click(object? sender, EventArgs e)
        {
            var targetPath = TB_TargetPath.Text;
            if (string.IsNullOrEmpty(targetPath))
            {
                MessageBox.Show("请选择目标存档", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                HorizonSave targetSave;
                if (File.Exists(targetPath))
                {
                    var dir = Path.GetDirectoryName(targetPath);
                    if (dir != null)
                        targetSave = HorizonSave.FromFolder(dir);
                    else
                    {
                        MessageBox.Show("无效的路径", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                else if (Directory.Exists(targetPath))
                {
                    targetSave = HorizonSave.FromFolder(targetPath);
                }
                else
                {
                    MessageBox.Show("无效的路径", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                DoCopy(targetSave);
                MessageBox.Show("梦境复刻完成！请手动保存", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载目标存档失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DoCopy(HorizonSave target)
        {
            var current = CurrentSave;
            var main = current.Main;
            var targetMain = target.Main;

            foreach (var cb in CBs)
            {
                if (!cb.Checked) continue;
                var tag = cb.Tag?.ToString() ?? "";

                switch (tag)
                {
                    case "MapLayer0":
                        main.SetFieldItemLayer0(targetMain.GetFieldItemLayer0());
                        break;
                    case "MapLayer1":
                        main.SetFieldItemLayer1(targetMain.GetFieldItemLayer1());
                        break;
                    case "Buildings":
                        main.Buildings = targetMain.Buildings;
                        break;
                    case "Plaza":
                        main.EventPlazaLeftUpX = targetMain.EventPlazaLeftUpX;
                        main.EventPlazaLeftUpZ = targetMain.EventPlazaLeftUpZ;
                        main.MainFieldParamUniqueID = targetMain.MainFieldParamUniqueID;
                        break;
                    case "Acre":
                        main.SetAcreBytes(targetMain.GetAcreBytes());
                        break;
                    case "Terrain":
                        main.SetTerrainTiles(targetMain.GetTerrainTiles());
                        break;
                    case "Designs":
                        main.SetDesigns(targetMain.GetDesigns());
                        break;
                    case "PRODesigns":
                        main.SetDesignsPRO(targetMain.GetDesignsPRO());
                        break;
                    case "PlayerHouses":
                        main.SetPlayerHouses(targetMain.GetPlayerHouses());
                        break;
                    case "PlayerProperties":
                        for (int i = 0; i < current.Players.Count && i < target.Players.Count; i++)
                        {
                            var cp = current.Players[i].Personal;
                            var tp = target.Players[i].Personal;
                            cp.Bank = tp.Bank;
                            cp.Wallet = tp.Wallet;
                            cp.NookMiles = tp.NookMiles;
                            cp.TotalNookMiles = tp.TotalNookMiles;
                            cp.PocketCount = tp.PocketCount;
                            cp.BagCount = tp.BagCount;
                            cp.ItemChestCount = tp.ItemChestCount;
                            cp.ProfileFruit = tp.ProfileFruit;
                        }
                        break;
                    case "TailorDesigns":
                        main.SetDesignsTailor(targetMain.GetDesignsTailor());
                        break;
                    case "MapProperties":
                        break;
                    case "Hemisphere":
                        main.Hemisphere = (Hemisphere)(byte)targetMain.Hemisphere;
                        UpdateHemisphereUI(main);
                        break;
                    case "AirportColor":
                        main.AirportThemeColor = (AirportColor)(byte)targetMain.AirportThemeColor;
                        UpdateAirportColorUI(main);
                        break;
                    case "WeatherSeed":
                        main.WeatherSeed = targetMain.WeatherSeed;
                        UpdateWeatherSeedUI(main);
                        break;
                    case "IslandFlag":
                        main.FlagMyDesign = targetMain.FlagMyDesign;
                        break;
                    case "AllVillagers":
                        main.SetVillagers(targetMain.GetVillagers());
                        main.SetVillagerHouses(targetMain.GetVillagerHouses());
                        UpdateVillagersUI(target);
                        break;
                    case "Museum":
                        main.Museum = targetMain.Museum;
                        break;
                    case "FruitFlower":
                        main.SpecialtyFruit = targetMain.SpecialtyFruit;
                        main.SisterFruit = targetMain.SisterFruit;
                        main.SpecialtyFlower = targetMain.SpecialtyFlower;
                        main.SisterFlower = targetMain.SisterFlower;
                        break;
                    case "RemoveMailbox":
                        for (int i = 0; i < current.Players.Count; i++)
                        {
                            current.Players[i].PostBox.Clear();
                        }
                        break;
                }
            }
        }

        private void UpdateHemisphereUI(MainSave m)
        {
            var mainForm = Application.OpenForms.OfType<Editor>().FirstOrDefault();
            if (mainForm != null)
            {
                var hemisphereField = typeof(Editor).GetField("CB_Hemisphere", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var cbHemisphere = hemisphereField?.GetValue(mainForm) as ComboBox;
                if (cbHemisphere != null) cbHemisphere.SelectedIndex = (int)m.Hemisphere;
            }
        }

        private void UpdateAirportColorUI(MainSave m)
        {
            var mainForm = Application.OpenForms.OfType<Editor>().FirstOrDefault();
            if (mainForm != null)
            {
                var field = typeof(Editor).GetField("CB_AirportColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var cbAirport = field?.GetValue(mainForm) as ComboBox;
                if (cbAirport != null) cbAirport.SelectedIndex = (int)m.AirportThemeColor;
            }
        }

        private void UpdateWeatherSeedUI(MainSave m)
        {
            var mainForm = Application.OpenForms.OfType<Editor>().FirstOrDefault();
            if (mainForm != null)
            {
                var weatherField = typeof(Editor).GetField("NUD_WeatherSeed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var nudWeather = weatherField?.GetValue(mainForm) as NumericUpDown;
                if (nudWeather != null) nudWeather.Value = m.WeatherSeed;
            }
        }

        private void UpdateVillagersUI(HorizonSave target)
        {
            var mainForm = Application.OpenForms.OfType<Editor>().FirstOrDefault();
            if (mainForm != null)
            {
                var villagersField = typeof(Editor).GetField("Villagers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var villagersEditor = villagersField?.GetValue(mainForm) as VillagerEditor;
                if (villagersEditor != null)
                {
                    villagersEditor.Villagers = target.Main.GetVillagers();
                    villagersEditor.Reload();
                }
            }
        }
    }
}

