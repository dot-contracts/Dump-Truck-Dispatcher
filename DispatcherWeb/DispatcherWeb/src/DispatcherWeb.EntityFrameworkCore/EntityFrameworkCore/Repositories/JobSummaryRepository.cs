using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Abp.Data;
using Abp.EntityFrameworkCore;
using DispatcherWeb.JobSummary;
using DispatcherWeb.Orders;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.EntityFrameworkCore.Repositories
{
    public class JobSummaryRepository : DispatcherWebRepositoryBase<OrderLine>, IJobSummaryRepository
    {
        private readonly IActiveTransactionProvider _transactionProvider;

        public JobSummaryRepository(IDbContextProvider<DispatcherWebDbContext> dbContextProvider, IActiveTransactionProvider transactionProvider)
            : base(dbContextProvider)
        {
            _transactionProvider = transactionProvider;
        }

        public async Task<List<JobCycle>> GetOrderTrucksLoadJobTripCycles(int tenantId, int orderLineId)
        {
            var jobCycles = new List<JobCycle>();

            await EnsureConnectionOpenAsync();

            var sqlParams = new[] {
                new SqlParameter("OL_ID", orderLineId),
                new SqlParameter("TENANT_ID", tenantId),
            };

            await using var command = await CreateCommand("sp_GetOrderTrucksOrderLineJobCycles", CommandType.StoredProcedure, sqlParams);
            await using var dataReader = await command.ExecuteReaderAsync();

            while (await dataReader.ReadAsync())
            {
                var tripType = (TruckTripTypes)Convert.ToInt32(dataReader["TripType"].ToString());
                var locationField = tripType == TruckTripTypes.ToLoadSite ? "LoadAt" : "DeliverTo";

                var jobCycle = new JobCycle
                {
                    LoadId = Convert.ToInt32(dataReader["LoadId"].ToString()),
                    TruckId = Convert.ToInt32(dataReader["TruckId"].ToString()),
                    DriverId = Convert.ToInt32(dataReader["DriverId"].ToString()),
                    DriverName = dataReader["DriverName"].ToString(),
                    TruckCode = dataReader["TruckCode"].ToString(),
                    TripType = tripType,

                    SourceLatitude = dataReader["SourceLatitude"] == DBNull.Value ? null : (double?)dataReader["SourceLatitude"],
                    SourceLongitude = dataReader["SourceLongitude"] == DBNull.Value ? null : (double?)dataReader["SourceLongitude"],
                    DestinationLatitude = dataReader["DestinationLatitude"] == DBNull.Value ? null : (double?)dataReader["DestinationLatitude"],
                    DestinationLongitude = dataReader["DestinationLongitude"] == DBNull.Value ? null : (double?)dataReader["DestinationLongitude"],

                    LocationName = dataReader[$"{locationField}Name"].ToString(),
                    LocationStreetAddress = dataReader[$"{locationField}StreetAddress"].ToString(),
                    LocationCity = dataReader[$"{locationField}City"].ToString(),
                    LocationState = dataReader[$"{locationField}State"].ToString(),

                    DeliveryDate = dataReader["DeliveryDate"] == DBNull.Value ? null : Convert.ToDateTime(dataReader["DeliveryDate"].ToString()),
                    TicketId = dataReader["TicketId"] == DBNull.Value ? null : Convert.ToInt32(dataReader["TicketId"].ToString()),
                    TicketQuantity = dataReader["TicketQuantity"] == DBNull.Value ? 0 : Convert.ToDecimal(dataReader["TicketQuantity"]),
                    TicketUomId = dataReader["TicketUomId"] == DBNull.Value ? null : Convert.ToInt32(dataReader["TicketUomId"].ToString()),
                    TicketUom = dataReader["TicketUom"].ToString(),

                    TripStart = dataReader["TripStart"] == DBNull.Value ? null : Convert.ToDateTime(dataReader["TripStart"].ToString()),
                    TripEnd = dataReader["TripEnd"] == DBNull.Value ? null : Convert.ToDateTime(dataReader["TripEnd"].ToString()),
                };
                jobCycles.Add(jobCycle);
            }
            return jobCycles;
        }

        private async Task<DbCommand> CreateCommand(string commandText, CommandType commandType, params SqlParameter[] parameters)
        {
            var context = await GetContextAsync();
            var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = commandText;
            command.CommandType = commandType;
            command.Transaction = await GetActiveTransaction();
            foreach (var parameter in parameters)
            {
                command.Parameters.Add(parameter);
            }
            return command;
        }
        private async Task EnsureConnectionOpenAsync()
        {
            var context = await GetContextAsync();
            var connection = context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }
        }

        private async Task<DbTransaction> GetActiveTransaction()
        {
            var activeTransactionProviderArgs = new ActiveTransactionProviderArgs
            {
                { "ContextType", typeof(DispatcherWebDbContext) },
                { "MultiTenancySide", MultiTenancySide },
            };
            return (DbTransaction)await _transactionProvider.GetActiveTransactionAsync(activeTransactionProviderArgs);
        }
    }
}
