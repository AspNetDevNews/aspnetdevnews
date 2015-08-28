using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Helpers
{
    public static class UrlFormatter
    {
        private class DocumentTypeLayout
        {
            public string Sezione { get; set; }
            public string Heading { get; set; }
            public string Organization { get; set; }
            public string Repository { get; set; }
            public string UrlBase { get; set; }
            public string Marker { get; set; }
        }

        private static List<DocumentTypeLayout> layouts { get; set; }

        private static void InitLayouts() {
            if (layouts == null) {
                layouts = new List<DocumentTypeLayout>();
                layouts.Add(new DocumentTypeLayout
                {
                    Sezione = "aspnet",
                    Heading = "aspnet/",
                    Organization = "aspnet",
                    Repository = "docs",
                    UrlBase = "http://docs.asp.net/en/latest/",
                    Marker = "aspnet/"
                });

                layouts.Add(new DocumentTypeLayout
                {
                    Sezione = "mvc",
                    Heading = "mvc/",
                    Organization = "aspnet",
                    Repository = "docs",
                    UrlBase = "http://docs.asp.net/projects/mvc/en/latest/",
                    Marker = "mvc/"
                });

                layouts.Add(new DocumentTypeLayout
                {
                    Sezione = "entityframework",
                    Heading = "docs/",
                    Organization = "aspnet",
                    Repository = "entityframework.docs",
                    UrlBase = "http://ef.readthedocs.org/en/latest/",
                    Marker = string.Empty
                });

                layouts.Add(new DocumentTypeLayout
                {
                    Sezione = "netcore",
                    Heading = "docs/",
                    Organization = "dotnet",
                    Repository = "core-docs",
                    UrlBase = "http://dotnet.readthedocs.org/en/latest/",
                    Marker = string.Empty
                });

            }
        }

        private static string ComposeLayout(DocumentTypeLayout layout, string filename)
        {
            string url = filename.Substring(layout.Heading.Length);
            url = url.Substring(0, url.Length - 4);
            url = layout.UrlBase + url;
            url += ".html";

            return url;
        }

        private static DocumentTypeLayout SelectLayout(string organization, string repository, string filename) {
            var availableLayouts = layouts.Where(lay => lay.Organization == organization.ToLower() && lay.Repository == repository.ToLower());
            if (availableLayouts.Count() == 1)
            {
                var selectedLayout = availableLayouts.First();
                return selectedLayout;
            }
            else if (availableLayouts.Count() > 1)
            {
                foreach (var layout in availableLayouts)
                {
                    if (filename.ToLower().StartsWith(layout.Marker, StringComparison.Ordinal))
                        return layout;
                }
            }
            return null;
        }

        public static string GetWorkingUrl (string organization, string repository, string filename) {
            InitLayouts();
            var layout = SelectLayout(organization, repository, filename);
            return ComposeLayout(layout, filename);
        }

    }
}
