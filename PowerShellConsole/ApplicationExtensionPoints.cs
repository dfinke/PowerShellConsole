namespace PowerShellConsole
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class ApplicationExtensionPoints
    {
        public string InvokeCurrentScript = "InvokeCurrentScript";
        public string InvokeInitializeConsole = "InvokeInitializeConsole";

        private static ApplicationExtensionPoints applicationExtensionPointsInstance;
        public static ApplicationExtensionPoints ApplicationExtensionPointsInstance
        {
            get
            {
                if (applicationExtensionPointsInstance == null)
                {
                    applicationExtensionPointsInstance = new ApplicationExtensionPoints();
                }

                return applicationExtensionPointsInstance;
            }
        }
    }
}
