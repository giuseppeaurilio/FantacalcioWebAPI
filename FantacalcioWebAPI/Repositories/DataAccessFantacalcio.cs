using FantacalcioWebAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FantacalcioWebAPI.Repositories
{
    public interface IDataAccessFantacalcio<TEntity, U> : IDataAccess<TEntity, U> where TEntity : class
    {
    }

    public class DataAccessFantacalcio : IDataAccessFantacalcio<Fantacalcio, int>
    {
        public int Add(Fantacalcio b)
        {
            throw new NotImplementedException();
        }

        public int Delete(int id)
        {
            throw new NotImplementedException();
        }

        public Fantacalcio Get(int id)
        {
            throw new NotImplementedException();
        }

        public Fantacalcio GetAll()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Fantacalcio> GetBooks()
        {
            throw new NotImplementedException();
        }

        public int Update(int id, Fantacalcio b)
        {
            throw new NotImplementedException();
        }
    }
}
