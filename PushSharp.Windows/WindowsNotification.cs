using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using PushSharp.Core;

namespace PushSharp.Windows
{
    public abstract class WindowsNotification : Notification
    {
        protected WindowsNotification()
        {
        }
        public string NotificationTag { get; set; }
        public string NotificationGroup { get; set; }

        public string ChannelUri { get; set; }

        public bool? RequestForStatus { get; set; }
        public int? TimeToLive { get; set; }

        public abstract string PayloadToString();

        public abstract WindowsNotificationType Type { get; }

        protected string XmlEncode(string text)
        {
            return System.Security.SecurityElement.Escape(text);
        }
        public override string ToString()
        {
            return GetType() + ": " + ChannelUri;
        }
    }

    public class WindowsTileNotification : WindowsNotification
    {
        public WindowsTileNotification()
            : base()
        {
            Visual = new TileVisual();
        }

        public override WindowsNotificationType Type
        {
            get { return WindowsNotificationType.Tile; }
        }

        public WindowsNotificationCachePolicyType? CachePolicy { get; set; }


        public TileVisual Visual { get; set; }

        public override string PayloadToString()
        {
            var tile = new XElement("tile");

            if (Visual != null)
                tile.Add(Visual.GenerateXmlElement());

            return tile.ToString();
        }
    }

    public class TileBinding
    {
        public TileBinding()
        {
            Images = new List<TileImage>();
            Texts = new List<TileText>();
        }

        public TileNotificationTemplate TileTemplate { get; set; }
        public string Fallback { get; set; }
        public string Language { get; set; }
        public string BaseUri { get; set; }
        public BrandingType? Branding { get; set; }
        public bool? AddImageQuery { get; set; }

        public List<TileImage> Images { get; set; }
        public List<TileText> Texts { get; set; }

        public XElement GenerateXmlElement()
        {
            var binding = new XElement("binding", new XAttribute("template", this.TileTemplate.ToString()));

            if (!string.IsNullOrEmpty(Fallback))
                binding.Add(new XAttribute("fallback", XmlEncode(Fallback)));

            if (!string.IsNullOrEmpty(Language))
                binding.Add(new XAttribute("lang", XmlEncode(Language)));

            if (Branding.HasValue)
                binding.Add(new XAttribute("branding", XmlEncode(Branding.Value.ToString().ToLowerInvariant())));

            if (!string.IsNullOrEmpty(BaseUri))
                binding.Add(new XAttribute("baseUri", XmlEncode(BaseUri)));

            if (AddImageQuery.HasValue)
                binding.Add(new XAttribute("addImageQuery", AddImageQuery.Value.ToString().ToLowerInvariant()));

            int idOn = 1;

            if (Images != null)
            {
                foreach (var img in Images)
                    binding.Add(img.GenerateXmlElement(idOn++));
            }

            idOn = 1;

            if (Texts != null)
            {
                foreach (var text in Texts)
                    binding.Add(text.GenerateXmlElement(idOn++));
            }

            return binding;
        }

        protected string XmlEncode(string text)
        {
            return System.Security.SecurityElement.Escape(text);
        }
    }

    public class TileVisual
    {
        public TileVisual()
        {
            Bindings = new List<TileBinding>();
        }

        public int? Version { get; set; }
        public string Language { get; set; }
        public string BaseUri { get; set; }
        public BrandingType? Branding { get; set; }
        public bool? AddImageQuery { get; set; }

        public List<TileBinding> Bindings { get; set; }

        public XElement GenerateXmlElement()
        {
            var visual = new XElement("visual");

            if (Version.HasValue)
                visual.Add(new XAttribute("version", Version.Value.ToString()));

            if (!string.IsNullOrEmpty(Language))
                visual.Add(new XAttribute("lang", XmlEncode(Language)));

            if (!string.IsNullOrEmpty(BaseUri))
                visual.Add(new XAttribute("baseUri", XmlEncode(BaseUri)));

            if (Branding.HasValue)
                visual.Add(new XAttribute("branding", Branding.Value.ToString().ToLowerInvariant()));

            if (AddImageQuery.HasValue)
                visual.Add(new XAttribute("addImageQuery", AddImageQuery.Value.ToString().ToLowerInvariant()));

            if (Bindings != null)
            {
                foreach (var binding in Bindings)
                    visual.Add(binding.GenerateXmlElement());
            }

            return visual;
        }

