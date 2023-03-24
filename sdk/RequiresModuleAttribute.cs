/*

    Copyright (c) 2023 Pocketz World. All rights reserved.

*/

namespace Highrise.API
{
    /// <summary>
    /// Adds a module requirement to a module.  Will automatically cause 
    /// the required module to be added to the bot if not already added.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RequiresModuleAttribute : Attribute
    {
        /// <summary>
        /// Module type that is required
        /// </summary>
        public Type ModuleType { get; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public RequiresModuleAttribute(Type type)
        {
            ModuleType = type;
        }
    }
}
