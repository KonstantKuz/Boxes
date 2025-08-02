using System.Collections.Generic;
using System.Linq;
using Infrastructure.Abstract;
using Reflex.Core;
using UnityEngine;

namespace Configuration
{
    public class MediatorInstaller : MonoBehaviour, IInstaller
    {
        [SerializeReference]
        private List<IServiceMediator> serviceMediators;

        void IInstaller.InstallBindings(ContainerBuilder containerBuilder)
        {
            foreach (IServiceMediator serviceMediator in serviceMediators)
            {
                containerBuilder.AddSingleton(serviceMediator, serviceMediator.GetType().GetInterfaces());
                string names = string.Join(", ", serviceMediator.GetType().GetInterfaces().Select(item => item.Name));
                Debug.Log("Install mediator: " + serviceMediator.GetType().Name + " with interfaces: " + names);
            }
        }
    }
}
