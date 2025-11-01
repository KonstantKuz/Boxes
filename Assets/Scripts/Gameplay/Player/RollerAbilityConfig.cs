using Infrastructure;
using Infrastructure.QuestService;
using UnityEngine;

namespace Gameplay.Player
{
    [CreateAssetMenu(
        fileName = nameof(RollerAbilityConfig),
        menuName = GlobalParams.ConfigPath + nameof(RollerAbilityConfig)
    )]
    public class RollerAbilityConfig : ScriptableObject
    {
        [SerializeField]
        private Quest requiredQuest;

        [SerializeField]
        private float speedModifier;

        [SerializeField]
        private float frictionModifier;

        public Quest RequiredQuest => requiredQuest;
        public float SpeedModifier => speedModifier;
        public float FrictionModifier => frictionModifier;
    }
}
