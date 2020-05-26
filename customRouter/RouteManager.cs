using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Components;

namespace customRouter
{
    public class RouteManager
    {
        public Route[] Routes { get; private set; }

        public void Initialize()
        {
            var pageComponentTypes = Assembly
                .GetExecutingAssembly()
                .ExportedTypes
                .Where(t => t.IsSubclassOf(typeof(ComponentBase)) 
                        && t.Namespace.Contains(".Pages"));

            var routesList = new List<Route>();
            foreach (var type in pageComponentTypes)
            {
                var newRoute = new Route
                {
                    UriSegments = type.FullName.Substring(type.FullName.IndexOf("Pages") + 6).Split('.'),
                    Handler = type
                };

                routesList.Add(newRoute);
            }

            Routes = routesList.ToArray();
        }

        public MatchResult Match(string[] segments)
        {
            if (segments.Length == 0)
            {
                var indexRoute = Routes.SingleOrDefault(x =>
                    x.Handler.FullName.EndsWith("index", StringComparison.OrdinalIgnoreCase));
                return MatchResult.Match(indexRoute);
            }

            foreach(var route in Routes)
            {
                var matchResult = route.Match(segments);

                if (matchResult.IsMatch)
                {
                    return matchResult;
                }
            }

            return MatchResult.NoMatch();
        }
    }
}