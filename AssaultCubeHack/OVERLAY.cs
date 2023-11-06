using AssaultCubeHack;
using bbjuyhgazdf.Windows;
using Com.Okmer.GameController;
using RL.Properties;
using SharpDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Utilities;
using VECRPROJECT.game;
using VECRPROJECT.util;
using WindowsInput;
using Menu = AssaultCubeHack.Menu;

namespace Examples
{
    public class Example : IDisposable
    {

        public static int _oldtargetWindowPositionLeft;
        public static int _oldtargetWindowPositionTop;

        private volatile bool ButtonCustomClick;
        private Clsini INIConfig = new Clsini("profiles/" + Settings.Default.Profiles + "/config.ini");

        [DllImport("User32.dll")]
        public static extern short GetAsyncKeyState(Keys vKey); // Keys enumeration

        public static InputSimulator mousecontrol = new InputSimulator();

        //refresh windows
        public static int refreshWindows = 1500;

        //AIMBOT
        private bool aim = false;
        private Player SelfPos;
        public static volatile int SelfLocalTEAM = 0;
        public volatile float Totalpitch;
        public volatile float TotalYaw;
        public volatile float SelfPitch;
        public volatile float SelfYaw;

        public readonly static float HEAD = 10F; //1
        public readonly static float BODY = 20F; //2
        public readonly static float FOOT = 54F; //3

        public static float TARGET = BODY;

        //CONTROLLER
        public static XBoxController controller = new XBoxController();

        //target process
        public const string processName = "plutonium-bootstrapper-win32";
        public const string processGame = "Black Ops II - Zombies";
        public static string processMainApp = @"\r\n";
        public static string processDirectory;
        public static Process process;

        //threads for updating rendering
        private Thread overlayThread;
        private Thread windowPosThread;
        public static bool isRunning = false;

        //game objects
        private List<Player> players = new List<Player>();
        private int numPlayers;
        private Matrix viewMatrix;
        public static int gameWidth, gameHeight;

        //keyboard commands
        private GlobalKeyboardHook gkh = new GlobalKeyboardHook();

        //GRAPHIC
        private volatile GraphicsWindow _window;

        private readonly Dictionary<string, bbjuyhgazdf.Drawing.SolidBrush> _brushes;
        private readonly Dictionary<string, bbjuyhgazdf.Drawing.Font> _fonts;
        private readonly Dictionary<string, bbjuyhgazdf.Drawing.Image> _images;

        //MENU
        public static bool Menu_Showed = false;
        public static bool Menu_Loading = false;

        public Example()
        {
            //Security.CVP();
            //Security.security();

            if (Security.bpAcc == true && Settings.Default.pfpf == 1)
            {
                Menu menu = new Menu();
                menu.InitializationCONTROLS();
            }
            else
            {
                //fail
                Security.FAILACC();
            }

            _brushes = new Dictionary<string, bbjuyhgazdf.Drawing.SolidBrush>();
            _fonts = new Dictionary<string, bbjuyhgazdf.Drawing.Font>();
            _images = new Dictionary<string, bbjuyhgazdf.Drawing.Image>();

            var gfx = new bbjuyhgazdf.Drawing.Graphics()
            {
                MeasureFPS = Settings.Default.ShowFPS,
                PerPrimitiveAntiAliasing = true,
                TextAntiAliasing = Settings.Default.TextAntiAliasing,
                VSync = Settings.Default.VSYNC,
                UseMultiThreadedFactories = true,
            };

            _window = new GraphicsWindow(0, 0, gameWidth, gameHeight, gfx)
            {
                FPS = Settings.Default.FPS,
                IsTopmost = true,
                IsVisible = true,
            };

            //MYCODE
            AttachToGameProcess();

            _window.DrawGraphics += _window_DrawGraphics;
            _window.DestroyGraphics += _window_DestroyGraphics;
            _window.SetupGraphics += _window_SetupGraphics;

            //CONTROLLER

            //Connection
            controller.Connection.ValueChanged += (s, e) => Settings.Default.ControllerConnected = e.Value;

            //Buttons A, B, X, Y
            #region //A BUTTON
            controller.A.ValueChanged += (s, e) =>
            {
                string buttonName = "A";
                bool controllerValue = (Convert.ToBoolean(controller.A.Value));

                if (controllerValue == true && INIConfig.Read("AIM_Key", "CONTROLLER") == buttonName)
                {
                    Settings.Default.ControllerAIMBOTButtonPressed = true;
                }
                else if (controllerValue == false && INIConfig.Read("AIM_Key", "CONTROLLER") == buttonName)
                {
                    Settings.Default.ControllerAIMBOTButtonPressed = false;
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_AIMBOT", "CONTROLLER") == buttonName)
                {
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

                if (controllerValue == true && INIConfig.Read("TOGGLE_ESP", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.ESP == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.ESP = true;
                        INIConfig.Write("Enable_ESP", "true", "ESP");
                    }
                    else
                    {
                        Settings.Default.ESP = false;
                        INIConfig.Write("Enable_ESP", "false", "ESP");
                    }
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_SNAPLINE", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.SNAPLINE == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.SNAPLINE = true;
                        INIConfig.Write("Enable_SNAPLINE", "true", "SNAPLINE");
                    }
                    else
                    {
                        Settings.Default.SNAPLINE = false;
                        INIConfig.Write("Enable_SNAPLINE", "false", "SNAPLINE");
                    }
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_VSAT", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.VSAT == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.VSAT = true;
                        INIConfig.Write("Enable_VSAT", "true", "MISC");
                    }
                    else
                    {
                        Settings.Default.VSAT = false;
                        INIConfig.Write("Enable_VSAT", "false", "MISC");
                    }
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_CROSSHAIR", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.CROSSHAIR == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.CROSSHAIR = true;
                        INIConfig.Write("Enable_CROSSHAIR", "true", "MISC");
                    }
                    else
                    {
                        Settings.Default.CROSSHAIR = false;
                        INIConfig.Write("Enable_CROSSHAIR", "false", "MISC");
                    }
                }

                if (Settings.Default.ActiveFuncKey == 1 && controllerValue == false)
                {
                    ActionControllerButtonClicked = true;
                    Settings.Default.ControllerButtonAIMBOT = buttonName;
                }
                else if (Settings.Default.ActiveFuncKey == 3)
                {
                    Settings.Default.ControllerToggleAIMBOT = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 4)
                {
                    Settings.Default.ControllerToggleESP = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 5)
                {
                    Settings.Default.ControllerToggleSNAPLINE = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 6)
                {
                    Settings.Default.ControllerToggleVSAT = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 7)
                {
                    Settings.Default.ControllerToggleCROSSHAIR = buttonName;
                    ActionControllerButtonClicked = true;
                }
            };
            #endregion
            #region //B BUTTON
            controller.B.ValueChanged += (s, e) =>
            {
                string buttonName = "B";
                bool controllerValue = (Convert.ToBoolean(controller.B.Value));

                if (controllerValue == true && INIConfig.Read("AIM_Key", "CONTROLLER") == buttonName)
                {
                    Settings.Default.ControllerAIMBOTButtonPressed = true;
                }
                else if (controllerValue == false && INIConfig.Read("AIM_Key", "CONTROLLER") == buttonName)
                {
                    Settings.Default.ControllerAIMBOTButtonPressed = false;
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_AIMBOT", "CONTROLLER") == buttonName)
                {
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

                if (controllerValue == true && INIConfig.Read("TOGGLE_ESP", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.ESP == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.ESP = true;
                        INIConfig.Write("Enable_ESP", "true", "ESP");
                    }
                    else
                    {
                        Settings.Default.ESP = false;
                        INIConfig.Write("Enable_ESP", "false", "ESP");
                    }
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_SNAPLINE", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.SNAPLINE == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.SNAPLINE = true;
                        INIConfig.Write("Enable_SNAPLINE", "true", "SNAPLINE");
                    }
                    else
                    {
                        Settings.Default.SNAPLINE = false;
                        INIConfig.Write("Enable_SNAPLINE", "false", "SNAPLINE");
                    }
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_VSAT", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.VSAT == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.VSAT = true;
                        INIConfig.Write("Enable_VSAT", "true", "MISC");
                    }
                    else
                    {
                        Settings.Default.VSAT = false;
                        INIConfig.Write("Enable_VSAT", "false", "MISC");
                    }
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_CROSSHAIR", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.CROSSHAIR == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.CROSSHAIR = true;
                        INIConfig.Write("Enable_CROSSHAIR", "true", "MISC");
                    }
                    else
                    {
                        Settings.Default.CROSSHAIR = false;
                        INIConfig.Write("Enable_CROSSHAIR", "false", "MISC");
                    }
                }

                if (Settings.Default.ActiveFuncKey == 1 && controllerValue == false)
                {
                    ActionControllerButtonClicked = true;
                    Settings.Default.ControllerButtonAIMBOT = buttonName;
                }
                else if (Settings.Default.ActiveFuncKey == 3)
                {
                    Settings.Default.ControllerToggleAIMBOT = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 4)
                {
                    Settings.Default.ControllerToggleESP = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 5)
                {
                    Settings.Default.ControllerToggleSNAPLINE = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 6)
                {
                    Settings.Default.ControllerToggleVSAT = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 7)
                {
                    Settings.Default.ControllerToggleCROSSHAIR = buttonName;
                    ActionControllerButtonClicked = true;
                }
            };
            #endregion
            #region //X BUTTON
            controller.X.ValueChanged += (s, e) =>
            {
                string buttonName = "X";
                bool controllerValue = (Convert.ToBoolean(controller.X.Value));

                if (controllerValue == true && INIConfig.Read("AIM_Key", "CONTROLLER") == buttonName)
                {
                    Settings.Default.ControllerAIMBOTButtonPressed = true;
                }
                else if (controllerValue == false && INIConfig.Read("AIM_Key", "CONTROLLER") == buttonName)
                {
                    Settings.Default.ControllerAIMBOTButtonPressed = false;
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_AIMBOT", "CONTROLLER") == buttonName)
                {
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

                if (controllerValue == true && INIConfig.Read("TOGGLE_ESP", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.ESP == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.ESP = true;
                        INIConfig.Write("Enable_ESP", "true", "ESP");
                    }
                    else
                    {
                        Settings.Default.ESP = false;
                        INIConfig.Write("Enable_ESP", "false", "ESP");
                    }
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_SNAPLINE", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.SNAPLINE == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.SNAPLINE = true;
                        INIConfig.Write("Enable_SNAPLINE", "true", "SNAPLINE");
                    }
                    else
                    {
                        Settings.Default.SNAPLINE = false;
                        INIConfig.Write("Enable_SNAPLINE", "false", "SNAPLINE");
                    }
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_VSAT", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.VSAT == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.VSAT = true;
                        INIConfig.Write("Enable_VSAT", "true", "MISC");
                    }
                    else
                    {
                        Settings.Default.VSAT = false;
                        INIConfig.Write("Enable_VSAT", "false", "MISC");
                    }
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_CROSSHAIR", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.CROSSHAIR == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.CROSSHAIR = true;
                        INIConfig.Write("Enable_CROSSHAIR", "true", "MISC");
                    }
                    else
                    {
                        Settings.Default.CROSSHAIR = false;
                        INIConfig.Write("Enable_CROSSHAIR", "false", "MISC");
                    }
                }

