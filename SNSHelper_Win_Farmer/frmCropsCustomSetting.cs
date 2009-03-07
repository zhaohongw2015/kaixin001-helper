using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using SNSHelper.Kaixin001.Entity.Garden;
using SNSHelper_Win_Garden.Entity;

namespace SNSHelper_Win_Garden
{
    public partial class frmCropsCustomSetting : DevComponents.DotNetBar.Office2007Form
    {
        public frmCropsCustomSetting()
        {
            InitializeComponent();
        }

        private void frmCropsCustomSetting_Load(object sender, EventArgs e)
        {
            string xml = File.ReadAllText(Path.Combine(Application.StartupPath, "SeedData.xml"));
            xml = xml.Replace("<?xml version=\"1.0\" encoding=\"utf-8\" ?>", "");

            SeedData seedData = new SeedData(xml);
            BindSeedCombox(seedData);

            ShowSeedMVPInCombox();
        }

        private void BindSeedCombox(SeedData seedData)
        {
            foreach (SeedItem item in seedData.SeedItems)
            {
                ckxCrops1.Items.Add(item.Name);
                ckxCrops1.SelectedIndex = 0;

                ckxCrops2.Items.Add(item.Name);
                ckxCrops2.SelectedIndex = 0;

                ckxCrops3.Items.Add(item.Name);
                ckxCrops3.SelectedIndex = 0;

                ckxCrops4.Items.Add(item.Name);
                ckxCrops4.SelectedIndex = 0;

                ckxCrops5.Items.Add(item.Name);
                ckxCrops5.SelectedIndex = 0;

                ckxCrops6.Items.Add(item.Name);
                ckxCrops6.SelectedIndex = 0;

                ckxCrops7.Items.Add(item.Name);
                ckxCrops7.SelectedIndex = 0;

                ckxCrops8.Items.Add(item.Name);
                ckxCrops8.SelectedIndex = 0;

                ckxCrops9.Items.Add(item.Name);
                ckxCrops9.SelectedIndex = 0;

                ckxCrops10.Items.Add(item.Name);
                ckxCrops10.SelectedIndex = 0;

                ckxCrops11.Items.Add(item.Name);
                ckxCrops11.SelectedIndex = 0;

                ckxCrops12.Items.Add(item.Name);
                ckxCrops12.SelectedIndex = 0;

                ckxCrops13.Items.Add(item.Name);
                ckxCrops13.SelectedIndex = 0;

                ckxCrops14.Items.Add(item.Name);
                ckxCrops14.SelectedIndex = 0;

                ckxCrops15.Items.Add(item.Name);
                ckxCrops15.SelectedIndex = 0;
            }
        }

        private void ShowSeedMVPInCombox()
        {
            foreach (SeedMVP item in SeedMVPs.SeedMVPList)
            {
                switch (item.Rank)
                {
                    case "1":
                        ckxCrops1.Text = item.SeedName;
                        break;
                    case "2":
                        ckxCrops2.Text = item.SeedName;
                        break;
                    case "3":
                        ckxCrops3.Text = item.SeedName;
                        break;
                    case "4":
                        ckxCrops4.Text = item.SeedName;
                        break;
                    case "5":
                        ckxCrops5.Text = item.SeedName;
                        break;
                    case "6":
                        ckxCrops6.Text = item.SeedName;
                        break;
                    case "7":
                        ckxCrops7.Text = item.SeedName;
                        break;
                    case "8":
                        ckxCrops8.Text = item.SeedName;
                        break;
                    case "9":
                        ckxCrops9.Text = item.SeedName;
                        break;
                    case "10":
                        ckxCrops10.Text = item.SeedName;
                        break;
                    case "11":
                        ckxCrops11.Text = item.SeedName;
                        break;
                    case "12":
                        ckxCrops12.Text = item.SeedName;
                        break;
                    case "13":
                        ckxCrops13.Text = item.SeedName;
                        break;
                    case "14":
                        ckxCrops14.Text = item.SeedName;
                        break;
                    case "15":
                        ckxCrops15.Text = item.SeedName;
                        break;
                    default:
                        break;
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SeedMVPs.SetSeedMVP("1", ckxCrops1.Text);
            SeedMVPs.SetSeedMVP("2", ckxCrops2.Text);
            SeedMVPs.SetSeedMVP("3", ckxCrops3.Text);
            SeedMVPs.SetSeedMVP("4", ckxCrops4.Text);
            SeedMVPs.SetSeedMVP("5", ckxCrops5.Text);
            SeedMVPs.SetSeedMVP("6", ckxCrops6.Text);
            SeedMVPs.SetSeedMVP("7", ckxCrops7.Text);
            SeedMVPs.SetSeedMVP("8", ckxCrops8.Text);
            SeedMVPs.SetSeedMVP("9", ckxCrops9.Text);
            SeedMVPs.SetSeedMVP("10", ckxCrops10.Text);
            SeedMVPs.SetSeedMVP("11", ckxCrops11.Text);
            SeedMVPs.SetSeedMVP("12", ckxCrops12.Text);
            SeedMVPs.SetSeedMVP("13", ckxCrops13.Text);
            SeedMVPs.SetSeedMVP("14", ckxCrops14.Text);
            SeedMVPs.SetSeedMVP("15", ckxCrops15.Text);

            if (SeedMVPs.SaveSeedMVPs())
            {
                DevComponents.DotNetBar.MessageBoxEx.Show(this, "保存成功！", "提示");
                this.Close();
            }
            else
            {
                DevComponents.DotNetBar.MessageBoxEx.Show(this, "保存失败！", "提示");
            }
        }
    }
}
