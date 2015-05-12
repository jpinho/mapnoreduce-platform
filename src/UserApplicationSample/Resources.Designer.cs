﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.0
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace UserApplicationSample {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("UserApplicationSample.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ##############################
        ///#  User Application Sample
        ///#  Version: 1.0
        ///##############################
        ///
        /// Parameters:
        ///
        ///    &lt;ENTRY-URL&gt;     The URL of a worker node to which the client can connect to submit jobs.
        ///
        ///    &lt;FILE&gt;          Is the path to the input file.
        ///                    The file will be subdivided into &lt;S&gt; splits across the machines in W.
        ///
        ///    &lt;OUTPUT&gt;        Is the path to an output directory on the local filesystem of the application,
        ///                    which will store one out [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string HelpMessage {
            get {
                return ResourceManager.GetString("HelpMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Job submitted waiting to completed! Please keep calm and be patient!.
        /// </summary>
        internal static string JOB_SUBMIT_WAIT {
            get {
                return ResourceManager.GetString("JOB_SUBMIT_WAIT", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to User Application, started with the following parameters:
        ///-EntryURL={0} -FilePath={1} -OutputPath={2} -Splits={3} -MapClassName={4} -AssemblyFilePath={5}.
        /// </summary>
        internal static string USER_APP_LOG {
            get {
                return ResourceManager.GetString("USER_APP_LOG", resourceCulture);
            }
        }
    }
}
