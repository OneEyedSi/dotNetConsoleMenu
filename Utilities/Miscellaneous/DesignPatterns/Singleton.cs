//-----------------------------------------------------------------------
// <copyright file="Singleton.cs" company="Datacom">
//     Copyright (c) 2011 Datacom. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Utilities.Miscellaneous.DesignPatterns
{
    /// <summary>
    /// Helper class for defining a singleton
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Singleton<T>
        where T : new()
    {
        private static T _instance;
        private static readonly object _lock = new object();

        public static T Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new T();
                    return _instance;
                }
            }
        }
    }
}
