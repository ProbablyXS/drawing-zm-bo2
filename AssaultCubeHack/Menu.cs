using RL.Properties;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VECRPROJECT.util;

namespace AssaultCubeHack
{
    public partial class Menu : Form
    {

        private Random rdm = new Random();

        private Clsini INIConfig = new Clsini("profiles/" + Settings.Default.Profiles + "/config.ini");
        private Clsini INIBestProfiles = new Clsini("profiles/startup.ini");
        private bool started = false;
        private System.Drawing.ColorConverter ConvertColor = new ColorConverter();

        [DllImport("User32.dll")]
        public static extern short GetAsyncKeyState(Keys vKey); // Keys enumeration

        //threads for updating rendering
        private Thread windowPosThread;




        // 'MOUVE FORM WITH MOUSE'
        private bool drag;
        private int mousex;
        private int mousey;
        public object WantApplicationExit = false;
        public int access = 0;

        private void Panel1_MouseDown(object sender, MouseEventArgs e)
        {
            drag = true;
            mousex = Cursor.Position.X - this.Left;
            mousey = Cursor.Position.Y - this.Top;
        }

        private void Panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (drag)
            {
                this.Top = Cursor.Position.Y - mousey;
                this.Left = Cursor.Position.X - mousex;
            }
        }

        private void Panel1_MouseUp(object sender, MouseEventArgs e)
        {
            drag = false;
        }
        // MOUVE FORM WITH MOUSE'




        public Menu()
        {
            InitializeComponent();
        }

        private void AssaultHack_Load(object sender, EventArgs e)
        {

            Security.CHANGENAME();

            //LOAD PARAMETERS
            InitializationCONTROLS();

            //try to attach to game
            AttachToGameProcess();

            Examples.Example.Menu_Showed = true;
            Examples.Example.Menu_Loading = false;

            //Show topmost first page
            this.Activate();
        }

        private void InitializeOverlayWindowAttributes()
        {
            TopMost = true; //set window on top of all others
            FormBorderStyle = FormBorderStyle.None; //remove form controls
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            DoubleBuffered = true;
            ACB.UseAnimation = true;
            ACB.SelectedTextColor = Color.White;
            ACB.Font = new Font("Arial Black", 8, FontStyle.Bold);
        }

        public async Task refreshControllerIsConnectedAsync()
        {

            while (true)
            {
                await Task.Delay(1000);

                if (Examples.Example.controller.Connection.Value == true)
                {
                    label109.Text = "ON";
                    label109.ForeColor = Color.FromArgb(0, 192, 0);
                }
                else
                {
                    label109.Text = "OFF";
                    label109.ForeColor = Settings.Default.MenuBorderColor;
                }
            }
        }

        public void AttachToGameProcess()
        {
            bool success = false;
            do
            {
                //check if game is running
                if (Memory.GetProcessesByName(Examples.Example.processName, out Examples.Example.process))
                {
                    try
                    {
                        //success  
                        IntPtr handle = Memory.OpenProcess(Examples.Example.process.Id);
                        if (handle != IntPtr.Zero)
                        {
                            success = true;
                        }
                        else
                        {
                            Security.FAILACC();
                        }
                    }
                    catch (Exception)
                    {
                        Security.FAILACC();
                    }
                }
                else
                {
                    Security.FAILACC();
                }
            } while (!success);

            InitializeOverlayWindowAttributes();
            StartThreads();
        }

