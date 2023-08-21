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
            if (game.Name.IsNullOrEmpty())
            {
                return null;
            }

            return string.Format(UrlFormat, Uri.EscapeUriString(game.Name));
        }

        protected abstract string UrlFormat { get; }
    }
}
