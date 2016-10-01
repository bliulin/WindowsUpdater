using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Cegeka.Updater.Logic.Configuration;
using Cegeka.Updater.Logic.Schedule;
using Cegeka.Updater.Logic.Utils;

namespace Cegeka.Updater.Logic.Reporting
{
    public class SqlStatusReporter : IStatusReporter
    {
        private readonly UpdateInstallationLog mUpdateLog = new UpdateInstallationLog();

        #region Constants

        private const string CStatusReportCommandText = @"
INSERT INTO [dbo].[UpdateStatusReport]
           ([Customer]
           ,[ServerName]
           ,[Timestamp]
           ,[KBArticleId]
           ,[Status]
           ,[Remarks])
     VALUES
           (@Customer
           ,@ServerName
           ,@Timestamp
           ,@KBArticleId
           ,@Status
           ,@Remarks)
";

        #endregion

        #region Properties

        public ILocalConfiguration LocalConfiguration { get; set; }
        public ITaskHandler WindowTaskHandler { get; set; }

        #endregion

        #region Constructors

        public SqlStatusReporter(ILocalConfiguration configuration, ITaskHandler taskHandler)
        {
            LocalConfiguration = configuration;
            WindowTaskHandler = taskHandler;
        }

        #endregion

        #region >> IStatusReporter Members

        public void ReportStatus(UpdateInstallationLogEntry logEntry)
        {
            mUpdateLog.Add(logEntry);
        }

        public bool Commit()
        {
            if (mUpdateLog.Count == 0)
            {
                return true;
            }

            using (var connection = new SqlConnection(LocalConfiguration.DatabaseConnectionString))
            {
                connection.Open();

                var commands = new List<SqlCommand>();
                foreach (var logEntry in mUpdateLog)
                {
                    var cmd = connection.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = CStatusReportCommandText;
                    cmd.Parameters.Add("@Customer", SqlDbType.NVarChar).Value = LocalConfiguration.CustomerName;
                    cmd.Parameters.Add("@ServerName", SqlDbType.NVarChar).Value = WindowTaskHandler.GetMachineName();
                    cmd.Parameters.Add("@Timestamp", SqlDbType.DateTime2).Value = DateTime.UtcNow;
                    cmd.Parameters.Add("@KBArticleId", SqlDbType.NVarChar).Value = logEntry.KbArticleId;
                    cmd.Parameters.Add("@Status", SqlDbType.Bit).Value = logEntry.UpdateInstallationStatus == InstallationStatus.Success;
                    cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar).Value = getReportMessage(logEntry);
                    commands.Add(cmd);
                }

                var transaction = connection.BeginTransaction();

                try
                {
                    foreach (var updateCommand in commands)
                    {
                        updateCommand.Connection = connection;
                        updateCommand.Transaction = transaction;
                        updateCommand.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
                catch (SqlException)
                {
                    transaction.Rollback();
                    throw;
                }                
            }

            return true;
        }

        public bool CommitWithRetry(IScheduler scheduler)
        {
            bool result = scheduler.Execute(Commit);
            mUpdateLog.Clear();
            return result;
        }

        private string getReportMessage(UpdateInstallationLogEntry logEntry)
        {
            string message = string.Empty;
            if (logEntry.UpdateInstallationStatus != InstallationStatus.Success)
            {
                message = logEntry.Message;
            }
            return message;
        }

        #endregion
    }
}