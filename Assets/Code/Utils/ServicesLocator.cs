using System;
using System.Collections.Generic;
using UnityEngine;

namespace Services
{
    /// <summary>
    /// The ServiceLocator class is a useful tool for managing services in a large codebase. It provides a centralized
    /// location for registering, retrieving, and unregistering services that implement the IService interface.
    /// The class uses a dictionary to store registered services, and it ensures that only one instance of each service
    /// type can be registered.
    ///
    ///To use the ServiceLocator, you first need to call the Initialize() method to create a singleton instance of
    /// the class. Once the instance is created, you can use the Register<T>() method to add services to the registry,
    /// and you can use the Get<T>() method to retrieve services from the registry. If a requested service has not
    /// been registered, an exception will be thrown.
    /// </summary>
    public class ServiceLocator
    {
        /// <summary>
        /// Holds a dictionary of registered services.
        /// </summary>
        private readonly Dictionary<string, IService> _services = new Dictionary<string, IService>();
    
        /// <summary>
        /// Private constructor to prevent direct instantiation of the class from outside the class.
        /// </summary>
        private ServiceLocator() { }
    
        /// <summary>
        /// Gets the singleton instance of the ServiceLocator class.
        /// </summary>
        public static ServiceLocator Instance { get; private set; }

        /// <summary>
        /// Creates a new instance of the ServiceLocator class and sets it as the value of the Instance property.
        /// This method must be called before using any other methods of the class.
        /// </summary>
        public static void Initialize()
        {
            Instance = new ServiceLocator();
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// Retrieves the instance of the service specified by the generic type parameter T.
        /// </summary>
        /// <typeparam name="T">The type of the service to retrieve. Must implement the IService interface.</typeparam>
        /// <returns>The instance of the service as a strongly-typed object of type T.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the service has not been registered with the ServiceLocator instance.</exception>
        public T Get<T>() where T : IService
        {
            string key = typeof(T).Name;
            if (_services.ContainsKey(key))
            {
                return (T)_services[key];
            }
            else
            {
                Debug.LogError($"{key} not registered with {GetType().Name}");
                throw new InvalidOperationException();
            }
        }
    
        /// <summary>
        /// Registers a service instance with the ServiceLocator instance.
        /// </summary>
        /// <typeparam name="T">The type of the service to register. Must implement the IService interface.</typeparam>
        /// <param name="service">The instance of the service to register.</param>
        /// <exception cref="ArgumentException">Thrown if a service of the same type has already been registered with the ServiceLocator instance.</exception>
        public void Register<T>(T service) where T : IService
        {
            string key = typeof(T).Name;
            if (!_services.ContainsKey(key))
            {
                _services.Add(key, service);
            }
            else
            {
                Debug.LogError($"Attempted to register service of type {key} which is already registered with the {GetType().Name}.");
                return;
            }
        }
    
        /// <summary>
        /// Unregisters a service instance from the ServiceLocator instance.
        /// </summary>
        /// <typeparam name="T">The type of the service to unregister. Must implement the IService interface.</typeparam>
        /// <exception cref="ArgumentException">Thrown if the service has not been registered with the ServiceLocator instance.</exception>
        public void Unregister<T>() where T : IService
        {
            string key = typeof(T).Name;
            if (_services.ContainsKey(key))
            {
                _services.Remove(key);
            }
            else
            {
                Debug.LogError($"Attempted to unregister service of type {key} which is not registered with the {GetType().Name}.");
                return;
            }
        }
    }
}