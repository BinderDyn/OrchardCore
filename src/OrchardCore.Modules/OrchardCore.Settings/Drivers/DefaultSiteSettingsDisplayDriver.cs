using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Settings.ViewModels;

namespace OrchardCore.Settings.Drivers
{
    public class DefaultSiteSettingsDisplayDriver : DisplayDriver<ISite>
    {
        public const string GroupId = "general";

        private readonly IStringLocalizer S;

        public DefaultSiteSettingsDisplayDriver(IStringLocalizer<DefaultSiteSettingsDisplayDriver> stringLocalizer)
        {
            S = stringLocalizer;
        }

        public override IDisplayResult Edit(ISite site)
        {
            return Initialize<SiteSettingsViewModel>("Settings_Edit", model =>
            {
                model.SiteName = site.SiteName;
                model.PageTitleFormat = site.PageTitleFormat;
                model.BaseUrl = site.BaseUrl;
                model.TimeZone = site.TimeZoneId;
                model.PageSize = site.PageSize;
                model.UseCdn = site.UseCdn;
                model.CdnBaseUrl = site.CdnBaseUrl;
                model.ResourceDebugMode = site.ResourceDebugMode;
                model.AppendVersion = site.AppendVersion;
                model.CacheMode = site.CacheMode;
            }).Location("Content:1").OnGroup(GroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(ISite site, UpdateEditorContext context)
        {
            if (context.GroupId == GroupId)
            {
                var model = new SiteSettingsViewModel();

                if (await context.Updater.TryUpdateModelAsync(model, Prefix))
                {
                    site.SiteName = model.SiteName;
                    site.PageTitleFormat = model.PageTitleFormat;
                    site.BaseUrl = model.BaseUrl;
                    site.TimeZoneId = model.TimeZone;
                    site.PageSize = model.PageSize.Value;
                    site.UseCdn = model.UseCdn;
                    site.CdnBaseUrl = model.CdnBaseUrl;
                    site.ResourceDebugMode = model.ResourceDebugMode;
                    site.AppendVersion = model.AppendVersion;
                    site.CacheMode = model.CacheMode;

                    if (model.PageSize.Value < 1)
                    {
                        context.Updater.ModelState.AddModelError(Prefix, nameof(model.PageSize), S["The page size must be greater than zero."]);
                    }

                    if (site.MaxPageSize > 0 && model.PageSize.Value > site.MaxPageSize)
                    {
                        context.Updater.ModelState.AddModelError(Prefix, nameof(model.PageSize), S["The page size must be less than or equal to {0}.", site.MaxPageSize]);
                    }
                }

                if (!String.IsNullOrEmpty(site.BaseUrl) && !Uri.TryCreate(site.BaseUrl, UriKind.Absolute, out _))
                {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(model.BaseUrl), S["The Base url must be a fully qualified URL."]);
                }
            }

            return Edit(site);
        }
    }
}
