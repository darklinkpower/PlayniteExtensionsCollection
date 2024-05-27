using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNDBNexus.Database
{
    public class GroupResult<TGroup, TEntity>
    {
        public TGroup Grouper { get; set; }
        public IEnumerable<TEntity> Entities { get; set; }
        public int TotalEntities => Entities.Count();

        public GroupResult(TGroup grouper, IEnumerable<TEntity> entities)
        {
            Grouper = grouper;
            Entities = entities;
        }
    }

    public class GroupedDictionary<TGroup, TEntity>
    {
        public List<GroupResult<TGroup, TEntity>> GroupedResults { get; } = new List<GroupResult<TGroup, TEntity>>();

        public GroupedDictionary()
        {

        }
        
        public GroupedDictionary(IEnumerable<TEntity> entities, Func<TEntity, TGroup> groupKeySelector, Func<IEnumerable<TEntity>, IEnumerable<TEntity>> sortFunc = null)
        {
            GroupedResults = entities
                .GroupBy(groupKeySelector)
                .Select(g => new GroupResult<TGroup, TEntity>(g.Key, sortFunc != null ? sortFunc(g.AsEnumerable()) : g.AsEnumerable()))
                .ToList();
        }

        public GroupedDictionary(IEnumerable<TEntity> entities, Func<TEntity, IEnumerable<TGroup>> groupKeySelector, Func<IEnumerable<TEntity>, IEnumerable<TEntity>> sortFunc = null)
        {
            GroupedResults = entities
                .SelectMany(entity => groupKeySelector(entity).Select(grouper => new { entity, grouper }))
                .GroupBy(x => x.grouper, x => x.entity)
                .Select(g => new GroupResult<TGroup, TEntity>(g.Key, sortFunc != null ? sortFunc(g.AsEnumerable()) : g.AsEnumerable()))
                .ToList();
        }

        public IEnumerable<TEntity> this[TGroup key]
        {
            get
            {
                var group = GroupedResults.FirstOrDefault(gr => EqualityComparer<TGroup>.Default.Equals(gr.Grouper, key));
                return group != null ? group.Entities : Enumerable.Empty<TEntity>();
            }
        }

        public List<GroupResult<TGroup, TEntity>> AsList() => GroupedResults;

        public IEnumerable<TGroup> Keys => GroupedResults.Select(gr => gr.Grouper);

        public IEnumerable<IEnumerable<TEntity>> Values => GroupedResults.Select(gr => gr.Entities);
    }
}