        protected string XmlEncode(string text)
        {
            return System.Security.SecurityElement.Escape(text);
        }
    }

    public class ToastBinding
    {
        public ToastBinding()
        {
            Texts = new List<ToastText>();
            Images = new List<ToastImage>();
        }

        public ToastNotificationTemplate ToastTemplate { get; set; }
        public List<ToastText> Texts { get; set; }
        public List<ToastImage> Images { get; set; }
        public string Fallback { get; set; }
        public string Language { get; set; }
        public string BaseUri { get; set; }
        public BrandingType? Branding { get; set; }
        public bool? AddImageQuery { get; set; }

        public XElement GenerateXmlElement()
        {
            var binding = new XElement("binding", new XAttribute("template", ToastTemplate.ToString()));

            if (!string.IsNullOrEmpty(Fallback))
                binding.Add(new XAttribute("fallback", XmlEncode(Fallback)));

            if (!string.IsNullOrEmpty(Language))
                binding.Add(new XAttribute("lang", XmlEncode(Language)));

            if (!string.IsNullOrEmpty(BaseUri))
                binding.Add(new XAttribute("baseUri", XmlEncode(BaseUri)));

            if (AddImageQuery.HasValue)
                binding.Add(new XAttribute("addImageQuery", AddImageQuery.Value.ToString().ToLowerInvariant()));

            int idOn = 1;
            if (Images != null)
            {
                foreach (var img in Images)
                    binding.Add(img.GenerateXmlElement(idOn));
            }

            idOn = 1;
            if (Texts != null)
            {
                foreach (var text in Texts)
                    binding.Add(text.GenerateXmlElement(idOn++));
            }

            return binding;
        }

        protected string XmlEncode(string text)
        {
            return System.Security.SecurityElement.Escape(text);
        }
    }

    public class ToastInput
    {
        public string ID { get; set; }
        public ToastActionInputType InputType { get; set; }
        /// <summary>
        /// Optional title
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Optional placeholder hint text
        /// </summary>
        public string PlaceHolderContent { get; set; }
        /// <summary>
        /// Optional default input text or selection id
        /// </summary>
        public string DefaultInput { get; set; }

        public List<ToastSelection> Selections { get; set; }

        public ToastInput(string id)
        {
            ID = id;
        }

        public XElement GenerateXmlElement()
        {
            var input = new XElement("input", new XAttribute("id", ID.ToString()));
            if (!string.IsNullOrEmpty(Title))
                input.Add(new XAttribute("title", XmlEncode(Title)));

            if (!string.IsNullOrEmpty(PlaceHolderContent))
                input.Add(new XAttribute("placeHolderContent", XmlEncode(PlaceHolderContent)));

            if (!string.IsNullOrEmpty(DefaultInput))
                input.Add(new XAttribute("defaultInput", XmlEncode(DefaultInput)));

            input.Add(new XAttribute("type", InputType.ToString()));

            if (Selections != null)
            {
                foreach (ToastSelection selection in Selections)
                {
                    input.Add(selection.GenerateXmlElement());
                }
            }

            return input;
        }

        protected string XmlEncode(string text)
        {
            return System.Security.SecurityElement.Escape(text);
        }
    }

    public class ToastSelection
    {
        string ID { get; set; }
        string Content { get; set; }

        public ToastSelection(string id)
        {
            ID = id;
        }

        public XElement GenerateXmlElement()
        {
            var selection = new XElement("selection", new XAttribute("id", ID.ToString()));
            if (!string.IsNullOrEmpty(Content))
                selection.Add(new XAttribute("content", XmlEncode(Content)));

            return selection;
        }

        protected string XmlEncode(string text)
        {
            return System.Security.SecurityElement.Escape(text);
        }
    }

    public class ToastAction
    {
        public string Content { get; set; }
        public string Arguments { get; set; }
        /// <summary>
        /// Optionally set the activation type. foreground | background | protocol | system. Default is foreground.
        /// </summary>
        public ToastActivationType? ActivationType { get; set; }
        /// <summary>
        /// Optional image that is displayed inside the action button along with the text content.
        /// </summary>
        public string ImageUri { get; set; }
        /// <summary>
        /// This is specifically used for the quick reply scenario.
        ///The value needs to be the id of the input element desired to be associated with.
        ///In mobile and desktop, this will put the button right next to the input box. 
        /// </summary>
        /// <returns></returns>
        public string HintInputId { get; set; }


