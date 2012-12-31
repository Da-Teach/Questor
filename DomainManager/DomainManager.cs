using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Onyx.DomainManager
{
    public interface IAssemblyLoader
    {
        void LoadAndRun(string file);
    }

    public class EntryPoint
    {
        [DllImport("User32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);

        [STAThread]
        public static int Entry(string args)
        {
            bool firstLoaded = false;
            while (true)
            {
                if (!firstLoaded)
                {
                    firstLoaded = true;
                    new OnyxDomain(args);
                }

                if ((GetAsyncKeyState(Keys.F11) & 1) == 1)
                {
                    new OnyxDomain(args);
                }

                Thread.Sleep(10);
            }
            return 0;
        }
    }

    public static class DomainManager
    {
        public static AppDomain CurrentDomain { get; set; }
        public static OnyxAssemblyLoader CurrentAssemblyLoader { get; set; }
    }

    public class OnyxAssemblyLoader : MarshalByRefObject, IAssemblyLoader
    {
        public OnyxAssemblyLoader()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        #region IAssemblyLoader Members

        public void LoadAndRun(string file)
        {
            Assembly asm = Assembly.Load(file);
            MethodInfo entry = asm.EntryPoint;
            //object o = asm.CreateInstance(entry.Name);
            
            entry.Invoke(null, null);
            //entry.Invoke(null, new object[] { new string[] { "foo", "bar" } });
        }

        #endregion

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name == Assembly.GetExecutingAssembly().FullName)
                return Assembly.GetExecutingAssembly();

            string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string shortAsmName = Path.GetFileName(args.Name);
            string fileName = Path.Combine(appDir, shortAsmName);

            if (File.Exists(fileName))
            {
                return Assembly.LoadFrom(fileName);
            }
            return Assembly.GetExecutingAssembly().FullName == args.Name ? Assembly.GetExecutingAssembly() : null;
        }
    }

    /// <summary> 
    /// The actual domain object we'll be using to load and run the Onyx binaries.
    /// </summary>
    public class OnyxDomain
    {
        private readonly Random _rand = new Random();
        public OnyxDomain(string assemblyName)
        {
            try
            {
                string appBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var ads = new AppDomainSetup { ApplicationBase = appBase, PrivateBinPath = appBase };
                DomainManager.CurrentDomain = AppDomain.CreateDomain("OnyxDomain_Internal_" + _rand.Next(0, 100000),
                                                                     null, ads);
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                DomainManager.CurrentAssemblyLoader =
                    (OnyxAssemblyLoader)
                    DomainManager.CurrentDomain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName,
                                                               typeof(OnyxAssemblyLoader).FullName);

                DomainManager.CurrentAssemblyLoader.LoadAndRun(
                    Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), assemblyName));
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            finally
            {
                DomainManager.CurrentAssemblyLoader = null;
                AppDomain.Unload(DomainManager.CurrentDomain);
            }
        }

        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                Assembly assembly = System.Reflection.Assembly.Load(args.Name);
                if (assembly != null)
                    return assembly;
            }
            catch
            {
                // ignore load error 
            }

            // *** Try to load by filename - split out the filename of the full assembly name
            // *** and append the base path of the original assembly (ie. look in the same dir)
            // *** NOTE: this doesn't account for special search paths but then that never
            //           worked before either.
            string[] Parts = args.Name.Split(',');
            string File = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + Parts[0].Trim() + ".dll";

            return System.Reflection.Assembly.LoadFrom(File);
        }
    }
}