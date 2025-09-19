using AutoMapper;
using Business.Services;
using DataAccess;
using DataAccess.Repositories;
using DataAccess.Interfaces;
using Domain.Interfaces.Services;
using Domain.Mappings;
using System.Web.Mvc;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Web;

namespace CafeMenu.App_Start
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
            // Simple Service Locator Pattern kullanarak Dependency Injection
            var services = new ServiceContainer();

            // AutoMapper konfigürasyonu
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            var mapper = config.CreateMapper();
            services.RegisterSingleton<IMapper>(mapper);

            // Entity Framework DbContext
            services.RegisterScoped<CafeMenuEntities, CafeMenuEntities>();

            // Unit of Work
            services.RegisterScoped<IUnitOfWork, UnitOfWork>();

            // Repository'ler
            services.RegisterScoped<ICategoryRepository, CategoryRepository>();
            services.RegisterScoped<IProductRepository, ProductRepository>();
            services.RegisterScoped<IUserRepository, UserRepository>();
            services.RegisterScoped<IPropertyRepository, PropertyRepository>();
            services.RegisterScoped<IProductPropertyRepository, ProductPropertyRepository>();
            services.RegisterScoped<ITenantRepository, TenantRepository>();

            // Services
            services.RegisterSingleton<ICacheService, MemoryCacheService>();
            services.RegisterScoped<ICategoryService, CategoryService>();
            services.RegisterScoped<IProductService, ProductService>();
            services.RegisterScoped<IUserService, UserService>();
            services.RegisterScoped<IPropertyService, PropertyService>();
            services.RegisterScoped<IProductPropertyService, ProductPropertyService>();
            services.RegisterScoped<ITenantService, TenantService>();

            // Controllers'ları da kaydet
            services.RegisterScoped<CafeMenu.Controllers.HomeController>();
            services.RegisterScoped<CafeMenu.Controllers.MenuController>();
            services.RegisterScoped<CafeMenu.Controllers.AuthController>();
            services.RegisterScoped<CafeMenu.Controllers.AdminController>();
            services.RegisterScoped<CafeMenu.Controllers.CategoryController>();
            services.RegisterScoped<CafeMenu.Controllers.ProductController>();
            services.RegisterScoped<CafeMenu.Controllers.TenantController>();
            services.RegisterScoped<CafeMenu.Controllers.SetupController>();

            // Cache Manager ve Performance Monitor (Circular dependency'yi önlemek için basit factory kullan)
            // Bu servisler isteğe bağlı olarak kullanılacak

            // MVC Dependency Resolver'ı ayarla
            DependencyResolver.SetResolver(new SimpleServiceLocator(services));
        }
    }

    // Basit Service Locator implementasyonu
    public class ServiceContainer
    {
        private readonly Dictionary<Type, ServiceDescriptor> _services = new Dictionary<Type, ServiceDescriptor>();

        public void RegisterSingleton<TInterface, TImplementation>()
            where TImplementation : class, TInterface
        {
            _services[typeof(TInterface)] = new ServiceDescriptor(typeof(TImplementation), ServiceLifetime.Singleton);
        }

        public void RegisterSingleton<TInterface>(TInterface instance)
        {
            _services[typeof(TInterface)] = new ServiceDescriptor(instance, ServiceLifetime.Singleton);
        }

        public void RegisterSingleton<TInterface>(Func<TInterface> factory)
        {
            _services[typeof(TInterface)] = new ServiceDescriptor(factory, ServiceLifetime.Singleton);
        }

        public void RegisterScoped<TInterface, TImplementation>()
            where TImplementation : class, TInterface
        {
            _services[typeof(TInterface)] = new ServiceDescriptor(typeof(TImplementation), ServiceLifetime.Scoped);
        }

        public void RegisterScoped<TImplementation>()
            where TImplementation : class
        {
            _services[typeof(TImplementation)] = new ServiceDescriptor(typeof(TImplementation), ServiceLifetime.Scoped);
        }

        public void RegisterScoped<TInterface>(Func<TInterface> factory)
        {
            _services[typeof(TInterface)] = new ServiceDescriptor(factory, ServiceLifetime.Scoped);
        }

        public ServiceDescriptor GetService(Type serviceType)
        {
            _services.TryGetValue(serviceType, out var descriptor);
            return descriptor;
        }

        public IEnumerable<ServiceDescriptor> GetServices(Type serviceType)
        {
            var descriptor = GetService(serviceType);
            return descriptor != null ? new[] { descriptor } : Enumerable.Empty<ServiceDescriptor>();
        }
    }

    public class ServiceDescriptor
    {
        public Type ImplementationType { get; }
        public object Instance { get; }
        public Func<object> Factory { get; }
        public ServiceLifetime Lifetime { get; }

        public ServiceDescriptor(Type implementationType, ServiceLifetime lifetime)
        {
            ImplementationType = implementationType;
            Lifetime = lifetime;
        }

        public ServiceDescriptor(object instance, ServiceLifetime lifetime)
        {
            Instance = instance;
            Lifetime = lifetime;
        }

        public ServiceDescriptor(Func<object> factory, ServiceLifetime lifetime)
        {
            Factory = factory;
            Lifetime = lifetime;
        }
    }

    public enum ServiceLifetime
    {
        Singleton,
        Scoped,
        Transient
    }

    public class SimpleServiceLocator : IDependencyResolver
    {
        private readonly ServiceContainer _container;
        private readonly Dictionary<Type, object> _singletonInstances = new Dictionary<Type, object>();
        
        // HttpContext-based scoped instances per request
        private const string SCOPED_INSTANCES_KEY = "__ScopedInstances__";

        public SimpleServiceLocator(ServiceContainer container)
        {
            _container = container;
        }

        private Dictionary<Type, object> GetScopedInstances()
        {
            if (System.Web.HttpContext.Current != null)
            {
                var items = System.Web.HttpContext.Current.Items;
                if (!items.Contains(SCOPED_INSTANCES_KEY))
                {
                    items[SCOPED_INSTANCES_KEY] = new Dictionary<Type, object>();
                }
                return (Dictionary<Type, object>)items[SCOPED_INSTANCES_KEY];
            }
            
            // Fallback for non-web scenarios
            return new Dictionary<Type, object>();
        }

        public object GetService(Type serviceType)
        {
            var descriptor = _container.GetService(serviceType);
            
            // Eğer service bulunamazsa ve bu bir controller ise, doğrudan oluşturmayı dene
            if (descriptor == null && serviceType.Name.EndsWith("Controller"))
            {
                try
                {
                    return CreateInstanceWithDependencies(serviceType);
                }
                catch
                {
                    return null;
                }
            }
            
            if (descriptor == null) return null;

            return CreateInstance(descriptor);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            var descriptors = _container.GetServices(serviceType);
            return descriptors.Select(CreateInstance);
        }

        private object CreateInstance(ServiceDescriptor descriptor)
        {
            if (descriptor.Instance != null)
                return descriptor.Instance;

            if (descriptor.Factory != null)
            {
                // Factory'den dönen tipi belirlemek için generic parametreyi kullan
                var factoryType = descriptor.Factory.GetType();
                Type returnType = null;
                
                if (factoryType.IsGenericType)
                {
                    var genericArgs = factoryType.GetGenericArguments();
                    if (genericArgs.Length > 0)
                        returnType = genericArgs[0];
                }
                
                if (returnType == null)
                    returnType = descriptor.Factory.Method.ReturnType;

                switch (descriptor.Lifetime)
                {
                    case ServiceLifetime.Singleton:
                        if (!_singletonInstances.ContainsKey(returnType))
                            _singletonInstances[returnType] = descriptor.Factory();
                        return _singletonInstances[returnType];
                    case ServiceLifetime.Scoped:
                        var scopedInstances = GetScopedInstances();
                        if (!scopedInstances.ContainsKey(returnType))
                            scopedInstances[returnType] = descriptor.Factory();
                        return scopedInstances[returnType];
                    default:
                        return descriptor.Factory();
                }
            }

            if (descriptor.ImplementationType != null)
            {
                switch (descriptor.Lifetime)
                {
                    case ServiceLifetime.Singleton:
                        if (!_singletonInstances.ContainsKey(descriptor.ImplementationType))
                            _singletonInstances[descriptor.ImplementationType] = CreateInstanceWithDependencies(descriptor.ImplementationType);
                        return _singletonInstances[descriptor.ImplementationType];
                    case ServiceLifetime.Scoped:
                        var scopedInstances = GetScopedInstances();
                        if (!scopedInstances.ContainsKey(descriptor.ImplementationType))
                            scopedInstances[descriptor.ImplementationType] = CreateInstanceWithDependencies(descriptor.ImplementationType);
                        return scopedInstances[descriptor.ImplementationType];
                    default:
                        return CreateInstanceWithDependencies(descriptor.ImplementationType);
                }
            }

            return null;
        }

        private object CreateInstanceWithDependencies(Type type)
        {
            var constructors = type.GetConstructors();
            var constructor = constructors.OrderByDescending(c => c.GetParameters().Length).First();
            var parameters = constructor.GetParameters();
            var args = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                var dependency = GetService(parameters[i].ParameterType);
                if (dependency == null)
                {
                    throw new InvalidOperationException($"Could not resolve dependency '{parameters[i].ParameterType.Name}' for type '{type.Name}'");
                }
                args[i] = dependency;
            }

            return Activator.CreateInstance(type, args);
        }
    }
}