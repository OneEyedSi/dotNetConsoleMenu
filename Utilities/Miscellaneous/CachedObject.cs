//
// $History: CachedObject.cs $
// 
// *****************  Version 1  *****************
// User: Brentfo      Date: 21/06/11   Time: 11:19a
// Created in $/UtilitiesClassLibrary/Utilities
// Added the CachedObject utility class to the library which simplifies
// the task of caching objects in the http application cache.
// 

using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Caching;

namespace Utilities
{
    /// <summary>
    /// This helper generic makes dealing with cached
    /// objects much easier.
    /// </summary>
    /// <typeparam name="TCached"></typeparam>
    public class CachedObject<TCached>
       where TCached : class
    {
        private static object _lockObject = new object();

        private LoadObject _loadObject = null;
        private int _cacheSeconds;
        private Cache _cache = null;

        public delegate TCached LoadObject();

        public CachedObject(LoadObject loadObject, int cacheSeconds)
        {
            if (loadObject == null)
                throw new ArgumentNullException("loadObject");

            _loadObject = loadObject;
            _cacheSeconds = cacheSeconds;
            _cache = HttpContext.Current != null ? HttpContext.Current.Cache : new Cache();
        }

        /// <summary>
        /// Get the cached object.
        /// </summary>
        public TCached Value
        {
            get
            {
                TCached obj = null;

                lock (_lockObject)
                {
                    obj = _cache[typeof(TCached).FullName] as TCached;
                    if (obj == null)
                    {
                        obj = _loadObject();
                        _cache.Insert(typeof(TCached).FullName, obj, null,
                           DateTime.UtcNow.AddSeconds(_cacheSeconds), Cache.NoSlidingExpiration);
                    }
                }

                return obj;
            }
        }

        /// <summary>
        /// Clear the cache for this item.
        /// </summary>
        public void Clear()
        {
            lock (_lockObject)
            {
                _cache.Remove(typeof(TCached).FullName);
            }
        }
    }
}
