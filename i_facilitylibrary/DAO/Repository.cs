using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAO
{
    public class Repository<T> where T : class
    {

        // Get List of Data based on tbl
        public List<T> GetList(string qry, IDbConnection con)
        {
            IList<T> Tlista;
            using (var _Conn = con)
            {

                Tlista = SqlMapper.Query<T>(_Conn, qry, null, commandType: CommandType.Text,commandTimeout:60).ToList();
                _Conn.Close();
      
            }
            // IList<T> Tlista = mys.Query<T>(con, qry, null, commandType: CommandType.Text).ToList();
            return Tlista.ToList();
        }

        // Get Single Row 
        public T GetFirstOrDefault(string qry, IDbConnection con)
        {
            T Tlista;
            using (var _Conn = con)
            {
                 Tlista = SqlMapper.Query<T>(_Conn, qry, null, commandType: CommandType.Text,commandTimeout:60).FirstOrDefault();
                _Conn.Close();
                
            }
            return Tlista;
        }

        public int Insert(string qry, IDbConnection con)
        {
            int a;
            using (var _Conn = con)
            {
                a = SqlMapper.Execute(_Conn, qry, null, commandType: CommandType.Text,commandTimeout:60);
                _Conn.Close();
            }
            return a;
        }
        public int update(string qry, IDbConnection con)
        {
            int a;
            using (var _Conn = con)
            {
                a = SqlMapper.Execute(_Conn, qry, null, commandType: CommandType.Text,commandTimeout:60);
                _Conn.Close();
            }
            return a;
        }
        public int delete(string qry, IDbConnection con)
        {
            int a;
            using (var _Conn = con)
            {
                a = SqlMapper.Execute(_Conn, qry, null, commandType: CommandType.Text,commandTimeout:60);
                _Conn.Close();
            }
            return a;
        }

    }
}
