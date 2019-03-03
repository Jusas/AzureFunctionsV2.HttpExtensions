using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AzureFunctionsV2.HttpExtensions.IL
{
    public static class AssemblyUtils
    {

        private static bool? _isIlModified;

        public static bool IsILModified()
        {
            if (_isIlModified == null)
            {
                var referringAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(p => !p.IsDynamic)
                    .Where(a => a.GetReferencedAssemblies()
                        .Any(r => r.Name == Assembly.GetAssembly(typeof(AssemblyUtils)).GetName().Name));

                var fodyMarkers = referringAssemblies.Select(a => a.GetType("ProcessedByFody"))
                    .Where(a => a != null)
                    .Distinct();
                var httpExtensionMarker = fodyMarkers.FirstOrDefault(
                    t => t.IsClass && 
                         t.GetFields(BindingFlags.NonPublic | BindingFlags.Static)
                             .Any(f => f.Name == "AzureFunctionsV2HttpExtensions"));

                if (httpExtensionMarker != null)
                    _isIlModified = true;
                else
                    _isIlModified = false;
            }

            return _isIlModified.Value;
        }
    }
}
