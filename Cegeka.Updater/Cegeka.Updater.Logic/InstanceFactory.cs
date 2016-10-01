using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;

namespace Cegeka.Updater.Logic
{
  public static class InstanceFactory
  {
    private static IUnityContainer sContainer;

    private static IUnityContainer Container
    {
      get
      {
        if (sContainer == null)
        {
          sContainer = new UnityContainer();
          var section = (UnityConfigurationSection)ConfigurationManager.GetSection("unity");
          if (section != null)
          {
            section.Configure(sContainer);
          }
        }
        return sContainer;
      }
    }

    public static void Register<I, T>()
    {
      Container.RegisterType(typeof(I), typeof(T));
    }

    public static void RegisterSingleton<I, T>()
    {
      Container.RegisterType(typeof(I), typeof(T), new ContainerControlledLifetimeManager());
    }

    public static T GetInstance<T>()
    {
      return Container.Resolve<T>();
    }
  }
}