using Playnite.SDK.Models;
using SearchCollection.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchCollection.BaseClasses
{
    public abstract class BaseSearchDefinition: ISearchDefinition
    {
        public abstract string Name { get; }
        public abstract string Icon { get; }

        public virtual string GetSearchUrl(Game game)
        {
            return GetSearchUrl(game.Name);
        }

        public virtual string GetSearchUrl(string searchTerm)
        {
            if (searchTerm.IsNullOrEmpty())
            {
                return null;
            }

            return string.Format(UrlFormat, Uri.EscapeUriString(searchTerm));
        }

        protected abstract string UrlFormat { get; }
    }
}
