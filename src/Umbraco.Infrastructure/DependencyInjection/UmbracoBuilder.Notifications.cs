using System;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Compose;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.DependencyInjection
{
    public static partial class UmbracoBuilderExtensions
    {
        public static IUmbracoBuilder AddContentRelationNotifications(this IUmbracoBuilder builder)
        {
            builder
                .AddNotificationHandler<ContentCopiedNotification, RelateOnCopyNotificationHandler>()
                .AddNotificationHandler<ContentMovedNotification, RelateOnTrashNotificationHandler>()
                .AddNotificationHandler<ContentMovedToRecycleBinNotification, RelateOnTrashNotificationHandler>()
                .AddNotificationHandler<MediaMovedNotification, RelateOnTrashNotificationHandler>()
                .AddNotificationHandler<MediaMovedToRecycleBinNotification, RelateOnTrashNotificationHandler>();
            return builder;
        }

        public static IUmbracoBuilder AddPropertyEditorNotifications(this IUmbracoBuilder builder)
        {
            builder
                .AddNotificationHandler<ContentSavingNotification, BlockEditorPropertyHandler>()
                .AddNotificationHandler<ContentCopyingNotification, BlockEditorPropertyHandler>()
                .AddNotificationHandler<ContentSavingNotification, NestedContentPropertyHandler>()
                .AddNotificationHandler<ContentCopyingNotification, NestedContentPropertyHandler>()
                .AddNotificationHandler<ContentCopiedNotification, FileUploadPropertyEditor>()
                .AddNotificationHandler<ContentDeletedNotification, FileUploadPropertyEditor>()
                .AddNotificationHandler<MediaDeletedNotification, FileUploadPropertyEditor>()
                .AddNotificationHandler<MediaSavingNotification, FileUploadPropertyEditor>()
                .AddNotificationHandler<MemberDeletedNotification, FileUploadPropertyEditor>()
                .AddNotificationHandler<ContentCopiedNotification, ImageCropperPropertyEditor>()
                .AddNotificationHandler<ContentDeletedNotification, ImageCropperPropertyEditor>()
                .AddNotificationHandler<MediaDeletedNotification, ImageCropperPropertyEditor>()
                .AddNotificationHandler<MediaSavingNotification, ImageCropperPropertyEditor>()
                .AddNotificationHandler<MemberDeletedNotification, ImageCropperPropertyEditor>();

            return builder;
        }

        public static IUmbracoBuilder AddRedirectTrackingNotifications(this IUmbracoBuilder builder)
        {
            builder
                .AddNotificationHandler<ContentPublishingNotification, RedirectTrackingHandler>()
                .AddNotificationHandler<ContentPublishedNotification, RedirectTrackingHandler>()
                .AddNotificationHandler<ContentMovingNotification, RedirectTrackingHandler>()
                .AddNotificationHandler<ContentMovedNotification, RedirectTrackingHandler>();
            return builder;
        }

        public static IUmbracoBuilder AddDistributedCacheNotifications(this IUmbracoBuilder builder)
        {
            builder
                .AddNotificationHandler<DictionaryItemDeletedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<DictionaryItemSavedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<LanguageSavedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<LanguageDeletedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<MemberSavedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<MemberDeletedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<PublicAccessEntrySavedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<PublicAccessEntryDeletedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<UserSavedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<UserDeletedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<UserGroupWithUsersSavedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<UserGroupDeletedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<MemberGroupDeletedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<MemberGroupSavedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<DataTypeDeletedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<DataTypeSavedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<TemplateDeletedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<TemplateSavedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<RelationTypeDeletedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<RelationTypeSavedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<DomainDeletedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<DomainSavedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<MacroSavedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<MacroDeletedNotification, DistributedCacheBinder>()
                .AddNotificationHandler<MediaTreeChangeNotification, DistributedCacheBinder>()
                .AddNotificationHandler<ContentTreeChangeNotification, DistributedCacheBinder>();

            return builder;
        }

        public static IUmbracoBuilder AddAuditNotifications(this IUmbracoBuilder builder)
        {
            builder
                .AddNotificationHandler<MemberSavedNotification, AuditNotificationsHandler>()
                .AddNotificationHandler<MemberDeletedNotification, AuditNotificationsHandler>()
                .AddNotificationHandler<AssignedMemberRolesNotification, AuditNotificationsHandler>()
                .AddNotificationHandler<RemovedMemberRolesNotification, AuditNotificationsHandler>()
                .AddNotificationHandler<ExportedMemberNotification, AuditNotificationsHandler>()
                .AddNotificationHandler<UserSavedNotification, AuditNotificationsHandler>()
                .AddNotificationHandler<UserDeletedNotification, AuditNotificationsHandler>()
                .AddNotificationHandler<UserGroupWithUsersSavedNotification, AuditNotificationsHandler>()
                .AddNotificationHandler<AssignedUserGroupPermissionsNotification, AuditNotificationsHandler>();
            return builder;
        }

        public static IUmbracoBuilder AddInfrastructureNotifications(this IUmbracoBuilder builder)
        {
            builder
                .AddContentRelationNotifications()
                .AddPropertyEditorNotifications()
                .AddRedirectTrackingNotifications()
                .AddDistributedCacheNotifications()
                .AddAuditNotifications();
            return builder;
        }

    }
}
