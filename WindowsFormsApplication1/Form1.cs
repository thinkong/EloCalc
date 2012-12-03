using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
//using System.Collections.Generic;

namespace PSRCalc
{
    public partial class Form1 : Form
    {
        //recipeDataGrid.DataSource = recipe;


        static Dictionary<string, Player> PlayerDics = new Dictionary<string, Player>();

        static Dictionary<string, Player> Players = new Dictionary<string, Player>();
        //  static List<CheckBox> cCheckBoxList = new List<CheckBox>();
        SQLiteDatabase cDB = new SQLiteDatabase("Dota.s3db");
        static bool bAnalyzed = false;
        DotaPSR cPSR;

        public Form1()
        {
            InitializeComponent();

            InitializeData();

            // add name check boxes
            int iCnt = 0;
            foreach (KeyValuePair<string, Player> pair in PlayerDics)
            {
                CheckBox cBox = new CheckBox();
                cBox.AutoSize = true;
                cBox.Location = new System.Drawing.Point(19 + (iCnt / 5) * 160, 17 + (iCnt % 5) * 22);
                cBox.Name = "checkBox" + iCnt.ToString();
                cBox.Size = new System.Drawing.Size(60, 16);
                cBox.TabIndex = 0;
                cBox.Text = pair.Value.Name;
                cBox.MouseHover += new EventHandler(cBox_MouseHover);
                cBox.UseVisualStyleBackColor = true;
                cBox.Checked = true;
                cBox.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);

                tabPage3.Controls.Add(cBox);
                ++iCnt;
                //     cCheckBoxList.Add(cBox);
            }
            if (Players.Count > 0)
            {
                foreach (GroupBox cb in groupBox1.Controls.OfType<GroupBox>())
                {
                    if (cb is GroupBox)
                    {
                        foreach (ComboBox combo in cb.Controls.OfType<ComboBox>())
                        {
                            combo.DataSource = new BindingSource(Players, null);
                            combo.DisplayMember = "Key";
                            combo.ValueMember = "Value";
                        }
                    }
                }
                foreach (GroupBox cb in groupBox7.Controls.OfType<GroupBox>())
                {
                    if (cb is GroupBox)
                    {
                        foreach (ComboBox combo in cb.Controls.OfType<ComboBox>())
                        {
                            combo.DataSource = new BindingSource(Players, null);
                            combo.DisplayMember = "Key";
                            combo.ValueMember = "Value";
                        }
                    }
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cBox = (ComboBox)sender;
            Control cParent = cBox.Parent;
            foreach (TextBox cTextBox in cParent.Controls.OfType<TextBox>())
            {
                try
                {
                    Player cObj = (Player)cBox.SelectedValue;
                    cTextBox.Text = cObj.PSR.ToString();
                }
                catch
                {
                    //Console.WriteLine(e.ToString());
                }

            }

            // now look for other combobox to switch
            foreach (GroupBox cb in groupBox1.Controls.OfType<GroupBox>())
            {
                if (cb is GroupBox)
                {
                    if (cb.Enabled)
                    {
                        foreach (ComboBox combo in cb.Controls.OfType<ComboBox>().Reverse())
                        {
                            if (combo.SelectedValue == null) continue;
                            Player cP = (Player)combo.SelectedValue;
                            if (cP.Name == "") continue;

                        }
                    }
                }
            }
        }

        private void cBox_MouseHover(object sender, EventArgs e)
        {
            Player cP;
            CheckBox cb = (CheckBox)sender;
            PlayerDics.TryGetValue(cb.Text, out cP);
            textBox10.Text = cP.RenderInfo();
        }

        private void InitializeData()
        {
            DataTable cdbplayers;
            String query = "select pname, psr, uid, win, lose from players";
            cdbplayers = cDB.GetDataTable(query);
            foreach (DataRow r in cdbplayers.Rows)
            {
                Player cPlayer = new Player(r["pname"].ToString(), Convert.ToUInt32(r["psr"]), Convert.ToInt32(r["uid"]));
                cPlayer.iLose = Convert.ToInt32(r["lose"]);
                cPlayer.iLose = Convert.ToInt32(r["win"]);
                PlayerDics.Add(r["pname"].ToString(), cPlayer);
            }
            // un comment this when db populating is needed
            //PopulateDB();
        }

        private void PopulateDB()
        {

            PlayerDics.Add("nokdu", new Player("nokdu", 1500));         //wlw
            PlayerDics.Add("hyuk", new Player("hyuk", 1500));           //lwl
            PlayerDics.Add("bublapse", new Player("bublapse", 1500));   //wlw
            PlayerDics.Add("wolfmad", new Player("wolfmad", 1500));     //wlw
            PlayerDics.Add("dooly", new Player("dooly", 1500));         //lwl
            PlayerDics.Add("xxxmind", new Player("xxxmind", 1500));     //wlw
            PlayerDics.Add("hellkite0", new Player("hellkite0", 1500)); //lwl
            PlayerDics.Add("soon", new Player("soon", 1500));           //lwl
            PlayerDics.Add("rejang", new Player("rejang", 1500));       //wlw
            PlayerDics.Add("cid", new Player("cid", 1500));             //lwl

            foreach (KeyValuePair<string, Player> p in PlayerDics)
            {
                Dictionary<string, string> sInsertData = new Dictionary<string, string>();
                sInsertData.Add("pname", p.Value.Name);
                sInsertData.Add("psr", p.Value.PSR.ToString());
                cDB.Insert("players", sInsertData);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Dictionary<string, Player> cParticipatingPlayers1 = new Dictionary<string, Player>();
            Dictionary<string, Player> cParticipatingPlayers2 = new Dictionary<string, Player>();
            Dictionary<string, Player> cParticipatingAllPlayer = new Dictionary<string, Player>();
            double iTotal = 0;

            foreach (GroupBox cb in groupBox1.Controls.OfType<GroupBox>())
            {
                if (cb is GroupBox)
                {
                    if (cb.Enabled)
                    {
                        foreach (ComboBox combo in cb.Controls.OfType<ComboBox>())
                        {
                            if (combo.SelectedValue == null) continue;
                            Player cP = (Player)combo.SelectedValue;
                            if (cP.Name == "") continue;
                            if (cParticipatingPlayers1.ContainsKey(cP.Name))
                            {
                                string sMessage = cP.Name + "은 이미 이 팀에 있습니다...";
                                MessageBox.Show(sMessage);
                                continue;
                            }
                            if (cParticipatingAllPlayer.ContainsKey(cP.Name))
                            {
                                string sMessage = cP.Name + "은 이미 이 상대팀에 있습니다...";
                                MessageBox.Show(sMessage);
                                continue;
                            }
                            cParticipatingPlayers1.Add(cP.Name, cP);
                            cParticipatingAllPlayer.Add(cP.Name, cP);
                            iTotal += cP.PSR;
                        }
                    }
                }
            }
            foreach (GroupBox cb in groupBox7.Controls.OfType<GroupBox>())
            {
                if (cb is GroupBox)
                {
                    if (cb.Enabled)
                    {
                        foreach (ComboBox combo in cb.Controls.OfType<ComboBox>())
                        {
                            if (combo.SelectedValue == null) continue;
                            Player cP = (Player)combo.SelectedValue;
                            if (cP.Name == "") continue;
                            if (cParticipatingPlayers2.ContainsKey(cP.Name))
                            {
                                string sMessage = cP.Name + "은 이미 이 팀에 있습니다...";
                                MessageBox.Show(sMessage);
                                continue;
                            }
                            if (cParticipatingAllPlayer.ContainsKey(cP.Name))
                            {
                                string sMessage = cP.Name + "은 이미 이 상대팀에 있습니다...";
                                MessageBox.Show(sMessage);
                                continue;
                            }
                            cParticipatingPlayers2.Add(cP.Name, cP);
                            cParticipatingAllPlayer.Add(cP.Name, cP);
                            iTotal += cP.PSR;
                        }
                    }
                }
            }

            // Balance here..

            string sToAppend = "Total: " + iTotal.ToString();
            AppendStringTab1(sToAppend);
            cPSR = new DotaPSR(cParticipatingPlayers1.Values.ToList(), cParticipatingPlayers2.Values.ToList());
            AppendStringTab1(cPSR.sDebugString);

            foreach (GroupBox cb in groupBox1.Controls.OfType<GroupBox>().Reverse())
            {
                groupBox1.Text = "team 1 : " + cPSR.iTeam1WinPerc.ToString();
                if (cb is GroupBox)
                {
                    if (cb.Enabled)
                    {
                        foreach (ComboBox combo in cb.Controls.OfType<ComboBox>())
                        {
                            if (combo.SelectedIndex == 0)
                            {
                                cb.Text = "No Player";
                                continue;
                            }
                            DotaPlayerPSR cPlayer;
                            cPSR.cWinLoseDic.TryGetValue(combo.Text, out cPlayer);
                            cb.Text = cPlayer.Name + ":" + cPlayer.fWinPoint + "/" + cPlayer.fLosePoint;
                        }
                    }
                }
            }
            foreach (GroupBox cb in groupBox7.Controls.OfType<GroupBox>().Reverse())
            {
                groupBox7.Text = "team 2 : " + cPSR.iTeam2WinPerc.ToString();
                if (cb is GroupBox)
                {
                    if (cb.Enabled)
                    {
                        foreach (ComboBox combo in cb.Controls.OfType<ComboBox>())
                        {
                            if (combo.SelectedIndex == 0)
                            {
                                cb.Text = "No Player";
                                continue;
                            }
                            DotaPlayerPSR cPlayer;
                            cPSR.cWinLoseDic.TryGetValue(combo.Text, out cPlayer);
                            cb.Text = cPlayer.Name + ":" + cPlayer.fWinPoint + "/" + cPlayer.fLosePoint;
                        }
                    }
                }
            }

            bAnalyzed = true;
        }

        private void AppendStringTab1(string s)
        {
            textBox11.AppendText(s);
            textBox11.AppendText(Environment.NewLine);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            bAnalyzed = false;
            Players.Clear();
            Players.Add("", new Player("", 0));
            foreach (CheckBox c in tabPage3.Controls.OfType<CheckBox>())
            {
                if (c.Checked)
                {
                    Player cP;
                    PlayerDics.TryGetValue(c.Text, out cP);
                    Players.Add(cP.Name, cP);
                }
            }
            if (Players.Count > 0)
            {
                int iGBoxCnt = 0;
                foreach (GroupBox cb in groupBox1.Controls.OfType<GroupBox>().Reverse())
                {
                    if (cb is GroupBox)
                    {
                        if (iGBoxCnt < Players.Count - 2)
                        {
                            cb.Enabled = true;
                            foreach (ComboBox combo in cb.Controls.OfType<ComboBox>())
                            {
                                combo.DataSource = null;
                                combo.DisplayMember = "Key";
                                combo.ValueMember = "Value";
                                combo.DataSource = new BindingSource(Players, null);
                                int iNewIndex = iGBoxCnt* 2 + 1;
                                if (iNewIndex >= Players.Count) iNewIndex = 0;
                                combo.SelectedIndex = iNewIndex;
                            }
                        }
                        else
                        {
                            cb.Enabled = false;
                            foreach (ComboBox combo in cb.Controls.OfType<ComboBox>())
                            {
                                combo.DataSource = null;
                            }
                        }
                        ++iGBoxCnt;
                    }
                }
                iGBoxCnt = 0;
                foreach (GroupBox cb in groupBox7.Controls.OfType<GroupBox>().Reverse())
                {
                    if (cb is GroupBox)
                    {
                        if (iGBoxCnt < Players.Count - 2)
                        {
                            cb.Enabled = true;
                            foreach (ComboBox combo in cb.Controls.OfType<ComboBox>())
                            {
                                combo.DataSource = null;
                                combo.DisplayMember = "Key";
                                combo.ValueMember = "Value";
                                combo.DataSource = new BindingSource(Players, null);
                                int iNewIndex = (iGBoxCnt + 1) * 2;
                                if (iNewIndex >= Players.Count) iNewIndex = 0;
                                combo.SelectedIndex = iNewIndex;
                            }
                        }
                        else
                        {
                            cb.Enabled = false;
                            foreach (ComboBox combo in cb.Controls.OfType<ComboBox>())
                            {
                                combo.DataSource = null;
                            }
                        }
                        ++iGBoxCnt;
                    }
                }
            }
            groupBox1.Enabled = true;
            groupBox7.Enabled = true;
            groupBox1.Refresh();
            groupBox7.Refresh();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            bAnalyzed = false;
            Players.Clear();
            //foreach (CheckBox c in tabPage3.Controls.OfType<CheckBox>())
            //{
            //    if (c.Checked)
            //    {
            //        Player cP;
            //        PlayerDics.TryGetValue(c.Text, out cP);
            //        Players.Add(cP.Name, cP);
            //    }
            //}
            if (Players.Count > 0)
            {
                int iGBoxCnt = 0;
                foreach (GroupBox cb in groupBox1.Controls.OfType<GroupBox>())
                {
                    if (cb is GroupBox)
                    {
                        if (iGBoxCnt < Players.Count)
                        {
                            cb.Enabled = false;
                        }
                        else cb.Enabled = true;
                        foreach (ComboBox combo in cb.Controls.OfType<ComboBox>())
                        {
                            combo.DataSource = new BindingSource(Players, null);
                            combo.DisplayMember = "Key";
                            combo.ValueMember = "Value";
                        }
                        ++iGBoxCnt;
                    }
                }
                iGBoxCnt = 0;
                foreach (GroupBox cb in groupBox7.Controls.OfType<GroupBox>())
                {
                    if (cb is GroupBox)
                    {
                        if (iGBoxCnt > Players.Count)
                        {
                            cb.Enabled = true;
                        }
                        else cb.Enabled = false;
                        foreach (ComboBox combo in cb.Controls.OfType<ComboBox>())
                        {
                            combo.DataSource = new BindingSource(Players, null);
                            combo.DisplayMember = "Key";
                            combo.ValueMember = "Value";
                        }
                        ++iGBoxCnt;
                    }
                }
            }
            groupBox1.Refresh();
            groupBox7.Refresh();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (!bAnalyzed)
            {
                MessageBox.Show("Team not analyzed yet");
                return;
            }
            string swinner = "1";
            string sloser = "2";
            // team1 wins
            InsertMatchInfo(swinner, sloser, 1);
            bAnalyzed = false;

        }

        private void InsertMatchInfo(string swinner, string sloser, int iWinningTeam)
        {
            Guid iGUID = Guid.NewGuid();    // new match id
            // insert the match first
            Dictionary<string, string> cMatchinfo = new Dictionary<string, string>();
            cMatchinfo.Add("UID", iGUID.ToString());
            cMatchinfo.Add("winner", swinner);
            cDB.Insert("match", cMatchinfo);

            // insert the match details
            foreach (GroupBox cb in groupBox1.Controls.OfType<GroupBox>())
            {
                if (cb is GroupBox)
                {
                    if (cb.Enabled)
                    {
                        InsertMatchPlayerInfo(swinner, iGUID, cb, iWinningTeam == 1);
                    }
                }
            }
            foreach (GroupBox cb in groupBox7.Controls.OfType<GroupBox>())
            {
                if (cb is GroupBox)
                {
                    if (cb.Enabled)
                    {
                        InsertMatchPlayerInfo(sloser, iGUID, cb, iWinningTeam == 2);
                    }
                }
            }
        }

        private void InsertMatchPlayerInfo(string swinner, Guid iGUID, GroupBox cb, bool bWin)
        {
            foreach (ComboBox combo in cb.Controls.OfType<ComboBox>())
            {
                if (combo.SelectedValue == null) continue;
                Player cP = (Player)combo.SelectedValue;
                if (cP.Name == "") continue;
                Dictionary<string, string> cMatchPlayerInfo = new Dictionary<string, string>();
                cMatchPlayerInfo.Add("matchid", iGUID.ToString());
                cMatchPlayerInfo.Add("playerid", cP.iUID.ToString());
                cMatchPlayerInfo.Add("team", swinner);
                cDB.Insert("match_info", cMatchPlayerInfo);

                // update player info
                if (bWin)
                {
                    ++cP.iWin;
                    cP.PSR += cPSR.cWinLoseDic[cP.Name].fWinPoint;
                }
                else
                {
                    ++cP.iLose;
                    cP.PSR += cPSR.cWinLoseDic[cP.Name].fLosePoint;
                }
                Dictionary<string, string> cPlayerUpdate = new Dictionary<string,string>();
                cPlayerUpdate.Add("win", cP.iWin.ToString());
                cPlayerUpdate.Add("lose", cP.iLose.ToString());
                cPlayerUpdate.Add("psr", cP.PSR.ToString());
                cDB.Update("players", cPlayerUpdate, string.Format("UID = {0}", cP.iUID));
                comboBox1_SelectedIndexChanged(combo, EventArgs.Empty);


            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (!bAnalyzed)
            {
                MessageBox.Show("Team not analyzed yet");
                return;
            }
            string swinner = "2";
            string sloser = "1";
            InsertMatchInfo(swinner, sloser, 2);
            bAnalyzed = false;
        }
    }

    public class Player
    {
        public string Name { get; set; }
        public double PSR { get; set; }
        public int iUID { get; set; }
        public int iWin { get; set; }
        public int iLose { get; set; }
        public Player(string _name)
        {
            Name = _name;
            PSR = 1500;
            iUID = 0;
            iWin = 0;
            iLose = 0;
        }
        public Player(string _name, uint _i)
        {
            Name = _name;
            PSR = _i;
            iUID = 0;
            iWin = 0;
            iLose = 0;
        }
        public Player(string _name, uint _i, int _iUID)
        {
            Name = _name;
            PSR = _i;
            iUID = _iUID;
            iWin = 0;
            iLose = 0;
        }

        public string RenderInfo()
        {
            return Name + "(" + PSR.ToString() + ")";
        }
    }

    
}