                if (Settings.Default.ActiveFuncKey == 1 && controllerValue == false)
                {
                    ActionControllerButtonClicked = true;
                    Settings.Default.ControllerButtonAIMBOT = buttonName;
                }
                else if (Settings.Default.ActiveFuncKey == 3)
                {
                    Settings.Default.ControllerToggleAIMBOT = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 4)
                {
                    Settings.Default.ControllerToggleESP = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 5)
                {
                    Settings.Default.ControllerToggleSNAPLINE = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 6)
                {
                    Settings.Default.ControllerToggleVSAT = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 7)
                {
                    Settings.Default.ControllerToggleCROSSHAIR = buttonName;
                    ActionControllerButtonClicked = true;
                }
            };
            #endregion
            #region //Y BUTTON
            controller.Y.ValueChanged += async (s, e) =>
            {
                string buttonName = "Y";
                bool controllerValue = (Convert.ToBoolean(controller.Y.Value));

                if (controllerValue == true && INIConfig.Read("AIM_Key", "CONTROLLER") == buttonName)
                {
                    Settings.Default.ControllerAIMBOTButtonPressed = true;
                }
                else if (controllerValue == false && INIConfig.Read("AIM_Key", "CONTROLLER") == buttonName)
                {
                    Settings.Default.ControllerAIMBOTButtonPressed = false;
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_AIMBOT", "CONTROLLER") == buttonName)
                {
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

                if (controllerValue == true && INIConfig.Read("TOGGLE_ESP", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.ESP == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.ESP = true;
                        INIConfig.Write("Enable_ESP", "true", "ESP");
                    }
                    else
                    {
                        Settings.Default.ESP = false;
                        INIConfig.Write("Enable_ESP", "false", "ESP");
                    }
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_SNAPLINE", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.SNAPLINE == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.SNAPLINE = true;
                        INIConfig.Write("Enable_SNAPLINE", "true", "SNAPLINE");
                    }
                    else
                    {
                        Settings.Default.SNAPLINE = false;
                        INIConfig.Write("Enable_SNAPLINE", "false", "SNAPLINE");
                    }
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_VSAT", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.VSAT == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.VSAT = true;
                        INIConfig.Write("Enable_VSAT", "true", "MISC");
                    }
                    else
                    {
                        Settings.Default.VSAT = false;
                        INIConfig.Write("Enable_VSAT", "false", "MISC");
                    }
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_CROSSHAIR", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.CROSSHAIR == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.CROSSHAIR = true;
                        INIConfig.Write("Enable_CROSSHAIR", "true", "MISC");
                    }
                    else
                    {
                        Settings.Default.CROSSHAIR = false;
                        INIConfig.Write("Enable_CROSSHAIR", "false", "MISC");
                    }
                }

                if (Settings.Default.ActiveFuncKey == 1 && controllerValue == false)
                {
                    ActionControllerButtonClicked = true;
                    Settings.Default.ControllerButtonAIMBOT = buttonName;
                }
                else if (Settings.Default.ActiveFuncKey == 3)
                {
                    Settings.Default.ControllerToggleAIMBOT = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 4)
                {
                    Settings.Default.ControllerToggleESP = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 5)
                {
                    Settings.Default.ControllerToggleSNAPLINE = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 6)
                {
                    Settings.Default.ControllerToggleVSAT = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 7)
                {
                    Settings.Default.ControllerToggleCROSSHAIR = buttonName;
                    ActionControllerButtonClicked = true;
                }
            };
            #endregion

            ////Buttons Start, Back
            #region //START BUTTON
            controller.Start.ValueChanged += (s, e) =>
            {
                string buttonName = "START";
                bool controllerValue = (Convert.ToBoolean(controller.Start.Value));

                if (controllerValue == true && INIConfig.Read("AIM_Key", "CONTROLLER") == buttonName)
                {
                    Settings.Default.ControllerAIMBOTButtonPressed = true;
                }
                else if (controllerValue == false && INIConfig.Read("AIM_Key", "CONTROLLER") == buttonName)
                {
                    Settings.Default.ControllerAIMBOTButtonPressed = false;
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_AIMBOT", "CONTROLLER") == buttonName)
                {
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

                if (controllerValue == true && INIConfig.Read("TOGGLE_ESP", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.ESP == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.ESP = true;
                        INIConfig.Write("Enable_ESP", "true", "ESP");
                    }
                    else
                    {
                        Settings.Default.ESP = false;
                        INIConfig.Write("Enable_ESP", "false", "ESP");
                    }
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_SNAPLINE", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.SNAPLINE == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.SNAPLINE = true;
                        INIConfig.Write("Enable_SNAPLINE", "true", "SNAPLINE");
                    }
                    else
                    {
                        Settings.Default.SNAPLINE = false;
                        INIConfig.Write("Enable_SNAPLINE", "false", "SNAPLINE");
                    }
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_VSAT", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.VSAT == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.VSAT = true;
                        INIConfig.Write("Enable_VSAT", "true", "MISC");
                    }
                    else
                    {
                        Settings.Default.VSAT = false;
                        INIConfig.Write("Enable_VSAT", "false", "MISC");
                    }
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_CROSSHAIR", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.CROSSHAIR == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.CROSSHAIR = true;
                        INIConfig.Write("Enable_CROSSHAIR", "true", "MISC");
                    }
                    else
                    {
                        Settings.Default.CROSSHAIR = false;
                        INIConfig.Write("Enable_CROSSHAIR", "false", "MISC");
                    }
                }

                if (Settings.Default.ActiveFuncKey == 1 && controllerValue == false)
                {
                    ActionControllerButtonClicked = true;
                    Settings.Default.ControllerButtonAIMBOT = buttonName;
                }
                else if (Settings.Default.ActiveFuncKey == 3)
                {
                    Settings.Default.ControllerToggleAIMBOT = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 4)
                {
                    Settings.Default.ControllerToggleESP = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 5)
                {
                    Settings.Default.ControllerToggleSNAPLINE = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 6)
                {
                    Settings.Default.ControllerToggleVSAT = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 7)
                {
                    Settings.Default.ControllerToggleCROSSHAIR = buttonName;
                    ActionControllerButtonClicked = true;
                }
            };
            #endregion
            #region //SELECT BUTTON
            controller.Back.ValueChanged += (s, e) =>
            {
                string buttonName = "SELECT";
                bool controllerValue = (Convert.ToBoolean(controller.Back.Value));

                if (controllerValue == true && INIConfig.Read("AIM_Key", "CONTROLLER") == buttonName)
                {
                    Settings.Default.ControllerAIMBOTButtonPressed = true;
                }
                else if (controllerValue == false && INIConfig.Read("AIM_Key", "CONTROLLER") == buttonName)
                {
                    Settings.Default.ControllerAIMBOTButtonPressed = false;
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_AIMBOT", "CONTROLLER") == buttonName)
                {
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

                if (controllerValue == true && INIConfig.Read("TOGGLE_ESP", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.ESP == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.ESP = true;
                        INIConfig.Write("Enable_ESP", "true", "ESP");
                    }
                    else
                    {
                        Settings.Default.ESP = false;
                        INIConfig.Write("Enable_ESP", "false", "ESP");
                    }
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_SNAPLINE", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.SNAPLINE == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.SNAPLINE = true;
                        INIConfig.Write("Enable_SNAPLINE", "true", "SNAPLINE");
                    }
                    else
                    {
                        Settings.Default.SNAPLINE = false;
                        INIConfig.Write("Enable_SNAPLINE", "false", "SNAPLINE");
                    }
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_VSAT", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.VSAT == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.VSAT = true;
                        INIConfig.Write("Enable_VSAT", "true", "MISC");
                    }
                    else
                    {
                        Settings.Default.VSAT = false;
                        INIConfig.Write("Enable_VSAT", "false", "MISC");
                    }
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_CROSSHAIR", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.CROSSHAIR == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.CROSSHAIR = true;
                        INIConfig.Write("Enable_CROSSHAIR", "true", "MISC");
                    }
                    else
                    {
                        Settings.Default.CROSSHAIR = false;
                        INIConfig.Write("Enable_CROSSHAIR", "false", "MISC");
                    }
                }

                if (Settings.Default.ActiveFuncKey == 1 && controllerValue == false)
                {
                    ActionControllerButtonClicked = true;
                    Settings.Default.ControllerButtonAIMBOT = buttonName;
                }
                else if (Settings.Default.ActiveFuncKey == 3)
                {
                    Settings.Default.ControllerToggleAIMBOT = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 4)
                {
                    Settings.Default.ControllerToggleESP = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 5)
                {
                    Settings.Default.ControllerToggleSNAPLINE = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 6)
                {
                    Settings.Default.ControllerToggleVSAT = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 7)
                {
                    Settings.Default.ControllerToggleCROSSHAIR = buttonName;
                    ActionControllerButtonClicked = true;
                }
            };
            #endregion
            ////Buttons D-Pad Up, Down, Left, Right
            #region //D-Pad Up BUTTON
            controller.Up.ValueChanged += (s, e) =>
            {
                string buttonName = "Up";
                bool controllerValue = (Convert.ToBoolean(controller.Up.Value));

                if (controllerValue == true && INIConfig.Read("AIM_Key", "CONTROLLER") == buttonName)
                {
                    Settings.Default.ControllerAIMBOTButtonPressed = true;
                }
                else if (controllerValue == false && INIConfig.Read("AIM_Key", "CONTROLLER") == buttonName)
                {
                    Settings.Default.ControllerAIMBOTButtonPressed = false;
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_AIMBOT", "CONTROLLER") == buttonName)
                {
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

                if (controllerValue == true && INIConfig.Read("TOGGLE_ESP", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.ESP == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.ESP = true;
                        INIConfig.Write("Enable_ESP", "true", "ESP");
                    }
                    else
                    {
                        Settings.Default.ESP = false;
                        INIConfig.Write("Enable_ESP", "false", "ESP");
                    }
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_SNAPLINE", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.SNAPLINE == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.SNAPLINE = true;
                        INIConfig.Write("Enable_SNAPLINE", "true", "SNAPLINE");
                    }
                    else
                    {
                        Settings.Default.SNAPLINE = false;
                        INIConfig.Write("Enable_SNAPLINE", "false", "SNAPLINE");
                    }
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_VSAT", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.VSAT == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.VSAT = true;
                        INIConfig.Write("Enable_VSAT", "true", "MISC");
                    }
                    else
                    {
                        Settings.Default.VSAT = false;
                        INIConfig.Write("Enable_VSAT", "false", "MISC");
                    }
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_CROSSHAIR", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.CROSSHAIR == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.CROSSHAIR = true;
                        INIConfig.Write("Enable_CROSSHAIR", "true", "MISC");
                    }
                    else
                    {
                        Settings.Default.CROSSHAIR = false;
                        INIConfig.Write("Enable_CROSSHAIR", "false", "MISC");
                    }
                }

                if (Settings.Default.ActiveFuncKey == 1 && controllerValue == false)
                {
                    ActionControllerButtonClicked = true;
                    Settings.Default.ControllerButtonAIMBOT = buttonName;
                }
                else if (Settings.Default.ActiveFuncKey == 3)
                {
                    Settings.Default.ControllerToggleAIMBOT = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 4)
                {
                    Settings.Default.ControllerToggleESP = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 5)
                {
                    Settings.Default.ControllerToggleSNAPLINE = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 6)
                {
                    Settings.Default.ControllerToggleVSAT = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 7)
                {
                    Settings.Default.ControllerToggleCROSSHAIR = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 7)
                {
                    Settings.Default.ControllerToggleCROSSHAIR = buttonName;
                    ActionControllerButtonClicked = true;
                }
            };
            #endregion
            #region //D-Pad Down BUTTON
            controller.Down.ValueChanged += (s, e) =>
            {
                string buttonName = "Down";
                bool controllerValue = (Convert.ToBoolean(controller.Down.Value));

                if (controllerValue == true && INIConfig.Read("AIM_Key", "CONTROLLER") == buttonName)
                {
                    Settings.Default.ControllerAIMBOTButtonPressed = true;
                }
                else if (controllerValue == false && INIConfig.Read("AIM_Key", "CONTROLLER") == buttonName)
                {
                    Settings.Default.ControllerAIMBOTButtonPressed = false;
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_AIMBOT", "CONTROLLER") == buttonName)
                {
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

                if (controllerValue == true && INIConfig.Read("TOGGLE_ESP", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.ESP == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.ESP = true;
                        INIConfig.Write("Enable_ESP", "true", "ESP");
                    }
                    else
                    {
                        Settings.Default.ESP = false;
                        INIConfig.Write("Enable_ESP", "false", "ESP");
                    }
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_SNAPLINE", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.SNAPLINE == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.SNAPLINE = true;
                        INIConfig.Write("Enable_SNAPLINE", "true", "SNAPLINE");
                    }
                    else
                    {
                        Settings.Default.SNAPLINE = false;
                        INIConfig.Write("Enable_SNAPLINE", "false", "SNAPLINE");
                    }
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_VSAT", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.VSAT == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.VSAT = true;
                        INIConfig.Write("Enable_VSAT", "true", "MISC");
                    }
                    else
                    {
                        Settings.Default.VSAT = false;
                        INIConfig.Write("Enable_VSAT", "false", "MISC");
                    }
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_CROSSHAIR", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.CROSSHAIR == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.CROSSHAIR = true;
                        INIConfig.Write("Enable_CROSSHAIR", "true", "MISC");
                    }
                    else
                    {
                        Settings.Default.CROSSHAIR = false;
                        INIConfig.Write("Enable_CROSSHAIR", "false", "MISC");
                    }
                }

                if (Settings.Default.ActiveFuncKey == 1 && controllerValue == false)
                {
                    ActionControllerButtonClicked = true;
                    Settings.Default.ControllerButtonAIMBOT = buttonName;
                }
                else if (Settings.Default.ActiveFuncKey == 3)
                {
                    Settings.Default.ControllerToggleAIMBOT = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 4)
                {
                    Settings.Default.ControllerToggleESP = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 5)
                {
                    Settings.Default.ControllerToggleSNAPLINE = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 6)
                {
                    Settings.Default.ControllerToggleVSAT = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 7)
                {
                    Settings.Default.ControllerToggleCROSSHAIR = buttonName;
                    ActionControllerButtonClicked = true;
                }
            };
            #endregion
            #region //D-Pad Left BUTTON
            controller.Left.ValueChanged += (s, e) =>
            {
                string buttonName = "Left";
                bool controllerValue = (Convert.ToBoolean(controller.Left.Value));

                if (controllerValue == true && INIConfig.Read("AIM_Key", "CONTROLLER") == buttonName)
                {
                    Settings.Default.ControllerAIMBOTButtonPressed = true;
                }
                else if (controllerValue == false && INIConfig.Read("AIM_Key", "CONTROLLER") == buttonName)
                {
                    Settings.Default.ControllerAIMBOTButtonPressed = false;
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_AIMBOT", "CONTROLLER") == buttonName)
                {
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

                if (controllerValue == true && INIConfig.Read("TOGGLE_ESP", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.ESP == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.ESP = true;
                        INIConfig.Write("Enable_ESP", "true", "ESP");
                    }
                    else
                    {
                        Settings.Default.ESP = false;
                        INIConfig.Write("Enable_ESP", "false", "ESP");
                    }
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_SNAPLINE", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.SNAPLINE == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.SNAPLINE = true;
                        INIConfig.Write("Enable_SNAPLINE", "true", "SNAPLINE");
                    }
                    else
                    {
                        Settings.Default.SNAPLINE = false;
                        INIConfig.Write("Enable_SNAPLINE", "false", "SNAPLINE");
                    }
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_VSAT", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.VSAT == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.VSAT = true;
                        INIConfig.Write("Enable_VSAT", "true", "MISC");
                    }
                    else
                    {
                        Settings.Default.VSAT = false;
                        INIConfig.Write("Enable_VSAT", "false", "MISC");
                    }
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_CROSSHAIR", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.CROSSHAIR == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.CROSSHAIR = true;
                        INIConfig.Write("Enable_CROSSHAIR", "true", "MISC");
                    }
                    else
                    {
                        Settings.Default.CROSSHAIR = false;
                        INIConfig.Write("Enable_CROSSHAIR", "false", "MISC");
                    }
                }

                if (Settings.Default.ActiveFuncKey == 1 && controllerValue == false)
                {
                    ActionControllerButtonClicked = true;
                    Settings.Default.ControllerButtonAIMBOT = buttonName;
                }
                else if (Settings.Default.ActiveFuncKey == 3)
                {
                    Settings.Default.ControllerToggleAIMBOT = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 4)
                {
                    Settings.Default.ControllerToggleESP = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 5)
                {
                    Settings.Default.ControllerToggleSNAPLINE = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 6)
                {
                    Settings.Default.ControllerToggleVSAT = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 7)
                {
                    Settings.Default.ControllerToggleCROSSHAIR = buttonName;
                    ActionControllerButtonClicked = true;
                }
            };
            #endregion
            #region //D-Pad Right BUTTON
            controller.Right.ValueChanged += (s, e) =>
            {
                string buttonName = "Right";
                bool controllerValue = (Convert.ToBoolean(controller.Right.Value));

                if (controllerValue == true && INIConfig.Read("AIM_Key", "CONTROLLER") == buttonName)
                {
                    Settings.Default.ControllerAIMBOTButtonPressed = true;
                }
                else if (controllerValue == false && INIConfig.Read("AIM_Key", "CONTROLLER") == buttonName)
                {
                    Settings.Default.ControllerAIMBOTButtonPressed = false;
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_AIMBOT", "CONTROLLER") == buttonName)
                {
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

                if (controllerValue == true && INIConfig.Read("TOGGLE_ESP", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.ESP == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.ESP = true;
                        INIConfig.Write("Enable_ESP", "true", "ESP");
                    }
                    else
                    {
                        Settings.Default.ESP = false;
                        INIConfig.Write("Enable_ESP", "false", "ESP");
                    }
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_SNAPLINE", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.SNAPLINE == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.SNAPLINE = true;
                        INIConfig.Write("Enable_SNAPLINE", "true", "SNAPLINE");
                    }
                    else
                    {
                        Settings.Default.SNAPLINE = false;
                        INIConfig.Write("Enable_SNAPLINE", "false", "SNAPLINE");
                    }
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_VSAT", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.VSAT == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.VSAT = true;
                        INIConfig.Write("Enable_VSAT", "true", "MISC");
                    }
                    else
                    {
                        Settings.Default.VSAT = false;
                        INIConfig.Write("Enable_VSAT", "false", "MISC");
                    }
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_CROSSHAIR", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.CROSSHAIR == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.CROSSHAIR = true;
                        INIConfig.Write("Enable_CROSSHAIR", "true", "MISC");
                    }
                    else
                    {
                        Settings.Default.CROSSHAIR = false;
                        INIConfig.Write("Enable_CROSSHAIR", "false", "MISC");
                    }
                }

                if (Settings.Default.ActiveFuncKey == 1 && controllerValue == false)
                {
                    ActionControllerButtonClicked = true;
                    Settings.Default.ControllerButtonAIMBOT = buttonName;
                }
                else if (Settings.Default.ActiveFuncKey == 3)
                {
                    Settings.Default.ControllerToggleAIMBOT = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 4)
                {
                    Settings.Default.ControllerToggleESP = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 5)
                {
                    Settings.Default.ControllerToggleSNAPLINE = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 6)
                {
                    Settings.Default.ControllerToggleVSAT = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 7)
                {
                    Settings.Default.ControllerToggleCROSSHAIR = buttonName;
                    ActionControllerButtonClicked = true;
                }
            };
            #endregion

            ////Buttons Shoulder Left, Right
            #region //L1 BUTTON
            controller.LeftShoulder.ValueChanged += (s, e) =>
            {
                string buttonName = "L1";
                bool controllerValue = (Convert.ToBoolean(controller.LeftShoulder.Value));

                if (controllerValue == true && INIConfig.Read("AIM_Key", "CONTROLLER") == buttonName)
                {
                    Settings.Default.ControllerAIMBOTButtonPressed = true;
                }
                else if (controllerValue == false && INIConfig.Read("AIM_Key", "CONTROLLER") == buttonName)
                {
                    Settings.Default.ControllerAIMBOTButtonPressed = false;
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_AIMBOT", "CONTROLLER") == buttonName)
                {
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

                if (controllerValue == true && INIConfig.Read("TOGGLE_ESP", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.ESP == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.ESP = true;
                        INIConfig.Write("Enable_ESP", "true", "ESP");
                    }
                    else
                    {
                        Settings.Default.ESP = false;
                        INIConfig.Write("Enable_ESP", "false", "ESP");
                    }
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_SNAPLINE", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.SNAPLINE == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.SNAPLINE = true;
                        INIConfig.Write("Enable_SNAPLINE", "true", "SNAPLINE");
                    }
                    else
                    {
                        Settings.Default.SNAPLINE = false;
                        INIConfig.Write("Enable_SNAPLINE", "false", "SNAPLINE");
                    }
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_VSAT", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.VSAT == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.VSAT = true;
                        INIConfig.Write("Enable_VSAT", "true", "MISC");
                    }
                    else
                    {
                        Settings.Default.VSAT = false;
                        INIConfig.Write("Enable_VSAT", "false", "MISC");
                    }
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_CROSSHAIR", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.CROSSHAIR == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.CROSSHAIR = true;
                        INIConfig.Write("Enable_CROSSHAIR", "true", "MISC");
                    }
                    else
                    {
                        Settings.Default.CROSSHAIR = false;
                        INIConfig.Write("Enable_CROSSHAIR", "false", "MISC");
                    }
                }

                if (Settings.Default.ActiveFuncKey == 1 && controllerValue == false)
                {
                    ActionControllerButtonClicked = true;
                    Settings.Default.ControllerButtonAIMBOT = buttonName;
                }
                else if (Settings.Default.ActiveFuncKey == 3)
                {
                    Settings.Default.ControllerToggleAIMBOT = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 4)
                {
                    Settings.Default.ControllerToggleESP = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 5)
                {
                    Settings.Default.ControllerToggleSNAPLINE = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 6)
                {
                    Settings.Default.ControllerToggleVSAT = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 7)
                {
                    Settings.Default.ControllerToggleCROSSHAIR = buttonName;
                    ActionControllerButtonClicked = true;
                }
            };
            #endregion
            #region //R1 BUTTON
            controller.RightShoulder.ValueChanged += (s, e) =>
            {
                string buttonName = "R1";
                bool controllerValue = (Convert.ToBoolean(controller.RightShoulder.Value));

                if (controllerValue == true && INIConfig.Read("AIM_Key", "CONTROLLER") == buttonName)
                {
                    Settings.Default.ControllerAIMBOTButtonPressed = true;
                }
                else if (controllerValue == false && INIConfig.Read("AIM_Key", "CONTROLLER") == buttonName)
                {
                    Settings.Default.ControllerAIMBOTButtonPressed = false;
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_AIMBOT", "CONTROLLER") == buttonName)
                {
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

                if (controllerValue == true && INIConfig.Read("TOGGLE_ESP", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.ESP == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.ESP = true;
                        INIConfig.Write("Enable_ESP", "true", "ESP");
                    }
                    else
                    {
                        Settings.Default.ESP = false;
                        INIConfig.Write("Enable_ESP", "false", "ESP");
                    }
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_SNAPLINE", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.SNAPLINE == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.SNAPLINE = true;
                        INIConfig.Write("Enable_SNAPLINE", "true", "SNAPLINE");
                    }
                    else
                    {
                        Settings.Default.SNAPLINE = false;
                        INIConfig.Write("Enable_SNAPLINE", "false", "SNAPLINE");
                    }
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_VSAT", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.VSAT == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.VSAT = true;
                        INIConfig.Write("Enable_VSAT", "true", "MISC");
                    }
                    else
                    {
                        Settings.Default.VSAT = false;
                        INIConfig.Write("Enable_VSAT", "false", "MISC");
                    }
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_CROSSHAIR", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.CROSSHAIR == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.CROSSHAIR = true;
                        INIConfig.Write("Enable_CROSSHAIR", "true", "MISC");
                    }
                    else
                    {
                        Settings.Default.CROSSHAIR = false;
                        INIConfig.Write("Enable_CROSSHAIR", "false", "MISC");
                    }
                }

                if (Settings.Default.ActiveFuncKey == 1 && controllerValue == false)
                {
                    ActionControllerButtonClicked = true;
                    Settings.Default.ControllerButtonAIMBOT = buttonName;
                }
                else if (Settings.Default.ActiveFuncKey == 3)
                {
                    Settings.Default.ControllerToggleAIMBOT = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 4)
                {
                    Settings.Default.ControllerToggleESP = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 5)
                {
                    Settings.Default.ControllerToggleSNAPLINE = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 6)
                {
                    Settings.Default.ControllerToggleVSAT = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 7)
                {
                    Settings.Default.ControllerToggleCROSSHAIR = buttonName;
                    ActionControllerButtonClicked = true;
                }
            };
            #endregion

            ////Buttons Thumb Left, Right
            //controller.LeftThumbclick.ValueChanged += (s, e) => val = Convert.ToBoolean(e.Value);
            //controller.RightThumbclick.ValueChanged += (s, e) => val = Convert.ToBoolean(e.Value);

            //Trigger Position Left, Right
            #region //L2 BUTTON
            controller.LeftTrigger.ValueChanged += (s, e) =>
            {
                string buttonName = "L2";
                bool controllerValue = (Convert.ToBoolean(controller.LeftTrigger.Value));

                if (controllerValue == true && INIConfig.Read("AIM_Key", "CONTROLLER") == buttonName)
                {
                    Settings.Default.ControllerAIMBOTButtonPressed = true;
                }
                else if (controllerValue == false && INIConfig.Read("AIM_Key", "CONTROLLER") == buttonName)
                {
                    Settings.Default.ControllerAIMBOTButtonPressed = false;
                }

                if (controllerValue == false && INIConfig.Read("TOGGLE_AIMBOT", "CONTROLLER") == buttonName)
                {
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

                if (controllerValue == false && INIConfig.Read("TOGGLE_ESP", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.ESP == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.ESP = true;
                        INIConfig.Write("Enable_ESP", "true", "ESP");
                    }
                    else
                    {
                        Settings.Default.ESP = false;
                        INIConfig.Write("Enable_ESP", "false", "ESP");
                    }
                }

                if (controllerValue == false && INIConfig.Read("TOGGLE_SNAPLINE", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.SNAPLINE == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.SNAPLINE = true;
                        INIConfig.Write("Enable_SNAPLINE", "true", "SNAPLINE");
                    }
                    else
                    {
                        Settings.Default.SNAPLINE = false;
                        INIConfig.Write("Enable_SNAPLINE", "false", "SNAPLINE");
                    }
                }

                if (controllerValue == false && INIConfig.Read("TOGGLE_VSAT", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.VSAT == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.VSAT = true;
                        INIConfig.Write("Enable_VSAT", "true", "MISC");
                    }
                    else
                    {
                        Settings.Default.VSAT = false;
                        INIConfig.Write("Enable_VSAT", "false", "MISC");
                    }
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_CROSSHAIR", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.CROSSHAIR == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.CROSSHAIR = true;
                        INIConfig.Write("Enable_CROSSHAIR", "true", "MISC");
                    }
                    else
                    {
                        Settings.Default.CROSSHAIR = false;
                        INIConfig.Write("Enable_CROSSHAIR", "false", "MISC");
                    }
                }

                if (Settings.Default.ActiveFuncKey == 1 && controllerValue == false)
                {
                    ActionControllerButtonClicked = true;
                    Settings.Default.ControllerButtonAIMBOT = buttonName;
                }
                else if (Settings.Default.ActiveFuncKey == 3 && controllerValue == false)
                {
                    Settings.Default.ControllerToggleAIMBOT = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 4 && controllerValue == false)
                {
                    Settings.Default.ControllerToggleESP = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 5 && controllerValue == false)
                {
                    Settings.Default.ControllerToggleSNAPLINE = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 6 && controllerValue == false)
                {
                    Settings.Default.ControllerToggleVSAT = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 7)
                {
                    Settings.Default.ControllerToggleCROSSHAIR = buttonName;
                    ActionControllerButtonClicked = true;
                }
            };
            #endregion
            #region //R2 BUTTON
            controller.RightTrigger.ValueChanged += (s, e) =>
            {
                string buttonName = "R2";
                bool controllerValue = (Convert.ToBoolean(controller.RightTrigger.Value));

                if (controllerValue == true && INIConfig.Read("AIM_Key", "CONTROLLER") == buttonName)
                {
                    Settings.Default.ControllerAIMBOTButtonPressed = true;
                }
                else if (controllerValue == false && INIConfig.Read("AIM_Key", "CONTROLLER") == buttonName)
                {
                    Settings.Default.ControllerAIMBOTButtonPressed = false;
                }

                if (controllerValue == false && INIConfig.Read("TOGGLE_AIMBOT", "CONTROLLER") == buttonName)
                {
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

                if (controllerValue == false && INIConfig.Read("TOGGLE_ESP", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.ESP == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.ESP = true;
                        INIConfig.Write("Enable_ESP", "true", "ESP");
                    }
                    else
                    {
                        Settings.Default.ESP = false;
                        INIConfig.Write("Enable_ESP", "false", "ESP");
                    }
                }

                if (controllerValue == false && INIConfig.Read("TOGGLE_SNAPLINE", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.SNAPLINE == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.SNAPLINE = true;
                        INIConfig.Write("Enable_SNAPLINE", "true", "SNAPLINE");
                    }
                    else
                    {
                        Settings.Default.SNAPLINE = false;
                        INIConfig.Write("Enable_SNAPLINE", "false", "SNAPLINE");
                    }
                }

                if (controllerValue == false && INIConfig.Read("TOGGLE_VSAT", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.VSAT == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.VSAT = true;
                        INIConfig.Write("Enable_VSAT", "true", "MISC");
                    }
                    else
                    {
                        Settings.Default.VSAT = false;
                        INIConfig.Write("Enable_VSAT", "false", "MISC");
                    }
                }

                if (controllerValue == true && INIConfig.Read("TOGGLE_CROSSHAIR", "CONTROLLER") == buttonName)
                {
                    if (Settings.Default.CROSSHAIR == false && (Settings.Default.pfpf == 1))
                    {
                        Settings.Default.CROSSHAIR = true;
                        INIConfig.Write("Enable_CROSSHAIR", "true", "MISC");
                    }
                    else
                    {
                        Settings.Default.CROSSHAIR = false;
                        INIConfig.Write("Enable_CROSSHAIR", "false", "MISC");
                    }
                }

                if (Settings.Default.ActiveFuncKey == 1 && controllerValue == false)
                {
                    ActionControllerButtonClicked = true;
                    Settings.Default.ControllerButtonAIMBOT = buttonName;
                }
                else if (Settings.Default.ActiveFuncKey == 3)
                {
                    Settings.Default.ControllerToggleAIMBOT = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 4)
                {
                    Settings.Default.ControllerToggleESP = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 5)
                {
                    Settings.Default.ControllerToggleSNAPLINE = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 6)
                {
                    Settings.Default.ControllerToggleVSAT = buttonName;
                    ActionControllerButtonClicked = true;
                }
                else if (Settings.Default.ActiveFuncKey == 7)
                {
                    Settings.Default.ControllerToggleCROSSHAIR = buttonName;
                    ActionControllerButtonClicked = true;
                }
            };
            #endregion

            ////Thumb Positions Left, Right
            //controller.LeftThumbstick.ValueChanged += (s, e) => val = Convert.ToBoolean(e.Value);
            //controller.RightThumbstick.ValueChanged += (s, e) => val = Convert.ToBoolean(e.Value);

            ////Rumble Left, Right
            //controller.LeftRumble.ValueChanged += (s, e) => val = Convert.ToBoolean(e.Value);
            //controller.RightRumble.ValueChanged += (s, e) => val = Convert.ToBoolean(e.Value);

            ////Rumble 0.25f speed for 500 milliseconds when the A or B button is pushed
            //controller.A.ValueChanged += (s, e) => controller.LeftRumble.Rumble(0.25f, 500);
            //controller.B.ValueChanged += (s, e) => controller.RightRumble.Rumble(0.25f, 500);

            ////Rumble at 1.0f speed for 1000 milliseconds when the X or Y button is pushed
            //controller.X.ValueChanged += (s, e) => controller.LeftRumble.Rumble(1.0f, 1000);
            //controller.Y.ValueChanged += (s, e) => controller.RightRumble.Rumble(1.0f, 1000);

        }

        public void AttachToGameProcess()
        {

            if (Security.bpAcc == true && Settings.Default.pfpf == 1)
            {

                var ConsoleAPP = NativeMethods.GetConsoleWindow();
                bool success = false;

                do
                {

                    if (Memory.GetProcessesByName(processName, out process))
                    {
                        if (process.MainWindowTitle.ToLower() == "")
                        {
                            try
                            {
                                process.Kill();
                                AttachToGameProcess();
                                return;
                            }
                            catch
                            {
                                AttachToGameProcess();
                                return;
                            }
                        }
                        if (process.MainWindowTitle.ToLower() == @"bin\plutonium-bootstrapper-win32.exe")
                        {
                            NativeMethods.ShowWindow(process.MainWindowHandle, NativeMethods.SW_HIDE);
                            AttachToGameProcess();
                            return;
                        }
                        Console.WriteLine("Attaching...");

                        //try to attach to game process
                        try
                        {
                            //success  
                            if (process.MainWindowTitle.ToLower().Contains(processGame.ToLower()))
                            {
                                IntPtr handle = Memory.OpenProcess(process.Id);
                                //IntPtr handle = Memory.OpenProcess(9520);
                                if (handle != IntPtr.Zero)
                                {
                                    NativeMethods.ShowWindow(process.MainWindowHandle, NativeMethods.SW_RESTORE);
                                    NativeMethods.SetForegroundWindow(process.MainWindowHandle);
                                    success = true;
                                    processDirectory = process.MainModule.FileName;
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("Attached Handle: " + handle);
                                    NativeMethods.ShowWindow(ConsoleAPP, NativeMethods.SW_HIDE);
                                }
                            }
                            else
                            {
                                //fail
                                Security.FAILACC();
                            }
                        }
                        catch
                        {
                            //fail
                            Security.FAILACC();
                        }
                    }
                    else
                    {
                        try
                        {
                            //fail
                            Security.FAILACC();
                        }
                        catch
                        {

                        }
                    }
                } while (!success);

                StartThreads();
            }

            else
            {
                //fail
                Security.FAILACC();
            }

        }

        private void StartThreads()
        {

            if (Security.bpAcc == true && Settings.Default.pfpf == 1)
            {

                //CALCUL SPECIFIC ADDRESS
                Offsets.baseGame = Memory.Read<int>(Offsets.baseGame);
                Offsets.PlayersList = (Offsets.baseGame + Offsets.PlayersList);
                //END

                //start thread flag
                isRunning = true;

                //start thread for positioning and sizing overlay on top of target process
                windowPosThread = new Thread(UpdateWindow);
                windowPosThread.IsBackground = false;
                windowPosThread.Start();

                //start thread for playing with memory and drawing overlay
                overlayThread = new Thread(UpdateHack);
                overlayThread.IsBackground = false;
                overlayThread.Start();

                //set up low level keyboard hooking to recieve key events while not in focus
                gkh.HookedKeys.Add(Settings.Default.Key_ShowMENU);
                gkh.KeyUp += new KeyEventHandler(KeyOpenMenu);

                if (Settings.Default.pfpf == 1 && Security.bpAcc == true)
                {
                    gkh.HookedKeys.Add((Keys)Enum.Parse(typeof(Keys), Settings.Default.AimKey));
                    gkh.KeyDown += new KeyEventHandler(KeyDownEvent);
                    gkh.KeyUp += new KeyEventHandler(KeyUpEvent);
                }

            }
            else
            {
                //fail
                Security.FAILACC();
            }
        }

        private void UpdateWindow(object handle)
        {
            //start thread security check
            //Security.SecurityCheck();

            //update flag, make sure game is still running

            while (isRunning)
            {
                if (!Memory.IsProcessRunning(process))
                {
                    isRunning = false;
                    Security.FAILACC();
                    continue;
                }

                gameWidth = 0; //FIX RESOLUTION
                gameHeight = 0; //FIX RESOLUTION

                //ensure we are in focus and on top of game
                SetOverlayPosition((IntPtr)_window.Handle);

                //Refresh Open Menu button
                gkh.HookedKeys.Clear();
                gkh.HookedKeys.Add(Settings.Default.Key_ShowMENU);

                if (Settings.Default.pfpf == 1 && Security.bpAcc == true)
                {
                    try
                    {
                        gkh.HookedKeys.Add((Keys)Enum.Parse(typeof(Keys), Settings.Default.AimKey));
                        gkh.KeyDown += new KeyEventHandler(KeyDownEvent);
                        gkh.KeyUp += new KeyEventHandler(KeyUpEvent);
                    }
                    catch { INIConfig.Write("AIM_Key", "X", "AIMBOT"); }
                }

                //Refresh FPS
                _window.FPS = Settings.Default.FPS;

                //CLEAN MEMORY
                Security.CleanMemory();

                //REFRESH INI FILE
                INIConfig = new Clsini("profiles/" + Settings.Default.Profiles + "/config.ini"); //get new config

                //sleep for a bit, we don't need to move around constantly
                Thread.Sleep(refreshWindows);
            }
        }

        private void SetOverlayPosition(IntPtr overlayHandle)
        {

            //get window handle
            IntPtr gameProcessHandle = process.MainWindowHandle;
            if (gameProcessHandle == IntPtr.Zero)
                return;

            //get position and size of window
            NativeMethods.RECT targetWindowPosition, targetWindowSize;

            //GAME
            if (!NativeMethods.GetWindowRect(gameProcessHandle, out targetWindowPosition))
                return;
            if (!NativeMethods.GetClientRect(gameProcessHandle, out targetWindowSize))
                return;
            if (overlayHandle.ToString() == null)
                return;
            if (gameProcessHandle == IntPtr.Zero)
                return;

            //calculate width and height of full target window
            int width = targetWindowPosition.Right - targetWindowPosition.Left;
            int height = targetWindowPosition.Bottom - targetWindowPosition.Top;

            //calculate inner window size without borders      
            int bWidth = targetWindowPosition.Right - targetWindowPosition.Left;
            int bHeight = targetWindowPosition.Bottom - targetWindowPosition.Top;

            width = targetWindowSize.Right - targetWindowSize.Left;
            height = targetWindowSize.Bottom - targetWindowSize.Top;

            int borderWidth = (bWidth - targetWindowSize.Right) / 2;
            int borderHeight = (bHeight - targetWindowSize.Bottom);
            borderHeight -= borderWidth; //remove bottom

            targetWindowPosition.Left += borderWidth;
            targetWindowPosition.Top += borderHeight;

            //Return function only if the windows doesn't have moved
            if (targetWindowPosition.Left == _oldtargetWindowPositionLeft
                    && targetWindowPosition.Top == _oldtargetWindowPositionTop
                    && gameWidth == width
                    && gameHeight == height
                    && _window.IsVisible == true
                    && _window.IsRunning == true
                    && _window.IsInitialized == true)
            {
                return;
            }

            //save window size for ESP WorldToScreen translation
            gameWidth = width;
            gameHeight = height;

            _window.Width = gameWidth;
            _window.Height = gameHeight;

            _window.X = targetWindowPosition.Left;
            _window.Y = targetWindowPosition.Top;

            //SAVE NEW TARGET WINDOWS POSITION
            _oldtargetWindowPositionLeft = targetWindowPosition.Left;
            _oldtargetWindowPositionTop = targetWindowPosition.Top;

            NativeMethods.MoveWindow(overlayHandle, targetWindowPosition.Left, targetWindowPosition.Top, width, height, true);
        }

        private void KeyOpenMenu(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Settings.Default.Key_ShowMENU)
            {

                Menu myForm = new Menu();
                gkh.HookedKeys.Clear();

                foreach (Form f in Application.OpenForms)
                {
                    if (Menu_Showed == true)
                    {
                        f.Close();
                        Menu_Showed = false;
                        return;

                    }
                }

                if (Menu_Showed == false && Menu_Loading == false)
                {
                    try
                    {
                        Menu_Loading = true;
                        Application.Run(myForm);
                        myForm.BringToFront();
                        myForm.Activate();
                    }
                    catch { }
                }
                e.Handled = true;
            }
        }

        private void KeyDownEvent(object sender, KeyEventArgs e)
        {
            aim = (e.KeyCode == (Keys)Enum.Parse(typeof(Keys), Settings.Default.AimKey));

            e.Handled = true;
        }

        private void KeyUpEvent(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == (Keys)Enum.Parse(typeof(Keys), Settings.Default.AimKey))
            {
                aim = false;
            }

            e.Handled = true;
        }

        private void UpdateHack()
        {

            //update loop
            while (isRunning == true)
            {

                //read
                ReadGameMemory();

                //aimbot
                if (Settings.Default.pfpf == 1 && Settings.Default.ActiveFuncKey == 0 && Security.bpAcc == true)
                {
                    UpdateAimbot();
                }

                //refresh ESP
                Thread.Sleep(1);
            }

            //cleanup
            Memory.CloseProcess();

        }


        public static bool ActionControllerButtonClicked;

        private void UpdateAimbot()
        {
            try
            {

                if (Settings.Default.pfpf == 0 || Security.bpAcc == false)
                {
                    //fail
                    Security.FAILACC();
                }

                if (GetActiveWindowTitleAIMBOT() == false) return;

                if (Settings.Default.ControllerAIMBOTButtonPressed == true && (INIConfig.Read("AIM_Key", "CONTROLLER") == Settings.Default.ControllerButtonAIMBOT)) //CONTROLLER
                {
                    ButtonCustomClick = true;
                }
                else if (GetAsyncKeyState(Keys.LButton) < 0 && (INIConfig.Read("AIM_Key", "AIMBOT") == "LButton")) //LEFT MOUSE
                {
                    ButtonCustomClick = true;
                }
                else if (GetAsyncKeyState(Keys.RButton) < 0 && (INIConfig.Read("AIM_Key", "AIMBOT") == "RButton")) //RIGHT MOUSE
                {
                    ButtonCustomClick = true;
                }
                else if (GetAsyncKeyState(Keys.XButton1) < 0 && (INIConfig.Read("AIM_Key", "AIMBOT") == "XButton1")) //Macro button 1
                {
                    ButtonCustomClick = true;
                }
                else if (GetAsyncKeyState(Keys.XButton2) < 0 && (INIConfig.Read("AIM_Key", "AIMBOT") == "XButton2")) //Macro button 2
                {
                    ButtonCustomClick = true;
                }
                else
                {
                    ButtonCustomClick = false;
                }

                //if not aiming or no players, escape
                if (!aim && ButtonCustomClick == false || players.Count == 0 || Settings.Default.AIMBOT == false || Settings.Default.pfpf == 0 || GetActiveWindowTitle() == false) return;

                Player target = null;
                //find closest enemy player
                if (Settings.Default.AIMBOT_PRIORITY == 1) { target = GetClosestEnemyToCrossHair(); }
                else if (Settings.Default.AIMBOT_PRIORITY == 2) { target = GetClosestEnemyToDistance(); }

                //if (target == null || target.PlayerISALIVE() != (2050, 1, 16777216)) return;
                if (target == null) return;

                //calculate verticle angle between enemy and player (pitch)

                if (Settings.Default.AIMBOTTarget == 1) //HEAD
                {
                    TARGET = HEAD;
                }
                else if (Settings.Default.AIMBOTTarget == 2) //BODY
                {
                    TARGET = BODY;
                }
                else if (Settings.Default.AIMBOTTarget == 3) //FOOT
                {
                    TARGET = FOOT;
                }

                //if (target.pl == 1) //FOOT
                //{
                //    TARGET = 15F;
                //}
                //if (target.PlayerCROUCH == 2) //FOOT
                //{
                //    TARGET = 16.5F;
                //}

                //set self angles to calculated angles

                Vector3 test = (target.PositionHead);
                test.z -= TARGET;

                Vector2 headPos;

                if (viewMatrix.WorldToScreen(target.PositionHead, gameWidth, gameHeight, out headPos) &&
                    viewMatrix.WorldToScreen(test, Example.gameWidth, Example.gameHeight, out headPos))
                {

                    float num = gameWidth / 2; //GameWidth
                    float num2 = gameHeight / 2;  //GameHeight
                    float smoothSpeed = Settings.Default.AIMBOTAcceleration;

                    float radius = -Settings.Default.AIMBOTFOV * 2 + gameHeight / Settings.Default.AIMBOTFOV;

                    //ADDING FOOT POS HEIGHT
                    //if (headPos.y >= -headPos.y)
                    //{
                    //    TARGET -= TARGET;
                    //}
                    //else
                    //{
                    //    TARGET += TARGET;
                    //}

                    if (!(Math.Abs(headPos.x - num) > radius) && !(Math.Abs(headPos.y - num2) > radius)) //FOV
                    {
                        float num3 = 0f;
                        float num4 = 0f;

                        if (headPos.x > num)
                        {
                            num3 = 0f - (num - headPos.x);
                            num3 /= smoothSpeed;
                        }
                        else if (headPos.x < num)
                        {
                            num3 = headPos.x - num;
                            num3 /= smoothSpeed;
                        }

                        if (headPos.y > num2)
                        {
                            num4 = (0f - (num2 - headPos.y));
                            num4 /= smoothSpeed;
                        }
                        else if (headPos.y < num2)
                        {
                            num4 = (headPos.y - num2);
                            num4 /= smoothSpeed;
                        }

                        mousecontrol.Mouse.MoveMouseBy((int)num3, (int)num4);

                        if (Settings.Default.AIMAutoFire == true && (Settings.Default.pfpf == 1) && Security.bpAcc == true)
                        {
                            int value01 = (int)Math.Abs(SelfPitch - Totalpitch);
                            int value02 = (int)Math.Abs(SelfYaw - TotalYaw);

                            if (Settings.Default.AimKey.ToString().ToLower() != "LButton".ToLower())
                            {
                                Rapid_Fire.Fire();
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private Player GetClosestEnemyToCrossHair()
        {

            try
            {

                if (Settings.Default.pfpf == 0 && Security.bpAcc == true)
                {
                    //fail
                    Security.FAILACC();
                }

                //find first living enemy player in view
                Vector2 targetPos = new Vector2();
                //Player target = players.Find(p => p.PlayerISALIVE() == (2050, 1, 16777216) &&
                Player target = players.Find(p =>
                viewMatrix.WorldToScreen(p.PositionHead, gameWidth, gameHeight, out targetPos));
                if (target == null) return null;
                //calculate distance to crosshair
                Vector2 crossHair = new Vector2(gameWidth / 2, gameHeight / 2);
                float dist = crossHair.Distance(targetPos);

                float num = gameWidth / 2; //GameWidth
                float num2 = gameHeight / 2;  //GameHeight

                //find player closest to crosshair
                foreach (Player p in players)
                {
                    //if (p.PlayerISALIVE() == (2050, 1, 16777216))
                    //{

                        Vector2 headPos;
                    if (viewMatrix.WorldToScreen(p.PositionHead, gameWidth, gameHeight, out headPos))
                    {
                        float radius = -Settings.Default.AIMBOTFOV * 2 + gameHeight / Settings.Default.AIMBOTFOV;

                        if (!(Math.Abs(headPos.x - num) > radius) && !(Math.Abs(headPos.y - num2) > radius)) //FOV
                        {
                            float newDist = crossHair.Distance(headPos);

                            if (newDist < dist)
                            {
                                target = p;
                                dist = newDist;
                            }
                        }
                    //}
                    }
                }

                return target;

            }
            catch { return null; }
        }

        private Player GetClosestEnemyToDistance()
        {

            try
            {

                if (Settings.Default.pfpf == 0 && Security.bpAcc == true)
                {
                    //fail
                    Security.FAILACC();
                }

                float num = gameWidth / 2; //GameWidth
                float num2 = gameHeight / 2;  //GameHeight

                //find first living enemy player in view
                Vector2 targetPos = new Vector2();
                //Player target = players.Find(p => p.PlayerISALIVE() == (2050, 1, 16777216) &&
                Player target = players.Find(p =>
                //Player target = players.Find(p =>
                viewMatrix.WorldToScreen(p.PositionHead, gameWidth, gameHeight, out targetPos));

                //read self
                int ptrPlayerSelfPos = (Offsets.baseGame + Offsets.SelfLocalPlayerPOSITION);
                SelfPos = new Player(ptrPlayerSelfPos);

                Vector3 MyDist = new Vector3(SelfPos.SelfPosHead.x, SelfPos.SelfPosHead.y, SelfPos.SelfPosHead.z);
                float FirstDist = 99999f;

                //find player closest to distance
                foreach (Player p in players)
                {
                    //if (p.PlayerISALIVE() == (2050, 1, 16777216))
                    //{
                        Vector2 headPos;
                    if (viewMatrix.WorldToScreen(p.PositionHead, gameWidth, gameHeight, out headPos))
                    {
                        float radius = -Settings.Default.AIMBOTFOV * 2 + gameHeight / Settings.Default.AIMBOTFOV;

                        if (!(Math.Abs(headPos.x - num) > radius) && !(Math.Abs(headPos.y - num2) > radius)) //FOV
                        {
                            float newDist = MyDist.Distance(p.PositionHead);

                            if (newDist <= FirstDist)
                            {
                                target = p;
                                FirstDist = newDist;
                            }
                        //}
                        }
                    }
            }
                if (target == null)
                {
                    //find closest enemy player
                    return target = GetClosestEnemyToCrossHair();
                }

                return target;

            }
            catch { return GetClosestEnemyToCrossHair(); }
        }

        private void ReadGameMemory()
        {
            //read view matrix
            viewMatrix = Memory.ReadMatrix(Offsets.viewMatrix);

            //passe seulement si le jeu est ouvert ou si le refreshdrawing == true + self local player est == a la valeur de base
            if (!isRunning || (GetActiveWindowTitle() == false)) return;

            numPlayers = 36;
            //numPlayers = 1;

            for (int i = 0; i <= numPlayers; i++)
            {
                //int PLAYERLIST = (Offsets.PlayersList + Offsets.ptrPlayerArray * i);
                int ptrPlayer = Offsets.PlayersList + Offsets.ptrPlayerArray * i;
                //int ptrPlayer = Offsets.testVAL + 0x20 * i ;

                if (players.Count >= numPlayers)
                {
                    if (players.Count > numPlayers)
                    {
                        players.RemoveAt(i);
                        continue;
                    }

                    if (i <= numPlayers - 1)
                    {

                        //Enlever son propre ESP
                        //if (SelfLocalPlayerNumberID == i)
                        //{
                        //    SelfLocalTEAM = PlayerTeam;
                        //    players[i] = new Player(0, 0);
                        //    continue;
                        //}
                        //END

                        //Enlever les esp ALLIES si desactivé
                        //if (Settings.Default.ShowAllies == false && PlayerTeam2 == 0)
                        //{
                        //    if (PlayerTeam == SelfLocalTEAM)
                        //    {
                        //        players[i] = new Player(0, 0);
                        //        continue;
                        //    }
                        //}

                        players[i] = new Player(ptrPlayer);

                        //END
                    }
                }
                else
                {
                    players.Add(new Player(ptrPlayer));
                }
            }
        }

        private void AssaultHack_FormClosing(object sender, FormClosingEventArgs e)
        {
            //kill threads
            isRunning = false;

            //wait for threads to finish
            windowPosThread.Join(2000);
            overlayThread.Join(2000);

            //detach from process
            Memory.CloseProcess();
            Security.FAILACC();
        }

        public void _window_SetupGraphics(object sender, SetupGraphicsEventArgs e)
        {

            try
            {
                var gfx = e.Graphics;

                if (e.RecreateResources)
                {
                    foreach (var pair in _brushes) pair.Value.Dispose();
                    foreach (var pair in _fonts) pair.Value.Dispose();
                    foreach (var pair in _images) pair.Value.Dispose();
                }

                _brushes["black"] = gfx.CreateSolidBrush(0, 0, 0);
                _brushes["red"] = gfx.CreateSolidBrush(255, 0, 0);
                _brushes["white"] = gfx.CreateSolidBrush(255, 255, 255);
                _brushes["green"] = gfx.CreateSolidBrush(0, 255, 0);
                _brushes["OVERLAY01"] = gfx.CreateSolidBrush(Settings.Default.DefaultForeColor.R, Settings.Default.DefaultForeColor.G, Settings.Default.DefaultForeColor.B);
                _brushes["OVERLAY02"] = gfx.CreateSolidBrush(Settings.Default.DefaultBackgroundColor.R, Settings.Default.DefaultBackgroundColor.G, Settings.Default.DefaultBackgroundColor.B);
                _brushes["ESPCOLOR"] = gfx.CreateSolidBrush(Settings.Default.ESPColor.R, Settings.Default.ESPColor.G, Settings.Default.ESPColor.B);
                _brushes["CROSSHAIR"] = gfx.CreateSolidBrush(Settings.Default.CROSSHAIRColor.R, Settings.Default.CROSSHAIRColor.G, Settings.Default.CROSSHAIRColor.B);
                _brushes["ESPFILLEDBORDERCOLOR"] = gfx.CreateSolidBrush(Settings.Default.ESPFILLEDBORDERCOLOR.R, Settings.Default.ESPFILLEDBORDERCOLOR.G, Settings.Default.ESPFILLEDBORDERCOLOR.B);
                _brushes["AIMBOT_FOV"] = gfx.CreateSolidBrush(Settings.Default.AIMBOT_FOV_Color.R, Settings.Default.AIMBOT_FOV_Color.G, Settings.Default.AIMBOT_FOV_Color.B);
                _brushes["ESP_VISIBLE_TEXT"] = gfx.CreateSolidBrush(Settings.Default.ESP_VISIBLE_TEXT.R, Settings.Default.ESP_VISIBLE_TEXT.G, Settings.Default.ESP_VISIBLE_TEXT.B);
                _brushes["SNAPLINE"] = gfx.CreateSolidBrush(Settings.Default.SNAPLINEColor.R, Settings.Default.SNAPLINEColor.G, Settings.Default.SNAPLINEColor.B);
                _brushes["background"] = gfx.CreateSolidBrush(0, 0, 0, 0);
                _brushes["TextRectangle"] = gfx.CreateSolidBrush(0, 0, 0, 150);


                //MemoryStream ms = new MemoryStream();
                //Image mypic = Image.FromFile(@"D:\DOWNLOAD\test.png");
                //mypic.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                //_images["test"] = gfx.CreateImage(ms.ToArray());

                if (e.RecreateResources) return;

                _fonts["arial"] = gfx.CreateFont("Arial", 12);
                _fonts["consolas"] = gfx.CreateFont("Consolas", 14);
                _fonts["OVERLAY01"] = gfx.CreateFont("Consolas", 9);
                _fonts["OVERLAY02"] = gfx.CreateFont("Consolas", 12);
                _fonts["ESP_TEXT_SIZE"] = gfx.CreateFont(Settings.Default.FontText, Settings.Default.ESP_SIZE_TEXT);

            }
            catch
            {
                //FAIL
                File.WriteAllBytes("profiles/" + Settings.Default.Profiles + "/config.ini", Resources.config);
                Security.FAILACC();
            }

        }

        private void _window_DestroyGraphics(object sender, DestroyGraphicsEventArgs e)
        {
            foreach (var pair in _brushes) pair.Value.Dispose();
            foreach (var pair in _fonts) pair.Value.Dispose();
            foreach (var pair in _images) pair.Value.Dispose();
            var gfx = e.Graphics;
            gfx.Destroy();
            gfx.Dispose();
            gfx.EndScene();
        }

        private void _window_DrawGraphics(object sender, DrawGraphicsEventArgs e)
        {
            var gfx = e.Graphics;
            gfx.ClearScene(); //Clear drawing after repeat

            if (GetActiveWindowTitle() == false) return; //Enter inside only if your windows is entered in game

            if (Settings.Default.ShowFPS == true)
            {
                var padding = 16;
                var infoText = new StringBuilder()
                    .Append("FPS: " + gfx.FPS.ToString().PadRight(padding))
                    //.Append("FrameTime: ").AppendLine(e.FrameTime.ToString().PadRight(padding))
                    //.Append("FrameCount: ").AppendLine(e.FrameCount.ToString().PadRight(padding))
                    //.Append("DeltaTime: ").AppendLine(e.DeltaTime.ToString().PadRight(padding))
                    .ToString();

                gfx.MeasureFPS = Settings.Default.ShowFPS;
                bbjuyhgazdf.Drawing.Point outputSize = gfx.MeasureString(_fonts["consolas"], infoText);

                if (Settings.Default.FPS_CORNER == 1) //TOP LEFT
                {
                    gfx.DrawTextWithBackground(_fonts["consolas"], _brushes["green"], _brushes["black"], 0, 0, infoText);
                }
                else if (Settings.Default.FPS_CORNER == 2) //TOP RIGHT
                {
                    gfx.DrawTextWithBackground(_fonts["consolas"], _brushes["green"], _brushes["black"], gameWidth - (outputSize.X), 0, infoText);
                }
                else if (Settings.Default.FPS_CORNER == 3) //BOT LEFT
                {
                    gfx.DrawTextWithBackground(_fonts["consolas"], _brushes["green"], _brushes["black"], 0, gameHeight - (outputSize.Y), infoText);
                }
                else if (Settings.Default.FPS_CORNER == 4) //BOT RIGHT
                {
                    gfx.DrawTextWithBackground(_fonts["consolas"], _brushes["green"], _brushes["black"], gameWidth - (outputSize.X), gameHeight - (outputSize.Y), infoText);
                }
            }

            DrawFigure(gfx);
        }

        private void DrawFigure(bbjuyhgazdf.Drawing.Graphics gfx)
        {

            if (GetActiveWindowTitle() == false) return; //Enter inside only if your windows is entered in game

            //DEBUT OVERLAY
            if (Settings.Default.OVERLAY == true)
            {
                int OverlayInt01 = 100;
                int OverlayInt02 = 97;

                int XWidth = Settings.Default.OverlaySavedVal;
                int YHeight = Settings.Default.OverlaySavedVal;

                //ARRIERE PLAN
                gfx.FillRectangle(OVERLAYCOLOR01(), 340, 23, 180, OverlayInt01);
                //FOND
                gfx.FillRectangle(OVERLAYCOLOR02(), 337, 26, 183, OverlayInt02);

                //TEXT
                gfx.DrawText(_fonts["consolas"], _brushes["white"], gameWidth - 95, gameHeight - 20, "VECR PROJECT");
                gfx.DrawText(_fonts["OVERLAY01"], _brushes["white"], 190, 28, "Show Menu: [" + Settings.Default.Key_ShowMENU + "]");
                gfx.DrawLine(OVERLAYCOLOR01(), 180, 50, 338, 50, 2F);

                gfx.DrawText(_fonts["OVERLAY01"], _brushes["white"], 190, 38, "Active Aimbot: [" + Settings.Default.AimKey + "|" + Settings.Default.ControllerButtonAIMBOT + "]");

                //FUNCTION            
                if (Settings.Default.ESP == false)
                {
                    gfx.DrawText(_fonts["OVERLAY02"], _brushes["white"], 190, 52, "ESP:");
                    gfx.DrawText(_fonts["OVERLAY02"], _brushes["red"], 220, 52, "[OFF]");
                }
                else
                {
                    gfx.DrawText(_fonts["OVERLAY02"], _brushes["white"], 190, 52, "ESP:");
                    gfx.DrawText(_fonts["OVERLAY02"], _brushes["green"], 220, 52, "[ON]");
                }

                if (Settings.Default.pfpf == 1 && Security.bpAcc == true)
                {
                    if (Settings.Default.SNAPLINE == false)
                    {
                        gfx.DrawText(_fonts["OVERLAY02"], _brushes["white"], 190, 66, "SNAPLINE:");
                        gfx.DrawText(_fonts["OVERLAY02"], _brushes["red"], 252, 66, "[OFF]");
                    }
                    else
                    {
                        gfx.DrawText(_fonts["OVERLAY02"], _brushes["white"], 190, 66, "SNAPLINE:");
                        gfx.DrawText(_fonts["OVERLAY02"], _brushes["green"], 252, 66, "[ON]");
                    }

                    if (Settings.Default.AIMBOT == false)
                    {
                        gfx.DrawText(_fonts["OVERLAY02"], _brushes["white"], 190, 80, "AIMBOT:");
                        gfx.DrawText(_fonts["OVERLAY02"], _brushes["red"], 239, 80, "[OFF]");
                    }
                    else
                    {
                        gfx.DrawText(_fonts["OVERLAY02"], _brushes["white"], 190, 80, "AIMBOT:");
                        gfx.DrawText(_fonts["OVERLAY02"], _brushes["green"], 239, 80, "[ON]");
                    }
                }
            }
            //END

            try
            {

                //AIM FOV
                if (Settings.Default.ShowAIMFov == true && Settings.Default.pfpf == 1 && Security.bpAcc == true)
                {
                    if (Settings.Default.AIMBOT == true)
                    {
                        float radius = -Settings.Default.AIMBOTFOV * 2 + gameHeight / Settings.Default.AIMBOTFOV;
                        gfx.DrawEllipse(AIMBOTFOVCOLOR(), gameWidth / 2, gameHeight / 2, radius, radius, 2f);
                    }
                }
                //END 

                //CROSSHAIR
                if (Settings.Default.CROSSHAIR == true)
                {
                    gfx.DrawLine(CROSSHAIRCOLOR(), gameWidth / 2, gameHeight / 2 - Settings.Default.CROSSHAIRSize, gameWidth / 2, gameHeight / 2 + Settings.Default.CROSSHAIRSize, Settings.Default.CROSSHAIRThickness);
                    gfx.DrawLine(CROSSHAIRCOLOR(), gameWidth / 2 - Settings.Default.CROSSHAIRSize, gameHeight / 2, gameWidth / 2 + Settings.Default.CROSSHAIRSize, gameHeight / 2, Settings.Default.CROSSHAIRThickness);
                }
                //END

                //DRAWING  //DRAW ESP, SNAPLINE
                if (Settings.Default.ESP == true)
                {

                    foreach (Player p in players.ToArray())
                    {
                        try
                        {

                            if (p.PlayerISALIVE() != (2050, 1, 16777216)) continue;

                            Vector2 headPos, footPos;

                            if (viewMatrix.WorldToScreen(p.PositionHead, gameWidth, gameHeight, out headPos) &&
                                viewMatrix.WorldToScreen(p.PositionFoot, gameWidth, gameHeight, out footPos))
                            {
                                float height = Math.Abs(headPos.y - footPos.y);
                                float width = height / 2F;

                                try
                                {

                                    //ESP SHOW DISTANCE
                                    if (Settings.Default.Show_DISTANCE == true || Settings.Default.Show_Name && (Settings.Default.pfpf == 1 && Security.bpAcc == true))
                                    {
                                        float dx = p.PositionFoot.x - p.SelfPosFoot.x;
                                        float dy = p.PositionFoot.y - p.SelfPosFoot.y;

                                        double distance = Math.Sqrt(dx * dx + dy * dy);

                                        distance = distance / 10;
                                        distance = Math.Abs(Convert.ToInt32(distance));
                                        string playerName = "";
                                        string playerPing = "";

                                        bbjuyhgazdf.Drawing.Point outputSize = gfx.MeasureString(ESP_TEXT_SIZE(), playerName + playerPing);

                                        if (Settings.Default.Show_Name == true)
                                        {
                                            playerName = "Zombie " + p.ZombieNumberPosition.ToString();
                                            outputSize = gfx.MeasureString(ESP_TEXT_SIZE(), playerName);
                                        }
                                        if (Settings.Default.Show_DISTANCE == true)
                                        {
                                            playerPing = "[" + distance + "m]";
                                            outputSize = gfx.MeasureString(ESP_TEXT_SIZE(), playerPing);
                                        }
                                        if (Settings.Default.Show_Name == true && Settings.Default.Show_DISTANCE == true)
                                        {
                                            playerName += " ";
                                            outputSize = gfx.MeasureString(ESP_TEXT_SIZE(), playerName + playerPing);
                                        }

                                        if (Settings.Default.Show_Text_Border == true)
                                        {
                                            var brush = _brushes["ESPFILLEDBORDERCOLOR"];
                                            brush.Color = new bbjuyhgazdf.Drawing.Color(255, 0, 0);
                                            gfx.DrawBox2D(_brushes["background"], _brushes["TextRectangle"], headPos.x - outputSize.X / 2f + -6, headPos.y - outputSize.Y + Settings.Default.ESP_SIZE_TEXT + 6, headPos.x + outputSize.X / 2f + 6, headPos.y - outputSize.Y, Settings.Default.ESPSize);
                                        }
                                        gfx.DrawText(ESP_TEXT_SIZE(), PLAYERCOLOR(p), headPos.x - outputSize.X / 2f, headPos.y - outputSize.Y, playerName + playerPing);
                                    }
                                    //END

                                    //ESP SHOW PING
                                    if (Settings.Default.Show_PING && (Settings.Default.pfpf == 1 && Security.bpAcc == true))
                                    {
                                        float dx = p.PositionFoot.x - p.SelfPosFoot.x;
                                        float dy = p.PositionFoot.y - p.SelfPosFoot.y;

                                        string Text = "";

                                        //if (Settings.Default.Show_PING == true)
                                        //{
                                        //    Text = p.PlayerPing + "ms";
                                        //}

                                        bbjuyhgazdf.Drawing.Point outputSize = gfx.MeasureString(ESP_TEXT_SIZE(), Text);

                                        if (Settings.Default.Show_Text_Border == true)
                                        {
                                            var brush = _brushes["ESPFILLEDBORDERCOLOR"];
                                            brush.Color = new bbjuyhgazdf.Drawing.Color(255, 0, 0);
                                            gfx.DrawBox2D(_brushes["background"], _brushes["TextRectangle"], headPos.x - outputSize.X / 2f + -6, headPos.y + height + Settings.Default.ESP_SIZE_TEXT + 6, headPos.x + outputSize.X / 2f + 6, headPos.y + height, Settings.Default.ESPSize);
                                        }

                                        gfx.DrawText(ESP_TEXT_SIZE(), PLAYERPINGCOLOR(p), headPos.x - outputSize.X / 2f, headPos.y + height, Text);
                                    }
                                    //END
                                }

                                catch { }

                                //ESP
                                if (Settings.Default.ESPForm == 1)
                                {
                                    gfx.DrawRoundedRectangle(ESPCOLOR(p), footPos.x - width / 2f, headPos.y, footPos.x + width / 2f, footPos.y, 2f, Settings.Default.ESPSize);

                                    //gfx.DrawImage(_images["test"], headPos.x - width + 256 / 2f, headPos.y + 150, headPos.x + width / 2f, headPos.y);

                                    //gfx.DrawHorizontalProgressBar(ESPCOLOR(p), CROSSHAIRCOLOR(), headPos.x - width / 2f, headPos.y, headPos.x + width / 2f, headPos.y, 2f, 100f); //A mettre pour plus tard quand j'aurais trouver  la vie des joeurs
                                }
                                else if (Settings.Default.ESPForm == 2)
                                {
                                    gfx.DrawBox2D(ESPFILLEDBORDERCOLOR(), ESPCOLOR(p), headPos.x - width / 2f, headPos.y, headPos.x + width / 2f, headPos.y, Settings.Default.ESPSize);
                                }
                                else if (Settings.Default.ESPForm == 3)
                                {
                                    gfx.DrawRoundedRectangle(ESPCOLOR(p), headPos.x - width / 2f, headPos.y, headPos.x + width / 2f, headPos.y, 888f, Settings.Default.ESPSize);
                                }
                                //END

                                if (Settings.Default.SNAPLINE == true && Settings.Default.pfpf == 1 && Security.bpAcc == true)
                                {
                                    //TOP LEFT
                                    if (Settings.Default.SNAPLINEStartingPoint == 1)
                                    {
                                        gfx.DrawLine(SNAPLINECOLOR(p), 0, 0, headPos.x, headPos.y + height, Settings.Default.SNAPLINESize);
                                    }
                                    //CENTER TOP
                                    else if (Settings.Default.SNAPLINEStartingPoint == 2)
                                    {
                                        gfx.DrawLine(SNAPLINECOLOR(p), gameWidth / 2, 0, headPos.x, headPos.y + height, Settings.Default.SNAPLINESize);
                                    }
                                    //TOP RIGHT
                                    else if (Settings.Default.SNAPLINEStartingPoint == 3)
                                    {
                                        gfx.DrawLine(SNAPLINECOLOR(p), gameWidth, 0, headPos.x, headPos.y + height, Settings.Default.SNAPLINESize);
                                    }
                                    //CENTER LEFT
                                    else if (Settings.Default.SNAPLINEStartingPoint == 4)
                                    {
                                        gfx.DrawLine(SNAPLINECOLOR(p), 0, gameHeight / 2, headPos.x, headPos.y + height, Settings.Default.SNAPLINESize);
                                    }
                                    //CENTER
                                    else if (Settings.Default.SNAPLINEStartingPoint == 5)
                                    {
                                        gfx.DrawLine(SNAPLINECOLOR(p), gameWidth / 2, gameHeight / 2, headPos.x, headPos.y + height, Settings.Default.SNAPLINESize);
                                    }
                                    //CENTER RIGHT
                                    else if (Settings.Default.SNAPLINEStartingPoint == 6)
                                    {
                                        gfx.DrawLine(SNAPLINECOLOR(p), gameWidth, gameHeight / 2, headPos.x, headPos.y + height, Settings.Default.SNAPLINESize);
                                    }
                                    //BOTTOM LEFT
                                    else if (Settings.Default.SNAPLINEStartingPoint == 7)
                                    {
                                        gfx.DrawLine(SNAPLINECOLOR(p), 0, gameHeight, headPos.x, headPos.y + height, Settings.Default.SNAPLINESize);
                                    }
                                    //CENTER BOT
                                    else if (Settings.Default.SNAPLINEStartingPoint == 8)
                                    {
                                        gfx.DrawLine(SNAPLINECOLOR(p), gameWidth / 2, gameHeight, headPos.x, headPos.y + height, Settings.Default.SNAPLINESize);
                                    }
                                    //BOTTOM RIGHT
                                    else if (Settings.Default.SNAPLINEStartingPoint == 9)
                                    {
                                        gfx.DrawLine(SNAPLINECOLOR(p), gameWidth, gameHeight, headPos.x, headPos.y + height, Settings.Default.SNAPLINESize);
                                    }
                                }

                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }

        private bbjuyhgazdf.Drawing.Font ESP_TEXT_SIZE()
        {
            try
            {

                var font = _fonts["ESP_TEXT_SIZE"];

                font = new bbjuyhgazdf.Drawing.Font(new Factory(), Settings.Default.FontText, Settings.Default.ESP_SIZE_TEXT);

                return font;

            }
            catch { return null; }
        }

        private bbjuyhgazdf.Drawing.SolidBrush ESPCOLOR(Player p)
        {

            try
            {

                var brush = _brushes["ESPCOLOR"];

                brush.Color = new bbjuyhgazdf.Drawing.Color(Settings.Default.ESPColor.R, Settings.Default.ESPColor.G, Settings.Default.ESPColor.B);

                //if (p.PlayerTEAMForFFA == 0)
                //{

                //    if (p.PlayerTEAM != p.SelfPlayerTeam)
                //    {
                //        brush.Color = new bbjuyhgazdf.Drawing.Color(Settings.Default.ESPColor.R, Settings.Default.ESPColor.G, Settings.Default.ESPColor.B);
                //    }
                //    else
                //    {
                //        brush.Color = new bbjuyhgazdf.Drawing.Color(Settings.Default.ESPALLIESCOLOR.R, Settings.Default.ESPALLIESCOLOR.G, Settings.Default.ESPALLIESCOLOR.B);
                //    }
                //}
                //else
                //{
                //    brush.Color = new bbjuyhgazdf.Drawing.Color(Settings.Default.ESPColor.R, Settings.Default.ESPColor.G, Settings.Default.ESPColor.B);
                //}

                //if (p.PlayerISALIVE > 0) //IF IS DEAD
                //{
                //    brush.Color = new bbjuyhgazdf.Drawing.Color(0, 0, 0);
                //}

                return brush;

            }
            catch { return null; }
        }

        private bbjuyhgazdf.Drawing.SolidBrush PLAYERCOLOR(Player p)
        {

            try
            {

                var brush = _brushes["ESPCOLOR"];

                //if (p.PlayerTEAMForFFA == 0)
                //{

                //    if (p.PlayerTEAM != p.SelfPlayerTeam)
                //    {
                //        brush.Color = new bbjuyhgazdf.Drawing.Color(Settings.Default.PlayerNameColor.R, Settings.Default.PlayerNameColor.G, Settings.Default.PlayerNameColor.B);
                //    }
                //    else
                //    {
                //        brush.Color = new bbjuyhgazdf.Drawing.Color(Settings.Default.ESPALLIESCOLOR.R, Settings.Default.ESPALLIESCOLOR.G, Settings.Default.ESPALLIESCOLOR.B);
                //    }
                //}
                //else
                //{
                //    brush.Color = new bbjuyhgazdf.Drawing.Color(Settings.Default.PlayerNameColor.R, Settings.Default.PlayerNameColor.G, Settings.Default.PlayerNameColor.B);
                //}

                //if (p.PlayerISALIVE > 0) //IF IS DEAD
                //{
                //    brush.Color = new bbjuyhgazdf.Drawing.Color(0, 0, 0);
                //}

                brush.Color = new bbjuyhgazdf.Drawing.Color(Settings.Default.PlayerNameColor.R, Settings.Default.PlayerNameColor.G, Settings.Default.PlayerNameColor.B);

                return brush;

            }
            catch { return null; }
        }

        private bbjuyhgazdf.Drawing.SolidBrush PLAYERPINGCOLOR(Player p)
        {

            try
            {

                var brush = _brushes["ESPCOLOR"];

                //if (p.PlayerTEAMForFFA == 0)
                //{

                //    if (p.PlayerTEAM != p.SelfPlayerTeam)
                //    {
                //        brush.Color = new bbjuyhgazdf.Drawing.Color(Settings.Default.PlayerPingColor.R, Settings.Default.PlayerPingColor.G, Settings.Default.PlayerPingColor.B);
                //    }
                //    else
                //    {
                //        brush.Color = new bbjuyhgazdf.Drawing.Color(Settings.Default.ESPALLIESCOLOR.R, Settings.Default.ESPALLIESCOLOR.G, Settings.Default.ESPALLIESCOLOR.B);
                //    }
                //}
                //else
                //{
                //    brush.Color = new bbjuyhgazdf.Drawing.Color(Settings.Default.PlayerPingColor.R, Settings.Default.PlayerPingColor.G, Settings.Default.PlayerPingColor.B);
                //}

                //if (p.PlayerISALIVE > 0) //IF IS DEAD
                //{
                //    brush.Color = new bbjuyhgazdf.Drawing.Color(0, 0, 0);
                //}

                brush.Color = new bbjuyhgazdf.Drawing.Color(Settings.Default.PlayerNameColor.R, Settings.Default.PlayerNameColor.G, Settings.Default.PlayerNameColor.B);

                return brush;

            }
            catch { return null; }
        }

        private bbjuyhgazdf.Drawing.SolidBrush CROSSHAIRCOLOR()
        {
            var brush = _brushes["CROSSHAIR"];

            brush.Color = new bbjuyhgazdf.Drawing.Color(Settings.Default.CROSSHAIRColor.R, Settings.Default.CROSSHAIRColor.G, Settings.Default.CROSSHAIRColor.B);

            return brush;
        }

        private bbjuyhgazdf.Drawing.SolidBrush ESPFILLEDBORDERCOLOR()
        {
            var brush = _brushes["ESPFILLEDBORDERCOLOR"];

            brush.Color = new bbjuyhgazdf.Drawing.Color(Settings.Default.ESPFILLEDBORDERCOLOR.R, Settings.Default.ESPFILLEDBORDERCOLOR.G, Settings.Default.ESPFILLEDBORDERCOLOR.B);

            return brush;
        }

        private bbjuyhgazdf.Drawing.SolidBrush AIMBOTFOVCOLOR()
        {
            var brush = _brushes["AIMBOT_FOV"];

            brush.Color = new bbjuyhgazdf.Drawing.Color(Settings.Default.AIMBOT_FOV_Color.R, Settings.Default.AIMBOT_FOV_Color.G, Settings.Default.AIMBOT_FOV_Color.B);

            return brush;
        }

        private bbjuyhgazdf.Drawing.SolidBrush SNAPLINECOLOR(Player p)
        {
            var brush = _brushes["SNAPLINE"];

            brush.Color = new bbjuyhgazdf.Drawing.Color(Settings.Default.SNAPLINEColor.R, Settings.Default.SNAPLINEColor.G, Settings.Default.SNAPLINEColor.B);

            //if (p.PlayerTEAMForFFA == 0)
            //{

            //    if (p.PlayerTEAM != p.SelfPlayerTeam)
            //    {
            //        brush.Color = new bbjuyhgazdf.Drawing.Color(Settings.Default.SNAPLINEColor.R, Settings.Default.SNAPLINEColor.G, Settings.Default.SNAPLINEColor.B);
            //    }
            //    else
            //    {
            //        brush.Color = new bbjuyhgazdf.Drawing.Color(Settings.Default.ESPALLIESCOLOR.R, Settings.Default.ESPALLIESCOLOR.G, Settings.Default.ESPALLIESCOLOR.B);
            //    }
            //}
            //else
            //{
            //    brush.Color = new bbjuyhgazdf.Drawing.Color(Settings.Default.SNAPLINEColor.R, Settings.Default.SNAPLINEColor.G, Settings.Default.SNAPLINEColor.B);
            //}

            //if (p.PlayerISALIVE > 0) //IF IS DEAD
            //{
            //    brush.Color = new bbjuyhgazdf.Drawing.Color(0, 0, 0);
            //}

            return brush;
        }

        private bbjuyhgazdf.Drawing.SolidBrush OVERLAYCOLOR01()
        {
            var brush = _brushes["OVERLAY01"];

            brush.Color = new bbjuyhgazdf.Drawing.Color(Settings.Default.MenuBorderColor.R, Settings.Default.MenuBorderColor.G, Settings.Default.MenuBorderColor.B);

            return brush;
        }

        private bbjuyhgazdf.Drawing.SolidBrush OVERLAYCOLOR02()
        {
            var brush = _brushes["OVERLAY02"];

            brush.Color = new bbjuyhgazdf.Drawing.Color(Settings.Default.DefaultBackgroundColor.R, Settings.Default.DefaultBackgroundColor.G, Settings.Default.DefaultBackgroundColor.B);

            return brush;
        }

        private bool GetActiveWindowTitle()
        {
            var handle = NativeMethods.GetForegroundWindow();
            var length = NativeMethods.GetWindowTextLength(handle);
            var builder = new StringBuilder(length + 1);

            NativeMethods.GetWindowText(handle, builder, builder.Capacity);

            if (builder.ToString().ToLower().Contains(processGame.ToLower()) == true || (builder.ToString().ToLower().Contains(processMainApp.ToLower()) == true))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool GetActiveWindowTitleAIMBOT()
        {
            var handle = NativeMethods.GetForegroundWindow();
            var length = NativeMethods.GetWindowTextLength(handle);
            var builder = new StringBuilder(length + 1);

            NativeMethods.GetWindowText(handle, builder, builder.Capacity);

            if (builder.ToString().ToLower().Contains(processMainApp.ToLower()) == true) return false;

            if (builder.ToString().ToLower().Contains(processGame.ToLower()) == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Run()
        {
            _window.Create();
            _window.Join();
        }

        public void Stop()
        {
            _window.Dispose();
        }

        public void Join()
        {
            _window.Join();
        }

        public void ReCreate()
        {
            _window.Recreate();
        }

        ~Example()
        {
            Dispose(false);
        }

        #region IDisposable Support
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
