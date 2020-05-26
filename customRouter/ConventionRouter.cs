using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace customRouter
{
    public class ConventionRouter : IComponent, IHandleAfterRender, IDisposable
    {
        private RenderHandle _renderHandle;
        private bool _navigationInterceptionEnabled;
        private string _location;

        [Inject] private NavigationManager NavigationManager { get; set; }
        [Inject] private INavigationInterception NavigationInterception { get; set; }
        [Inject] private RouteManager RouteManager { get; set; }
            
        [Parameter] public RenderFragment NotFound { get; set; }
        [Parameter] public RenderFragment<RouteData> Found { get; set; }

        public void Attach(RenderHandle renderHandle)
        {
            _renderHandle = renderHandle;
            _location = NavigationManager.Uri;
            NavigationManager.LocationChanged += HandleLocationChanged;
        }

        public void Dispose()
        {
            NavigationManager.LocationChanged -= HandleLocationChanged;
        }

        public Task OnAfterRenderAsync()
        {
            if (!_navigationInterceptionEnabled)
            {
                _navigationInterceptionEnabled = true;
                return NavigationInterception.EnableNavigationInterceptionAsync();
            }

            return Task.CompletedTask;
        }

        public Task SetParametersAsync(ParameterView parameters)
        {
            parameters.SetParameterProperties(this);

            if (Found == null)
            {
                throw new InvalidOperationException($"The {nameof(ConventionRouter)} component requires a value for the parameter {nameof(Found)}");
            }

            if (NotFound == null)
            {
                throw new InvalidOperationException($"The {nameof(ConventionRouter)} component requires a value for the parameter {nameof(NotFound)}");
            }

            RouteManager.Initialize();
            Refresh();

            return Task.CompletedTask;
        }

        private void Refresh()
        {
            var relativeUri = NavigationManager.ToBaseRelativePath(_location);
            var parameters = ParseQueryString(relativeUri);

            if (relativeUri.IndexOf('?') > -1)
            {
                relativeUri = relativeUri.Substring(0, relativeUri.IndexOf('?'));
            }
            
            var segments = relativeUri.Trim().Split('/', StringSplitOptions.RemoveEmptyEntries);
            var matchResult = RouteManager.Match(segments);

            if (matchResult.IsMatch)
            {
                var routeData = new RouteData(matchResult.MatchedRoute.Handler, parameters);
                _renderHandle.Render(Found(routeData));
            }
            else
            {
                _renderHandle.Render(NotFound);
            }
        }

        private void HandleLocationChanged(object sender, LocationChangedEventArgs e)
        {
            _location = e.Location;
            Refresh();
        }

        private Dictionary<string, object> ParseQueryString(string relativeUri)
        {
            var querystring = new Dictionary<string, object>();

            foreach(var kvp in relativeUri.Substring(relativeUri.IndexOf("?") + 1).Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (kvp != "" && kvp.Contains("="))
                {
                    var pair = kvp.Split("=");
                    querystring.Add(pair[0], pair[1]);
                }
            }
            
            return querystring;
        }
    }
}