        public XElement GenerateXmlElement()
        {
            var action = new XElement("action");
            if (!string.IsNullOrEmpty(Arguments))
                action.Add(new XAttribute("arguments", XmlEncode(Arguments)));

            if (ActivationType.HasValue)
                action.Add(new XAttribute("activationType", XmlEncode(ActivationType.Value.ToString().ToLowerInvariant())));

            if (!string.IsNullOrEmpty(ImageUri))
                action.Add(new XAttribute("imageUri", ImageUri));
            if (!string.IsNullOrEmpty(HintInputId))
                action.Add(new XAttribute("hint-inputId", HintInputId));

            action.Add(new XAttribute("content", Content));

            return action;
        }

        protected string XmlEncode(string text)
        {
            return System.Security.SecurityElement.Escape(text);
        }
    }

    public class ToastAudio
    {
        public ToastAudioSource Source { get; set; }

        /// <summary>
        /// Only for Windows Phone 8.1
        /// </summary>
        public string CustomAudioSource { get; set; }

        public bool Loop { get; set; }

        public XElement GenerateXmlElement()
        {
            var audio = new XElement("audio");

            if (Source == ToastAudioSource.Silent)
            {
                audio.Add(new XAttribute("silent", "true"));
            }
            else
            {
                if (Loop)
                    audio.Add(new XAttribute("loop", "true"));

                //Default sound is LoopingCall, so don't need to add it if that's the case
                if (Source != ToastAudioSource.LoopingCall)
                {
                    string audioSrc = null;
                    switch (Source)
                    {
                        case ToastAudioSource.IM:
                            audioSrc = "ms-winsoundevent:Notification.IM";
                            break;
                        case ToastAudioSource.Mail:
                            audioSrc = "ms-winsoundevent:Notification.Mail";
                            break;
                        case ToastAudioSource.Reminder:
                            audioSrc = "ms-winsoundevent:Notification.Reminder";
                            break;
                        case ToastAudioSource.SMS:
                            audioSrc = "ms-winsoundevent:Notification.SMS";
                            break;
                        case ToastAudioSource.LoopingAlarm:
                            audioSrc = "ms-winsoundevent:Notification.Looping.Alarm";
                            break;
                        case ToastAudioSource.LoopingAlarm2:
                            audioSrc = "ms-winsoundevent:Notification.Looping.Alarm2";
                            break;
                        case ToastAudioSource.LoopingCall:
                            audioSrc = "ms-winsoundevent:Notification.Looping.Call";
                            break;
                        case ToastAudioSource.LoopingCall2:
                            audioSrc = "ms-winsoundevent:Notification.Looping.Call2";
                            break;
                        case ToastAudioSource.Custom:
                            audioSrc = CustomAudioSource;
                            break;
                    }

                    audio.Add(new XAttribute("src", audioSrc));
                }
            }

            return audio;
        }
    }
    public class ToastActions
    {

        public List<ToastInput> Inputs { get; set; }
        public List<ToastAction> Actions { get; set; }

        public XElement GenerateXmlElement()
        {
            var actions = new XElement("actions");
            if (Actions != null)
            {

                foreach (var input in Inputs)
                    actions.Add(input.GenerateXmlElement());
            }
            if (Actions != null)
            {
                foreach (var action in Actions)
                    actions.Add(action.GenerateXmlElement());
            }
            return actions;
        }

        protected string XmlEncode(string text)
        {
            return System.Security.SecurityElement.Escape(text);
        }
    }
    public class Windows10ToastVisual : IToastVisual
    {
        public string Language { get; set; }
        public string BaseUri { get; set; }
        public bool? AddImageQuery { get; set; }

        public ToastBinding Binding { get; set; }

        public XElement GenerateXmlElement()
        {
            var visual = new XElement("visual");

            if (!string.IsNullOrEmpty(Language))
                visual.Add(new XAttribute("lang", XmlEncode(Language)));

            if (!string.IsNullOrEmpty(BaseUri))
                visual.Add(new XAttribute("baseUri", XmlEncode(BaseUri)));

            if (AddImageQuery.HasValue)
                visual.Add(new XAttribute("addImageQuery", AddImageQuery.Value.ToString().ToLowerInvariant()));

            if (Binding != null)
                visual.Add(Binding.GenerateXmlElement());

            return visual;
        }

