﻿using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Denifia.Stardew.SendItemsApi.Domain
{
    public interface ITableStorageRepository
    {
        Task<bool> InsertOrReplace<TEntity>(TEntity entity) where TEntity : ITableEntity;
        Task<TEntity> Retrieve<TEntity>(string partitionKey, string rowKey) where TEntity : ITableEntity;
        Task<bool> Delete<TEntity>(TEntity entity) where TEntity : ITableEntity;
        Task<List<TEntity>> Query<TEntity>(string filter) where TEntity : ITableEntity, new();
        Task<int> Count<TEntity>() where TEntity : ITableEntity, new();
    }
}
