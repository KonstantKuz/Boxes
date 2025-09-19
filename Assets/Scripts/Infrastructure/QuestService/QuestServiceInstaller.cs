using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Infrastructure.QuestService.Abstract;
using Infrastructure.QuestService.State;
using Reflex.Core;
using UnityEngine;

namespace Infrastructure.QuestService
{
    public class QuestServiceInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField]
        private ActiveQuestStateHolder questStateHolder;

        [SerializeReference]
        private IQuestService questService;

        void IInstaller.InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.AddSingleton(questStateHolder, questStateHolder.GetType().GetInterfaces());
            containerBuilder.AddSingleton(questService, questService.GetType().GetInterfaces());

            IEnumerable<Type> types = Assembly
                .GetAssembly(typeof(ITask))
                .GetTypes()
                .OrderBy(type => type.FullName)
                .Where(type => typeof(ITask).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);

            foreach (Type taskType in types)
            {
                containerBuilder.AddTransient(taskType);
            }
        }
    }
}
