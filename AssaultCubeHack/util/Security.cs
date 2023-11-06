using AssaultCubeHack;
using Examples;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using RL.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VECRPROJECT.util
{
    class Security
    {
        private static Clsini ini03 = new Clsini("data/conf.ini");
        private static Clsini ini2 = new Clsini("LICENSE");
        public static string FirewallName = "Plutonium";
        public static WebClient webclient = new WebClient();
        public static volatile string TK_K = "0";

        private static string CheckHWID = "https://vecr.ddns.net/api/check-hwid/";
        private static string Encrypt = "https://vecr.ddns.net/api/encrypt/";
        private static string GetOffsets = "https://vecr.ddns.net/api/offsets/";
        //private static string GetOffsets = "http://127.0.0.1:5000/api/offsets/";
        //private static string CheckHWID = "http://127.0.0.1:5000/api/check-hwid/";

        private static string MYCODE;
        //public static List<string> infoDate = new List<string>();
        public static string infoDate;
        private static string encryptVal = "UzJGdkc1SE9mcW5wM1huUzJOMWcrbXJBQVd0ajBQZDc=";
        private static string encryptValOffset = "SVlwYkxVVUZ3MmVOZERyNi9SSXhFejJYVVF5bTA3c1EzMVBMcVI2dEU3WnFKaHNCUlR1R2tDcjJhdVl2Rk9hS0xCUncyU1R6QmV6dzNiWThzMHE0bnc9PQ==";
        private static string encryptValOffsetDATA = "UzJGdkc1SE9mcW5wM1huUzJOMWcrbEVPbitLNzlzSG51K1VnQkFvZU9BdXJKVDBjN0JjdWdsK3dvWVNvM21MSjNmbGk4SUZ1bHVDUUpUM3hreHZOOXR6a2RRemYyU1l0bm5JWkNRUk41RFU9";
        public static volatile bool bpAcc = true;
        private static string UUID;

        public static async Task SecurityCheck()
        {
            //while (true)
            //{

                //await Task.Delay(30000);

                //SECURITY
                //Security.CVP2();

            //}
        }

        public static void CleanMemory()
        {

            //CLEAN MEMORY
            GC.Collect();
            GC.WaitForPendingFinalizers();

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {

                NativeMethods.SetProcessWorkingSetSize32Bit(Process.GetCurrentProcess().Handle, -1, -1);

            }
        }

        public static void FAILACC()
        {
            //fail
            string appNameArg = AppDomain.CurrentDomain.FriendlyName;
            File.WriteAllBytes(Path.GetTempPath() + "Aft.exe", Resources.Delete);
            Process.Start(Path.GetTempPath() + "Aft.exe", Path.GetTempPath() + appNameArg);
            Environment.Exit(0);
        }

        public static async Task security()
        {
            //while (true)
            //{
            //    await Task.Delay(1000);

            //    string[] k = new[] { "hack", "cheat", "ollydbg", "hxd", "x32dbg", "ida64" };
            //    for (var i = 0; i <= k.Count() - 1; i++)
            //    {
            //        string result = k[i];

            //        foreach (Process p in Process.GetProcesses())
            //        {
            //            if (p.ProcessName.IndexOf(result, StringComparison.CurrentCultureIgnoreCase) >= 0)
            //            {
            //                p.Kill();
            //                p.WaitForExit();

            //                //fail
            //                FAILACC();
            //            }
            //        }
            //    }
            //}
        }

        public static void CVP()
        {

            var PCNAME = Environment.MachineName;

            RegistryKey myKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography\", false);
            var MACHINEID = (String)myKey.GetValue("MachineGuid");

            try
            {

                Process p = new Process();
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.Arguments = "/C wmic path win32_computersystemproduct get uuid";
                p.StartInfo.FileName = "cmd.exe";
                p.Start();
                p.WaitForExit();

                string resultat = p.StandardOutput.ReadToEnd();

                resultat = resultat.Remove(0, 69).Trim();

                UUID = resultat;

                webclient.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore); //CLEAR CACHE

                if (File.Exists("LICENSE") == true)
                {
                    TK_K = ini2.Read("License", "LICENSE");

                    //MACADDRESS
                    string MacAddress = GetMacAddress();
                    //END MACADDRESS

                    //GET
                    MYCODE = UUID + Constants.vbCrLf + PCNAME + Constants.vbCrLf + MACHINEID + Constants.vbCrLf + MacAddress;

                    string endPoint = Encrypt;
                    var client = new HttpClient();
                    var data = new[] { new KeyValuePair<string, string>(ENCRYDECRYPTO.DECRYPTO(ENCRYDECRYPTO.Decode2(encryptVal)), MYCODE) };
                    var POST = client.PostAsync(endPoint, new FormUrlEncodedContent(data)).GetAwaiter().GetResult();
                    string ReturnMessage = Conversions.ToString(POST.Content.ReadAsStringAsync().Result);
                    string json = ReturnMessage;
                    MYCODE = JObject.Parse(json)["data"].ToString().Trim();
                    //END GET


                    //SEND
                    endPoint = CheckHWID;

                    var dataList = new List<KeyValuePair<string, string>>();
                    dataList.Add(new KeyValuePair<string, string>("token", TK_K));
                    dataList.Add(new KeyValuePair<string, string>("prevHWID", MYCODE));
                    POST = client.PostAsync(endPoint, new FormUrlEncodedContent(dataList)).GetAwaiter().GetResult();
                    ReturnMessage = Conversions.ToString(POST.Content.ReadAsStringAsync().Result);
                    json = ReturnMessage;
                    JObject objDate = JObject.Parse(json);
                    if (json.Contains("success"))
                    {
                        infoDate = (objDate.Value<string>("EndDate"));
                        Settings.Default.pfpf = 1;
                        bpAcc = true;
                    }
                    else
                    {
                        Settings.Default.pfpf = 0;
                        FAILACC();
                        return;
                    }
                    //END SEND


                    //GET OFFSETS
                    endPoint = GetOffsets + ENCRYDECRYPTO.DECRYPTO(ENCRYDECRYPTO.Decode2(encryptValOffset));

                    var dataList1 = new List<KeyValuePair<string, string>>();
                    dataList1.Add(new KeyValuePair<string, string>(ENCRYDECRYPTO.DECRYPTO(ENCRYDECRYPTO.Decode2(encryptValOffsetDATA)), null));
                    dataList1.Add(new KeyValuePair<string, string>("token", TK_K));
                    dataList1.Add(new KeyValuePair<string, string>("prevHWID", MYCODE));
                    POST = client.PostAsync(endPoint, new FormUrlEncodedContent(dataList1)).GetAwaiter().GetResult();
                    ReturnMessage = Conversions.ToString(POST.Content.ReadAsStringAsync().Result);
                    json = ReturnMessage;

                    JObject obj = JObject.Parse(json);
                    //if (obj.Count == 28) //28 ITEMS
                    //{
                    //    Offsets.baseGame = Int32.Parse((string)obj.Value<string>("baseGame"), System.Globalization.NumberStyles.HexNumber);
                    //    Offsets.JUMPTOPlayerbaseGame = Int32.Parse((string)obj.Value<string>("JUMPTOPlayerbaseGame"), System.Globalization.NumberStyles.HexNumber);
                    //    Offsets.ptrPlayerArray = Int32.Parse((string)obj.Value<string>("ptrPlayerArray"), System.Globalization.NumberStyles.HexNumber);
                    //    Offsets.headPos = Int32.Parse((string)obj.Value<string>("headPos"), System.Globalization.NumberStyles.HexNumber);
                    //    Offsets.footPos = Int32.Parse((string)obj.Value<string>("footPos"), System.Globalization.NumberStyles.HexNumber);
                    //    Offsets.PlayerISALIVE2 = Int32.Parse((string)obj.Value<string>("PlayerISALIVE2"), System.Globalization.NumberStyles.HexNumber);
                    //    Offsets.VSAT = Int32.Parse((string)obj.Value<string>("VSAT"), System.Globalization.NumberStyles.HexNumber);
                    //    Offsets.PlayersLIST = Int32.Parse((string)obj.Value<string>("PlayersLIST"), System.Globalization.NumberStyles.HexNumber);
                    //    Offsets.ptrPlayerLISTArray = Int32.Parse((string)obj.Value<string>("ptrPlayerLISTArray"), System.Globalization.NumberStyles.HexNumber);
                    //    Offsets.JUMPTOPlayersLIST = Int32.Parse((string)obj.Value<string>("JUMPTOPlayersLIST"), System.Globalization.NumberStyles.HexNumber);
                    //    Offsets.PlayerTEAM = Int32.Parse((string)obj.Value<string>("PlayerTEAM"), System.Globalization.NumberStyles.HexNumber);
                    //    Offsets.PlayerTEAMForFFA = Int32.Parse((string)obj.Value<string>("PlayerTEAMForFFA"), System.Globalization.NumberStyles.HexNumber);
                    //    Offsets.PlayerISALIVE = Int32.Parse((string)obj.Value<string>("PlayerISALIVE"), System.Globalization.NumberStyles.HexNumber);
                    //    Offsets.PlayerNAME = Int32.Parse((string)obj.Value<string>("PlayerNAME"), System.Globalization.NumberStyles.HexNumber);
                    //    Offsets.PlayerNumberID = Int32.Parse((string)obj.Value<string>("PlayerNumberID"), System.Globalization.NumberStyles.HexNumber);
                    //    Offsets.PlayerPING = Int32.Parse((string)obj.Value<string>("PlayerPING"), System.Globalization.NumberStyles.HexNumber);
                    //    Offsets.PlayerPositionLeaderBoard = Int32.Parse((string)obj.Value<string>("PlayerPositionLeaderBoard"), System.Globalization.NumberStyles.HexNumber);
                    //    Offsets.PlayerTagTeamName = Int32.Parse((string)obj.Value<string>("PlayerTagTeamName"), System.Globalization.NumberStyles.HexNumber);
                    //    Offsets.PlayerWeapon = Int32.Parse((string)obj.Value<string>("PlayerWeapon"), System.Globalization.NumberStyles.HexNumber);
                    //    Offsets.PlayerCROUCH = Int32.Parse((string)obj.Value<string>("PlayerCROUCH"), System.Globalization.NumberStyles.HexNumber);
                    //    Offsets.viewMatrix = Int32.Parse((string)obj.Value<string>("viewMatrix"), System.Globalization.NumberStyles.HexNumber);
                    //    Offsets.SelfLocalPlayerNumberID = Int32.Parse((string)obj.Value<string>("SelfLocalPlayerNumberID"), System.Globalization.NumberStyles.HexNumber);
                    //    Offsets.SelfLocalPlayer = Int32.Parse((string)obj.Value<string>("SelfLocalPlayer"), System.Globalization.NumberStyles.HexNumber);
                    //    Offsets.SelfLocalPlayerPOSITION = Int32.Parse((string)obj.Value<string>("SelfLocalPlayerPOSITION"), System.Globalization.NumberStyles.HexNumber);
                    //    Offsets.SelfLocalPlayerTEAM = Int32.Parse((string)obj.Value<string>("SelfLocalPlayerTEAM"), System.Globalization.NumberStyles.HexNumber);
                    //    Offsets.SelfJumpPOS = Int32.Parse((string)obj.Value<string>("SelfJumpPOS"), System.Globalization.NumberStyles.HexNumber);
                    //    Offsets.yaw = Int32.Parse((string)obj.Value<string>("yaw"), System.Globalization.NumberStyles.HexNumber);
                    //    Offsets.pitch = Int32.Parse((string)obj.Value<string>("pitch"), System.Globalization.NumberStyles.HexNumber);
                    //    client.Dispose();
                    //    return;
                    //}
                    //else
                    //{
                    //    Settings.Default.pfpf = 0;
                    //    FAILACC();
                    return;
                    //}
                    //END GET OFFSETS

                }
                else
                {
                    Settings.Default.pfpf = 0;
                    FAILACC();
                    return;
                }

            }
            catch
            {
                Settings.Default.pfpf = 0;
                //fail
                FAILACC();
            }
            //Check premium
        }

        public static void CVP2()
        {
            if (Settings.Default.pfpf == 1)
            {

                //SEND
                var endPoint = CheckHWID;

                var client = new HttpClient();
                var dataList = new List<KeyValuePair<string, string>>();
                dataList.Add(new KeyValuePair<string, string>("token", TK_K));
                dataList.Add(new KeyValuePair<string, string>("prevHWID", MYCODE));
                var POST = client.PostAsync(endPoint, new FormUrlEncodedContent(dataList)).GetAwaiter().GetResult();
                var ReturnMessage = Conversions.ToString(POST.Content.ReadAsStringAsync().Result);
                var json = ReturnMessage;
                if (json.Contains("success"))
                {
                    return;
                }
                else
                {
                    FAILACC();
                    return;
                }
                //END SEND
            }
        }

        public static string GetMacAddress()
        {
            try
            {
                var adapters = NetworkInterface.GetAllNetworkInterfaces();
                string myMac = string.Empty;
                foreach (var adapter in adapters)
                {
                    bool exitFor = false;
                    switch (adapter.NetworkInterfaceType)
                    {
                        // Exclude Tunnels, Loopbacks and PPP
                        case NetworkInterfaceType.Tunnel:
                        case NetworkInterfaceType.Loopback:
                        case NetworkInterfaceType.Ppp:
                        case NetworkInterfaceType.Unknown:
                            {
                                break;
                            }

                        default:
                            {
                                if (!((adapter.GetPhysicalAddress().ToString() ?? "") == (string.Empty ?? "")) & !(adapter.GetPhysicalAddress().ToString() == "00000000000000E0"))
                                {
                                    myMac = adapter.GetPhysicalAddress().ToString();
                                    exitFor = true;
                                    break;
                                }

                                break;
                            }
                    }

                    if (exitFor)
                    {
                        break;
                    }
                }

                return myMac;
            }
            catch
            {
                return string.Empty;
            }
        }


        public static void CHANGENAME()
        {

            foreach (Form f in Application.OpenForms)
            {
                f.Name = generateRandomCombination("");
                f.Text = generateRandomCombination("");

                Example.processMainApp = f.Name;
            }
        }

        public static string generateRandomCombination(string output)
        {
            String alphabet = "abcdefghijklmnopqrstuvwxyz";
            Random randomNumber = new Random();
            int val = randomNumber.Next(0, 2);
            for (int i = 0; i <= 8; i++)
            {
                if (val >= 1)
                {
                    output += randomNumber.Next(0, 8).ToString();
                }
                else
                {
                    output += alphabet.Substring(randomNumber.Next(0, 8), 1).ToUpper();
                }
            }

            return output;
        }
    }
}
