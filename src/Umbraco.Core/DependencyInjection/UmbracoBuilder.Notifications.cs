using Umbraco.Cms.Core.Compose;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.DependencyInjection
{
    public static partial class UmbracoBuilderExtensions
    {
        /// <summary>
        /// Adds handlers for user notifications
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IUmbracoBuilder AddUserNotifications(this IUmbracoBuilder builder)
        {
            builder.Services.AddUnique<UserNotificationsHandler.Notifier>();
            builder
                .AddNotificationHandler<ContentSavedNotification, UserNotificationsHandler>()
                .AddNotificationHandler<ContentSortedNotification, UserNotificationsHandler>()
                .AddNotificationHandler<ContentPublishedNotification, UserNotificationsHandler>()
                .AddNotificationHandler<ContentMovedNotification, UserNotificationsHandler>()
                .AddNotificationHandler<ContentMovedToRecycleBinNotification, UserNotificationsHandler>()
                .AddNotificationHandler<ContentCopiedNotification, UserNotificationsHandler>()
                .AddNotificationHandler<ContentRolledBackNotification, UserNotificationsHandler>()
                .AddNotificationHandler<ContentSentToPublishNotification, UserNotificationsHandler>()
                .AddNotificationHandler<ContentUnpublishedNotification, UserNotificationsHandler>()
                .AddNotificationHandler<AssignedUserGroupPermissionsNotification, UserNotificationsHandler>()
                .AddNotificationHandler<PublicAccessEntrySavedNotification, UserNotificationsHandler>();
            return builder;
        }

        public static IUmbracoBuilder AddCoreNotifications(this IUmbracoBuilder builder)
        {
            builder
                .AddUserNotifications();
            return builder;
        }
    }
}