        protected string XmlEncode(string text)
        {
            return System.Security.SecurityElement.Escape(text);
        }
    }
    public interface IToastVisual
    {
        string Language { get; set; }
        string BaseUri { get; set; }
        bool? AddImageQuery { get; set; }
        ToastBinding Binding { get; set; }
        XElement GenerateXmlElement();
    }

    public class ToastVisual : IToastVisual
    {
        public int? Version { get; set; }
        public string Language { get; set; }
        public string BaseUri { get; set; }
        public BrandingType? Branding { get; set; }
        public bool? AddImageQuery { get; set; }

        public ToastBinding Binding { get; set; }

        public XElement GenerateXmlElement()
        {
            var visual = new XElement("visual");

            if (Version.HasValue)
                visual.Add(new XAttribute("version", Version.Value.ToString()));

            if (!string.IsNullOrEmpty(Language))
                visual.Add(new XAttribute("lang", XmlEncode(Language)));

            if (!string.IsNullOrEmpty(BaseUri))
                visual.Add(new XAttribute("baseUri", XmlEncode(BaseUri)));

            if (Branding.HasValue)
                visual.Add(new XAttribute("branding", Branding.Value.ToString().ToLowerInvariant()));

            if (AddImageQuery.HasValue)
                visual.Add(new XAttribute("addImageQuery", AddImageQuery.Value.ToString().ToLowerInvariant()));

            if (Binding != null)
                visual.Add(Binding.GenerateXmlElement());

            return visual;
        }

        protected string XmlEncode(string text)
        {
            return System.Security.SecurityElement.Escape(text);
        }
    }
    public class WindowsDeleteToastNotification : WindowsToastNotification
    {
        public override WindowsNotificationType Type
        {
            get
            {
                return WindowsNotificationType.DeleteNotification;
            }
        }

    }
    public class Windows10ToastNotification : WindowsToastNotification
    {
        public ToastActions Actions { get; set; }

        public Windows10ToastNotification()
            : base()
        {
            Visual = new Windows10ToastVisual();
        }
        public override string PayloadToString()
        {
            var toast = new XElement("toast");

            if (!string.IsNullOrEmpty(this.Launch))
                toast.Add(new XAttribute("launch", XmlEncode(this.Launch)));

            if (Duration != ToastDuration.Short)
                toast.Add(new XAttribute("duration", Duration.ToString().ToLowerInvariant()));

            if (Audio != null)
                toast.Add(Audio.GenerateXmlElement());

            if (Visual != null)
                toast.Add(Visual.GenerateXmlElement());
            if (Actions != null)
            {
                toast.Add(Actions.GenerateXmlElement());
            }

            return toast.ToString();
        }
    }

    public class WindowsToastNotification : WindowsNotification
    {
        public WindowsToastNotification()
            : base()
        {
            Visual = new ToastVisual();
        }

        public override WindowsNotificationType Type
        {
            get { return WindowsNotificationType.Toast; }
        }

        public string Launch { get; set; }
        public ToastDuration Duration { get; set; }

        public ToastAudio Audio { get; set; }
        public IToastVisual Visual { get; set; }
        public bool SuppressPopup { get; set; }
        public override string PayloadToString()
        {
            var toast = new XElement("toast");

            if (!string.IsNullOrEmpty(this.Launch))
                toast.Add(new XAttribute("launch", XmlEncode(this.Launch)));

            if (Duration != ToastDuration.Short)
                toast.Add(new XAttribute("duration", Duration.ToString().ToLowerInvariant()));

            if (Audio != null)
                toast.Add(Audio.GenerateXmlElement());

            if (Visual != null)
                toast.Add(Visual.GenerateXmlElement());

            return toast.ToString();
        }
    }

    public class WindowsBadgeGlyphNotification : WindowsNotification
    {
        public override WindowsNotificationType Type
        {
            get { return WindowsNotificationType.Badge; }
        }

        public WindowsNotificationCachePolicyType? CachePolicy { get; set; }

        public BadgeGlyphValue Glyph { get; set; }
        public int? Version { get; set; }

