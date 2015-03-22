using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Resources;

namespace WiFindUs.IO
{
    /// <summary>
    /// A resource cache designed to reduce memory overhead and ease disposal.
    /// </summary>
    /// <typeparam name="T">A class which implements IDisposable that represents something you'd want to load from disk or from the application resource cache,
    /// like an Image.</typeparam>
    public class ResourceLoader<T> : IDisposable where T : IDisposable
    {
        private Dictionary<string, T> fileObjects = new Dictionary<string, T>();
        private Dictionary<Assembly, Dictionary<string, T>> resourceObjects = new Dictionary<Assembly, Dictionary<string, T>>();
        private Dictionary<Assembly, ResourceReader> resourceReaders = new Dictionary<Assembly, ResourceReader>();
        private bool disposed = false;
        private Func<string, T> fileObjectLoader = null;
        private Type objectType;
        private string objectTypeName = "";

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        public ResourceLoader(Func<string, T> fileObjectLoader = null, string[] filePaths = null, string[] resourceNames = null)
        {
            objectType = typeof(T);
            objectTypeName = objectType.FullName;

            this.fileObjectLoader = fileObjectLoader;

            if (fileObjectLoader == null)
                Debugger.W("ResourceLoader<" + objectTypeName + ">: No file object loader function was passed; you can only use this Loader for embedded resources.");
            else if (filePaths != null && filePaths.Length > 0)
            {
                foreach (string path in filePaths)
                {
                    string p = path;
                    if (p == null || (p = p.Trim()).Length == 0)
                    {
                        Debugger.W("ResourceLoader<" + objectTypeName + ">: Skipping blank path!");
                        continue;
                    }
                    try
                    {
                        File(p);
                    }
                    catch (Exception e)
                    {
                        Debugger.Ex(e);
                    }
                }
            }

            if (resourceNames != null && resourceNames.Length > 0)
            {
                foreach (string resource in resourceNames)
                {
                    string p = resource;
                    if (p == null || (p = p.Trim()).Length == 0)
                    {
                        Debugger.W("ResourceLoader<" + objectTypeName + ">: Skipping blank resource!");
                        continue;
                    }
                    try
                    {
                        Resource(p);
                    }
                    catch (Exception e)
                    {
                        Debugger.Ex(e);
                    }
                }
            }
        }

        /////////////////////////////////////////////////////////////////////
        // PUBLIC METHODS
        /////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the resource from the given file. If it has not been loaded already, it will be loaded, otherwise it will be returned immediately.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>The loaded or cached object.</returns>
        public T File(string path)
        {
            if (fileObjectLoader == null)
                throw new ArgumentNullException("fileObjectLoader", "ResourceLoader<" + objectTypeName + ">.File(): No file loading function was set during construction.");
            
            if (path == null || (path = path.Trim()).Length == 0)
                throw new ArgumentException("ResourceLoader<" + objectTypeName + ">.File(): Path cannot be null or blank.");
            path = Path.GetFullPath(path);
            string key = path.ToLower();
            T fileObject = default(T);
            if (!fileObjects.TryGetValue(key, out fileObject))
            {
                if (!System.IO.File.Exists(path))
                    throw new FileNotFoundException("ResourceLoader<" + objectTypeName + ">.File(): File did not exist.", path);
                fileObject = fileObjects[key] = fileObjectLoader(path);
            }
            return fileObject;
        }

        public T Resource(string key, Assembly assembly = null)
        {
            if (key == null || (key = key.Trim().ToLower()).Length == 0)
                throw new ArgumentException("ResourceLoader<" + objectTypeName + ">: Key cannot be null or blank.");
            if (assembly == null)
                assembly = Assembly.GetCallingAssembly();

            //get the resource objects for the assembly
            Dictionary<string, T> assemblyResources = null;
            if (!resourceObjects.TryGetValue(assembly, out assemblyResources))
                assemblyResources = resourceObjects[assembly] = new Dictionary<string, T>();

            //get the resource
            T resourceObject = default(T);
            if (!assemblyResources.TryGetValue(key, out resourceObject))
            {
                //get the resource reader for the assembly
                ResourceReader reader = null;
                if (!resourceReaders.TryGetValue(assembly, out reader))
                    reader = resourceReaders[assembly] = new ResourceReader(
                        assembly.GetManifestResourceStream(assembly.GetName().Name + ".Properties.Resources.resources"));

                //search for the resource by key
                IDictionaryEnumerator resourceEnumerator = reader.GetEnumerator();
                resourceEnumerator.Reset();
                while (resourceEnumerator.MoveNext())
                {
                    string valueKey = resourceEnumerator.Key as string;

                    if (valueKey != null && (valueKey = valueKey.ToLower()).CompareTo(key) == 0)
                    {
                        object value = resourceEnumerator.Value;
                        Type valueType = (value == null ? null : value.GetType());
                        //check type (resources can be many types)
                        if (valueType != null && objectType.IsAssignableFrom(valueType))
                            resourceObject = assemblyResources[key] = (T)value;
                        break;
                    }
                }
            }
            return resourceObject;
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
                //clear file objects
                this.fileObjectLoader = null;
                foreach (KeyValuePair<string, T> resource in fileObjects)
                    resource.Value.Dispose();
                fileObjects.Clear();

                //clear resource objects
                foreach (KeyValuePair<Assembly, Dictionary<string, T>> resources in resourceObjects)
                {
                    foreach (KeyValuePair<string, T> resource in resources.Value)
                        resource.Value.Dispose();
                    resources.Value.Clear();
                }
                resourceObjects.Clear();

                //clear resource readers
                foreach (KeyValuePair<Assembly, ResourceReader> reader in resourceReaders)
                    reader.Value.Dispose();
                resourceReaders.Clear();
            }

            disposed = true;
        }
    }
}
