using System.Linq.Expressions;

// We shoulde not use any NuGet for this scope.

// ************************************************************* Explain This Interface *************************************************************
// **                                                                                                                                              **
// **     This Inteface create for : Some methods for data base services in any data base model like (SQL Server, Sqlite, ...).                    **
// **                                And for every data base model we have uniqe Generic class.                                                    **
// **                                                                                                                                              **
// **     Tasks :                                                                                                                                  **
// **                                                                                Return                                                        **
// **                                                                                                                                              **
// **             => GetAllAsync ()                                                    =>   <IEnumerable<TEntity>>                                 **
// **                                                                                                                                              **
// **             => GetByIdAsync (TKey id) =>                                         =>   <TEntity?>                                             **
// **                                                                                                                                              **
// **             => GetByIdsAsync (IEnumerable<TKey> ids)                             =>   <IEnumerable<TEntity>>                                 **
// **                                                                                                                                              **
// **             => ExistsAsync (TKey id)                                             =>   <bool>                                                 **
// **                                                                                                                                              **
// **             => FindAllAsync (Expression<Func<TEntity, bool>> predicate)          =>   <IEnumerable<TEntity>>                                 **
// **                                                                                                                                              **
// **             => AddAsync (TEntity entity)                                         =>   <TEntity>                                              **
// **                                                                                                                                              **
// **             => AddRangeAsync (IEnumerable<TEntity> entities)                     =>   <(bool Success, int Count, IEnumerable<TEntity>)>      **
// **                                                                                                                                              **
// **             => DeleteAsync (TKey id)                                             =>   <bool>                                                 **
// **                                                                                                                                              **
// **             => DeleteRangeAsync (Expression<Func<TEntity, bool>> predicate)      =>   <(bool Success, int Coun)>                             **
// **                                                                                                                                              **
// **             => UpdateAsync (TKey id, TEntity entity)                             =>   <TEntity?>                                             **
// **                                                                                                                                              **
// **             => UpdateAsync (TKey id, Action<TEntity> updateAction)               =>   <TEntity?>                                             **
// **                                                                                                                                              **
// **             => UpdateRangeAsync (IEnumerable<(TKey id, TEntity entity)> updates) =>   <(bool Success, int Count, IEnumerable<TEntity>)>      **
// **                                                                                                                                              **
// **             => CountAsync ()                                                     =>   <int>                                                  **
// **                                                                                                                                              **
// **             => CountAsync (Expression<Func<TEntity, bool>> filter)               =>   <int>                                                  **
// **                                                                                                                                              **
// ************************************************************************************************************************************************** 

namespace MyUtilities.DataBase
{
    public interface IDataBaseServices<TEntity, TKey> where TEntity : class
    {
        /// <summary>
        /// Get list of all entities from your DataBase.
        /// Can include (sort, filter parameters) option.
        /// </summary>
        /// <returns>List of all entities. </returns>
        Task<IEnumerable<TEntity>> GetAllAsync();
        /// <summary>
        /// Get one entity according to own main key.
        /// </summary>
        /// <param name="id">Value of entity main key. (ID)</param>
        /// <returns>Entity or if nothing found return null.</returns>
        Task<TEntity?> GetByIdAsync(TKey id);
        /// <summary>
        /// Get multi entities according to their main key. 
        /// </summary>
        /// <param name="ids">List of </param>
        /// <returns>List of entities.</returns>
        Task<IEnumerable<TEntity>> GetByIdsAsync(IEnumerable<TKey> ids);
        Task<bool> ExistsAsync(TKey id);
        Task<IEnumerable<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity> AddAsync(TEntity entity);
        Task<(bool Success, int Count, IEnumerable<TEntity>)> AddRangeAsync(IEnumerable<TEntity> entities);
        Task<bool> DeleteAsync(TKey id);
        Task<(bool Success,int Count)> DeleteRangeAsync(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity?> UpdateAsync(TKey id, TEntity entity);
        Task<TEntity?> UpdateAsync(TKey id, Action<TEntity> updateAction);
        Task<(bool Success, int Count, IEnumerable<TEntity>)> UpdateRangeAsync(IEnumerable<(TKey id, TEntity entity)> updates);
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<TEntity, bool>> filter);
    }
}
