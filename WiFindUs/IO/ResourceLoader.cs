using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WiFindUs.IO
{
    /// <summary>
    /// A generic resource pool class designed to load Disposable resources from disk.
    /// </summary>
    /// <typeparam name="T">A class which implements IDisposable that represents something you'd want to load from disk, like an Image.</typeparam>
    public class ResourceLoader<T> : IDisposable where T : IDisposable
    {
        private Dictionary<string, T> resources = new Dictionary<string, T>();
        private bool disposed = false;
        private Func<string, T> loader = null;
        private string typeName = "";

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        public ResourceLoader(Func<string, T> loader, params string[] paths)
        {
            typeName = typeof(T).FullName;
            
            if (loader == null)
                throw new ArgumentNullException("loader");
            this.loader = loader;          
  
            if (paths == null || paths.Length == 0)
                return;

            foreach (string path in paths)
            {
                string p = path;
                if (p == null || (p = p.Trim()).Length == 0)
                {
                    Debugger.W("ResourceLoader<" + typeName + ">: Skipping blank path!");
                    continue;
                }
                Get(p);
            }
        }

        /////////////////////////////////////////////////////////////////////
        // PUBLIC METHODS
        /////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the resource. If it has not been loaded already, it will be loaded, otherwise it will be returned immediately.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public T Get(string path)
        {
            if (path == null || (path = path.Trim()).Length == 0)
                throw new ArgumentException("ResourceLoader<" + typeName + ">: Path cannot be null or blank.");
            path = Path.GetFullPath(path);
            string key = path.ToLower();
            T resource = default(T);
            if (!resources.TryGetValue(key, out resource))
            {
                if (!File.Exists(path))
                    throw new FileNotFoundException("ResourceLoader<" + typeName + ">: Resource file did not exist.", path);
                resource = resources[key] = loader(path);
            }
            return resource;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); 
        }

        /////////////////////////////////////////////////////////////////////
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                this.loader = null;
                foreach (KeyValuePair<string,T> resource in resources)
                    resource.Value.Dispose();
                resources.Clear();
            }
            disposed = true;
        }
    }
}
