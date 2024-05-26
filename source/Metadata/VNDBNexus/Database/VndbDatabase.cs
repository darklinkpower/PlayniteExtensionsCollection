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
using VndbApiDomain.StaffAggregate;
using VndbApiDomain.TagAggregate;
using VndbApiDomain.TraitAggregate;
using VndbApiDomain.VisualNovelAggregate;

namespace VNDBNexus.Database
{
    public class VndbDatabase
    {
        public LiteDbRepository<Character> Characters { get; }
        public LiteDbRepository<Producer> Producers { get; }
        public LiteDbRepository<Release> Releases { get; }
        public LiteDbRepository<Staff> Staff { get; }
        public LiteDbRepository<Tag> Tags { get; }
        public LiteDbRepository<Trait> Traits { get; }
        public LiteDbRepository<VisualNovel> VisualNovels { get; }
        public LiteDbRepository<VisualNovelRelations> VisualNovelRelations { get; }

        public VndbDatabase(string rootDatabasePath)
        {
            Characters = new LiteDbRepository<Character>(Path.Combine(rootDatabasePath, "Characters.db"));
            Producers = new LiteDbRepository<Producer>(Path.Combine(rootDatabasePath, "Producers.db"));
            Releases = new LiteDbRepository<Release>(Path.Combine(rootDatabasePath, "Releases.db"));
            Staff = new LiteDbRepository<Staff>(Path.Combine(rootDatabasePath, "Staff.db"));
            Tags = new LiteDbRepository<Tag>(Path.Combine(rootDatabasePath, "Tags.db"));
            Traits = new LiteDbRepository<Trait>(Path.Combine(rootDatabasePath, "Traits.db"));
            VisualNovels = new LiteDbRepository<VisualNovel>(Path.Combine(rootDatabasePath, "VisualNovels.db"));
            VisualNovelRelations = new LiteDbRepository<VisualNovelRelations>(Path.Combine(rootDatabasePath, "VisualNovelRelations.db"));
        }
    }
}