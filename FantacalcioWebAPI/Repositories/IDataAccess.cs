using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FantacalcioWebAPI.Repositories
{
    public interface IDataAccess<TEntity, U> where TEntity : class
    {
        IEnumerable<TEntity> GetBooks();
        TEntity Get(U id);
        TEntity GetAll();
        int Add(TEntity b);
        int Update(U id, TEntity b);
        int Delete(U id);
    }
}
