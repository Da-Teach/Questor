namespace TreeTest1
{
    using System;
    using System.Reflection;
    using System.Linq;
    using System.Windows.Forms;
    using InnerSpaceAPI;
    using DirectEve;
    using TreeSharp;

    public partial class MainForm : Form
    {
        [AttributeUsage(AttributeTargets.All)]
        public class Test : System.Attribute
        {
            public readonly string _desc;

            public Test(string desc)
            {
                this._desc = desc;
            }

            public Test()
            {
                this._desc = null;
            }
        }

        private int _frameCount;
        private DateTime _lastFrame;
        private DateTime _lastPulse;
        private DateTime _lastSessionChange;
        private DirectEve _directEve;
        private Composite _behavior = null;
        private string _activeBehavior;

        public MainForm()
        {
            InitializeComponent();

            Type t = typeof(MainForm);
            foreach (MethodInfo mi in t.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                object[] attrs = mi.GetCustomAttributes(typeof(Test),false);
                foreach (object attr in attrs)
                {
                    cbBehaviors.Items.Add(mi.Name);
                }
            }
            cbBehaviors.SelectedIndex = 0;

            _directEve = new DirectEve(new StandaloneFramework());
            Cache.Instance.DirectEve = _directEve;
            _directEve.OnFrame += OnFrame;
        }

        private void Log(string format, params object[] parms)
        {
            InnerSpace.Echo(string.Format("{0:HH:mm:ss} {1}", DateTime.Now, string.Format(format, parms)));
        }

        private void OnFrame(object sender, EventArgs e)
        {
            if (_directEve == null)
                return;

            try
            {
                _frameCount++;

                _lastFrame = DateTime.Now;
                // Only pulse state changes every 1.5s
                if (DateTime.Now.Subtract(_lastPulse).TotalMilliseconds < (int)1500)
                    return;
                _lastPulse = DateTime.Now;

                if (_behavior != null)
                {
                    _behavior.Tick(this);
                }
            }
            catch (Exception ex)
            {
                Log("Exception: {0}", ex);
            }
        }

        private void InvokeCreateBehaviorMethod(string methodName)
        {
            if (!String.IsNullOrEmpty(methodName))
            {
                Type t = typeof(MainForm);
                MethodInfo mi = t.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
                object[] attrs = mi.GetCustomAttributes(typeof(Test),false);
                foreach (object attr in attrs)
                {
                    Test ta = (Test)attr;
                    if (String.IsNullOrEmpty(ta._desc))
                    {
                        Log("Creating {0} behavior...", mi.Name);
                    }
                    else
                    {
                        Log("Creating {0} behavior...", ta._desc);
                    }
                }
                _behavior = (Composite)mi.Invoke(this, null);
            }
        }

        [Test("Simple Undock")]
        private Composite CreateBehavior_SimpleUndock()
        {
            return (
                new Sequence(
                    // Session must be ready (i.e. we are logged in)...
                    new Decorator(ret => Cache.Instance.DirectEve.Session.IsReady, new ActionAlwaysSucceed()),

                    // We must be in station...
                    new Decorator(ret => Cache.Instance.InStation, new ActionAlwaysSucceed()),
   
                    new TreeSharp.Action(delegate { Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdExitStation); }),
   
                    // Wait for undock to start...
                    new Wait(6, ret => !Cache.Instance.InStation, new ActionAlwaysSucceed()),
   
                    // Wait for undock to complete...
                    new WaitContinue(6, ret => !Cache.Instance.InSpace, new ActionAlwaysSucceed())
                ));
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _directEve.Dispose();
            _directEve = null;
        }

        private void btnStartStop_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_activeBehavior))
            {
                _activeBehavior = cbBehaviors.Text;
                InvokeCreateBehaviorMethod(_activeBehavior);
                if (_behavior != null)
                {
                    _behavior.Start(this);
                }
                btnStartStop.Text = "Stop";
            }
            else
            {
                _activeBehavior = null;
                if (_behavior != null)
                {
                    _behavior.Stop(this);
                }
                _behavior = null;
                btnStartStop.Text = "Start";
            }
        }
    }
}
