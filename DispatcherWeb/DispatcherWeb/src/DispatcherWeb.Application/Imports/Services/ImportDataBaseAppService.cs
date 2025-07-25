using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Runtime.Validation;
using Abp.Timing;
using DispatcherWeb.BackgroundJobs;
using DispatcherWeb.Identity;
using DispatcherWeb.Imports.DataResolvers.OfficeResolvers;
using DispatcherWeb.Imports.Dto;
using DispatcherWeb.Imports.RowReaders;

namespace DispatcherWeb.Imports.Services
{
    public abstract class ImportDataBaseAppService<T> : DispatcherWebAppServiceBase, IImportDataBaseAppService where T : IImportRow
    {
        protected readonly ImportResultDto _result = new ImportResultDto();
        protected string _timeZone;
        protected ImportJobArgs _importJobArgs;
        protected int _tenantId;
        protected long _userId;

        public IOfficeResolver OfficeResolver
        {
            [RemoteService(false)]
            [AbpAllowAnonymous]
            get;

            [RemoteService(false)]
            [AbpAllowAnonymous]
            set;
        }

        protected ImportDataBaseAppService()
        {
        }

        [UnitOfWork(isTransactional: false)]
        [DisableValidation]
        [RemoteService(false)]
        [AbpAllowAnonymous]
        public async Task<ImportResultDto> Import(TextReader textReader, ImportJobArgs args)
        {
            _importJobArgs = args;
            _tenantId = args.RequestorUser.GetTenantId();
            _userId = args.RequestorUser.UserId;

            SetImportParametersForLog(_tenantId);
            LogInfo("Import started");

            using (Session.Use(_tenantId, _userId))
            using (CurrentUnitOfWork.SetTenantId(_tenantId))
            using (var reader = new ImportReader(textReader, args.FieldMap))
            {
                _timeZone = await SettingManager.GetSettingValueAsync(TimingSettingNames.TimeZone);

                if (!await CacheResourcesBeforeImportAsync(reader))
                {
                    return _result;
                }

                var rowNumber = 0;
                foreach (T row in reader.AsEnumerable<T>())
                {
                    rowNumber++;
                    if (IsRowEmpty(row))
                    {
                        if (_result.EmptyRows.Count < 50)
                        {
                            _result.EmptyRows.Add(rowNumber);
                        }
                        _result.SkippedNumber++;
                        WriteRowErrors(row, rowNumber);
                        continue;
                    }

                    await ImportRowAndSaveAsync(row, rowNumber);
                }

                if (!await PostImportTasksAsync())
                {
                    return _result;
                }

                await CurrentUnitOfWork.SaveChangesAsync();

                _result.IsImported = true;
                LogInfo("Import successfully finished");
                return _result;
            }
        }

        protected void WriteRowErrors(T row, int rowNumber)
        {
            if (row.ParseErrors.Count > 0)
            {
                _result.ParseErrors.Add(rowNumber, row.ParseErrors);
                LogParseErrors(rowNumber, row.ParseErrors);
            }

            if (row.StringExceedErrors.Count > 0)
            {
                _result.StringExceedErrors.Add(rowNumber, row.StringExceedErrors);
                LogStringExceedErrors(rowNumber, row.StringExceedErrors);
            }
        }

        protected virtual async Task ImportRowAndSaveAsync(T row, int rowNumber)
        {
            await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                using (CurrentUnitOfWork.SetTenantId(_tenantId))
                {
                    if (await ImportRowAsync(row))
                    {
                        _result.ImportedNumber++;
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }

                    WriteRowErrors(row, rowNumber);
                }
            });
        }

        protected DateTime ConvertLocalDateTimeToUtcDateTime(DateTime utcDateTime)
        {
            return utcDateTime.ConvertTimeZoneFrom(_timeZone);
        }

        protected abstract bool IsRowEmpty(T row);
        protected abstract Task<bool> ImportRowAsync(T row);
        protected virtual Task<bool> CacheResourcesBeforeImportAsync(IImportReader reader)
        {
            return Task.FromResult(true);
        }

        protected virtual Task<bool> PostImportTasksAsync()
        {
            return Task.FromResult(true);
        }

        private string _importParameters;

        private void SetImportParametersForLog(int tenantId)
        {
            _importParameters = $"TenantId={tenantId}";
        }

        protected void LogInfo(string message)
        {
            Logger.Info($"{message}. Time: {DateTime.UtcNow} UTC, {_importParameters}");
        }

        protected void LogDebug(string message)
        {
            Logger.Debug($"{message}. Time: {DateTime.UtcNow} UTC, {_importParameters}");
        }

        protected void LogParseErrors(int rowNumber, Dictionary<string, (string value, Type type)> rowParseErrors)
        {
            LogDebug($"Parse errors at row {rowNumber}: {rowParseErrors.Select(pair => "Column: " + pair.Key + ", Value: " + pair.Value.value).JoinAsString(", ")}");
        }
        protected void LogStringExceedErrors(int rowNumber, Dictionary<string, Tuple<string, int>> rowStringExceedErrors)
        {
            LogDebug($"Exceed length errors at row {rowNumber}: {rowStringExceedErrors.Select(pair => "Column: " + pair.Key + ", Value: " + pair.Value.Item1 + ", Max Length: " + pair.Value.Item2).JoinAsString(", ")}");
        }
        protected void LogResourceErrors(List<string> resourceErrors)
        {
            if (resourceErrors.Count > 0)
            {
                LogDebug($"Resources not found: {resourceErrors.JoinAsString(", ")}");
            }
        }

        protected void AddResourceError(string error)
        {
            if (!_result.ResourceErrors.Contains(error))
            {
                _result.ResourceErrors.Add(error);
            }
        }

        protected bool TryParseCityStateZip(string addressRow, out string city, out string state, out string zip)
        {
            city = null;
            state = null;
            zip = null;
            var addressRowParts = addressRow.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (addressRowParts.Length < 3)
            {
                return false;
            }

            var zipCandidate = addressRowParts.Last();
            if (!zipCandidate.All(c => char.IsDigit(c) || c == '-'))
            {
                return false;
            }
            zip = zipCandidate;

            var stateCandidate = addressRowParts.SkipLast(1).Last();
            if (stateCandidate.Length != 2)
            {
                return false;
            }
            state = stateCandidate;

            var cityCandidate = string.Join(" ", addressRowParts.SkipLast(2)).TrimEnd(',');
            if (cityCandidate.IsNullOrEmpty())
            {
                return false;
            }
            city = cityCandidate;

            return true;
        }
    }
}
