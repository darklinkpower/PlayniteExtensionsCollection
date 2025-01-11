using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericEntityJsonRepository
{
    public interface IEntityRepository<TEntity, TId>
    {
        void PersistData(IEnumerable<TEntity> entities);
        void PersistData(TEntity entity);
        List<TEntity> LoadPersistedData();
        bool ClearPersistedData();
        TEntity GetById(TId id);
        bool RemoveById(TId id);
    }
}