        public override string PayloadToString()
        {
            var badge = new XElement("badge");

            if (Version.HasValue)
                badge.Add(new XAttribute("version", this.Version.Value.ToString()));

            badge.Add(new XAttribute("value", Glyph.ToString().ToLowerInvariant()));

            return badge.ToString();
        }
    }

    public class WindowsBadgeNumericNotification : WindowsNotification
    {
        public override WindowsNotificationType Type
        {
            get { return WindowsNotificationType.Badge; }
        }

        public WindowsNotificationCachePolicyType? CachePolicy { get; set; }

        public int BadgeNumber { get; set; }
        public int? Version { get; set; }

        public override string PayloadToString()
        {
            var badge = new XElement("badge");

            if (Version.HasValue)
                badge.Add(new XAttribute("version", this.Version.Value));

            badge.Add(new XAttribute("value", BadgeNumber.ToString().ToLowerInvariant()));

            return badge.ToString();
        }
    }

    public class TileImage
    {
        public string Source { get; set; }
        public string Alt { get; set; }
        public bool? AddImageQuery { get; set; }

        public XElement GenerateXmlElement(int id)
        {
            var img = new XElement("image", new XAttribute("id", id.ToString()),
                new XAttribute("src", Source));

            if (!string.IsNullOrEmpty(Alt))
                img.Add(new XAttribute("alt", XmlEncode(Alt)));

            if (AddImageQuery.HasValue)
                img.Add(new XAttribute("addImageQuery", AddImageQuery.Value.ToString().ToLowerInvariant()));

            return img;
        }

        protected string XmlEncode(string text)
        {
            return System.Security.SecurityElement.Escape(text);
        }
    }

    public class ToastImage
    {
        public string Source { get; set; }
        public string Alt { get; set; }
        public bool? AddImageQuery { get; set; }
        /// <summary>
        /// Windows 10 only property for setting image placement
        /// </summary>
        public ToastImagePlacement? Placement { get; set; }
        public ToastImageHintCrop? HintCrop { get; set; }

        public XElement GenerateXmlElement(int id)
        {
            var img = new XElement("image", new XAttribute("id", id.ToString()),
                new XAttribute("src", Source));

            if (!string.IsNullOrEmpty(Alt))
                img.Add(new XAttribute("alt", XmlEncode(Alt)));

            if (AddImageQuery.HasValue)
                img.Add(new XAttribute("addImageQuery", AddImageQuery.Value.ToString().ToLowerInvariant()));

            if (Placement.HasValue)
            {
                img.Add(new XAttribute("placement", Placement.Value.ToString()));
            }
            if (HintCrop.HasValue)
            {
                img.Add(new XAttribute("hint-crop", HintCrop.Value.ToString()));
            }

            return img;
        }

        protected string XmlEncode(string text)
        {
            return System.Security.SecurityElement.Escape(text);
        }
    }

    public class TileText
    {
        public string Text { get; set; }
        public string Language { get; set; }

        public XElement GenerateXmlElement(int id)
        {
            var text = new XElement("text", new XAttribute("id", id.ToString()));

            if (!string.IsNullOrEmpty(Language))
                text.Add(new XAttribute("lang", XmlEncode(Language)));

            if (!string.IsNullOrEmpty(Text))
                text.Add(XmlEncode(Text));

            return text;
        }

        protected string XmlEncode(string text)
        {
            return System.Security.SecurityElement.Escape(text);
        }
    }

    public class ToastText
    {
        public string Text { get; set; }
        public string Language { get; set; }

        public XElement GenerateXmlElement(int id)
        {
            var text = new XElement("text", new XAttribute("id", id.ToString()));

            if (!string.IsNullOrEmpty(Language))
                text.Add(new XAttribute("lang", XmlEncode(Language)));

            if (!string.IsNullOrEmpty(Text))
                text.Add(Text);//XmlEncode(Text));

            return text;
        }

        protected string XmlEncode(string text)
        {
            return System.Security.SecurityElement.Escape(text);
        }
    }

    public class WindowsRawNotification : WindowsNotification
    {
        public override WindowsNotificationType Type
        {
            get { return WindowsNotificationType.Raw; }
        }

        public string RawXml { get; set; }

        public override string PayloadToString()
        {
            return RawXml;
        }
    }

