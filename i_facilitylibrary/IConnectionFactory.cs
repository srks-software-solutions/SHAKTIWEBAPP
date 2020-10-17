using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary
{
    public interface IConnectionFactory
    {
        IDbConnection GetConnection { get; }
        IDbConnection GetConnectionConfig { get; }
        IDbConnection GetConnectionLive { get; }
        IDbConnection GetConnectionDashboard { get; }
        IDbConnection GetConnectionHistory { get; }
    }
}