        public void resetConfigFile()
        {
            File.WriteAllBytes("profiles/" + Settings.Default.Profiles + "/config.ini", Resources.config);
            MessageBox.Show("ERROR: 8, Resetting config.ini", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            InitializationCONTROLS();
        }

        public void InitializationCONTROLS()
        {
            //--------PAGE--------
            ACB.SelectTab(0);

            AIMBOTpage.BaseColor = Color.FromArgb(30, 30, 30);
            MISCpage.BaseColor = Color.FromArgb(30, 30, 30);

            metroSetComboBox1.ForeColor = Settings.Default.NormalColor;
            metroSetComboBox1.BorderColor = Settings.Default.NormalColor;


            //HOME

            //REFRESH CONTROLLER INFORMATION
            refreshControllerIsConnectedAsync();

            label11.Text = "Welcome: " + Environment.MachineName;
            label100.Text = "Expires: " + Security.infoDate;
            label13.Text = "Current version: " + Application.ProductVersion;
            //END --------PAGE--------


            //CHECK INI FILE EXIST
            try
            {

                if (File.Exists("profiles/startup.ini") == true)
                {
                    Settings.Default.Profiles = INIBestProfiles.Read("Best", "STARTUP");
                }
                else
                {
                    Directory.CreateDirectory("profiles/" + Settings.Default.Profiles);
                    File.WriteAllBytes("profiles/startup.ini", Resources.startup);
                }

                if (File.Exists("profiles/" + Settings.Default.Profiles + "/config.ini") == false)
                {
                    Directory.CreateDirectory("profiles/" + Settings.Default.Profiles);
                    File.WriteAllBytes("profiles/" + Settings.Default.Profiles + "/config.ini", Resources.config);
                }

                INIConfig = new Clsini("profiles/" + Settings.Default.Profiles + "/config.ini"); //get new config

                KeysConverter kc = new KeysConverter(); //Convert to int val to string val ex: 10 = SPACE

                //AIMBOT
                Settings.Default.AIMBOT = Convert.ToBoolean(INIConfig.Read("Enable_AIMBOT", "AIMBOT"));
                Settings.Default.AIMBOTAcceleration = Convert.ToInt32(INIConfig.Read("Acceleration", "AIMBOT"));
                Settings.Default.ShowAIMFov = Convert.ToBoolean(INIConfig.Read("Show_Aim_Fov", "AIMBOT"));
                Settings.Default.AIMBOTFOV = Convert.ToInt32(INIConfig.Read("Aim_FOV", "AIMBOT"));
                Settings.Default.AIMBOT_PRIORITY = Convert.ToInt32(INIConfig.Read("Priority", "AIMBOT"));
                Settings.Default.AIMBOTTarget = Convert.ToInt32(INIConfig.Read("Target", "AIMBOT"));
                Settings.Default.AimKey = (INIConfig.Read("AIM_Key", "AIMBOT"));
                Settings.Default.AIMAutoFire = Convert.ToBoolean(INIConfig.Read("AutoFire", "AIMBOT"));
                Settings.Default.AUTO_FIRE_MS = Convert.ToInt32(INIConfig.Read("AutoFireMS", "AIMBOT"));

                //ESP
                Settings.Default.ESP = Convert.ToBoolean(INIConfig.Read("Enable_ESP", "ESP"));
                Settings.Default.ShowAllies = Convert.ToBoolean(INIConfig.Read("Show_Allies", "ESP"));
                Settings.Default.ESPSize = Convert.ToInt32(INIConfig.Read("Box_Size", "ESP"));
                Settings.Default.ESPForm = Convert.ToInt32(INIConfig.Read("Box_Form", "ESP"));
                Settings.Default.Show_Name = Convert.ToBoolean(INIConfig.Read("Show_Name", "ESP"));
                Settings.Default.Show_DISTANCE = Convert.ToBoolean(INIConfig.Read("Show_Distance", "ESP"));
                Settings.Default.Show_PING = Convert.ToBoolean(INIConfig.Read("Show_Ping", "ESP"));

                //SNAPLINE
                Settings.Default.SNAPLINE = Convert.ToBoolean(INIConfig.Read("Enable_SNAPLINE", "SNAPLINE"));
                Settings.Default.SNAPLINESize = Convert.ToInt32(INIConfig.Read("Size", "SNAPLINE"));
                Settings.Default.SNAPLINEStartingPoint = Convert.ToInt32(INIConfig.Read("Starting_Point", "SNAPLINE"));

                //STYLES
                Settings.Default.ESPColor = (Color)ConvertColor.ConvertFromString(INIConfig.Read("Enemies_Color", "STYLES"));
                Settings.Default.ESPALLIESCOLOR = (Color)ConvertColor.ConvertFromString(INIConfig.Read("Allies_Color", "STYLES"));
                Settings.Default.ESPFILLEDBORDERCOLOR = (Color)ConvertColor.ConvertFromString(INIConfig.Read("Filled_Border_Color", "STYLES"));
                Settings.Default.SNAPLINEColor = (Color)ConvertColor.ConvertFromString(INIConfig.Read("Snapline_Color", "STYLES"));
                Settings.Default.CROSSHAIRColor = (Color)ConvertColor.ConvertFromString(INIConfig.Read("Crosshair_Color", "STYLES"));
                Settings.Default.AIMBOT_FOV_Color = (Color)ConvertColor.ConvertFromString(INIConfig.Read("FOV_Color", "STYLES"));
                Settings.Default.MenuBorderColor = (Color)ConvertColor.ConvertFromString(INIConfig.Read("Menu_Color", "STYLES"));
                Settings.Default.PlayerNameColor = (Color)ConvertColor.ConvertFromString(INIConfig.Read("Player_Name_Color", "STYLES"));
                Settings.Default.PlayerPingColor = (Color)ConvertColor.ConvertFromString(INIConfig.Read("Player_Ping_Color", "STYLES"));

                //MISC
                Settings.Default.CROSSHAIR = Convert.ToBoolean(INIConfig.Read("Enable_CROSSHAIR", "MISC"));
                Settings.Default.CROSSHAIRSize = Convert.ToInt32(INIConfig.Read("Size", "MISC"));
                Settings.Default.CROSSHAIRThickness = Convert.ToInt32(INIConfig.Read("Thickness", "MISC"));
                Settings.Default.Show_Text_Border = Convert.ToBoolean(INIConfig.Read("Show_Text_Border", "MISC"));
                Settings.Default.ESP_SIZE_TEXT = Convert.ToInt32(INIConfig.Read("Text_Size", "MISC"));
                Settings.Default.FontText = Convert.ToString(INIConfig.Read("Font_Text", "MISC"));
                Settings.Default.VSAT = Convert.ToBoolean(INIConfig.Read("Enable_VSAT", "MISC"));

                //CONTROLLER
                Settings.Default.ControllerButtonAIMBOT = (INIConfig.Read("AIM_Key", "CONTROLLER"));
                Settings.Default.ControllerToggleAIMBOT = (INIConfig.Read("TOGGLE_AIMBOT", "CONTROLLER"));
                Settings.Default.ControllerToggleESP = (INIConfig.Read("TOGGLE_ESP", "CONTROLLER"));
                Settings.Default.ControllerToggleSNAPLINE = (INIConfig.Read("TOGGLE_SNAPLINE", "CONTROLLER"));
                Settings.Default.ControllerToggleVSAT = (INIConfig.Read("TOGGLE_VSAT", "CONTROLLER"));
                Settings.Default.ControllerToggleCROSSHAIR = (INIConfig.Read("TOGGLE_CROSSHAIR", "CONTROLLER"));

                //SETTING
                Settings.Default.FPS = Convert.ToInt32(INIConfig.Read("FPS", "SETTINGS"));
                Settings.Default.Key_ShowMENU = (Keys)kc.ConvertFromString(INIConfig.Read("Show_Menu", "SETTINGS"));
                Settings.Default.OVERLAY = Convert.ToBoolean(INIConfig.Read("Show_Overlay", "SETTINGS"));
                Settings.Default.ShowFPS = Convert.ToBoolean(INIConfig.Read("Show_FPS", "SETTINGS"));
                Settings.Default.VSYNC = Convert.ToBoolean(INIConfig.Read("VSYNC", "SETTINGS"));
                Settings.Default.FPS_CORNER = Convert.ToInt32(INIConfig.Read("FPS_Corner", "SETTINGS"));
                Settings.Default.Menu_Opacity = Convert.ToDouble(INIConfig.Read("Menu_Opacity", "SETTINGS"));
                Settings.Default.TextAntiAliasing = Convert.ToBoolean(INIConfig.Read("Anti_Aliasing", "SETTINGS"));
            }
            catch
            {
                resetConfigFile();
            }

            //--------READ CONFIG--------

            //AIMBOT
            label43.Text = Settings.Default.AimKey.ToString();

            metroSetTrackBar4.Value = (int)Settings.Default.AIMBOTAcceleration;
            label36.Text = Settings.Default.AIMBOTAcceleration.ToString();

            metroSetTrackBar8.Value = (int)Settings.Default.AUTO_FIRE_MS;
            label105.Text = Settings.Default.AUTO_FIRE_MS.ToString() + " MS";

            if (Settings.Default.AIMBOTFOV <= metroSetTrackBar6.Maximum)
            {
                metroSetTrackBar6.Value = Settings.Default.AIMBOTFOV;
                label57.Text = Settings.Default.AIMBOTFOV.ToString();
            }

            if (Settings.Default.AIMBOT == true && (Settings.Default.pfpf == 1))
            {
                CheckBoxAIMBOT.Ovalchecked = true;
            }
            else
            {
                CheckBoxAIMBOT.Ovalchecked = false;
            }

            if (Settings.Default.ShowAIMFov == true && (Settings.Default.pfpf == 1))
            {
                customCheckBox7.Ovalchecked = true;
                label59.Text = "Enabled";
            }
            else
            {
                customCheckBox7.Ovalchecked = false;
                label59.Text = "Disabled";
            }

            if (Settings.Default.AIMAutoFire == true && (Settings.Default.pfpf == 1))
            {
                customCheckBox4.Ovalchecked = true;
                label31.Text = "Enabled";
            }
            else
            {
                customCheckBox4.Ovalchecked = false;
                label31.Text = "Disabled";
            }

            if (Settings.Default.AIMBOT_PRIORITY == 1 && (Settings.Default.pfpf == 1))
            {
                metroSetEllipse8.NormalColor = Settings.Default.MenuBorderColor;
                metroSetEllipse7.NormalColor = Settings.Default.DefaultColor;
                label12.Text = "CROSSHAIR";
            }
            else if (Settings.Default.AIMBOT_PRIORITY == 2)
            {
                metroSetEllipse7.NormalColor = Settings.Default.MenuBorderColor;
                metroSetEllipse8.NormalColor = Settings.Default.DefaultColor;
                label12.Text = "DISTANCE";
            }

            if (Settings.Default.AIMBOTTarget == 1 && (Settings.Default.pfpf == 1))
            {
                metroSetEllipse4.NormalColor = Settings.Default.MenuBorderColor;
                metroSetEllipse2.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipse3.NormalColor = Settings.Default.DefaultColor;
                label34.Text = "HEAD";
            }
            else if (Settings.Default.AIMBOTTarget == 2)
            {
                metroSetEllipse2.NormalColor = Settings.Default.MenuBorderColor;
                metroSetEllipse3.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipse4.NormalColor = Settings.Default.DefaultColor;
                label34.Text = "BODY";
            }
            else if (Settings.Default.AIMBOTTarget == 3)
            {
                metroSetEllipse3.NormalColor = Settings.Default.MenuBorderColor;
                metroSetEllipse2.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipse4.NormalColor = Settings.Default.DefaultColor;
                label34.Text = "FOOT";
            }

            if (Settings.Default.AIMAutoFire == true && (Settings.Default.pfpf == 1) && Settings.Default.AimKey.ToString().ToLower() == "LButton".ToLower())
            {
                customCheckBox4.Ovalchecked = false;
                customCheckBox4.Enabled = false;
                label31.Text = "Disabled";
            }

            //ESP
            metroSetTrackBar1.Value = Settings.Default.ESPSize;
            labelESPSIZE.Text = metroSetTrackBar1.Value.ToString();

            if (Settings.Default.ESP == true)
            {
                CheckBoxESP.Ovalchecked = true;
            }
            else
            {
                CheckBoxESP.Ovalchecked = false;
            }

            if (Settings.Default.ShowAllies == true)
            {
                customCheckBox10.Ovalchecked = true;
                label72.Text = "Showed";
            }
            else
            {
                customCheckBox10.Ovalchecked = false;
                label72.Text = "Hidden";
            }

            if (Settings.Default.ESPForm == 1)
            {
                button7.BackgroundImage = Resources.Square;
                label10.Text = "Rectangle";
            }
            else if (Settings.Default.ESPForm == 2)
            {
                button7.BackgroundImage = Resources.SquareFULL;
                label10.Text = "Filled Rect";
            }
            else if (Settings.Default.ESPForm == 3)
            {
                button7.BackgroundImage = Resources.Ellipse;
                label10.Text = "Ellipse";
            }

            if (Settings.Default.Show_Name == true && Settings.Default.pfpf == 1)
            {
                customCheckBox11.Ovalchecked = true;
                label74.Text = "Showed";
            }
            else
            {
                customCheckBox11.Ovalchecked = false;
                label74.Text = "Hidden";
            }

            if (Settings.Default.Show_DISTANCE == true && Settings.Default.pfpf == 1)
            {
                customCheckBox2.Ovalchecked = true;
                label41.Text = "Showed";
            }
            else
            {
                customCheckBox2.Ovalchecked = false;
                label41.Text = "Hidden";
            }

            if (Settings.Default.Show_PING == true && Settings.Default.pfpf == 1)
            {
                customCheckBox5.Ovalchecked = true;
                label96.Text = "Showed";
            }
            else
            {
                customCheckBox5.Ovalchecked = false;
                label96.Text = "Hidden";
            }


            //SNAPLINE
            metroSetTrackBarSNAPLINESize.Value = Settings.Default.SNAPLINESize;

            labelSNAPLINESIZE.Text = metroSetTrackBarSNAPLINESize.Value.ToString();

            if (Settings.Default.SNAPLINE == true && (Settings.Default.pfpf == 1))
            {
                CheckBoxSnapLine.Ovalchecked = true;
            }
            else
            {
                CheckBoxSnapLine.Ovalchecked = false;

            }

            if (Settings.Default.ESPForm == 1)
            {
                button7.BackgroundImage = Resources.Square;
                label10.Text = "Rectangle";
            }
            else if (Settings.Default.ESPForm == 2)
            {
                button7.BackgroundImage = Resources.SquareFULL;
                label10.Text = "Filled Rect";
            }
            else if (Settings.Default.ESPForm == 3)
            {
                button7.BackgroundImage = Resources.Ellipse;
                label10.Text = "Ellipse";
            }

            if (Settings.Default.SNAPLINEStartingPoint == 1 && (Settings.Default.pfpf == 1))
            {
                metroSetEllipseSNAPLINE_TOP_LEFT.NormalColor = Settings.Default.MenuBorderColor;

                metroSetEllipseSNAPLINE_CENTER_TOP.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_TOP_RIGHT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_RIGHT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_BOTTOM_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_BOT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_BOTTOM_RIGHT.NormalColor = Settings.Default.DefaultColor;
                labelSNAPLINEStartingPoint.Text = "TOP LEFT";
            }
            else if (Settings.Default.SNAPLINEStartingPoint == 2)
            {
                metroSetEllipseSNAPLINE_CENTER_TOP.NormalColor = Settings.Default.MenuBorderColor;

                metroSetEllipseSNAPLINE_TOP_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_TOP_RIGHT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_RIGHT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_BOTTOM_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_BOT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_BOTTOM_RIGHT.NormalColor = Settings.Default.DefaultColor;
                labelSNAPLINEStartingPoint.Text = "CENTER TOP";
            }
            else if (Settings.Default.SNAPLINEStartingPoint == 3)
            {
                metroSetEllipseSNAPLINE_TOP_RIGHT.NormalColor = Settings.Default.MenuBorderColor;

                metroSetEllipseSNAPLINE_TOP_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_TOP.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_RIGHT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_BOTTOM_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_BOT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_BOTTOM_RIGHT.NormalColor = Settings.Default.DefaultColor;
                labelSNAPLINEStartingPoint.Text = "TOP RIGHT";
            }
            else if (Settings.Default.SNAPLINEStartingPoint == 4)
            {
                metroSetEllipseSNAPLINE_CENTER_LEFT.NormalColor = Settings.Default.MenuBorderColor;

                metroSetEllipseSNAPLINE_TOP_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_TOP.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_TOP_RIGHT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_RIGHT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_BOTTOM_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_BOT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_BOTTOM_RIGHT.NormalColor = Settings.Default.DefaultColor;
                labelSNAPLINEStartingPoint.Text = "CENTER L";
            }
            else if (Settings.Default.SNAPLINEStartingPoint == 5)
            {
                metroSetEllipseSNAPLINE_CENTER.NormalColor = Settings.Default.MenuBorderColor;

                metroSetEllipseSNAPLINE_TOP_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_TOP.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_TOP_RIGHT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_RIGHT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_BOTTOM_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_BOT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_BOTTOM_RIGHT.NormalColor = Settings.Default.DefaultColor;
                labelSNAPLINEStartingPoint.Text = "CENTER";
            }
            else if (Settings.Default.SNAPLINEStartingPoint == 6)
            {
                metroSetEllipseSNAPLINE_CENTER_RIGHT.NormalColor = Settings.Default.MenuBorderColor;

                metroSetEllipseSNAPLINE_TOP_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_TOP.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_TOP_RIGHT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_BOTTOM_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_BOT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_BOTTOM_RIGHT.NormalColor = Settings.Default.DefaultColor;
                labelSNAPLINEStartingPoint.Text = "CENTER R";
            }
            else if (Settings.Default.SNAPLINEStartingPoint == 7)
            {
                metroSetEllipseSNAPLINE_BOTTOM_LEFT.NormalColor = Settings.Default.MenuBorderColor;

                metroSetEllipseSNAPLINE_TOP_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_TOP.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_TOP_RIGHT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_RIGHT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_BOT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_BOTTOM_RIGHT.NormalColor = Settings.Default.DefaultColor;
                labelSNAPLINEStartingPoint.Text = "BOTTOM L";
            }
            else if (Settings.Default.SNAPLINEStartingPoint == 8)
            {
                metroSetEllipseSNAPLINE_CENTER_BOT.NormalColor = Settings.Default.MenuBorderColor;

                metroSetEllipseSNAPLINE_TOP_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_TOP.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_TOP_RIGHT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_RIGHT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_BOTTOM_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_BOTTOM_RIGHT.NormalColor = Settings.Default.DefaultColor;
                labelSNAPLINEStartingPoint.Text = "BOTTOM";
            }
            else if (Settings.Default.SNAPLINEStartingPoint == 9)
            {
                metroSetEllipseSNAPLINE_BOTTOM_RIGHT.NormalColor = Settings.Default.MenuBorderColor;

                metroSetEllipseSNAPLINE_TOP_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_TOP.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_TOP_RIGHT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_RIGHT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_BOTTOM_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_BOT.NormalColor = Settings.Default.DefaultColor;
                labelSNAPLINEStartingPoint.Text = "BOTTOM R";
            }

            //STYLES
            label3.Text = Settings.Default.ESPColor.Name;
            label90.Text = Settings.Default.ESPALLIESCOLOR.Name;
            label88.Text = Settings.Default.ESPFILLEDBORDERCOLOR.Name;
            label22.Text = Settings.Default.SNAPLINEColor.Name;
            label47.Text = Settings.Default.CROSSHAIRColor.Name;
            label78.Text = Settings.Default.AIMBOT_FOV_Color.Name;
            label103.Text = Settings.Default.MenuBorderColor.Name;
            label66.Text = Settings.Default.PlayerNameColor.Name;
            label110.Text = Settings.Default.PlayerPingColor.Name;

            metroSetEllipse1.NormalColor = Settings.Default.ESPColor;
            metroSetEllipse1.HoverColor = Settings.Default.ESPColor;

            metroSetEllipse11.NormalColor = Settings.Default.ESPALLIESCOLOR;
            metroSetEllipse11.HoverColor = Settings.Default.ESPALLIESCOLOR;

            metroSetEllipse10.NormalColor = Settings.Default.ESPFILLEDBORDERCOLOR;
            metroSetEllipse10.HoverColor = Settings.Default.ESPFILLEDBORDERCOLOR;

            metroSetEllipseSNAPLINEColor.NormalColor = Settings.Default.SNAPLINEColor;
            metroSetEllipseSNAPLINEColor.HoverColor = Settings.Default.SNAPLINEColor;

            metroSetEllipse5.NormalColor = Settings.Default.CROSSHAIRColor;
            metroSetEllipse5.HoverColor = Settings.Default.CROSSHAIRColor;

            metroSetEllipse6.NormalColor = Settings.Default.AIMBOT_FOV_Color;
            metroSetEllipse6.HoverColor = Settings.Default.AIMBOT_FOV_Color;

            metroSetEllipse15.NormalColor = Settings.Default.MenuBorderColor;
            metroSetEllipse15.HoverColor = Settings.Default.MenuBorderColor;

            metroSetEllipse16.NormalColor = Settings.Default.PlayerNameColor;
            metroSetEllipse16.HoverColor = Settings.Default.PlayerNameColor;

            metroSetEllipse18.NormalColor = Settings.Default.PlayerPingColor;
            metroSetEllipse18.HoverColor = Settings.Default.PlayerPingColor;

            //MISC
            if (Settings.Default.CROSSHAIR == false)
            {
                customCheckBox6.Ovalchecked = false;
            }
            else
            {
                customCheckBox6.Ovalchecked = true;
            }

            if (Settings.Default.VSAT == false)
            {
                customCheckBox9.Ovalchecked = false;
            }
            else
            {
                customCheckBox9.Ovalchecked = true;
            }

            if (Settings.Default.Show_Text_Border == true && Settings.Default.pfpf == 1)
            {
                customCheckBox13.Ovalchecked = true;
                label98.Text = "Showed";
            }
            else
            {
                customCheckBox13.Ovalchecked = false;
                label98.Text = "Hidden";
            }

            metroSetTrackBar2.Value = Settings.Default.CROSSHAIRSize;
            label52.Text = Settings.Default.CROSSHAIRSize.ToString();

            metroSetTrackBar5.Value = Settings.Default.CROSSHAIRThickness;
            label55.Text = Settings.Default.CROSSHAIRThickness.ToString();

            metroSetTrackBar3.Value = Settings.Default.ESP_SIZE_TEXT;
            label82.Text = Settings.Default.ESP_SIZE_TEXT.ToString();

            label114.Text = Settings.Default.FontText;

            //CONTROLLER
            label117.Text = Settings.Default.ControllerButtonAIMBOT.ToString();
            label118.Text = Settings.Default.ControllerToggleAIMBOT.ToString();
            label95.Text = Settings.Default.ControllerToggleESP.ToString();
            label121.Text = Settings.Default.ControllerToggleSNAPLINE.ToString();
            label125.Text = Settings.Default.ControllerToggleVSAT.ToString();
            label129.Text = Settings.Default.ControllerToggleCROSSHAIR.ToString();

            //SETTING
            metroSetTrackBar7.Value = Settings.Default.FPS;
            label67.Text = metroSetTrackBar7.Value.ToString();
            if (metroSetTrackBar7.Value >= 999)
            {
                label67.Text = "Max";
            }
            label16.Text = Settings.Default.Key_ShowMENU.ToString();

            if (Settings.Default.OVERLAY == false)
            {
                customCheckBox3.Ovalchecked = false;
                label29.Text = "Hidden";
            }
            else
            {
                customCheckBox3.Ovalchecked = true;
                label29.Text = "Showed";
            }

            if (Settings.Default.ShowFPS == false)
            {
                customCheckBox8.Ovalchecked = false;
                label69.Text = "Hidden";
            }
            else
            {
                customCheckBox8.Ovalchecked = true;
                label69.Text = "Showed";
            }

            if (Settings.Default.VSYNC == false)
            {
                customCheckBox12.Ovalchecked = false;
                label76.Text = "Disabled";
            }
            else
            {
                customCheckBox12.Ovalchecked = true;
                label76.Text = "Enabled";
            }

            if (Settings.Default.FPS_CORNER == 1 && (Settings.Default.pfpf == 1))
            {
                metroSetEllipse9.NormalColor = Settings.Default.MenuBorderColor;

                metroSetEllipse14.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipse12.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipse13.NormalColor = Settings.Default.DefaultColor;
                label101.Text = "TOP L";
            }
            else if (Settings.Default.FPS_CORNER == 2 && (Settings.Default.pfpf == 1))
            {
                metroSetEllipse14.NormalColor = Settings.Default.MenuBorderColor;

                metroSetEllipse9.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipse12.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipse13.NormalColor = Settings.Default.DefaultColor;
                label101.Text = "TOP R";
            }
            else if (Settings.Default.FPS_CORNER == 3 && (Settings.Default.pfpf == 1))
            {
                metroSetEllipse12.NormalColor = Settings.Default.MenuBorderColor;

                metroSetEllipse9.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipse14.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipse13.NormalColor = Settings.Default.DefaultColor;
                label101.Text = "BOT L";
            }
            else if (Settings.Default.FPS_CORNER == 4 && (Settings.Default.pfpf == 1))
            {
                metroSetEllipse13.NormalColor = Settings.Default.MenuBorderColor;

                metroSetEllipse9.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipse14.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipse12.NormalColor = Settings.Default.DefaultColor;
                label101.Text = "BOT R";
            }

            metroSetTrackBar9.Value = (int)Settings.Default.Menu_Opacity;

            label112.Text = metroSetTrackBar9.Value.ToString();
            if (metroSetTrackBar9.Value >= 100)
            {
                label112.Text = "Max";
            }

            if (Settings.Default.TextAntiAliasing == false)
            {
                customCheckBox1.Ovalchecked = false;
                label92.Text = "Disabled";
            }
            else
            {
                customCheckBox1.Ovalchecked = true;
                label92.Text = "Enabled";
            }

            metroSetComboBox1.Items.Clear();
            foreach (string Dir in Directory.GetDirectories("profiles"))
            {
                FileInfo result = new FileInfo(Dir);

                if (result.Name.Length > 8)
                {
                    continue;
                }

                metroSetComboBox1.Items.Add(result.Name);
            }
            metroSetComboBox1.Items.Add("New..");
            metroSetComboBox1.Text = Settings.Default.Profiles;

            label81.Text = Settings.Default.Profiles;

            //END --------READ CONFIG--------


            //--------THEME--------
            BackColor = Settings.Default.MenuBorderColor;

            //HOME
            label65.ForeColor = Settings.Default.MenuBorderColor;
            label1.ForeColor = Settings.Default.MenuBorderColor;
            label80.ForeColor = Settings.Default.MenuBorderColor;

            button2.ForeColor = Settings.Default.MenuBorderColor;
            button4.ForeColor = Settings.Default.MenuBorderColor;

            //AIMBOT
            CheckBoxAIMBOT.LabelColor = Settings.Default.MenuBorderColor;
            CheckBoxAIMBOT.ovalcolorFalse = Settings.Default.MenuBorderColor;
            CheckBoxAIMBOT.ovalcolorTrue = Settings.Default.MenuBorderColor;
            CheckBoxAIMBOT.RectangleborderovalcolorTrue = Settings.Default.MenuBorderColor;

            metroSetTrackBar4.ValueColor = Settings.Default.MenuBorderColor;
            metroSetTrackBar6.ValueColor = Settings.Default.MenuBorderColor;
            metroSetTrackBar8.ValueColor = Settings.Default.MenuBorderColor;

            button8.ForeColor = Settings.Default.MenuBorderColor;
            button16.ForeColor = Settings.Default.MenuBorderColor;
            button18.ForeColor = Settings.Default.MenuBorderColor;
            button19.ForeColor = Settings.Default.MenuBorderColor;
            button20.ForeColor = Settings.Default.MenuBorderColor;

            metroSetEllipse2.HoverColor = Settings.Default.MenuBorderColor;
            metroSetEllipse3.HoverColor = Settings.Default.MenuBorderColor;
            metroSetEllipse4.HoverColor = Settings.Default.MenuBorderColor;
            metroSetEllipse7.HoverColor = Settings.Default.MenuBorderColor;
            metroSetEllipse8.HoverColor = Settings.Default.MenuBorderColor;

            customCheckBox7.LabelColor = Settings.Default.MenuBorderColor;
            customCheckBox7.ovalcolorFalse = Settings.Default.MenuBorderColor;
            customCheckBox7.ovalcolorTrue = Settings.Default.MenuBorderColor;
            customCheckBox7.RectangleborderovalcolorTrue = Settings.Default.MenuBorderColor;

            customCheckBox4.LabelColor = Settings.Default.MenuBorderColor;
            customCheckBox4.ovalcolorFalse = Settings.Default.MenuBorderColor;
            customCheckBox4.ovalcolorTrue = Settings.Default.MenuBorderColor;
            customCheckBox4.RectangleborderovalcolorTrue = Settings.Default.MenuBorderColor;

            //ESP
            CheckBoxESP.LabelColor = Settings.Default.MenuBorderColor;
            CheckBoxESP.ovalcolorFalse = Settings.Default.MenuBorderColor;
            CheckBoxESP.ovalcolorTrue = Settings.Default.MenuBorderColor;
            CheckBoxESP.RectangleborderovalcolorTrue = Settings.Default.MenuBorderColor;
            customCheckBox10.LabelColor = Settings.Default.MenuBorderColor;
            customCheckBox10.ovalcolorFalse = Settings.Default.MenuBorderColor;
            customCheckBox10.ovalcolorTrue = Settings.Default.MenuBorderColor;
            customCheckBox10.RectangleborderovalcolorTrue = Settings.Default.MenuBorderColor;
            customCheckBox11.LabelColor = Settings.Default.MenuBorderColor;
            customCheckBox11.ovalcolorFalse = Settings.Default.MenuBorderColor;
            customCheckBox11.ovalcolorTrue = Settings.Default.MenuBorderColor;
            customCheckBox11.RectangleborderovalcolorTrue = Settings.Default.MenuBorderColor;
            customCheckBox2.LabelColor = Settings.Default.MenuBorderColor;
            customCheckBox2.ovalcolorFalse = Settings.Default.MenuBorderColor;
            customCheckBox2.ovalcolorTrue = Settings.Default.MenuBorderColor;
            customCheckBox2.RectangleborderovalcolorTrue = Settings.Default.MenuBorderColor;
            customCheckBox5.LabelColor = Settings.Default.MenuBorderColor;
            customCheckBox5.ovalcolorFalse = Settings.Default.MenuBorderColor;
            customCheckBox5.ovalcolorTrue = Settings.Default.MenuBorderColor;
            customCheckBox5.RectangleborderovalcolorTrue = Settings.Default.MenuBorderColor;

            metroSetTrackBar1.ValueColor = Settings.Default.MenuBorderColor;

            button1.ForeColor = Settings.Default.MenuBorderColor;
            button5.ForeColor = Settings.Default.MenuBorderColor;
            button6.ForeColor = Settings.Default.MenuBorderColor;

            //SNAPLINE
            CheckBoxSnapLine.LabelColor = Settings.Default.MenuBorderColor;
            CheckBoxSnapLine.ovalcolorFalse = Settings.Default.MenuBorderColor;
            CheckBoxSnapLine.ovalcolorTrue = Settings.Default.MenuBorderColor;
            CheckBoxSnapLine.RectangleborderovalcolorTrue = Settings.Default.MenuBorderColor;

            metroSetEllipseSNAPLINE_TOP_LEFT.HoverColor = Settings.Default.MenuBorderColor;
            metroSetEllipseSNAPLINE_CENTER_TOP.HoverColor = Settings.Default.MenuBorderColor;
            metroSetEllipseSNAPLINE_TOP_RIGHT.HoverColor = Settings.Default.MenuBorderColor;
            metroSetEllipseSNAPLINE_CENTER_LEFT.HoverColor = Settings.Default.MenuBorderColor;
            metroSetEllipseSNAPLINE_CENTER.HoverColor = Settings.Default.MenuBorderColor;
            metroSetEllipseSNAPLINE_CENTER_RIGHT.HoverColor = Settings.Default.MenuBorderColor;
            metroSetEllipseSNAPLINE_BOTTOM_LEFT.HoverColor = Settings.Default.MenuBorderColor;
            metroSetEllipseSNAPLINE_CENTER_BOT.HoverColor = Settings.Default.MenuBorderColor;
            metroSetEllipseSNAPLINE_BOTTOM_RIGHT.HoverColor = Settings.Default.MenuBorderColor;

            metroSetTrackBarSNAPLINESize.ValueColor = Settings.Default.MenuBorderColor;

            button8.ForeColor = Settings.Default.MenuBorderColor;
            button9.ForeColor = Settings.Default.MenuBorderColor;
            button10.ForeColor = Settings.Default.MenuBorderColor;

            //STYLE
            button27.ForeColor = Settings.Default.MenuBorderColor;
            button28.ForeColor = Settings.Default.MenuBorderColor;
            button29.ForeColor = Settings.Default.MenuBorderColor;

            //MISC
            customCheckBox6.LabelColor = Settings.Default.MenuBorderColor;
            customCheckBox6.ovalcolorFalse = Settings.Default.MenuBorderColor;
            customCheckBox6.ovalcolorTrue = Settings.Default.MenuBorderColor;
            customCheckBox6.RectangleborderovalcolorTrue = Settings.Default.MenuBorderColor;
            customCheckBox9.LabelColor = Settings.Default.MenuBorderColor;
            customCheckBox9.ovalcolorFalse = Settings.Default.MenuBorderColor;
            customCheckBox9.ovalcolorTrue = Settings.Default.MenuBorderColor;
            customCheckBox9.RectangleborderovalcolorTrue = Settings.Default.MenuBorderColor;
            customCheckBox13.LabelColor = Settings.Default.MenuBorderColor;
            customCheckBox13.ovalcolorFalse = Settings.Default.MenuBorderColor;
            customCheckBox13.ovalcolorTrue = Settings.Default.MenuBorderColor;
            customCheckBox13.RectangleborderovalcolorTrue = Settings.Default.MenuBorderColor;

            metroSetTrackBar2.ValueColor = Settings.Default.MenuBorderColor;
            metroSetTrackBar5.ValueColor = Settings.Default.MenuBorderColor;
            metroSetTrackBar3.ValueColor = Settings.Default.MenuBorderColor;

            button22.ForeColor = Settings.Default.MenuBorderColor;
            button23.ForeColor = Settings.Default.MenuBorderColor;
            button24.ForeColor = Settings.Default.MenuBorderColor;
            button37.ForeColor = Settings.Default.MenuBorderColor;

            //CONTROLLER
            label109.ForeColor = Settings.Default.MenuBorderColor;

            button44.ForeColor = Settings.Default.MenuBorderColor;
            button41.ForeColor = Settings.Default.MenuBorderColor;
            button42.ForeColor = Settings.Default.MenuBorderColor;
            button43.ForeColor = Settings.Default.MenuBorderColor;
            button50.ForeColor = Settings.Default.MenuBorderColor;
            button53.ForeColor = Settings.Default.MenuBorderColor;
            button48.ForeColor = Settings.Default.MenuBorderColor;
            button45.ForeColor = Settings.Default.MenuBorderColor;
            button46.ForeColor = Settings.Default.MenuBorderColor;
            button47.ForeColor = Settings.Default.MenuBorderColor;
            button49.ForeColor = Settings.Default.MenuBorderColor;
            button51.ForeColor = Settings.Default.MenuBorderColor;

            button35.ForeColor = Settings.Default.MenuBorderColor;
            button38.ForeColor = Settings.Default.MenuBorderColor;
            button39.ForeColor = Settings.Default.MenuBorderColor;
            button40.ForeColor = Settings.Default.MenuBorderColor;

            //SETTINGS
            customCheckBox8.LabelColor = Settings.Default.MenuBorderColor;
            customCheckBox8.ovalcolorFalse = Settings.Default.MenuBorderColor;
            customCheckBox8.ovalcolorTrue = Settings.Default.MenuBorderColor;
            customCheckBox8.RectangleborderovalcolorTrue = Settings.Default.MenuBorderColor;
            customCheckBox12.LabelColor = Settings.Default.MenuBorderColor;
            customCheckBox12.ovalcolorFalse = Settings.Default.MenuBorderColor;
            customCheckBox12.ovalcolorTrue = Settings.Default.MenuBorderColor;
            customCheckBox12.RectangleborderovalcolorTrue = Settings.Default.MenuBorderColor;
            customCheckBox3.LabelColor = Settings.Default.MenuBorderColor;
            customCheckBox3.ovalcolorFalse = Settings.Default.MenuBorderColor;
            customCheckBox3.ovalcolorTrue = Settings.Default.MenuBorderColor;
            customCheckBox3.RectangleborderovalcolorTrue = Settings.Default.MenuBorderColor;
            customCheckBox1.LabelColor = Settings.Default.MenuBorderColor;
            customCheckBox1.ovalcolorFalse = Settings.Default.MenuBorderColor;
            customCheckBox1.ovalcolorTrue = Settings.Default.MenuBorderColor;
            customCheckBox1.RectangleborderovalcolorTrue = Settings.Default.MenuBorderColor;

            metroSetEllipse9.HoverColor = Settings.Default.MenuBorderColor;
            metroSetEllipse12.HoverColor = Settings.Default.MenuBorderColor;
            metroSetEllipse13.HoverColor = Settings.Default.MenuBorderColor;
            metroSetEllipse14.HoverColor = Settings.Default.MenuBorderColor;

            metroSetTrackBar7.ValueColor = Settings.Default.MenuBorderColor;
            metroSetTrackBar9.ValueColor = Settings.Default.MenuBorderColor;

            button15.ForeColor = Settings.Default.MenuBorderColor;
            button21.ForeColor = Settings.Default.MenuBorderColor;
            button30.ForeColor = Settings.Default.MenuBorderColor;
            button31.ForeColor = Settings.Default.MenuBorderColor;
            button11.ForeColor = Settings.Default.MenuBorderColor;
            button12.ForeColor = Settings.Default.MenuBorderColor;
            button13.ForeColor = Settings.Default.MenuBorderColor;
            button32.ForeColor = Settings.Default.MenuBorderColor;
            button33.ForeColor = Settings.Default.MenuBorderColor;
            button34.ForeColor = Settings.Default.MenuBorderColor;

            metroSetComboBox1.ArrowColor = Settings.Default.MenuBorderColor;
            metroSetComboBox1.BorderColor = Settings.Default.MenuBorderColor;
            metroSetComboBox1.ForeColor = Settings.Default.MenuBorderColor;
            metroSetComboBox1.DisabledBackColor = Settings.Default.MenuBorderColor;
            metroSetComboBox1.DisabledBorderColor = Settings.Default.MenuBorderColor;
            metroSetComboBox1.DisabledForeColor = Settings.Default.MenuBorderColor;
            metroSetComboBox1.SelectedItemForeColor = Settings.Default.MenuBorderColor;

            textBox1.ForeColor = Settings.Default.MenuBorderColor;
            //END --------THEME--------

            //CHECK PRM
            if (Settings.Default.pfpf == 1)
            {
                ACB.Visible = true;
                label42.Visible = true;
                label42.Enabled = true;
                label75.Visible = true;
                label75.Enabled = true;
                customCheckBox2.Visible = true;
                customCheckBox2.Enabled = true;
                customCheckBox5.Visible = true;
                customCheckBox5.Enabled = true;
                customCheckBox11.Visible = true;
                customCheckBox11.Enabled = true;
                label41.Visible = true;
                label41.Enabled = true;
                label74.Visible = true;
                label74.Enabled = true;
                label96.Visible = true;
                label96.Enabled = true;
                label97.Visible = true;
                label97.Enabled = true;
                panel2.Visible = true;
                panel2.Enabled = true;
                metroSetComboBox1.Visible = true;
                return;
            }
            else
            {
                Security.FAILACC();
            }
        }

        private void StartThreads()
        {
            //start thread flag
            Examples.Example.isRunning = true;

            //start thread for positioning and sizing overlay on top of target process
            windowPosThread = new Thread(UpdateWindow);
            windowPosThread.IsBackground = false;
            windowPosThread.Start(Handle);
        }

        private void UpdateWindow(object handle)
        {
            //update flag, make sure game is still running
            while (Examples.Example.isRunning)
            {

                if (!Memory.IsProcessRunning(Examples.Example.process))
                {
                    Examples.Example.isRunning = false;
                    Security.FAILACC();
                    continue;
                }

                //ensure we are in focus and on top of game
                SetOverlayPosition((IntPtr)handle);

                //sleep for a bit, we don't need to move around constantly
                Thread.Sleep(Examples.Example.refreshWindows);
            }
        }

        private void SetOverlayPosition(IntPtr overlayHandle)
        {

            //get window handle
            IntPtr gameProcessHandle = Examples.Example.process.MainWindowHandle;
            if (gameProcessHandle == IntPtr.Zero)
                return;

            //get position and size of window
            NativeMethods.RECT targetWindowPosition, targetWindowSize;
            if (!NativeMethods.GetWindowRect(gameProcessHandle, out targetWindowPosition))
                return;
            if (!NativeMethods.GetClientRect(gameProcessHandle, out targetWindowSize))
                return;

            //calculate width and height of full target window
            int width = targetWindowPosition.Right - targetWindowPosition.Left;
            int height = targetWindowPosition.Bottom - targetWindowPosition.Top;

            //calculate inner window size without borders      
            int bWidth = targetWindowPosition.Right - targetWindowPosition.Left;
            int bHeight = targetWindowPosition.Bottom - targetWindowPosition.Top;

            //check if window has borders
            int dwStyle = NativeMethods.GetWindowLong(gameProcessHandle, NativeMethods.GWL_STYLE);
            if ((dwStyle & NativeMethods.WS_BORDER) != 0)
            {

                width = targetWindowSize.Right - targetWindowSize.Left;
                height = targetWindowSize.Bottom - targetWindowSize.Top;

                int borderWidth = (bWidth - targetWindowSize.Right) / 2;
                int borderHeight = (bHeight - targetWindowSize.Bottom);
                borderHeight -= borderWidth; //remove bottom

                targetWindowPosition.Left += borderWidth;
                targetWindowPosition.Top += borderHeight;

                //Return function only if windows border exceeds the limit of the game
                if ((Location.X > targetWindowPosition.Right - Size.Width - borderWidth)
                    || Location.X < targetWindowPosition.Left
                    || Location.Y < targetWindowPosition.Top
                    || Location.Y > targetWindowPosition.Bottom - Size.Height - borderWidth)
                {
                    //move and resize self window to match target window
                    NativeMethods.MoveWindow(overlayHandle, (width / 2 + targetWindowPosition.Left) - Width / 2, (height / 2 + targetWindowPosition.Top) - Height / 2, Size.Width, Size.Height, true);
                }

                //Return function only if the windows doesn't have moved
                if (targetWindowPosition.Left == Examples.Example._oldtargetWindowPositionLeft
                        && targetWindowPosition.Top == Examples.Example._oldtargetWindowPositionTop
                        && Examples.Example.gameWidth == width
                        && Examples.Example.gameHeight == height)
                {
                    return;
                }

                //move and resize self window to match target window
                NativeMethods.MoveWindow(overlayHandle, (width / 2 + targetWindowPosition.Left) - Width / 2, (height / 2 + targetWindowPosition.Top) - Height / 2, Size.Width, Size.Height, true);
            }
        }

        private void AssaultHack_FormClosing(object sender, FormClosingEventArgs e)
        {
            try

            {

                windowPosThread.Abort(200);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Dispose(true);
                Close();
            }
            catch { }
            //kill threads
        }

        private void CustomCheckBoxSnapLine_Ovalclick()
        {

            if (Settings.Default.SNAPLINE == false && (Settings.Default.pfpf == 1))
            {
                Settings.Default.SNAPLINE = true;
            }
            else
            {
                Settings.Default.SNAPLINE = false;
            }
            INIConfig.Write("Enable_SNAPLINE", Convert.ToString(Settings.Default.SNAPLINE), "SNAPLINE");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Security.FAILACC();
        }

        private void button4_Click(object sender, EventArgs e)
        {

            try
            {
                windowPosThread.Abort(200);
                Examples.Example.Menu_Showed = false;
                Close();
            }
            catch { }
            //kill threads

        }

        private void metroSetTrackBar1_Scroll(object sender)
        {

            Settings.Default.ESPSize = metroSetTrackBar1.Value;
            labelESPSIZE.Text = metroSetTrackBar1.Value.ToString();
            INIConfig.Write("Box_Size", Convert.ToString(Settings.Default.ESPSize), "ESP");
        }

        private void metroSetTrackBar2_Scroll(object sender)
        {

            Settings.Default.SNAPLINESize = metroSetTrackBarSNAPLINESize.Value;
            labelSNAPLINESIZE.Text = metroSetTrackBarSNAPLINESize.Value.ToString();

            INIConfig.Write("Size", Convert.ToString(Settings.Default.SNAPLINESize), "SNAPLINE");
        }

        private void metroSetEllipse1_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();

            Settings.Default.ESPColor = colorDialog1.Color;

            label3.Text = Settings.Default.ESPColor.Name;

            metroSetEllipse1.NormalColor = colorDialog1.Color;
            metroSetEllipse1.HoverColor = colorDialog1.Color;

            INIConfig.Write("Enemies_Color", ConvertColor.ConvertToString(Settings.Default.ESPColor.ToArgb()), "STYLES");
        }