    public enum ToastDuration
    {
        Short = 0,
        Long = 1
    }

    public enum ToastAudioSource
    {
        /// <summary>
        /// The default toast audio sound.
        /// </summary>
        Default = 0,
        /// <summary>
        /// Audio that corresponds to new mail arriving.
        /// </summary>
        Mail,
        /// <summary>
        /// Audio that corresponds to a new SMS message arriving.
        /// </summary>
        SMS,
        /// <summary>
        /// Audio that corresponds to a new IM arriving.
        /// </summary>
        IM,
        /// <summary>
        /// Audio that corresponds to a reminder.
        /// </summary>
        Reminder,
        /// <summary>
        /// The default looping sound.  Audio that corresponds to a call.
        /// Only valid for toasts that are have the duration set to "Long".
        /// </summary>
        LoopingCall,
        /// <summary>
        /// Audio that corresponds to a call.
        /// Only valid for toasts that are have the duration set to "Long".
        /// </summary>
        LoopingCall2,
        /// <summary>
        /// Audio that corresponds to an alarm.
        /// Only valid for toasts that are have the duration set to "Long".
        /// </summary>
        LoopingAlarm,
        /// <summary>
        /// Audio that corresponds to an alarm.
        /// Only valid for toasts that are have the duration set to "Long".
        /// </summary>
        LoopingAlarm2,
        /// <summary>
        /// No audio should be played when the toast is displayed.
        /// </summary>
        Silent,
        /// <summary>
        /// Provide a URI for a custom notification sound. Only for Windows Phone 8.1
        /// </summary>
        Custom
    }

    public enum BadgeGlyphValue
    {
        /// <summary>
        /// No glyph.  If there is a numeric badge, or a glyph currently on the badge,
        /// it will be removed.
        /// </summary>
        None = 0,
        /// <summary>
        /// A glyph representing application activity.
        /// </summary>
        Activity,
        /// <summary>
        /// A glyph representing an alert.
        /// </summary>
        Alert,
        /// <summary>
        /// A glyph representing availability status.
        /// </summary>
        Available,
        /// <summary>
        /// A glyph representing away status
        /// </summary>
        Away,
        /// <summary>
        /// A glyph representing busy status.
        /// </summary>
        Busy,
        /// <summary>
        /// A glyph representing that a new message is available.
        /// </summary>
        NewMessage,
        /// <summary>
        /// A glyph representing that media is paused.
        /// </summary>
        Paused,
        /// <summary>
        /// A glyph representing that media is playing.
        /// </summary>
        Playing,
        /// <summary>
        /// A glyph representing unavailable status.
        /// </summary>
        Unavailable,
        /// <summary>
        /// A glyph representing an error.
        /// </summary>
        Error,
        /// <summary>
        /// A glyph representing attention status.
        /// </summary>
        Attention
    }

    public enum WindowsNotificationCachePolicyType
    {
        Cache,
        NoCache
    }

    public enum WindowsNotificationType
    {
        Badge,
        Tile,
        Toast,
        Raw,
        DeleteNotification
    }

    public enum ToastNotificationTemplate
    {
        ToastText01,
        ToastText02,
        ToastText03,
        ToastText04,
        ToastImageAndText01,
        ToastImageAndText02,
        ToastImageAndText03,
        ToastImageAndText04,
        /// <summary>
        /// Use for windows 10
        /// </summary>
        ToastGeneric
    }

    public enum BrandingType
    {
        None = 0,
        Logo = 1,
        Name = 2
    }

    public enum ToastActionInputType
    {
        text,
        selection
    }

    public enum ToastActivationType
    {
        /// <summary>
        /// Activating the app in the foreground, with an action-specific argument that can be used to navigate to a specific page/context
        /// </summary>
        foreground,
        /// <summary>
        /// Activating the app’s background task without affecting the user
        /// </summary>
        background,
        /// <summary>
        /// Activating another app via protocol launch
        /// </summary>
        protocol,
        /// <summary>
        /// Specify a system action to perform. The current available system actions are "snooze" and "dismiss" scheduled alarm/reminder
        /// </summary>
        system
    }

    public enum ToastImagePlacement
    {
        inline,
        hero,
        appLogoOverride
    }

    public enum ToastImageHintCrop
    {
        none,
        circle
    }
}
