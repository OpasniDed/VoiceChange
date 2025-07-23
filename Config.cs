using Exiled.API.Features;
using Exiled.API.Interfaces;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoiceChange.Enums;

namespace VoiceChange
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        [Description("Роль и питч голоса")]
        public Dictionary<RoleTypeId, float> pitchSettings { get; set; } = new()
        {
            { RoleTypeId.Scp049, 0.75f },
            { RoleTypeId.Scp106, 0.6f },
        };
        public List<RoleTypeId> allowedScps { get; set; } = new() { RoleTypeId.Scp049, RoleTypeId.Scp106, RoleTypeId.Scp096, RoleTypeId.Scp0492, RoleTypeId.Scp173, RoleTypeId.Scp939 };
        [Description("Питч для SCP-999")]
        public float scp999Pitch { get; set; } = 2f;
        [Description("Айди SCP-999 (CUSTOMROLES)")]
        public int scp999Id { get; set; } = 55555;
        [Description("Активация (NoClip или ServerSpecific)")]
        public ActivationType ActivationType { get; set; } = ActivationType.ServerSpecificSettings;
        [Description("Будут ли слышать СЦП в общем войсе")]
        public bool UseDefaultChat { get; set; } = false;
        [Description("Громкость")]
        public float volume { get; set; } = 5f;
        [Description("Минимальная дистанция")]
        public float MinDistance { get; set; } = 2f;
        [Description("Максимальная дистанция")]
        public float MaxDistance { get; set; } = 10f;

        [Description("Хинт который будет показан")]
        public Message ProximityEnabled { get; set; } = new()
        {
            Type = Exiled.API.Enums.MessageType.Hint,
            Content = "Включен общий чат",
            Duration = 2,
            Show = true
        };
        public Message ProximityDisabled { get; set; } = new()
        {
            Type = Exiled.API.Enums.MessageType.Hint,
            Content = "Выключен общий чат",
            Duration = 2,
            Show = true
        };
        public Message ShowHint { get; set; } = new()
        {
            Type = Exiled.API.Enums.MessageType.Broadcast,
            Content = "Вы можете включить общий указав бинд в: Settings -> ServerSpecificSettings",
            Duration = 3,
            Show = true
        };

        [Description("Текст по центру")]
        public string SettingHeaderLabel { get; set; } = "Общий чат";
        [Description("Бинд")]
        public int KeybindId { get; set; } = 200;
        [Description("Текст бинда")]
        public string KeybindLabel { get; set; } = "Включить общий чат";
        [Description("Описание бинла")]
        public string KeybindHintLabel { get; set; } = "Включить или выключить общий чат";
    }
}
