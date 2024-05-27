using DatabaseCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.CharacterAggregate;
using VndbApiDomain.ProducerAggregate;
using VndbApiDomain.ReleaseAggregate;
using VndbApiDomain.SharedKernel.Entities;
using VndbApiDomain.StaffAggregate;
using VndbApiDomain.TagAggregate;
using VndbApiDomain.TraitAggregate;
using VndbApiDomain.VisualNovelAggregate;
using LiteDB;


namespace VNDBNexus.Database
{
    public class VisualNovelReleasesSearchResults : IAggregateRoot
    {
        public string Id { get; set; }

        public List<Release> Members { get; set; }

        public VisualNovelReleasesSearchResults(VisualNovel visualNovel, List<Release> members)
        {
            Id = visualNovel.Id;
            Members = members;
        }

        private VisualNovelReleasesSearchResults()
        {

        }
    }

    public class VisualNovelCharactersSearchResults : IAggregateRoot
    {
        public string Id { get; set; }

        public List<Character> Members { get; set; }

        public VisualNovelCharactersSearchResults(VisualNovel visualNovel, List<Character> members)
        {
            Id = visualNovel.Id;
            Members = members;
        }

        private VisualNovelCharactersSearchResults()
        {

        }
    }

    public class VndbDatabase
    {
        public LiteDbRepository<VisualNovelCharactersSearchResults> Characters { get; }
        //public LiteDbRepository<VisualNovelSearchResults<Producer>> Producers { get; }
        public LiteDbRepository<VisualNovelReleasesSearchResults> Releases { get; }
        //public LiteDbRepository<VisualNovelSearchResults<Staff>> Staff { get; }
        public LiteDbRepository<Tag> Tags { get; }
        public LiteDbRepository<Trait> Traits { get; }
        public LiteDbRepository<VisualNovel> VisualNovels { get; }

        public VndbDatabase(string rootDatabasePath)
        {
            Characters = new LiteDbRepository<VisualNovelCharactersSearchResults>(Path.Combine(rootDatabasePath, "VisualNovelsCharacters.db"));
            //Producers = new LiteDbRepository<VisualNovelSearchResults<Producer>>(Path.Combine(rootDatabasePath, "Producers.db"));
            Releases = new LiteDbRepository<VisualNovelReleasesSearchResults>(Path.Combine(rootDatabasePath, "VisualNovelsReleases.db"));
            //Staff = new LiteDbRepository<VisualNovelSearchResults<Staff>>(Path.Combine(rootDatabasePath, "Staff.db"));
            Tags = new LiteDbRepository<Tag>(Path.Combine(rootDatabasePath, "Tags.db"));
            Traits = new LiteDbRepository<Trait>(Path.Combine(rootDatabasePath, "Traits.db"));
            VisualNovels = new LiteDbRepository<VisualNovel>(Path.Combine(rootDatabasePath, "VisualNovels.db"));
        }
    }
}