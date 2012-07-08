namespace PowerShellConsole
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class ScriptEntryPoints
    {
        public string InvokeCurrentScript = "InvokeCurrentScript";
    
        private static ScriptEntryPoints scriptEntryPointsInstance;
        public static ScriptEntryPoints ScriptEntryPointsInstance
        {
            get
            {
                if (scriptEntryPointsInstance == null)
                {
                    scriptEntryPointsInstance = new ScriptEntryPoints();
                }

                return scriptEntryPointsInstance;
            }
        }
    }
}
