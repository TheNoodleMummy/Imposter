using Disqord;
using System;

namespace Discord.Addons.Interactive
{
    public class PaginatedAppearanceOptions
    {
        public static PaginatedAppearanceOptions Default = new PaginatedAppearanceOptions();

        public IEmoji First = new LocalEmoji("⏮");
        public IEmoji Back = new LocalEmoji("◀");
        public IEmoji Next = new LocalEmoji("▶");
        public IEmoji Last = new LocalEmoji("⏭");
        public IEmoji Stop = new LocalEmoji("⏹");
        public IEmoji Jump = new LocalEmoji("🔢");
        public IEmoji Info = new LocalEmoji("ℹ");

        public string FooterFormat = "Page {0}/{1}";
        public string InformationText = "This is a paginator. React with the respective icons to change page.";

        public JumpDisplayOptions JumpDisplayOptions = JumpDisplayOptions.WithManageMessages;
        public bool DisplayInformationIcon = true;

        public TimeSpan? Timeout = null;
        public TimeSpan InfoTimeout = TimeSpan.FromSeconds(30);
    }

    public enum JumpDisplayOptions
    {
        Never,
        WithManageMessages,
        Always
    }
}
