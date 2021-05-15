﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using PBL3REAL.BLL;
using PBL3REAL.DAL;
using PBL3REAL.ViewModel;
namespace PBL3REAL.View
{
    public partial class Form_Switch_Role : Form
    {
        private QLUserBLL qLUserBLL;
        public Form_Switch_Role()
        {
            InitializeComponent();
            qLUserBLL = new QLUserBLL();
            loadData();
            addToCbb();
        }
        //Load Data
        private void loadData()
        {
            //Solution: (https://)stackoverflow.com/questions/17193825/loading-picturebox-image-from-resource-file-with-path-part-3
            picbx_Header.Image = Image.FromFile(@QLUserBLL.stoUser.ListImg[0].ImgstoUrl);
            picbx_Header.SizeMode = PictureBoxSizeMode.StretchImage;
            lb_Header.Text = QLUserBLL.stoUser.UserCode;
        }
        private void addToCbb()
        {
            cbb_LoginRole.DataSource = qLUserBLL.getRoleForUser();
            cbb_LoginRole.DisplayMember = "RoleName";
        }
        //Events
        private void btn_Login_Click(object sender, EventArgs e)
        {
            string role = ((RoleVM)cbb_LoginRole.SelectedItem).RoleName;
            if (role != null)
            {
                foreach (RoleVM roleVM in QLUserBLL.stoUser.ListRole)
                {
                    if (roleVM.RoleName.Equals(role))
                    {
                        Form_Home_Admin f = new Form_Home_Admin();
                        this.Hide();
                        f.ShowDialog();
                        this.Show();
                        break;
                    }
                }
            }
        }
        private void btn_Logout_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}