        private void metroSetEllipse2_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();

            Settings.Default.SNAPLINEColor = colorDialog1.Color;

            label22.Text = Settings.Default.SNAPLINEColor.Name;

            metroSetEllipseSNAPLINEColor.NormalColor = colorDialog1.Color;
            metroSetEllipseSNAPLINEColor.HoverColor = colorDialog1.Color;

            INIConfig.Write("Snapline_Color", ConvertColor.ConvertToString(Settings.Default.SNAPLINEColor.ToArgb()), "STYLES");
        }

        private void button_MouseEnter(object sender, EventArgs e)
        {

            Button button = (Button)sender;
            button.ForeColor = Color.FromArgb(65, 177, 225);

        }

        private void button_MouseLeave(object sender, EventArgs e)
        {

            Button button = (Button)sender;
            button.ForeColor = Settings.Default.MenuBorderColor;

        }

        private void button7_Click(object sender, EventArgs e)
        {

            if (Settings.Default.ESPForm == 1)
            {
                button7.BackgroundImage = Resources.SquareFULL;
                label10.Text = "Ellipse";
                Settings.Default.ESPForm = 2;
            }
            else if (Settings.Default.ESPForm == 2)
            {
                button7.BackgroundImage = Resources.Ellipse;
                label10.Text = "Filled Rect";
                Settings.Default.ESPForm = 3;
            }
            else if (Settings.Default.ESPForm == 3)
            {
                button7.BackgroundImage = Resources.Square;
                label10.Text = "Rectangle";
                Settings.Default.ESPForm = 1;
            }
            INIConfig.Write("Box_Form", Convert.ToString(Settings.Default.ESPForm), "ESP");
        }

        private void customCheckBox1_Ovalclick()
        {

            //Security.CVP2();

            if (Settings.Default.ESP == false)
            {
                Settings.Default.ESP = true;
            }
            else
            {
                Settings.Default.ESP = false;
            }

            INIConfig.Write("Enable_ESP", Convert.ToString(Settings.Default.ESP), "ESP");

        }

        private void metroSetEllipseSNAPLINEStartingPoint_Click(object sender, EventArgs e)
        {

            MetroSet_UI.Controls.MetroSetEllipse metroSetEllipse = (MetroSet_UI.Controls.MetroSetEllipse)sender;

            if (metroSetEllipse.Name == metroSetEllipseSNAPLINE_TOP_LEFT.Name)
            {
                Settings.Default.SNAPLINEStartingPoint = 1;
                labelSNAPLINEStartingPoint.Text = "TOP LEFT";

                metroSetEllipseSNAPLINE_TOP_RIGHT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_TOP.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_BOTTOM_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_BOTTOM_RIGHT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_BOT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_RIGHT.NormalColor = Settings.Default.DefaultColor;
            }
            else if (metroSetEllipse.Name == metroSetEllipseSNAPLINE_CENTER_TOP.Name)
            {
                Settings.Default.SNAPLINEStartingPoint = 2;
                labelSNAPLINEStartingPoint.Text = "CENTER T";

                metroSetEllipseSNAPLINE_TOP_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_TOP_RIGHT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_BOTTOM_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_BOTTOM_RIGHT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_BOT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_RIGHT.NormalColor = Settings.Default.DefaultColor;
            }
            else if (metroSetEllipse.Name == metroSetEllipseSNAPLINE_TOP_RIGHT.Name)
            {
                Settings.Default.SNAPLINEStartingPoint = 3;
                labelSNAPLINEStartingPoint.Text = "TOP RIGHT";

                metroSetEllipseSNAPLINE_TOP_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_TOP.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_BOTTOM_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_BOTTOM_RIGHT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_BOT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_RIGHT.NormalColor = Settings.Default.DefaultColor;
            }
            else if (metroSetEllipse.Name == metroSetEllipseSNAPLINE_CENTER_LEFT.Name)
            {
                Settings.Default.SNAPLINEStartingPoint = 4;
                labelSNAPLINEStartingPoint.Text = "CENTER L";

                metroSetEllipseSNAPLINE_TOP_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_TOP.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_BOTTOM_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_BOTTOM_RIGHT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_BOT.NormalColor = Settings.Default.DefaultColor;

                metroSetEllipseSNAPLINE_TOP_RIGHT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_RIGHT.NormalColor = Settings.Default.DefaultColor;
            }
            else if (metroSetEllipse.Name == metroSetEllipseSNAPLINE_CENTER.Name)
            {
                Settings.Default.SNAPLINEStartingPoint = 5;
                labelSNAPLINEStartingPoint.Text = "CENTER";

                metroSetEllipseSNAPLINE_TOP_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_TOP.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_BOTTOM_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_BOTTOM_RIGHT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_BOT.NormalColor = Settings.Default.DefaultColor;

                metroSetEllipseSNAPLINE_TOP_RIGHT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_RIGHT.NormalColor = Settings.Default.DefaultColor;
            }
            else if (metroSetEllipse.Name == metroSetEllipseSNAPLINE_CENTER_RIGHT.Name)
            {
                Settings.Default.SNAPLINEStartingPoint = 6;
                labelSNAPLINEStartingPoint.Text = "CENTER R";

                metroSetEllipseSNAPLINE_TOP_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_TOP.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_BOTTOM_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_BOTTOM_RIGHT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_BOT.NormalColor = Settings.Default.DefaultColor;

                metroSetEllipseSNAPLINE_TOP_RIGHT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER.NormalColor = Settings.Default.DefaultColor;
            }
            else if (metroSetEllipse.Name == metroSetEllipseSNAPLINE_BOTTOM_LEFT.Name)
            {
                Settings.Default.SNAPLINEStartingPoint = 7;
                labelSNAPLINEStartingPoint.Text = "BOTTOM L";

                metroSetEllipseSNAPLINE_TOP_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_TOP_RIGHT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_TOP.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_BOTTOM_RIGHT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_BOT.NormalColor = Settings.Default.DefaultColor;

                metroSetEllipseSNAPLINE_CENTER.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_RIGHT.NormalColor = Settings.Default.DefaultColor;
            }
            else if (metroSetEllipse.Name == metroSetEllipseSNAPLINE_CENTER_BOT.Name)
            {
                Settings.Default.SNAPLINEStartingPoint = 8;
                labelSNAPLINEStartingPoint.Text = "CENTER B";

                metroSetEllipseSNAPLINE_TOP_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_TOP_RIGHT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_TOP.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_BOTTOM_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_BOTTOM_RIGHT.NormalColor = Settings.Default.DefaultColor;

                metroSetEllipseSNAPLINE_CENTER.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_RIGHT.NormalColor = Settings.Default.DefaultColor;
            }
            else if (metroSetEllipse.Name == metroSetEllipseSNAPLINE_BOTTOM_RIGHT.Name)
            {
                Settings.Default.SNAPLINEStartingPoint = 9;
                labelSNAPLINEStartingPoint.Text = "BOTTOM R";

                metroSetEllipseSNAPLINE_TOP_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_TOP_RIGHT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_TOP.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_BOTTOM_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_BOT.NormalColor = Settings.Default.DefaultColor;

                metroSetEllipseSNAPLINE_CENTER.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_LEFT.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipseSNAPLINE_CENTER_RIGHT.NormalColor = Settings.Default.DefaultColor;
            }

            metroSetEllipse.NormalColor = Settings.Default.MenuBorderColor;
            metroSetEllipse.HoverColor = Settings.Default.MenuBorderColor;

            INIConfig.Write("Starting_Point", Convert.ToString(Settings.Default.SNAPLINEStartingPoint), "SNAPLINE");
        }

        private async Task GetKeyController()
        {
            KeysConverter kc = new KeysConverter(); //Convert to int val to string val

            if (started == true)
            {
                return;
            }

            started = true;

            while (true)
            {
                await Task.Delay(1);

                //CONTROLLER AIMBOT
                if (Settings.Default.ActiveFuncKey == 1)
                {
                    if (Examples.Example.ActionControllerButtonClicked == true)
                    {
                        label117.Text = kc.ConvertToString(Settings.Default.ControllerButtonAIMBOT);
                        button44.Text = "Change";
                        INIConfig.Write("AIM_Key", kc.ConvertToString(Settings.Default.ControllerButtonAIMBOT), "CONTROLLER");

                        Settings.Default.ActiveFuncKey = 0;
                        started = false;
                        Examples.Example.ActionControllerButtonClicked = false;
                        return;
                    }
                }
                //-----CONTROLLER AIMBOT END-----

                //CONTROLLER TOGGLE AIMBOT
                else if (Settings.Default.ActiveFuncKey == 3)
                {
                    if (Examples.Example.ActionControllerButtonClicked == true)
                    {
                        label118.Text = kc.ConvertToString(Settings.Default.ControllerToggleAIMBOT);
                        button41.Text = "Change";
                        INIConfig.Write("TOGGLE_AIMBOT", kc.ConvertToString(Settings.Default.ControllerToggleAIMBOT), "CONTROLLER");

                        Settings.Default.ActiveFuncKey = 0;
                        started = false;
                        Examples.Example.ActionControllerButtonClicked = false;
                        return;
                    }
                }
                //-----CONTROLLER TOGGLE AIMBOT END-----

                //CONTROLLER TOGGLE ESP
                else if (Settings.Default.ActiveFuncKey == 4)
                {
                    if (Examples.Example.ActionControllerButtonClicked == true)
                    {
                        label95.Text = kc.ConvertToString(Settings.Default.ControllerToggleESP);
                        button42.Text = "Change";
                        INIConfig.Write("TOGGLE_ESP", kc.ConvertToString(Settings.Default.ControllerToggleESP), "CONTROLLER");

                        Settings.Default.ActiveFuncKey = 0;
                        started = false;
                        Examples.Example.ActionControllerButtonClicked = false;
                        return;
                    }
                }
                //-----CONTROLLER TOGGLE ESP END-----

                //CONTROLLER TOGGLE SNAPLINE
                else if (Settings.Default.ActiveFuncKey == 5)
                {
                    if (Examples.Example.ActionControllerButtonClicked == true)
                    {
                        label121.Text = kc.ConvertToString(Settings.Default.ControllerToggleSNAPLINE);
                        button43.Text = "Change";
                        INIConfig.Write("TOGGLE_SNAPLINE", kc.ConvertToString(Settings.Default.ControllerToggleSNAPLINE), "CONTROLLER");

                        Settings.Default.ActiveFuncKey = 0;
                        started = false;
                        Examples.Example.ActionControllerButtonClicked = false;
                        return;
                    }
                }
                //-----CONTROLLER TOGGLE SNAPLINE END-----

                //CONTROLLER TOGGLE VSAT
                else if (Settings.Default.ActiveFuncKey == 6)
                {
                    if (Examples.Example.ActionControllerButtonClicked == true)
                    {
                        label125.Text = kc.ConvertToString(Settings.Default.ControllerToggleVSAT);
                        button50.Text = "Change";
                        INIConfig.Write("TOGGLE_VSAT", kc.ConvertToString(Settings.Default.ControllerToggleVSAT), "CONTROLLER");

                        Settings.Default.ActiveFuncKey = 0;
                        started = false;
                        Examples.Example.ActionControllerButtonClicked = false;
                        return;
                    }
                }
                //-----CONTROLLER TOGGLE VSAT END-----

                //CONTROLLER TOGGLE CROSSHAIR
                else if (Settings.Default.ActiveFuncKey == 7)
                {
                    if (Examples.Example.ActionControllerButtonClicked == true)
                    {
                        label129.Text = kc.ConvertToString(Settings.Default.ControllerToggleCROSSHAIR);
                        button53.Text = "Change";
                        INIConfig.Write("TOGGLE_CROSSHAIR", kc.ConvertToString(Settings.Default.ControllerToggleCROSSHAIR), "CONTROLLER");

                        Settings.Default.ActiveFuncKey = 0;
                        started = false;
                        Examples.Example.ActionControllerButtonClicked = false;
                        return;
                    }
                }
                //-----CONTROLLER TOGGLE CROSSHAIR END-----

            }
        }

        private async Task GetKeybinding()
        {

            KeysConverter kc = new KeysConverter(); //Convert to int val to string val

            if (started == true)

            {
                return;
            }

            started = true;
            while (true)
            {
                await Task.Delay(1);

                for (int i = 0; i <= 999; i++)
                {

                    int keypressed = GetAsyncKeyState((Keys)i);
                    if (keypressed == -32767)
                    {
                        if (Settings.Default.ActiveFuncKey == 1)
                        {
                            Settings.Default.AimKey = kc.ConvertToString(i);
                            label43.Text = kc.ConvertToString(i);
                            button16.Text = "Change";
                            INIConfig.Write("AIM_Key", kc.ConvertToString(Settings.Default.AimKey), "AIMBOT");

                            if (customCheckBox4.Enabled == true && (Settings.Default.pfpf == 1) && Settings.Default.AimKey.ToString().ToLower() == "LButton".ToLower())
                            {
                                customCheckBox4.Ovalchecked = false;
                                customCheckBox4.Enabled = false;
                                label31.Text = "Disabled";
                            }
                            else
                            {
                                if (Settings.Default.AIMAutoFire == true)
                                {
                                    customCheckBox4.Ovalchecked = true;
                                    customCheckBox4.Enabled = true;
                                    label31.Text = "Enabled";
                                }
                                else if (customCheckBox4.Enabled == false)
                                {
                                    customCheckBox4.Enabled = true;
                                    label31.Text = "Disabled";
                                }
                            }
                        }
                        else if (Settings.Default.ActiveFuncKey == 2)
                        {
                            Settings.Default.Key_ShowMENU = (Keys)i;
                            label16.Text = kc.ConvertToString(i);
                            button15.Text = "Change";
                            INIConfig.Write("Show_Menu", kc.ConvertToString(Settings.Default.Key_ShowMENU), "SETTINGS");
                        }

                        if (keypressed == -1)
                        {
                            Settings.Default.ActiveFuncKey = 0;
                            started = false;
                            return;
                        }

                        Settings.Default.ActiveFuncKey = 0;
                        started = false;
                        return;
                    }
                }
            }
        }

        private void button15_Click(object sender, EventArgs e)
        {
            Settings.Default.ActiveFuncKey = 2;
            button15.Text = "Press...";
            GetKeybinding();
        }

        private void CustomCheckBox3_Click()
        {

            if (Settings.Default.OVERLAY == false)
            {
                Settings.Default.OVERLAY = true;
                label29.Text = "Showed";
            }
            else
            {
                Settings.Default.OVERLAY = false;
                label29.Text = "Hidden";
            }

            INIConfig.Write("Show_Overlay", Convert.ToString(Settings.Default.OVERLAY), "SETTINGS");

        }

        private void customCheckBox2_Ovalclick()
        {
            if (Settings.Default.Show_DISTANCE == false && Settings.Default.pfpf == 1)
            {
                Settings.Default.Show_DISTANCE = true;
                label41.Text = "Showed";
            }
            else
            {
                Settings.Default.Show_DISTANCE = false;
                label41.Text = "Hidden";
            }
            INIConfig.Write("Show_Distance", Convert.ToString(Settings.Default.Show_DISTANCE), "ESP");
        }

        private void customCheckBox1_Ovalclick_1()
        {

            //Security.CVP2();

            if (Settings.Default.AIMBOT == false && (Settings.Default.pfpf == 1))
            {
                Settings.Default.AIMBOT = true;
                INIConfig.Write("Enable_AIMBOT", "true", "AIMBOT");
            }
            else
            {
                Settings.Default.AIMBOT = false;
                INIConfig.Write("Enable_AIMBOT", "false", "AIMBOT");
            }

        }

        private void button16_Click(object sender, EventArgs e)
        {
            Settings.Default.ActiveFuncKey = 1;
            button16.Text = "Press...";
            GetKeybinding();
        }

        private void customCheckBox4_Ovalclick()
        {

            if (Settings.Default.AIMAutoFire == false && (Settings.Default.pfpf == 1) && Settings.Default.AimKey.ToString().ToLower() != "LButton".ToLower())
            {
                Settings.Default.AIMAutoFire = true;
                label31.Text = "Enabled";
            }
            else
            {
                Settings.Default.AIMAutoFire = false;
                label31.Text = "Disabled";
            }

            INIConfig.Write("AutoFire", Convert.ToString(Settings.Default.AIMAutoFire), "AIMBOT");

        }

        private void customCheckBox6_Ovalclick()
        {

            if (Settings.Default.CROSSHAIR == false)
            {
                Settings.Default.CROSSHAIR = true;
            }
            else
            {
                Settings.Default.CROSSHAIR = false;
            }

            INIConfig.Write("Enable_CROSSHAIR", Convert.ToString(Settings.Default.CROSSHAIR), "MISC");

        }

        private void metroSetTrackBar2_Scroll_1(object sender)
        {
            Settings.Default.CROSSHAIRSize = metroSetTrackBar2.Value;
            label52.Text = metroSetTrackBar2.Value.ToString();
            INIConfig.Write("Size", Convert.ToString(Settings.Default.CROSSHAIRSize), "MISC");
        }

        private void metroSetTrackBar5_Scroll(object sender)
        {
            Settings.Default.CROSSHAIRThickness = metroSetTrackBar5.Value;
            label55.Text = metroSetTrackBar5.Value.ToString();
            INIConfig.Write("Thickness", Convert.ToString(Settings.Default.CROSSHAIRThickness), "MISC");
        }

        private void metroSetEllipse5_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();

            Settings.Default.CROSSHAIRColor = colorDialog1.Color;

            label47.Text = Settings.Default.CROSSHAIRColor.Name;

            metroSetEllipse5.NormalColor = colorDialog1.Color;
            metroSetEllipse5.HoverColor = colorDialog1.Color;

            INIConfig.Write("Crosshair_Color", ConvertColor.ConvertToString(Settings.Default.CROSSHAIRColor.ToArgb()), "STYLES");
        }

        private void metroSetEllipse4_Click(object sender, EventArgs e)
        {

            MetroSet_UI.Controls.MetroSetEllipse metroSetEllipse = (MetroSet_UI.Controls.MetroSetEllipse)sender;

            if (metroSetEllipse.Name == metroSetEllipse4.Name)
            {
                Settings.Default.AIMBOTTarget = 1;
                label34.Text = "HEAD";

                metroSetEllipse2.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipse3.NormalColor = Settings.Default.DefaultColor;
            }

            else if (metroSetEllipse.Name == metroSetEllipse2.Name)
            {
                Settings.Default.AIMBOTTarget = 2;
                label34.Text = "BODY";

                metroSetEllipse4.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipse3.NormalColor = Settings.Default.DefaultColor;
            }

            else if (metroSetEllipse.Name == metroSetEllipse3.Name)
            {
                Settings.Default.AIMBOTTarget = 3;
                label34.Text = "FOOT";

                metroSetEllipse4.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipse2.NormalColor = Settings.Default.DefaultColor;
            }

            metroSetEllipse.NormalColor = Settings.Default.MenuBorderColor;
            metroSetEllipse.HoverColor = Settings.Default.MenuBorderColor;
            INIConfig.Write("Target", Convert.ToString(Settings.Default.AIMBOTTarget), "AIMBOT");
        }

        private void metroSetTrackBar4_Scroll(object sender)
        {
            Settings.Default.AIMBOTAcceleration = metroSetTrackBar4.Value;
            label36.Text = Settings.Default.AIMBOTAcceleration.ToString();
            INIConfig.Write("Acceleration", Convert.ToString(Settings.Default.AIMBOTAcceleration), "AIMBOT");
        }

        private void button21_Click(object sender, EventArgs e)
        {
            File.WriteAllBytes("profiles/" + Settings.Default.Profiles + "/config.ini", Resources.config);
            InitializationCONTROLS();
            label45.Text = "Done";
        }

        private void metroSetTrackBar6_Scroll(object sender)
        {
            Settings.Default.AIMBOTFOV = metroSetTrackBar6.Value;
            label57.Text = Settings.Default.AIMBOTFOV.ToString();
            INIConfig.Write("Aim_FOV", Convert.ToString(Settings.Default.AIMBOTFOV), "AIMBOT");
        }

        private void customCheckBox7_Ovalclick()
        {
            if (Settings.Default.ShowAIMFov == false && (Settings.Default.pfpf == 1))
            {
                Settings.Default.ShowAIMFov = true;
                label59.Text = "Enabled";
            }
            else
            {
                Settings.Default.ShowAIMFov = false;
                label59.Text = "Disabled";
            }

            INIConfig.Write("Show_Aim_Fov", Convert.ToString(Settings.Default.ShowAIMFov), "AIMBOT");
        }

        private void button25_Click(object sender, EventArgs e)
        {
            Process.Start("https://vecrproject.com");
        }

        private void button25_MouseEnter(object sender, EventArgs e)
        {
            button25.BackgroundImage.Dispose();
            button25.BackgroundImage = RL.Properties.Resources._256x256_ME;
        }

        private void button25_MouseLeave(object sender, EventArgs e)
        {
            button25.BackgroundImage.Dispose();
            button25.BackgroundImage = RL.Properties.Resources._256x256;
        }

        private void metroSetTrackBar7_Scroll(object sender)
        {
            Settings.Default.FPS = metroSetTrackBar7.Value;
            label67.Text = metroSetTrackBar7.Value.ToString();
            if (metroSetTrackBar7.Value >= 999)
            {
                label67.Text = "Max";
            }
            INIConfig.Write("FPS", Convert.ToString(Settings.Default.FPS), "SETTINGS");
        }

        private void customCheckBox8_Ovalclick()
        {
            if (Settings.Default.ShowFPS == false)
            {
                Settings.Default.ShowFPS = true;
                label69.Text = "Showed";
            }
            else
            {
                Settings.Default.ShowFPS = false;
                label69.Text = "Hidden";
            }

            INIConfig.Write("Show_FPS", Convert.ToString(Settings.Default.ShowFPS), "SETTINGS");
        }

        private void customCheckBox9_Ovalclick()
        {
            if (Settings.Default.VSAT == false)
            {
                Settings.Default.VSAT = true;
            }
            else
            {
                Settings.Default.VSAT = false;
            }

            INIConfig.Write("Enable_VSAT", Convert.ToString(Settings.Default.VSAT), "MISC");
        }

        private void customCheckBox10_Ovalclick()
        {
            if (Settings.Default.ShowAllies == false)
            {
                Settings.Default.ShowAllies = true;
                label72.Text = "Showed";
            }
            else
            {
                Settings.Default.ShowAllies = false;
                label72.Text = "Hidden";
            }

            INIConfig.Write("Show_Allies", Convert.ToString(Settings.Default.ShowAllies), "ESP");
        }

        private void customCheckBox11_Ovalclick()
        {
            if (Settings.Default.Show_Name == false && Settings.Default.pfpf == 1)
            {
                Settings.Default.Show_Name = true;
                label74.Text = "Showed";
            }
            else
            {
                Settings.Default.Show_Name = false;
                label74.Text = "Hidden";
            }
            INIConfig.Write("Show_Name", Convert.ToString(Settings.Default.Show_Name), "ESP");
        }

        private void customCheckBox12_Ovalclick()
        {
            if (Settings.Default.VSYNC == false)
            {
                Settings.Default.VSYNC = true;

                if (label76.Text == "Restart required")
                {
                    label76.Font = new Font("Microsoft Sans Serif", 11, FontStyle.Bold);
                    label76.Text = "Enabled";
                }
                else
                {
                    label76.Font = new Font("Microsoft Sans Serif", 8, FontStyle.Bold);
                    label76.Text = "Restart required";
                }
            }
            else
            {
                Settings.Default.VSYNC = false;

                if (label76.Text == "Restart required")
                {
                    label76.Font = new Font("Microsoft Sans Serif", 11, FontStyle.Bold);
                    label76.Text = "Disabled";
                }
                else
                {
                    label76.Font = new Font("Microsoft Sans Serif", 8, FontStyle.Bold);
                    label76.Text = "Restart required";
                }
            }
            INIConfig.Write("VSYNC", Convert.ToString(Settings.Default.VSYNC), "SETTINGS");
        }

        private void metroSetEllipse8_Click(object sender, EventArgs e)
        {
            MetroSet_UI.Controls.MetroSetEllipse metroSetEllipse = (MetroSet_UI.Controls.MetroSetEllipse)sender;

            if (metroSetEllipse.Name == metroSetEllipse8.Name)
            {
                Settings.Default.AIMBOT_PRIORITY = 1;
                label12.Text = "CROSSHAIR";

                metroSetEllipse7.NormalColor = Settings.Default.DefaultColor;
            }

            else if (metroSetEllipse.Name == metroSetEllipse7.Name)
            {
                Settings.Default.AIMBOT_PRIORITY = 2;
                label12.Text = "DISTANCE";

                metroSetEllipse8.NormalColor = Settings.Default.DefaultColor;
            }

            metroSetEllipse.NormalColor = Settings.Default.MenuBorderColor;
            metroSetEllipse.HoverColor = Settings.Default.MenuBorderColor;
            INIConfig.Write("Priority", Convert.ToString(Settings.Default.AIMBOT_PRIORITY), "AIMBOT");
        }

        private void metroSetEllipse6_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();

            Settings.Default.AIMBOT_FOV_Color = colorDialog1.Color;

            label78.Text = Settings.Default.AIMBOT_FOV_Color.Name;

            metroSetEllipse6.NormalColor = colorDialog1.Color;
            metroSetEllipse6.HoverColor = colorDialog1.Color;

            INIConfig.Write("FOV_Color", ConvertColor.ConvertToString(Settings.Default.AIMBOT_FOV_Color.ToArgb()), "STYLES");
        }

        private void metroSetTrackBar3_Scroll(object sender)
        {
            Settings.Default.ESP_SIZE_TEXT = metroSetTrackBar3.Value;
            label82.Text = metroSetTrackBar3.Value.ToString();
            INIConfig.Write("Text_Size", Convert.ToString(Settings.Default.ESP_SIZE_TEXT), "MISC");
        }

        private void metroSetEllipse11_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();

            Settings.Default.ESPALLIESCOLOR = colorDialog1.Color;

            label90.Text = Settings.Default.ESPALLIESCOLOR.Name;

            metroSetEllipse11.NormalColor = colorDialog1.Color;
            metroSetEllipse11.HoverColor = colorDialog1.Color;

            INIConfig.Write("Allies_Color", ConvertColor.ConvertToString(Settings.Default.ESPALLIESCOLOR.ToArgb()), "STYLES");
        }

        private void metroSetEllipse10_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();

            Settings.Default.ESPFILLEDBORDERCOLOR = colorDialog1.Color;

            label88.Text = Settings.Default.ESPFILLEDBORDERCOLOR.Name;

            metroSetEllipse10.NormalColor = colorDialog1.Color;
            metroSetEllipse10.HoverColor = colorDialog1.Color;

            INIConfig.Write("Filled_Border_Color", ConvertColor.ConvertToString(Settings.Default.ESPFILLEDBORDERCOLOR.ToArgb()), "STYLES");
        }

        private void button31_Click(object sender, EventArgs e)
        {

            if (button31.Text == "Load")
            {
                Settings.Default.Profiles = metroSetComboBox1.Text;
                INIConfig = new Clsini("profiles/" + Settings.Default.Profiles + "/config.ini");
                INIBestProfiles.Write("Best", ConvertColor.ConvertToString(metroSetComboBox1.Text), "STARTUP");
                InitializationCONTROLS();

                label45.Text = "Loaded";
            }
            else if (button31.Text == "Apply and Save")
            {
                Settings.Default.Profiles = textBox1.Text;
                INIConfig = new Clsini("profiles/" + Settings.Default.Profiles + "/config.ini");

                Directory.CreateDirectory("profiles/" + Settings.Default.Profiles);
                File.WriteAllBytes("profiles/" + Settings.Default.Profiles + "/config.ini", Resources.config);
                INIBestProfiles.Write("Best", ConvertColor.ConvertToString(textBox1.Text), "STARTUP");
                InitializationCONTROLS();

                label45.Text = "Saved";
                button31.Size = new Size(78, 24);
                button31.Text = "Load";
                metroSetComboBox1.Visible = true;
                button30.Visible = true;
                textBox1.Visible = false;
                textBox1.Clear();
            }
        }

        private void metroSetComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int last = metroSetComboBox1.Items.Count - 1;
            if (metroSetComboBox1.SelectedIndex == last)
            {
                metroSetComboBox1.Visible = false;
                button30.Visible = false;
                textBox1.Visible = true;
                button31.Size = new Size(164, 24);
                button31.Text = "Apply and Save";
            }
        }

        private void button30_Click(object sender, EventArgs e)
        {
            try
            {
                string Dir = @"profiles\" + metroSetComboBox1.Text;
                Directory.Delete(Dir, true);
                InitializationCONTROLS();

                label45.Text = "Done";
            }
            catch { label45.Text = "Error"; }
        }

        private void customCheckBox5_Ovalclick()
        {
            if (Settings.Default.Show_PING == false && Settings.Default.pfpf == 1)
            {
                Settings.Default.Show_PING = true;
                label96.Text = "Showed";
            }
            else
            {
                Settings.Default.Show_PING = false;
                label96.Text = "Hidden";
            }
            INIConfig.Write("Show_Ping", Convert.ToString(Settings.Default.Show_PING), "ESP");
        }

        private void customCheckBox13_Ovalclick()
        {
            if (Settings.Default.Show_Text_Border == false && Settings.Default.pfpf == 1)
            {
                Settings.Default.Show_Text_Border = true;
                label98.Text = "Showed";
            }
            else
            {
                Settings.Default.Show_Text_Border = false;
                label98.Text = "Hidden";
            }
            INIConfig.Write("Show_Text_Border", Convert.ToString(Settings.Default.Show_Text_Border), "MISC");
        }

        private void metroSetEllipse15_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();

            Settings.Default.MenuBorderColor = colorDialog1.Color;

            label103.Text = Settings.Default.AIMBOT_FOV_Color.Name;

            metroSetEllipse15.NormalColor = colorDialog1.Color;
            metroSetEllipse15.HoverColor = colorDialog1.Color;

            INIConfig.Write("Menu_Color", ConvertColor.ConvertToString(Settings.Default.MenuBorderColor.ToArgb()), "STYLES");

            InitializationCONTROLS();

        }

        private void metroSetEllipse9_Click(object sender, EventArgs e)
        {
            MetroSet_UI.Controls.MetroSetEllipse metroSetEllipse = (MetroSet_UI.Controls.MetroSetEllipse)sender;

            if (metroSetEllipse.Name == metroSetEllipse9.Name)
            {
                Settings.Default.FPS_CORNER = 1;
                label101.Text = "TOP L";

                metroSetEllipse12.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipse13.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipse14.NormalColor = Settings.Default.DefaultColor;
            }

            if (metroSetEllipse.Name == metroSetEllipse14.Name)
            {
                Settings.Default.FPS_CORNER = 2;
                label101.Text = "TOP R";

                metroSetEllipse12.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipse13.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipse9.NormalColor = Settings.Default.DefaultColor;
            }

            if (metroSetEllipse.Name == metroSetEllipse12.Name)
            {
                Settings.Default.FPS_CORNER = 3;
                label101.Text = "BOT L";

                metroSetEllipse9.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipse13.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipse14.NormalColor = Settings.Default.DefaultColor;
            }

            if (metroSetEllipse.Name == metroSetEllipse13.Name)
            {
                Settings.Default.FPS_CORNER = 4;
                label101.Text = "BOT R";

                metroSetEllipse12.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipse9.NormalColor = Settings.Default.DefaultColor;
                metroSetEllipse14.NormalColor = Settings.Default.DefaultColor;
            }

            metroSetEllipse.NormalColor = Settings.Default.MenuBorderColor;
            metroSetEllipse.HoverColor = Settings.Default.MenuBorderColor;
            INIConfig.Write("FPS_Corner", Convert.ToString(Settings.Default.FPS_CORNER), "SETTINGS");
        }

        private void metroSetTrackBar8_Scroll(object sender)
        {
            Settings.Default.AUTO_FIRE_MS = metroSetTrackBar8.Value;
            label105.Text = Settings.Default.AUTO_FIRE_MS.ToString() + " MS";
            INIConfig.Write("AutoFireMS", Convert.ToString(Settings.Default.AUTO_FIRE_MS), "AIMBOT");
        }

        private void metroSetEllipse16_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();

            Settings.Default.PlayerNameColor = colorDialog1.Color;

            label66.Text = Settings.Default.PlayerNameColor.Name;

            metroSetEllipse16.NormalColor = colorDialog1.Color;
            metroSetEllipse16.HoverColor = colorDialog1.Color;

            INIConfig.Write("Player_Name_Color", ConvertColor.ConvertToString(Settings.Default.PlayerNameColor.ToArgb()), "STYLES");
        }

        private void metroSetEllipse18_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();

            Settings.Default.PlayerPingColor = colorDialog1.Color;

            label110.Text = Settings.Default.PlayerPingColor.Name;

            metroSetEllipse18.NormalColor = colorDialog1.Color;
            metroSetEllipse18.HoverColor = colorDialog1.Color;

            INIConfig.Write("Player_Ping_Color", ConvertColor.ConvertToString(Settings.Default.PlayerPingColor.ToArgb()), "STYLES");
        }

        private void metroSetTrackBar9_Scroll(object sender)
        {
            Settings.Default.Menu_Opacity = metroSetTrackBar9.Value;
            label112.Text = metroSetTrackBar9.Value.ToString();
            if (metroSetTrackBar9.Value >= 100)
            {
                label112.Text = "Max";
            }
            INIConfig.Write("Menu_Opacity", Convert.ToString(Settings.Default.Menu_Opacity), "SETTINGS");
            double Result = Convert.ToInt32(Settings.Default.Menu_Opacity) / (double)100;
            this.Opacity = Result;
        }

        private void Menu_Shown(object sender, EventArgs e)
        {
            double Result = Convert.ToInt32(Settings.Default.Menu_Opacity) / (double)100;
            this.Opacity = Result;
        }

        private void button37_Click(object sender, EventArgs e)
        {
            fontDialog1.ShowDialog();

            Settings.Default.FontText = fontDialog1.Font.Name.ToString();
            INIConfig.Write("Font_Text", Convert.ToString(Settings.Default.FontText), "MISC");
            label114.Text = Settings.Default.FontText;

        }

        private void button41_Click(object sender, EventArgs e)
        {
            Settings.Default.ActiveFuncKey = 3;
            button41.Text = "Press...";
            GetKeyController();
        }

        private void customCheckBox1_Ovalclick_2()
        {
            if (Settings.Default.TextAntiAliasing == false)
            {
                Settings.Default.TextAntiAliasing = true;

                if (label92.Text == "Restart required")
                {
                    label92.Font = new Font("Microsoft Sans Serif", 11, FontStyle.Bold);
                    label92.Text = "Enabled";
                }
                else
                {
                    label92.Font = new Font("Microsoft Sans Serif", 8, FontStyle.Bold);
                    label92.Text = "Restart required";
                }
            }
            else
            {
                Settings.Default.TextAntiAliasing = false;

                if (label92.Text == "Restart required")
                {
                    label92.Font = new Font("Microsoft Sans Serif", 11, FontStyle.Bold);
                    label92.Text = "Disabled";
                }
                else
                {
                    label92.Font = new Font("Microsoft Sans Serif", 8, FontStyle.Bold);
                    label92.Text = "Restart required";
                }
            }

            INIConfig.Write("Anti_Aliasing", Convert.ToString(Settings.Default.TextAntiAliasing), "SETTINGS");
        }

        private void button44_Click(object sender, EventArgs e)
        {
            Settings.Default.ActiveFuncKey = 1;
            button44.Text = "Press...";
            GetKeyController();
        }

        private void buttonRemoveController_Click(object sender, EventArgs e)
        {
            KeysConverter kc = new KeysConverter(); //Convert to int val to string val

            Button button = (Button)sender;

            if (button48.Name == button.Name)
            {
                INIConfig.Write("AIM_Key", kc.ConvertToString("Unknown"), "CONTROLLER");
                Settings.Default.ControllerButtonAIMBOT = INIConfig.Read("AIM_Key", "CONTROLLER");
                label117.Text = kc.ConvertToString(Settings.Default.ControllerButtonAIMBOT);
            }
            else if (button45.Name == button.Name)
            {
                INIConfig.Write("TOGGLE_AIMBOT", kc.ConvertToString("Unknown"), "CONTROLLER");
                Settings.Default.ControllerToggleAIMBOT = INIConfig.Read("TOGGLE_AIMBOT", "CONTROLLER");
                label118.Text = kc.ConvertToString(Settings.Default.ControllerToggleAIMBOT);
            }
            else if (button46.Name == button.Name)
            {
                INIConfig.Write("TOGGLE_ESP", kc.ConvertToString("Unknown"), "CONTROLLER");
                Settings.Default.ControllerToggleESP = INIConfig.Read("TOGGLE_ESP", "CONTROLLER");
                label95.Text = kc.ConvertToString(Settings.Default.ControllerToggleAIMBOT);
            }
            else if (button47.Name == button.Name)
            {
                INIConfig.Write("TOGGLE_SNAPLINE", kc.ConvertToString("Unknown"), "CONTROLLER");
                Settings.Default.ControllerToggleSNAPLINE = INIConfig.Read("TOGGLE_SNAPLINE", "CONTROLLER");
                label121.Text = kc.ConvertToString(Settings.Default.ControllerToggleSNAPLINE);
            }
            else if (button49.Name == button.Name)
            {
                INIConfig.Write("TOGGLE_VSAT", kc.ConvertToString("Unknown"), "CONTROLLER");
                Settings.Default.ControllerToggleVSAT = INIConfig.Read("TOGGLE_VSAT", "CONTROLLER");
                label125.Text = kc.ConvertToString(Settings.Default.ControllerToggleVSAT);
            }
            else if (button51.Name == button.Name)
            {
                INIConfig.Write("TOGGLE_CROSSHAIR", kc.ConvertToString("Unknown"), "CONTROLLER");
                Settings.Default.ControllerToggleCROSSHAIR = INIConfig.Read("TOGGLE_CROSSHAIR", "CONTROLLER");
                label129.Text = kc.ConvertToString(Settings.Default.ControllerToggleCROSSHAIR);
            }
        }

        private void button42_Click(object sender, EventArgs e)
        {
            Settings.Default.ActiveFuncKey = 4;
            button42.Text = "Press...";
            GetKeyController();
        }

        private void button43_Click(object sender, EventArgs e)
        {
            Settings.Default.ActiveFuncKey = 5;
            button43.Text = "Press...";
            GetKeyController();
        }

        private void button50_Click(object sender, EventArgs e)
        {
            Settings.Default.ActiveFuncKey = 6;
            button50.Text = "Press...";
            GetKeyController();
        }

        private void button53_Click(object sender, EventArgs e)
        {
            Settings.Default.ActiveFuncKey = 7;
            button53.Text = "Press...";
            GetKeyController();
        }
    }
}
