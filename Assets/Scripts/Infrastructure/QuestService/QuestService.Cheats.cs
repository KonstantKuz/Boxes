#if DEBUG
using System;
using System.Collections.Generic;
using System.Reflection;
using Infrastructure.Cheats;
using Infrastructure.QuestService.Abstract;
using UnityEngine;

namespace Infrastructure.QuestService
{
    public partial class QuestService : ICheatsProvider
    {
        private GUILayoutDropdown<Quest> questDropdown;
        private GUILayoutDropdown<int> taskDropdown;

        bool ICheatsProvider.IsOpen { get; set; }

        string ICheatsProvider.GetLabel() => "Quest Cheats";

        void ICheatsProvider.RenderCheats()
        {
            if (questDropdown == null && quests != null && quests.Count > 0)
            {
                questDropdown = new GUILayoutDropdown<Quest>(quests.ToArray(), currentQuestIndex, quest => quest.name);

                taskDropdown = new GUILayoutDropdown<int>(
                    MakeTaskIndices(currentQuestIndex),
                    currentTaskIndex,
                    index => MakeTaskName(currentQuestIndex, index)
                );
            }

            GUILayout.Label("Quest:");
            questDropdown.OnGUI();

            if (questDropdown.SelectedIndex != currentQuestIndex)
            {
                currentQuestIndex = questDropdown.SelectedIndex;
                currentTaskIndex = 0;
                taskDropdown = new GUILayoutDropdown<int>(
                    MakeTaskIndices(currentQuestIndex),
                    0,
                    index => MakeTaskName(currentQuestIndex, index)
                );
            }

            GUILayout.Label("Task:");
            taskDropdown.OnGUI();
            currentTaskIndex = taskDropdown.SelectedIndex;

            if (GUILayout.Button("Change quest"))
            {
                ((IQuestService)this).RestartQuest();
            }
        }

        private string MakeTaskName(int questIndex, int taskIndex)
        {
            if (questIndex < 0 || questIndex >= quests.Count)
            {
                return taskIndex.ToString();
            }

            List<ITask> tasks = quests[questIndex].TaskSequence.Tasks;
            if (taskIndex < 0 || taskIndex >= tasks.Count)
            {
                return taskIndex.ToString();
            }

            ITask task = tasks[taskIndex];

            FieldInfo field = task.GetType().GetField("config", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                TaskDescription description = field.GetValue(task) as TaskDescription;
                if (description != null && description.Title != null)
                {
                    return description.Title.GetLocalizedString();
                }
            }

            return task.GetType().Name;
        }

        private int[] MakeTaskIndices(int questIndex)
        {
            if (questIndex < 0 || questIndex >= quests.Count)
            {
                return Array.Empty<int>();
            }

            int count = quests[questIndex].TaskSequence.TaskCount;
            int[] result = new int[count];

            for (int i = 0; i < count; i++)
            {
                result[i] = i;
            }

            return result;
        }
    }
}
#endif
