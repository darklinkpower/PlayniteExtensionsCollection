using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseCommon
{
    public interface IRepository<T>
    {
        IEnumerable<T> GetAll();
        T GetById(BsonValue id);
        void Insert(T entity);
        bool Update(T entity);
        bool Delete(BsonValue id);
    }
}