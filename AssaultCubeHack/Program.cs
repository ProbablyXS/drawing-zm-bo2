using Examples;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using VECRPROJECT.util;

namespace AssaultCubeHack
{

    class Program
    {

        [STAThread]

        static void Main(string[] args)
        {

            try
            {

                //if (args == null || args.Length == 0 || args[0].ToString() == "EyZNR0ygT7BUnEDTzcLRvje5es1b6syJim9AKNqhzoDKCKTv" == false)
                //{
                //    Security.FAILACC();
                //}
            }
            catch { Security.FAILACC(); }

            //SINGLE INSTANCE APP
            var appName = Assembly.GetEntryAssembly().GetName().Name;
            var notAlreadyRunning = true;
            using (var mutex = new Mutex(true, appName, out notAlreadyRunning))
            {
                if (notAlreadyRunning == false)
                {
                    Security.FAILACC();
                }
            }

            AppDomain.CurrentDomain.AssemblyResolve += (sender, bargs) =>
            {
                String dllName = new AssemblyName(bargs.Name).Name + ".dll";
                var assem = Assembly.GetExecutingAssembly();
                String resourceName = assem.GetManifestResourceNames().FirstOrDefault(rn => rn.EndsWith(dllName));
                if (resourceName == null) return null; // Not found, maybe another handler will find it
                using (var stream = assem.GetManifestResourceStream(resourceName))
                {
                    Byte[] assemblyData = new Byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }
            };

            try
            {

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                bbjuyhgazdf.TimerService.EnableHighPrecisionTimers();

                using (var example = new Example())
                {
                    example.Run();
                }
            }
            catch
            {
                Security.FAILACC();
            }

        }

    }
}
