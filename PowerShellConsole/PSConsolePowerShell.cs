namespace PowerShellConsole
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using System.Reflection;

    public static class PSConsolePowerShell
    {
        static PowerShell powerShell;
        static Runspace rs;

        public static PowerShell PowerShellInstance
        {
            get
            {
                if (powerShell == null)
                {
                    PSConsoleRunspace.SessionStateProxy.SetVariable("sep", ScriptEntryPoints.ScriptEntryPointsInstance);

                    powerShell = PowerShell.Create();
                    powerShell.Runspace = PSConsoleRunspace;
                }

                return powerShell;
            }
        }

        internal static Runspace PSConsoleRunspace
        {
            get
            {
                if (rs == null)
                {
                    rs = RunspaceFactory.CreateRunspace();
                    rs.ThreadOptions = PSThreadOptions.UseCurrentThread;
                    rs.Open();
                }

                return rs;
            }
        }

        public static Collection<PSObject> ExecutePS(this string script)
        {
            return PowerShellInstance
                .AddScript(script)
                .AddCommand("Out-String")
                .Invoke();
        }

        public static void ExecuteScriptEntryPoint(this string scriptEntryPoint)
        {
            foreach (var file in GetScriptsToRun(scriptEntryPoint))
            {
                var scriptToRun = ". " + file;

                foreach (var item in scriptToRun.ExecutePS())
                {
                    Debug.WriteLine(item);
                }
            }
        }

        private static IEnumerable<string> GetScriptsToRun(string scriptEntryPoint)
        {
            var scriptDirectory = GetScriptsDirectory();

            if (Directory.Exists(scriptDirectory))
            {
                return Directory.GetFiles(scriptDirectory, scriptEntryPoint + "*.ps1").ToList();
            }

            var message = string.Format("{0} does not exist, no scripts to run.", scriptDirectory);
            Debug.WriteLine(message);
            return new List<string>();
        }

        private static string GetScriptsDirectory()
        {
            var location = Assembly.GetExecutingAssembly().Location;
            var path = Path.GetDirectoryName(location);

            return Path.Combine(path, "scripts");
        }
    }
}